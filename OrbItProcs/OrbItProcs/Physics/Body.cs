using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs
{
    public class Body : Collider
    {
        //public Shape shape;
        //public Vector2 pos = new Vector2(0, 0);
        public Vector2 velocity = new Vector2(0, 0);

        public override bool HandlersEnabled
        {
            get { return _HandlersEnabled; }
            set
            {
                if (parent != null && parent.room.game.ui != null && parent.collision.active && parent.collision.AllHandlersEnabled)
                {
                    if (!_HandlersEnabled && value && parent != parent.room.game.ui.sidebar.ActiveDefaultNode)
                    {
                        parent.room.CollisionSet.Add(this);
                    }
                    else if (_HandlersEnabled && !value && !_ResolveCollision)
                    {
                        parent.room.CollisionSet.Remove(this);
                    }
                }
                _HandlersEnabled = value;
            }
        }

        private bool _ResolveCollision = true;
        public bool isSolid
        {
            get { return _ResolveCollision; }
            set 
            {
                if (parent != null && parent.room.game.ui != null && parent.collision.active)
                {
                    if (!_ResolveCollision && value && parent != parent.room.game.ui.sidebar.ActiveDefaultNode)
                    {
                        parent.room.CollisionSet.Add(this);
                    }
                    else if (_ResolveCollision && !value && !_HandlersEnabled)
                    {
                        parent.room.CollisionSet.Remove(this);
                    }
                }
                _ResolveCollision = value; 
            }
        }
        
        public override float radius
        {
            get { return shape.radius; }
            set
            {
                float halfwidth = getTexture() != null ? (parent.getTexture().Width / 2f) : 25f;
                if (_scale != (float)value / halfwidth) _scale = value / halfwidth;
                //_radius = value;
                shape.radius = value;
            }
        }
        public float scale
        {
            get { return _scale; }
            set 
            {
                float halfwidth = getTexture() != null ? (parent.getTexture().Width / 2f) : 25f;
                //_radius = value * halfwidth;
                shape.radius = value * halfwidth;
                _scale = value;
            }
        }
        public float mass
        {
            get { return _mass; }
            set { _mass = value; if (value == 0) invmass = 0; else invmass = 1.0f / value; } //infinite mass is represented by 0
        }
        public float inertia
        {
            get { return _inertia; }
            set { _inertia = value; if (value == 0) invinertia = 0; else invinertia = 1.0f / value; } //infinite mass is represented by 0
        }
        public float angularVelocity { get { return _angularVelocity; } set { _angularVelocity = value; } }
        public float torque { get { return _torque; } set { _torque = value; } }
        public float staticFriction { get { return _staticFriction; } set { _staticFriction = value; } }
        public float dynamicFriction { get { return _dynamicFriction; } set { _dynamicFriction = value; } }
        public float restitution { get { return _restitution; } set { _restitution = value; } }

        public bool DrawCircle { get; set; }
        public float X
        {
            get { return pos.X; }
            set { pos.X = value; }
        }
        public float Y
        {
            get { return pos.Y; }
            set { pos.Y = value; }
        }

        public Color color = new Color(255, 255, 255);
        public Color permaColor = new Color(255, 255, 255);
        private textures _texture = textures.whitecircle;

        [Info(UserLevel.Never)]
        public float[] positionP { get { return pos.toFloatArray(); }
            set { pos = new Vector2(value[0], value[1]); } }
        [Info(UserLevel.Never)]
        public float[] velocityP { get { return velocity.toFloatArray(); }
            set { velocity = new Vector2(value[0], value[1]); } }
        [Info(UserLevel.Never)]
        public Vector2 effvelocity = new Vector2(0, 0);
        [Info(UserLevel.Never)]
        public float[] effvelocityP
        {
            get { return effvelocity.toFloatArray(); }
            set { effvelocity = new Vector2(value[0], value[1]); }
        }
        public Vector2 force = new Vector2(0, 0);

        private float _angularVelocity = 0;
        private float _torque = 0;
        private float _orient = 0;
        private float _inertia; // moment of inertia
        private float _mass = 10f;

        private float _staticFriction = 0.5f;
        private float _dynamicFriction = 0.3f;
        private float _restitution = 0.2f;

        //private float _radius = 25f;
        private float _scale = 1f;

        

        public float orient
        {
            get { return _orient; }
            set
            {
                _orient = value;
                if (shape != null) shape.SetOrient(value);
            }
        }
        [Info(UserLevel.Never)]
        public Color colorP
        {
            get { return color; }
            set { color = value; }
        }
        
        
        public float invmass
        {
            get;
            protected set;
        }

        
        public float invinertia
        {
            get;
            protected set;
        }


        public textures texture
        {
            get { return _texture; }
            set { _texture = value; }
        }
        [Info(UserLevel.Never)]
        public bool PolenterHack
        {
            get { return true; }
            set
            {
                if (shape != null) shape.body = this;
            }
        }

        public Body() : this(shape: null) { }
        public Body(Shape shape = null, Node parent = null)
        {
            if (parent != null) this.parent = parent;
            //com = comp.body;
            //methods = mtypes.none;
            this.shape = shape ?? new Circle(25);
            this.shape.body = this;
            this.shape.Initialize();
            DrawCircle = true;

            AfterCloning();
        }


        public override void CheckCollisionBody(Body other)
        {
            //if (!active || !other.active) { return; }
            //if (exclusions.Contains(other)) return;
            if (invmass == 0 && other.invmass == 0)
                return;

            Manifold m = new Manifold(this, other);
            m.Solve();

            if (m.contact_count > 0)
            {
                if (HandlersEnabled)
                {
                    //todo:add to handler list
                    if (OnCollisionStay != null)
                    {
                        OnCollisionStay(parent, other.parent);
                    }
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
                if (other.HandlersEnabled)
                {
                    if (other.OnCollisionStay != null)
                    {
                        other.OnCollisionStay(other.parent, parent);
                    }
                    bool otherEnter = other.OnCollisionEnter != null;
                    if (otherEnter || other.OnCollisionExit != null || other.OnCollisionFirstEnter != null || other.OnCollisionAllExit != null)
                    {
                        //HashSet<Node> lastframe = other.collision.currentIsCol1 ? other.collision.collisions1 : other.collision.collisions2;
                        //HashSet<Node> thisframe = !other.collision.currentIsCol1 ? other.collision.collisions1 : other.collision.collisions2;
                        HashSet<Collider> lastframe = other.previousCollision;
                        HashSet<Collider> thisframe = other.currentCollision;
                        thisframe.Add(this);
                        if (!lastframe.Contains(this) && otherEnter)
                        {
                            other.OnCollisionEnter(other.parent, parent);
                        }
                    }
                }
                if (isSolid && other.isSolid)
                    parent.room.AddManifold(m);
            }
        }

        public override void CheckCollisionCollider(Collider other)
        {
            //if (!active || !other.active) { return; }
            //if (exclusions.Contains(other)) return;

            //Manifold m = new Manifold(this, other);
            //m.Solve();
            bool iscolliding = Collision.CheckCollision(this, other);

            if (iscolliding)
            {
                if (other.HandlersEnabled)
                {
                    //todo:add to handler list
                    //if (OnCollisionStay != null)
                    //{
                    //    OnCollisionStay(parent, other.parent);
                    //}
                    //bool parentEnter = OnCollisionEnter != null;
                    //if (parentEnter || OnCollisionExit != null || OnCollisionFirstEnter != null || OnCollisionAllExit != null)
                    //{
                    //    HashSet<Collider> lastframe = previousCollision;
                    //    HashSet<Collider> thisframe = currentCollision;
                    //    thisframe.Add(other);
                    //    if (!lastframe.Contains(other) && parentEnter)
                    //    {
                    //        OnCollisionEnter(parent, other.parent);
                    //    }
                    //}

                    if (other.OnCollisionStay != null)
                    {
                        other.OnCollisionStay(other.parent, parent);
                    }
                    bool otherEnter = other.OnCollisionEnter != null;
                    if (otherEnter || other.OnCollisionExit != null || other.OnCollisionFirstEnter != null || other.OnCollisionAllExit != null)
                    {
                        //HashSet<Node> lastframe = other.collision.currentIsCol1 ? other.collision.collisions1 : other.collision.collisions2;
                        //HashSet<Node> thisframe = !other.collision.currentIsCol1 ? other.collision.collisions1 : other.collision.collisions2;
                        HashSet<Collider> lastframe = other.previousCollision;
                        HashSet<Collider> thisframe = other.currentCollision;
                        thisframe.Add(this);
                        if (!lastframe.Contains(this) && otherEnter)
                        {
                            other.OnCollisionEnter(other.parent, parent);
                        }
                    }
                }
            }
        }

        public void AfterCloning()
        {
            this.mass = this.mass;
            this.inertia = this.inertia;
        }

        public void SetShape(Shape shape)
        {
            shape.body = this;
            this.shape = shape;
            
        }

        public void ApplyForce(Vector2 f)
        {
            force += f;
        }
        public void ApplyImpulse(Vector2 impulse, Vector2 contactVector)
        {
            //
            velocity += invmass * impulse;
            //if (float.IsNaN(velocity.X)) System.Diagnostics.Debugger.Break();
            angularVelocity += invinertia * (float)VMath.Cross(contactVector, impulse);
            //if (float.IsInfinity(angularVelocity)) System.Diagnostics.Debugger.Break();
        }

        public void SetStatic(float newInerta = 0, float newMass = 0)
        {
            inertia = newInerta;
            mass = newMass;
        }

        public void SetOrient(float radians)
        {
            orient = radians;
            shape.SetOrient(radians);
        }

        public virtual Texture2D getTexture()
        {
            if (parent != null)
            {
                return parent.getTexture();
            }
            return null;
        }

    }
}
