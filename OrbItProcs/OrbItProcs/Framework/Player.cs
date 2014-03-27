using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OrbItProcs
{
    public class PlayerData
    {
        public PlayerData() { }
    }


    public class Player
    {
        public Room room;
        public Node _node;
        public Node node { get { return _node; } set { if (_node != null) _node.player = null; _node = value; if (value != null) value.player = this; } }
        public Body body { get { return node.body; } }

        public int playerIndex;
        public float score { get; set; }

        public Color pColor;
        
        public HalfController halfController;

        public Dictionary<Type, PlayerData> playerDatas = new Dictionary<Type, PlayerData>();

        public static Player GetNew(int playerIndex)
        {
            bool success = false;
            Player p = new Player(playerIndex, ref success);
            return success ? p : null;
        }

        public T Data<T>() where T : PlayerData
        {
            Type t = typeof(T);
            if (playerDatas.ContainsKey(t)) return (T)playerDatas[t];
            return null;
        }
        public bool HasData<T>() where T : PlayerData
        {
            return playerDatas.ContainsKey(typeof(T));
        }

        public Player(int playerIndex, ref bool sucess)
        {
            halfController = HalfController.GetNew(playerIndex, FullPadMode.mirrorMode);
            if (halfController == null)
            {
                sucess = false;
                return;
            }
            sucess = true;
            room = Program.getRoom();
            score = 0;
            this.playerIndex = playerIndex;
            
            switch (playerIndex)
            {
                case 1: pColor = Color.Blue; break;
                case 2: pColor = Color.Green; break;
                case 3: pColor = Color.Red; break;
                case 4: pColor = Color.Yellow; break;
            }
            //SwitchPlayerNode(node);
            room.masterGroup.fullSet.Add(node);

            Action<Node, Node> switchPlayer = (n1, n2) =>
            {
                
            };
        }
        //bool switchAvailable = true;
        //public void SwitchPlayerNode(Node n, bool addToSavedNodes = true, bool requireKeypress = false, Player other = null)
        //{
        //    if (!switchAvailable || n == null || n.body.mass == 0 ) return;
        //
        //    if (requireKeypress && !UserInterface.keybState.IsKeyDown(Keys.LeftControl)) return;
        //    if (other != null) //swapping with player
        //    {
        //        node.body.color = other.node.body.color;
        //        Node temp = node;
        //        node.collision.SwapCollider(other.node, "trigger");
        //
        //        node = other.node;
        //        node.body.color = pColor;
        //        other.node = temp;
        //
        //        other.switchAvailable = false;
        //        switchAvailable = false;
        //        room.scheduler.doAfterXMilliseconds(nn => other.switchAvailable = true, 1000);
        //    }
        //    else //swapping with non-player node
        //    {
        //        node.body.color = Color.White;
        //        node.body.texture = textures.whitecircle;
        //        if (node != bigtony) node.body.mass = smallmass;
        //        node.collision.SwapCollider(n, "trigger");
        //
        //        Collider col = node.collision.GetCollider("trigger");
        //        if (col != null) col.HandlersEnabled = false;
        //
        //        Collider col2 = n.collision.GetCollider("trigger");
        //        if (col2 != null) col2.HandlersEnabled = true;
        //
        //        node = n;
        //        if (node != bigtony) node.body.mass = bigmass;
        //
        //        
        //        node.body.texture = textures.blackorb;
        //        node.body.color = pColor;
        //    }
        //    
        //    switchAvailable = false;
        //    if (Game1.soundEnabled) Scheduler.start.Play(0.2f, 0f, 0f);
        //    room.scheduler.doAfterXMilliseconds(nn => this.switchAvailable = true, 1000);
        //}


        HalfPadState newHalfPadState;
        HalfPadState oldHalfPadState;

        public void Update(GameTime gametime)
        {
            /*if (node == null) return;
            newHalfPadState = halfController.getState();
            if (node != bigtony) node.collision.colliders["trigger"].radius = body.radius * 1.5f;
            else node.collision.colliders["trigger"].radius = body.radius * 1.2f;

            Vector2 stick;

            bool clicked = false;
            stick = newHalfPadState.stick1.v2;
            clicked = newHalfPadState.Btn3 == ButtonState.Pressed || newHalfPadState.Btn1 == ButtonState.Pressed;
            stick.Y *= -1;
            stick *= 0.4f;
            stick *= absaccel;
            //stick = new Vector2(0, 0);
            if ((body.velocity.X != 0 || body.velocity.Y != 0))
            {
                stick += body.velocity * -friction;
            }
            stick *= body.mass;
            //todo: update maxvel?
            body.ApplyForce(stick);

            if (clicked)
            {
                SwitchPlayer(stick);
            }
            oldHalfPadState = newHalfPadState;*/
        }

        public virtual void Draw()
        {

        }

        public void SwitchPlayer(Vector2 stick)
        {
            /*var list = node.collision.colliders["trigger"].previousCollision;

            foreach(var p in room.players)
            {
                if (p == this) continue;
                if (list.Contains(p.node.body))
                {
                    SwitchPlayerNode(p.node, false, false, p);
                    return;
                }
            }
            Node n = null;
            float master = -10000000f;
            float current;
            var playernodes = room.playerNodes;
            foreach (Collider q in list)
            {
                if (q.parent == node || playernodes.Contains(q.parent)) continue; //fixed?
                Vector2 dir = q.pos - body.pos; VMath.NormalizeSafe(ref dir);
                Vector2 stickN = stick; VMath.NormalizeSafe(ref stickN);

                if ((current = Vector2.Dot(dir, stickN)) > master)
                {
                    master = current;
                    n = q.parent;
                }
            }
            Console.WriteLine(master);

            //n = list.ElementAt(Utils.random.Next(list.Count));
            SwitchPlayerNode(n, false, false);*/
        }
        //
        public static Node launchNode { get; set; }
        public static int bulletlife { get; set; }
        public static int firefreq { get; set; }
        public static int firefreqCounter = 0;
        static Dictionary<dynamic, dynamic> launchProps;
        public static void InitLaunchNode()
        {
            launchProps = new Dictionary<dynamic, dynamic>()
            {
                { nodeE.texture, textures.whitecircle },
                { comp.movement, true },
                { comp.basicdraw, true },
                { comp.laser, true },
                { comp.gravity, true },
            };

            launchNode = new Node(launchProps);
            //launchNode.comps[comp.laser].brightness = 0.5f;
            launchNode.Comp<Laser>().thickness = 3f;
            launchNode.Comp<Movement>().maxVelocity.value = 5;
            firefreq = 1;
        }
        public void FireNode()
        {
            if (node == null) return;
            Vector2 pos = UserInterface.WorldMousePos;
            Node newNode = new Node();
            Node.cloneNode(launchNode, newNode);
            newNode.body.velocity = pos - body.pos;
            newNode.body.pos = body.pos + body.velocity * 5;
            room.game.spawnNode(newNode, lifetime: bulletlife);
        }

        public void FireNodes()
        {
            if (node == null) return;
            if (firefreq != 0)
            {
                if (firefreqCounter++ % firefreq != 0)
                    return;
            }
            //FireNode();

            Vector2 pos = UserInterface.WorldMousePos;
            Node newNode = new Node();
            Node.cloneNode(launchNode, newNode);
            newNode.body.velocity = pos - body.pos;
            newNode.body.pos = body.pos + body.velocity * 5;

            room.game.spawnNode(newNode, lifetime: bulletlife);
        }
    }
}
