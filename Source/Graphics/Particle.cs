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
            Text, Texture
        }

        public Vector2 Position { get; set; }

        public float LiveTime;

        public Type type;

        private SpriteFont font;
        private string text;

        private Texture2D texture;
        public float angle;
        public Vector2 velocity;
        public float angularVelocity;
        public Vector2 Size;
        public Color color;

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

        public Particle(Vector2 position, Vector2 size, Texture2D texture, float angle, Vector2 velocity, float angularVelocity, float lifeTime, Color color)
        {
            this.texture = texture;
            this.angle = angle;
            this.velocity = velocity;
            this.angularVelocity = angularVelocity;
            this.color = color;
            Size = size;
            LiveTime = lifeTime;
            Position = position;
            type = Type.Texture;
        }
        


        public void Draw(SpriteBatch spriteBatch)
        {
            switch (type)
            {
                case Type.Text:
                    spriteBatch.DrawString(font, text, ConvertUnits.ToDisplayUnits(Position), Color.YellowGreen);
                    break;

                case Type.Texture:
                    spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(Position), null, color, angle, new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), ConvertUnits.ToDisplayUnits(Size) / new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), SpriteEffects.None, 0f);
                    break;
            }
        }
    }
}
