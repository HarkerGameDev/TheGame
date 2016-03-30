using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Source.Collisions
{
    /// <summary>
    /// A body, could be any polygon or circle
    /// </summary>
    public abstract class Body
    {
        public Vector2 Velocity;

        public float Rotation { get; protected set; }
        public Vector2 Size { get; private set; }
        public Vector2 Origin { get; protected set; }
        public Vector2 Position { get; private set; }

        public Color Color;

        protected Vector2[] Points, Edges;

        public Texture2D texture { get; private set; }

        public Body(Texture2D texture, Vector2 position, Vector2 size, float rotation = 0f)
        {
            this.texture = texture;
            Position = position;
            Size = size;
            Rotation = MathHelper.WrapAngle(rotation);

            Velocity = Vector2.Zero;
            Color = Color.White;
            Origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
        }

        /// <summary>
        /// Currently making a small rectangle and using Intersect to TestPoint
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool TestPoint(Vector2 point)
        {
            return Intersects(new AABB(texture, point, Vector2.Zero)) != Vector2.Zero;
        }

        public virtual void Update(float deltaTime)
        {
            Vector2 move = Velocity * deltaTime;
            MoveByPosition(move);
        }

        public void MoveByPosition(Vector2 by)
        {
            Position += by;
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] += by;
            }
        }

        public void MoveToPosition(Vector2 pos)
        {
            MoveByPosition(pos - Position);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(Position), null, Color, Rotation, Origin, ConvertUnits.ToDisplayUnits(Size) / (Origin * 2), SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Intersection detection function
        /// Mostly derived from http://www.codeproject.com/Articles/15573/2D-Polygon-Collision-Detection
        /// </summary>
        /// <param name="other">The other Body to check intersection with</param>
        /// <returns>Vector2.Zero for no intersection, and minimum translation vector if there is an intersection</returns>
        public abstract Vector2 Intersects(AABB other);

        /// <summary>
        /// Gets the parametric T value for the ray on start in direction dir, 0 if nothing
        /// Based on http://ncase.me/sight-and-light/
        /// </summary>
        /// <param name="rayStart"></param>
        /// <param name="rayDir"></param>
        /// <returns></returns>
        public abstract float Raycast(Vector2 rayStart, Vector2 rayDir);
    }
}
