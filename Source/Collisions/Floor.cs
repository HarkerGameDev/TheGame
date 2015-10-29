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
        public const float FLOOR_HEIGHT = 0.2f;
        public bool JumpUp = true;
        public bool Solid;

        public Floor(Texture2D texture, Vector2 position, float width, float rotation = 0f) : base(texture, position, new Vector2(width, FLOOR_HEIGHT), rotation)
        {
            color = Color.Azure;
        }

        public Floor(Texture2D texture, Vector2 position, Vector2 size, float rotation = 0f)
            : base(texture, position, size, rotation)
        {
            if (rotation % MathHelper.Pi == 0)
                rotation = 0;
            color = Color.Azure;
        }

        public Floor(Texture2D texture, Vector2 start, Vector2 end)
            : base(texture, 
            start + (end - start) / 2, 
            new Vector2((end - start).Length(), FLOOR_HEIGHT),
            (float)Math.Atan2(end.Y - start.Y, end.X - start.X)) {
            color = Color.Azure;
        }
    }
}
