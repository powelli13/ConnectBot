using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectBot
{
    /// <summary>
    /// Collection of helper methods used to manipulate ulong
    /// that hold bit representations of the Connect 4 board.
    /// Columns are laid out consecutively with a bit at the 
    /// end to designate whether the column is movable 1 or not 0.
    /// If a column is open a 1 is used to designate the lowest 
    /// most open space, everything above that 1 should be zeroes
    /// and anything below should represent the discs in the column.
    /// In a closed column, or in an open column below the top 1:
    /// 1 represents black and 0 represents red
    /// </summary>
    public static class BitBoard
    {
        /*
         * Below is a map of indices on the ulong bits with where 
         * on the board they represent. Tops of columns are flags
         * indicating if the columns is open 1 or closed 0.
         * 
         * 6  13  20  27  34  41  48
         * 5  12  19  26  33  40  47
         * 4  11  18  25  32  39  46
         * 3  10  17  24  31  38  45
         * 2   9  16  23  30  37  44
         * 1   8  15  22  29  36  43
         * 0   7  14  21  28  35  42
         * 
         * To move over a column add 7.
         */

        static int ColumnOneOpenFlagIndex = 6;
        static int ColumnTwoOpenFlagIndex = 13;
        static int ColumnThreeOpenFlagIndex = 20;
        static int ColumnFourOpenFlagIndex = 27;
        static int ColumnFiveOpenFlagIndex = 34;
        static int ColumnSixOpenFlagIndex = 41;
        static int ColumnSevenOpenFlagIndex = 48;
        
        // TODO this may never be used because the board will come from the
        // ConnectGame's columns
        public static ulong GetNewBoard()
        {
            ulong board = 0;

            // Indicate that all columns are open
            board = SetSingleBit(board, ColumnOneOpenFlagIndex);
            board = SetSingleBit(board, ColumnTwoOpenFlagIndex);
            board = SetSingleBit(board, ColumnThreeOpenFlagIndex);
            board = SetSingleBit(board, ColumnFourOpenFlagIndex);
            board = SetSingleBit(board, ColumnFiveOpenFlagIndex);
            board = SetSingleBit(board, ColumnSixOpenFlagIndex);
            board = SetSingleBit(board, ColumnSevenOpenFlagIndex);

            // Set the bottom spaces for each column as open
            board = SetSingleBit(board, 0);
            board = SetSingleBit(board, 7);
            board = SetSingleBit(board, 14);
            board = SetSingleBit(board, 21);
            board = SetSingleBit(board, 28);
            board = SetSingleBit(board, 35);
            board = SetSingleBit(board, 42);

            return board;
        }

        public static bool CheckSingleBit(ulong board, int index)
            => (board & (1ul << index)) != 0;

        public static ulong SetSingleBit(ulong board, int index)
            => (board | (1ul << index));

        // Checks for whether or not individual columns are open
        public static bool IsColumnOneOpen(ulong board)
            => CheckSingleBit(board, ColumnOneOpenFlagIndex);

        public static bool IsColumnTwoOpen(ulong board)
            => CheckSingleBit(board, ColumnTwoOpenFlagIndex);

        public static bool IsColumnThreeOpen(ulong board)
            => CheckSingleBit(board, ColumnThreeOpenFlagIndex);

        public static bool IsColumnFourOpen(ulong board)
            => CheckSingleBit(board, ColumnFourOpenFlagIndex);

        public static bool IsColumnFiveOpen(ulong board)
            => CheckSingleBit(board, ColumnFiveOpenFlagIndex);

        public static bool IsColumnSixOpen(ulong board)
            => CheckSingleBit(board, ColumnSixOpenFlagIndex);

        public static bool IsColumnSevenOpen(ulong board)
            => CheckSingleBit(board, ColumnSevenOpenFlagIndex);


        // find highest open spot in a column
        // move into an open column given index and color


        // retrieve open columns
        // copy a board
        // populate an entire column at once, used when given bot a bit board

        // check horizontal four
        // check vertical four
        // check rising diagonal four
        // check falling diagonal four
        // five disc color given row column, take into account openness
    }
}
