using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Source.Collisions
{
    /// <summary>
    /// A wall is pretty much a body with a color and specific width
    /// </summary>
    public class Wall : Body
    {
        public const float WALL_WIDTH = 0.4f;

        public int Health;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        /// <param name="height"></param>
        /// <param name="health">-1 makes wall invincible</param>
        /// <param name="rotation"></param>
        public Wall(Texture2D texture, Vector2 position, float height, int health, float rotation = 0f)
            : base(texture, position, new Vector2(WALL_WIDTH, height), rotation)
        {
            Health = health < 0 ? 90000 : health;   // a very large number is just infinite health
            SetColor();
        }

        public void SetColor()
        {
            Color = new Color(Color.AliceBlue.ToVector3() * (1f - ((float)Health - 1f) / (GameData.WALL_HEALTH)));
        }
    }
}
