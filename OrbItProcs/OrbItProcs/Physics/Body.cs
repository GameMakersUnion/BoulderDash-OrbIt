using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs
{
    public class Body : Component
    {
        public Color color = new Color(255, 255, 255);
        private textures _texture = textures.whitecircle;

        public Shape shape;
        public Shape shapeP { get { return shape; } set { shape = value.Clone();} }

        public Vector2 pos = new Vector2(0, 0);
        public Vector2 velocity = new Vector2(0, 0);

        public float[] positionP { get { return pos.toFloatArray(); } 
            set { pos = new Vector2(value[0], value[1]); } }
        public float[] velocityP { get { return velocity.toFloatArray(); } 
            set { velocity = new Vector2(value[0], value[1]); } }
        
        public Vector2 effvelocity = new Vector2(0, 0);
        public Vector2 force = new Vector2(0, 0);

        private float _angularVelocity = 0;
        private float _torque = 0;
        private float _orient = 0;
        private float _inertia; // moment of inertia
        private float _mass = 10f;

        private float _staticFriction = 0.5f;
        private float _dynamicFriction = 0.3f;
        private float _restitution = 0.2f;


        private float _radius = 25f;
        private float _scale = 1f;

        public float angularVelocity { get { return _angularVelocity; } set { _angularVelocity = value; } }
        public float torque { get { return _torque; } set { _torque = value; } }
        public float staticFriction { get { return _staticFriction; } set { _staticFriction = value; } }
        public float dynamicFriction { get { return _dynamicFriction; } set { _dynamicFriction = value; } }
        public float restitution { get { return _restitution; } set { _restitution = value; } }


        public float orient
        {
            get { return _orient; }
            set
            {
                _orient = value;
                if (shape != null) shape.SetOrient(value);
            }
        }

        public Color colorP
        {
            get { return color; }
            set { color = value; }
        }
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
        public float scale
        {
            get { return _scale; }
            set { _scale = value; }
        }
        public float radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                if (getTexture() != null)
                {
                    scale = (float)value / (parent.getTexture().Width / 2);
                }
                else
                {
                    scale = (float)value / (50 / 2);
                }
            }
        }
        public float mass
        {
            get { return _mass; }
            set { _mass = value; if (value == 0) invmass = 0; else invmass = 1.0f / value; } //infinite mass is represented by 0
        }
        public float invmass
        {
            get;
            protected set;
        }

        public float inertia
        {
            get { return _inertia; }
            set { _inertia = value; if (value == 0) invinertia = 0; else invinertia = 1.0f / value; } //infinite mass is represented by 0
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

        public Body() : this(shape: null) { }
        public Body(Shape shape = null, Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.body;
            methods = mtypes.none;

            this.shape = shape ?? new Circle(25);
            this.shape.body = this;
            this.shape.Initialize();

            AfterCloning();
        }

        public override void AfterCloning()
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

        


        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }

        public override void AffectSelf()
        {
        }
        public override void Draw(SpriteBatch spritebatch)
        {
        }
    }
}
