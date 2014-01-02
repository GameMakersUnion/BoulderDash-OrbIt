using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs.Components
{
    public class Circler : Component {

        
        private float _delta = 0.01f;
        public float delta { get { return _delta; } set { _delta = value; } }
        private float _deltaPrime = 0.001f;
        public float deltaPrime { get { return _deltaPrime; } set { _deltaPrime = value; } }

        private float _maxDelta = 1f;
        public float maxDelta { get { return _maxDelta; } set { _maxDelta = value; } }
        private float _minDelta = 0.05f;
        public float minDelta { get { return _minDelta; } set { _minDelta = value; } }

        private float _angle = 0f;
        public float angle { get { return _angle; } set { _angle = value; } }
        private float _maxAngle = 3.14f;
        public float maxAngle { get { return _maxAngle; } set { _maxAngle = value; } }
        private float _minAngle = -3.14f;
        public float minAngle { get { return _minAngle; } set { _minAngle = value; } }


        private bool _loop = true;
        public bool loop { get { return _loop; } set { _loop = value; } }

        public Circler() : this(null) { }
        public Circler(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.circler;
            methods = mtypes.affectself; 
        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }
        public override void OnSpawn()
        {
            //base.OnSpawn();
            if (parent != null && parent.transform.velocity.Length() > 0)
            {
                angle = (float)Math.Atan2(parent.transform.velocity.Y, parent.transform.velocity.X);
            }
        }



        public override void AffectSelf()
        {
            angle += delta;
            if (angle < minAngle)
            {
                delta *= -1;
                angle = minAngle;
            }
            else if (angle > maxAngle)
            {
                if (loop)
                {
                    angle = minAngle;
                }
                else
                {
                    delta *= -1;
                    angle = maxAngle;
                }
            }
            delta += deltaPrime;
            if (delta < minDelta)
            {
                deltaPrime *= -1;
                delta = minDelta;
            }
            else if (delta > maxDelta)
            {
                deltaPrime *= -1;
                delta = maxDelta;
            }

            float length = parent.transform.velocity.Length();
            float x = length * (float)Math.Sin(angle);
            float y = length * (float)Math.Cos(angle);
            parent.transform.velocity = new Vector2(x, y);

        }

        public override void Draw(SpriteBatch spritebatch)
        {

        }
    }
}
