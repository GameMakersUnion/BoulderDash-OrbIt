using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using System.Reflection;
using OrbItProcs.Processes;
using OrbItProcs.Interface;

namespace OrbItProcs.Components
{
    public class Modifier : Component
    {
        /*
        public Node parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                //UpdateReferences();
            }
        }
        //*/
        private ModifierInfo _modifierInfo = null;
        public ModifierInfo modifierInfo { get { return _modifierInfo; } set { _modifierInfo = value;  } }

        public Modifier() : this(null) { }
        public Modifier(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.modifier;
            methods = mtypes.affectself;
        }

        public void UpdateReferences()
        {
            if (modifierInfo != null && parent != null)
            {
                int count = 0;
                for (int i = 0; i < modifierInfo.fpInfos.Keys.Count; i++)
                {
                    if (i <= modifierInfo.fpInfos.Keys.Count)
                    {
                        //we need to fix this in the case that some objects being referenced aren't the parent node
                        modifierInfo.fpInfos[modifierInfo.fpInfos.ElementAt(i).Key].ob = parent;
                        modifierInfo.fpInfosObj[modifierInfo.fpInfosObj.ElementAt(i).Key] = parent;
                    }
                }
            }
        }

        

        public override void Initialize(Node parent)
        {
            this.parent = parent;
            if (modifierInfo != null)
            {
                UpdateReferences();
            }
        }
        public void Initialize(Node parent, ModifierInfo modifierInfo)
        {
            this.parent = parent;
            if (modifierInfo != null)
            {
                this.modifierInfo = modifierInfo;
            }
            UpdateReferences();
        }

        public override void AffectOther(Node other)
        {
        }
        public override void AffectSelf()
        {
            if (modifierInfo != null)
            {
                if (modifierInfo.delegateName != "")
                {
                    MethodInfo meth = typeof(DelegateManager).GetMethod(modifierInfo.delegateName);
                    if (meth != null)
                    {
                        object[] parameters = new object[2] { modifierInfo.args, modifierInfo };

                        meth.Invoke(null, parameters);
                        return;
                    }
                }
                if (modifierInfo.modifierDelegate != null)
                modifierInfo.modifierDelegate(modifierInfo.args, modifierInfo);
            }

        }

        public override void Draw(SpriteBatch spritebatch)
        {

        }
    }
}
