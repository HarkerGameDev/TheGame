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
        //public bool CanJump = false;
        //public bool Ghost = false;
        //public Body Ignore = null;
        //public float oldY = 0;
        //public int CollideBottom = 0;

        public State CurrentState = State.Jumping;
        public double TimeSinceDeath = 0;
        public int Score = 0;
        public List<Projectile> Projectiles;

        public bool InAir { get { return CurrentState == State.Jumping || CurrentState == State.Slamming; } }
        public bool CanJump { get { return CurrentState == State.Walking || CurrentState == State.Boosting; } }

        public enum State
        {
            Jumping=0, Walking=1, Slamming=2, Sliding=3, Boosting=4
        }

        public AnimatedSprite Sprite;

        public Player(Texture2D texture, Vector2 position, Color color)
            : base(texture, position, new Vector2(0.6f, 1.8f))
        {
            this.Color = color;

            Projectiles = new List<Projectile>();
            Velocity.X = GameData.RUN_VELOCITY;

            int[] animationFrames = { 4, 4, 2, 2, 4 };
            Origin = new Vector2(Origin.X / animationFrames.Max(), Origin.Y / animationFrames.Length);
            float textureScale = 1.8f / Origin.Y / 2f * (20f / 18f);
            Sprite = new AnimatedSprite(texture, this, animationFrames, textureScale);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(Position), null, Color, Rotation, Origin, ConvertUnits.ToDisplayUnits(textureScale), SpriteEffects.None, 0f);
            Sprite.Draw(spriteBatch);
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
