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
        private const float GRAVITY = 8f;

        private Player player;
        private List<Body> bodies; // try to optimize this somehow (something faster than a list maybe)

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
            player.Velocity.Y += GRAVITY * deltaTime;
            player.Position += player.Velocity * deltaTime;
            foreach (Body body in bodies)
            {
                if (player.Intersects(body))
                {
                    player.Position += player.MinimumTranslationVector;
                }
            }
        }

        public Body TestPoint(Vector2 point)
        {
            return null;
        }
    }
}
