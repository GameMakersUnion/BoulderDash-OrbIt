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

        private Shape _shape;
        public Shape shape { get { return _shape; } set { _shape = value; } }

        public Vector2 position = new Vector2(0, 0);
        public Vector2 velocity = new Vector2(0, 0);
        public Vector2 effvelocity = new Vector2(0, 0);
        public Vector2 force = new Vector2(0, 0);

        private double _angularVelocity = 0;
        private double _torque = 0;
        private double _orient = 0;
        private double _inertia; // moment of inertia
        private double _mass = 10f;

        private double _staticFriction = 0.5;
        private double _dynamicFriction = 0.3;
        private double _restitution = 0.2;


        private double _radius = 25f;
        private float _scale = 1f;

        public double angularVelocity { get { return _angularVelocity; } set { _angularVelocity = value; } }
        public double torque { get { return _torque; } set { _torque = value; } }
        public double staticFriction { get { return _staticFriction; } set { _staticFriction = value; } }
        public double dynamicFriction { get { return _dynamicFriction; } set { _dynamicFriction = value; } }
        public double restitution { get { return _restitution; } set { _restitution = value; } }

        
        public double orient
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
        public float scale
        {
            get { return _scale; }
            set { _scale = value; }
        }
        public double radius
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
        public double mass
        {
            get { return _mass; }
            set { _mass = value; if (value == 0) invmass = 0; else invmass = 1 / value; } //infinite mass is represented by 0
        }
        public double invmass
        {
            get;
            protected set;
        }

        public double inertia
        {
            get { return _inertia; }
            set { _inertia = value; if (value == 0) invinertia = 0; else invinertia = 1 / value; } //infinite mass is represented by 0
        }
        public double invinertia
        {
            get;
            protected set;
        }


        public textures texture
        {
            get { return _texture; }
            set { _texture = value; }
        }

        public Body() : this(null) { }
        public Body(Shape shape = null, Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.body;
            methods = mtypes.none;

            this.shape = shape ?? new Circle(25);
            this.shape.body = this;
            this.shape.Initialize();

        }

        public void ApplyForce(ref Vector2 f)
        {
            force += f;
        }
        public void ApplyImpulse(Vector2 impulse, Vector2 contactVector)
        {
            //
            velocity += (float)invmass * impulse;
            angularVelocity += invinertia * VMath.Cross(contactVector, impulse);

        }

        public void SetStatic(double newInerta = 0, double newMass = 0)
        {
            inertia = newInerta;
            mass = newMass;
        }

        public void SetOrient(double radians)
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
