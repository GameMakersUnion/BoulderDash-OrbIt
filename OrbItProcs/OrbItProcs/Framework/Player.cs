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

        public Dictionary<ItemSlots, Component> itemSlots = new Dictionary<ItemSlots, Component>()
        {
            {ItemSlots.Y_Yellow, null},
            {ItemSlots.A_Green, null},
            {ItemSlots.B_Red, null},
            {ItemSlots.X_Blue, null},
        };
        public ItemSlots _currentItem = ItemSlots.Y_Yellow;
        public ItemSlots currentItem { 
            get { return _currentItem; } 
            set 
            {
                foreach (var item in itemSlots.Keys) 
                {
                    if (itemSlots[item] != null)
                    {
                        if (item != value) itemSlots[item].active = false;
                        else itemSlots[item].active = true;
                    }
                }
                _currentItem = value; 
            } 
        }
        public ItemSlots occupiedSlots = ItemSlots.None;

        public void AddItem(Component comp)
        {
            int count = 0;
            foreach(var slot in itemSlots.Keys.ToList())
            {
                if (itemSlots[slot] != null)
                {
                    count++;
                    if (itemSlots[slot].GetType() == comp.GetType())
                    {
                        itemSlots[slot] = comp;
                        if (slot != currentItem) comp.active = false;
                        if (count == 0) currentItem = slot;
                        return;
                    }
                }
            }
            if (count == 4) return;
            foreach (var slot in itemSlots.Keys.ToList())
            {
                if (itemSlots[slot] == null)
                {
                    itemSlots[slot] = comp;
                    occupiedSlots |= slot;
                    if (slot != currentItem) comp.active = false;
                    return;
                }
            }
        }
        public void RemoveItem(Component comp)
        {
            foreach(var slot in itemSlots.Keys.ToList())
            {
                if (comp == itemSlots[slot])
                {
                    occupiedSlots = occupiedSlots ^ slot;
                    itemSlots[slot] = null;
                    return;
                }
            }
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
        public static void ResetPlayers(Room room)
        {
            room.playerGroup.EmptyGroup();
            Controller.ResetControllers();
            CreatePlayers(room);
            OrbIt.ui.sidebar.playerView.InitializePlayers();
        }
        public static bool EnablePlayers = true;
        public static void CreatePlayers(Room room)
        {
            if (!EnablePlayers) return;
            Shooter.MakeBullet(room);
            Node def = room.masterGroup.defaultNode.CreateClone(room);
            //def.addComponent(comp.shooter, true);
            room.playerGroup.defaultNode = def;
            for (int i = 1; i < 5; i++)
            {
                Player p = Player.GetNew(i);
                if (p == null) break;
                double angle = Utils.random.NextDouble() * Math.PI * 2;
                angle -= Math.PI;
                float dist = 200;
                float x = dist * (float)Math.Cos(angle);
                float y = dist * (float)Math.Sin(angle);
                Vector2 spawnPos = new Vector2((room.worldWidth / 4) * i - (room.worldWidth / 8), room.worldHeight - 200);// -new Vector2(x, y);
                Node node = def.CreateClone(room);
                node.body.pos = spawnPos;
                
                node.name = "player" + p.ColorName;
                node.SetColor(p.pColor);
                //node.addComponent(comp.shooter, true);
                //node.addComponent(comp.sword, true);
                //node.Comp<Sword>().sword.collision.DrawRing = false;
                p.node = node;
                room.playerGroup.IncludeEntity(node);
                node.meta.healthBar = Meta.HealthBarMode.Bar;
                node.OnSpawn();
                node.body.velocity = Vector2.Zero;
            }
        }

        
    }
}
