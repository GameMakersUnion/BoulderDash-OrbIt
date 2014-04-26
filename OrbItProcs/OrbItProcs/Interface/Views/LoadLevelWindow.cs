using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TomShane.Neoforce.Controls;
using EventArgs = TomShane.Neoforce.Controls.EventArgs;
using EventHandler = TomShane.Neoforce.Controls.EventHandler;
using TomShaneSidebar = TomShane.Neoforce.Controls.SideBar;
using System.IO;

namespace OrbItProcs
{
    public class LoadLevelWindow
    {
        public Manager manager;
        public Sidebar sidebar;
        public TomShaneSidebar tomShaneSidebar;
        public int HeightCounter = 5;
        public int LeftPadding = 5;
        public Label lblComp, lblProperties;
        public Button btnLoad, btnCancel;
        //public Control under;
        public NormalView normalView;
        Dictionary<LevelSave, ObservableHashSet<Node>> levelSaves = new Dictionary<LevelSave, ObservableHashSet<Node>>();
        bool previouslyPaused = false;
        Group wallGroup = null;
        ObservableHashSet<Node> previousWallNodes = new ObservableHashSet<Node>();
        public LoadLevelWindow(Sidebar sidebar)//, Control under)
        {
            previouslyPaused = sidebar.ui.IsPaused;
            sidebar.ui.IsPaused = true;
            sidebar.master.Visible = false;
            Control par = sidebar.tbcViews.TabPages[0];
            UserInterface.GameInputDisabled = true;
            this.manager = sidebar.manager;
            this.sidebar = sidebar;
            tomShaneSidebar = new TomShaneSidebar(manager);
            tomShaneSidebar.Init();
            tomShaneSidebar.Left = sidebar.master.Left;
            tomShaneSidebar.Width = par.Width;
            tomShaneSidebar.Top = 20;
            tomShaneSidebar.Height = par.Height + 15;
            tomShaneSidebar.BevelBorder = BevelBorder.All;
            tomShaneSidebar.BevelColor = Color.Black;
            tomShaneSidebar.Left = LeftPadding;
            tomShaneSidebar.Text = "Load Level";
            tomShaneSidebar.BackColor = new Color(30, 60, 30);
            manager.Add(tomShaneSidebar);

            wallGroup = sidebar.room.wallGroup;
            foreach(Node n in wallGroup.entities)
            {
                previousWallNodes.Add(n);
            }

            TitlePanel titlePanelAddComponent = new TitlePanel(sidebar, tomShaneSidebar, "Load Level", false);
            //titlePanelAddComponent.btnBack.Click += Close;
            HeightCounter += titlePanelAddComponent.Height;

            normalView = new NormalView(sidebar, tomShaneSidebar, 0, 100, Height: 350);
            normalView.Width -= 15;
            PopulateLevelSaves();

            normalView.OnSelectionChanged += normalView_OnSelectionChanged;

            btnLoad = new Button(manager);
            btnLoad.Init();
            tomShaneSidebar.Add(btnLoad);
            btnLoad.Text = "Load";
            btnLoad.Top = 500;
            btnLoad.Left = LeftPadding * 4;
            btnLoad.Width = 70;
            btnLoad.Click += btnLoad_Click;

            btnCancel = new Button(manager);
            btnCancel.Init();
            tomShaneSidebar.Add(btnCancel);
            btnCancel.Text = "Cancel";
            btnCancel.Top = 500;
            btnCancel.Left = btnLoad.Left + btnLoad.Width + 50;
            btnCancel.Width = 70;
            btnCancel.Click += btnCancel_Click;
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            foreach (Node n in wallGroup.entities.ToList())
            {
                wallGroup.DiscludeEntity(n);
            }
            foreach (Node n in previousWallNodes)
            {
                wallGroup.IncludeEntity(n);
            }
            CloseWindow();
        }

        void btnLoad_Click(object sender, EventArgs e)
        {
            CloseWindow();
        }

        void normalView_OnSelectionChanged(ViewItem item)
        {
            if (item == null) return;
            if (!(item.obj is LevelSave)) return;
            LevelSave levelSave = (LevelSave)item.obj;
            if (!levelSaves.ContainsKey(levelSave))
            {
                ObservableHashSet<Node> nodes = new ObservableHashSet<Node>();
                for (int i = 0; i < levelSave.polygonVertices.Count; i++)
                {

                    Node newNode = new Node(sidebar.ui.game.mainRoom, ShapeType.ePolygon);
                    Polygon poly = (Polygon)newNode.body.shape;
                    //poly.SetCenterOfMass(vertices);
                    float[] list = levelSave.polygonVertices[i];
                    for (int j = 0; j < list.Length / 2; j++)
                    {
                        poly.vertices[j] = new Vector2(list[j * 2], list[(j * 2) + 1]);
                    }
                    poly.vertexCount = list.Length / 2;
                    newNode.body.pos = new Vector2(levelSave.polygonPositions[i][0], levelSave.polygonPositions[i][1]);
                    newNode.body.SetStatic();
                    newNode.body.orient = 0;
                    newNode.movement.mode = movemode.free;
                    newNode.body.restitution = 1f;
                    newNode.meta.maxHealth.enabled = false;
                    poly.ComputeNormals();
                    nodes.Add(newNode);
                }
                levelSaves[levelSave] = nodes;
            }
            ObservableHashSet<Node> incomingNodes = levelSaves[levelSave];
            foreach (Node n in wallGroup.entities.ToList())
            {
                wallGroup.DiscludeEntity(n);
            }
            foreach (Node n in incomingNodes)
            {
                wallGroup.IncludeEntity(n);
            }

        }
        public void PopulateLevelSaves()
        {
            foreach (string file in Directory.GetFiles(Assets.levelsFilepath, "*.xml"))
            {
                try
                {
                    OrbIt.game.serializer = new Polenter.Serialization.SharpSerializer();
                    LevelSave levelSave = (LevelSave)OrbIt.game.serializer.Deserialize(file);
                    normalView.AddObject(levelSave);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Failed to deserialize level: {0}", ex.Message);
                }
            }
        }

        public void CloseWindow()
        {
            

            UserInterface.GameInputDisabled = false;
            manager.Remove(tomShaneSidebar);
            //under.Visible = true;
            sidebar.master.Visible = true;
            if (!previouslyPaused) sidebar.ui.IsPaused = false;

            foreach (Node n in wallGroup.entities)
            {
                n.collision.UpdateCollisionSet();
            }
        }

    }
}
