using System;
using System.Collections.Generic;
using System.Text;

namespace SqliteToCsv
{
    public static class ParallelConfig
    {
        public static int MaxExtractors { get; set; } = 1;
        public static int MaxProcessors { get; set; } = 2;
        public static int MaxWriters { get; set; } = 1;
        public static int MaxProcessingQueue { get; set; } = 5000;
        public static int MaxWritingQueue { get; set; } = 5000;
    }
}
