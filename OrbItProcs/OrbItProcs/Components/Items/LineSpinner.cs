using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace OrbItProcs
{
    /// <summary>
    /// LineSpinner is an item that deploys lines that rotate randomly from the center of the node, upon pressing the trigger. (Bumpers to remove lines.)
    /// </summary>
    [Info(UserLevel.User, "LineSpinner is an item that deploys lines that rotate randomly from the center of the node, upon pressing the trigger. (Bumpers to remove lines.)", CompType)]
    public class LineSpinner : Component
    {
        public const mtypes CompType = mtypes.item | mtypes.playercontrol;
        public override mtypes compType { get { return CompType; } set { } }
        public LineSpinner() : this(null) { }
        public LineSpinner(Node parent)
        {
            this.parent = parent;
        }
        public override void PlayerControl(Input input)
        {
            if (input.JustPressed(InputButtons.RightTrigger_Mouse1))
            {
                RandomizeSpinningLines();
            }
            else if (input.JustPressed(InputButtons.RightBumper_E))
            {
                if (spinningLines.Count != 0)
                {
                    spinningLines.Dequeue();
                }
            }
            else if (input.JustPressed(InputButtons.LeftBumper_Q))
            {
                int count2 = spinningLines.Count;
                for (int i = 0; i < count2; i++)
                {
                    spinningLines.Dequeue();
                }
            }

            if (spinningLines.Count > 0)
            {
                foreach (var line in spinningLines)
                {
                    line.UpdateLines();
                    line.DrawLines(this);
                }
            }
        }

        Queue<SpinningLine> spinningLines = new Queue<SpinningLine>();
        public void RandomizeSpinningLines()
        {
            Color color = Utils.randomColor();
            Vector2 center = parent.body.pos;
            float maxDist = (float)Utils.random.NextDouble() * 100f + 20f;
            //float angle = (float)Utils.random.NextDouble() * GMath.TwoPI;
            float speed = (float)Utils.random.NextDouble() * 4f + 1f;
            float rotationSpeed = (float)Utils.random.NextDouble() / 50f;
            int numberOfLines = Utils.random.Next(10) + 2;
            //float angleIncrement = GMath.TwoPI / numberOfNodes;
            int copyCount = Utils.random.Next(4);
            float? copyOffset = Utils.random.Next(100) % 2 == 0 ? (float)Utils.random.NextDouble() * GMath.PI : (float?)null;

            int distCopyCount = Utils.random.Next(4) + 1;
            for (int i = 0; i < distCopyCount; i++)
            {
                SpinningLine spin = new SpinningLine(center, rotationSpeed, maxDist, speed, numberOfLines, copyCount, copyOffset, parent.body.color);
                spin.dist = maxDist / distCopyCount * i;
                spinningLines.Enqueue(spin);
            }
        }
    }
    public class SpinningLine
    {
        public Vector2 center;
        public float rotation, rotationSpeed, dist, maxDist, speed;
        public int lineCount, copyCount;
        private float distLengthRatio, angleIncrement, copyOffset;
        public Color? color = null;
        public SpinningLine(Vector2 center, float rotationSpeed, float maxDist, float speed, int lineCount, int copyCount, float? copyOffset, Color? color = null)
        {
            this.center = center;
            this.rotation = 0;
            this.rotationSpeed = rotationSpeed;
            this.maxDist = maxDist;
            this.speed = speed;
            this.dist = 0;
            this.lineCount = lineCount;
            this.color = color;
            this.angleIncrement = GMath.TwoPI / lineCount;
            this.distLengthRatio = (float)Math.Tan(angleIncrement / 2) * 2;
            this.copyCount = copyCount;
            this.copyOffset = copyOffset ?? GMath.TwoPI / copyCount;
        }
        public void UpdateLines()
        {
            rotation = (rotation + rotationSpeed) % GMath.TwoPI;
            dist += speed;
            if (dist > maxDist) { dist = maxDist; speed *= -1; }
            else if (dist < 0) { dist = 0; speed *= -1; }
        }
        public void DrawLines(LineSpinner lineSpinner)
        {
            for (int o = 0; o < copyCount; o++)
            {
                for (int i = 0; i < lineCount; i++)
                {
                    float dirAngle = angleIncrement * i;
                    Vector2 dir = VMath.AngleToVector(dirAngle);
                    dir *= dist;
                    float rotationAngle = dirAngle + GMath.PIbyTwo;//) % GMath.TwoPI;
                    rotationAngle = (rotationAngle + rotation + copyOffset * o) % GMath.TwoPI;
                    float length = dist * distLengthRatio;

                    //symmetry.room.camera.Draw(textures.whitepixel, dir + symmetry.parent.body.pos, color ?? symmetry.parent.body.color, new Vector2(1f, length), rotationAngle, Layers.Under1);
                    lineSpinner.room.camera.AddPermanentDraw(textures.whitepixel, dir + lineSpinner.parent.body.pos, color ?? lineSpinner.parent.body.color, new Vector2(1f, length), rotationAngle, 10);

                }
            }
        }
    }
}
