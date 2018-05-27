using System;
using System.Collections.Generic;
using System.Text;

namespace SqliteToCsv.Models
{
    public class Table
    {
        public Table(string name)
        {
            Name = name;
            Columns = new List<string>();

            Extracted = new Stat();
            Processed = new Stat();
            Written = new Stat();
        }

        public string Name { get; set; }
        public List<string> Columns { get; set; }

        public Stat Extracted { get; set; } 
        public Stat Processed { get; set; }
        public Stat Written { get; set; }
    }
}
