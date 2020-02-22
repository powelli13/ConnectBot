using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ConnectBot
{
    class GameMenus
    {
        public class PlayAgainMenu
        {
            /// <summary>
            /// Spacing constants for drawing.
            /// </summary>
            readonly int XBuffer = 560;
            readonly int YBuffer = 12;

            readonly int XQuestionBuffer = 10;
            readonly int YQuestionBuffer = 10;
            
            readonly int XYesButtonBuffer = 10;
            readonly int YYesButtonBuffer = 90;
            
            readonly int XNoButtonBuffer = 190;
            readonly int YNoButtonBuffer = 90;

            Dictionary<string, Rectangle> DrawingRectangles;

            public PlayAgainMenu()
            {
                // Create rectangles that will be used for drawinng
                DrawingRectangles = new Dictionary<string, Rectangle>();

                DrawingRectangles[ImageNames.PLAY_AGAIN_BACKGROUND] = new Rectangle(XBuffer, YBuffer, 320, 160);
                DrawingRectangles[ImageNames.PLAY_AGAIN_DRAWN_QUESTION] = new Rectangle(XBuffer, YBuffer, 320, 160);

                DrawingRectangles[ImageNames.PLAY_AGAIN_QUESTION] = new Rectangle(
                    XBuffer + XQuestionBuffer,
                    YBuffer + YQuestionBuffer,
                    320, 80
                );

                DrawingRectangles[ImageNames.YES_BUTTON] = new Rectangle(
                    XBuffer + XYesButtonBuffer,
                    YBuffer + YYesButtonBuffer,
                    120, 70
                );

                DrawingRectangles[ImageNames.NO_BUTTON] = new Rectangle(
                    XBuffer + XNoButtonBuffer,
                    YBuffer + YNoButtonBuffer,
                    120, 70
                );

            }

            /// <summary>
            /// Draws the play again menu to the screen.
            /// </summary>
            public void Draw(SpriteBatch sb, Dictionary<string, Texture2D> images, bool gameDrawn = false)
            {
                sb.Draw(
                    images[ImageNames.PLAY_AGAIN_BACKGROUND], 
                    DrawingRectangles[ImageNames.PLAY_AGAIN_BACKGROUND], Color.Wheat);

                var questionImageName = gameDrawn
                    ? ImageNames.PLAY_AGAIN_DRAWN_QUESTION
                    : ImageNames.PLAY_AGAIN_QUESTION;
                sb.Draw(
                    images[questionImageName],
                    DrawingRectangles[questionImageName], Color.Wheat);
                
                sb.Draw(
                    images[ImageNames.YES_BUTTON], 
                    DrawingRectangles[ImageNames.YES_BUTTON], Color.Wheat);
                sb.Draw(
                    images[ImageNames.NO_BUTTON], 
                    DrawingRectangles[ImageNames.NO_BUTTON], Color.Wheat);
            }

            /// <summary>
            /// Determine if the yes button contains the given mouse position.
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            public bool YesButtonContainsMouse(Point p)
            {

                return DrawingRectangles[ImageNames.YES_BUTTON].Contains(p);
                
            }

            /// <summary>
            /// Determine if the no button contains the given mouse position.
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            public bool NoButtonContainsMouse(Point p)
            {

                return DrawingRectangles[ImageNames.NO_BUTTON].Contains(p);

            }

        }
    }
}
