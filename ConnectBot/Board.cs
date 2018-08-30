using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
            private Texture2D discSprite;
            private int discColor = 0;
            private bool falling = false;

            public int DiscColor
            {
                get
                {
                    return discColor;
                }
                set
                {
                    discColor = value;
                }
            }

            public bool Falling
            {
                get
                {
                    return falling;
                }
                set
                {
                    falling = value;
                }
            }

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
            public void Draw(SpriteBatch sb, Dictionary<string, Texture2D> images)
            {
                string imageName = (discColor == 1 ? "black_disc" : "red_disc");
                
                if (discColor != 0)
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
        }
        #endregion

        #region Class : BoardColumn
        /// <summary>
        /// Column to hold spaces and column animations.
        /// Handles click detection for moves in a column.
        /// </summary>
        public class BoardColumn
        {
            private Texture2D blueArrow;
            private Texture2D columnHolder;
            // TODO change this to an index
            private Space[] columnSpaces;

            /// <summary>
            /// Determines if the column can still be played in.
            /// </summary>
            private bool movable;

            // Rectangles for the blue arrow and column holder.
            private Rectangle columnHolderRect;
            private Rectangle blueArrowRect;

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
                blueArrowRect = new Rectangle(x, y - SpaceSize, SpaceSize, SpaceSize);
                columnHolderRect = new Rectangle(x, y, SpaceSize, SpaceSize * NumRows);

                blueArrow = ba;
                columnHolder = ch;

                // Initialize spaces in column.
                columnSpaces = new Space[6];

                int yPos = y + NumRows * SpaceSize;

                for (int row = 0; row < NumRows; row++)
                {
                    // Add space and move up column.
                    // Subtract first because x, y in constructor are the top left of rectangle TODO is this thie case?
                    // Move up to ensure that we appear inside column container.
                    yPos -= SpaceSize;
                    columnSpaces[row] = new Space(x, yPos);
                }

                movable = true;
            }

            /// <summary>
            /// Sets the spaces at rowIndex in the column to be disc.
            /// </summary>
            /// <param name="disc"></param>
            public void SetSpace(int disc)
            {
                // Fill the first available space with the given disc.
                for (int t = 0; t < NumRows; t++)
                {
                    if (columnSpaces[t].DiscColor == 0)
                    {
                        columnSpaces[t].DiscColor = disc;
                        //columnSpaces[t].Falling = true;

                        // Determine if column is full or still movable.
                        if (t == NumRows - 1)
                        {
                            movable = false;
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
            public int GetSpace(int row)
            {

                return columnSpaces[row].DiscColor;

            }

            /// <summary>
            /// Clears the spaces in the column.
            /// </summary>
            public void ResetSpaces()
            {
                for (int r = 0; r < NumRows; r++)
                {
                    columnSpaces[r].DiscColor = 0;
                }

                movable = true;
            }

            /// <summary>
            /// Determines if the column contians the given mouse point.
            /// </summary>
            /// <param name="p">Point representing mouse's location.</param>
            /// <returns>True if the column is clickable and contains mouse point.</returns>
            public bool ContainMouse(Point p)
            {
                
                return movable && (columnHolderRect.Contains(p) || blueArrowRect.Contains(p));
                
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

                sb.Draw(columnHolder, columnHolderRect, Color.White);
                if (movable && drawBlueArrow)
                {
                    sb.Draw(blueArrow, blueArrowRect, Color.White);
                }
            }
        }
        #endregion



        #region Constructor
        /// <summary>
        /// Constructor for board class.
        /// </summary>
        public Board()
        {
            // TODO what should / could be in here?
        }
        #endregion

    }
}
