﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Source.Graphics;

namespace Source.Collisions
{
    /// <summary>
    /// Holder for collisions. This will have most of the collision checking code
    /// Game1.cs should only be used to combine all the things together
    /// </summary>
    public class World
    {
        //private static float SLOPE_JUMP = (float)Math.Atan2(Source.GameData.JUMP_SPEED, Source.GameData.RUN_VELOCITY);
        public const float BOTTOM = 10000f;        // bottom of the level

        private Game1 game;
        private Random rand;

        public World(Game1 game)
        {
            this.game = game;
            rand = new Random();
        }

        public void Step(float deltaTime)
        {
            // Handle global particles
            for (int i = game.particles.Count - 1; i >= 0; i--)
            {
                Particle part = game.particles[i];
                part.LiveTime -= deltaTime;
                float alpha = part.LiveTime / GameData.PARTICLE_LIFETIME;
                part.Color.A = (byte)(alpha * alpha * 256);
                //Console.WriteLine("Alpha: " + part.Color.A);
                if (part.LiveTime < 0)
                    game.particles.RemoveAt(i);
                else if (TestPoint(part.Position) == null)
                {
                    part.Velocity.Y += GameData.GRAVITY_PART * deltaTime;
                    part.Angle += part.AngularVelocity * deltaTime;
                    part.Position += part.Velocity * deltaTime;
                }
            }

            // Handle drops
            for (int i = game.drops.Count - 1; i >= 0; i--)
            {
                Drop drop = game.drops[i];

                drop.Velocity.Y += GameData.GRAVITY * deltaTime;
                drop.Update(deltaTime);
                foreach (Polygon target in game.platforms)
                {
                    Vector2 translation = target.Intersects(drop);
                    if (translation != Vector2.Zero)
                    {
                        drop.MoveByPosition(translation);
                        drop.Velocity.X -= drop.Velocity.X * GameData.DROP_FRICTION * deltaTime;
                        if (translation.Y == 0)
                            drop.Velocity.X = 0;
                        if (translation.X == 0)
                            drop.Velocity.Y = 0;
                    }
                }

                drop.LiveTime -= deltaTime;
                switch (drop.Type)
                {
                    case Drop.Types.Bomb:
                        if (drop.LiveTime < 0)
                        {
                            ApplyImpulse(GameData.BOMB_FORCE, drop.Player, drop.Position);
                            foreach (Player player in game.players)
                            {
                                if (player != drop.Player && (player.Position - drop.Position).LengthSquared() < GameData.STUN_RADIUS)
                                {
                                    player.CurrentState = Player.State.Stunned;
                                    player.StunTime = GameData.STUN_TIME;
                                }
                            }
                            game.drops.RemoveAt(i);
                        }
                        break;
                    case Drop.Types.Singularity:
                        if (drop.LiveTime < 0)
                            game.drops.RemoveAt(i);
                        else
                            ApplyForce(-GameData.GRAVITY_FORCE, drop.Player, drop.Position, deltaTime);
                        break;
                    case Drop.Types.Trap:
                        if (drop.LiveTime < 0)
                        {
                            ApplyImpulse(GameData.TRAP_FORCE, drop.Player, drop.Position);
                            MakeParticles(drop.Position, drop.texture, GameData.TRAP_PARTICLES, 0, 0, drop.Color);
                            game.drops.RemoveAt(i);
                        }
                        else
                        {
                            foreach (Player player in game.players)
                            {
                                if (player != drop.Player && player.Intersects(drop) != Vector2.Zero)
                                {
                                    player.CurrentState = Player.State.Stunned;
                                    player.StunTime = GameData.STUN_TIME;
                                    drop.LiveTime = 0;
                                }
                            }
                        }
                        break;
                }
            }

            // Handle players and their projectiles
            int projStep = (int)Math.Ceiling(deltaTime * GameData.PROJ_SPEED / GameData.PROJ_WIDTH);
            float projDeltaTime = deltaTime / projStep;
            for (int x=game.players.Count-1; x>=0; x--)
            {
                Player player = game.players[x];
                if (!player.Alive)
                    continue;

                for (int i = player.Projectiles.Count - 1; i >= 0; i--)
                {
                    for (int j = 0; j < projStep; j++)
                    {
                        if (!CalculateProjectile(player, projDeltaTime, i))
                            break;
                    }
                }

                if (player.SpawnedPlatform != null)
                {
                    player.PlatformTime -= deltaTime;
                    if (player.PlatformTime < 0)
                    {
                        game.platforms.Remove(player.SpawnedPlatform);
                        player.SpawnedPlatform = null;
                    }
                }

                if (player.ClonedPlayer != null)
                {
                    player.CloneTime -= deltaTime;
                    AI clone = player.ClonedPlayer;
                    if (player.CloneTime < 0)
                    {
                        if (clone.SpawnedPlatform != null)
                        {
                            game.platforms.Remove(clone.SpawnedPlatform);
                            clone.SpawnedPlatform = null;
                        }
                        player.MoveToPosition(clone.Position);
                        player.Velocity = clone.Velocity;
                        game.playerControls.Remove(clone.Controls);
                        game.players.Remove(clone);
                        player.ClonedPlayer = null;
                        player.AbilityTwoTime = GameData.CLONE_COOLDOWN;
                    }
                }

                float gravity = GameData.GRAVITY * deltaTime;
                //if (player.WallJump != Player.Jump.None && player.Velocity.Y > 0)
                //    gravity *= GameData.WALL_SLIDE_SCALE;

                //if (player.GrappleTarget != Vector2.Zero)
                //{
                //    Vector2 dist = player.GrappleTarget - player.Position;
                //    float angle = (float)Math.Atan2(dist.X, dist.Y);
                //    float oppAngle = MathHelper.PiOver2 - angle;
                //    float component = gravity * (float)Math.Sin(angle);
                //    player.Velocity += new Vector2(component * (float)Math.Sin(oppAngle), component * (float)Math.Cos(oppAngle));
                //}
                //else
                    player.Velocity.Y += gravity;

                //int playerStep = Math.Max((int)Math.Ceiling(deltaTime * Math.Abs(player.Velocity.Y) / player.Size.Y * 1.5f),
                //                            (int)Math.Ceiling(deltaTime * Math.Abs(player.Velocity.X) / player.Size.X * 1.5f))
                //                            / 2;
                int playerStep = 1;
                if (playerStep < 1) playerStep = 1;

                if (playerStep > 1)
                    Console.WriteLine("Player step: " + playerStep);

                for (int i = 0; i < playerStep; i++)
                {
                    player.Update(deltaTime / playerStep);
                    CheckPlatforms(player);
                }
                CheckObstacles(player);

                //if (player.Ability1)
                //    PerformSpecial1(player, deltaTime);
                //if (player.Ability2)
                //    PerformSpecial2(player, deltaTime);
                //if (player.Ability3)
                //    PerformSpecial3(player, deltaTime);
            }
        }

        /// <summary>
        /// Updates, moves, and collides all projectiles for a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="deltaTime"></param>
        /// <param name="projIndex"></param>
        /// <returns>True if particle exists, false if it was removed</returns>
        private bool CalculateProjectile(Player player, float deltaTime, int projIndex)
        {
            Projectile proj = player.Projectiles[projIndex];
            switch (proj.Type)
            {
                case Projectile.Types.Rocket:
                    proj.Velocity.Y += GameData.ROCKET_GRAVITY * deltaTime;
                    break;
                case Projectile.Types.Boomerang:
                    Vector2 dist = proj.Position - player.Position;
                    if (dist != Vector2.Zero)
                    {
                        float radius = dist.Length();
                        dist /= radius;
                        //proj.Velocity.Y += GameData.BOOMERANG_GRAVITY * deltaTime * radius;
                        proj.Velocity -= dist * GameData.BOOMERANG_ATTRACT * deltaTime / radius;
                        //Console.WriteLine("Proj Velocity: {0}", proj.Velocity);
                        ApplyForce(-GameData.BOOMERANG_FORCE, player, proj.Position, deltaTime);
                    }
                    break;
                case Projectile.Types.Hook:
                    proj.Velocity.Y += GameData.HOOK_GRAVITY * deltaTime;
                    break;
            }
            proj.Update(deltaTime);

            if (proj.LiveTime < 0)
            {
                player.Projectiles.RemoveAt(projIndex);
                return false;
            }

            foreach (Player target in game.players)
            {
                if (proj.Intersects(target) != Vector2.Zero)
                {
                    if (proj.Type == Projectile.Types.Boomerang)
                    {
                        if (target == player && proj.LiveTime < GameData.BOOMERANG_LIFE_CUTOFF)
                        {
                            player.Projectiles.RemoveAt(projIndex);
                            player.AbilityTwoTime = 0;
                            return false;
                        }
                    }
                    else if (target != player)
                    {
                        switch (proj.Type)
                        {
                            case Projectile.Types.Rocket:
                                ApplyImpulse(GameData.ROCKET_FORCE, player, proj.Position);
                                MakeParticles(proj.Position, Game1.whiteRect, GameData.ROCKET_EXPLODE_PART, 0, 0, Color.Firebrick);
                                target.StunTime = GameData.ROCKET_STUN;
                                target.CurrentState = Player.State.Stunned;
                                break;
                            case Projectile.Types.Hook:
                                player.HookedPlayer = target;
                                //player.StunTime = target.StunTime = GameData.HOOK_STUN;
                                //player.CurrentState = target.CurrentState = Player.State.Stunned;
                                break;
                        }
                        player.Projectiles.RemoveAt(projIndex);
                        return false;
                    }
                }
            }
            foreach (Platform platform in game.platforms)
            {
                if (proj.Intersects(platform) != Vector2.Zero)
                {
                    switch (proj.Type)
                    {
                        case Projectile.Types.Rocket:
                            ApplyImpulse(GameData.ROCKET_FORCE, player, proj.Position);
                            MakeParticles(proj.Position, Game1.whiteRect, GameData.ROCKET_EXPLODE_PART, 0, 0, Color.Firebrick);
                            break;
                        case Projectile.Types.Hook:
                            player.HookedLocation = proj.Position;
                            break;
                    }

                    player.Projectiles.RemoveAt(projIndex);
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
        /// <param name="texture"></param>
        /// <param name="amount"></param>
        /// <param name="x">0 is left and right, 1 is right, -1 is left</param>
        /// <param name="y">0 is up and down, 1 is down, -1 is up</param>
        /// <param name="color"></param>
        public void MakeParticles(Vector2 pos, Texture2D texture, int amount, int x, int y, Color color)
        {
            // TODO make particles more general (ex: jetpack vs trap)
            for (int i = 0; i < amount; i++)
            {
                Vector2 velocity = randomVector(x, y, new Vector2(GameData.PARTICLE_X, GameData.PARTICLE_Y));
                float angularVelocity = 0.1f * ((float)rand.NextDouble() * 2 - 1);
                float size = (float)rand.NextDouble();
                float liveTime = 0.5f + 2f * (float)rand.NextDouble();

                game.particles.Add(new Particle(texture, pos, velocity, 0f, angularVelocity, color, size, liveTime));
            }
        }

        private void CheckPlatforms(Player player)
        {
            int totalCollisions = 0;
            foreach (Platform platform in game.platforms)
            {
                Vector2 translation = player.Intersects(platform);
                if (translation != Vector2.Zero)
                {
                    totalCollisions++;

                    //if (Math.Abs(translation.X) > Math.Abs(translation.Y)/* && !player.WallAbove*/)
                    //{
                    //    player.CurrentState = Player.State.Climbing;
                    //    //player.Velocity.Y = player.ActionTime > 0 ? -GameData.CLIMB_SPEED_FAST : -GameData.CLIMB_SPEED;
                    //}
                    //else if (player.CurrentState == Player.State.Climbing)
                    //    player.CurrentState = Player.State.Walking;

                    if (translation.Y == 0)    // Horizontal collision
                    {
                        if (player.CurrentState == Player.State.Stunned)
                            player.Velocity.X *= -1;
                        else
                        {
                            player.Velocity.X = 0;
                            //if (player.InAir && player.JumpTime <= 0)
                            //{
                            //if (player.WallJump == Player.Jump.None && player.Velocity.Y > 0)
                            //    player.Velocity.Y *= GameData.WALL_STICK_SCALE;
                            if (player.Velocity.Y >= GameData.WALL_STICK_VEL)
                            {
                                if (translation.X > 0 && player.TargetVelocity == Player.Direction.Right ||
                                    translation.X < 0 && player.TargetVelocity == Player.Direction.Left)
                                    player.CurrentState = Player.State.WallStick;
                            }
                            //player.WallJumpLeway = GameData.WALL_JUMP_LEWAY;
                            //}
                        }
                    }
                    else        // Vertical or diagonal collision
                    {
                        if (player.CurrentState == Player.State.Stunned)
                            player.Velocity.Y *= -1;
                        else
                        {
                            player.Velocity.Y = 0;
                            player.JumpTime = 0f;
                            if (translation.Y > 0 && player.InAir)
                            {
                                player.CurrentState = Player.State.Walking;
                                player.GrappleTarget = Vector2.Zero;
                                player.JetpackTime = GameData.JETPACK_TIME;
                                player.JetpackEnabled = false;
                                player.JumpsLeft = GameData.TOTAL_JUMPS;
                            }
                        }
                    }
                    player.MoveByPosition(-translation);
#if DEBUG
                    if (translation.LengthSquared() > 1)
                    {
                        Console.WriteLine("Collision translation of {0}", translation);
                        platform.Color = Color.Green;
                    }
#endif
                }
                //else        // player is Slamming or Stunned
                //{
                //    if (platform.Rotation != 0)
                //    {
                //        MakeParticles(player.Position, platform, GameData.NUM_PART_FLOOR, 0, 1);
                //    }
                //    else
                //    {
                //        MakeParticles(player.Position, platform, GameData.NUM_PART_FLOOR, 0, 1);

                //        float newFloorX = platform.Position.X + player.Position.X;
                //        float sizeDiff = platform.Size.X / 2 + GameData.FLOOR_HOLE / 2;
                //        float halfWidth = platform.Size.X / 2 - GameData.FLOOR_HOLE / 2;
                //        float playerDist = player.Position.X - platform.Position.X;

                //        if (halfWidth + playerDist > GameData.MIN_FLOOR_WIDTH)
                //            game.platforms.Add(new Platform(platform.texture, new Vector2((newFloorX - sizeDiff) / 2, platform.Position.Y), halfWidth + playerDist));
                //        if (halfWidth - playerDist > GameData.MIN_FLOOR_WIDTH)
                //            game.platforms.Add(new Platform(platform.texture, new Vector2((newFloorX + sizeDiff) / 2, platform.Position.Y), halfWidth - playerDist));
                //    }

                //    game.platforms.Remove(platform);
                //    break;
                //}
            }

			if (player.Position.Y > BOTTOM) {  // bottom of the level
				// Kill the player
                player.Reset();
				//if (player.Score == 0) {
				//	for (int i = 0; i < game.players.Count; i++) {
				//		game.players [i].Score++;
				//	}
				//}
				//player.Score--;
            }

            if (totalCollisions == 0 && player.CurrentState != Player.State.Stunned && player.WallJump == Player.Direction.None)
                player.CurrentState = Player.State.Jumping;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">0 is left and right, 1 is right, -1 is left</param>
        /// <param name="y">0 is up and down, 1 is down, -1 is up</param>
        /// <param name="amplifier"></param>
        /// <returns></returns>
        private Vector2 randomVector(int x, int y, Vector2 amplifier)
        {
            float randX = 0;
            float randY = 0;

            if (x == 0) // Left & Right
                randX = ((float)rand.NextDouble() * 2 - 1) * amplifier.X;
            else if (x == 1) // Right
                randX = (float)rand.NextDouble() * amplifier.X;
            else if (x == -1) // Left
                randX = (float)rand.NextDouble() * -1 * amplifier.X;

            if (y == 0) // Up & Down
                randY = ((float)rand.NextDouble() * 2 - 1) * amplifier.Y;
            else if (y == 1) // Down
                randY = (float)rand.NextDouble() * amplifier.Y;
            else if (y == -1) // Up
                randY = (float)rand.NextDouble() * -1 * amplifier.Y;

            return new Vector2(randX, randY);
        }

        private void CheckObstacles(Player player)
        {
            for (int i = game.obstacles.Count - 1; i >= 0; i--)
            {
                Obstacle obstacle = game.obstacles[i];
                Vector2 translation = player.Intersects(obstacle);
                if (translation != Vector2.Zero)
                {
                    //if (player.CurrentState == Player.State.Slamming)
                    //{
                    //    game.obstacles.RemoveAt(i);
                    //    MakeParticles(obstacle.Position, obstacle, GameData.NUM_PART_OBSTACLE, 0, 0);
                    //}
                    if (player.Velocity.Y < 0)  // player going up
                    {
                        player.Velocity.Y = -GameData.OBSTACLE_JUMP;
                        player.StunTime = GameData.OBSTACLE_STUN;
                        //player.CurrentState = Player.State.Flying;
                    }
                    else if (translation.Y == 0) // hitting from side
                    {
                        player.Velocity.X *= GameData.OBSTACLE_SLOW;
                        player.Velocity.Y = 0;
                        player.CurrentState = Player.State.Stunned;
                        player.StunTime = GameData.OBSTACLE_HIT_STUN;
                        game.obstacles.RemoveAt(i);
                        MakeParticles(obstacle.Position, obstacle.texture, GameData.NUM_PART_OBSTACLE, 0, 0, obstacle.Color);
                    }
                    else if (translation.Y > 0) // hitting from top
                    {
                        player.MoveByPosition(-translation);
                        player.Velocity.Y = 0;
                        player.CurrentState = Player.State.Walking;
                    }
                }
            }
        }

        private void ApplyImpulse(float scale, Player player, Vector2 position)
        {
            // Impulse drops off as 1/r
            foreach (Player body in game.players)
            {
                if (body != player)
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
                            //Console.WriteLine("Applying impulse: " + force);
                            body.Velocity = force; // 1/r for gravity
                        }
                        else
                            body.Velocity += force;
                    }
                }
            }
        }

        private void ApplyForce(float scale, Polygon player, Vector2 position, float deltaTime)
        {
            // Force drops off as 1/r for objects and 1/r^2 for players
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
                    part.Velocity += force * deltaTime;
                }
            }
            foreach (Drop drop in game.drops)
            {
                Vector2 dist = drop.Position - position;
                float length = dist.Length();
                if (length != 0)
                {
                    Vector2 force = scale * dist / (length * length);
                    if (force.LengthSquared() > GameData.MAX_FORCE)
                    {
                        force.Normalize();
                        force *= GameData.MAX_FORCE;
                    }
                    drop.Velocity += force * deltaTime;
                }
            }
            foreach (Player body in game.players)
            {
                if (body != player)
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
                        Console.WriteLine("Applying force: " + force);
                        body.Velocity += force * deltaTime; // 1/r for gravity

                        // User basically loses control if they are closer than the threshold
                        if (length < GameData.BOOMERANG_ANTIGRAV)
                        {
                            //if (force.X * body.Velocity.X > 0)      // Velocity.X and force are in same direction
                            //    body.TargetVelocity = body.Velocity.X;
                            body.Velocity.Y -= GameData.GRAVITY * deltaTime;
                        }
                        //Console.WriteLine("TargetVelocity: {0}", body.TargetVelocity);
                    }
                }
            }
        }

        public Polygon TestPoint(Vector2 point)
        {
            foreach (Polygon body in game.platforms)
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
