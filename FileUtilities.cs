using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SqlLiteToCsv
{
    public static class FileUtilities
    {
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
