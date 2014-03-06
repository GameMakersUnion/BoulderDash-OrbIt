using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public class CountArray<T>
    {
        public T[] array;
        public int count;
        public CountArray(T[] array)
        {
            this.array = array;
            this.count = array.Length;
        }
        public CountArray(T[] array, int count)
        {
            this.array = array;
            this.count = count;
        }

    }
}
