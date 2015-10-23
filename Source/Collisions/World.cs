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

            Console.WriteLine(player.Velocity);

            player.Position.X += player.Velocity.X * deltaTime;
            player.Position.Y += player.Velocity.Y * deltaTime;
            foreach (Body body in bodies)
            {
                if(!player.Ghost)
                    player.Intersects(body, deltaTime);
                if (player.Ghost && player.Position.Y > player.oldY + player.Size.Y + 0.2f)
                    player.Ghost = false;

            }

        }

        public Body TestPoint(Vector2 point)
        {
            return null;
        }
    }
}
