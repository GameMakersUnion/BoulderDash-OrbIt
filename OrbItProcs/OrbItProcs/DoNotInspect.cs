﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    [System.AttributeUsage(System.AttributeTargets.Field|
                           System.AttributeTargets.Property)
    ]
    public class DoNotInspect : System.Attribute
    {
        public DoNotInspect()
        {
        }
    }
}
