using System;
using System.Collections.Generic;
using System.Text;

namespace SqlLiteToCsv
{
    public class Table
    {
        public Table(string name)
        {
            Name = name;
            Columns = new List<string>();
        }

        public string Name { get; set; }
        public List<string> Columns { get; set; }
    }
}
