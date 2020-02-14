using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ConnectBot
{
    // TODO should visual aspects of board be separated from logical?
    // I would like the logical parts of the board and AI functions to be unit testable
    // the easiest way to do this would be to put them in their own classes
    class Board
    {
        /// <summary>
        /// Spacing display constants.
        /// </summary>
        const int SpaceSize = 80;
        const int SideBuffer = 80;
        const int TopBuffer = 100;

        const int NumRows = 6;
        const int NumColumns = 7;

        #region Class : Space
        /// <summary>
        /// Class to contain essential game space information.
        /// </summary>
        public class Space
        {
            // Rectangle that represents the space
            private Rectangle rect;

            // Rectangle used to draw discs falling over time
            private Rectangle drawRect;

            public DiscColor Disc { get; set; }

            public bool Falling { get; set; }

            /// <summary>
            /// Constructor requires the rect placement in pixels.
            /// </summary>
            public Space(int x, int y)
            {
                rect = new Rectangle(x, y, SpaceSize, SpaceSize);

                // Draw rectangle starting y is the top most disc space for a column
                // Top buffer of board to edge of screen add space size to account for blue arrows
                drawRect = new Rectangle(x, TopBuffer + SpaceSize, SpaceSize, SpaceSize);
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
                    sb.Draw(images[imageName], drawRect, Color.Wheat);

                    if (drawRect.Y < rect.Y)
                    {
                        drawRect.Y += 8;
                    }

                    if (drawRect.Y >= rect.Y)
                    {
                        drawRect.Y = rect.Y;
                    }
                }
            }

            public void Reset()
            {
                Disc = 0;
                drawRect.Y = TopBuffer + SpaceSize;
            }
        }
        #endregion

        #region Class : BoardColumn
        /// <summary>
        /// Column to hold spaces and column animations.
        /// Handles click detection for moves in a column.
        /// </summary>
        public class BoardColumn
        {
            private Texture2D BlueArrow { get; set; }
            private Texture2D ColumnHolder { get; set; }
            // TODO change this to an index
            private Space[] columnSpaces;

            /// <summary>
            /// Determines if the column can still be played in.
            /// </summary>
            private bool Movable { get; set; }

            // Rectangles for the blue arrow and column holder.
            private Rectangle ColumnHolderRect { get; set; }
            private Rectangle BlueArrowRect { get; set; }

            /// <summary>
            /// Constructor for the column container and click handler.
            /// </summary>
            /// <param name="x">x position in pixels of rectangle</param>
            /// <param name="y">y position in pixels of rectangle</param>
            /// <param name="ch">sprite of column holder</param>
            /// <param name="ba">sprite of blue arrow</param>
            public BoardColumn(int x, int y, Texture2D ch, Texture2D ba)
            {
                // Create rectangles and save sprites.
                BlueArrowRect = new Rectangle(x, y - SpaceSize, SpaceSize, SpaceSize);
                ColumnHolderRect = new Rectangle(x, y, SpaceSize, SpaceSize * NumRows);

                BlueArrow = ba;
                ColumnHolder = ch;

                // Initialize spaces in column.
                columnSpaces = new Space[6];

                int yPos = y + NumRows * SpaceSize;

                for (int row = 0; row < NumRows; row++)
                {
                    // Add space and move up column.
                    // Subtract first because x, y in constructor are the top left of rectangle
                    // Move up to ensure that we appear inside column container.
                    yPos -= SpaceSize;
                    columnSpaces[row] = new Space(x, yPos);
                }

                Movable = true;
            }

            /// <summary>
            /// Sets the spaces at rowIndex in the column to be disc.
            /// </summary>
            /// <param name="disc"></param>
            public void SetSpace(DiscColor disc)
            {
                // Fill the first available space with the given disc.
                for (int t = 0; t < NumRows; t++)
                {
                    if (columnSpaces[t].Disc == 0)
                    {
                        columnSpaces[t].Disc = disc;

                        // Determine if column is full or still movable.
                        if (t == NumRows - 1)
                        {
                            Movable = false;
                        }

                        return;
                    }
                }
            }

            /// <summary>
            /// Retrieve the disc color at a given row.
            /// </summary>
            /// <param name="row"></param>
            /// <returns></returns>
            public DiscColor GetSpace(int row)
            {
                return columnSpaces[row].Disc;
            }

            /// <summary>
            /// Clears the spaces in the column.
            /// </summary>
            public void ResetSpaces()
            {
                for (int r = 0; r < NumRows; r++)
                {
                    columnSpaces[r].Reset();
                }

                Movable = true;
            }

            /// <summary>
            /// Determines if the column contians the given mouse point.
            /// </summary>
            /// <param name="p">Point representing mouse's location.</param>
            /// <returns>True if the column is clickable and contains mouse point.</returns>
            public bool ContainMouse(Point p)
            {
                return Movable && (ColumnHolderRect.Contains(p) || BlueArrowRect.Contains(p));   
            }

            /// <summary>
            /// Draws column holder, blue arrow if applicable
            /// and all contained discs to the screen.
            /// </summary>
            /// <param name="sb"></param>
            /// <param name="images"></param>
            public void Draw(SpriteBatch sb, Dictionary<string, Texture2D> images, bool drawBlueArrow)
            {
                for (int i = 0; i < columnSpaces.Length; i++)
                {
                    columnSpaces[i].Draw(sb, images);
                }

                sb.Draw(ColumnHolder, ColumnHolderRect, Color.White);
                if (Movable && drawBlueArrow)
                {
                    sb.Draw(BlueArrow, BlueArrowRect, Color.White);
                }
            }
        }
        #endregion
    }
}
