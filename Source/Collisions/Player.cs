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
    /// A player uses a rectangle for collisions (for now)
    /// </summary>
    public class Player : Body
    {
        private const float BAR_WIDTH = 1f; // length of bar in meters
        private const float BAR_HEIGHT = 0.25f;

        //public bool CanJump = false;
        //public bool Ghost = false;
        //public Body Ignore = null;
        //public float oldY = 0;
        //public int CollideBottom = 0;

        public State CurrentState = State.Jumping;
        public double TimeSinceDeath = 0;
        public int Score = 0;
        public List<Projectile> Projectiles;
        public float BoostTime = GameData.BOOST_LENGTH;
        public float StunTime = GameData.STUN_LENGTH;
        public bool WallAbove = false;
        public float TargetVelocity = GameData.RUN_VELOCITY;
        public float SpawnY = 0;

        public bool InAir { get { return CurrentState == State.Jumping || CurrentState == State.Slamming; } }
        public bool CanJump { get { return CurrentState == State.Walking || CurrentState == State.Boosting; } }

        public enum State
        {
            Jumping=0, Walking=1, Slamming=2, Sliding=3, Boosting=4, Climbing=5
        }

        public AnimatedSprite Sprite;

        public Player(Texture2D texture, Vector2 position, Color color)
            : base(texture, position, new Vector2(0.6f, 1.8f))
        {
            this.Color = color;

            Projectiles = new List<Projectile>();
            Velocity.X = 0f;

            int[] animationFrames = { 4, 4, 2, 2, 4, 2 };
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
            float diff = Velocity.X - TargetVelocity;
            if (Math.Abs(diff) < GameData.MIN_VELOCITY)
                Velocity.X = TargetVelocity;
            else // if (!InAir)
                Velocity.X -= Math.Sign(diff) * deltaTime * GameData.MAX_ACCEL;

            if (StunTime > 0)
                base.MovePosition(Velocity * GameData.STUN_SCALE * deltaTime); // I know this scales Y velocity too, but that might be interesting
            else
     	        base.Move(deltaTime);

            StunTime -= deltaTime;

            if (CurrentState == State.Boosting)
                BoostTime -= deltaTime;
            else if (BoostTime < GameData.BOOST_LENGTH)
                BoostTime += deltaTime * GameData.BOOST_LENGTH / GameData.BOOST_REGEN;

            //if (CurrentState == State.Climbing)
            //    Console.WriteLine("Climbing");

            if (BoostTime < 0)
            {
                TargetVelocity = GameData.RUN_VELOCITY;
                CurrentState = State.Walking;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(Position), null, Color, Rotation, Origin, ConvertUnits.ToDisplayUnits(textureScale), SpriteEffects.None, 0f);
            Sprite.Draw(spriteBatch);

            //Color = WallAbove ? Color.Green : Color.Red;

            Vector2 pos = new Vector2(Position.X - BAR_WIDTH / 2, Position.Y - Size.Y * 0.7f);
            Game1.DrawRectangle(spriteBatch, pos, Color.LightSalmon, new Vector2(BAR_WIDTH, BAR_HEIGHT));
            Game1.DrawRectangle(spriteBatch, pos, Color.Crimson, new Vector2(BAR_WIDTH * BoostTime / GameData.BOOST_LENGTH, BAR_HEIGHT));
        }

        public void Kill(Random rand)
        {
            TimeSinceDeath = GameData.DEAD_TIME;
            Projectiles.Clear();
            SpawnY = -rand.Next(GameData.MIN_SPAWN, GameData.MAX_SPAWN);
            BoostTime = GameData.BOOST_LENGTH;
        }

        //public void setRotation(Body body)
        //{
        //    if (body.Size.X > body.Size.Y)
        //        rotation = body.Rotation;
        //    else
        //        rotation = body.Rotation - MathHelper.PiOver2;
        //}

        //public void setRotation(float rot)
        //{
        //    rotation = rot;
        //}
    }
}
