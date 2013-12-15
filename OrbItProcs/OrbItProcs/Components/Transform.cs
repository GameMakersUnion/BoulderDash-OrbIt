using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs.Components
{
    public class Transform : Component
    {
        public Color color = new Color(255,255,255);
        public Vector2 position = new Vector2(0,0);
        public Vector2 velocity = new Vector2(0,0);
        //private float _X = 0;
        //private float _Y = 0;
        private float _scale = 1f;
        private float _radius = 25f;
        private float _mass = 10f;
        private textures _texture = textures.whitecircle;

        public float X
        {
            get { return position.X; }
            set { position.X = value; }
        }
        public float Y
        {
            get { return position.Y; }
            set { position.Y = value; }
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
                    scale = value / (parent.getTexture().Width / 2);
                }
                else
                {
                    scale = value / (50 / 2);
                }
            }
        }
        public float mass 
        {
            get { return _mass; } 
            set { _mass = value; } 
        }
        public textures texture 
        {
            get { return _texture; } 
            set { _texture = value; } 
        }

        public Transform() : this(null) { }
        public Transform(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.transform;
            methods = mtypes.none;
        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }

        public override void AffectOther(Node other)
        {
        }
        public override void AffectSelf()
        {
        }
        public override void Draw(SpriteBatch spritebatch)
        {
        }
    }
}
