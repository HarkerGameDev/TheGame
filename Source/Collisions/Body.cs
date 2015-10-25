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
        public Vector2 Velocity;

        public float Rotation { get { return rotation; } }
        public Vector2 Size { get { return size; } }
        public Vector2 Origin { get { return origin; } }
        public Vector2 Position { get { return position; } }

        private float rotation;
        private Vector2 size;
        private Vector2 position;

        private Vector2[] Points;
        private Vector2[] Edges;

        private Vector2 origin;

        protected Color color;

        private Texture2D texture;

        public float Left { get { return position.X - size.X / 2; } }
        public float Right { get { return position.X + size.X / 2; } }
        public float Top { get { return position.Y - size.Y / 2; } }
        public float Bottom { get { return position.Y + size.Y / 2; } }

        public Body(Texture2D texture, Vector2 pos, Vector2 scale, float rot = 0f)
        {
            this.texture = texture;
            position = pos;
            size = scale;
            rotation = MathHelper.WrapAngle(rot);

            Velocity = Vector2.Zero;
            color = Color.White;
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);

            Points = new Vector2[4];
            Vector2 half = Size / 2;
            Points[0] = RotatePoint(position, new Vector2(position.X - half.X, position.Y - half.Y), rotation);
            Points[1] = RotatePoint(position, new Vector2(position.X + half.X, position.Y - half.Y), rotation);
            Points[2] = RotatePoint(position, new Vector2(position.X + half.X, position.Y + half.Y), rotation);
            Points[3] = RotatePoint(position, new Vector2(position.X - half.X, position.Y + half.Y), rotation);

            Edges = new Vector2[4];
            for (int x = 0; x < Edges.Length; x++)
            {
                Edges[x] = new Vector2(Points[x].X - Points[(x + 1) % 4].X, Points[x].Y - Points[(x + 1) % 4].Y);
            }
        }

        private Vector2 RotatePoint(Vector2 pivot, Vector2 point, float angle)
        {
            double leX = ((Math.Cos(angle) * (point.X - pivot.X)) -
                               (Math.Sin(angle) * (point.Y - pivot.Y)) +
                               pivot.X);
            double leY = ((Math.Sin(angle) * (point.X - pivot.X)) +
                               (Math.Cos(angle) * (point.Y - pivot.Y)) +
                               pivot.Y);
            return new Vector2((float)leX, (float)leY);
        }

        //private bool Between(float val, float min, float max)
        //{
        //    return val >= min && val <= max;
        //}

        public bool TestPoint(Vector2 point)
        {
            return false;
        }

        public void Move(float deltaTime)
        {
            Vector2 move = Velocity * deltaTime;
            position += move;
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] += move;
            }
        }

        public void MovePosition(Vector2 by)
        {
            position += by;
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] += by;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(position), null, color, rotation, origin, ConvertUnits.ToDisplayUnits(size), SpriteEffects.None, 0f);

            // testing corners.
            Vector2 testingSize = new Vector2(10, 10);
            spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(Points[0]), null, Color.Purple, 0f, origin, testingSize, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(Points[1]), null, Color.Orange, 0f, origin, testingSize, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(Points[2]), null, Color.Yellow, 0f, origin, testingSize, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(Points[3]), null, Color.Black, 0f, origin, testingSize, SpriteEffects.None, 0f);

            //Vector2 dist = tL - bL;
            //float x = bL.X + 1f;
            //Vector2 pos = new Vector2(x, (dist.Y / dist.X) * (x - bL.X) + bL.Y);
            //spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(pos), null, Color.Pink, 0f, origin, testingSize, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Intersection detection function, stores results in WillIntersect and Intersect
        /// Mostly derived from http://www.codeproject.com/Articles/15573/2D-Polygon-Collision-Detection
        /// </summary>
        /// <param name="other">The other Body to check intersection with</param>
        /// <returns>Vector2.Zero for no intersection, and minimum translation vector if there is an intersection</returns>
        public Vector2 Intersects(Body other)
        {
            //Separating Axis Theorem
            int edgeCountA = 4;
            int edgeCountB = 4;
            float minIntervalDistance = 0;
            Vector2 translationAxis = Vector2.Zero;
            Vector2 edge;

            for (int edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
            {
                if (edgeIndex < edgeCountA)
                {
                    edge = this.Edges[edgeIndex];
                }
                else
                {
                    edge = other.Edges[edgeIndex - edgeCountA];
                }

                // ===== 1. Find if the polygons are currently intersecting =====

                // Find the axis perpendicular to the current edge
                Vector2 axis = new Vector2(-edge.Y, edge.X);
                axis.Normalize();

                // Find the projection of the polygon on the current axis
                float minA = 0, minB = 0, maxA = 0, maxB = 0;
                ProjectRectangle(axis, this, ref minA, ref maxA);
                ProjectRectangle(axis, other, ref minB, ref maxB);

                // Check if the polygon projections are currentlty intersecting
                float intervalDistance = IntervalDistance(minA, maxA, minB, maxB);
                if (intervalDistance > 0)
                {
                    return Vector2.Zero;
                }

                // Check if the current interval distance is the minimum one. If so store
                // the interval distance and the current distance.
                // This will be used to calculate the minimum translation Vector2
                if (intervalDistance > minIntervalDistance || minIntervalDistance == 0)
                {
                    minIntervalDistance = intervalDistance;
                    translationAxis = axis;

                    Vector2 d = this.position - other.position;
                    if (d.X * translationAxis.X + d.Y * translationAxis.Y < 0)
                        translationAxis = -translationAxis;
                }
            }
            //Console.WriteLine(minIntervalDistance);
            return translationAxis * minIntervalDistance;
        }

        private static void ProjectRectangle(Vector2 axis, Body body, ref float min, ref float max)
        {
            // Project a point on an axis using the dot product.
            float dotProduct = axis.X * body.Points[0].X + axis.Y * body.Points[0].Y;
            min = dotProduct;
            max = dotProduct;
            for (int i = 0; i < body.Points.Length; i++)
            {
                dotProduct = axis.X * body.Points[i].X + axis.Y * body.Points[i].Y;
                if (dotProduct < min)
                {
                    min = dotProduct;
                }
                else
                {
                    if (dotProduct > max)
                    {
                        max = dotProduct;
                    }
                }
            }
        }

        private static float IntervalDistance(float minA, float maxA, float minB, float maxB)
        {
            if (minA < minB)
            {
                return minB - maxA;
            }
            else
            {
                return minA - maxB;
            }
        }
    }
}
