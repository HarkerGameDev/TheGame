using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Source.Collisions
{
    /// <summary>
    /// A player uses a rectangle for collisions (for now)
    /// </summary>
    public class Player : Body
    {
        public bool CanJump = false;
        public double JumpWait = 0.5;
        public bool Ghost = false;
        public float oldY = 0f;

        private float VSS = 0.1f; //Height of sensors
        public Sensor sB;
        public Sensor sT;
        public Sensor sL;
        public Sensor sR;

        public Player(Texture2D texture, Vector2 position)
            : base(texture, position, new Vector2(0.8f, 1.8f))
        {
            color = Color.Red;
            sB = new Sensor(texture, new Vector2(Position.X, Position.Y + Size.Y / 2 + VSS / 2), new Vector2(Size.X - VSS, VSS));
            sT = new Sensor(texture, new Vector2(Position.X, Position.Y - Size.Y / 2 - VSS / 2), new Vector2(Size.X - VSS, VSS));
            sL = new Sensor(texture, new Vector2(Position.X - Size.X / 2 - VSS / 2, Position.Y), new Vector2(VSS, Size.Y - VSS));
            sR = new Sensor(texture, new Vector2(Position.X + Size.X / 2 + VSS / 2, Position.Y), new Vector2(VSS, Size.Y - VSS));
        }

        /// <summary>
        /// Intersection detection function (only works for horizontal and vertical walls)
        /// </summary>
        /// <param name="other">The other Body to check intersection with</param>
        /// <returns>True if intersects and false if not</returns>
        public void Intersects(Body other, float deltaTime)
        {
            sB.Position = new Vector2(Position.X, Position.Y + Size.Y / 2 + VSS / 2);
            sT.Position = new Vector2(Position.X, Position.Y - Size.Y / 2 - VSS / 2);
            sL.Position = new Vector2(Position.X - Size.X / 2 - VSS / 2, Position.Y);
            sR.Position = new Vector2(Position.X + Size.X / 2 + VSS / 2, Position.Y);

            if (sB.Intersects(other))
            {
                Position.Y = other.Position.Y - other.Size.Y / 2 - Size.Y / 2 - VSS;
                Velocity.Y = 0;
                CanJump = true;
            }else if (sT.Intersects(other))
            {
                Position.Y = other.Position.Y + other.Size.Y / 2 + Size.Y / 2 + VSS;
                Velocity.Y = 0;
            }else if (sL.Intersects(other))
            {
                Position.X -= Velocity.X * deltaTime;
                Velocity.X = 0;
            }else if (sR.Intersects(other))
            {
                Position.X -= Velocity.X * deltaTime;
                Velocity.X = 0;
            }
        }

        private bool between(float a, float b, float c)
        {
            return a >= b && a <= c;
        }


        public class Sensor : Body
        {
            public Vector2 Position;
            public Vector2 Size;
            public Sensor(Texture2D texture, Vector2 position, Vector2 size)
                : base(texture, position, size)
            {
                Position = position;
                Size = size;
            }

            public bool Intersects(Body other)
            {
                bool left = between(Position.X - Size.X / 2, other.Position.X - other.Size.X / 2, other.Position.X + other.Size.X / 2);
                bool right = between(Position.X + Size.X / 2, other.Position.X - other.Size.X / 2, other.Position.X + other.Size.X / 2);
                bool top = between(Position.Y - Size.Y / 2, other.Position.Y - other.Size.Y / 2, other.Position.Y + other.Size.Y / 2);
                bool bottom = between(Position.Y + Size.Y / 2, other.Position.Y - other.Size.Y / 2, other.Position.Y + other.Size.Y / 2);
                return (left || right) && (top || bottom);
            }

            private bool between(float a, float b, float c)
            {
                return a >= b && a <= c;
            }
        }
    }
}
