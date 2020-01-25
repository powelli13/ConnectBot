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

                drawingRectangles[ImageNames.PLAY_AGAIN_BACKGROUND] = new Rectangle(XBuffer, YBuffer, 450, 300);

                drawingRectangles[ImageNames.PLAY_AGAIN_QUESTION] = new Rectangle(
                    XBuffer + XQuestionBuffer,
                    YBuffer + YQuestionBuffer,
                    320, 80
                );

                drawingRectangles[ImageNames.YES_BUTTON] = new Rectangle(
                    XBuffer + XYesButtonBuffer,
                    YBuffer + YYesButtonBuffer,
                    160, 80
                );

                drawingRectangles[ImageNames.NO_BUTTON] = new Rectangle(
                    XBuffer + XNoButtonBuffer,
                    YBuffer + YNoButtonBuffer,
                    160, 80
                );

            }
            #endregion

            /// <summary>
            /// Draws the play again menu to the screen.
            /// </summary>
            /// <param name="sb"></param>
            /// <param name="images"></param>
            public void Draw(SpriteBatch sb, Dictionary<string, Texture2D> images)
            {

                sb.Draw(
                    images[ImageNames.PLAY_AGAIN_BACKGROUND], 
                    drawingRectangles[ImageNames.PLAY_AGAIN_BACKGROUND], Color.Wheat);
                sb.Draw(
                    images[ImageNames.PLAY_AGAIN_QUESTION], 
                    drawingRectangles[ImageNames.PLAY_AGAIN_QUESTION], Color.Wheat);

                sb.Draw(
                    images[ImageNames.YES_BUTTON], 
                    drawingRectangles[ImageNames.YES_BUTTON], Color.Wheat);
                sb.Draw(
                    images[ImageNames.NO_BUTTON], 
                    drawingRectangles[ImageNames.NO_BUTTON], Color.Wheat);

            }

            /// <summary>
            /// Determine if the yes button contains the given mouse position.
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            public bool YesButtonContainsMouse(Point p)
            {

                return drawingRectangles[ImageNames.YES_BUTTON].Contains(p);
                
            }

            /// <summary>
            /// Determine if the no button contains the given mouse position.
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            public bool NoButtonContainsMouse(Point p)
            {

                return drawingRectangles[ImageNames.NO_BUTTON].Contains(p);

            }

        }
        #endregion
    }
}
