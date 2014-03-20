using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public class Toggle<T>
    {
        public T value { get; set; }
        public bool enabled { get; set; }
        //Sup, polentner
        public Toggle() { }
        public Toggle(T value, bool enabled = true)
        {
            this.value = value; this.enabled = enabled;
        }

        public static implicit operator bool(Toggle<T> d)
        {
            if (typeof(T) == typeof(bool)) throw new SystemException("Don't use the Implicit operator with boolean Toggles");
            return d.enabled;
        }
        public static implicit operator T(Toggle<T> d)
        {
            if (typeof(T) == typeof(bool)) throw new SystemException("Don't use the Implicit operator with boolean Toggles");
            return d.value;
        }
        public static bool operator <(Toggle<T> a, T b)
        {
            if (typeof(T).GetInterface("IComparable") != null)
                return (a.value as IComparable).CompareTo(b as IComparable) < 0;
            else throw new SystemException("Tried to Compare non-Comparables");
        }
        public static bool operator >(Toggle<T> a, T b)
        {
            if (typeof(T).GetInterface("IComparable") != null)
                return (a.value as IComparable).CompareTo(b as IComparable) > 0;
            else throw new SystemException("Tried to Compare non-Comparables");
        }

        public static bool operator ==(Toggle<T> a, T b)
        {
            if (typeof(T).GetInterface("IComparable") != null)
                return (a.value as IComparable).CompareTo(b as IComparable) == 0;
            else return (a.value.Equals(b));
        }

        public static bool operator !=(Toggle<T> a, T b)
        {
            if (typeof(T).GetInterface("IComparable") != null)
                return !((a.value as IComparable).CompareTo(b as IComparable) == 0);
            else return (!a.value.Equals(b));
        }
    }
}
