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

        public Wall(Texture2D texture, Vector2 position, float height, float rotation = 0f)
            : base(texture, position, new Vector2(WALL_WIDTH, height), rotation)
        {
            Color = Color.Beige;
        }
    }
}
