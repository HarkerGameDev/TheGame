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

        public void Step(float deltaTime)
        {
            player.CanJump = false;
            player.Velocity.Y += GRAVITY * deltaTime;
            player.CanJump = false;

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
                if (player.Intersects(body))
                {
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
