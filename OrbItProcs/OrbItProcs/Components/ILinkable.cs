using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs.Components
{
    public interface ILinkable
    {
        Node parent { get; set; }

        
        void AffectSelf();
        void AffectOther(Node other);
        void Draw(SpriteBatch spritebatch);
    }
}
