using System;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SqliteToCsv;

namespace SqlLiteToCsv
{
    class Program
    {
        static void Main(string[] args)
        {
            string source = @"Z:\HDD\12-17-2017\forums.db.old";
            //string source = @"C:\Temp\forums.db";

            using(ParallelProcessor processor = new ParallelProcessor(source, @"Z:\HDD\12-17-2017\DbExport\"))
            {
                Task.Run(async () =>
                {
                    await processor.Start();
                }).GetAwaiter().GetResult();
                Console.ReadLine();
            }


            //using (SqLiteProcessor processor = new SqLiteProcessor(source, @"Z:\HDD\12-17-2017\DbExport\"))
            //{
            //    processor.ProcessTables();
            //}


        }
    }
}
