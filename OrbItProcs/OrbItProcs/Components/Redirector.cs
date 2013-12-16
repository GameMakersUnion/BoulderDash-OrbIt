using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace OrbItProcs.Components
{
    public class Redirector
    {
        private dynamic _TargetObject;
        public dynamic TargetObject
        {
            get
            {
                return _TargetObject;
            }
            set
            {
                _TargetObject = value;

            }
        }

        public Dictionary<Type, Dictionary<string, dynamic>> getters = new Dictionary<Type, Dictionary<string, dynamic>>();
        public Dictionary<Type, Dictionary<string, dynamic>> setters = new Dictionary<Type, Dictionary<string, dynamic>>();

        public void PopulateDelegates(Type type)
        {
            if (getters[type] == null) getters[type] = new Dictionary<string, dynamic>();
            if (setters[type] == null) setters[type] = new Dictionary<string, dynamic>();

            List<PropertyInfo> propertyinfos = type.GetProperties().ToList();
            foreach (PropertyInfo info in propertyinfos)
            {
                Type[] types = new Type[] { type, info.PropertyType };
                if (getters[type][info.Name] == null)
                {
                    MethodInfo getmethod = info.GetGetMethod();
                    Type methodtype = Expression.GetFuncType(types);

                    
                }
            }

        }

    }
}
