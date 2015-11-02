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
        private const float MAX_SLOPE = MathHelper.PiOver4;
        private static float SLOPE_JUMP = (float)Math.Atan2(Source.GameData.JUMP_IMPULSE, Source.GameData.MAX_VELOCITY);
        public const float BOTTOM = -0.8f;        // bottom of the level

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
                    for (int i = player.Projectiles.Count - 1; i >= 0; i--)
                    {
                        Projectile proj = player.Projectiles[i];
                        proj.Move(deltaTime);
                        if (proj.LiveTime > Projectile.MAX_LIVE)
                            player.Projectiles.RemoveAt(i);
                        else
                        {
                            foreach (Player target in players)
                            {
                                if (proj.Intersects(target) != Vector2.Zero)
                                {
                                    target.Velocity.X = 0;
                                    player.Projectiles.RemoveAt(i);
                                    proj = null;
                                    break;
                                }
                            }

                            if (proj != null)
                            {
                                foreach (Floor floor in floors)
                                {
                                    if (proj.Intersects(floor) != Vector2.Zero)
                                        player.Projectiles.RemoveAt(i);
                                }
                            }
                        }
                    }

                    int totalCollisions = 0;

                    player.Velocity.Y += GRAVITY * deltaTime;
                    player.CanJump = false;

                    player.Move(deltaTime);

                    if (player.Position.Y > BOTTOM)  // bottom of the level
                    {
                        player.Velocity.Y = 0;
                        player.MoveToPosition(new Vector2(player.Position.X, BOTTOM));
                        player.CanJump = true;
                    }

                    //Console.WriteLine(player.Velocity);
                    foreach (Floor floor in floors)
                    {
                        Vector2 translation = player.Intersects(floor);
                        if (translation != Vector2.Zero)
                        {
                            totalCollisions++;
                            //Console.WriteLine("Rotation: " + floor.Rotation);

                            if (!player.Ghost || floor.Solid)
                            {
                                if (translation.X != 0 && (Math.Abs(floor.Rotation) >= MAX_SLOPE || floor.Rotation == 0))
                                    player.Velocity.X = 0;

                                if (Math.Abs(floor.Rotation) >= SLOPE_JUMP && player.Velocity.Y < 0)        // dealing with speed bost when holding jump on a slope
                                {
                                    player.MovePosition(new Vector2(0f, -player.Velocity.Y * deltaTime));
                                    translation = player.Intersects(floor);                                 // recalculate minimum translation vector
                                    //Console.WriteLine("Moving back");
                                }

                                totalCollisions++;

                                if (translation.Y != 0)
                                {
                                    player.Velocity.Y = 0;
                                    if (translation.Y > 0)
                                        player.CanJump = true;
                                    else
                                        player.CanJump = false;
                                }
                                player.MovePosition(-translation);

                                //Writing all this to console lags the game
                                //Console.WriteLine("Colliding with: " + floor.Position + "   Pushing to:   " + newPosition + "   Vector:    "+ new Vector2(-1 * translation.X, -1 * translation.Y));
                            }
                        }
                    }

                    if (totalCollisions == 0)
                        player.Ghost = false;
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
