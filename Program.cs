using System;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Diagnostics;

namespace SqlLiteToCsv
{
    class Program
    {
        static void Main(string[] args)
        {
            string source = @"Z:\HDD\12-17-2017\forums.db.old";
            //string source = @"C:\Temp\forums.db";

            using (SqLiteProcessor processor = new SqLiteProcessor(source))
            {
                processor.ExportTable("Posts", @"C:\Temp\");
            }


            Console.ReadLine();
            return;
            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={source};New=False"))
            {
                connection.Open();
                DataTable tables = connection.GetSchema("Tables");

                foreach (DataColumn column in tables.Columns)
                {
                    //tableNames.Add(column.ToString());
                }

                /*SQLiteCommand query = new SQLiteCommand("Select * FROM Posts ", connection);
                SQLiteDataReader reader = query.ExecuteReader(CommandBehavior.SequentialAccess);

                Stopwatch stopwatch = new Stopwatch();
                long time = 0;
                int count = 0;
                stopwatch.Start();
                while (reader.Read())
                {
                    count++;
                    if (count % 100000 == 0 && stopwatch.ElapsedMilliseconds > 0)
                    {
                        //stopwatch.Stop();
                        float countPerSec = (((float)count / (float)stopwatch.ElapsedMilliseconds) *1000);
                        Console.SetCursorPosition(0, 0);
                        Console.WriteLine($"{String.Format("{0:n0}", count)} Total Records Read");
                        Console.WriteLine($"{String.Format("{0:n0}", countPerSec)} records/s");
                        //Console.WriteLine($"{String.Format("{0:n0}", (float)stopwatch.ElapsedMilliseconds / 1000)} seconds elapsed");
                        //stopwatch.Start();
                    }
                }
                stopwatch.Stop();
                Console.WriteLine();
                Console.WriteLine(count);
                Console.WriteLine((float)stopwatch.ElapsedMilliseconds/1000);*/
            }
            

            Console.ReadLine();
        }
    }
}
