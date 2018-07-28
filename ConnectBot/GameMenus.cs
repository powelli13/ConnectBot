using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ConnectBot
{
    class GameMenus
    {

        #region Class : PlayAgainMenu
        public class PlayAgainMenu
        {
            /// <summary>
            /// Spacing constants for drawing.
            /// </summary>
            int XBuffer = 120;
            int YBuffer = 120;

            int XQuestionBuffer = 10;
            int YQuestionBuffer = 10;

            int XYesButtonBuffer = 10;
            int YYesButtonBuffer = 90;

            int XNoButtonBuffer = 190;
            int YNoButtonBuffer = 90;

            Dictionary<string, Rectangle> drawingRectangles;
            


            #region Constructor
            public PlayAgainMenu()
            {
                // Create rectangles that will be used for drawinng
                drawingRectangles = new Dictionary<string, Rectangle>();

                drawingRectangles["play_again_background"] = new Rectangle(XBuffer, YBuffer, 450, 300);

                drawingRectangles["play_again_question"] = new Rectangle(
                    XBuffer + XQuestionBuffer,
                    YBuffer + YQuestionBuffer,
                    320, 80);

                drawingRectangles["yes_button"] = new Rectangle(
                    XBuffer + XYesButtonBuffer,
                    YBuffer + YYesButtonBuffer,
                    160, 80);

                drawingRectangles["no_button"] = new Rectangle(
                    XBuffer + XNoButtonBuffer,
                    YBuffer + YNoButtonBuffer,
                    160, 80);

            }
            #endregion

            /// <summary>
            /// Draws the play again menu to the screen.
            /// </summary>
            /// <param name="sb"></param>
            /// <param name="images"></param>
            public void Draw(SpriteBatch sb, Dictionary<string, Texture2D> images)
            {

                sb.Draw(images["play_again_background"], drawingRectangles["play_again_background"], Color.Wheat);
                sb.Draw(images["play_again_question"], drawingRectangles["play_again_question"], Color.Wheat);

                sb.Draw(images["yes_button"], drawingRectangles["yes_button"], Color.Wheat);
                sb.Draw(images["no_button"], drawingRectangles["no_button"], Color.Wheat);

            }

        }
        #endregion
    }
}
