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

        public Player(Texture2D texture, Vector2 position)
            : base(texture, position, new Vector2(0.8f, 1.8f))
        {
            color = Color.Red;
        }

        /// <summary>
        /// Intersection detection function (only works for horizontal and vertical walls)
        /// </summary>
        /// <param name="other">The other Body to check intersection with</param>
        /// <returns>True if intersects and false if not</returns>
        public bool Intersects(Body other)
        {
            if (Bottom > other.Top && Bottom < other.Bottom && Left < other.Right && Right > other.Left)
                CanJump = true;
            return Bottom > other.Top && Top < other.Bottom && Left < other.Right && Right > other.Left;
        }
    }
}
