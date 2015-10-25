﻿using System;
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
        public bool Ghost = false;
        public Body Ignore = null;
        public int CollideBottom = 0;

        public Player(Texture2D texture, Vector2 position)
            : base(texture, position, new Vector2(0.8f, 1.8f))
        {
            color = Color.Red;
        }

        /// <summary>
        /// Intersection detection function (only works for horizontal and vertical walls) and updates player status appropriately
        /// </summary>
        /// <param name="other">The other Body to check intersection with</param>
        /// <returns>True if intersects and false if not</returns>
        public bool Intersects(Body other)
        {
            if (other.Rotation == 0f)
            {
                if (Bottom > other.Top && Bottom < other.Bottom && Right < other.Right && Left > other.Left)
                    CanJump = true;
                return Bottom > other.Top && Top < other.Bottom && Left < other.Right && Right > other.Left;
            }
            else
            {
                return other.Contains(this);
            }
        }

        public void setRotation(Body body)
        {
            if (body.Size.X > body.Size.Y)
                rotation = body.Rotation;
            else
                rotation = body.Rotation - MathHelper.PiOver2;
        }

        public void setRotation(float rot)
        {
            rotation = rot;
        }
    }
}
