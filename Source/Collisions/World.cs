﻿using System;
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
        private const float GRAVITY = 26f;
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
                    part.angle += part.angularVelocity * deltaTime;
                    part.Position += part.velocity * deltaTime;
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

                    player.Velocity.Y += GRAVITY * deltaTime;
                    //player.CanJump = false;

                    player.Move(deltaTime);

                    CheckWalls(player);

                    CheckFloors(player);
                }
            }
        }

        private void CalculateProjectile(Player player, float deltaTime, int projIndex)
        {
            Projectile proj = player.Projectiles[projIndex];
            proj.Move(deltaTime);
            if (proj.LiveTime > Projectile.MAX_LIVE)
            {
                player.Projectiles.RemoveAt(projIndex);
                return;
            }
            foreach (Player target in game.players)
            {
                if (proj.Intersects(target) != Vector2.Zero)
                {
                    target.StunTime = Player.STUN_LENGTH;
                    
                    player.Projectiles.RemoveAt(projIndex);
                    return;
                }
            }
            foreach (Floor floor in game.floors)
            {
                if (proj.Intersects(floor) != Vector2.Zero)
                {
                    player.Projectiles.RemoveAt(projIndex);
                    return;
                }
            }
            for (int i = game.walls.Count - 1; i >= 0; i--)
            {
                Wall wall = game.walls[i];
                if (proj.Intersects(wall) != Vector2.Zero)
                {
                    game.walls.RemoveAt(i);
					for(int x = 0; x < 10; x ++)
                        game.particles.Add(new Particle(wall.Position, new Vector2(GameData.PARTICLE_WIDTH, GameData.PARTICLE_WIDTH), wall.texture, 0f, rand(0, 0, new Vector2(GameData.PARTICLE_Y, GameData.PARTICLE_Y)), 0f, GameData.PARTICLE_LIFETIME/2, wall.Color));
					game.particles.Add(new Particle(wall.Position, game.font, "BAM!"));
					game.wallLengths [0]--;
                    player.Projectiles.RemoveAt(projIndex);
                    return;
                }
            }
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
                        translation.X = 0;
                        if (translation.Y != 0)
                        {
                            player.Velocity.Y = 0;
                            if (translation.Y > 0 && player.InAir)
                                player.CurrentState = Player.State.Walking;
                        }
                        player.MovePosition(-translation);

                        game.particles.Add(new Particle(player.Position + new Vector2(0f, player.Size.Y / 2), new Vector2(GameData.PARTICLE_WIDTH, GameData.PARTICLE_WIDTH), floor.texture, 0f, rand(0, -1, new Vector2(GameData.PARTICLE_X, GameData.PARTICLE_Y)), (float)game.rand.NextDouble() * GameData.PARTICLE_MAX_SPIN, GameData.PARTICLE_LIFETIME, Color.Azure));
                    }
                    else
                    {
                        // two things wrong with this and the one above it. Everything here is a magic number, and the line is about 150 columns long.
                        // please make the code readable (for this past commit and all future ones)
                        for(int i = 0; i < 5; i ++)
                            game.particles.Add(new Particle(player.Position, new Vector2(GameData.PARTICLE_WIDTH, GameData.PARTICLE_WIDTH), floor.texture, 0f, rand(0, 1, new Vector2(GameData.PARTICLE_X, GameData.PARTICLE_Y)), 0f, GameData.PARTICLE_LIFETIME, Color.Azure));
                        
                        float newFloorX = floor.Position.X + player.Position.X;
                        float sizeDiff = floor.Size.X / 2 + GameData.FLOOR_HOLE / 2;
                        float halfWidth = floor.Size.X / 2 - GameData.FLOOR_HOLE / 2;
                        float playerDist = player.Position.X - floor.Position.X;
                        game.floors.Add(new Floor(floor.texture, new Vector2((newFloorX - sizeDiff) / 2, floor.Position.Y), halfWidth + playerDist));
                        game.floors.Add(new Floor(floor.texture, new Vector2((newFloorX + sizeDiff) / 2, floor.Position.Y), halfWidth - playerDist));

                        game.floors.Remove(floor);
                        break;
                    }
                }
            }
            
			if (player.Position.Y > BOTTOM) {  // bottom of the level
				player.Velocity.Y = 0;

				// Kill the player
				player.TimeSinceDeath = GameData.DEAD_TIME;
				player.Projectiles.Clear ();
				//player.MovePosition(new Vector2(0f, -10f));
				if (player.Score == 0) {
					for (int i = 0; i < game.players.Count; i++) {
						game.players [i].Score++;
					}
				}
					player.Score--;
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
            for (int i = game.walls.Count - 1; i >= 0; i--)
            {
                Wall wall = game.walls[i];
                Vector2 translation = player.Intersects(wall);
                if (translation != Vector2.Zero)
                {
                    translation.Y = 0;
                    player.MovePosition(-translation);
                    if (translation.X != 0)
                        player.Velocity.X = 0;
                    //else
                    //    player.Velocity.Y = 0;
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
            return null;
        }
    }
}
