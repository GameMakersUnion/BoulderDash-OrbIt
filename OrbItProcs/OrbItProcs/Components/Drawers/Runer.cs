using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace OrbItProcs
{
    /// <summary>
    /// Draws a rune image after the node's basic draw.
    /// </summary>
    [Info(UserLevel.User, "Draws a rune image after the node's basic draw.")]
    public class Runer : Component
    {
        public const mtypes CompType = mtypes.draw;
        public override mtypes compType { get { return CompType; } set { } }
        /// <summary>
        /// The rune texture to draw.
        /// </summary>
        [Info(UserLevel.User, "The rune texture to draw.")]
        public textures runeTexture { get; set; }

        /// <summary>
        /// Toggles whether runes are randomly generated upon spawning.
        /// </summary>
        [Info(UserLevel.User, "Toggles whether runes are randomly generated upon spawning.")]
        public bool randomRune { get; set; }

        public Runer() : this(null) { }
        public Runer(Node parent)
        {
            this.parent = parent;
            randomRune = false; 
            runeTexture = textures.rune1;
        }

        public override void OnSpawn()
        {
            if (!randomRune) return;
            int r = Utils.random.Next(16);
            runeTexture = (textures)r;
            
        }

        public override void Draw()
        {
            int r = (parent.body.color.R + 128) % 255;
            int g = (parent.body.color.G + 128) % 255;
            int b = (parent.body.color.B + 128) % 255;
            Color col = new Color(r, g, b);
            parent.room.camera.Draw(runeTexture, parent.body.pos, col, parent.body.scale, parent.body.orient);
        }

    }
}
