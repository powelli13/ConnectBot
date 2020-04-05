using System;
using System.Collections.Generic;
using System.Text;
using static ConnectBot.LogicalBoardHelpers;

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
        public readonly static int ColumnOneTop = 5;
        public readonly static int ColumnTwoTop = 11;
        public readonly static int ColumnThreeTop = 17;  
        public readonly static int ColumnFourTop = 23;  
        public readonly static int ColumnFiveTop = 29;  
        public readonly static int ColumnSixTop = 35;  
        public readonly static int ColumnSevenTop = 41;

        /* 
         * Below are precalculated ulongs used to quickly
         * check scoring fours on the bit board.
         */
        // TODO should these be | instead of +? would it make a performance difference?
        public static readonly ulong[][] RowHorizontals = new ulong[][]
        {
            new ulong[4]
            {
                1ul + (1ul << 6) + (1ul << 12) + (1ul << 18),
                (1ul << 6) + (1ul << 12) + (1ul << 18) + (1ul << 24),
                (1ul << 12) + (1ul << 18) + (1ul << 24) + (1ul << 30),
                (1ul << 18) + (1ul << 24) + (1ul << 30) + (1ul << 36)
            },
            new ulong[4]
            {
                (1ul << 1) + (1ul << 7) + (1ul << 13) + (1ul << 19),
                (1ul << 7) + (1ul << 13) + (1ul << 19) + (1ul << 25),
                (1ul << 13) + (1ul << 19) + (1ul << 25) + (1ul << 31),
                (1ul << 19) + (1ul << 25) + (1ul << 31) + (1ul << 37)
            },
            new ulong[4]
            {
                (1ul << 2) + (1ul << 8) + (1ul << 14) + (1ul << 20),
                (1ul << 8) + (1ul << 14) + (1ul << 20) + (1ul << 26),
                (1ul << 14) + (1ul << 20) + (1ul << 26) + (1ul << 32),
                (1ul << 20) + (1ul << 26) + (1ul << 32) + (1ul << 38)
            },
            new ulong[4]
            {
                (1ul << 3) + (1ul << 9) + (1ul << 15) + (1ul << 21),
                (1ul << 9) + (1ul << 15) + (1ul << 21) + (1ul << 27),
                (1ul << 15) + (1ul << 21) + (1ul << 27) + (1ul << 33),
                (1ul << 21) + (1ul << 27) + (1ul << 33) + (1ul << 39)
            },
            new ulong[4]
            {
                (1ul << 4) + (1ul << 10) + (1ul << 16) + (1ul << 22),
                (1ul << 10) + (1ul << 16) + (1ul << 22) + (1ul << 28),
                (1ul << 16) + (1ul << 22) + (1ul << 28) + (1ul << 34),
                (1ul << 22) + (1ul << 28) + (1ul << 34) + (1ul << 40)
            },
            new ulong[4]
            {
                (1ul << 5) + (1ul << 11) + (1ul << 17) + (1ul << 23),
                (1ul << 11) + (1ul << 17) + (1ul << 23) + (1ul << 29),
                (1ul << 17) + (1ul << 23) + (1ul << 29) + (1ul << 35),
                (1ul << 23) + (1ul << 29) + (1ul << 35) + (1ul << 41)
            }
        };

        public static readonly ulong[][] ColumnVerticals = new ulong[][]
        { 
            new ulong []
            { 
                1ul + (1ul << 1) + (1ul << 2) + (1ul << 3),
                (1ul << 1) + (1ul << 2) + (1ul << 3) + (1ul << 4),
                (1ul << 2) + (1ul << 3) + (1ul << 4) + (1ul << 5)
            },
            new ulong []
            {
                (1ul << 6) + (1ul << 7) + (1ul << 8) + (1ul << 9),
                (1ul << 7) + (1ul << 8) + (1ul << 9) + (1ul << 10),
                (1ul << 8) + (1ul << 9) + (1ul << 10) + (1ul << 11)
            },
            new ulong []
            {
                (1ul << 12) + (1ul << 13) + (1ul << 14) + (1ul << 15),
                (1ul << 13) + (1ul << 14) + (1ul << 15) + (1ul << 16),
                (1ul << 14) + (1ul << 15) + (1ul << 16) + (1ul << 17)
            },
            new ulong []
            {
                (1ul << 18) + (1ul << 19) + (1ul << 20) + (1ul << 21),
                (1ul << 19) + (1ul << 20) + (1ul << 21) + (1ul << 22),
                (1ul << 20) + (1ul << 21) + (1ul << 22) + (1ul << 23)
            },
            new ulong []
            {
                (1ul << 24) + (1ul << 25) + (1ul << 26) + (1ul << 27),
                (1ul << 25) + (1ul << 26) + (1ul << 27) + (1ul << 28),
                (1ul << 26) + (1ul << 27) + (1ul << 28) + (1ul << 29)
            },
            new ulong []
            {
                (1ul << 30) + (1ul << 31) + (1ul << 32) + (1ul << 33),
                (1ul << 31) + (1ul << 32) + (1ul << 33) + (1ul << 34),
                (1ul << 32) + (1ul << 33) + (1ul << 34) + (1ul << 35)
            },
            new ulong []
            {
                (1ul << 36) + (1ul << 37) + (1ul << 38) + (1ul << 39),
                (1ul << 37) + (1ul << 38) + (1ul << 39) + (1ul << 40),
                (1ul << 38) + (1ul << 39) + (1ul << 40) + (1ul << 41)
            },
        };

        public static readonly ulong[][] FallingDiagonals = new ulong[][]
        {
            new ulong[]
            { 
                (1ul << 3) + (1ul << 8) + (1ul << 13) + (1ul << 18),
                (1ul << 4) + (1ul << 9) + (1ul << 14) + (1ul << 19),
                (1ul << 5) + (1ul << 10) + (1ul << 15) + (1ul << 20)
            },
            new ulong[]
            {
                (1ul << 9) + (1ul << 14) + (1ul << 19) + (1ul << 24),
                (1ul << 10) + (1ul << 15) + (1ul << 20) + (1ul << 25),
                (1ul << 11) + (1ul << 16) + (1ul << 21) + (1ul << 26)
            },
            new ulong[]
            {
                (1ul << 15) + (1ul << 20) + (1ul << 25) + (1ul << 30),
                (1ul << 16) + (1ul << 21) + (1ul << 26) + (1ul << 31),
                (1ul << 17) + (1ul << 22) + (1ul << 27) + (1ul << 32)
            },
            new ulong[]
            {
                (1ul << 21) + (1ul << 26) + (1ul << 31) + (1ul << 36),
                (1ul << 22) + (1ul << 27) + (1ul << 32) + (1ul << 37),
                (1ul << 23) + (1ul << 28) + (1ul << 33) + (1ul << 38)
            }
        };

        public static readonly ulong[][] RisingDiagonals = new ulong[][]
        {
            new ulong[]
            {
                1ul + (1ul << 7) + (1ul << 14) + (1ul << 21),
                (1ul << 1) + (1ul << 8) + (1ul << 15) + (1ul << 22),
                (1ul << 2) + (1ul << 9) + (1ul << 16) + (1ul << 23)
            },
            new ulong[]
            {
                (1ul << 6) + (1ul << 13) + (1ul << 20) + (1ul << 27),
                (1ul << 7) + (1ul << 14) + (1ul << 21) + (1ul << 28),
                (1ul << 8) + (1ul << 15) + (1ul << 22) + (1ul << 29)
            },
            new ulong[]
            {
                (1ul << 12) + (1ul << 19) + (1ul << 26) + (1ul << 33),
                (1ul << 13) + (1ul << 20) + (1ul << 27) + (1ul << 34),
                (1ul << 14) + (1ul << 21) + (1ul << 28) + (1ul << 35)
            },
            new ulong[]
            {
                (1ul << 18) + (1ul << 25) + (1ul << 32) + (1ul << 39),
                (1ul << 19) + (1ul << 26) + (1ul << 33) + (1ul << 40),
                (1ul << 20) + (1ul << 27) + (1ul << 34) + (1ul << 41)
            },
        };

        public static bool CheckSingleBit(in ulong board, int index)
            => (board & (1ul << index)) != 0;

        public static ulong SetSingleBit(in ulong board, int index)
            => (board | (1ul << index));

        /// <summary>
        /// Returns true if the given column on the board is available
        /// for movement, otherwise false.
        /// </summary>
        /// <param name="board">BitBoard representing current state</param>
        /// <param name="column">Index of the column to check, starts at zero</param>
        public static bool IsColumnOpen(in BitBoard board, int column)
        {
            int checkIndex = 5 + (column * 6);

            return !CheckSingleBit(board.RedDiscs | board.BlackDiscs, checkIndex);
        }

        public static BitBoard GetNewBoard()
            => new BitBoard(0, 0);

        // find highest open spot in a column
        // move into an open column given index and color
        public static BitBoard BitBoardMove(in BitBoard board, int column, DiscColor disc)
        {
            if (!IsColumnOpen(board, column))
                throw new InvalidOperationException($"Column {column} is unavailable for movement.");

            // TODO find or think of a more clever way to do with with some bit masks
            int openBitIndex = 6 * column;

            for (int r = 0; r < NUM_ROWS; r++)
            {
                if (!CheckSingleBit(board.RedDiscs | board.BlackDiscs, openBitIndex))
                    break;

                openBitIndex++;
            }

            if (disc == DiscColor.Red)
            {
                return new BitBoard(SetSingleBit(board.RedDiscs, openBitIndex), board.BlackDiscs);
            }

            return new BitBoard(board.RedDiscs, SetSingleBit(board.BlackDiscs, openBitIndex));
        }

        //public static void BitBoardMove(in BitBoard board, int column, DiscColor disc)
        //{
        //    if (!IsColumnOpen(board, column))
        //        throw new InvalidOperationException($"Column {column} is unavailable for movement.");

        //    // TODO find or think of a more clever way to do with with some bit masks
        //    int openBitIndex = 6 * column;

        //    for (int r = 0; r < NUM_ROWS; r++)
        //    {
        //        if (!CheckSingleBit(board.RedDiscs | board.BlackDiscs, openBitIndex))
        //            break;

        //        openBitIndex++;
        //    }

        //    if (disc == DiscColor.Red)
        //    {
        //        board.RedDiscs = SetSingleBit(board.RedDiscs, openBitIndex);   
        //    }
        //    else
        //    {
        //        board.BlackDiscs = SetSingleBit(board.BlackDiscs, openBitIndex);
        //    }
        //}

        // retrieve open columns
        public static List<int> GetOpenColumns(in BitBoard board)
        {
            var openColumns = new List<int>();

            //foreach (var c in new int[] { 3, 4, 2, 5, 1, 6, 0 })
            for (int c = 0; c < NUM_COLUMNS; c++)
            {
                if (IsColumnOpen(board, c))
                    openColumns.Add(c);
            }

            return openColumns;
        }

        public static bool IsScorable(DiscColor disc, in BitBoard board, ulong possibleFour)
        {
            if (disc == DiscColor.Red)
            {
                // opponent discs
                if ((board.BlackDiscs & possibleFour) != 0)
                    return false;

                // there are no friendly discs
                if ((board.RedDiscs & possibleFour) == 0)
                    return false;
            }
            else
            {
                // opponent discs
                if ((board.RedDiscs & possibleFour) != 0)
                    return false;

                // there are no friendly discs
                if ((board.BlackDiscs & possibleFour) == 0)
                    return false;
            }

            return true;
        }

        public static decimal PossibleFourValue(ulong possibleFour)
        {
            ulong count = 0;

            while (possibleFour > 0)
            {
                count += possibleFour & 1ul;
                possibleFour >>= 1;
            }

            switch (count)
            {
                case 1:
                    return 1.0m;
                case 2:
                    return 4.0m;
                case 3:
                    return 16.0m;
                case 4:
                    return 100000.0m;
                default:
                    return 0.0m;
            }
        }

        public static decimal EvaluateBoardState(in BitBoard board)
        {
            // TODO adjust when moving to negamax
            var redPossiblesValue = CountAllPossibles(board, DiscColor.Red);
            var blackPossiblesValue = CountAllPossibles(board, DiscColor.Black);

            return blackPossiblesValue + (redPossiblesValue * -1.0m);
        }

        public static decimal CountAllPossibles(in BitBoard board, DiscColor disc)
        {
            var ret = ScorePossibleHorizontals(in board, disc);
            ret += ScorePossibleVerticals(in board, disc);
            ret += ScorePossibleRisingDiagonals(in board, disc);
            ret += ScorePossibleFallingDiagonals(in board, disc);

            return ret;
        }

        // check horizontal four
        public static decimal ScorePossibleHorizontals(in BitBoard board, DiscColor disc)
            => ScorePossibleFours(in board, disc, RowHorizontals);

        // check vertical four
        public static decimal ScorePossibleVerticals(in BitBoard board, DiscColor disc)
            => ScorePossibleFours(in board, disc, ColumnVerticals);

        // check rising diagonal four
        public static decimal ScorePossibleRisingDiagonals(in BitBoard board, DiscColor disc)
            => ScorePossibleFours(in board, disc, RisingDiagonals);

        // check falling diagonal four
        public static decimal ScorePossibleFallingDiagonals(in BitBoard board, DiscColor disc)
            => ScorePossibleFours(in board, disc, FallingDiagonals);

        /// <summary>
        /// Generic possible score generator that calculates using alignments 
        /// and groupings of possible fours within each alignment group.
        /// e.g. all horizontal rows, all vertical columns
        /// </summary>
        /// <param name="board"></param>
        /// <param name="disc"></param>
        /// <param name="scoringAlignments"></param>
        /// <returns></returns>
        public static decimal ScorePossibleFours(in BitBoard board, DiscColor disc, ulong[][] scoringAlignments)
        {
            decimal ret = 0.0m;

            foreach (ulong[] alignment in scoringAlignments)
            {
                foreach (ulong grouping in alignment)
                {
                    if (disc == DiscColor.Red)
                    {
                        if (IsScorable(disc, in board, grouping))
                            ret += PossibleFourValue(board.RedDiscs & grouping);
                    }
                    else
                    {
                        if (IsScorable(disc, in board, grouping))
                            ret += PossibleFourValue(board.BlackDiscs & grouping);
                    }
                }
            }

            return ret;
        }

        static DiscColor CheckGroupingsVictory(in BitBoard board, ulong[][] scoringAlignments)
        {
            foreach (var groupings in scoringAlignments)
            {
                foreach (var grouping in groupings)
                {
                    if ((grouping & board.RedDiscs) == grouping)
                        return DiscColor.Red;

                    if ((grouping & board.BlackDiscs) == grouping)
                        return DiscColor.Black;
                }
            }

            return DiscColor.None;
        }

        public static DiscColor CheckVictory(in BitBoard board)
        {
            // This would represent both colors
            // having discs on the same space so it
            // is an invalid board state.
            if ((board.RedDiscs & board.BlackDiscs) != 0ul)
                throw new ArgumentException("Invalid board state, both color discs on same space", nameof(board));

            var check = DiscColor.None;

            check = CheckGroupingsVictory(in board, RowHorizontals);
            if (check != DiscColor.None)
                return check;

            check = CheckGroupingsVictory(in board, ColumnVerticals);
            if (check != DiscColor.None)
                return check;

            check = CheckGroupingsVictory(in board, FallingDiagonals);
            if (check != DiscColor.None)
                return check;

            check = CheckGroupingsVictory(in board, RisingDiagonals);
            if (check != DiscColor.None)
                return check;

            return DiscColor.None;
        }

        /*
        * 5  11  17  23  29  35  41
        * 4  10  16  22  28  34  40
        * 3   9  15  21  27  33  39
        * 2   8  14  20  26  32  38
        * 1   7  13  19  25  31  37
        * 0   6  12  18  24  30  36 
        */
        public static string GetPrettyPrint(in BitBoard board)
        {
            var sb = new StringBuilder();

            var colTracker = 5;
            // for row count
            // scan acorss top row (num of columns)
            for (var _row = 0; _row < LogicalBoardHelpers.NUM_ROWS; _row++)
            {
                sb.Append("|");
                sb.Append(GetColorString(in board, colTracker));
                sb.Append("|");
                sb.Append(GetColorString(in board, colTracker + 6));
                sb.Append("|");
                sb.Append(GetColorString(in board, colTracker + 12));
                sb.Append("|");
                sb.Append(GetColorString(in board, colTracker + 18));
                sb.Append("|");
                sb.Append(GetColorString(in board, colTracker + 24));
                sb.Append("|");
                sb.Append(GetColorString(in board, colTracker + 30));
                sb.Append("|");
                sb.Append(GetColorString(in board, colTracker + 36));
                sb.Append("|");
                sb.AppendLine();
                colTracker--;
            }

            return sb.ToString();
        }

        public static string GetColorString(in BitBoard board, int index)
        {
            if (CheckSingleBit(board.BlackDiscs, index))
                return "O";

            if (CheckSingleBit(board.RedDiscs, index))
                return "X";

            return " ";
        }
    }
}
