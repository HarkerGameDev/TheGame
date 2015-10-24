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

        private Player player;
        private List<Body> bodies;

        public World(Player player)
        {
            this.player = player;
            bodies = new List<Body>();
        }

        public void Add(Body body)
        {
            bodies.Add(body);
        }

        public void Remove(Body body)
        {
            bodies.Remove(body);
        }

        public void Clear()
        {
            bodies.Clear();
        }

        public void Step(float deltaTime)
        {
            player.CanJump = false;
            player.Velocity.Y += GRAVITY * deltaTime;
            player.CanJump = false;

            if (player.surfaceRotated != null && Math.Sign(2 * (float)Math.PI - player.surfaceRotated.Rotation) == Math.Sign(player.Velocity.X))
            {
                player.Velocity.X = 30f;
                player.Position.X += player.Velocity.X * (float)Math.Cos(player.surfaceRotated.Rotation) * deltaTime;
                player.Position.Y -= player.Velocity.X * (float)Math.Sin(player.surfaceRotated.Rotation) * deltaTime;
                player.Velocity.Y = 0;
                player.Intersects(player.surfaceRotated);
                Step(deltaTime);
                return;
            }

            
            player.Position.X += player.Velocity.X * deltaTime;

            foreach (Body body in bodies)
            {
                if (player.Intersects(body))
                {
                    player.Position.X -= player.Velocity.X * deltaTime;
                    player.Velocity.X = 0;
                }
            }

            
            player.Position.Y += player.Velocity.Y * deltaTime;
            foreach (Body body in bodies)
            {
                if (player.Intersects(body) || (player.Left < body.Right && player.Right > body.Left && player.Bottom > body.Top && player.Bottom-player.Velocity.Y*deltaTime < body.Top))
                {
                    //Console.WriteLine("Intersection with: " + body.Position);
                    player.Position.Y -= player.Velocity.Y * deltaTime;
                    player.Velocity.Y = 0;
                }
            }
        }

        public Body TestPoint(Vector2 point)
        {
            foreach (Body body in bodies)
            {
                if (body.TestPoint(point))
                {
                    Console.WriteLine("Found body at " + body.Position);
                    return body;
                }
            }
            return null;
        }
    }
}
