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
        public List<Body> CollisionExceptions; //Exceptions to collisions(ie. player player collision exception)
        public Vector2 Velocity;

        public float Rotation { get { return rotation; } }
        public Vector2 Size { get { return size; } }
        public Vector2 Origin { get { return origin; } }

        private Vector2 topLeft, topRight, botLeft, botRight;

        protected float rotation;
        private Vector2 size;
        private Vector2 origin;

        protected Color color;

        private Texture2D texture;

        public float Left { get { return Position.X - size.X / 2; } }
        public float Right { get { return Position.X + size.X / 2; } }
        public float Top { get { return Position.Y - size.Y / 2; } }
        public float Bottom { get { return Position.Y + size.Y / 2; } }

        public Body(Texture2D texture, Vector2 position, Vector2 size, float rotation = 0f)
        {
            this.texture = texture;
            this.Position = position;
            this.size = size;
            this.rotation = MathHelper.WrapAngle(rotation);

            CollisionExceptions = new List<Body>();
            Velocity = Vector2.Zero;
            color = Color.White;
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);


            Console.WriteLine("Rotation1: " + rotation);
            if (Math.Abs(rotation) == MathHelper.Pi)            // floor is flat
            {
                rotation = 0f;
            }
            else if (Math.Abs(rotation) == MathHelper.PiOver2)  // floor is vertical
            {
                rotation = 0f;
                size = new Vector2(size.Y, size.X);
            }
            else if (rotation < -MathHelper.PiOver2)
            {
                rotation += MathHelper.Pi;
            }
            else if (rotation < 0)
            {
                size = new Vector2(size.Y, size.X);
                rotation += MathHelper.PiOver2;
            }
            else if (rotation > MathHelper.PiOver2)
            {
                size = new Vector2(size.Y, size.X);
                rotation -= MathHelper.PiOver2;
            }
            Console.WriteLine("Rotation2: " + rotation);


            float cos = (float)Math.Cos(rotation);
            float sin = (float)Math.Sin(rotation);
            Vector2 distX = new Vector2(size.X * cos, size.X * sin) / 2;
            Vector2 distY = new Vector2(size.Y * sin, -size.Y * cos) / 2;

            topLeft = -distX + distY;
            topRight = distX + distY;
            botLeft = -distX - distY;
            botRight = distX - distY;
        }

        public bool TestPoint(Vector2 point)
        {
            if (rotation == 0f)
                return Left <= point.X && Right >= point.X && Top <= point.Y && Bottom >= point.Y;
            else
            {
                Vector2 tL = Position + topLeft;
                Vector2 tR = Position + topRight;
                Vector2 bL = Position + botLeft;
                Vector2 bR = Position + botRight;

                Vector2 pos;
                if (Between(point.X, bL.X, tL.X))         // check left half of top
                    pos = LineFunction(point.X, bL, tL);
                else if (Between(point.X, tL.X, tR.X))   // check right half of top
                    pos = LineFunction(point.X, tL, tR);
                else
                    return false;

                if (pos.Y <= point.Y)
                {
                    if (Between(point.X, bL.X, bR.X))    // check left half of bottom
                    {
                        //Console.WriteLine("From: " + botLeft + " | To: " + botRight);
                        Vector2 pos2 = LineFunction(point.X, bL, bR);
                        //Console.WriteLine("Point: " + point + " | Pos: " + pos + " | Pos2: " + pos2);
                        return pos2.Y >= point.Y;
                    }
                    else                                // check right half of bottom
                    {
                        Vector2 pos2 = LineFunction(point.X, bR, tR);
                        return pos2.Y >= point.Y;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Checks if given body is inside this body
        /// </summary>
        /// <param name="player">Body must have 0 rotation to work properly</param>
        /// <returns></returns>
        public bool Contains(Player player)
        {
            Vector2 tL = Position + topLeft;
            Vector2 tR = Position + topRight;
            Vector2 bL = Position + botLeft;
            Vector2 bR = Position + botRight;

            bool bottom = player.TestPoint(bL) || player.TestPoint(bR);
            if (bottom)
                player.CollideBottom = 2;

            if (player.TestPoint(tL) || player.TestPoint(tR) || bottom)
                return true;

            return Check(player, player.Left) | Check(player, player.Right);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player">Body must have 0 rotation to work properly</param>
        /// <returns></returns>
        private bool Check(Player player, float x)
        {
            Vector2 tL = Position + topLeft;
            Vector2 tR = Position + topRight;
            Vector2 bL = Position + botLeft;
            Vector2 bR = Position + botRight;

            Vector2 pos;
            if (Between(x, bL.X, tL.X))         // check left half of top
                pos = LineFunction(x, bL, tL);
            else if (Between(x, tL.X, tR.X))   // check right half of top
                pos = LineFunction(x, tL, tR);
            else
                return false;

            if (pos.Y > player.Top)
            {
                player.CollideBottom++;
                //Console.WriteLine("Top");
            }

            if (pos.Y <= player.Bottom)
            {
                if (Between(x, bL.X, bR.X))    // check left half of bottom
                {
                    Vector2 pos2 = LineFunction(x, bL, bR);
                    return pos2.Y >= player.Top;
                }
                else                            // check right half of bottom
                {
                    Vector2 pos2 = LineFunction(x, bR, tR);
                    return pos2.Y >= player.Top;
                }
            }

            return false;
        }

        private bool Between(float val, float min, float max)
        {
            return val >= min && val <= max;
        }

        private Vector2 LineFunction(float x, Vector2 start, Vector2 end)
        {
            Vector2 dist = end - start;
            return new Vector2(x, dist.Y / dist.X * (x - start.X) + start.Y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(Position), null, color, rotation, origin, ConvertUnits.ToDisplayUnits(size), SpriteEffects.None, 0f);

            // testing corners.
            Vector2 tL = Position + topLeft;
            Vector2 tR = Position + topRight;
            Vector2 bL = Position + botLeft;
            Vector2 bR = Position + botRight;

            Vector2 testingSize = new Vector2(10, 10);
            spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(tL), null, Color.Purple, 0f, origin, testingSize, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(tR), null, Color.Orange, 0f, origin, testingSize, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(bL), null, Color.Yellow, 0f, origin, testingSize, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(bR), null, Color.Black, 0f, origin, testingSize, SpriteEffects.None, 0f);

            Vector2 dist = tL - bL;
            float x = bL.X + 1f;
            Vector2 pos = new Vector2(x, (dist.Y / dist.X) * (x - bL.X) + bL.Y);
            spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(pos), null, Color.Pink, 0f, origin, testingSize, SpriteEffects.None, 0f);
        }


        //private Vector2 RotatePoint(Vector2 pivot, Vector2 point, float angle)
        //{
        //    // Rotate counterclockwise, angle in radians
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
