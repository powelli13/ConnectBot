﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectBot
{
    /// <summary>
    /// Common methods to perform on a 6x7 array of integers
    /// that represents the logical board state.
    /// </summary>
    public static class LogicalBoardHelpers
    {
        public static int NUM_ROWS = 6;
        public static int NUM_COLUMNS = 7;

        public static int DISC_COLOR_BLACK = 1;
        public static int DISC_COLOR_RED = -1;
        public static int DISC_COLOR_NONE = 0;

        // TODO this should move to logical board
        /// <summary>
        /// Returns the winning color, otherwise zero to indicate no winner.
        /// </summary>
        public static int CheckVictory(int[,] board)
        {
            if (board.Rank != 2) throw new ArgumentException("Must give a two dimensional array.", nameof(board));
            if (board.GetLength(0) != NUM_COLUMNS) throw new ArgumentException("Invalid number of columns.", nameof(board));
            if (board.GetLength(1) != NUM_ROWS) throw new ArgumentException("Invalid number of rows.", nameof(board));

            // TODO display board after victory for a few seconds.
            // when checking up add 1, 2, 3 to rows
            //      check up on 0 - 6 columns
            //      check up on 0 - 2 rows
            // when checking right add 1, 2, 3, to columns
            //      check right on 0 - 3 columns
            //      check right on 0 - 6 rows
            // when checking up left subtract 1 from cols, add 1 to rows
            //      check up left on columns 3 - 6
            //      check up right on rows 0 - 2
            // when checking up right add 1 to cols, add 1 to rows
            //      check up right on columns 0 - 3
            //      check up right on rows 0 - 2
            // TODO could only check when latest move is in the mix
            int first;
            int second;
            int third;
            int fourth;


            // check verticals
            for (int chkUpCol = 0; chkUpCol < NUM_COLUMNS; chkUpCol++)
            {
                for (int chkUpRow = 0; chkUpRow < 3; chkUpRow++)
                {
                    first = board[chkUpCol, chkUpRow];
                    second = board[chkUpCol, chkUpRow + 1];
                    third = board[chkUpCol, chkUpRow + 2];
                    fourth = board[chkUpCol, chkUpRow + 3];

                    if (first != 0 &&
                        first == second &&
                        first == third &&
                        first == fourth)
                    {
                        return first;
                    }
                }
            }

            // check horizontals
            for (int chkCrossCol = 0; chkCrossCol < 4; chkCrossCol++)
            {
                for (int chkCrossRow = 0; chkCrossRow < NUM_ROWS; chkCrossRow++)
                {
                    first = board[chkCrossCol, chkCrossRow];
                    second = board[chkCrossCol + 1, chkCrossRow];
                    third = board[chkCrossCol + 2, chkCrossRow];
                    fourth = board[chkCrossCol + 3, chkCrossRow];

                    if (first != 0 &&
                        first == second &&
                        first == third &&
                        first == fourth)
                    {
                        return first;
                    }
                }
            }

            // check left diagonal
            for (int chkLDiagCol = 3; chkLDiagCol < NUM_COLUMNS; chkLDiagCol++)
            {
                for (int chkLDiagRow = 0; chkLDiagRow < 3; chkLDiagRow++)
                {
                    first = board[chkLDiagCol, chkLDiagRow];
                    second = board[chkLDiagCol - 1, chkLDiagRow + 1];
                    third = board[chkLDiagCol - 2, chkLDiagRow + 2];
                    fourth = board[chkLDiagCol - 3, chkLDiagRow + 3];

                    if (first != 0 &&
                        first == second &&
                        first == third &&
                        first == fourth)
                    {
                        return first;
                    }
                }
            }

            // check right diagonal
            for (int chkRDiagCol = 0; chkRDiagCol < 4; chkRDiagCol++)
            {
                for (int chkRDiagRow = 0; chkRDiagRow < 3; chkRDiagRow++)
                {
                    first = board[chkRDiagCol, chkRDiagRow];
                    second = board[chkRDiagCol + 1, chkRDiagRow + 1];
                    third = board[chkRDiagCol + 2, chkRDiagRow + 2];
                    fourth = board[chkRDiagCol + 3, chkRDiagRow + 3];

                    if (first != 0 &&
                        first == second &&
                        first == third &&
                        first == fourth)
                    {
                        return first;
                    }
                }
            }

            return 0;
        }
    }
}
