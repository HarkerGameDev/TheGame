using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Source.Graphics;

namespace Source.Collisions
{
    /// <summary>
    /// Holder for collisions. This will have most of the collision checking code
    /// Game1.cs should only be used to combine all the things together
    /// </summary>
    public class World
    {
        //private const float MAX_SLOPE = MathHelper.PiOver4;
        private static float SLOPE_JUMP = (float)Math.Atan2(Source.GameData.JUMP_SPEED, Source.GameData.RUN_VELOCITY);
        public const float BOTTOM = -0.8f;        // bottom of the level

        private Game1 game;

        public World(Game1 game)
        {
            this.game = game;
        }

        public void Step(float deltaTime)
        {
            for (int i = game.particles.Count - 1; i >= 0; i--)
            {
                Particle part = game.particles[i];
                part.LiveTime -= deltaTime;
                if (part.LiveTime < 0)
                    game.particles.RemoveAt(i);
                else if(part.type.Equals(Particle.Type.Texture))
                {
                    if (TestPoint(part.Position) == null)
                    {
                        part.Velocity.Y += GameData.GRAVITY_PART * deltaTime;
                        part.Angle += part.AngularVelocity * deltaTime;
                        part.Position += part.Velocity * deltaTime;
                        //part.Color = Color.Green;
                    }
                    //else
                        //TestPoint(part.Position).Color = Color.Purple;
                }
            }

            foreach (Player player in game.players)
            {
                if (player.TimeSinceDeath <= 0)
                {
                    for (int i = player.Projectiles.Count - 1; i >= 0; i--)
                    {
                        CalculateProjectile(player, deltaTime, i);
                    }

                    player.Velocity.Y += GameData.GRAVITY * deltaTime;
                    //player.CanJump = false;

                    //int steps = game.IsFixedTimeStep ? GameData.PLAYER_STEP : 1;
                    int steps = 2;
                    for (int i = 0; i < steps; i++)
                    {
                        player.Move(deltaTime / steps);
                        CheckWalls(player);
                    }
                    CheckFloors(player);
                    CheckObstacles(player);

                    if (player.AbilityActive)
                        PerformSpecial(player, deltaTime);
                }
            }
        }

        private void CalculateProjectile(Player player, float deltaTime, int projIndex)
        {
            Projectile proj = player.Projectiles[projIndex];
            proj.Move(deltaTime);
            if (proj.LiveTime > GameData.PROJ_LIVE)
            {
                player.Projectiles.RemoveAt(projIndex);
                return;
            }
            //foreach (Player target in game.players)
            //{
            //    if (proj.Intersects(target) != Vector2.Zero)
            //    {
            //        target.StunTime = GameData.STUN_LENGTH;
                    
            //        player.Projectiles.RemoveAt(projIndex);
            //        return;
            //    }
            //}
            for (int i = game.walls.Count - 1; i >= 0; i--)
            {
                Wall wall = game.walls[i];
                if (proj.Intersects(wall) != Vector2.Zero)
                {
                    if (--wall.Health <= 0)
                    {
                        wall.Color = Color.Azure;
                        game.walls.RemoveAt(i);
                        MakeParticles(proj.Position, wall, GameData.NUM_PART_WALL, 0, 0);
                        game.particles.Add(new Particle(wall.Position, game.font, "BAM!"));
                    }
                    else
                        wall.SetColor();

                    player.Projectiles.RemoveAt(projIndex);
                    return;
                }
            }
            foreach (Floor floor in game.floors)
            {
                if (proj.Intersects(floor) != Vector2.Zero)
                {
                    player.Projectiles.RemoveAt(projIndex);
                    if (--floor.Health == 0)
                    {
                        game.floors.Remove(floor);
                        MakeParticles(proj.Position, floor, GameData.NUM_PART_WALL, 1, 0);
                    }
                    return;
                }
            }
            foreach (Obstacle obstacle in game.obstacles)
            {
                if (proj.Intersects(obstacle) != Vector2.Zero)
                {
                    player.Projectiles.RemoveAt(projIndex);
                    return;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="source"></param>
        /// <param name="amount"></param>
        /// <param name="x">0 is left and right, 1 is right, -1 is left</param>
        /// <param name="y">0 is up and down, 1 is down, -1 is up</param>
        private void MakeParticles(Vector2 pos, Body source, int amount, int x, int y)
        {
            for (int i = 0; i < amount; i++)
                game.particles.Add(new Particle(pos, new Vector2(GameData.PARTICLE_WIDTH),
                    source.texture, 0f, rand(x, y, new Vector2(GameData.PARTICLE_X, GameData.PARTICLE_Y)), 0f, GameData.PARTICLE_LIFETIME, source.Color));
        }

        private void CheckFloors(Player player)
        {
            int totalCollisions = 0;
            //Console.WriteLine(player.Velocity);
            foreach (Floor floor in game.floors)
            {
                Vector2 translation = player.Intersects(floor);
                if (translation != Vector2.Zero)
                {
                    if (player.CurrentState != Player.State.Slamming)
                    {
                        totalCollisions++;

                        //if (translation.X != 0 && (/*Math.Abs(floor.Rotation) >= MAX_SLOPE || */floor.Rotation == 0))
                        //    player.Velocity.X = 0;

                        if (Math.Abs(translation.X) > Math.Abs(translation.Y) && !player.WallAbove)
                        {
                            player.CurrentState = Player.State.Climbing;
                            player.Velocity.Y = -GameData.CLIMB_SPEED;
                        }
                        else if (player.CurrentState == Player.State.Climbing)
                            player.CurrentState = Player.State.Walking;

                        if (translation.Y != 0)
                        {
                            player.Velocity.Y = 0;
                            if (translation.Y > 0 && player.InAir)
                                player.CurrentState = Player.State.Walking;
                        }
                        player.MovePosition(-translation);

                        //game.particles.Add(new Particle(player.Position + new Vector2(0f, player.Size.Y / 2 - GameData.PARTICLE_WIDTH * 2),
                        //    new Vector2(GameData.PARTICLE_WIDTH, GameData.PARTICLE_WIDTH), floor.texture, 0f,
                        //    rand(0, -1, new Vector2(GameData.PARTICLE_X, GameData.PARTICLE_Y)),
                        //    (float)game.rand.NextDouble() * GameData.PARTICLE_MAX_SPIN, GameData.PARTICLE_LIFETIME, Color.Azure));
                    }
                    else        // player is Slamming
                    {
                        if (floor.Health > 0)
                        {
                            MakeParticles(player.Position, floor, GameData.NUM_PART_WALL, 0, 1);
                        }
                        else
                        {
                            MakeParticles(player.Position, floor, GameData.NUM_PART_FLOOR, 0, 1);

                            float newFloorX = floor.Position.X + player.Position.X;
                            float sizeDiff = floor.Size.X / 2 + GameData.FLOOR_HOLE / 2;
                            float halfWidth = floor.Size.X / 2 - GameData.FLOOR_HOLE / 2;
                            float playerDist = player.Position.X - floor.Position.X;

                            if (halfWidth + playerDist > GameData.MIN_FLOOR_WIDTH)
                                game.floors.Add(new Floor(floor.texture, new Vector2((newFloorX - sizeDiff) / 2, floor.Position.Y), halfWidth + playerDist));
                            if (halfWidth - playerDist > GameData.MIN_FLOOR_WIDTH)
                                game.floors.Add(new Floor(floor.texture, new Vector2((newFloorX + sizeDiff) / 2, floor.Position.Y), halfWidth - playerDist));
                        }

                        game.floors.Remove(floor);
                        break;
                    }
                }
            }

			if (player.Position.Y > BOTTOM) {  // bottom of the level
				player.Velocity.Y = 0;

				// Kill the player
                player.Kill(game.rand);
				//player.MovePosition(new Vector2(0f, -10f));
				if (player.Score == 0) {
					for (int i = 0; i < game.players.Count; i++) {
						game.players [i].Score++;
					}
				}
				player.Score--;
                player.Velocity = Vector2.Zero;
                //player.MoveToPosition(new Vector2(player.Position.X, BOTTOM));
                //if (player.InAir)
                //{
                //    player.CurrentState = Player.State.Walking;
                //    //Console.WriteLine("Start walking");
                //}
                //totalCollisions++;
            }

            if (totalCollisions == 0 && !player.InAir)
            {
                player.CurrentState = Player.State.Jumping;
                //Console.WriteLine("Touching nothing");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">0 is left and right, 1 is right, -1 is left</param>
        /// <param name="y">0 is up and down, 1 is down, -1 is up</param>
        /// <param name="amplifier"></param>
        /// <returns></returns>
        private Vector2 rand(int x, int y, Vector2 amplifier)
        {
            float randX = 0;
            float randY = 0;

            if (x == 0) // Left & Right
                randX = ((float)game.rand.NextDouble() * 2 - 1) * amplifier.X;
            else if (x == 1) // Right
                randX = (float)game.rand.NextDouble() * amplifier.X;
            else if (x == -1) // Left
                randX = (float)game.rand.NextDouble() * -1 * amplifier.X;

            if (y == 0) // Up & Down
                randY = ((float)game.rand.NextDouble() * 2 - 1) * amplifier.Y;
            else if (y == 1) // Down
                randY = (float)game.rand.NextDouble() * amplifier.Y;
            else if (y == -1) // Up
                randY = (float)game.rand.NextDouble() * -1 * amplifier.Y;

            return new Vector2(randX, randY);
        }

        private void CheckWalls(Player player)
        {
            player.WallAbove = false;

            for (int i = game.walls.Count - 1; i >= 0; i--)
            {
                Wall wall = game.walls[i];
                Vector2 translation = player.Intersects(wall);
                if (translation != Vector2.Zero)
                {
                    if (player.CurrentState == Player.State.Slamming && wall.Health == 1)
                    {
                        game.walls.RemoveAt(i);
                        MakeParticles(player.Position, wall, GameData.NUM_PART_WALL, 0, 1);
                    }
                    else
                    {
                        translation.Y = 0;
                        player.Velocity.X = 0;
                        player.MovePosition(-translation);

                        if (wall.Position.Y < player.Position.Y)
                        {
                            player.WallAbove = true;
                            //Console.WriteLine("Wall at " + wall.Position);
                            //Console.WriteLine("Player at " + player.Position);
                        }
                    }

                    //if (translation.X != 0)
                    //    player.Velocity.X = 0;
                    //else
                    //    player.Velocity.Y = 0;
                }
            }

            player.MovePosition(new Vector2(0.0001f, 0)); // an extremely small amount to still render floor climbing
        }

        private void CheckObstacles(Player player)
        {
            for (int i = game.obstacles.Count - 1; i >= 0; i--)
            {
                Obstacle obstacle = game.obstacles[i];
                Vector2 translation = player.Intersects(obstacle);
                if (translation != Vector2.Zero)
                {
                    if (player.CurrentState == Player.State.Slamming)
                    {
                        game.walls.RemoveAt(i);
                        MakeParticles(player.Position, obstacle, GameData.NUM_PART_FLOOR, 0, 0);
                    }

                    if (translation.X != 0)
                        player.Velocity.X = 0;
                    else
                        player.Velocity.Y = 0;

                    player.MovePosition(-translation);
                }
            }

            player.MovePosition(new Vector2(0.0001f, 0)); // an extremely small amount to still render floor climbing
        }

        private void PerformSpecial(Player player, float deltaTime)
        {
            switch (player.CurrentAbility)
            {
                case Player.Ability.GravityPull:
                case Player.Ability.GravityPush:
                    float scale = GameData.GRAVITY_FORCE * (player.CurrentAbility == Player.Ability.GravityPull ? -1 : 1);

                    foreach (Particle part in game.particles)
                    {
                        Vector2 dist = part.Position - player.Position;
                        float length = dist.Length();
                        if (length != 0)
                        {
                            part.Velocity += deltaTime * scale * dist / (length * length); //the vectored form of the gravitational formula
                        }
                    }
                    foreach (Player body in game.players)
                    {
                        if (body != player)
                        {
                            Vector2 dist = body.Position - player.Position;
                            float length = dist.Length();
                            if (length != 0)
                            {
                                body.Velocity += deltaTime * scale * dist / (length * length); //the vectored form of the gravitational formula
                            }
                        }
                    }
                    break;
            }
        }

        public Body TestPoint(Vector2 point)
        {
            foreach (Body body in game.floors)
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
