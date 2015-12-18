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

        public State CurrentState = State.Jumping;
        public double TimeSinceDeath = 0;
        public int Score = 0;
        public List<Projectile> Projectiles;
        public float BoostTime = GameData.BOOST_LENGTH;
        public float StunTime = 0;
        public bool WallAbove = false;
        public float TargetVelocity = GameData.RUN_VELOCITY;
        public float SpawnY = 0;
        public bool AbilityActive = false;
        public Ability CurrentAbility = Ability.GravityPull;

        public bool InAir { get { return CurrentState == State.Jumping || CurrentState == State.Slamming; } }
        public bool CanJump { get { return CurrentState == State.Walking || CurrentState == State.Boosting; } }

        public enum State
        {
            Jumping=0, Walking=1, Slamming=2, Boosting=3, Climbing=4, Stunned=5
        }

        public enum Ability
        {
            GravityPull, GravityPush, Explosive, Singularity
        }

        public AnimatedSprite Sprite;

        public Player(Texture2D texture, Vector2 position, Color color, Ability ability)
            : base(texture, position, new Vector2(0.6f, 1.8f))
        {
            this.Color = color;
            CurrentAbility = ability;

            Projectiles = new List<Projectile>();
            Velocity.X = 0f;

            int[] animationFrames = { 4, 4, 2, 4, 2, 1 };
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
            if (CurrentState == State.Stunned)
            {
                StunTime -= deltaTime;
                if (StunTime < 0)
                    CurrentState = State.Walking;
            }
            else
            {
                float diff = Velocity.X - TargetVelocity;
                if (Math.Abs(diff) < GameData.MIN_VELOCITY)
                    Velocity.X = TargetVelocity;
                else // if (!InAir)
                    Velocity.X -= Math.Sign(diff) * deltaTime * GameData.MAX_ACCEL;

                if (CurrentState == State.Boosting)
                    BoostTime -= deltaTime;
                else if (BoostTime < GameData.BOOST_LENGTH)
                    BoostTime += deltaTime * GameData.BOOST_LENGTH / GameData.BOOST_REGEN;

                if (BoostTime < 0)
                {
                    TargetVelocity = GameData.RUN_VELOCITY;
                    CurrentState = State.Walking;
                }
            }
            base.Move(deltaTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Sprite.Draw(spriteBatch);

            Vector2 pos = new Vector2(Position.X - BAR_WIDTH / 2, Position.Y - Size.Y * 0.7f);
            Game1.DrawRectangle(spriteBatch, pos, Color.LightSalmon, new Vector2(BAR_WIDTH, BAR_HEIGHT));
            Game1.DrawRectangle(spriteBatch, pos, Color.Crimson, new Vector2(BAR_WIDTH * BoostTime / GameData.BOOST_LENGTH, BAR_HEIGHT));
        }

        public void Kill(Random rand)
        {
            Velocity = new Vector2(GameData.RUN_VELOCITY, 0f);
            TimeSinceDeath = GameData.DEAD_TIME;
            Projectiles.Clear();
            SpawnY = -rand.Next(GameData.MIN_SPAWN, GameData.MAX_SPAWN);
            BoostTime = GameData.BOOST_LENGTH;
            AbilityActive = false;
        }
    }
}
