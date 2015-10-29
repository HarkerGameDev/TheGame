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
        public bool Ghost = false;
        public Body Ignore = null;
        public float oldY = 0;
        public int CollideBottom = 0;
        public double TimeSinceDeath = 0;
        public int Score = 0;

        public Player(Texture2D texture, Vector2 position, Color color)
            : base(texture, position, new Vector2(2f, 1.6f))
        {
            this.color = color;
        }

        //public void setRotation(Body body)
        //{
        //    if (body.Size.X > body.Size.Y)
        //        rotation = body.Rotation;
        //    else
        //        rotation = body.Rotation - MathHelper.PiOver2;
        //}

        //public void setRotation(float rot)
        //{
        //    rotation = rot;
        //}
    }
}
