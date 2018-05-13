using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SqlLiteToCsv
{
    public class SqLiteProcessor : IDisposable
    {
        private SQLiteConnection _connection;

        public SqLiteProcessor(string dbPath)
        {
            _connection = new SQLiteConnection($"Data Source={dbPath};New=False");
            _connection.Open();Tables = new List<Table>();
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

                    FileUtilities.AppendToFile(path, $"{tableName}.csv", lines);
                    sanitizedData.Clear();
                    estimatedBytesSize = 0;
                }
            }

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
