using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public class Collider
    {
        public HashSet<Collider> collisions1 = new HashSet<Collider>();
        public HashSet<Collider> collisions2 = new HashSet<Collider>();
        [Polenter.Serialization.ExcludeFromSerialization]
        public HashSet<Collider> currentCollision
        {
            get
            {
                return currentIsCol1 ? collisions1 : collisions2;
            }
            set { }
        }
        [Polenter.Serialization.ExcludeFromSerialization]
        public HashSet<Collider> previousCollision
        {
            get
            {
                return !currentIsCol1 ? collisions1 : collisions2;
            }
            set { }
        }
        private bool currentIsCol1 = true;

        public Shape shape;
        public Vector2 pos = new Vector2(0, 0);
        public Vector2 objectSpacePos = new Vector2(0, 0);
        //public Vector2 velocity = new Vector2(0, 0);

        [Polenter.Serialization.ExcludeFromSerialization]
        public Node parent { get; set; }

        protected bool _HandlersEnabled = true;
        public virtual bool HandlersEnabled
        {
            get { return _HandlersEnabled; }
            set
            {
                if (parent != null && parent.room.game.ui != null && parent.collision.active && parent.collision.AllHandlersEnabled)
                {
                    if (value && !parent.IsDefault)
                    {
                        parent.room.AddCollider(this);
                    }
                    else if (!value)
                    {
                        parent.room.RemoveCollider(this);
                    }
                }
                _HandlersEnabled = value;
            }
        }
        [Info(UserLevel.Never)]
        public Shape shapeP { get { return shape; } set { shape = value.Clone(); } }

        public virtual float radius { get { return shape.radius; } set { shape.radius = value; } }

        public CollisionDelegate OnCollisionEnter;
        public CollisionDelegate OnCollisionExit;
        public CollisionDelegate OnCollisionStay;
        public Action<Node> OnCollisionFirstEnter;
        public Action<Node> OnCollisionAllExit;

        public void InvokeOnCollisionEnter(Node other)
        {
            if (OnCollisionEnter != null) OnCollisionEnter(parent, other);
        }
        public void InvokeOnCollisionExit(Node other)
        {
            if (OnCollisionExit != null) OnCollisionExit(parent, other);
        }
        public void InvokeOnCollisionStay(Node other)
        {
            if (OnCollisionStay != null) OnCollisionStay(parent, other);
        }
        public void InvokeOnCollisionFirstEnter()
        {
            if (OnCollisionFirstEnter != null) OnCollisionFirstEnter(parent);
        }
        public void InvokeOnCollisionAllExit()
        {
            if (OnCollisionAllExit != null) OnCollisionAllExit(parent);
        }

        public void ClearHandlers()
        {
            OnCollisionEnter -= OnCollisionEnter;
            OnCollisionExit -= OnCollisionExit;
            OnCollisionStay -= OnCollisionStay;
            OnCollisionFirstEnter -= OnCollisionFirstEnter;
            OnCollisionAllExit -= OnCollisionAllExit;
        }

        public Collider() : this(shape: null) { }
        public Collider(Shape shape = null, Node parent = null)
        {
            float rad = 25;
            if (parent != null)
            {
                this.parent = parent;
                rad = parent.body.radius;
            }
            if (shape != null)
            {
                this.shape = shape;
            }
            else
            {
                this.shape = new Circle(rad);
            }
        }

        public virtual void CheckCollisionBody(Body other)
        {

            //if (!active || !other.active) { return; }
            //if (exclusions.Contains(other)) return;

            //Manifold m = new Manifold(this, other);
            //m.Solve();
            bool iscolliding = Collision.CheckCollision(this, other);

            if (iscolliding)
            {
                if (HandlersEnabled)
                {
                    //todo:add to handler list
                    InvokeOnCollisionStay(other.parent);

                    bool parentEnter = OnCollisionEnter != null;
                    if (parentEnter || OnCollisionExit != null || OnCollisionFirstEnter != null || OnCollisionAllExit != null)
                    {
                        HashSet<Collider> lastframe = previousCollision;
                        HashSet<Collider> thisframe = currentCollision;
                        thisframe.Add(other);
                        if (!lastframe.Contains(other) && parentEnter)
                        {
                            OnCollisionEnter(parent, other.parent);
                        }
                    }
                }
                //other.InvokeOnCollisionStay(parent);
                //
                //bool otherEnter = other.OnCollisionEnter != null;
                //if (otherEnter || other.OnCollisionExit != null || other.OnCollisionFirstEnter != null || other.OnCollisionAllExit != null)
                //{
                //    //HashSet<Node> lastframe = other.collision.currentIsCol1 ? other.collision.collisions1 : other.collision.collisions2;
                //    //HashSet<Node> thisframe = !other.collision.currentIsCol1 ? other.collision.collisions1 : other.collision.collisions2;
                //    HashSet<Collider> lastframe = other.previousCollision;
                //    HashSet<Collider> thisframe = other.currentCollision;
                //    thisframe.Add(this);
                //    if (!lastframe.Contains(this) && otherEnter)
                //    {
                //        other.OnCollisionEnter(other.parent, parent);
                //    }
                //}

            }
        }

        public virtual void CheckCollisionCollider(Collider other)
        {
            //bool iscolliding = Collision.CheckCollision(this, other);
            //
            //if (iscolliding)
            //{
            //    //todo:add to handler list
            //    InvokeOnCollisionStay(other.parent);
            //
            //    bool parentEnter = OnCollisionEnter != null;
            //    if (parentEnter || OnCollisionExit != null || OnCollisionFirstEnter != null || OnCollisionAllExit != null)
            //    {
            //        HashSet<Collider> lastframe = previousCollision;
            //        HashSet<Collider> thisframe = currentCollision;
            //        thisframe.Add(other);
            //        if (!lastframe.Contains(other) && parentEnter)
            //        {
            //            OnCollisionEnter(parent, other.parent);
            //        }
            //    }
            //
            //    other.InvokeOnCollisionStay(parent);
            //
            //    bool otherEnter = other.OnCollisionEnter != null;
            //    if (otherEnter || other.OnCollisionExit != null || other.OnCollisionFirstEnter != null || other.OnCollisionAllExit != null)
            //    {
            //        //HashSet<Node> lastframe = other.collision.currentIsCol1 ? other.collision.collisions1 : other.collision.collisions2;
            //        //HashSet<Node> thisframe = !other.collision.currentIsCol1 ? other.collision.collisions1 : other.collision.collisions2;
            //        HashSet<Collider> lastframe = other.previousCollision;
            //        HashSet<Collider> thisframe = other.currentCollision;
            //        thisframe.Add(this);
            //        if (!lastframe.Contains(this) && otherEnter)
            //        {
            //            other.OnCollisionEnter(other.parent, parent);
            //        }
            //    }
            //
            //}
        }

        public void ClearCollisionList()
        {
            
            //if (!active) return;
            //HashSet<Node> lastframe = isCollisions1 ? collisions1 : collisions2;
            //HashSet<Node> thisframe = !isCollisions1 ? collisions1 : collisions2;
            HashSet<Collider> lastframe = previousCollision;
            HashSet<Collider> thisframe = currentCollision;

            if (HandlersEnabled)
            {
                if (OnCollisionFirstEnter != null && lastframe.Count == 0 && thisframe.Count > 0)
                {
                    OnCollisionFirstEnter(parent);
                }
                else if (OnCollisionAllExit != null && lastframe.Count > 0 && thisframe.Count == 0)
                {
                    OnCollisionAllExit(parent);
                }
            }

            foreach (Collider c in lastframe.ToList())
            {
                if (!thisframe.Contains(c) && OnCollisionExit != null && HandlersEnabled)// && TriggerHandlers)
                {
                    OnCollisionExit(parent, c.parent);
                }
                lastframe.Remove(c);
            }
            currentIsCol1 = !currentIsCol1;
        }
    }
}
