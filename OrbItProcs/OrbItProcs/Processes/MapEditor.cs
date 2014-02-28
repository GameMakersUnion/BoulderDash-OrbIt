using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs
{
    public class MapEditor : Process
    {
        public Level level { get; set; }
        public List<Vector2> verts;


        public MapEditor(Level level) : base()
        {
            this.level = level;
            Draw += DrawEditor;
            verts = new List<Vector2>();
            addProcessKeyAction("placevertice", KeyCodes.LeftClick, OnPress: PlaceVertice);
            addProcessKeyAction("placevertice", KeyCodes.Enter, OnPress: FinishWall);
        }

        public void PlaceVertice()
        {
            int vertX = 0, vertY = 0;
            MouseToGrid(ref vertX, ref vertY);
            Vector2 vert = new Vector2(vertX, vertY);// / room.mapzoom;
            if (verts.Contains(vert))
            {
                if (verts.IndexOf(vert) == 0)
                {
                    FinishWall();
                }
                else
                {
                    verts.Remove(vert);
                }
            }
            else
            {
                verts.Add(vert);
            }
        }

        public void FinishWall()
        {
            if (verts.Count < 3) return;
            Vector2[] vertices = verts.ToArray();
            Node newNode = new Node();
            Node.cloneObject(room.game.ui.sidebar.ActiveDefaultNode, newNode);
            Polygon poly = new Polygon();
            poly.body = newNode.body;
            //poly.FindCentroid(vertices);
            poly.SetCenterOfMass(vertices);
            newNode.body.shape = poly;
            newNode.body.SetStatic();
            newNode.body.SetOrient(0);
            newNode.movement.mode = movemode.free;
            newNode.body.restitution = 1f;
            room.game.spawnNode(newNode, g: room.masterGroup.childGroups["Walls"]);
            verts = new List<Vector2>();
        }

        public void DrawEditor(SpriteBatch batch)
        {
            int vertX = 0, vertY = 0;
            MouseToGrid(ref vertX, ref vertY);
            Vector2 vert = new Vector2(vertX, vertY) / room.mapzoom;

            Texture2D tx = room.game.textureDict[textures.whitecircle];
            Vector2 cen = new Vector2(tx.Width / 2f, tx.Height / 2f);//store this in another textureDict to avoid recalculating

            batch.Draw(room.game.textureDict[textures.whitecircle], vert, null, Color.White, 0f, cen, 0.3f, SpriteEffects.None, 0);

            foreach(Vector2 v in verts)
            {
                batch.Draw(room.game.textureDict[textures.whitecircle], v / room.mapzoom, null, Color.Red, 0f, cen, 0.3f, SpriteEffects.None, 0);
            }
        }



        public void MouseToGrid(ref int x, ref int y)
        {
            Vector2 MousePos = UserInterface.WorldMousePos;
            double dx = MousePos.X / (double)level.cellWidth;
            double dy = MousePos.Y / (double)level.cellHeight;

            x = (int)Math.Floor(dx + 0.5) * level.cellWidth;
            y = (int)Math.Floor(dy + 0.5) * level.cellHeight;
        }
    }
}
