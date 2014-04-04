﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TomShane.Neoforce.Controls;
using EventHandler = TomShane.Neoforce.Controls.EventHandler;
namespace OrbItProcs
{
    public class GamemodeWindow
    {
        public Manager manager;
        public Sidebar sidebar;
        public Window window;
        public int HeightCounter = 5;
        public int LeftPadding = 5;

        public ComboBox cbMode;
        public Label lblMode;
        public Button btnOk;
        public InspectorView insViewModes, insViewGlobal;
        public GamemodeWindow(Sidebar sidebar)
        {
            this.manager = sidebar.manager;
            this.sidebar = sidebar;

            window = new Window(manager);
            window.Init();
            window.Left = sidebar.master.Left;
            window.Width = sidebar.master.Width;
            window.Top = 0;
            window.Height = 600;
            window.Text = "Game Mode";
            manager.Add(window);

            TitlePanel titlePanel = new TitlePanel(sidebar, window, "Game Mode", true);
            titlePanel.btnBack.Click += (s, e) => window.Close();

            HeightCounter += titlePanel.topPanel.Height + LeftPadding * 2;

            lblMode = new Label(manager);
            lblMode.Init();
            lblMode.Parent = window;
            lblMode.Top = HeightCounter;
            lblMode.Left = LeftPadding;
            lblMode.Text = "Game Mode Options";
            lblMode.Width = 120;
            lblMode.TextColor = Color.Black;

            //cbMode = new ComboBox(manager);
            //cbMode.Init();
            //cbMode.Parent = window;
            //cbMode.Left = lblMode.Left + lblMode.Width;
            //cbMode.Top = HeightCounter;
            //cbMode.Width = 100;
            //
            //foreach (GameModes m in Enum.GetValues(typeof(GameModes)))
            //{
            //    cbMode.Items.Add(m);
            //}
            HeightCounter += lblMode.Height + LeftPadding * 3;

            insViewModes = new InspectorView(sidebar, window, LeftPadding, HeightCounter);
            insViewModes.Width -= 20;
            insViewModes.Height -= 100;
            HeightCounter += insViewModes.Height + LeftPadding * 3;
            insViewModes.SetRootObject(OrbIt.globalGameMode);

            //insViewGlobal = new InspectorView(sidebar, window, LeftPadding, HeightCounter);
            //insViewGlobal.Width -= 20;
            //insViewGlobal.Height -= 100;

            window.Refresh();
        }
    }
    public enum GameModes
    {
        t2vs2,
        tAllvs1,
        FreeForAll,
        Cooperative,
    }
    public enum ScoringModes
    {
        playerKills,
        nodeKills,
        allKills,
        allDamage,
        damageDone,
        leastDamageTaken,
    }
    //public class GameModeManager
    //{
    //    public GlobalGameMode globalGameMode;
    //    //public TeamsGameMode teamsGameMode;
    //    //public FreeForAllGameMode freeForAllGameMode;
    //    //public CooperativeGameMode cooperativeGameMode;
    //}
    public class GlobalGameMode
    {
        public OrbIt game;
        /// <summary>
        /// If enabled, the players will damage each other, multiplied by the given value.
        /// </summary>
        [Info(UserLevel.User, "If enabled, the players will damage each other, multiplied by the given value.")]
        public Toggle<float> playersHurtPlayers { get; set; }
        /// <summary>
        /// If enabled, normal nodes will hurt players upon collision, by the given value.
        /// </summary>
        [Info(UserLevel.User, "If enabled, normal nodes will hurt players upon collision.")]
        public Toggle<float> nodesHurtPlayers { get; set; }
        /// <summary>
        /// If enabled, the players can hurt nodes.
        /// </summary>
        [Info(UserLevel.User, "If enabled, the players can hurt nodes.")]
        public Toggle<float> playersHurtNodes { get; set; }
        /// <summary>
        /// If enabled, non-player-nodes will hurt eachother on collision.
        /// </summary>
        [Info(UserLevel.User, "If enabled, non-player-nodes will hurt eachother on collision.")]
        public Toggle<float> nodesHurtNodes { get; set; }
        /// <summary>
        /// The game mode determines who the players will be aiming to kill during gameplay.
        /// </summary>
        [Info(UserLevel.User, "The game mode determines who the players will be aiming to kill during gameplay.")]
        public GameModes gameMode
        {
            get { return _gameMode; }
            set
            {
                bool changed = value != _gameMode;
                _gameMode = value;
                if (changed) SetUpTeams();
            }
        }
        private GameModes _gameMode;
        /// <summary>
        /// The scoring mode determines how each player will increase their score.
        /// </summary>
        [Info(UserLevel.User, "The scoring mode determines how each player will increase their score.")]
        public ScoringModes scoringMode { get; set; }
        public Dictionary<Player, HashSet<Player>> playerTeammates;

        

        public GlobalGameMode(OrbIt game)
        {
            this.game = game;
            playersHurtPlayers = new Toggle<float>(1f, true);
            nodesHurtPlayers = new Toggle<float>(1f, false);
            playersHurtNodes = new Toggle<float>(1f, true);
            nodesHurtNodes = new Toggle<float>(1f, false);
            scoringMode = ScoringModes.playerKills;
            _gameMode = GameModes.FreeForAll;
            SetUpTeams();
        }

        public void Draw()
        {
            if (gameMode == GameModes.FreeForAll || gameMode == GameModes.Cooperative) return;
            foreach(var p in playerTeammates.Keys)
            {
                if (playerTeammates[p] != null)
                {
                    foreach(var pp in playerTeammates[p])
                    {
                        if (p == pp) continue;
                        Utils.DrawLine(game.room, p.node.body.pos, pp.node.body.pos, 3f, Color.White, Layers.Under1);
                    }
                }
            }
        }

        public void SetUpTeams()
        {
            playerTeammates = new Dictionary<Player, HashSet<Player>>();
            if (gameMode == GameModes.tAllvs1)
            {
                if (game.room.playerGroup.entities.Count <= 2)
                {
                    gameMode = GameModes.FreeForAll;
                    return;
                }
                else if (game.room.playerGroup.entities.Count == 3)
                {
                    var hs = new HashSet<Player>() { game.room.playerGroup.entities.ElementAt(0).player, game.room.playerGroup.entities.ElementAt(1).player };
                    playerTeammates[game.room.playerGroup.entities.ElementAt(0).player] = hs;
                    playerTeammates[game.room.playerGroup.entities.ElementAt(1).player] = hs;
                    playerTeammates[game.room.playerGroup.entities.ElementAt(2).player] = new HashSet<Player>() { game.room.playerGroup.entities.ElementAt(2).player };
                }
                else if (game.room.playerGroup.entities.Count == 4)
                {
                    var hs = new HashSet<Player>() { game.room.playerGroup.entities.ElementAt(0).player, game.room.playerGroup.entities.ElementAt(1).player, game.room.playerGroup.entities.ElementAt(2).player };
                    playerTeammates[game.room.playerGroup.entities.ElementAt(0).player] = hs;
                    playerTeammates[game.room.playerGroup.entities.ElementAt(1).player] = hs;
                    playerTeammates[game.room.playerGroup.entities.ElementAt(2).player] = hs;
                    playerTeammates[game.room.playerGroup.entities.ElementAt(3).player] = new HashSet<Player>() { game.room.playerGroup.entities.ElementAt(3).player };
                }
            }
            else if (gameMode == GameModes.t2vs2)
            {
                if (game.room.playerGroup.entities.Count <= 3)
                {
                    gameMode = GameModes.tAllvs1;
                    return;
                }
                var hs1 = new HashSet<Player>() { game.room.playerGroup.entities.ElementAt(0).player, game.room.playerGroup.entities.ElementAt(1).player };
                var hs2 = new HashSet<Player>() { game.room.playerGroup.entities.ElementAt(2).player, game.room.playerGroup.entities.ElementAt(3).player };
                playerTeammates[game.room.playerGroup.entities.ElementAt(0).player] = hs1;
                playerTeammates[game.room.playerGroup.entities.ElementAt(1).player] = hs1;
                playerTeammates[game.room.playerGroup.entities.ElementAt(2).player] = hs2;
                playerTeammates[game.room.playerGroup.entities.ElementAt(3).player] = hs2;
            }
            else if (gameMode == GameModes.Cooperative)
            {
                HashSet<Player> playerset = new HashSet<Player>();
                foreach (Node n in game.room.playerGroup.entities)
                {
                    playerset.Add(n.player);
                }
                foreach (Node n in game.room.playerGroup.entities)
                {
                    playerTeammates[n.player] = playerset;
                }
            }

        }
        public float DetermineDamage(Node damager, Node damagee, float dmg)
        {
            if (damager.player != null)
            {
                //both players
                if (damagee.player != null)
                {
                    if (gameMode == GameModes.Cooperative) return 0;
                    float mult = playersHurtPlayers.enabled ? playersHurtPlayers.value : 0;
                    if (gameMode == GameModes.FreeForAll) return dmg * mult;
                    if (playerTeammates[damager.player].Contains(damagee.player)) return 0;
                    return dmg * mult;
                    //
                }
                    //player hurting node
                else
                {
                    if (playersHurtNodes.enabled)
                    {
                        return dmg * playersHurtNodes.value;
                    }
                    return 0;
                }
            }
            else
            {
                //node hurting player
                if (damagee.player != null)
                {
                    if (nodesHurtPlayers.enabled)
                    {
                        return dmg * nodesHurtPlayers.value;
                    }
                    return 0;
                }
                    //both nodes
                else
                {
                    if (nodesHurtNodes.enabled)
                    {
                        return dmg * nodesHurtNodes.value;
                    }
                    return 0;
                }
            }
            //return dmg;
        }

    }
    //public class TeamsGameMode
    //{
    //
    //}
    //public class FreeForAllGameMode
    //{
    //
    //}
    //public class CooperativeGameMode
    //{
    //
    //}
    
}