using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    [System.AttributeUsage(System.AttributeTargets.Field |
                           System.AttributeTargets.Property)
    ]
    public class DoNotInspect : System.Attribute { }


    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class InspectMethod : System.Attribute { }

    
    public enum UserLevel
    {
        User = 0,
        Advanced = 1,
        Developer = 2,
        Debug = 3,
    }

    [System.AttributeUsage(System.AttributeTargets.Field |
                           System.AttributeTargets.Property)]
    public class AbstractionLevel : System.Attribute {
        

        public UserLevel userLevel;
        public AbstractionLevel(UserLevel userLevel)
        {
            this.userLevel = userLevel;
        }
    }
}
