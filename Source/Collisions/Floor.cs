using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Source.Collisions
{
    /// <summary>
    /// A floor is pretty much a body with a color and specific height
    /// </summary>
    public class Floor : Body
    {
        private const float FLOOR_HEIGHT = 0.2f;

        public Floor(Texture2D texture, Vector2 position, float width, float rotation = 0f) : base(texture, position, new Vector2(width, FLOOR_HEIGHT), rotation)
        {
            color = Color.Azure;
        }

        public Floor(Texture2D texture, Vector2 start, Vector2 end)
            : base(texture, Vector2.Zero, Vector2.Zero) {
            color = Color.Azure;

            Vector2 dist = end - start;
            Position = start + dist / 2;
            Size = new Vector2(dist.Length(), FLOOR_HEIGHT);
            Rotation = (float)Math.Atan2(dist.Y, dist.X);
        }
    }
}
