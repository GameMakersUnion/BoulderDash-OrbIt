using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Microsoft.Xna.Framework;
using TomShane.Neoforce.Controls;
using OrbItProcs;
using Microsoft.Xna.Framework.Input;


namespace OrbItProcs
{
    public class Randomizer : Process
    {
        public Queue<Group> savedGroups = new Queue<Group>();
        public Queue<Dictionary<dynamic, dynamic>> savedDicts = new Queue<Dictionary<dynamic, dynamic>>();
        int queuePos = 0;

        public Randomizer()
            : base()
        {
            //LeftClick += CreateNode;
            //KeyEvent += KeyEv;
            //RightClick += SpawnFromQueue;
            addProcessKeyAction("createrandom", KeyCodes.LeftClick, OnPress: CreateNode);
            addProcessKeyAction("plus", KeyCodes.OemPlus, OnPress: Plus);
            addProcessKeyAction("minus", KeyCodes.OemMinus, OnPress: Minus);
            addProcessKeyAction("spawnfromqueue", KeyCodes.RightClick, OnPress: SpawnFromQueue);
        }

        public void SpawnFromQueue()
        {
            //System.Console.WriteLine(queuePos + " " + savedDicts.Count);
            if (queuePos >= savedDicts.Count) return;

            Dictionary<dynamic, dynamic> dict = savedDicts.ElementAt(savedDicts.Count - queuePos - 1);
            Group g = savedGroups.ElementAt(savedDicts.Count - queuePos - 1);

            dict[nodeE.position] = UserInterface.WorldMousePos;

            Node n = room.spawnNode(dict, blank: true, lifetime: 5000);
            if (n != null) g.entities.Add(n);
        }

        public void Plus() { queuePos = Math.Min(savedDicts.Count - 1, queuePos + 1); }
        public void Minus() { queuePos = Math.Max(0, queuePos - 1); }
        //public void Enter() { }


        /*
        public void KeyEv()
        {
            if (DetectKeyPress(Keys.OemPlus))
            {
                queuePos = Math.Min(savedDicts.Count - 1, queuePos + 1);
            }
            else if (DetectKeyPress(Keys.OemMinus))
            {
                queuePos = Math.Max(0, queuePos - 1);
            }
        }
        */


        public void CreateNode()
        {

            Vector2 pos = UserInterface.WorldMousePos;

            //new node(s)
            Dictionary<dynamic, dynamic> userP = new Dictionary<dynamic, dynamic>() {
                                { nodeE.position, pos },
            };
            HashSet<comp> comps = new HashSet<comp>() { comp.basicdraw, comp.movement, comp.lifetime };
            HashSet<comp> allComps = new HashSet<comp>();
            foreach (comp c in Enum.GetValues(typeof(comp)))
            {
                allComps.Add(c);
            }

            int enumsize = allComps.Count;
            int total = enumsize - comps.Count;


            Random random = Utils.random;
            int compsToAdd = random.Next(total);

            int a = 21 * 21;
            int randLog = random.Next(a);
            int root = (int)Math.Ceiling(Math.Sqrt(randLog));
            root = 21 - root;
            compsToAdd = root;

            //System.Console.WriteLine(compsToAdd + " " + root);

            int counter = 0;
            while (compsToAdd > 0)
            {
                if (counter++ > enumsize) break;
                int compNum = random.Next(enumsize - 1);
                comp newcomp = (comp)compNum;
                if (comps.Contains(newcomp))
                {
                    continue;
                }
                comps.Add(newcomp);
                compsToAdd--;
            }

            foreach (comp c in comps)
            {
                userP.Add(c, true);
            }


            Node n = room.spawnNode(userP, blank: true, lifetime: 5000);
            if (n != null)
            {
                savedDicts.Enqueue(userP);
                Group p = room.masterGroup.childGroups["Link Groups"];
                Group g = new Group(room, n, p, n.name);
                //p.AddGroup(g.Name, g);
                OrbIt.ui.sidebar.UpdateGroupComboBoxes();
                savedGroups.Enqueue(g);

            }
            //ListBox lst = Game1.ui.sidebar.lstMain;
            //Node newNode = (Node)lst.Items.ElementAt(lst.ItemIndex + 1);
            //System.Console.WriteLine(newNode.name);
        }


    }
}
