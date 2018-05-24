using SqlLiteToCsv;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqliteToCsv
{
    public class ParallelProcessor : IDisposable
    {
        private SQLiteConnection _connection;
        private FileStream _stream;
        private StreamWriter _writer;
        private string _dbPath;
        private string _outputPath;
        private List<Table> _tables;

        private string _currentTableName = "";

        public ParallelProcessor(string dbPath, string outputPath)
        {
            _dbPath = dbPath;
            _outputPath = outputPath;

            _connection = new SQLiteConnection($"Data Source={dbPath};New=False");
            _connection.Open();

            _tables = Utilities.GetTablesInfo(_connection);

            Tasks = new ConcurrentDictionary<int, Task>();
            Extractors = new ConcurrentDictionary<int, Task>();
            Workers = new ConcurrentDictionary<int, Task>();
            ProcessingQueue = new BlockingCollection<object[]>(ParallelConfig.MaxProcessingQueue);
            WritingQueue = new BlockingCollection<string>(ParallelConfig.MaxWritingQueue);
            Writers = new ConcurrentDictionary<int, Task>();

        }

        public bool Active { get; private set; } = false;
        public volatile bool extractorsActive = false;
        public volatile bool workersActive = false;

        public ConcurrentDictionary<int, Task> Tasks { get; private set; }
        public ConcurrentDictionary<int, Task> Extractors { get; private set; }
        public ConcurrentDictionary<int, Task> Workers { get; private set; }
        public ConcurrentDictionary<int, Task> Writers { get; private set; }

        public BlockingCollection<object[]> ProcessingQueue { get; private set; }
        public BlockingCollection<string> WritingQueue { get; private set; }

        public async Task Start()
        {
            Active = true;
            foreach (Table table in _tables)
            {
                _currentTableName = table.Name;

                _stream = new FileStream(Path.Combine(_outputPath, $"{_currentTableName}.csv"), FileMode.Create);
                _writer = new StreamWriter(_stream, Encoding.Unicode, 1024 * 1024 * 8);

                await InitilizeTasks();
                await StartTasks();
                await WaitOnTasks();

                await _writer.FlushAsync();
                await _stream.FlushAsync();
                _writer.Dispose();
                _stream.Dispose();
                _stream = null;
                _writer = null;

                ClearTasks();
            }

        }

        private async Task WaitOnTasks()
        {
            Task[] extractors = new Task[Extractors.Count];
            Task[] workers = new Task[Workers.Count];
            Task[] writers = new Task[Writers.Count];

            Extractors.Values.CopyTo(extractors, 0);
            Workers.Values.CopyTo(workers, 0);
            Writers.Values.CopyTo(writers, 0);

            Task.WaitAll(extractors);
            extractorsActive = false;

            Task.WaitAll(workers);
            workersActive = false;

            Task.WaitAll(writers);
        }

        private async Task ClearTasks()
        {
            Tasks.Clear();
            Extractors.Clear();
            Workers.Clear();
            Writers.Clear();
        }

        private async Task InitilizeTasks()
        {
            await InitilizeTasks(DoExtractionWork, Extractors, ParallelConfig.MaxExtractors);
            await InitilizeTasks(DoProcessingnWork, Workers, ParallelConfig.MaxProcessors);
            await InitilizeTasks(DoWritingWork, Writers, ParallelConfig.MaxWriters);
        }

        private async Task InitilizeTasks(Func<object, Task> workFunction, ConcurrentDictionary<int, Task> taskCollection, int number)
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < number; i++)
            {
                TaskState state = new TaskState();
                Task task = new Task<Task>(workFunction, state);
                state.Id = task.Id;

                taskCollection.TryAdd(i, task);
                Tasks.TryAdd(task.Id, task);
            }
        }

        private async Task StartTasks()
        {
            extractorsActive = true;
            workersActive = true;

            foreach (Task task in Tasks.Values)
            {
                task.Start();
            }
        }



        private async Task DoExtractionWork(object state)
        {
            SQLiteCommand query = new SQLiteCommand($"Select * FROM {_currentTableName} ", _connection);
            SQLiteDataReader reader = query.ExecuteReader(CommandBehavior.SequentialAccess);
            TaskState taskState = (TaskState)state;

            Stopwatch stopwatch = new Stopwatch();
            long totalExtracted = 0;

            stopwatch.Start();
            object[] columns = _tables.Where(x => x.Name == _currentTableName).Single().Columns.Select(x => (object)x).ToArray();
            ProcessingQueue.Add(columns);

            while (reader.Read())
            {
                /*if(ProcessingQueue.Count >= ParallelConfig.MaxProcessingQueue)
                {
                    while (ProcessingQueue.Count >= ParallelConfig.MaxProcessingQueue)
                    {
                        taskState.Status = State.Paused;
                        await Task.Delay(500);
                    }
                }*/

                taskState.Status = State.Running;

                object[] rowValues = new object[reader.FieldCount];
                reader.GetValues(rowValues);
                ProcessingQueue.Add(rowValues);

                totalExtracted++;
                if(stopwatch.ElapsedMilliseconds % 500 == 0)
                {
                    Console.WriteLine($"Queue size: {ProcessingQueue.Count}. Writing Queue: {WritingQueue.Count} Speed: {(float)totalExtracted / ((float)stopwatch.ElapsedMilliseconds / 1000) } items/s");
                }
            }

            Console.WriteLine($"Extraction complete. {totalExtracted} records");
        }

        private async Task DoProcessingnWork(object state)
        {
            TaskState taskState = (TaskState)state;

            while (extractorsActive || ProcessingQueue.Count > 0)
            {
                /*if(WritingQueue.Count >= ParallelConfig.MaxWritingQueue || ProcessingQueue.Count == 0)
                {
                    taskState.Status = State.Paused;
                    await Task.Delay(500);
                }*/
                taskState.Status = State.Running;

                object[] data;
                if (!ProcessingQueue.TryTake(out data, 500))
                {
                    continue;
                }
                string sanitized = String.Join(",", Utilities.SanitizeRowOfStrings2(data));
                WritingQueue.Add(sanitized);
            }
        }

        private async Task DoWritingWork(object state)
        {
            TaskState taskState = (TaskState)state;

            while (workersActive || WritingQueue.Count > 0)
            {
                /*if(WritingQueue.Count == 0)
                {
                    taskState.Status = State.Paused;
                    await Task.Delay(500);
                    continue;
                }*/

                taskState.Status = State.Running;

                string toWrite;
                if (!WritingQueue.TryTake(out toWrite, 500))
                {
                    continue;
                }

                _writer.WriteLine(toWrite);
                int test = 99;
            }
        }

        public void Dispose()
        {
            if(!(_connection is null))
            {
                _connection.Dispose();
            }

            if (!(_stream is null))
            {
                _stream.Dispose();
            }

            if (!(_writer is null))
            {
                _writer.Dispose();
            }
        }

    }
}
