using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Source.Collisions
{
    /// <summary>
    /// Holder for collisions. This will have most of the collision checking code
    /// Game1.cs should only be used to combine all the things together
    /// </summary>
    public class World
    {
        private const float GRAVITY = 26f;
        private const float MAX_CLIMB_SLOPE = MathHelper.PiOver4;

        private Player player;
        private List<Floor> bodies;

        public World(Player player)
        {
            this.player = player;
            bodies = new List<Floor>();
        }

        public void Add(Floor body)
        {
            bodies.Add(body);
        }

        public void Remove(Floor body)
        {
            bodies.Remove(body);
        }

        public void Clear()
        {
            bodies.Clear();
        }

        public void Step(float deltaTime)
        {
            player.Velocity.Y += GRAVITY * deltaTime;
            player.CanJump = false;
            int totalCollisions = 0;

            player.Position.X += player.Velocity.X * deltaTime;
            player.setRotation(0);
            foreach (Floor body in bodies)
            {
                if (player.Intersects(body))
                {
                    totalCollisions++;

                    if (player.Velocity.Y < 0 && body.JumpUp)
                        player.Ghost = true;

                    if (!player.Ghost || body.Size.Y > body.Size.X && body.Rotation == 0f)
                    {
                        float speed = player.Velocity.X * deltaTime;
                        if (body.Rotation == 0)
                            player.Velocity.X = 0;
                        else
                        {
                            if (player.CollideBottom >= 2)
                            {
                                player.setRotation(body);
                                player.Position.X += speed * (float)Math.Cos(body.Rotation);
                                player.Position.Y -= Math.Abs(speed) * (float)Math.Sin(body.Rotation);
                                player.CanJump = true;
                            }
                            else
                            {
                                player.Velocity.X = 0;
                            }
                        }
                        player.Position.X -= speed;
                    }
                }
            }

            player.Position.Y += player.Velocity.Y * deltaTime;
            foreach (Floor body in bodies)
            {
                if (player.Intersects(body))
                {
                    totalCollisions++;

                    if (player.Velocity.Y < 0 && body.JumpUp)
                        player.Ghost = true;

                    if (!player.Ghost || body.Size.Y > body.Size.X && body.Rotation == 0f)
                    {
                        player.Position.Y -= player.Velocity.Y * deltaTime;
                        player.Velocity.Y = 0;
                    }
                }
            }

            if (totalCollisions == 0)
                player.Ghost = false;
        }

        public Body TestPoint(Vector2 point)
        {
            foreach (Body body in bodies)
            {
                if (body.TestPoint(point))
                {
                    Console.WriteLine("Rotation = " + body.Rotation);
                    return body;
                }
            }
            return null;
        }
    }
}
