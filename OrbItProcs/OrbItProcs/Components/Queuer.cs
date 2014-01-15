using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;



namespace OrbItProcs
{
    [Flags]
    public enum queues
    {
        none = 0x00,
        position = 0x01,
        angle = 0x02,
        scale = 0x04,
        customs = 0x08,
    }

    public class Queuer : Component
    {
        private queues _qs = queues.none;
        public queues qs { get { return _qs; } set { _qs = value; } }

        public Queue<Vector2> positions;
        public Queue<float> angles;
        public Queue<float> scales;
        private int _queuecount = 10;
        public int queuecount { get { return _queuecount; }
            set
            {
                _queuecount = value;
            }
        } 


        private Dictionary<queues,int> _qcounts = new Dictionary<queues,int>();
        public Dictionary<queues, int> qcounts { get { return _qcounts; } set { _qcounts = value; } }

        private int timer = 0, _timerMax = 1;
        public int timerMax { get { return _timerMax; } set { _timerMax = value; } }

        private Dictionary<string, QueueInfo> _customqueues = new Dictionary<string, QueueInfo>();
        public Dictionary<string, QueueInfo> customqueues { get { return _customqueues; } set { _customqueues = value; } }

        public Queuer() : this(null) { }
        public Queuer(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.queuer;
            methods = mtypes.affectself;
            InitializeLists();
            
            qcounts[queues.position] = queuecount;
            qcounts[queues.angle] = queuecount;
            qcounts[queues.scale] = queuecount;

        }

        public override void OnSpawn()
        {
            if (parent.comps.ContainsKey(comp.waver))
            {
                positions = parent.comps[comp.waver].metapositions;
            }
        }

        public override void InitializeLists()
        {
            positions = new Queue<Vector2>();
            angles = new Queue<float>();
            scales = new Queue<float>();
        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;
            if (customqueues.Count > 0) qs = qs | queues.customs;
        }


        public override void AffectSelf()
        {
            

            if (++timer % timerMax == 0)
            {
                if ((qs & queues.position) == queues.position)
                {
                    //TODO: make all three of these pull from the qcounts dictionary when convienient (dictionary editable in sidebar)
                    if (positions.Count > queuecount)
                    {
                        positions.Dequeue();
                    }
                    positions.Enqueue(parent.transform.position);
                }
                if ((qs & queues.scale) == queues.scale)
                {

                    if (scales.Count > queuecount)
                    {
                        scales.Dequeue();
                    }
                    scales.Enqueue((float)parent.transform.scale);
                }
                if ((qs & queues.angle) == queues.angle)
                {

                    if (angles.Count > queuecount)
                    {
                        angles.Dequeue();
                    }
                    float angle = (float)(Math.Atan2(parent.transform.velocity.Y, parent.transform.velocity.X) + (Math.PI / 2));
                    angles.Enqueue(angle);
                }
                if ((qs & queues.customs) == queues.customs)
                {
                    foreach (QueueInfo qinfo in customqueues.Values)
                    {
                        qinfo.TriggerQueueify();
                    }
                }
            }
        }

        public void AddCustomQueue (string queuename, QueueInfo queueinfo)
        {
            if (!customqueues.ContainsKey(queuename))
            {
                customqueues.Add(queuename, queueinfo);
            }
            qs = qs | queues.customs;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

    }
}
