using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ConnectBot
{
    class ConnectAI
    {


        // this shouldn't need a buffer
        //
        // Foremost indices in sub arrays represent 'bottom' spaces of Connect 4 board.
        // 1 is a black disc, 2 is red.
        // TODO I think black goes first
        // 

        /// <summary>
        /// Foremost indices in sub arrays represent 'bottom' spaces of Connect 4 board.
        /// 1 is a black disc, 2 is red.
        /// </summary>
        protected int[] gameDiscs = new int[]
        {
            0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0
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

            InitializeTreeBuilder();

            rando = new Random();
        }
        
        /// <summary>
        /// Update internal board state.
        /// </summary>
        /// <param name="newBoard">Array representing updated board state.</param>
        public void UpdateBoard(int[] newBoard)
        {
            for (int ix = 9; ix < 42; ix++)
            {
                gameDiscs[ix] = newBoard[ix];
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

        /// <summary>
        /// Stops the AI's worker thread from continuing to build the tree.
        /// </summary>
        public void Stop()
        {
            if (treeBuilder.WorkerSupportsCancellation)
            {
                treeBuilder.CancelAsync();
            }
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

        /// <summary>
        /// Setup worker class to build move tree.
        /// </summary>
        private void InitializeTreeBuilder()
        {
            treeBuilder = new BackgroundWorker();
            treeBuilder.DoWork += new DoWorkEventHandler(treeBuilder_DoWork);
            treeBuilder.RunWorkerCompleted += new RunWorkerCompletedEventHandler(treeBuilder_WorkCompleted);
            treeBuilder.WorkerSupportsCancellation = true;

            treeBuilder.RunWorkerAsync();
        }

        /// <summary>
        /// Performs background work for building the tree and 
        /// updating the current optimal move as the game proceeds. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeBuilder_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            while (buildTree)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }

                taskCounter++;

                if (taskCounter % 100000 == 0)
                {
                    Console.WriteLine("Another ten thousand in builder thread. Total: {0}", taskCounter);
                }

                if (taskCounter >= 1000000)
                {
                    taskCounter = 0;
                }
            }
        }

        /// <summary>
        /// Called when the background worker completes or is cancelled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeBuilder_WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Console.WriteLine("Canceled.");
                return;
            }

            Console.WriteLine("Completed.");
        }
    }
}
