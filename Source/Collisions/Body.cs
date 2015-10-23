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
        public Vector2 Origin;

        //private List<Vector2> Points;
        //public List<List<Vector2>> Edges;

        protected Color color;

        private Texture2D texture;

        //public bool WillIntersect; //If it will intersect depending on velocity       -- Updated in Intersect method
        //public bool Intersect; //If it already intersects                             -- Updated in Intersect method
        //public Vector2 MinimumTranslationVector; //Vector needed to push them apart   -- Updated in Intersect method

        public float Left
        {
            get
            {
                return Center.X - Size.X / 2 + Position.X;
            }
        }
        public float Right
        {
            get
            {
                return Center.X + Size.X / 2 + Position.X;
            }
        }
        public float Top
        {
            get
            {
                return Center.Y - Size.Y / 2 + Position.Y;
            }
        }
        public float Bottom
        {
            get
            {
                return Center.Y + Size.Y / 2 + Position.Y;
            }
        }

        public Body(Texture2D texture, Vector2 position, Vector2 size, float rotation = 0f)
        {
            this.texture = texture;
            this.Position = position;
            this.Size = size;
            this.Rotation = rotation;

            CollisionExceptions = new List<Body>();
            Velocity = Vector2.Zero;
            color = Color.White;
            Origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);

            //Vector2 dposition = ConvertUnits.ToDisplayUnits(Position);
            //Center = new Vector2(dposition.X + size.X / 2, dposition.Y + size.Y / 2);
            //Points = new List<Vector2>();
            //Points.Add(RotatePoint(dposition, new Vector2(dposition.X - size.X / 2, dposition.Y - size.Y / 2), rotation * (float)Math.PI / 180));
            //Points.Add(RotatePoint(dposition, new Vector2(dposition.X + size.X / 2, dposition.Y - size.Y / 2), rotation * (float)Math.PI / 180));
            //Points.Add(RotatePoint(dposition, new Vector2(dposition.X + size.X / 2, dposition.Y + size.Y / 2), rotation * (float)Math.PI / 180));
            //Points.Add(RotatePoint(dposition, new Vector2(dposition.X - size.X / 2, dposition.Y + size.Y / 2), rotation * (float)Math.PI / 180));

            /*Points.Add(new Vector2(dposition.X - size.X / 2, dposition.Y - size.Y / 2));
            Points.Add(new Vector2(dposition.X + size.X / 2, dposition.Y - size.Y / 2));
            Points.Add(new Vector2(dposition.X + size.X / 2, dposition.Y + size.Y / 2));
            Points.Add(new Vector2(dposition.X - size.X / 2, dposition.Y + size.Y / 2));*/
            //Edges = new List<List<Vector2>>();
            //for (int x = 0; x < 4; x++)
            //{
            //    List<Vector2> line = new List<Vector2>();
            //    line.Add(new Vector2(Points[x].X, Points[x].Y));
            //    line.Add(new Vector2(Points[(x + 1) % 4].X, Points[(x + 1) % 4].Y));
            //    Edges.Add(line);
            //}

        }

        public bool TestPoint(Vector2 point)
        {
            return Left <= point.X && Right >= point.X && Top <= point.Y && Bottom >= point.Y;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(Position), null, color, Rotation, Origin, ConvertUnits.ToDisplayUnits(Size), SpriteEffects.None, 0f);
        }


        //private Vector2 RotatePoint(Vector2 pivot, Vector2 point, float CWAngle)
        //{
        //    // Rotate counterclockwise, angle in radians
        //    float angle = 2 * (float)Math.PI - CWAngle;
        //    double leX = ((Math.Cos(angle) * (point.X - pivot.X)) -
        //                       (Math.Sin(angle) * (point.Y - pivot.Y)) +
        //                       pivot.X);
        //    double leY = ((Math.Sin(angle) * (point.X - pivot.X)) +
        //                       (Math.Cos(angle) * (point.Y - pivot.Y)) +
        //                       pivot.Y);
        //    return new Vector2((float)leX, (float)leY);
        //}




        ///// <summary>
        ///// Intersection detection function
        ///// </summary>
        ///// <param name="other">The other Body to check intersection with</param>
        ///// <returns>True if intersects and false if not</returns>
        //public int Intersects(Body other)
        //{   // 0: No intersection
        //    // 1: Right side intersection
        //    // 2: Left side intersection
        //    // 3: Top intersection
        //    // 4: Bottom intersection
        //    // 5: Everything else
        //    bool left = between(Position.X - Size.X / 2, other.Position.X - other.Size.X / 2, other.Position.X + other.Size.X / 2);
        //    bool right = between(Position.X + Size.X / 2, other.Position.X - other.Size.X / 2, other.Position.X + other.Size.X / 2);
        //    bool top = between(Position.Y - Size.Y / 2, other.Position.Y - other.Size.Y / 2, other.Position.Y + other.Size.Y / 2);
        //    bool bottom = between(Position.Y + Size.Y / 2, other.Position.Y - other.Size.Y / 2, other.Position.Y + other.Size.Y / 2);
        //    int code = 0;
        //    if (left && right && bottom && !top)
        //        code = 4;
        //    else if (left && right && top && !bottom)
        //        code = 3;
        //    else if (!left && right && !top && !bottom && between(other.Position.Y - other.Size.Y / 2, Position.Y - Size.Y / 2, Position.Y + Size.Y / 2))
        //        code = 1;
        //    else if (left && !right && !top && !bottom && between(other.Position.Y - other.Size.Y / 2, Position.Y - Size.Y / 2, Position.Y + Size.Y / 2))
        //        code = 2;
        //    else if ((left || right) && (top || bottom))
        //        code = 5;
        //    else
        //        code = 0;

        //    return code;
        //}

        
        
        //private bool between(float a, float b, float c)
        //{
        //    return a >= b && a <= c;
        //}
            



            /*Separating Axis Theorem
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


                return Intersect;
            
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
        }*/
    }
}
