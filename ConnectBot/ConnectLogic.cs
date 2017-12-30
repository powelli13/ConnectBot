using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectBot
{
    class ConnectLogic
    {
        /// <summary>
        /// Array of int arrays. Represents columns and rows where the discs are.
        /// Sub arrays represent columns. Board has 7 columns and 6 rows.
        /// Foremost indices in sub arrays represent 'bottom' spaces of Connect 4 board.
        /// 1 is a black disc, 2 is red.
        /// TODO I think black goes first
        /// TODO this entire class is unnecessary to do the simplicity of the game.
        /// TODO only possibly useful part is the integer array to represent board
        /// TODO AI can get a succinct board representation given to it by the Connect Game class
        /// </summary>
        protected int[][] gameDiscs = new int[][]
        {
            new int[] {0, 0, 0, 0, 0, 0},
            new int[] {0, 0, 0, 0, 0, 0},
            new int[] {0, 0, 0, 0, 0, 0},
            new int[] {0, 0, 0, 0, 0, 0},
            new int[] {0, 0, 0, 0, 0, 0},
            new int[] {0, 0, 0, 0, 0, 0},
            new int[] {0, 0, 0, 0, 0, 0}
        };

        protected int turn = 1;

        public ConnectLogic()
        {
            ResetBoard();
        }

        /// <summary>
        /// Set all game state variables to initial state.
        /// </summary>
        protected void ResetBoard()
        {
            for (int col = 0; col < 7; col++)
            {
                for (int row = 0; row < 6; row++)
                {
                    gameDiscs[col][row] = 0;
                }
            }

            turn = 1;
        }

        /// <summary>
        /// Receives user click data from GUI and changes game state accordingly.
        /// </summary>
        /// <param name="columnIndex">Index of the column left most is 0.</param>
        protected void HandleClick(int columnIndex)
        {

        }

        /// <summary>
        /// Return the current board state to the GUI 
        /// </summary>
        /// <returns></returns>
        public int[][] GetBoardState()
        {
            return gameDiscs;
        }
    }
}
