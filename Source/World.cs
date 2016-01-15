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
                else if(part.type == Particle.Type.Texture)
                {
                    if (TestPoint(part.Position) == null)
                    {
                        part.Velocity.Y += GameData.GRAVITY_PART * deltaTime;
                        part.Angle += part.AngularVelocity * deltaTime;
                        part.Position += part.Velocity * deltaTime;
                    }
                }
            }

            for (int i = game.drops.Count - 1; i >= 0; i--)
            {
                Drop drop = game.drops[i];
                drop.LiveTime -= deltaTime;
                if (drop.LiveTime < 0)
                {
                    if (drop.type == Drop.Type.Bomb)
                    {
                        ApplyGravity(GameData.BOMB_FORCE, drop.Player, drop.Position, 1);
                        foreach (Player player in game.players)
                        {
                            if (player != drop.Player && (player.Position - drop.Position).LengthSquared() < GameData.STUN_RADIUS)
                            {
                                player.CurrentState = Player.State.Stunned;
                                player.StunTime = GameData.STUN_TIME;
                            }
                        }
                    }
                    game.drops.RemoveAt(i);
                }
                else
                {
                    drop.Velocity.Y += GameData.GRAVITY * deltaTime;
                    drop.Move(deltaTime);
                    foreach (Body target in game.floors)
                    {
                        Vector2 translation = target.Intersects(drop);
                        if (translation != Vector2.Zero)
                        {
                            drop.MovePosition(translation);
                            drop.Velocity.X -= drop.Velocity.X * GameData.DROP_FRICTION * deltaTime;
                            if (translation.Y == 0)
                                drop.Velocity.X = 0;
                            if (translation.X == 0)
                                drop.Velocity.Y = 0;
                        }
                    }
                    foreach (Body target in game.walls)
                    {
                        Vector2 translation = target.Intersects(drop);
                        if (translation != Vector2.Zero)
                        {
                            drop.MovePosition(translation);
                            if (translation.Y == 0)
                                drop.Velocity.X = 0;
                            if (translation.X == 0)
                                drop.Velocity.Y = 0;
                        }
                    }
                    if (drop.type == Drop.Type.Singularity)
                        ApplyGravity(-GameData.GRAVITY_FORCE, drop.Player, drop.Position, deltaTime);
                }
            }

            int projStep = (int)Math.Ceiling(deltaTime * GameData.PROJ_SPEED / GameData.PROJ_WIDTH);
            float projDeltaTime = deltaTime / projStep;
            foreach (Player player in game.players)
            {
                if (player.TimeSinceDeath <= 0)
                {
                    while (player.BoostPart < 0)
                    {
                        player.BoostPart += GameData.BOOST_PART_TIME;
                        MakeParticles(new Vector2(player.Position.X, player.Position.Y + player.Size.Y / 3),
                            game.floors[0], 1, 0, -1);
                    }

                    for (int i = player.Projectiles.Count - 1; i >= 0; i--)
                    {
                        for (int j = 0; j < projStep; j++)
                        {
                            if (!CalculateProjectile(player, projDeltaTime, i))
                                break;
                        }
                    }

                    player.Velocity.Y += GameData.GRAVITY * deltaTime;
                    int playerStep = Math.Max((int)Math.Ceiling(deltaTime * player.Velocity.Y / player.Size.Y / 1.5f),
                                              (int)Math.Ceiling(deltaTime * player.Velocity.X / player.Size.X / 1.5f));
                    if (playerStep < 1) playerStep = 1;

                    for (int i = 0; i < playerStep; i++)
                    {
                        player.Move(deltaTime / playerStep);
                        CheckWalls(player);
                    }
                    CheckFloors(player);
                    CheckObstacles(player);

                    if (player.AbilityActive)
                        PerformSpecial(player, deltaTime);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="deltaTime"></param>
        /// <param name="projIndex"></param>
        /// <returns>True if particle exists, false if it was removed</returns>
        private bool CalculateProjectile(Player player, float deltaTime, int projIndex)
        {
            Projectile proj = player.Projectiles[projIndex];
            proj.Move(deltaTime);
            if (proj.LiveTime > GameData.PROJ_LIVE)
            {
                player.Projectiles.RemoveAt(projIndex);
                return false;
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
                        game.particles.Add(new Particle(wall.Position, game.fontSmall, "BAM!"));
                    }
                    else
                        wall.SetColor();

                    player.Projectiles.RemoveAt(projIndex);
                    return false;
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
                    return false;
                }
            }
            foreach (Obstacle obstacle in game.obstacles)
            {
                if (proj.Intersects(obstacle) != Vector2.Zero)
                {
                    player.Projectiles.RemoveAt(projIndex);
                    return false;
                }
            }
            return true;
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
            foreach (Floor floor in game.floors)
            {
                Vector2 translation = player.Intersects(floor);
                if (translation != Vector2.Zero)
                {
                    if (player.CurrentState != Player.State.Slamming && player.CurrentState != Player.State.Stunned)
                    {
                        totalCollisions++;

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
                    }
                    else        // player is Slamming or Stunned
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
				if (player.Score == 0) {
					for (int i = 0; i < game.players.Count; i++) {
						game.players [i].Score++;
					}
				}
				player.Score--;
                player.Velocity = Vector2.Zero;
            }

            if (totalCollisions == 0 && !player.InAir && player.CurrentState != Player.State.Stunned)
                player.CurrentState = Player.State.Jumping;
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
                    if ((player.CurrentState == Player.State.Slamming || player.CurrentState == Player.State.Stunned) && wall.Health == 1)
                    {
                        game.walls.RemoveAt(i);
                        MakeParticles(player.Position, wall, GameData.NUM_PART_WALL, 0, 1);
                    }
                    else
                    {
                        translation.Y = 0;
                        if (wall.Health > 1)
                        {
                            player.Velocity.X = 0;
                            player.MovePosition(-translation);
                        }
                        else
                        {
                            player.Velocity.X *= GameData.WALL_SLOW;
                            game.walls.RemoveAt(i);
                            MakeParticles(player.Position, wall, GameData.NUM_PART_WALL, 1, 0);
                        }

                        if (wall.Position.Y < player.Position.Y)
                            player.WallAbove = true;
                    }
                }
            }

            player.MovePosition(new Vector2(0.0001f, 0)); // move an extremely small amount to still render floor climbing
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
                        game.obstacles.RemoveAt(i);
                        MakeParticles(obstacle.Position, obstacle, GameData.NUM_PART_OBSTACLE, 0, 0);
                    }
                    else if (player.Velocity.Y < 0)  // player going up
                    {
                        player.Velocity.Y = -GameData.OBSTACLE_JUMP;
                        player.StunTime = GameData.OBSTACLE_STUN;
                        player.CurrentState = Player.State.Stunned;
                        Console.WriteLine("OBSTACLE JUMP");
                    }
                    else if (translation.Y == 0) // hitting from side
                    {
                        player.Velocity.X *= GameData.WALL_SLOW;
                        player.CurrentState = Player.State.Stunned;
                        player.StunTime = GameData.OBSTACLE_HIT_STUN;
                        game.obstacles.RemoveAt(i);
                        MakeParticles(obstacle.Position, obstacle, GameData.NUM_PART_OBSTACLE, 0, 0);
                        Console.WriteLine("OBSTACLE STUN");
                    }
                    else if (translation.Y > 0) // hitting from top
                    {
                        player.MovePosition(-translation);
                        player.Velocity.Y = 0;
                        player.CurrentState = Player.State.Walking;
                        Console.WriteLine("OBSTACLE WALK");
                    }
                }
            }
        }

        private void PerformSpecial(Player player, float deltaTime)
        {
            switch (player.CurrentAbility)
            {
                case Player.Ability.GravityPull:
                    ApplyGravity(-GameData.GRAVITY_FORCE, player, player.Position, deltaTime);
                    break;
                case Player.Ability.GravityPush:
                    ApplyGravity(GameData.GRAVITY_FORCE, player, player.Position, deltaTime);
                    break;
                case Player.Ability.Explosive:
                    game.drops.Add(new Drop(player, Game1.whiteRect, player.Position, 0.16f, Drop.Type.Bomb));
                    player.AbilityActive = false;
                    break;
                case Player.Ability.Singularity:
                    game.drops.Add(new Drop(player, Game1.whiteRect, player.Position, 0.08f, Drop.Type.Singularity));
                    player.AbilityActive = false;
                    break;
            }
        }

        private void ApplyGravity(float scale, Body player, Vector2 position, float deltaTime)
        {
            foreach (Particle part in game.particles)
            {
                Vector2 dist = part.Position - position;
                float length = dist.Length();
                if (length != 0)
                {
                    Vector2 force = scale * dist / (length * length);
                    if (force.LengthSquared() > GameData.MAX_FORCE)
                    {
                        force.Normalize();
                        force *= GameData.MAX_FORCE;
                    }
                    part.Velocity += force * deltaTime; // 1/r for gravity
                }
            }
            foreach (Player body in game.players)
            {
                if (body != player && body.TimeSinceDeath <= 0)
                {
                    //Console.WriteLine(body.Velocity + "\t\t" + player.Velocity);
                    Vector2 dist = body.Position - position;
                    float length = dist.Length();
                    if (length != 0)
                    {
                        Vector2 force = scale * dist / (length * length);
                        if (force.Length() > GameData.MAX_FORCE)
                        {
                            force.Normalize();
                            force *= GameData.MAX_FORCE;
                        }
                        //Console.WriteLine("Applying force: " + force);
                        body.Velocity += force * deltaTime; // 1/r for gravity
                    }
                }
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
            foreach (Body body in game.walls)
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
