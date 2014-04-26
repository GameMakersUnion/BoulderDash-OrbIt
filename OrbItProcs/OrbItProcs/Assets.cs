﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public enum textures
    {
        rune1,
        rune2,
        rune3,
        rune4,
        rune5,
        rune6,
        rune7,
        rune8,
        rune9,
        rune10,
        rune11,
        rune12,
        rune13,
        rune14,
        rune15,
        rune16,
        whitecircle,
        orientedcircle,
        blackorb,
        whitesphere,
        ring,
        whiteorb,
        blueorb,
        colororb,
        whitepixel,
        whitepixeltrans,
        sword,
        randompixels,
        innerL,
        innerR,
        outerL,
        outerR,
        pointer,
        itemLight,
        itemWhisper,
        fist,
        cage,
        robot1,
    }

    static class Assets
    {
        public const string filepath = "Presets//Nodes/";
        public const string levelsFilepath = "Presets//Levels/";

        public static SpriteFont font;
        public static Dictionary<textures, Texture2D> textureDict;
        public static Dictionary<textures, Vector2> textureCenters;
        public static Texture2D[,] btnTextures;
        public static ObservableCollection<object> NodePresets = new ObservableCollection<object>();
        public static Effect shaderEffect; // Shader code
        

        public static void LoadAssets(ContentManager content)
        {
            if (!Directory.Exists(filepath)) Directory.CreateDirectory(filepath);
            textureDict = new Dictionary<textures, Texture2D>(){
            { textures.blueorb, content.Load<Texture2D>("Textures/bluesphere"               )},
            { textures.whiteorb, content.Load<Texture2D>("Textures/whiteorb"                )},
            { textures.colororb, content.Load<Texture2D>("Textures/colororb"                )},
            { textures.whitepixel, content.Load<Texture2D>("Textures/whitepixel"            )},
            { textures.whitepixeltrans, content.Load<Texture2D>("Textures/whitepixeltrans"  )},
            { textures.whitecircle, content.Load<Texture2D>("Textures/whitecircle"          )},
            { textures.whitesphere, content.Load<Texture2D>("Textures/whitesphere"          )},
            { textures.blackorb, content.Load<Texture2D>("Textures/blackorb"                )},
            { textures.ring, content.Load<Texture2D>("Textures/ring"                        )},
            { textures.orientedcircle, content.Load<Texture2D>("Textures/orientedcircle"    )},
            { textures.sword, content.Load<Texture2D>("Textures/sword"                      )},
            { textures.randompixels, content.Load<Texture2D>("Textures/randompixels"        )},
            { textures.innerL, content.Load<Texture2D>("Textures/innerL"                    )},
            { textures.innerR, content.Load<Texture2D>("Textures/innerR"                    )},
            { textures.outerL, content.Load<Texture2D>("Textures/outerL"                    )},
            { textures.outerR, content.Load<Texture2D>("Textures/outerR"                    )},
            { textures.pointer, content.Load<Texture2D>("Textures/pointer"                  )},
            { textures.itemLight, content.Load<Texture2D>("Textures/itemLight"              )},
            { textures.itemWhisper, content.Load<Texture2D>("Textures/itemWhisper"          )},
            { textures.cage, content.Load<Texture2D>("Textures/cage"                        )},
            { textures.fist, content.Load<Texture2D>("Textures/fist"                        )},
            { textures.robot1, content.Load<Texture2D>("Textures/Robot1"                    )},
            };

            for (int i = 0; i < 16; i++)
            {
                textures rune = (textures)i;
                string s = "Textures/Runes/" + (i + 1) + " symboli";
                textureDict.Add(rune, content.Load<Texture2D>(s));
            }

            textureCenters = new Dictionary<textures, Vector2>();
            foreach (var tex in textureDict.Keys)
            {
                Texture2D t = textureDict[tex];
                textureCenters[tex] = new Vector2(t.Width / 2f, t.Height / 2f);
            }

            font = content.Load<SpriteFont>("Courier New");
            shaderEffect = content.Load<Effect>("Effects/Shader");
            btnTextures = content.Load<Texture2D>("Textures/buttons").sliceSpriteSheet(2, 5);
        }


        public static void LoadNodes()
        {
            throw new NotImplementedException("We need to Rework Serialization.");
            foreach (string file in Directory.GetFiles(filepath, "*.xml"))
            {
                try
                {
                    Node presetnode = (Node) OrbIt.game.serializer.Deserialize(file);
                    NodePresets.Add(presetnode);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to deserialize node: {0}", e.Message);
                }
            }
        }

        public static void saveNode(Node node, string name, bool overWrite = false)
        {
            throw new NotImplementedException("We need to Rework Serialization.");
            if (name.Equals("") || node == null) return;
            name = name.Trim();
            string filename = "Presets//Nodes//" + name + ".xml";
            Action completeSave = delegate
            {
                OrbIt.ui.sidebar.inspectorArea.editNode.name = name;
                Node serializenode = new Node(node.room);
                Node.cloneNode(OrbIt.ui.sidebar.inspectorArea.editNode, serializenode);
                OrbIt.game.serializer.Serialize(serializenode, filename);
                Assets.NodePresets.Add(serializenode);
            };

            if (File.Exists(filename))
            { //we must be overwriting, therefore don't update the live presetList
                PopUp.Prompt("OverWrite?", "O/W?", delegate(bool c, object a) { if (c) { completeSave(); PopUp.Toast("Node was overridden"); } return true; });
            }
            else { PopUp.Toast("Node Saved"); completeSave(); }

        }

        internal static void deletePreset(Node p)
        {
            throw new NotImplementedException("We need to Rework Serialization.");
            Console.WriteLine("Deleting file: " + p);
            File.Delete(Assets.filepath + p.name + ".xml");
            Assets.NodePresets.Remove(p);
        }


    }
}