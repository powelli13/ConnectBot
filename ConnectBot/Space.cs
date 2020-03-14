using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ConnectBot
{
    /// <summary>
    /// Class to contain essential game space information.
    /// </summary>
    public class Space
    {
        // Rectangle that represents the space
        private Rectangle rect;

        // Rectangle used to draw discs falling over time
        private Rectangle discDrawRect;

        //private Rectangle holderDrawRect;

        public DiscColor Disc { get; set; }

        /// <summary>
        /// Constructor requires the rect placement in pixels.
        /// </summary>
        public Space(int x, int y)
        {
            rect = new Rectangle(x, y, DrawingConstants.SpaceSize, DrawingConstants.SpaceSize);

            // Draw rectangle starting y is the top most disc space for a column
            // Top buffer of board to edge of screen add space size to account for blue arrows
            discDrawRect = new Rectangle(x,
                DrawingConstants.TopBuffer + DrawingConstants.SpaceSize,
                DrawingConstants.SpaceSize,
                DrawingConstants.SpaceSize);

        }

        /// <summary>
        /// Draws disc to the screen, only draws if it has a disc.
        /// </summary>
        /// <param name="sb"></param>
        /// // TODO passing in the entire dictionary seems very inefficient.
        /// // a disc should know it's color and Texture2D object for it's entire existance
        /// // this is because the disc is different than the space, consider redesigning 
        public void Draw(SpriteBatch sb, Dictionary<string, Texture2D> images)
        {
            string imageName = Disc == DiscColor.Black 
                ? ImageNames.BLACK_DISC 
                : ImageNames.RED_DISC;
                
            if (Disc != 0)
            {
                sb.Draw(images[imageName], discDrawRect, Color.Wheat);

                if (discDrawRect.Y < rect.Y)
                {
                    discDrawRect.Y += 8;
                }

                if (discDrawRect.Y >= rect.Y)
                {
                    discDrawRect.Y = rect.Y;
                }
            }
            else
            {
                sb.Draw(images[ImageNames.PLACE_HOLDER], rect, Color.Wheat);
            }
        }

        public void Reset()
        {
            Disc = DiscColor.None;
            discDrawRect.Y = DrawingConstants.TopBuffer + DrawingConstants.SpaceSize;
        }
    }
}
