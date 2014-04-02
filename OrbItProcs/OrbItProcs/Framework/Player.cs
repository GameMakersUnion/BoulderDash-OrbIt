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
        public Body body { get { return node != null ? node.body : null; } }

        public int playerIndex;

        public Color pColor;
        public string ColorName;

        public Controller controller;

        public Dictionary<Type, PlayerData> playerDatas = new Dictionary<Type, PlayerData>();
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
        public static Player GetNew(int playerIndex)
        {
            bool success = false;
            Player p = new Player(playerIndex, ref success);
            return success ? p : null;
        }
        private Player(int playerIndex, ref bool sucess)
        {
            controller = FullController.GetNew(playerIndex);
            if (controller == null)
            {
                sucess = false;
                return;
            }
            sucess = true;
            room = OrbIt.game.room;
            this.playerIndex = playerIndex;
            
            switch (playerIndex)
            {
                case 1: pColor = Color.Blue; ColorName = "Blue"; break;
                case 2: pColor = Color.Green; ColorName = "Green"; break;
                case 3: pColor = Color.Red; ColorName = "Red"; break;
                case 4: pColor = Color.Yellow; ColorName = "Yellow"; break;
            }
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
            room.spawnNode(newNode, lifetime: bulletlife);
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

            room.spawnNode(newNode, lifetime: bulletlife);
        }
        public static void ResetPlayers()
        {
            Room r = OrbIt.game.room;
            r.playerGroup.EmptyGroup();
            Controller.ResetControllers();
            CreatePlayers();
            OrbIt.ui.sidebar.playerView.InitializePlayers();
        }
        public static bool EnablePlayers = true;
        public static void CreatePlayers()
        {
            if (!EnablePlayers) return;
            Room r = OrbIt.game.room;
            Shooter.MakeBullet();
            Node def = r.masterGroup.defaultNode.CreateClone();
            //def.addComponent(comp.shooter, true);
            r.playerGroup.defaultNode = def;
            for (int i = 1; i < 5; i++)
            {
                Player p = Player.GetNew(i);
                if (p == null) break;
                double angle = Utils.random.NextDouble() * Math.PI * 2;
                angle -= Math.PI;
                float dist = 200;
                float x = dist * (float)Math.Cos(angle);
                float y = dist * (float)Math.Sin(angle);
                Vector2 spawnPos = new Vector2(r.worldWidth / 2, r.worldHeight / 2) - new Vector2(x, y);
                Node node = def.CreateClone();
                node.body.pos = spawnPos;
                node.name = "player" + p.ColorName;
                node.SetColor(p.pColor);
                //node.addComponent(comp.shooter, true);
                //node.addComponent(comp.sword, true);
                //node.Comp<Sword>().sword.collision.DrawRing = false;
                p.node = node;
                r.playerGroup.IncludeEntity(node);
                node.OnSpawn();
            }
        }
    }
}
