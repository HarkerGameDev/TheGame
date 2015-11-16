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
    public class Projectile : Body
    {
        public float LiveTime;

        public Projectile(Texture2D texture, Vector2 position, Color color)
            : base(texture, position, new Vector2(1f, 0.25f))
        {
            this.Color = color;

            Velocity = new Vector2(GameData.PROJ_SPEED * (float)Math.Cos(Rotation), GameData.PROJ_SPEED * (float)Math.Sin(Rotation));
            LiveTime = 0f;
        }

        public override void Move(float deltaTime) {
            base.Move(deltaTime);
            LiveTime += deltaTime;
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
