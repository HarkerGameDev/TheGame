﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Source.Collisions
{
    /// <summary>
    /// A projectile can hit stuff and has no gravity
    /// </summary>
    public class Projectile : Body
    {
        public float LiveTime;
        public Types Type;

        public enum Types
        {
            Rocket, Hook, Boomerang
        }

        public Projectile(Texture2D texture, Vector2 position, Color color, Types type)
            : base(texture, position, new Vector2(GameData.PROJ_WIDTH, GameData.PROJ_HEIGHT))
        {
            this.Color = color;
            Type = type;

            Velocity = new Vector2(GameData.PROJ_SPEED * (float)Math.Cos(Rotation), GameData.PROJ_SPEED * (float)Math.Sin(Rotation));
            LiveTime = 0f;
        }

        public override void Move(float deltaTime) {
            base.Move(deltaTime);
            LiveTime += deltaTime;
        }
    }
}
