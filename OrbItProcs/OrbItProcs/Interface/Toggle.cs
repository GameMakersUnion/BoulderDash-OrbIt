﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public class Toggle<T>
    {

        public T value { get; set; }
        public bool enabled { get; set; }
        //Sup, polentcner
        public Toggle() { }
        public Toggle(T Value, bool Enabled = true)
        {
            value = Value; 
            enabled = Enabled;
        }

        public override string ToString()
        {
            if (value == null) return "";
            return value.ToString();
        }


        public override bool Equals(object obj)
        {
            Type t = obj.GetType();
            if (t == this.GetType())
            {
                dynamic tog = obj;
                return enabled == tog.enabled && value == tog.value;
            }
            else if (t == typeof(T))
            {
                return obj.Equals(value);
            }
            else if (t == typeof(bool))
            {
                return obj.Equals(enabled);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + value.GetType().GetHashCode();
            hash = (hash * 7) + value.GetHashCode();
            hash = (hash * 7) + enabled.GetHashCode();
            return hash;
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
