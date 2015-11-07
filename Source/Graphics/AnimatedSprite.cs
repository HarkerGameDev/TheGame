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
        public int Rows;
        public int Columns;

        private int currentFrame;
        private int totalFrames;
        private float frameLength;
        private float scale;

        public AnimatedSprite(Texture2D texture, int columns, int rows, float scale = 1f)
        {
            Texture = texture;
            Rows = rows;
            Columns = columns;
            this.scale = scale;

            currentFrame = 0;
            totalFrames = Rows * Columns;
            frameLength = FRAME_TIME;
        }

        public void Update(float deltaTime)
        {
            frameLength -= deltaTime;
            if (frameLength < 0)
            {
                currentFrame = ++currentFrame % totalFrames;
                frameLength = FRAME_TIME;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Player player)
        {
            int width = Texture.Width / Columns;
            int height = Texture.Height / Rows;
            int row = currentFrame / Columns;
            int column = currentFrame % Columns;

            Rectangle sourceRectangle = new Rectangle(width * column, height * row, width, height);
            Vector2 size = new Vector2(width, height) * scale;
            Rectangle destinationRectangle = new Rectangle(ConvertUnits.ToDisplayUnits(player.Position).ToPoint(), ConvertUnits.ToDisplayUnits(size).ToPoint());

            //spriteBatch.Draw(Texture, destinationRectangle, sourceRectangle, Color.White);
            //spriteBatch.Draw(texture, ConvertUnits.ToDisplayUnits(Position), null, Color, Rotation, Origin, ConvertUnits.ToDisplayUnits(textureScale), SpriteEffects.None, 0f);
            spriteBatch.Draw(Texture, destinationRectangle, sourceRectangle, player.Color, player.Rotation, player.Origin, SpriteEffects.None, 0f);
        }
    }
}
