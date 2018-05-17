using System;
using System.Collections.Generic;
using System.Text;

namespace SqliteToCsv
{
    public static class ParallelConfig
    {
        /* Internal settings */
        private static int maxExtractors = 1;
        private static int maxProcessors = 2;
        private static int maxWriters = 1;
        private static int maxProcessingQueue = 100000;
        private static int maxWritingQueue = 100000;


        /* Per Thread Initilized Settings Getters */

        /* Per Thread Initilized Settings backers. Not for direct use */
        [ThreadStatic]
        public static bool initilized;
        [ThreadStatic]
        private static int m_maxExtractors;
        [ThreadStatic]
        private static int m_maxProcessors;
        [ThreadStatic]
        private static int m_maxWriters;
        [ThreadStatic]
        private static int m_maxProcessingQueue;
        [ThreadStatic]
        private static int m_maxWritingQueue;




        public static int MaxExtractors
        {
            get
            {
                if (!initilized)
                {
                    Initilize();
                }
                return m_maxExtractors;
            }
        }

        public static int MaxProcessors
        {
            get
            {
                if (!initilized)
                {
                    Initilize();
                }
                return m_maxProcessors;
            }
        }

        public static int MaxWriters
        {
            get
            {
                if (!initilized)
                {
                    Initilize();
                }
                return m_maxWriters;
            }
        }

        public static int MaxProcessingQueue
        {
            get
            {
                if (!initilized)
                {
                    Initilize();
                }
                return m_maxProcessingQueue;
            }
        }

        public static int MaxWritingQueue
        {
            get
            {
                if (!initilized)
                {
                    Initilize();
                }
                return m_maxWritingQueue;
            }
        }

        public static void Initilize()
        {
            m_maxExtractors = maxExtractors;
            m_maxProcessors = maxProcessors;
            m_maxWriters = maxWriters;
            m_maxProcessingQueue = maxProcessingQueue;
            m_maxWritingQueue = maxWritingQueue;
            initilized = true;
        }
    }
}
