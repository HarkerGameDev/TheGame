﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Source.Collisions
{
    /// <summary>
    /// An obstacle is a thing the player must jump over
    /// </summary>
    public class Obstacle : Body
    {
        public const float OBSTACLE_WIDTH = 0.8f;
        public const float OBSTACLE_HEIGHT = 0.5f;

        public Obstacle(Texture2D texture, Vector2 position, float scale = 1f)
            : base(texture, position, new Vector2(OBSTACLE_WIDTH, OBSTACLE_HEIGHT) * scale)
        {
            Color = Color.DarkSlateBlue;
        }
    }
}
