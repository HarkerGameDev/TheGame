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
        private List<Floor> floors;

        public World(Player player, List<Floor> floors)
        {
            this.player = player;
            this.floors = floors;
        }

        public void Step(float deltaTime)
        {
            player.Velocity.Y += GRAVITY * deltaTime;
            player.CanJump = false;

            player.Move(deltaTime);

            foreach (Floor floor in floors)
            {
                if (player.Intersects(floor) != Vector2.Zero)
                {
                    player.Velocity = Vector2.Zero;
                    Console.WriteLine("Colliding with: " + floor.Position);
                }
            }
        }

        public Body TestPoint(Vector2 point)
        {
            foreach (Body body in floors)
            {
                if (body.TestPoint(point))
                {
                    return body;
                }
            }
            return null;
        }
    }
}
