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
        public float oldY = 0;
        public int CollideBottom = 0;
        public double TimeSinceDeath = 0;
        public int Score = 0;
        public List<Projectile> Projectiles;

        private Vector2 textureScale;

        public Player(Texture2D texture, Vector2 position, Color color)
            : base(texture, position, new Vector2(0.6f, 1.8f))
        {
            this.Color = color;
            this.textureScale = new Vector2(1.8f / Origin.Y / 2);

            Projectiles = new List<Projectile>();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(Position), null, Color, Rotation, Origin, ConvertUnits.ToDisplayUnits(textureScale), SpriteEffects.None, 0f);
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
