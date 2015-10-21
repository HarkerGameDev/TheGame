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
        public List<Body> CollisionExceptions; //Exceptions to collisions(ie. player player collision exception)
        public Vector2 Center;
        public Vector2 Velocity;

        private List<Vector2> Points;
        private List<Vector2> Edges;

        protected Color color;

        private Texture2D texture;
        private Vector2 origin;

        public bool WillIntersect; //If it will intersect depending on velocity       -- Updated in Intersect method
        public bool Intersect; //If it already intersects                             -- Updated in Intersect method
        public Vector2 MinimumTranslationVector; //Vector needed to push them apart   -- Updated in Intersect method

        public Body(Texture2D texture, Vector2 position, Vector2 size, float rotation = 0f)
        {
            this.texture = texture;
            this.Position = position;
            this.Size = size;
            this.Rotation = rotation;
            this.CollisionExceptions = new List<Body>();
            color = Color.White;
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            
            Center = new Vector2(position.X + size.X / 2, position.Y + size.Y / 2);
            Points = new List<Vector2>();
            Points.Add(RotatePoint(Center, new Vector2(position.X, position.Y), rotation * (float)Math.PI / 180));
            Points.Add(RotatePoint(Center, new Vector2(position.X + size.X, position.Y), rotation * (float)Math.PI / 180));
            Points.Add(RotatePoint(Center, new Vector2(position.X + size.X, position.Y + size.Y), rotation * (float)Math.PI / 180));
            Points.Add(RotatePoint(Center, new Vector2(position.X, position.Y + size.Y), rotation * (float)Math.PI / 180));
            Edges = new List<Vector2>();
            for(int x = 0; x < 4; x++)
            {
                Edges.Add(new Vector2(Points[x].X-Points[(x+1)%4].X, Points[x].Y - Points[(x + 1) % 4].Y));
            }

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(Position), null, color, Rotation, origin, ConvertUnits.ToDisplayUnits(Size), SpriteEffects.None, 0f);
        }


        private Vector2 RotatePoint(Vector2 pivot, Vector2 point, float CWAngle)
        {
            // Rotate counterclockwise, angle in radians
            float angle = 2 * (float)Math.PI - CWAngle;
            double leX = ((Math.Cos(angle) * (point.X - pivot.X)) -
                               (Math.Sin(angle) * (point.Y - pivot.Y)) +
                               pivot.X);
            double leY = ((Math.Sin(angle) * (point.X - pivot.X)) +
                               (Math.Cos(angle) * (point.Y - pivot.Y)) +
                               pivot.Y);
            return new Vector2((float)leX, (float)leY);
        }




        /// <summary>
        /// Intersection detection function, stores results in WillIntersect and Intersect
        /// Mostly derived from http://www.codeproject.com/Articles/15573/2D-Polygon-Collision-Detection
        /// </summary>
        /// <param name="other">The other Body to check intersection with</param>
        /// <returns>True if intersects and false if not</returns>
        public bool Intersects(Body other)
        {
            //Separating Axis Theorem
                Intersect = true;
                WillIntersect = true;
                int edgeCountA = 4;
                int edgeCountB = 4;
                float minIntervalDistance = float.PositiveInfinity;
                Vector2 translationAxis = new Vector2();
                Vector2 edge;
                Vector2 velocity = Velocity - other.Velocity;


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
                    float minA = 0; float minB = 0; float maxA = 0; float maxB = 0;
                    ProjectRectangle(axis, this, ref minA, ref maxA);
                    ProjectRectangle(axis, other, ref minB, ref maxB);

                    // Check if the polygon projections are currentlty intersecting
                    if (IntervalDistance(minA, maxA, minB, maxB) > 0)
                    {
                        Intersect = false;
                    }

                    // ===== 2. Now find if the polygons *will* intersect =====

                    // Project the velocity on the current axis
                    float velocityProjection = axis.X * velocity.X + axis.Y * velocity.Y;

                    // Get the projection of polygon A during the movement
                    if (velocityProjection < 0)
                    {
                        minA += velocityProjection;
                    }
                    else
                    {
                        maxA += velocityProjection;
                    }

                    // Do the same test as above for the new projection
                    float intervalDistance = IntervalDistance(minA, maxA, minB, maxB);
                    if (intervalDistance > 0) WillIntersect = false;

                    // If the polygons are not intersecting and won't intersect, exit the loop
                    if (!Intersect && !WillIntersect) break;

                    // Check if the current interval distance is the minimum one. If so store
                    // the interval distance and the current distance.
                    // This will be used to calculate the minimum translation Vector2
                    intervalDistance = Math.Abs(intervalDistance);
                    if (intervalDistance < minIntervalDistance)
                    {
                        minIntervalDistance = intervalDistance;
                        translationAxis = axis;

                        Vector2 d = this.Center - other.Center;
                        if (d.X * translationAxis.X + d.Y * translationAxis.Y < 0)
                            translationAxis = -translationAxis;
                    }
                }

                // The minimum translation vector
                // can be used to push the polygons appart.
                if (WillIntersect)
                {
                    MinimumTranslationVector = translationAxis * minIntervalDistance;
                }


                if (Intersect)
                        return true;
                    else
                        return false;
            
        }

        private void ProjectRectangle(Vector2 axis, Body body, ref float min, ref float max)
        {
            // Project a point on an axis using the dot product. Truthfully, I never really paid attention in precalc
            float dotProduct = axis.X * body.Points[0].X + axis.Y * body.Points[0].Y;
            min = dotProduct;
            max = dotProduct;
            for (int i = 0; i < body.Points.Count; i++)
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

        private float IntervalDistance(float minA, float maxA, float minB, float maxB)
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
