using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;

namespace OrbItProcs
{
    public class LevelSave
    {
        public Vector2 levelSize;
        public List<List<Vector2>> polygonVertices;
        public string name;
        public LevelSave(Group group, Vector2 levelSize, string name)
        {
            this.levelSize = levelSize;
            polygonVertices = new List<List<Vector2>>();
            foreach(Node n in group.fullSet)
            {
                if (n.body.shape is Polygon)
                {
                    Polygon p = (Polygon)n.body.shape;
                    List<Vector2> verts = new List<Vector2>();
                    for(int i = 0; i < p.vertexCount; i++)
                    {
                        verts.Add(p.vertices[i]);
                    }
                    if (verts.Count > 2)
                    {
                        polygonVertices.Add(verts);
                    }
                }
            }
        }

        public static void SaveLevel(Group group, int levelWidth, int levelHeight, string name)
        {
            if (name.Equals("")) return;
            name = name.Trim();
            //string filename = "Presets//Nodes//" + name + ".xml";
            string filename = Assets.levelsFilepath + "/" + name + ".xml";
            Action completeSave = delegate
            {
                LevelSave levelSave = new LevelSave(group, new Vector2(levelWidth, levelHeight), name);
                OrbIt.game.serializer = new Polenter.Serialization.SharpSerializer();
                OrbIt.game.serializer.Serialize(levelSave, filename);
                //Assets.NodePresets.Add(serializenode);
            };

            if (File.Exists(filename))
            { //we must be overwriting, therefore don't update the live presetList
                PopUp.Prompt("OverWrite?", "O/W?", delegate(bool c, object a) { if (c) { completeSave(); PopUp.Toast("Level '" + name + "' was overwritten."); } return true; });
            }
            else { PopUp.Toast("Level Saved as " + name); completeSave(); }
        }
    }
}
