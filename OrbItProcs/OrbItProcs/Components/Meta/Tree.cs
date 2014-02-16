using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace OrbItProcs
{
    public class Tree : Component
    {
        private int _queuecount = 100;
        public int queuecount { get { return _queuecount; } set { _queuecount = value; } }

        private float r1, g1, b1;

        private Queue<Vector2> positions;
        private Queue<float> scales;

        private int _depth = 0;
        public int depth { get { return _depth; } set { _depth = value; } }
        private int _maxdepth = 4;
        public int maxdepth { get { return _maxdepth; } set { _maxdepth = value; } }
        private float _anglerange = 45;
        public float anglerange { get { return _anglerange; } set { _anglerange = value; } }
        private int _randlife = 20;
        public int randlife { get { return _randlife; } set { _randlife = value; } }
        private int _lifeleft = 0;
        public int lifeleft { get { return _lifeleft; } set { _lifeleft = value; } }
        private int _maxchilds = 3;
        public int maxchilds { get { return _maxchilds; } set { _maxchilds = value; } }

        public Tree() : this(null) { }
        public Tree(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.tree;
            methods = mtypes.affectself | mtypes.draw;
            InitializeLists();

        }

        public override void InitializeLists()
        {
            positions = new Queue<Vector2>();
            scales = new Queue<float>();

        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;
            r1 = Utils.random.Next(255) / 255f;
            g1 = Utils.random.Next(255) / 255f;
            b1 = Utils.random.Next(255) / 255f;
        }

        public override void AffectSelf()
        {
            if (depth == -1)
            {
                //parent.nodeState = state.drawOnly;
                return;
            }
            if (depth > maxdepth)
            {
                lifeleft = -1;
                parent.body.velocity = new Vector2(0, 0);
                depth = -1;
                //return;
            }

            //angle = Math.Atan2(parent.transform.velocity.Y, parent.transform.velocity.X) + (Math.PI / 2);
            float scaledown = 1.0f - 0.01f;
            parent.body.scale *= scaledown;
            if (lifeleft > 0)
            {
                if (positions.Count < queuecount)
                {
                    positions.Enqueue(parent.body.pos);
                    scales.Enqueue(parent.body.scale);

                }
                else
                {
                    positions.Dequeue();
                    positions.Enqueue(parent.body.pos);
                    scales.Dequeue();
                    scales.Enqueue(parent.body.scale);
                }
            }

            //branchdeath
            if (lifeleft > randlife)
            {
                
                //Console.WriteLine("{0} {1} > {2}",parent.name, lifeleft, randlife);
                lifeleft = -1;
                
                int velLength = (int)parent.body.velocity.Length();
                double angle = Math.Atan2(parent.body.velocity.Y, parent.body.velocity.X);

                parent.body.velocity = new Vector2(0, 0);
                //if (parent.comps.ContainsKey(comp.gravity)) parent.comps[comp.gravity].active = false;

                int childcount = Utils.random.Next(maxchilds+1) + 1;
                //Console.WriteLine(childcount);
                for(int i = 0; i < childcount; i++)
                {
                    float childscale = parent.body.scale * scaledown;
                    Vector2 childpos = parent.body.pos;
                    float anglechange = Utils.random.Next((int)anglerange) - (anglerange / 2);
                    //
                    anglechange = anglechange * (float)(Math.PI / 180);
                    angle += anglechange; //we might need to do a Math.Max(360,Math.Min(0,x));
                    Vector2 childvel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * velLength;
                    int randomlife = (Utils.random.Next(randlife - randlife / 10 - 5)) + 5;
                    //int randomlife = randlife;
                    //userP[node.transform.position] = childpos;
                    //userP[node.scale] = childscale;
                    //userP[node.velocity] = childvel;
                    //userP[node.name] = "node" + Node.nodeCounter;

                    Node newNode = new Node();
                    Node.cloneObject(parent, newNode);
                    newNode.body.velocity = childvel;
                    if (newNode.body.velocity.IsFucked()) System.Diagnostics.Debugger.Break();
                    newNode.name = "node" + Node.nodeCounter;
                    //newNode.acceptUserProps(userP);
                    newNode.comps[comp.tree].depth = depth + 1;
                    newNode.comps[comp.tree].randlife = randomlife;
                    newNode.comps[comp.tree].lifeleft = 0;
                    newNode.comps[comp.tree].maxchilds = Math.Max(1,maxchilds - (depth % 2));
                    //parent.room.nodesToAdd.Enqueue(newNode);
                    //parent.room.masterGroup.childGroups.Values.ElementAt(1).IncludeEntity(newNode);
                    TomShane.Neoforce.Controls.ComboBox cmb = parent.room.game.ui.sidebar.cbListPicker;
                    Group g = parent.room.masterGroup.FindGroup(cmb.Items.ElementAt(cmb.ItemIndex).ToString());
                    g.IncludeEntity(newNode);
                }
                //parent.nodeState = state.drawOnly;

                HashSet<Node> hs = new HashSet<Node>();
                

            }
            if (lifeleft >= 0)
            {
                lifeleft++;
            }


        }

        public override void Draw(SpriteBatch spritebatch)
        {
            Room room = parent.room;
            float mapzoom = room.mapzoom;

            Color col = new Color(0, 0, 0, 0.3f);
            float a, b, c;
            a = b = c = 0;

            //Vector2 screenPos = parent.transform.position / mapzoom;
            //Console.WriteLine(parent.transform.scale);
            int count = 0;
            foreach (Vector2 pos in positions)
            {
                //color = new Color(color.R, color.G, color.B, 255/queuecount * count);
                a += r1 / 10;
                b += g1 / 10;
                c += b1 / 10;
                col = new Color(a, b, c, 0.8f);
                if (parent.comps.ContainsKey(comp.hueshifter) && parent.comps[comp.hueshifter].active) col = parent.body.color;

                spritebatch.Draw(parent.getTexture(), pos / mapzoom, null, col, 0, parent.TextureCenter(), scales.ElementAt(count) / mapzoom, SpriteEffects.None, 0);
                count++;
            }

            //float testangle = (float)(Math.Atan2(parent.transform.velocity.Y, parent.transform.velocity.X) + (Math.PI / 2));
            if (parent.comps.ContainsKey(comp.hueshifter)) col = parent.body.color;
            spritebatch.Draw(parent.getTexture(), parent.body.pos / mapzoom, null, col, 0, parent.TextureCenter(), parent.body.scale / mapzoom, SpriteEffects.None, 0);

        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
        }

    }
}
