using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SqliteToCsv.Models
{
    public class Stat
    {
        public Stat() { }
        public Stat(long count, long timetaken)
        {
            m_count = count;
            m_time = timetaken;
        }

        private long m_count = 0;
        private long m_time = 0;

        public long Count { get => m_count; }
        public long Time { get => m_time; }

        public void Incriment()
        {
            Interlocked.Increment(ref m_count);
        }

        public void Incriment(long time, bool replace)
        {
            Interlocked.Increment(ref m_count);

            if (replace)
            {
                Interlocked.Exchange(ref m_time, time);
            }
            else
            {
                Interlocked.Add(ref m_time, time);
            }
        }

        public float AverageTime
        {
            get
            {
                if (Count > 0)
                {
                    return (float)Time / (float)Count;
                }
                return float.NaN;
            }
        }

        public float AverageCountPerSecond
        {
            get
            {
                if(Count > 0)
                {
                    return (float)Count / ((float)Time / 1000f);
                }
                return float.NaN;
            }
        }

        public static Stat operator +(Stat s1, Stat s2)
        {
            return new Stat(s1.Count + s2.Count, s1.Time + s2.Time);
        }
    }
}
