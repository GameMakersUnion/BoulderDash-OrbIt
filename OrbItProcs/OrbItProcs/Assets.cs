using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Point = System.Drawing.Point;
using Color = Microsoft.Xna.Framework.Color;

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
        shoveltip,
        spiderhead,
        spiderleg1,
        rock1,
        boulder1,
        goat,
        gradient1,
        gradient2,
        ridgesR,
        ridgesL,
        boulderShine,
        endLight,
        black,
        controller,
        Logo,
        Player1_1,
        Player1_2,
        Player2_2,
        Player3_1,
        Player3_2,
        Player4_1,
        Player4_2,
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


        public static class Spider{
            const string fp = "Textures//SpiderFrames/";
            public static Texture2D Wait = OrbIt.game.Content.Load<Texture2D>("Textures//SpiderFrames/SpiderAni0001");
            public static Texture2D[] Protect = new Texture2D[23];
            public static Texture2D[] Stab = new Texture2D[25];
            public static Texture2D[] Walk = new Texture2D[28];

            static int protectCount = 0, stabCount = 0, walkCount = 0, masterCount = 0, masterMod = 3;
            public enum state
            {
                waiting,
                protecting,
                stabbing,
                walking,
            }
            public static state State = state.waiting;
            public static float spiderPos = -100f;
            static Texture2D currentTexture = null;
            static int freezeCount = 0, freezeMax = 10;
            static bool frozen = false;
            static public float scale = .6f;
            public static int spiderAttackDamage = 5;
            public static Vector2 finalpos, spiderHead;
            public static void UpdateSpider(Room room)
            {
                if (room.loading) return;
                finalpos = room.gridsystemAffect.position + new Vector2((room.worldWidth - Assets.Spider.Wait.Width*scale)/2, room.gridsystemAffect.gridHeight - (Assets.Spider.Wait.Height / 2) - spiderPos + 400);

                room.camera.Draw(currentTexture, finalpos, Color.White, scale, Layers.Over4, center: false);

                spiderHead = new Vector2(room.gridsystemAffect.gridWidth / 2, room.gridsystemAffect.position.Y + room.gridsystemAffect.gridHeight - spiderPos);
                float radiusReach = 300f;
                
                if (frozen)room.camera.Draw(textures.whitecircle, spiderHead, Color.Red * 0.4f, radiusReach / 64f, 0, Layers.Under5);

                if (masterCount++ % masterMod != 0) return;
                if (State == state.waiting)
                {
                    currentTexture = Assets.Spider.Wait;
                    //room.camera.Draw(Assets.Spider.Wait, finalpos, Color.White, .5f, Layers.Over4, center: false);
                    int r = Utils.random.Next(3);
                    if (r == 0)
                    {
                        State = state.stabbing;
                    }
                    else if (r == 1)
                    {
                        State = state.protecting;
                    }
                    else
                    {
                        State = state.walking;
                    }
                }
                else if (State == state.walking)
                {
                    currentTexture = Walk[walkCount];
                    //Texture2D t = Walk[walkCount];
                    //room.camera.Draw(t, finalpos, Color.Blue, .5f, Layers.Over4, center: false);
                    walkCount++;
                    spiderPos += 1.5f;
                    if (walkCount >= Walk.Length)
                    {
                        walkCount = 0;
                        State = state.waiting;
                    }
                    
                }
                else if (State == state.protecting)
                {
                    currentTexture = Protect[protectCount];
                    if (protectCount == 15)
                    {
                        frozen = true;
                    }
                    if (protectCount == 19)
                    {
                        frozen = false;
                    }
                    if (protectCount == 16 || protectCount == 18)
                    {
                        foreach (Node n in room.masterGroup.fullSet.ToList())
                        {
                            if (!n.IsPlayer)
                            {
                                float dist = Vector2.Distance(n.body.pos, spiderHead);
                                if (dist < 250 && n.body.texture == textures.boulder1)
                                {
                                    n.body.texture = textures.boulderShine;
                                    n.collision.active = false;
                                }

                                continue;
                            }
                            if (Vector2.Distance(n.body.pos, spiderHead) > radiusReach) continue;
                            n.meta.CalculateDamage(null, spiderAttackDamage);
                            //n.body.velocity = new Vector2(0, -2);

                            n.movement.maxVelocity.value = 30f;
                            n.body.velocity = new Vector2(0, -30);
                            CollisionDelegate callback = null;
                            callback = delegate(Node n1, Node n2)
                            {
                                n.movement.maxVelocity.value = 2;
                                n.body.OnCollisionEnter -= callback;
                            };
                            n.body.OnCollisionEnter += callback;
                        }
                    }
                    if (protectCount == 17)
                    {
                        freezeCount++;
                        if (freezeCount < freezeMax) return;
                        freezeCount = 0;
                    }
                    //Texture2D t = Protect[protectCount];
                    //room.camera.Draw(t, finalpos, Color.Green, .5f, Layers.Over4, center: false);
                    protectCount++;
                    spiderPos -= 0;
                    if (protectCount >= Protect.Length)
                    {
                        protectCount = 0;
                        State = state.waiting;
                    }

                }
                else if (State == state.stabbing)
                {
                    currentTexture = Stab[stabCount];
                    //Texture2D t = Stab[stabCount];
                    //room.camera.Draw(t, finalpos, Color.Red, .5f, Layers.Over4, center: false);
                    stabCount++;
                    if (stabCount >= Stab.Length)
                    {
                        stabCount = 0;
                        State = state.waiting;
                    }

                }
            }

            static Spider(){
                int count = 0;
                //var fullList = Directory.GetFiles(fp);
                for (int i = 2; i <= 24; i++)
                {
                    string filename = "Textures//SpiderFrames/SpiderAni00";
                    if (i < 10) filename += "0";
                    filename += i;
                    Protect[i - 2] = OrbIt.game.Content.Load<Texture2D>(filename);
                }

                for (int i = 26; i <= 50; i++)
                {
                    string filename = "Textures//SpiderFrames/SpiderAni00";
                    if (i < 10) filename += "0";
                    filename += i;
                    Stab[i - 26] = OrbIt.game.Content.Load<Texture2D>(filename);
                }

                for (int i = 52; i <= 79; i++)
                {
                    string filename = "Textures//SpiderFrames/SpiderAni00";
                    if (i < 10) filename += "0";
                    filename += i;
                    Walk[i - 52] = OrbIt.game.Content.Load<Texture2D>(filename);
                }
                currentTexture = Wait;
            }
        }
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
            { textures.goat, content.Load<Texture2D>("Textures/Boulder_3"                        )},
            { textures.robot1, content.Load<Texture2D>("Textures/Robot1"                    )},
            { textures.shoveltip, content.Load<Texture2D>("Textures/ShovelTip"              )},
            { textures.spiderhead, content.Load<Texture2D>("Textures/SpiderHead"            )},
            { textures.spiderleg1, content.Load<Texture2D>("Textures/SpiderLeg1"            )},
            { textures.rock1, content.Load<Texture2D>("Textures/RockTexture1"               )},
            { textures.boulder1, content.Load<Texture2D>("Textures/Bolders"                )},
            { textures.gradient1, content.Load<Texture2D>("Textures/gradient"                )},
            { textures.gradient2, content.Load<Texture2D>("Textures/gradient2"                )},
            { textures.ridgesL, content.Load<Texture2D>("Textures/RidgesL"                )},
            { textures.ridgesR, content.Load<Texture2D>("Textures/RidgesR"                )},
            { textures.boulderShine, content.Load<Texture2D>("Textures/boulderShine"                )},
            { textures.endLight, content.Load<Texture2D>("Textures/endLight"                )},
            { textures.black, content.Load<Texture2D>("Textures/GUI/black"               )},
            { textures.controller, content.Load<Texture2D>("Textures/GUI/controller"               )},
            { textures.Logo, content.Load<Texture2D>("Textures/GUI/Logo"               )},
            { textures.Player1_2, content.Load<Texture2D>("Textures/GUI/Player1_2"               )},
            { textures.Player1_1, content.Load<Texture2D>("Textures/GUI/Player1_1"               )},
            { textures.Player2_2, content.Load<Texture2D>("Textures/GUI/Player2_2"               )},
            { textures.Player3_1, content.Load<Texture2D>("Textures/GUI/Player3_1"               )},
            { textures.Player3_2, content.Load<Texture2D>("Textures/GUI/Player3_2"               )},
            { textures.Player4_1, content.Load<Texture2D>("Textures/GUI/Player4_1"               )},
            { textures.Player4_2, content.Load<Texture2D>("Textures/GUI/Player4_2"               )},
            
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


        public static Texture2D ClippedBitmap(Texture2D t2d, Point[] pointsArray,  out Point position)
        {

            MemoryStream mStream = new MemoryStream();
            t2d.SaveAsPng(mStream, t2d.Width, t2d.Height);
            Bitmap texture = new Bitmap(mStream);
            int minX = pointsArray.Min(x => x.X);//margin.X >= 0 ? x.X : x.X + margin.X);
            int maxX = pointsArray.Max(x => x.X);//margin.X <= 0 ? x.X : x.X + margin.X);
            int minY = pointsArray.Min(x => x.Y);//margin.Y >= 0 ? x.Y : x.Y + margin.Y);
            int maxY = pointsArray.Max(x => x.Y);//margin.Y <= 0 ? x.X : x.X + margin.X);
            position = new Point(minX, minY);
            if (maxX - minX <= 0 || maxY - minY <= 0) return Assets.textureDict[textures.whitepixel];
            Bitmap bmp = new Bitmap(maxX - minX, maxY - minY);
            Point[] offset = new Point[pointsArray.Length];
            pointsArray.CopyTo(offset, 0);
            offset = Array.ConvertAll(offset, x => x = new Point(x.X - minX, x.Y - minY));
            Graphics g = Graphics.FromImage(bmp);
            TextureBrush tb = new TextureBrush(texture);
            g.FillPolygon(tb, offset);

            Color[] pixels = new Color[bmp.Width * bmp.Height];
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    System.Drawing.Color c = bmp.GetPixel(x, y);
                    pixels[(y * bmp.Width) + x] = new Color(c.R, c.G, c.B, c.A);
                }
            }

            Texture2D myTex = new Texture2D(
              OrbIt.game.GraphicsDevice,
              bmp.Width,
              bmp.Height);

            myTex.SetData<Color>(pixels);
            return myTex;
        }

    }
}