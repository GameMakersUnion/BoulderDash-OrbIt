﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace OrbItProcs
{
    /// <summary>
    /// The symmetry component allows you to spawn a set of nodes than move in a pattern, based on the symmetry to their starting conditions... and a link.
    /// </summary>
    [Info(UserLevel.User, "The symmetry component allows you to spawn a set of nodes than move in a pattern, based on the symmetry to their starting conditions... and a link.", CompType)]
    public class Symmetry : Component
    {
        public const mtypes CompType = mtypes.playercontrol | mtypes.item | mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }

        public Symmetry() : this(null) { }
        public Symmetry(Node parent)
        {
            this.parent = parent;
        }
        public override void PlayerControl(Controller controller)
        {
            if (!(controller is FullController)) return;
            FullController fc = (FullController)controller;
            if (fc.newGamePadState.Triggers.Right > 0.5f && fc.oldGamePadState.Triggers.Right < 0.5f)
            {
                RandomizeSymmetry();
            }
            else if (fc.newGamePadState.Buttons.RightShoulder == Microsoft.Xna.Framework.Input.ButtonState.Pressed
                && fc.oldGamePadState.Buttons.RightShoulder == Microsoft.Xna.Framework.Input.ButtonState.Released)
            {
                if (links.Count != 0)
                {
                    DestroyLink(links.Dequeue());
                }
            }
            else if (fc.newGamePadState.Buttons.LeftShoulder == Microsoft.Xna.Framework.Input.ButtonState.Pressed
                && fc.oldGamePadState.Buttons.LeftShoulder == Microsoft.Xna.Framework.Input.ButtonState.Released)
            {
                int count = links.Count;
                for (int i = 0; i < count; i++)
                {
                    DestroyLink(links.Dequeue());
                }
            }
        }
        private void DestroyLink(Link link)
        {
            link.active = false;
            link.sourceNode.group.DiscludeEntity(link.sourceNode);
            foreach (Node n in link.targets)
            {
                n.group.DiscludeEntity(n);
            }
        }

        Queue<Link> links = new Queue<Link>();
        public void RandomizeSymmetry()
        {
            Group group = room.groups.general.childGroups.Values.ElementAt(0);
            Color color = Utils.randomColor();
            Vector2 center = parent.body.pos;
            float dist = (float)Utils.random.NextDouble() * 100f + 20f;
            float angle = (float)Utils.random.NextDouble() * GMath.TwoPI;
            float speed = (float)Utils.random.NextDouble() * 5f + 1f;
            int numberOfNodes = Utils.random.Next(10) + 2;

            float angleIncrement = GMath.TwoPI / numberOfNodes;

            Node centerNode = room.defaultNode.CreateClone(room);
            centerNode.body.color = color;
            centerNode.collision.active = false;
            centerNode.body.pos = center;
            room.spawnNode(centerNode, lifetime: -1, g: group);
            HashSet<Node> outerNodes = new HashSet<Node>();
            for(int i = 0; i < numberOfNodes; i++)
            {
                Node n = centerNode.CreateClone(room);
                float angleFromCenter = angleIncrement * i;
                Vector2 spawnPosition = (VMath.AngleToVector(angleFromCenter) * dist) + center;
                Vector2 spawnVelocity = VMath.AngleToVector(angleFromCenter + angle) * speed;
                room.spawnNode(n, lifetime: -1, g: group);
                n.body.pos = spawnPosition;
                n.body.velocity = spawnVelocity;
                n.body.radius = 5f;
                n.body.mass = 10f;
                n.body.color = color * 0.5f;
                n.movement.mode = movemode.free;
                outerNodes.Add(n);

                //n.addComponent<PhaseOrb>(true);
                //n.Comp<PhaseOrb>().phaserLength = 200;

                //n.addComponent<Laser>(true);
                //n.Comp<Laser>().laserLength = 200;

                n.addComponent<Waver>(true);
                n.Comp<Waver>().waveLength = 200;
                n.Comp<Waver>().reflective = true;

                n.addComponent<ColorChanger>(true);
            }
            centerNode.movement.active = false;
            centerNode.basicdraw.active = false;

            Gravity grav = new Gravity();
            grav.multiplier = 20f;
            grav.radius = float.MaxValue;

            Spring spring = new Spring();
            spring.restdist = 100;
            spring.radius = float.MaxValue;

            Link link = new Link(centerNode, outerNodes, grav);
            link.active = true;
            link.DrawLinkLines = false;
            links.Enqueue(link);
        }

    }
}
