using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Source.Collisions
{
    /// <summary>
    /// Default body is a rectangle
    /// </summary>
    public abstract class Body
    {
        public Vector2 Position;
        public float Rotation;
        public Vector2 Size;

        protected Color color;

        private Texture2D texture;
        private Vector2 origin;

        public Body(Texture2D texture, Vector2 position, Vector2 size, float rotation = 0f)
        {
            this.texture = texture;
            this.Position = position;
            this.Size = size;
            this.Rotation = rotation;
            color = Color.White;
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(Position), null, color, Rotation, origin, ConvertUnits.ToDisplayUnits(Size), SpriteEffects.None, 0f);
        }
    }
}
