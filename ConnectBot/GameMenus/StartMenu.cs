using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static ConnectBot.DrawingConstants;

namespace ConnectBot.GameMenus
{
    class StartMenu
    {
        // TODO these menus are completely hot garbage
        // it'd be cool to have them dynamically move around and dynamically
        // build relative to the starting menu position
        // although I'm unsure how much more effort I want to put into
        // the visual side of the monogame version
        SpriteFont _font { get; }
        Dictionary<string, Rectangle> DrawingRectangles { get; }



        public StartMenu(SpriteFont font)
        {
            _font = font ?? throw new ArgumentNullException(nameof(font));

            DrawingRectangles = new Dictionary<string, Rectangle>();

            DrawingRectangles[ImageNames.BLACK_DISC] = new Rectangle(
                TOP_BUFFER,
                TOP_BUFFER,
                SPACE_SIZE,
                SPACE_SIZE);

            DrawingRectangles[ImageNames.RED_DISC] = new Rectangle(
                TOP_BUFFER + SPACE_SIZE,
                TOP_BUFFER,
                SPACE_SIZE,
                SPACE_SIZE);
        }

        public void Draw(SpriteBatch sb, Dictionary<string, Texture2D> images)
        {
            sb.DrawString(
                _font, 
                "Which color would you like to play as?", 
                new Vector2(10, 10), 
                Color.Black);

            sb.Draw(
                images[ImageNames.BLACK_DISC], 
                DrawingRectangles[ImageNames.BLACK_DISC], 
                BACKGROUND_COLOR);

            sb.Draw(
                images[ImageNames.RED_DISC],
                DrawingRectangles[ImageNames.RED_DISC],
                BACKGROUND_COLOR);
        }

        public bool BlackDiscContainsMouse(Point p)
            => DrawingRectangles[ImageNames.BLACK_DISC].Contains(p);

        public bool RedDiscContainsMouse(Point p)
            => DrawingRectangles[ImageNames.RED_DISC].Contains(p);
    }
}
