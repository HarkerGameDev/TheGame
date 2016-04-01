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
    /// A player uses a rectangle for collisions
    /// </summary>
    public class Player : Polygon
    {
        private const float BAR_WIDTH = 1f; // length of bar in meters
        private const float BAR_HEIGHT = 0.25f;

        public bool Alive;
        public State CurrentState;
        public float StunTime;
        public float JumpTime;
        public float JumpSpeed;
        public bool PrevJump;
        public float AbilityOneTime, AbilityTwoTime, AbilityThreeTime;
        public Direction WallJump;
        public Direction TargetVelocity;
        public Character CurrentCharacter;
        public SpriteEffects Flip;
        public bool FacingRight { get { return TargetVelocity == 0 ? Flip == SpriteEffects.None : TargetVelocity > 0; } }

        // Character-specific variables
        public Platform SpawnedPlatform;
        public float PlatformTime;
        public float JetpackTime;
        public bool JetpackEnabled;
        public int JumpsLeft;
        public List<Tuple<Vector2, Vector2>> PrevStates;     // Position, Velocity

        public Vector2 GrappleTarget;
        public float TargetRadius;
        public bool GrappleRight;

        public List<Projectile> Projectiles;

        public bool InAir { get { return CurrentState == State.Jumping || CurrentState == State.Stunned || CurrentState == State.WallStick; } }
        public bool CanJump { get { return CurrentState == State.Walking || CurrentState == State.Sliding; } }

        public enum State
        {
            Jumping=0, Walking=1, WallStick=2, Sliding=5, Stunned=6
        }

        public enum Direction
        {
            None, Right, Left
        }

        public AnimatedSprite Sprite;

        public void ResetValues()
        {
            Alive = true;
            CurrentState = State.Jumping;
            StunTime = 0;
            JumpTime = 0;
            PrevJump = false;
            AbilityOneTime = 0;
            AbilityTwoTime = 0;
            AbilityThreeTime = 0;
            TargetVelocity = 0;
            WallJump = Direction.None;
            Flip = SpriteEffects.None;

            PrevStates = new List<Tuple<Vector2, Vector2>>();
            Projectiles = new List<Projectile>();
            Velocity = Vector2.Zero;
        }

        public Player(Texture2D texture, Vector2 position, Character character)
            : base(texture, position, new Vector2(GameData.PLAYER_WIDTH, GameData.PLAYER_HEIGHT))
        {
            Color = character.Color;
            CurrentCharacter = character;
            SpawnedPlatform = null;

            ResetValues();

            int[] animationFrames = { 4, 4, 2, 4, 2, 1, 1 };
            Origin = new Vector2(Origin.X / animationFrames.Max(), Origin.Y / animationFrames.Length);
            float textureScale = GameData.PLAYER_HEIGHT / Origin.Y / 2f * (20f / 18f);
            Sprite = new AnimatedSprite(texture, this, animationFrames, textureScale);
        }

        /// <summary>
        /// This method is pretty much like an update() function for the player body since it is called every tick
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            if (!Alive)
                throw new Exception("Moving dead player");

            //ActionTime -= deltaTime;
            if (CurrentState == State.Stunned)
            {
                StunTime -= deltaTime;
                if (StunTime < 0)
                    CurrentState = State.Walking;
            }
            else
            {
                AbilityOneTime -= deltaTime;
                AbilityTwoTime -= deltaTime;
                AbilityThreeTime -= deltaTime;

                if (CurrentState == State.Jumping)
                    JumpTime -= deltaTime;

                // face left and right
                if (Velocity.X > 0)
                    Flip = SpriteEffects.None;
                else if (Velocity.X < 0)
                    Flip = SpriteEffects.FlipHorizontally;

                // apply drag, acceleration, and stick to wall if moving towards it
                if (InAir)
                {
                    if (TargetVelocity == Direction.Right)
                    {
                        if (Velocity.X < 0)
                            Velocity.X += GameData.AIR_DRAG * deltaTime;
                        Velocity.X += GameData.AIR_ACCEL * deltaTime;
                        if (WallJump == Direction.Right && Velocity.Y >= GameData.WALL_STICK_VEL)
                            CurrentState = State.WallStick;
                    }
                    else if (TargetVelocity == Direction.Left)
                    {
                        if (Velocity.X > 0)
                            Velocity.X -= GameData.AIR_DRAG * deltaTime;
                        Velocity.X -= GameData.AIR_ACCEL * deltaTime;
                        if (WallJump == Direction.Left && Velocity.Y >= GameData.WALL_STICK_VEL)
                            CurrentState = State.WallStick;
                    }
                    else
                    {
                        if (Math.Abs(Velocity.X) < GameData.MIN_VELOCITY)
                            Velocity.X = 0f;
                        else
                            Velocity.X -= Velocity.X * GameData.AIR_DRAG * deltaTime;
                    }
                }
                else
                {
                    CurrentState = State.Walking;
                    if (TargetVelocity == Direction.Right)
                    {
                        if (Velocity.X < 0)
                        {
                            Velocity.X -= Velocity.X * GameData.LAND_DRAG * deltaTime;
                            CurrentState = State.Sliding;
                        }
                        Velocity.X += GameData.LAND_ACCEL * deltaTime;
                    }
                    else if (TargetVelocity == Direction.Left)
                    {
                        if (Velocity.X > 0)
                        {
                            Velocity.X -= Velocity.X * GameData.LAND_DRAG * deltaTime;
                            CurrentState = State.Sliding;
                        }
                        Velocity.X -= GameData.LAND_ACCEL * deltaTime;
                    }
                    else
                    {
                        if (Math.Abs(Velocity.X) < GameData.MIN_VELOCITY)
                            Velocity.X = 0f;
                        else
                        {
                            Velocity.X -= Velocity.X * GameData.LAND_DRAG * deltaTime;
                            CurrentState = State.Sliding;
                        }
                    }
                }

                if (Velocity.X > GameData.MAX_VELOCITY)
                    Velocity.X = GameData.MAX_VELOCITY;
                else if (Velocity.X < -GameData.MAX_VELOCITY)
                    Velocity.X = -GameData.MAX_VELOCITY;

                if (CurrentState == State.WallStick)
                {
                    //Console.WriteLine("Wall stick");
                    if (Velocity.Y > GameData.WALL_STICK_VEL + GameData.MIN_VELOCITY)
                        Velocity.Y -= GameData.WALL_STICK_ACCEL * deltaTime;
                    else
                        Velocity.Y = GameData.WALL_STICK_VEL;
                }
            }

            // swing on grapple
            if (GrappleTarget != Vector2.Zero)
            {
                Vector2 prevPosition = Position;

                base.Update(deltaTime);

                //if (GrappleRight)
                //    angle -= GameData.GRAPPLE_SPEED * deltaTime;
                //else
                //    angle += GameData.GRAPPLE_SPEED * deltaTime;

                Vector2 dist = GrappleTarget - Position;
                float angle = (float)Math.Atan2(dist.Y, dist.X);

                // move towards TargetRadius
                float radius = dist.Length();
                if (radius > TargetRadius)
                    radius -= (radius - TargetRadius) * GameData.GRAPPLE_ELASTICITY * deltaTime;
                else
                    TargetRadius += (radius - TargetRadius) * GameData.GRAPPLE_ELASTICITY * deltaTime;
                MoveToPosition(new Vector2(GrappleTarget.X - (float)Math.Cos(angle) * radius,
                    GrappleTarget.Y - (float)Math.Sin(angle) * radius));
                Velocity = (Position - prevPosition) / deltaTime;

                //Console.WriteLine("Velocity: " + Velocity);
                //Console.WriteLine("prevPosition: " + prevPosition + "\tPosition: " + Position);
                //Console.WriteLine("X: " + -Math.Cos(angle) + "\tY: " + Math.Sin(angle));
            }
            else        // normal move (non-grapple)
                base.Update(deltaTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Alive)
                throw new Exception("Drawing dead player");

            if (GrappleTarget != Vector2.Zero)
            {
                Vector2 dist = GrappleTarget - Position;
                float rot = (float)Math.Atan2(dist.Y, dist.X);
                Vector2 origin = new Vector2(0f, 0.5f);
                Vector2 scale = new Vector2(ConvertUnits.ToDisplayUnits(dist.Length()), GameData.GRAPPLE_HEIGHT);
                spriteBatch.Draw(Game1.whiteRect, ConvertUnits.ToDisplayUnits(Position), null, Color.Brown, rot, origin, scale, SpriteEffects.None, 0f);
            }

            Sprite.Draw(spriteBatch);
            
            //Vector2 pos = new Vector2(Position.X - BAR_WIDTH / 2, Position.Y - Size.Y * 0.7f);
            //Game1.DrawRectangle(spriteBatch, pos, Color.LightSalmon, new Vector2(BAR_WIDTH, BAR_HEIGHT));
            //Game1.DrawRectangle(spriteBatch, pos, Color.Crimson, new Vector2(BAR_WIDTH * BoostTime / GameData.BOOST_LENGTH, BAR_HEIGHT));
        }

        public void Reset()
        {
            Velocity = Vector2.Zero;
            MoveToPosition(GameData.PLAYER_START);
            Projectiles.Clear();
        }

        public void Kill()
        {
            Alive = false;
        }
    }
}
