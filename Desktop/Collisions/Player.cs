using System;
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
        public bool Alive;
        public int Score;
        public int Checkpoints;
        public float Progress;      // progress through the level (based on checkpoints)
        public LinkedListNode<Vector2> Node;    // Last passed checkpoint node
        public int Place;           // the place in the race
        public State CurrentState;
        public float StunTime;
        public float JumpTime;
        public float JumpSpeed;
        public bool PrevJump;
        public float AttackTime;
        public bool ShowSmear;
        public float AbilityOneTime, AbilityTwoTime, AbilityThreeTime;
        public Direction WallJump;
        public Direction TargetVelocity;
        public Character CurrentCharacter;
        public SpriteEffects Flip;
        public bool FacingRight { get { return TargetVelocity == Direction.None ? Flip == SpriteEffects.None : TargetVelocity == Direction.Right; } }

        public ParticleEmitter SlideEmitter, JetpackEmitter;

        // Character-specific variables
        public Platform SpawnedPlatform;
        public float PlatformTime;
        public AI ClonedPlayer;
        public float CloneTime;
        public float JetpackTime;
        public bool JetpackEnabled;
        public int JumpsLeft;
        public List<Tuple<Vector2, Vector2>> PrevStates;     // Position, Velocity

        public Vector2 GrappleTarget;
        public Player HookedPlayer;
        public Vector2 HookedLocation;
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
            AttackTime = 0;
            ShowSmear = false;
            AbilityOneTime = 0;
            AbilityTwoTime = 0;
            AbilityThreeTime = 0;
            TargetVelocity = Direction.None;
            WallJump = Direction.None;
            Flip = SpriteEffects.None;

            PrevStates = new List<Tuple<Vector2, Vector2>>();
            Projectiles = new List<Projectile>();
            Velocity = Vector2.Zero;

            GrappleTarget = Vector2.Zero;
            HookedPlayer = null;
            HookedLocation = Vector2.Zero;
        }

        public Player(Texture2D texture, Vector2 position, Character character, LinkedListNode<Vector2> checkpoint)
            : base(texture, position, new Vector2(GameData.PLAYER_WIDTH, GameData.PLAYER_HEIGHT))
        {
            Color = character.Color;
            CurrentCharacter = character;
            SpawnedPlatform = null;
            ClonedPlayer = null;
            Score = 0;
            Checkpoints = 0;
            Progress = 0;
            Place = 0;
            Node = checkpoint;

            ResetValues();

            int[] animationFrames = { 4, 4, 2, 4, 2, 1, 1 };
            Origin = new Vector2(Origin.X / animationFrames.Max(), Origin.Y / animationFrames.Length);
            float textureScale = GameData.PLAYER_HEIGHT / Origin.Y / 2f * (20f / 18f);
            Sprite = new AnimatedSprite(texture, this, animationFrames, textureScale);

            SlideEmitter = new ParticleEmitter(GameData.SLIDE_TEXTURES, Position, 75f);
            SlideEmitter.Red = SlideEmitter.Blue = SlideEmitter.Green = 1f;
            SlideEmitter.RedVar = SlideEmitter.BlueVar = SlideEmitter.GreenVar = 0f;
            SlideEmitter.AngVelVar = 0.001f;
            SlideEmitter.LiveTime = 5f;
            SlideEmitter.VelVarX = SlideEmitter.VelVarY = 0.5f;

            JetpackEmitter = new ParticleEmitter(GameData.JETPACK_TEXTURES, Position, 140f);
            JetpackEmitter.Size = 1.5f;
            JetpackEmitter.Red = 0.62f;
            JetpackEmitter.Blue = 0.16f;
            JetpackEmitter.Green = 0.1f;
            JetpackEmitter.RedVar = JetpackEmitter.BlueVar = JetpackEmitter.GreenVar = 0f;
        }

        /// <summary>
        /// This method is pretty much like an update() function for the player body since it is called every tick
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            if (!Alive)
                throw new Exception("Moving dead player");

            ShowSmear = false;

            // pull towards hooked player
            if (HookedPlayer != null)
            {
                Vector2 dist = HookedPlayer.Position - Position;
                if (dist.LengthSquared() < 1)
                    HookedPlayer = null;
                else
                {
                    dist.Normalize();
                    dist *= GameData.HOOK_PULL * deltaTime;
                    Velocity += dist;
                    HookedPlayer.Velocity -= dist;
                }
            }
            else if (HookedLocation != Vector2.Zero)
            {
                Vector2 dist = HookedLocation - Position;
                if (dist.LengthSquared() < 1)
                    HookedLocation = Vector2.Zero;
                else
                {
                    dist.Normalize();
                    dist *= GameData.HOOK_PULL * deltaTime;
                    Velocity += dist;
                }
            }

            //ActionTime -= deltaTime;
            if (CurrentState == State.Stunned)
            {
                StunTime -= deltaTime;
                if (StunTime < 0)
                    CurrentState = State.Walking;
            }
            else
            {
                AttackTime -= deltaTime;
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
                        //if (WallJump == Direction.Right && Velocity.Y >= GameData.WALL_STICK_VEL)
                        //    CurrentState = State.WallStick;
                        if (WallJump == Direction.Left)
                            WallJump = Direction.None;
                    }
                    else if (TargetVelocity == Direction.Left)
                    {
                        if (Velocity.X > 0)
                            Velocity.X -= GameData.AIR_DRAG * deltaTime;
                        Velocity.X -= GameData.AIR_ACCEL * deltaTime;
                        //if (WallJump == Direction.Left && Velocity.Y >= GameData.WALL_STICK_VEL)
                        //    CurrentState = State.WallStick;
                        if (WallJump == Direction.Right)
                            WallJump = Direction.None;
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

                Vector2 bottom = new Vector2(Position.X, Position.Y + Size.Y / 2f);
                if (CurrentState == State.Sliding)
                {
                    SlideEmitter.Enabled = true;
                    SlideEmitter.EmitterLocation = bottom;
                    SlideEmitter.VelY = -SlideEmitter.VelVarY;
                }
                else if (CurrentState == State.WallStick)
                {
                    SlideEmitter.Enabled = true;
                    SlideEmitter.EmitterLocation = Position;
                    SlideEmitter.VelY = 0f;
                }
                else
                {
                    SlideEmitter.Enabled = false;
                }
                //SlideEmitter.Update(deltaTime);

                JetpackEmitter.Enabled = JetpackEnabled && JetpackTime > 0;
                JetpackEmitter.EmitterLocation = bottom;
                //JetpackEmitter.Update(deltaTime);
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

            // Draw lines
            if (GrappleTarget != Vector2.Zero)
            {
                DrawLine(spriteBatch, Position, GrappleTarget, GameData.GRAPPLE_HEIGHT, Color.Brown);
            }
            foreach (Projectile proj in Projectiles)
            {
                if (proj.Type == Projectile.Types.Hook)
                {
                    DrawLine(spriteBatch, Position, proj.Position, GameData.HOOK_HEIGHT, Color.SaddleBrown);
                }
            }
            if (HookedPlayer != null)
            {
                DrawLine(spriteBatch, Position, HookedPlayer.Position, GameData.HOOK_HEIGHT, Color.Salmon);
            }
            else if (HookedLocation != Vector2.Zero)
            {
                DrawLine(spriteBatch, Position, HookedLocation, GameData.HOOK_HEIGHT, Color.Salmon);
            }

            if (ShowSmear)
            {
                // TODO replace attack with some proper animation with sparky flash effects
                spriteBatch.Draw(Game1.smear,
                    ConvertUnits.ToDisplayUnits(new Vector2(Position.X + (FacingRight ? 1f : -1f), Position.Y)),
                    null, Color.White, 0f, Game1.smear.Bounds.Size.ToVector2() / 2f, ConvertUnits.ToDisplayUnits(0.01f),
                    FacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            }

            Sprite.Draw(spriteBatch);

            //SlideEmitter.Draw(spriteBatch);
            //JetpackEmitter.Draw(spriteBatch);
            //particles!!!

            Vector2 pos = ConvertUnits.ToDisplayUnits(new Vector2(Position.X, Position.Y - Size.Y * 0.7f));
            Vector2 origin = new Vector2(0.5f);
            spriteBatch.Draw(Game1.whiteRect, pos, null, GameData.BAR_1_COLOR, 0f, origin,
                ConvertUnits.ToDisplayUnits(new Vector2(JetpackTime > 0 ? JetpackTime : AbilityOneTime * GameData.BAR_SCALE, GameData.BAR_HEIGHT)), SpriteEffects.None, 0.1f);
            spriteBatch.Draw(Game1.whiteRect, pos, null, GameData.BAR_2_COLOR, 0f, origin,
                ConvertUnits.ToDisplayUnits(new Vector2(AbilityTwoTime * GameData.BAR_SCALE, GameData.BAR_HEIGHT)), SpriteEffects.None, 0.1f);
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, float height, Color color)
        {
            Vector2 dist = end - start;
            float rot = (float)Math.Atan2(dist.Y, dist.X);
            Vector2 origin = new Vector2(0f, 0.5f);
            Vector2 scale = new Vector2(ConvertUnits.ToDisplayUnits(dist.Length()), height);
            spriteBatch.Draw(Game1.whiteRect, ConvertUnits.ToDisplayUnits(start), null, color, rot, origin, scale, SpriteEffects.None, 1f);
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
