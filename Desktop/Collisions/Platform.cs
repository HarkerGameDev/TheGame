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
    public class Platform : Polygon
    {
        /// <summary>
        /// Creates a new platform
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="position">Center of the platform</param>
        /// <param name="size">Size of the platform</param>
        /// <param name="rotation">Rotation of the platform in radians</param>
        public Platform(Texture2D texture, Vector2 position, Vector2 size, float rotation = 0f)
            : base(texture, position, size, null, rotation)
        {
            Color = Color.White;
        }
    }
}
