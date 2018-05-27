using BetterConsoleTables;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SqliteToCsv
{
    public class ConsoleStats : IDisposable
    {
        private Timer timer;
        private ParallelProcessor processor;
        private DateTime startTime;

        public ConsoleStats(ParallelProcessor processor)
        {
            startTime = DateTime.Now;
            this.processor = processor;
            timer = new Timer(UpdateConsole, null, 250, Timeout.Infinite);
        }

        private void UpdateConsole(object state)
        {
            Console.SetCursorPosition(0, 0);
            ConsoleTables tables = new ConsoleTables();
            tables.AddTable(GetCurrentStats());
            tables.AddTable(GetTableStats());

            string output = tables.ToString();
            Console.Write(output);

            timer.Change(250, Timeout.Infinite);
        }

        private Table GetCurrentStats()
        {
            Table table = new Table(Config.Markdown(), "Time Elapsed", "Processing Queue", "Writing Queue");
            table.AddRow(DateTime.Now.Subtract(startTime),processor.ProcessingQueue.Count, processor.WritingQueue.Count);
            return table;
        }

        private Table GetTableStats()
        {
            Table table = new Table(Config.Markdown(), "Table", "Extracted", "Processed", "Written", "Processing Speed");

            foreach(SqliteToCsv.Models.Table sTable in processor.Tables)
            {
                string countPerSec = $"{String.Format("{0:n0}", sTable.Processed.AverageCountPerSecond) }/s";
                table.AddRow(
                    String.Format("{0:n0}", sTable.Name), 
                    String.Format("{0:n0}", sTable.Extracted.Count),
                    String.Format("{0:n0}", sTable.Processed.Count),
                    String.Format("{0:n0}", sTable.Written.Count), 
                    countPerSec);
            }
            return table;
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
