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
    public class Obstacle : AABB
    {
        public const float OBSTACLE_WIDTH = 2f;
        public const float OBSTACLE_HEIGHT = 1.8f;

        public ObstacleType Type { get; private set; }

        public enum ObstacleType
        {
            Platform
        }

        public Obstacle(Texture2D texture, Vector2 position, Vector2 size, ObstacleType type)
            : base(texture, position, size)
        {
            Type = type;
            Color = Color.SaddleBrown;
        }
    }
}
