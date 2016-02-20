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
    public class Player : Body
    {
        private const float BAR_WIDTH = 1f; // length of bar in meters
        private const float BAR_HEIGHT = 0.25f;

        public State CurrentState;
        public float StunTime;
        public float JumpTime;
        public bool PrevJump;
        public float AbilityOneTime, AbilityTwoTime, AbilityThreeTime;
        public Jump WallJump;
        public float WallJumpLeway;
        public float TargetVelocity;
        public Character CurrentCharacter;
        public SpriteEffects Flip;

        // Character-specific variables
        public Platform SpawnedPlatform;
        public float PlatformTime;

        public Vector2 GrappleTarget;
        public float TargetRadius;
        public bool GrappleRight;

        public bool Blink;

        public List<Projectile> Projectiles;

        public bool InAir { get { return CurrentState == State.Jumping || CurrentState == State.Stunned
                    || CurrentState == State.Flying; } }
        public bool CanJump { get { return CurrentState == State.Walking; } }

        public enum State
        {
            Jumping=0, Walking=1, Stunned=5, Flying=6
        }

        public enum Jump
        {
            None, Right, Left
        }

        public AnimatedSprite Sprite;

        public void ResetValues()
        {
            CurrentState = State.Jumping;
            StunTime = 0;
            JumpTime = 0;
            PrevJump = false;
            AbilityOneTime = 0;
            AbilityTwoTime = 0;
            AbilityThreeTime = 0;
            TargetVelocity = 0;
            WallJump = Jump.None;
            WallJumpLeway = 0;
            Flip = SpriteEffects.None;

            Projectiles = new List<Projectile>();
            Velocity = Vector2.Zero;
        }

        public Player(Texture2D texture, Vector2 position, Character character)
            : base(texture, position, new Vector2(0.9f, 2.8f))
        {
            Color = character.Color;
            CurrentCharacter = character;
            SpawnedPlatform = null;

            ResetValues();

            int[] animationFrames = { 4, 4, 2, 4, 2, 1, 1 };
            Origin = new Vector2(Origin.X / animationFrames.Max(), Origin.Y / animationFrames.Length);
            float textureScale = 2.8f / Origin.Y / 2f * (20f / 18f);
            Sprite = new AnimatedSprite(texture, this, animationFrames, textureScale);
        }

        /// <summary>
        /// This method is pretty much like an update() function for the player body since it is called every tick
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Move(float deltaTime)
        {
            //ActionTime -= deltaTime;
            if (CurrentState == State.Stunned || CurrentState == State.Flying)
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

                // stop wall sliding
                if (WallJump == Jump.Left || WallJump == Jump.Right)
                {
                    if (WallJumpLeway < 0)
                        WallJump = Jump.None;
                    WallJumpLeway -= deltaTime;
                }

                // face left and right
                if (Velocity.X > 0)
                    Flip = SpriteEffects.None;
                else if (Velocity.X < 0)
                    Flip = SpriteEffects.FlipHorizontally;

                // accelerate to target velocity
                float diff = Velocity.X - TargetVelocity;
                if (Math.Abs(diff) < GameData.MIN_VELOCITY)
                    Velocity.X = TargetVelocity;
                else if (InAir)
                    Velocity.X -= Math.Sign(diff) * deltaTime * GameData.AIR_ACCEL;
                else
                    Velocity.X -= Math.Sign(diff) * deltaTime * GameData.MAX_ACCEL;
            }

            // swing on grapple
            if (GrappleTarget != Vector2.Zero)
            {
                Vector2 prevPosition = Position;

                base.Move(deltaTime);

                //if (GrappleRight)
                //    angle -= GameData.GRAPPLE_SPEED * deltaTime;
                //else
                //    angle += GameData.GRAPPLE_SPEED * deltaTime;

                Vector2 dist = GrappleTarget - Position;
                float angle = (float)Math.Atan2(dist.Y, dist.X);

                // move towards TargetRadius
                float radius = dist.Length();
                radius -= (radius - TargetRadius) * GameData.GRAPPLE_ELASTICITY * deltaTime;
                MoveToPosition(new Vector2(GrappleTarget.X - (float)Math.Cos(angle) * radius,
                    GrappleTarget.Y - (float)Math.Sin(angle) * radius));
                Velocity = (Position - prevPosition) / deltaTime;

                //Console.WriteLine("Velocity: " + Velocity);
                //Console.WriteLine("prevPosition: " + prevPosition + "\tPosition: " + Position);
                //Console.WriteLine("X: " + -Math.Cos(angle) + "\tY: " + Math.Sin(angle));
            }
            else        // normal move (non-grapple)
                base.Move(deltaTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
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

        public void Kill()
        {
            Velocity = Vector2.Zero;
            MoveToPosition(GameData.PLAYER_START);
            Projectiles.Clear();
        }
    }
}
