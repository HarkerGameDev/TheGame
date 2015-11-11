using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Source.Collisions;

namespace Source.Graphics
{
    /// <summary>
    /// Displays a particle effect. Currently, just displays text
    /// </summary>
    public class Particle
    {
        private const float LIFETIME = 0.17f;
        public enum Type
        {
            Text
        }

        public Vector2 Position { get; private set; }

        public float LiveTime;

        private Type type;

        private SpriteFont font;
        private string text;

        public Particle(Vector2 position, Type type)
        {
            this.Position = position;
            this.type = type;
            LiveTime = LIFETIME;
        }

        public Particle(Vector2 position, SpriteFont font, string text)
        {
            this.font = font;
            this.text = text;
            LiveTime = LIFETIME;

            Position = new Vector2(position.X - ConvertUnits.ToSimUnits(font.MeasureString(text).X / 2f), position.Y);
            type = Type.Text;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            switch (type)
            {
                case Type.Text:
                    spriteBatch.DrawString(font, text, ConvertUnits.ToDisplayUnits(Position), Color.YellowGreen);
                    break;
            }
        }
    }
}
