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

            //Console.WriteLine(player.Velocity);
            foreach (Floor floor in floors)
            {
                Vector2 translation = player.Intersects(floor);
                if (translation != Vector2.Zero)
                {
                    float newX;
                    float newY;
                    if (translation.X == 0)
                        newX = player.Velocity.X;
                    else
                    {
                        newX = -1 * translation.X;
                        player.Velocity.X = 0;
                    }

                    if (translation.Y == 0)
                        newY = player.Velocity.Y;
                    else
                    {
                        newY = -1 * translation.Y;
                        player.Velocity.Y = 0;
                        if (newY < 0)
                            player.CanJump = true;
                    }
                    Vector2 newPosition = new Vector2(-1*translation.X, -1*translation.Y);
                    player.MovePosition(newPosition);

                    //Console.WriteLine("Colliding with: " + floor.Position + "   Pushing to:   " + newPosition + "   Vector:    "+ new Vector2(-1 * translation.X, -1 * translation.Y));
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
