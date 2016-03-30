using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Source.Collisions
{
    /// <summary>
    /// Polygon, not necessarily axis-aligned
    /// </summary>
    public class Polygon : Body
    {
        public Polygon(Texture2D texture, Vector2 position, Vector2 size, Vector2[] points = null, float rotation = 0f)
            : base(texture, position, size, rotation)
        {
            if (points == null)     // Make a collision rectangle that matches the drawn size
            {
                Points = new Vector2[4];
                Vector2 half = Size / 2;
                Points[0] = RotatePoint(Position, new Vector2(Position.X - half.X, Position.Y - half.Y), Rotation);
                Points[1] = RotatePoint(Position, new Vector2(Position.X + half.X, Position.Y - half.Y), Rotation);
                Points[2] = RotatePoint(Position, new Vector2(Position.X + half.X, Position.Y + half.Y), Rotation);
                Points[3] = RotatePoint(Position, new Vector2(Position.X - half.X, Position.Y + half.Y), Rotation);
            }
            else
            {
                Points = points;
            }

            Edges = new Vector2[Points.Length];
            for (int x = 0; x < Edges.Length; x++)
            {
                Edges[x] = new Vector2(Points[(x + 1) % 4].X - Points[x].X, Points[(x + 1) % 4].Y - Points[x].Y);
            }
        }

        protected Vector2 RotatePoint(Vector2 pivot, Vector2 point, float angle)
        {
            double leX = ((Math.Cos(angle) * (point.X - pivot.X)) -
                               (Math.Sin(angle) * (point.Y - pivot.Y)) +
                               pivot.X);
            double leY = ((Math.Sin(angle) * (point.X - pivot.X)) +
                               (Math.Cos(angle) * (point.Y - pivot.Y)) +
                               pivot.Y);
            return new Vector2((float)leX, (float)leY);
        }

        public void Rotate(float angle)
        {
            Rotation += angle;
            Vector2 half = Size / 2;
            Points[0] = RotatePoint(Position, new Vector2(Position.X - half.X, Position.Y - half.Y), Rotation);
            Points[1] = RotatePoint(Position, new Vector2(Position.X + half.X, Position.Y - half.Y), Rotation);
            Points[2] = RotatePoint(Position, new Vector2(Position.X + half.X, Position.Y + half.Y), Rotation);
            Points[3] = RotatePoint(Position, new Vector2(Position.X - half.X, Position.Y + half.Y), Rotation);
            for (int x = 0; x < Edges.Length; x++)
            {
                Edges[x] = new Vector2(Points[(x + 1) % 4].X - Points[x].X, Points[(x + 1) % 4].Y - Points[x].Y);
            }
        }

        /// <summary>
        /// Intersection detection function
        /// Mostly derived from http://www.codeproject.com/Articles/15573/2D-Polygon-Collision-Detection
        /// </summary>
        /// <param name="other">The other Body to check intersection with</param>
        /// <returns>Vector2.Zero for no intersection, and minimum translation vector if there is an intersection</returns>
        public override Vector2 Intersects(Polygon other)
        {
            //Separating Axis Theorem
            int edgeCountA = this.Edges.Length;
            int edgeCountB = other.Edges.Length;
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

                    Vector2 d = this.Position - other.Position;
                    if (d.X * translationAxis.X + d.Y * translationAxis.Y < 0)
                        translationAxis = -translationAxis;
                }
            }
            //Console.WriteLine(minIntervalDistance);
            return translationAxis * minIntervalDistance;
        }

        private static void ProjectRectangle(Vector2 axis, Polygon body, ref float min, ref float max)
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

        /// <summary>
        /// Gets the parametric T value for the ray on start in direction dir, 0 if nothing
        /// Based on http://ncase.me/sight-and-light/
        /// </summary>
        /// <param name="rayStart"></param>
        /// <param name="rayDir"></param>
        /// <returns></returns>
        public override float Raycast(Vector2 rayStart, Vector2 rayDir)
        {
            float minT1 = float.MaxValue;
            for (int i=0; i<Points.Length; i++)
            {
                Vector2 segStart = Points[i];
                Vector2 segDir = Edges[i];
                float t2 = (rayDir.X * (segStart.Y - rayStart.Y) + rayDir.Y * (rayStart.X - segStart.X))
                    / (segDir.X * rayDir.Y - segDir.Y * rayDir.X);
                float t1 = (segStart.X + segDir.X * t2 - rayStart.X) / rayDir.X;
                if (t1 > 0 && t1 < minT1 && t2 > 0 && t2 < 1)
                    minT1 = t1;
            }
            return minT1;
        }
    }
}
