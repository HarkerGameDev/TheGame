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

        private List<Player> players;
        private List<Floor> floors;

        public World(List<Player> player, List<Floor> floors)
        {
            this.players = player;
            this.floors = floors;
        }

        public void Step(float deltaTime)
        {
            foreach (Player player in players)
            {
                if (player.TimeSinceDeath <= 0)
                {
                    player.Velocity.Y += GRAVITY * deltaTime;
                    player.CanJump = false;

                    player.Move(deltaTime);

                    //Console.WriteLine(player.Velocity);
                    if (!player.Ghost)
                    {
                        foreach (Floor floor in floors)
                        {
                            Vector2 translation = player.Intersects(floor);
                            if (translation != Vector2.Zero)
                            {
                                float newX;
                                float newY;
                                if (translation.X != 0 && floor.Rotation == 0)
                                    player.Velocity.X = 0;


                                if (translation.Y != 0)
                                {
                                    player.Velocity.Y = 0;
                                    if (translation.Y > 0)
                                        player.CanJump = true;
                                    else
                                        player.CanJump = false;
                                }
                                Vector2 newPosition = new Vector2(-1 * translation.X, -1 * translation.Y);
                                player.MovePosition(newPosition);

                                //Writing all this to console lags the game
                                //Console.WriteLine("Colliding with: " + floor.Position + "   Pushing to:   " + newPosition + "   Vector:    "+ new Vector2(-1 * translation.X, -1 * translation.Y));
                            }
                        }
                    }
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
