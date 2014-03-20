using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Audio;

namespace OrbItProcs
{
    public enum ResumeType
    {
        Forget,
        Stack,
        Accumulate,
        Postpone,
    }
    /// <summary>
    /// Component that keeps track of events occurring related to this node. Mostly for internal use.
    /// </summary>
    [Info(UserLevel.Developer, "Component that keeps track of events occurring related to this node. Mostly for internal use.")]
    public class Scheduler : Component
    {
        public const mtypes CompType = mtypes.affectself |  mtypes.essential;
        public override mtypes compType { get { return CompType; } set { } }
        public static SoundEffect start;
        public static SoundEffect end;
        public static SoundEffect fanfare;
        public override bool active
        {
            get
            {
                return _active;
            }
            set
            {
                if (resumeType == ResumeType.Forget)
                {

                }
                _active = value;
            }
        }

        public ResumeType resumeType = ResumeType.Forget;
        public List<Appointment> appointments = new List<Appointment>();

        public Scheduler() : this(null) { }
        public Scheduler(Node parent = null)
        {
            start = Program.getGame().Content.Load<SoundEffect>("croc");
            end = Program.getGame().Content.Load<SoundEffect>("coin");
            fanfare = Program.getGame().Content.Load<SoundEffect>("fanfare");
            if (parent != null) this.parent = parent;
            com = comp.scheduler;
        }

        public override void OnSpawn()
        {
            Utils.Infect(parent);
            /*Appointment appt = new Appointment(null, Utils.random.Next(10000), 10);
            AppointmentDelegate a = (n, d) => { n.body.color = Utils.randomColor(); appt.interval = Utils.random.Next(10000); };
            appt += a;
            AddAppointment(appt);
            appt.SetTimer();*/
        }

        public void CheckAppointments()
        {
            bool sort = false;
            while(appointments.Count != 0)
            {
                Appointment a = appointments.ElementAt(0);
                if (Room.totalElapsedMilliseconds > a.scheduledTime)
                {
                    a.InvokeAppointment(parent);
                    if (a.infinite || a.repetitions > 1)
                    {
                        appointments.Remove(a);
                        appointments.Add(a);
                        sort = true;
                        if (!a.infinite) a.repetitions--;
                    }
                    else
                    {
                        appointments.Remove(a);
                    }
                }
                else
                {
                    break;
                }
            }
            if (sort)
            {
                SortAppointments();    //appointments.OrderBy(a => a.scheduledTime);
            }
        }
        public void doAfterXMilliseconds(Action<Node> action, int X, bool playSound = false)
        {
            if (playSound) start.Play();
            AppointmentDelegate a = delegate(Node n, DataStore d) { action(n); };
            Appointment appt = new Appointment(a, X, playSound: playSound);
            appt.SetTimer();
            AddAppointment(appt);
            
        }
        public void doEveryXMilliseconds(Action<Node> action, int X, bool playSound = false)
        {
            if (playSound) start.Play();
            AppointmentDelegate a = delegate(Node n, DataStore d) { action(n); };
            Appointment appt = new Appointment(a, X, infinite: true, playSound: playSound);
            appt.SetTimer();
            AddAppointment(appt);
        }
        public void SortAppointments()
        {
            appointments = appointments.OrderBy(a => a.scheduledTime).ToList();
        }

        public void AddAppointment(Appointment app)
        {
            appointments.Add(app);
            SortAppointments();
        }
        public void RemoveAppointment(Appointment app)
        {
            //if (!appointments.Contains(app)) return;
            appointments.Remove(app);
            //appointments.OrderBy(a => a.scheduledTime);
        }
        public void ClearAppointments()
        {
            appointments = new List<Appointment>();
        }

        public override void AffectSelf()
        {
            CheckAppointments();
        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

    }

    public delegate void AppointmentDelegate(Node n, DataStore d);

    public class Appointment /*: IComparer<Appointment>*/
    {
        public List<AppointmentDelegate> actions { get; set; }
        public bool infinite { get; set; }
        public bool playSound { get; set; }
        public int repetitions { get; set; }
        public int interval { get; set; }
        public DataStore dataStore { get; set; }
        public long scheduledTime { get; set; }

        public Appointment(AppointmentDelegate action, int interval, int repetitions = 1, bool infinite = false, DataStore dataStore = null, bool playSound = false)
        {
            actions = new List<AppointmentDelegate>();
            if (action != null) actions.Add(action);
            this.repetitions = repetitions;
            this.infinite = infinite;
            this.dataStore = dataStore;
            this.interval = interval;
            this.scheduledTime = -1;
            this.playSound = playSound;
        }

        public void InvokeAppointment(Node n)
        {
            if (playSound) {
                Scheduler.end.Play(); }
            foreach(var a in actions)
            {
                a(n, dataStore);
            }
            SetTimer();

        }

        public static Appointment operator +(Appointment appt, AppointmentDelegate a)
        {
            appt.AddAction(a);
            return appt;
        }

        public static int Compare(Appointment a1, Appointment a2)
        {
            if (a1.scheduledTime < a2.scheduledTime) return -1;
            if (a1.scheduledTime > a2.scheduledTime) return 1;
            return 0;
        }

        public void SetTimer()
        {
            scheduledTime = Room.totalElapsedMilliseconds + interval;
        }

        public void AddAction(AppointmentDelegate action)
        {
            if (actions.Contains(action))
                Console.WriteLine("Warning: adding duplicate action.");
            actions.Add(action);
        }
        public void RemoveAction(AppointmentDelegate action)
        {
            if (actions.Contains(action)) actions.Remove(action);
        }
    }
}
