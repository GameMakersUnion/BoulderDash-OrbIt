﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public class GraphData : Process
    {
        public GraphData()
        {
            addProcessKeyAction("showgraphdata", KeyCodes.S, OnPress: ShowFloatGraph);
            addProcessKeyAction("toggledraw", KeyCodes.D, OnPress: TogDraw);
            addProcessKeyAction("toggleactivated", KeyCodes.A, OnPress: TogActivated);
            addProcessKeyAction("clear", KeyCodes.C, OnPress: ClearFloatData);
        }

        static bool drawing = false;
        static bool activated = false;
        static Vector2 min = Vector2.Zero;
        static Vector2 max = Vector2.Zero;
        static float roundFactor = 4f;
        public static Dictionary<float, int> floatData = new Dictionary<float, int>();

        
        public static void ClearFloatData()
        {
            floatData = new Dictionary<float, int>();
        }

        public static void AddFloat(float f)
        {
            if (!activated) return;
            float rounded = f;
            if (roundFactor != -1) rounded = (float)Math.Round(f / roundFactor) * roundFactor;

            if (floatData.ContainsKey(rounded))
            {
                floatData[rounded]++;
            }
            else
            {
                floatData[rounded] = 1;
            }
        }

        public static void ShowFloatGraph()
        {
            min = new Vector2(float.MaxValue, float.MaxValue);
            max = new Vector2(-float.MaxValue, -float.MaxValue);
            foreach (float f in floatData.Keys)
            {
                if (f < min.X) min.X = f;
                if (f > max.X) max.X = f;
                if (floatData[f] < min.Y) min.Y = floatData[f];
                if (floatData[f] > max.Y) max.Y = floatData[f];
            }
            drawing = true;
        }

        public static void TogDraw()
        {
            ToggleDraw();
        }
        public static void ToggleDraw(bool? value = null)
        {
            if (value != null) drawing = (bool)value;
            else drawing = !drawing;
        }
        public static void TogActivated()
        {
            ToggleActivated();
        }
        public static void ToggleActivated(bool? value = null)
        {
            if (value != null) activated = (bool)value;
            else activated = !activated;
        }
        public static void DrawGraph()
        {
            if (!drawing) return;
            ShowFloatGraph();
            float xRange = max.X - min.X;
            float yRange = max.Y - min.Y;
            Room room = Program.getRoom();
            foreach (float f in floatData.Keys)
            {
                float ratio = (f - min.X) / (max.X - min.X);
                float hue = ratio * 360;
                float x = ratio * room.worldWidth;

                float y = (room.worldHeight - ((floatData[f] - min.Y + 1) / (max.Y - min.Y + 1) * room.worldHeight * 0.5f));
                
                Utils.DrawLine(room, new Vector2(x, room.worldHeight), new Vector2(x, y), 1, ColorChanger.getColorFromHSV(hue));
            }
            

        }


        /*public static Dictionary<int, int> intData = new Dictionary<int, int>();
        public void ClearIntData()
        {
            intData = new Dictionary<int, int>();
        }
        public void AddInt(int i)
        {
            if (!intData.ContainsKey(i))
            {
                intData[i]++;
            }
            else
            {
                intData[i] = 1;
            }
        }*/
    }
}
