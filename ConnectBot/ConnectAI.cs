using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ConnectBot
{
    class ConnectAI
    {
        /// <summary>
        /// Array of int arrays. Represents columns and rows where the discs are.
        /// Sub arrays represent columns. Board has 7 columns and 6 rows.
        /// Negative 1s to buffer board for win detection make board 9 columns and 8 rows.
        /// Foremost indices in sub arrays represent 'bottom' spaces of Connect 4 board.
        /// 1 is a black disc, 2 is red.
        /// TODO I think black goes first
        /// TODO this entire class is unnecessary to do the simplicity of the game.
        /// TODO only possibly useful part is the integer array to represent board
        /// TODO AI can get a succinct board representation given to it by the Connect Game class
        /// TODO are buffers even useful?
        /// </summary>
        //protected int[][] gameDiscs = new int[][]
        //{
        //    new int[] {-1, -1, -1, -1, -1, -1, -1, -1},
        //    new int[] {-1, 0, 0, 0, 0, 0, 0, -1},
        //    new int[] {-1, 0, 0, 0, 0, 0, 0, -1},
        //    new int[] {-1, 0, 0, 0, 0, 0, 0, -1},
        //    new int[] {-1, 0, 0, 0, 0, 0, 0, -1},
        //    new int[] {-1, 0, 0, 0, 0, 0, 0, -1},
        //    new int[] {-1, 0, 0, 0, 0, 0, 0, -1},
        //    new int[] {-1, 0, 0, 0, 0, 0, 0, -1},
        //    new int[] {-1, -1, -1, -1, -1, -1, -1, -1}
        //};

        // this shouldn't need a buffer
        protected int[] gameDiscs = new int[]
        {
            -1, -1, -1, -1, -1, -1, -1, -1,
            -1, 0, 0, 0, 0, 0, 0, -1,
            -1, 0, 0, 0, 0, 0, 0, -1,
            -1, 0, 0, 0, 0, 0, 0, -1,
            -1, 0, 0, 0, 0, 0, 0, -1,
            -1, 0, 0, 0, 0, 0, 0, -1,
            -1, 0, 0, 0, 0, 0, 0, -1,
            -1, 0, 0, 0, 0, 0, 0, -1,
            -1, -1, -1, -1, -1, -1, -1, -1
        };
        
        protected int aiColor;
        // TODO have best move variable updated as tree is explored? return when called upon?

        /// <summary>
        /// used for testing
        /// </summary>
        protected Random rando;
        protected int taskCounter;
        /// <summary>
        /// Used to stop the async tree builder.
        /// </summary>
        protected bool buildTree = true;

        private BackgroundWorker treeBuilder;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="color">Color for AI to play.</param>
        public ConnectAI(int color)
        {
            aiColor = color;

            treeBuilder = new BackgroundWorker();
            treeBuilder.DoWork += new DoWorkEventHandler(treeBuilder_DoWork);
            treeBuilder.RunWorkerAsync();

            //buildTree = true;

            //int treeInt = BuildTree();

            rando = new Random();
        }
        
        /// <summary>
        /// Update internal board state.
        /// </summary>
        /// <param name="newBoard">Array representing updated board state.</param>
        public void UpdateBoard(int[] newBoard)
        {
            // starts at 9 skips two every six for -1 buffer
            int trace = 0;
            int skipper = 0;

            for (int ix = 9; ix < 63; ix++)
            {
                gameDiscs[ix] = newBoard[trace];
                trace++;
                skipper++;
                
                if (skipper == 5)
                {
                    skipper = 0;
                    ix += 2;
                }
            }
            //TODO trim tree
        }
        

        /// <summary>
        /// Returns the column index that the AI would like to move to.
        /// </summary>
        /// <returns>Index of AI to move to.</returns>
        public int Move()
        {
            //TODO use tree
            return rando.Next(0, 7);
        }

        //TODO could be useful for analyzing
        // when checking up add 1, 2, 3,
        //      check up on 0 - 6 columns
        //      check up on 0 - 2 rows
        // when checking right add 7, 8, 9
        //      check right on 0 - 3 columns
        //      check right on 0 - 6 rows
        // when checking up left subtract 6, 12, 19
        //      check up left on columns 3 - 6
        //      check up right on rows 0 - 2
        // when checking up right add 8, 16, 24
        //      check up right on columns 0 - 3
        //      check up right on rows 0 - 2
        // NOTE: spaces that have intersections with more possible winning sequences are more valuable.
        // AI should focus on finding squares that have the most winning sequences for it's side and take them
        // AI should auto play any wins and auto block any losses.


        // TODO work thread runs while game plays however UI becomes unresponsive.
        // TODO build tree in stages, tell AI to stop background workers when done.
        private void treeBuilder_DoWork(object sender, EventArgs e)
        {
            while (buildTree)
            {
                taskCounter++;

                if (taskCounter % 1000 == 0)
                {
                    System.Console.WriteLine("Another ten thousand in builder thread. Total: {0}", taskCounter);
                }

                if (taskCounter >= 1000000000)
                {
                    taskCounter = 0;
                }
            }
        }

        //public async Task<int> BuildTree()
        //{

        //    while (buildTree)
        //    {

        //    }

        //    System.Console.WriteLine("Stopped building tree.");

        //    return 0;
        //}
    }
}
