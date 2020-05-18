using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static ConnectBot.DrawingConstants;

namespace ConnectBot.GameMenus
{
    class PlayAgainMenu
    {
        readonly int BUTTON_WIDTH = 60;
        readonly int BUTTON_HEIGHT = 35;

        readonly SpriteFont _font;
        readonly Rectangle _yesRectange;
        readonly Rectangle _noRectange;

        readonly Vector2 _yesPosition;
        readonly Vector2 _noPosition;

        readonly Texture2D _buttonBackgroundTexture;

        public PlayAgainMenu(SpriteFont font, GraphicsDevice graphicsDevice)
        {
            _font = font;

            // Create rectangles that will be used for detecting clicks
            _yesRectange = new Rectangle(
                TOP_BUFFER,
                60,
                BUTTON_WIDTH,
                BUTTON_HEIGHT
            );

            _noRectange = new Rectangle(
                TOP_BUFFER + BUTTON_WIDTH + 10,
                60,
                BUTTON_WIDTH,
                BUTTON_HEIGHT
            );

            var data = new Color[_yesRectange.Width * _yesRectange.Height];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.White;
            }

            _buttonBackgroundTexture = new Texture2D(
                graphicsDevice,
                _yesRectange.Width,
                _yesRectange.Height);
            _buttonBackgroundTexture.SetData(data);

            _yesPosition = new Vector2(_yesRectange.Left, _yesRectange.Top);
            _noPosition = new Vector2(_noRectange.Left, _noRectange.Top);
        }

        /// <summary>
        /// Draws the play again menu to the screen.
        /// </summary>
        public void Draw(
            SpriteBatch sb, 
            Dictionary<string, Texture2D> images,
            DiscColor winner,
            bool gameDrawn = false)
        {
            var menuText = gameDrawn
                ? "Game drawn! Play again?"
                : $"{winner} has won! Play again?";

            sb.DrawString(
                _font,
                menuText,
                new Vector2(10, 10),
                Color.Black);

            sb.Draw(
                _buttonBackgroundTexture, 
                _yesPosition, 
                Color.White);

            sb.DrawString(
                _font,
                "Yes",
                new Vector2(
                    TOP_BUFFER,
                    60),
                Color.Black);

            sb.Draw(
                _buttonBackgroundTexture, 
                _noPosition, 
                Color.White);

            sb.DrawString(
                _font,
                "No",
                new Vector2(
                    TOP_BUFFER + BUTTON_WIDTH + 10,
                    60),
                Color.Black);
        }

        public bool YesButtonContainsMouse(Point p)
            => _yesRectange.Contains(p);

        public bool NoButtonContainsMouse(Point p)
            => _noRectange.Contains(p);
    }
}
