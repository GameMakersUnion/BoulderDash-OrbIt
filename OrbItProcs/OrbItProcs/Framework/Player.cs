﻿using System;
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

        //public Controller controller;
        public Input input;

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
        /*
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
            SetPlayerColor();
        }
        */
        public Player(int playerIndex)
        {
            this.playerIndex = playerIndex;
            this.input = new ControllerFullInput(this, (PlayerIndex)(playerIndex - 1));
            this.room = OrbIt.game.room;
            SetPlayerColor();
        }
        public void SetPlayerColor()
        {
            switch (playerIndex)
            {
                case 1: pColor = Color.Blue; ColorName = "Blue"; break;
                case 2: pColor = Color.Green; ColorName = "Green"; break;
                case 3: pColor = Color.Red; ColorName = "Red"; break;
                case 4: pColor = Color.Yellow; ColorName = "Yellow"; break;
            }
            byte min = 40;
            if (pColor.R == 0) pColor.R = min;
            if (pColor.G == 0) pColor.G = min;
            if (pColor.B == 0) pColor.B = min;
        }
        //
        public static void ResetPlayers(Room room)//todo:fix
        {
            room.groups.player.EmptyGroup();
            Controller.ResetControllers();
            CreatePlayers(room);
            OrbIt.ui.sidebar.playerView.InitializePlayers();
        }

        public static Player[] players = new Player[5]; //0th for keyboard, 1st-4th for controllers (for now)

        public static bool EnablePlayers = true;
        public static void CreatePlayers(Room room)
        {
            room.groups.player.defaultNode = room.masterGroup.defaultNode.CreateClone(room);
            Shooter.MakeBullet(room);
            if (!EnablePlayers) return;
            //def.addComponent(comp.shooter, true);
            
            for (int i = 1; i < 5; i++)
            {
                TryCreatePlayer(room, room.groups.player.defaultNode, i, false);
            }
            //OrbIt.ui.sidebar.playerView.InitializePlayers();
        }

        public static void CheckForPlayers(Room room)
        {
            for (int i = 1; i < 5; i++)
            {
                if (players[i] == null)// && GamePad.GetState((PlayerIndex)(i-1)).IsConnected)
                {
                    TryCreatePlayer(room, room.groups.player.defaultNode, i, true);
                }
            }
        }

        private static void TryCreatePlayer(Room room, Node defaultNode, int playerIndex, bool updateUI)
        {
            GamePadState gamePadState = GamePad.GetState((PlayerIndex)(playerIndex - 1));
            if (!gamePadState.IsConnected || gamePadState.Buttons.Back == ButtonState.Released) return;

            //Player p = Player.GetNew(playerIndex);
            Player p = new Player(playerIndex);
            players[playerIndex] = p;
            if (p == null) return;
            double angle = Utils.random.NextDouble() * Math.PI * 2;
            angle -= Math.PI;
            float dist = 200;
            float x = dist * (float)Math.Cos(angle);
            float y = dist * (float)Math.Sin(angle);
            Vector2 spawnPos = new Vector2((room.worldWidth / 4) * playerIndex - (room.worldWidth / 8), room.worldHeight - 600);// -new Vector2(x, y);
            Node node = defaultNode.CreateClone(room);
            node.body.pos = spawnPos;

            node.name = "player" + p.ColorName;
            node.SetColor(p.pColor);
            //node.addComponent(comp.shooter, true);
            //node.addComponent(comp.sword, true);
            //node.Comp<Sword>().sword.collision.DrawRing = false;
            p.node = node;
            room.groups.player.IncludeEntity(node);
            node.meta.healthBar = Meta.HealthBarMode.Bar;
            //node.OnSpawn();
            node.body.velocity = Vector2.Zero;
            //node.body.mass = 0.1f;
            node.movement.maxVelocity.value = 6f;
            //node.addComponent<LinkGun>(true);
            node.OnSpawn();
            node.texture = textures.robot1;

            if (updateUI)
            {
                OrbIt.ui.sidebar.playerView.InitializePlayers();
            }
        }

        
    }
}
