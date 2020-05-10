using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ConnectBot
{
    
    /// <summary>
    /// Column to hold spaces and column animations.
    /// Handles click detection for moves in a column.
    /// </summary>
    public class BoardColumn
    {
        private Texture2D BlueArrow { get; set; }
        private Texture2D ColumnHolder { get; set; }
        private Texture2D HighlightedColumnHolder { get; set; }

        private Space[] columnSpaces;

        private bool IsMovable { get; set; }

        public bool IsFocused { get; set; }

        // Rectangles for the blue arrow and column holder.
        private Rectangle ColumnHolderRect { get; set; }
        private Rectangle BlueArrowRect { get; set; }

        /// <summary>
        /// Constructor for the column container and click handler.
        /// </summary>
        /// <param name="x">x position in pixels of rectangle</param>
        /// <param name="y">y position in pixels of rectangle</param>
        /// <param name="columnHolder">sprite of column holder</param>
        /// <param name="blueArrow">sprite of blue arrow</param>
        public BoardColumn(int x, int y, Texture2D columnHolder, Texture2D highlightedColumnHolder, Texture2D blueArrow)
        {
            ColumnHolderRect = new Rectangle(x, y, 
                DrawingConstants.SPACE_SIZE, 
                DrawingConstants.SPACE_SIZE * LogicalBoardHelpers.NUM_ROWS);

            BlueArrowRect = new Rectangle(x, 
                y - DrawingConstants.SPACE_SIZE,
                DrawingConstants.SPACE_SIZE,
                DrawingConstants.SPACE_SIZE);

            ColumnHolder = columnHolder;
            HighlightedColumnHolder = highlightedColumnHolder;
            BlueArrow = blueArrow;

            // Initialize spaces in column.
            columnSpaces = new Space[6];

            int yPos = y + LogicalBoardHelpers.NUM_ROWS * DrawingConstants.SPACE_SIZE;

            for (int row = 0; row < LogicalBoardHelpers.NUM_ROWS; row++)
            {
                // Add space and move up column.
                // Subtract first because x, y in constructor are the top left of rectangle
                // Move up to ensure that we appear inside column container.
                yPos -= DrawingConstants.SPACE_SIZE;
                columnSpaces[row] = new Space(x, yPos);
            }

            IsMovable = true;
        }

        /// <summary>
        /// Sets the spaces at rowIndex in the column to be disc.
        /// </summary>
        /// <param name="disc"></param>
        public void SetSpace(DiscColor disc)
        {
            // Fill the first available space with the given disc.
            for (int t = 0; t < LogicalBoardHelpers.NUM_ROWS; t++)
            {
                if (columnSpaces[t].Disc == 0)
                {
                    columnSpaces[t].Disc = disc;

                    // Determine if column is full or still movable.
                    if (t == LogicalBoardHelpers.NUM_ROWS - 1)
                    {
                        IsMovable = false;
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
            for (int r = 0; r < LogicalBoardHelpers.NUM_ROWS; r++)
            {
                columnSpaces[r].Reset();
            }

            IsMovable = true;
        }

        /// <summary>
        /// Determines if the column contians the given mouse point.
        /// </summary>
        /// <param name="p">Point representing mouse's location.</param>
        /// <returns>True if the column is clickable and contains mouse point.</returns>
        public bool ContainMouse(Point p)
        {
            return IsMovable && (ColumnHolderRect.Contains(p) || BlueArrowRect.Contains(p));   
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

            if (IsFocused)
            {
                sb.Draw(HighlightedColumnHolder, ColumnHolderRect, Color.White);
            }
            else
            {
                sb.Draw(ColumnHolder, ColumnHolderRect, Color.White);
            }

            if (IsMovable && drawBlueArrow)
            {
                sb.Draw(BlueArrow, BlueArrowRect, Color.White);
            }
        }

        /// <summary>
        /// Returns the current column's state represented 
        /// in the first six positions of a BitBoard. These bits
        /// should be shifted according to the columns position
        /// and then OR'ed to get the board position.
        /// </summary>
        // TODO unit tests around this
        public BitBoard GetBitColumn()
        {
            ulong redDiscs = 0;
            ulong blackDiscs = 0;

            for (int row = 0; row < LogicalBoardHelpers.NUM_ROWS; row++)
            {
                if (GetSpace(row) == DiscColor.Red)
                {
                    redDiscs = BitBoardHelpers.SetSingleBit(redDiscs, row);
                }
                else if (GetSpace(row) == DiscColor.Black)
                {
                    blackDiscs = BitBoardHelpers.SetSingleBit(blackDiscs, row);
                }
                else
                {
                    break;
                }
            }

            return new BitBoard(redDiscs, blackDiscs);
        }
    }
}
