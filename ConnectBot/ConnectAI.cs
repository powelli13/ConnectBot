using System;
using System.ComponentModel;
using System.Collections.ObjectModel;

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

        const int numRows = 6;
        const int numColumns = 7;

        /// <summary>
        /// Multidimensional array of integers representing the board
        /// indexed by [column, row]. Lowest column index is left most 
        /// on the screen. Lowest row index is lowest in the column stack.
        /// </summary>
        protected int[,] gameDiscs = new int[numColumns, numRows];
        const int black = 1;
        const int red = 2;
        
        /// <summary>
        /// Stores the AIs disc color. 
        /// 1 is black, moves first with positive eval score.
        /// 2 is red, moves second with negative eval score.
        /// </summary>
        protected int aiColor;
        // TODO have best move variable updated as tree is explored? return when called upon?

        /// <summary>
        /// used for testing
        /// </summary>
        protected Random rando;
        protected int taskCounter;

        // Variables used to build and use the move tree.
        /// <summary>
        /// Used to stop the async tree builder.
        /// </summary>
        protected bool buildTree = true;

        /// <summary>
        /// Background worker to construct the minmax tree 
        /// used to determine optimal moves.
        /// </summary>
        private BackgroundWorker treeBuilder;

        /// <summary>
        /// Index of the current best column to move to
        /// updated as the tree is built.
        /// </summary>
        private int currentBestMoveColumn;

        /// <summary>
        /// Lightweight class used to store a board state, turn
        /// and evaluation score of the board state.
        /// // TODO parent move? probably not. should be in tree.
        /// // TODO could this just have the column index that would generate that node? does it need the whole board?
        /// </summary>
        private class Node
        {
            private int[,] boardDiscState;
            public double evalScore;
            public int column;

            public Node(int column, double evalScore)
            {
                this.column = column;
                this.evalScore = evalScore;
            }
        }

        /// <summary>
        /// Collection of tree nodes used to organize a node's children.
        /// </summary>
        private class NodeList : Collection<Node>
        {
            public NodeList() : base() { }

            public Node FindHighestScore()
            {
                return new Node(0, 0.0);
            }

            public void AddNode(Node n)
            {
                base.Items.Add(n);
            }
        }


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
        public void UpdateBoard(int[,] newBoard)
        {
            for (int c = 0; c < numColumns; c++)
            {
                for (int r = 0; r < numRows; r++)
                {
                    gameDiscs[c, r] = newBoard[c, r];
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
            currentBestMoveColumn = 0;

            // TODO have best score auto adjust based on AI's color
            double bestScore = 100.0;
            double tempScore = 1000.0;

            for (int ix = 0; ix < numColumns; ix++)
            {
                tempScore = EvaluateBoardState(GenerateBoardState(ix, aiColor, gameDiscs));
                if (tempScore < bestScore)
                {
                    bestScore = tempScore;
                    currentBestMoveColumn = ix;
                }
            }

            string resultString = String.Format("AI found best move at column: {0} with a score of: {1}", currentBestMoveColumn, bestScore);
            Console.WriteLine(resultString);
            return currentBestMoveColumn;
            //return rando.Next(7);
        }
        
        // TODO determine which columns are movable and only generate states for them
        /// <summary>
        /// Generate a new board state based on a column index, disc color
        /// and an existing board state.
        /// </summary>
        /// <param name="moveColumn"></param>
        /// <param name="discColor"></param>
        /// <param name="currState"></param>
        /// <returns></returns>
        protected int[,] GenerateBoardState(int moveColumn, int discColor, int[,] currState)
        {
            // TODO is this needed or can c# copy multi dimensional arrays?
            int[,] newState = new int[numColumns, numRows];

            for (int c = 0; c < numColumns; c++)
            {
                for (int r = 0; r < numRows; r++)
                {
                    newState[c, r] = currState[c, r];
                }
            }

            // TODO currently assuming that the move is valid.
            // this will probably break.
            for (int row = 0; row < numRows; row++)
            {
                if (newState[moveColumn, row] == 0)
                {
                    //Console.WriteLine("AI generated a board state.");
                    newState[moveColumn, row] = discColor;
                    break;
                }
            }

            return newState;
        }

        /// <summary>
        /// Evalues the board state and returns a real number score
        /// postive numbers favor black, negative favor red.
        /// Every disc is worth one. Adjacent discs of the same color 
        /// add .125 if adjacent discs help to form a line they are
        /// worth .25
        /// </summary>
        /// <param name="boardState"></param>
        /// <returns></returns>
        /// TODO there has to be a better way to structure board than all this rigorous checking
        protected double EvaluateBoardState(int[,] boardState)
        {
            double ret = 0.0;
            int tempCheck = 0;
            int tempCol = -1;
            int tempRow = -1;
            int colorMulti = 1;
            
            /*
             * For both colors scan the entire board.
             * From each space reach out in scorable
             * directions adding on more value for free
             * spaces that could be used to score,
             * even more value for same color discs
             * and breaking on opponents blocking
             * dics.
             */
            for (int color = 1; color < 3; color++)
            {
                for (int c = 0; c < numColumns; c++)
                {
                    for (int r = 0; r < numRows; r++)
                    {
                        // from every square reach out in all eight directions in loops break when not in bounds
                        // if space is open add .125 (possible scoring)
                        // if oppoosite color break
                        // if same color add .25


                        // TODO board state == color?
                        // TODO it doesn't try to stop opponents from scoring
                        // make potential victories be worth a huge amount to incentivise stopping
                        if (boardState[c, r] == black)
                        {
                            ret += 1.0;
                            tempCheck = 1;
                            colorMulti = 1;
                        }
                        else if (boardState[c, r] == red)
                        {
                            ret -= 1.0;
                            tempCheck = 2;
                            colorMulti = -1;
                        }

                        if (tempCheck != 0)
                        {
                            ScorePossibles(boardState, tempCheck, c, r, ref ret);
                        }

                        tempCheck = 0;
                        colorMulti = 1;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Modify the board evaluation value based on 
        /// the scoring potentials of a given space.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="checkColor"></param>
        /// <param name="c"></param>
        /// <param name="r"></param>
        /// <param name="val"></param>
        private void ScorePossibles(int[,] board, int checkColor, int c, int r, ref double val)
        {
            int tempCol = -1;
            int tempRow = -1;
            int colorMulti = 1;

            if (checkColor == black)
            {
                colorMulti = 1;
            }
            else if (checkColor == red)
            {
                colorMulti = -1;
            }

            for (int m = 1; m < 4; m++)
            {
                for (int cdiff = -1; cdiff < 2; cdiff++)
                {
                    for (int rdiff = -1; rdiff < 2; rdiff++)
                    {
                        tempCol = c + (cdiff * m);
                        tempRow = r + (rdiff * m);

                        if (InBounds(tempCol, tempRow))
                        {
                            if (board[tempCol, tempRow] == 0)
                            {
                                val += 0.125 * colorMulti;
                            }
                            else if (board[tempCol, tempRow] == checkColor)
                            {
                                val += 0.25 * colorMulti;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a [column, row] index pair
        /// are in legal bounds of the board.
        /// </summary>
        /// <returns></returns>
        private bool InBounds(int col, int row)
        {
            if ((col > -1 && col < 7) && (row > -1 && row < 6))
            {
                return true;
            }
            
            return false;
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

                //if (taskCounter % 100000 == 0)
                //{
                //    Console.WriteLine("Another ten thousand in builder thread. Total: {0}", taskCounter);
                //}

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
