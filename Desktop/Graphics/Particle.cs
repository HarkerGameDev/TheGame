﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Source.Collisions;

namespace Source.Graphics
{
    /// <summary>
    /// Displays a particle effect as a texture. Motion is simulated.
    /// </summary>
    public class Particle
    {
        public Texture2D Texture;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Angle;
        public float AngularVelocity;
        public Color Color;
        public float Size;
        public float LiveTime;

        public Particle(Texture2D texture, Vector2 position, Vector2 velocity,
            float angle, float angularVelocity, Color color, float size, float liveTime)
        {
            Texture = texture;
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angularVelocity;
            Color = color;
            Size = size / Texture.Width;
            LiveTime = liveTime;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //Console.WriteLine("Part color: " + Color);
            spriteBatch.Draw(Texture, ConvertUnits.ToDisplayUnits(Position), null, Color,
                Angle, new Vector2(Texture.Width / 2f, Texture.Height / 2f), ConvertUnits.ToDisplayUnits(Size), SpriteEffects.None, 0f);
        }

        public void Update(float deltaTime)
        {
            LiveTime -= deltaTime;
            Position += Velocity * deltaTime;
            Angle += AngularVelocity;
        }
    }
}
