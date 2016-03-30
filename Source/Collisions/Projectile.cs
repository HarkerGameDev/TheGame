using System;
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
    public class Projectile : Polygon
    {
        public float LiveTime;
        public Types Type;

        public enum Types
        {
            Rocket, Hook, Boomerang
        }

        public Projectile(Texture2D texture, Vector2 position, Color color, Types type, Vector2 velocity)
            : base(texture, position, new Vector2(GameData.PROJ_WIDTH, GameData.PROJ_HEIGHT))
        {
            this.Color = color;
            Type = type;
            Velocity = velocity;

            LiveTime = GameData.PROJ_LIVE;
        }
    }
}
