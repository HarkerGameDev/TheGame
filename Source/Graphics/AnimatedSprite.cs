using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Source.Collisions;

namespace Source.Graphics
{
    public class AnimatedSprite
    {
        private const float FRAME_TIME = 0.2f;

        public Texture2D Texture;
        public Vector2 Size;

        private int currentFrame;
        private int[] totalFrames;
        private int currentState;
        private double frameLength;
        private float scale;
        private Player player;

        public AnimatedSprite(Texture2D texture, Player player, int[] frames, float scale = 1f)
        {
            Texture = texture;
            this.scale = scale;
            totalFrames = frames;
            this.player = player;

            currentFrame = 0;
            frameLength = FRAME_TIME;
            currentState = 0;
            Size = new Vector2(Texture.Width / frames.Max(), Texture.Height / frames.Length);
        }

        public void Update(double deltaTime)
        {
            if (currentState != (int)player.CurrentState)
            {
                currentState = (int)player.CurrentState;
                frameLength = FRAME_TIME;
                currentFrame = 0;
            }
            else
            {
                frameLength -= deltaTime;
                if (frameLength < 0)
                {
                    currentFrame = ++currentFrame % totalFrames[currentState];
                    frameLength = FRAME_TIME;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int width = (int)Size.X;
            int height = (int)Size.Y;
            int row = currentState;
            int column = currentFrame;

            Rectangle sourceRectangle = new Rectangle(width * column, height * row, width, height);
            Vector2 displaySize = new Vector2(width, height) * scale;
            Rectangle destinationRectangle = new Rectangle(ConvertUnits.ToDisplayUnits(player.Position).ToPoint(), ConvertUnits.ToDisplayUnits(displaySize).ToPoint());

            spriteBatch.Draw(Texture, destinationRectangle, sourceRectangle, player.Color, player.Rotation, player.Origin, player.Flip, 0f);
        }
    }
}
