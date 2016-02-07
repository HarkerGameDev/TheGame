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
        public float TargetVelocity;
        public bool Ability1;
        public bool Ability2;
        public bool Ability3;
        public Character CurrentCharacter;

        public List<Projectile> Projectiles;

        public bool InAir { get { return CurrentState == State.Jumping || CurrentState == State.Stunned
                    || CurrentState == State.Flying || CurrentState == State.Climbing; } }
        public bool CanJump { get { return CurrentState == State.Walking; } }

        public enum State
        {
            Jumping=0, Walking=1, Climbing=4, Stunned=5, Flying=6
        }

        public AnimatedSprite Sprite;

        public void ResetValues()
        {
            CurrentState = State.Jumping;
            StunTime = 0;
            TargetVelocity = 0;
            Ability1 = false;
            Ability2 = false;
            Ability3 = false;

            Projectiles = new List<Projectile>();
            Velocity = Vector2.Zero;
        }

        public Player(Texture2D texture, Vector2 position, Character character)
            : base(texture, position, new Vector2(0.6f, 1.8f))
        {
            Color = character.Color;
            CurrentCharacter = character;

            ResetValues();

            int[] animationFrames = { 4, 4, 2, 4, 2, 1, 1 };
            Origin = new Vector2(Origin.X / animationFrames.Max(), Origin.Y / animationFrames.Length);
            float textureScale = 1.8f / Origin.Y / 2f * (20f / 18f);
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
                if (CurrentState == State.Jumping)
                    JumpTime -= deltaTime;

                float diff = Velocity.X - TargetVelocity;
                if (Math.Abs(diff) < GameData.MIN_VELOCITY)
                    Velocity.X = TargetVelocity;
                else if (InAir)
                    Velocity.X -= Math.Sign(diff) * deltaTime * GameData.AIR_ACCEL;
                else
                    Velocity.X -= Math.Sign(diff) * deltaTime * GameData.MAX_ACCEL;
            }
            base.Move(deltaTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
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
            Ability1 = false;
            Ability2 = false;
            Ability3 = false;
        }
    }
}
