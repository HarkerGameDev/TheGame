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
        private const float GRAVITY = 10f;

        private Player player;

        public World(Player player)
        {
            this.player = player;
        }

        public void Add(Body body)
        {

        }

        public void Step(float deltaTime)
        {

        }

        public Body TestPoint(Vector2 point)
        {
            return null;
        }
    }
}
