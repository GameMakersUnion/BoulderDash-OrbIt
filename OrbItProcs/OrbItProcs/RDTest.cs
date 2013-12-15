using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public class RDTest : Redirectable
    {
        public Redirectable _parent = new RDTest2();
        public override Redirectable parent { get { return _parent; } set { _parent = value; } }
        public int _x = 5;
        public override int x
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }
    }

    public class RDTest2 : Redirectable
    {
        new public Redirectable parent;
        public int _x = 10;
        public override int x
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }
        public string _y = "My name is number 2";
        public override string y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
        }
    }
}
