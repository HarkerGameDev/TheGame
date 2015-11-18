using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Source.Graphics
{
    class Button
    {
        private Texture2D texture;
        private Rectangle drawRect;
        private Color color;

        public Action OnClick;

        private SpriteFont font;
        private string text;
        private Vector2 textDraw;
        private Color textColor;

        public Button(Texture2D texture, Vector2 topLeft, Vector2 scale, Action onClick, Color color)
        {
            this.texture = texture;
            drawRect = new Rectangle(topLeft.ToPoint(), scale.ToPoint());
            this.OnClick = onClick;
            this.color = color;
        }

        public Button(Texture2D texture, Vector2 topLeft, Vector2 scale, Action onClick, Color color, SpriteFont font, string text, Color textColor)
        {
            this.texture = texture;
            drawRect = new Rectangle(topLeft.ToPoint(), scale.ToPoint());
            this.OnClick = onClick;
            this.color = color;

            this.font = font;
            this.text = text;
            textDraw = topLeft + scale / 2f - font.MeasureString(text) / 2f;
            this.textColor = textColor;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, drawRect, Color.White);
            if (font != null)
                spriteBatch.DrawString(font, text, textDraw, textColor);
        }

        public bool TestPoint(Point point)
        {
            return drawRect.Contains(point);
        }
    }
}
