using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace OrbItProcs.Processes
{
    public class FPInfo
    {
        private FieldInfo _fieldInfo;
        [Polenter.Serialization.ExcludeFromSerialization]
        public FieldInfo fieldInfo { get { return _fieldInfo; } set { _fieldInfo = value; } }

        private PropertyInfo _propertyInfo;
        [Polenter.Serialization.ExcludeFromSerialization]
        public PropertyInfo propertyInfo { get { return _propertyInfo; } set { _propertyInfo = value; } }
        //[Polenter.Serialization.ExcludeFromSerialization]
        public object ob;

        public string Name { get; set; }

        public FPInfo () { /*serializeationiantiszeatned;*/ }
        public FPInfo (FieldInfo fieldInfo)
        {
            this.fieldInfo = fieldInfo;
            Name = fieldInfo.Name;
        }
        public FPInfo (PropertyInfo propertyInfo)
        {
            this.propertyInfo = propertyInfo;
            Name = propertyInfo.Name;
        }
        public FPInfo(FieldInfo fieldInfo, PropertyInfo propertyInfo) //for copying component use
        {
            this.propertyInfo = propertyInfo;
            this.fieldInfo = fieldInfo;
            if (propertyInfo != null) Name = propertyInfo.Name;
            else if (fieldInfo != null) Name = fieldInfo.Name;
            else Name = "error_Name_1";
            //ob = null;
        }
        public FPInfo(FPInfo old) //for copying component use
        {
            this.propertyInfo = old.propertyInfo;
            this.fieldInfo = old.fieldInfo;
            if (propertyInfo != null) Name = propertyInfo.Name;
            else if (fieldInfo != null) Name = fieldInfo.Name;
            else Name = "error_Name_2";

            //ob = null;
        }
        public FPInfo (string name, object obj)
        {
            ob = obj;
            propertyInfo = obj.GetType().GetProperty(name);
            Name = name;
            if (propertyInfo == null)
            {
                fieldInfo = obj.GetType().GetField(name);
                if (fieldInfo == null)
                {
                    Console.WriteLine("member was not found.");
                    name = "error_Name_3";

                }
            }
            
        }

        public static FPInfo GetNew(string name, object obj)
        {
            return new FPInfo(name, obj);
        }

        public object GetValue()
        {
            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(ob, null);
            }
            else if (fieldInfo != null)
            {
                return fieldInfo.GetValue(ob);
            }
            return null;
        }
        public object GetValue(object obj)
        {
            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(obj, null);
            }
            else if (fieldInfo != null)
            {
                return fieldInfo.GetValue(obj);
            }
            return null;
        }


        public void SetValue(object value)
        {
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(ob, value, null);
            }
            else if (fieldInfo != null)
            {
                fieldInfo.SetValue(ob, value);
            }
        }
        public void SetValue(object value, object obj)
        {
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(obj, value, null);
            }
            else if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
        }

        public static object GetValue(string name, object obj)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty(name);
            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(obj,null);
            }
            else
            {
                FieldInfo fieldInfo = obj.GetType().GetField(name);
                if (fieldInfo != null)
                {
                    return fieldInfo.GetValue(obj);
                }
            }
            return null;
        }

        //calls a method with no parameters on either fieldinfo or propertyinfo object
        public object CallMethod(string methodname)
        {
            //if (ob == null) return;
            try
            {
                if (propertyInfo != null)
                {

                    return propertyInfo.GetType().GetMethod(methodname).Invoke(propertyInfo, null);
                }
                else if (fieldInfo != null)
                {
                    return fieldInfo.GetType().GetMethod(methodname).Invoke(fieldInfo, null);
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("FPInfo exception: {0}", e.Message);
                return null;
            }
        }
        /*
        public string Name()
        {
            if (propertyInfo != null)
            {

                return propertyInfo.Name;
            }
            else if (fieldInfo != null)
            {
                return fieldInfo.Name;
            }
            return "nameless";
        }
        */
        public Type FPType()
        {
            if (propertyInfo != null)
            {

                return propertyInfo.PropertyType;
            }
            else if (fieldInfo != null)
            {
                return fieldInfo.FieldType;
            }
            return null;
        }

        public static void SetValue(string name, object obj, object value)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty(name);
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(obj, value, null);
            }
            else
            {
                FieldInfo fieldInfo = obj.GetType().GetField(name);
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(obj, value);
                }
            }

        }

    }
}
