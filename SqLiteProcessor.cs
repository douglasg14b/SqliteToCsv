using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SqlLiteToCsv
{
    public class SqLiteProcessor : IDisposable
    {
        private SQLiteConnection _connection;
        private string _dbPath;
        private string _outputPath;

        public SqLiteProcessor(string dbPath, string outputPath)
        {
            _dbPath = dbPath;
            _outputPath = outputPath;

            _connection = new SQLiteConnection($"Data Source={dbPath};New=False");
            _connection.Open();

            Tables = new List<Table>();

            GetTablesInfo();
        }

        public List<Table> Tables { get; set; }



        private void GetTablesInfo()
        {
            DataTable tablesData = _connection.GetSchema("Tables");
            foreach (DataRow row in tablesData.Rows)
            {
                Table newTable = new Table(row["TABLE_NAME"].ToString());
                SQLiteCommand cmd = new SQLiteCommand($"select * from {newTable.Name}", _connection);
                SQLiteDataReader reader = cmd.ExecuteReader();
                for(int i = 0; i < reader.FieldCount; i++)
                {
                    newTable.Columns.Add(reader.GetName(i));
                }

                Tables.Add(newTable);
            }

            
        }

        public void ProcessTables()
        {
            Stopwatch stopwatch = new Stopwatch();

            foreach (Table table in Tables)
            {
                ProcessTable(table);
            }
        }

        private void ProcessTable(Table table)
        {
            Stopwatch stopwatch = new Stopwatch();
            SQLiteCommand query = new SQLiteCommand($"Select * FROM {table.Name} ", _connection);
            SQLiteDataReader reader = query.ExecuteReader(CommandBehavior.SequentialAccess);
            List<string[]> sanitizedStaging = new List<string[]>();

            long time = 0;
            int count = 0;
            long estimatedBytesSize = 0;
            float countPerSec = 1000; //To trigger the first check

            using (FileStream stream = new FileStream(Path.Combine(_outputPath, $"{table.Name}.csv"), FileMode.Create))
            using (StreamWriter writer = new StreamWriter(stream, Encoding.Unicode, (1024 * 1024 * 32)))
            {
                stopwatch.Start();

                while (reader.Read())
                {
                    object[] rowValues = new object[reader.FieldCount - 1];
                    reader.GetValues(rowValues);
                    string[] sanitizedValues = SanitizeRowOfStrings2(rowValues);

                    foreach (string item in sanitizedValues)
                    {
                        estimatedBytesSize += 26 + (item.Length * 2);
                    }

                    sanitizedStaging.Add(sanitizedValues);

                    count++;
                    if (stopwatch.ElapsedMilliseconds > 0 && stopwatch.ElapsedMilliseconds % 100 == 0)
                    {
                        countPerSec = ((float)count / (float)stopwatch.ElapsedMilliseconds) * 1000;
                        Console.SetCursorPosition(0, 0);
                        Console.WriteLine($"{String.Format("{0:n0}", count)} Total Records Read");
                        Console.WriteLine($"{String.Format("{0:n0}", countPerSec)} records/s");
                        Console.WriteLine($"~{String.Format("{0:n0}", estimatedBytesSize)} bytes");
                    }

                    if (estimatedBytesSize >= (1024 * 1024 * 32)) //32MB
                    {
                        FlushToFile(sanitizedStaging, writer);
                        estimatedBytesSize = 0;
                    }
                }

                FlushToFile(sanitizedStaging, writer);
                estimatedBytesSize = 0;
            }
        }

        private void FlushToFile(List<string[]> data, StreamWriter writer)
        {
            for (int i = 0; i < data.Count; i++)
            {
                writer.WriteLine(String.Join(',', data[i]));
            }

            data.Clear();
            GC.Collect(); //Necessary to property clean up the lists memory
        }

        public void ExportTable(string tableName, string path)
        {
            Table table = Tables.Find(x => x.Name == tableName);
            if (table is null)
            {
                throw new ArgumentException("Table does not exist");
            }
            SQLiteCommand query = new SQLiteCommand($"Select * FROM {tableName} ", _connection);
            SQLiteDataReader reader = query.ExecuteReader(CommandBehavior.SequentialAccess);


            Stopwatch stopwatch = new Stopwatch();
            long time = 0;
            int count = 0;
            long estimatedBytesSize = 0;
            stopwatch.Start();

            List<string[]> sanitizedData = new List<string[]>();
            while (reader.Read())
            {
                object[] rowValues = new object[reader.FieldCount - 1];
                reader.GetValues(rowValues);
                string[] sanitizedValues = SanitizeRowOfStrings2(rowValues);

                foreach(string item in sanitizedValues)
                {
                    estimatedBytesSize += 26 + (item.Length * 2);
                }

                sanitizedData.Add(sanitizedValues);

                count++;
                if (count % 10000 == 0 && stopwatch.ElapsedMilliseconds > 0)
                {
                    float countPerSec = (((float)count / (float)stopwatch.ElapsedMilliseconds) * 1000);
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine($"{String.Format("{0:n0}", count)} Total Records Read");
                    Console.WriteLine($"{String.Format("{0:n0}", countPerSec)} records/s");
                    Console.WriteLine($"~{String.Format("{0:n0}", estimatedBytesSize)} bytes");
                }

                if(estimatedBytesSize >= (1000*1000*500)) //500MB
                {
                    string[] lines = new string[sanitizedData.Count];
                    for(int i = 0; i < sanitizedData.Count; i++)
                    {
                        lines[i] = String.Join(',', sanitizedData[i]);
                    }

                    Utilities.AppendToFile(path, $"{tableName}.csv", lines);
                    sanitizedData.Clear();
                    GC.Collect();
                    estimatedBytesSize = 0;
                }
            }

        }

        public void AppendToFile()
        {

        }


        public List<string> SanitizeRowOfStrings(object[] input)
        {
            
            List<string> output = new List<string>(input.Length);
            foreach (object item in input)
            {
                output.Add(SanitizeString(Convert.ToString(item)));
            }
            return output;
        }

        public string[] SanitizeRowOfStrings2(object[] input)
        {

            string[] output = new string[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = SanitizeString(Convert.ToString(input[i]));
            }
            return output;
        }

        private string SanitizeString(string input)
        {
            int capacity = input.Length + (int)(input.Length * 0.1f); //Input length + 10%
            StringBuilder builder = new StringBuilder(capacity);

            builder.Append('"');
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '"')
                {
                    builder.Append('"');
                }
                builder.Append(input[i]);
            }

            builder.Append('"');
            return builder.ToString();
        }

        public void Dispose()
        {
            if(!(_connection is null))
            {
                _connection.Dispose();
            }
        }
    }
}
