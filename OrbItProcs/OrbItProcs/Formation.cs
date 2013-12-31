using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace OrbItProcs
{
    public enum formationtype
    {
        AllToAll,
        NearestN,
        Random,
        Special,

    }

    public class Formation
    {
        Link link;
        formationtype FormationType;
        bool Uninhabited;
        int UpdateFrequency;
        int Clock = 0;
        int NearestNValue;
        Dictionary<Node, ObservableHashSet<Node>> AffectionSets;

        public Formation(   Link link, 
                            formationtype FormationType = formationtype.AllToAll,
                            bool Uninhabited = false,
                            int UpdateFrequency = -1,
                            int NearestNValue = 1)
        {
            this.link = link;
            this.FormationType = FormationType;
            this.Uninhabited = Uninhabited;
            this.UpdateFrequency = UpdateFrequency;
            this.NearestNValue = NearestNValue;
            this.AffectionSets = new Dictionary<Node, ObservableHashSet<Node>>();

            UpdateFormation();



        }

        public void Update()
        {
            if (UpdateFrequency <= 0 || FormationType == formationtype.AllToAll) return;

            if (Clock++ % UpdateFrequency ==  0)
            {
                UpdateFormation();
            }
        }

        public void UpdateFormation()
        {
            AffectionSets = new Dictionary<Node, ObservableHashSet<Node>>();
            if (FormationType == formationtype.AllToAll)
            {
                AllToAll();
            }
            else if (FormationType == formationtype.NearestN)
            {
                NearestN();
            }

        }

        public void AllToAll()
        {
            link.sources.ToList().ForEach(delegate(Node source) {
                AffectionSets[source] = link.targets;
                //HashSet<Node> set = AffectionSets[source];
                //set = new HashSet<Node>();
                //link.targets.ToList().ForEach(delegate(Node target) {
                //    set.Add(target);
                //});
            });
        }

        public void NearestN()
        {
            
            //not effecient if NearestNValue == 1 because it sorts the entire list of distances
            link.sources.ToList().ForEach(delegate(Node source)
            {
                ObservableHashSet<Node> set = AffectionSets[source];
                set = new ObservableHashSet<Node>();

                List<Tuple<float, Node>> DistancesList = new List<Tuple<float, Node>>();
                Comparison<Tuple<float, Node>> comparer = delegate(Tuple<float, Node> first, Tuple<float, Node> second)
                {
                    if (first.Item1 > second.Item1) return -1;
                    else if (first.Item1 < second.Item1) return 1;
                    return 0;
                };

                link.targets.ToList().ForEach(delegate(Node target) {
                    DistancesList.Add(new Tuple<float, Node>(Vector2.DistanceSquared(source.transform.position, target.transform.position), target));
                });

                DistancesList.Sort(comparer);

                int min = Math.Min(NearestNValue,DistancesList.Count);

                for (int i = 0; i < min; i++)
                {
                    set.Add(DistancesList.ElementAt(i).Item2);
                }
            });
        }

                         

    }
}
