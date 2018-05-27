using SqliteToCsv.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace SqliteToCsv
{
    public static class Utilities
    {

        public static List<Table> GetTablesInfo(SQLiteConnection connection)
        {
            List<Table> tables = new List<Table>();
            DataTable tablesData = connection.GetSchema("Tables");
            foreach (DataRow row in tablesData.Rows)
            {
                Table newTable = new Table(row["TABLE_NAME"].ToString());
                SQLiteCommand cmd = new SQLiteCommand($"select * from {newTable.Name}", connection);
                SQLiteDataReader reader = cmd.ExecuteReader();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    newTable.Columns.Add(reader.GetName(i));
                }

                tables.Add(newTable);
            }
            return tables;
        }

        public static string[] SanitizeRowOfStrings2(object[] input)
        {

            string[] output = new string[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = SanitizeString(Convert.ToString(input[i]));
            }
            return output;
        }

        public static string SanitizeString(string input)
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

        public static void AppendToFile(string path, string fileName, string[] data)
        {
            EnsurePathExists(path);
            File.AppendAllLines(Path.Combine(path, fileName), data);
        }


        public static void EnsurePathExists(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
