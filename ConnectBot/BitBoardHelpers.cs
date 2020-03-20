using System;
using System.Collections.Generic;

namespace ConnectBot
{
    /// <summary>
    /// Collection of helper methods used to manipulate ulong
    /// that hold bit representations of the Connect 4 board.
    /// Columns are laid out consecutively.
    /// </summary>
    /*
    * 5  11  17  23  29  35  41
    * 4  10  16  22  28  34  40
    * 3   9  15  21  27  33  39
    * 2   8  14  20  26  32  38
    * 1   7  13  19  25  31  37
    * 0   6  12  18  24  30  36 
    */
    public static class BitBoardHelpers
    {
        public static int ColumnOneTop = 5;
        public static int ColumnTwoTop = 11;
        public static int ColumnThreeTop = 17;  
        public static int ColumnFourTop = 23;  
        public static int ColumnFiveTop = 29;  
        public static int ColumnSixTop = 35;  
        public static int ColumnSevenTop = 41;

        public static bool CheckSingleBit(ulong board, int index)
            => (board & (1ul << index)) != 0;

        public static ulong SetSingleBit(ulong board, int index)
            => (board | (1ul << index));

        /// <summary>
        /// Returns true if the given column on the board is available
        /// for movement, otherwise false.
        /// </summary>
        /// <param name="board">BitBoard representing current state</param>
        /// <param name="column">Index of the column to check, starts at zero</param>
        public static bool IsColumnOpen(BitBoard board, int column)
        {
            int checkIndex = 5 + (column * 6);

            return !CheckSingleBit(board.FullBoard, checkIndex);
        }

        public static BitBoard GetNewBoard()
            => new BitBoard(0, 0);

        // find highest open spot in a column
        // move into an open column given index and color
        public static BitBoard Move(BitBoard board, int column, DiscColor disc)
        {
            if (!IsColumnOpen(board, column))
                throw new InvalidOperationException($"Column {column} is unavailable for movement.");

            // TODO find or think of a more clever way to do with with some bit masks
            int openBitIndex = 6 * column;

            for (int r = 0; r < LogicalBoardHelpers.NUM_ROWS; r++)
            {
                if (!CheckSingleBit(board.FullBoard, openBitIndex))
                    break;

                openBitIndex++;    
            }

            if (disc == DiscColor.Red)
            {
                return new BitBoard(SetSingleBit(board.RedDiscs, openBitIndex), board.BlackDiscs);
            }

            return new BitBoard(board.RedDiscs, SetSingleBit(board.BlackDiscs, openBitIndex));
        }


        // retrieve open columns
        public static List<int> GetOpenColumns(BitBoard board)
        {
            var openColumns = new List<int>();

            for (int c = 0; c < LogicalBoardHelpers.NUM_COLUMNS; c++)
            {
                if (IsColumnOpen(board, c))
                    openColumns.Add(c);
            }

            return openColumns;
        }

        // copy a board
        // populate an entire column at once, used when given bot a bit board



        // check horizontal four
        // check vertical four
        // check rising diagonal four
        // check falling diagonal four
        // five disc color given row column, take into account openness
    }
}
