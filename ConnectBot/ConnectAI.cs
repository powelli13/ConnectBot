using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

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
        const int red = -1;
        
        /// <summary>
        /// Stores the AIs disc color. 
        /// 1 is black, moves first with positive eval score.
        /// -1 is red, moves second with negative eval score.
        /// </summary>
        protected int aiColor;
        protected int opponentColor;
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
        /// Root of the move tree.
        /// </summary>
        private Node treeRoot;

        /// <summary>
        /// Index of the current best column to move to
        /// updated as the tree is built.
        /// </summary>
        private int currentBestMoveColumn;

        #region Struct : ScoreColumn
        /// <summary>
        /// Lightweight struct used to return the 
        /// score of a position and the column that generated it.
        /// </summary>
        private struct ScoreColumn
        {
            public double score;
            public int column;

            public ScoreColumn(double s, int c)
            {
                score = s;
                column = c;
            }
        }
        #endregion

        #region Class : Node
        /// <summary>
        /// Node of the move tree stores a board state, 
        /// turn disc and column for move that generated this state
        /// and evaluation score of the board state.
        /// </summary>
        private class Node
        {
            // Fields should be private and camelCase exposed by public properties that are PascalCase
            // fields should also have full words used in naming
            // this is overkill in a small project but I'm trying to build good habits and learn
            private int[,] boardDiscState;
            private int colorMoved;
            private int columnMoved;
            private double positionalScore;
            private List<Node> children;

            /// <summary>
            /// Position of the discs on the board for the node.
            /// </summary>
            public int[,] BoardDiscState
            {
                get
                {
                    return boardDiscState;
                }
            }

            /// <summary>
            /// Color to next move at that node.
            /// </summary>
            public int ColorMoved
            {
                get
                {
                    return colorMoved;
                }
            }

            /// <summary>
            /// Column that was moved in to generate this node.
            /// </summary>
            public int ColumnMoved
            {
                get
                {
                    return columnMoved;
                }
            }

            /// <summary>
            /// The evaluated positional score for that node.
            /// </summary>
            public double PositionalScore
            {
                get
                {
                    return positionalScore;
                }
            }

            /// <summary>
            /// List of children moves from this node.
            /// </summary>
            public List<Node> Children
            {
                get
                {
                    return children;
                }

                set
                {
                    children = value;
                }
            }
            
            public Node(int[,] board, int column, int turn, double score)
            {
                this.boardDiscState = board;
                this.columnMoved = column;
                this.positionalScore = score;
                this.colorMoved = turn;// TODO will this pass the turn that just moved?
                this.children = new List<Node>();
            }
        }
        #endregion

        #region Class : ReversibleNode
        /// <summary>
        /// Node representing current game state that can reverse moves
        /// in order to track back up the tree. Will be used in more
        /// efficient searching.
        /// </summary>
        protected class ReversibleNode
        {
            private int[,] boardState;
            private int colorToMove;
            private Stack<int> moveHistory;
            
            /// <summary>
            /// Initializes empty board state with black player to move.
            /// </summary>
            public ReversibleNode()
            {
                // Initialize game state and move history
                boardState = new int[numColumns, numRows];
                colorToMove = black;

                for (int c = 0; c < numColumns; c++)
                {
                    for (int r = 0; r < numRows; r++)
                    {
                        boardState[c, r] = 0;
                    }
                }

                moveHistory = new Stack<int>();
            }

            /// <summary>
            /// Perform move on the reversible board.
            /// </summary>
            /// <param name="column"></param>
            /// <returns>Boolean to signify if the move was correct and performed or not.</returns>
            public bool Move(int column)
            {
                // TODO resolve huge bug where AI thinks it is moving successfully but does not move. 
                // the game should request moves from the AI until it gets a legal one.

                // TODO does move history need to hold color or can it be implied?
                if (boardState[column, numRows - 1] == 0)
                {
                    int rowMoved = 0;

                    while (boardState[column, rowMoved] != 0)
                    {
                        rowMoved++;
                    }

                    boardState[column, rowMoved] = colorToMove;
                    colorToMove = (colorToMove == black ? red : black);
                    moveHistory.Push(column);

                    return true;
                }
                
                return false;
            }

            /// <summary>
            /// Reverses the last move performed.
            /// </summary>
            /// <returns>Boolean to signify if there was a move to reverse.</returns>
            public bool ReverseMove()
            {
                if (moveHistory.Count > 0)
                {
                    int rowRemoved = numRows - 1;
                    int lastColumn = moveHistory.Pop();

                    while (boardState[lastColumn, rowRemoved] == 0)
                    {
                        rowRemoved--;
                    }

                    boardState[lastColumn, rowRemoved] = 0;
                    colorToMove = (colorToMove == black ? red : black);

                    return true;
                }

                return false;
            }
        }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="color">Color for AI to play.</param>
        public ConnectAI(int color, int[,] newBoard, int column, int currTurn)
        {
            aiColor = color;
            opponentColor = (aiColor == black ? red : black);

            // TODO should the root only ever start on the AI's turn? yeah probably
            double score = EvaluateBoardState(newBoard);
            treeRoot = new Node(newBoard, column, currTurn, score);
            currentBestMoveColumn = -1;

            //InitializeTreeBuilder();
            
            rando = new Random();
        }
        
        /// <summary>
        /// Update internal board state.
        /// </summary>
        /// <param name="newBoard">Array representing updated board state.</param>
        /// <param name="currTurn"></param>
        /// <param name="lastMove">Last move made on the new board. Negative one signifies first board update.</param>
        public void UpdateBoard(int[,] newBoard, int currTurn, int lastMove)
        {
            for (int c = 0; c < numColumns; c++)
            {
                for (int r = 0; r < numRows; r++)
                {
                    gameDiscs[c, r] = newBoard[c, r];
                }
            }

            // Initialize tree root if it is null.
            // Moves in 0 column for the root initialization. 
            // TODO this seems strange but shouldn't cause problems maybe make column -1 to signify no move made to reach that position
            //if (treeRoot == null)
            //{
            // TODO when trimming the last move that generated that state may need to be remembered? should automatically happen because of tree structure

            if (lastMove != -1)
            {
                double score = EvaluateBoardState(newBoard);

                treeRoot = new Node(newBoard, lastMove, currTurn, score);
                Stop();
                //InitializeTreeBuilder();
            }
        }

        // Used to call and test methods within the AI class.
        public void AISelfTest()
        {
            // evaluate and empty board
            /*
            int[,] emptyBoard = new int[numColumns, numRows];

            for (int c = 0; c < numColumns; c++)
            {
                for (int r = 0; r < numRows; r++)
                {
                    emptyBoard[c, r] = 0;
                }
            }

            double ts;
            List<int> colors = new List<int>();
            colors.Add(red);
            colors.Add(black);

            foreach (int tc in colors)
            {
                for (int ix = 0; ix < numColumns; ix++)
                {
                    int[,] newBoard = GenerateBoardState(ix, tc, emptyBoard);
                    ts = EvaluateBoardState(newBoard);

                    Console.WriteLine(String.Format("AI scored {0} for color {1} on colum {2} for empty board move.", ts, tc, ix));
                }
            }
            */
            //treeBuilder_GrowChildren(treeRoot, 4);
            // TODO do we need a safety mechanism to not expand children too large?
            System.Console.WriteLine("examine tree root here");
        }

        /// <summary>
        /// Returns the column index that the AI would like to move to.
        /// </summary>
        /// <returns>Index of AI to move to.</returns>
        public int Move_temp()
        {
            currentBestMoveColumn = 0;

            // TODO have best score auto adjust based on AI's color, actually with min max it should not need to
            // TODO i think this bad simple move is making weird moves, make it always maximize and use min max properly to solve this
            double bestScore = 100.0;
            double tempScore = 1000.0;
            List<int> openColumns = GetOpenColumns(gameDiscs);

            foreach (int ix in openColumns)
            {
                int[,] newBoard = GenerateBoardState(ix, aiColor, gameDiscs);
                tempScore = EvaluateBoardState(newBoard);

                Console.WriteLine(String.Format("AI generated state value {0} at column {1}", tempScore, ix));

                if (tempScore < bestScore)
                {
                    bestScore = tempScore;
                    currentBestMoveColumn = ix;
                }
            }

            // TODO should we ever do something to buy the AI more time for it's worker thread?

            string resultString = String.Format("AI found best move at column: {0} with a score of: {1}", currentBestMoveColumn, bestScore);
            Console.WriteLine(resultString);
            return currentBestMoveColumn;
            //return rando.Next(7);
        }
        
        public async Task<int> Move()
        {


            // TODO left off here. because treeRoot doesn't change during update this will only ever return 3 (best move at start)
            // update board needs to trim tree, update root and possilby start off background worker again.
            // check open columns
            // when player moves first AI is not taking that into account. tree should only start building on first update call i think

            //AISelfTest();

            Node n = new Node(gameDiscs, 0, aiColor, 0.0);

            int retMove = AlphaBetaSearch(n);

            // TODO
            // randomly moving if the AI doesn't find something, bad temporary fix
            if (retMove == -1)
            {
                retMove = rando.Next(0, 6);

                while (gameDiscs[retMove, 5] != 0)
                {
                    retMove = rando.Next(0, 6);
                }
            }

            Thread.Sleep(1000);

            return retMove;
            //return retMove;
        }

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
        /// Returns list of column indices open to moves.
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        protected List<int> GetOpenColumns(int[,] board)
        {
            List<int> ret = new List<int>();

            for (int c = 0; c < numColumns; c++)
            {
                if (board[c, numRows - 1] == 0)
                {
                    ret.Add(c);
                }
            }

            return ret;
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
            
            /*
             * For both colors scan the entire board.
             * From each space reach out in scorable
             * directions adding on more value for free
             * spaces that could be used to score,
             * even more value for same color discs
             * and breaking on opponents blocking
             * dics.
             */
            //for (int color = 1; color < 3; color++)
            //{
            for (int c = 0; c < numColumns; c++)
            {
                for (int r = 0; r < numRows; r++)
                {
                    // from every square reach out in all eight directions in loops break when not in bounds
                    // TODO it doesn't try to stop opponents from scoring
                    // make potential victories be worth a huge amount to incentivise stopping
                    // this evaluator does not 
                    //if (boardState[c, r] == black)
                    //{
                    //    // No point in adding value based on individual discs since it will always cancel out
                    //    //ret += 1.0;
                    //    tempCheck = 1;
                    //}
                    //else if (boardState[c, r] == red)
                    //{
                    //    //ret -= 1.0;
                    //    tempCheck = 2;
                    //}

                    if (boardState[c, r] != 0)
                    {
                        ScorePossibles(boardState, boardState[c, r], c, r, ref ret);
                    }

                    //tempCheck = 0;
                }
            }
            //}

            return ret;
        }

        /// <summary>
        /// Modify the board positional score based on scoring
        /// potentials of both sides.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="checkColor"></param>
        /// <param name="c"></param>
        /// <param name="r"></param>
        /// <param name="val"></param>
        private void ScorePossibles(int[,] board, int checkColor, int c, int r, ref double val)
        {
            /*
             * 4 in a row is worth as 1 point.
             * Each like colored disc in a possible 
             * (only open or same color, free from opposing color)
             * 4 long sequence of spaces, is worth 0.25 points.
             * 
             * Reach out in all eight directions and move rolling
             * 4 long sequence through spot being checked to determine total value.
             * short circuit when opposing pieces are hit.
             * 
             * 
             */

            // The total number of possible open scoring combinations 
            // that the checked disc participates.
            int participatedPossibles = 0;
            int oppositeColor = (checkColor == black ? red : black);
            
            bool addPossible = true;

            // Horizontal scan
            // same row
            // column +- 3
            // The square looks from three to the left to zero, zero being itself.
            // For each of these spaces to the left it scans four to the right. 
            // This will examine all four potential horizontal scorings depending 
            // on board position.
            for (int ch = -3; ch < 1; ch++)
            {
                addPossible = true;

                for (int trace = 0; trace < 4; trace++)
                {
                    if (InBounds(c + ch + trace, r))
                    {
                        if (board[c + ch + trace, r] == oppositeColor)
                        {
                            addPossible = false;
                            break;
                        }
                    }
                    else
                    {
                        addPossible = false;
                        break;
                    }
                }

                if (addPossible)
                {
                    participatedPossibles += 1;
                }
            }

            // Vertical scan
            // same column
            // row +- 3
            // Look down three spaces.
            // For each space count up four and add possibles when clear.
            for (int rh = -3; rh < 1; rh++)
            {
                addPossible = true;

                for (int trace = 0; trace < 4; trace++)
                {
                    if (InBounds(c, r + rh + trace))
                    {
                        if (board[c, r + rh + trace] == oppositeColor)
                        {
                            addPossible = false;
                            break;
                        }
                    }
                    else
                    {
                        addPossible = false;
                        break;
                    }
                }

                if (addPossible)
                {
                    participatedPossibles += 1;
                }
            }

            // Angled up right scan
            // row and column +- 3 always same difference for row and column
            // row + when column + and row - when column -
            for (int ur = -3; ur < 1; ur++)
            {
                addPossible = true;
                
                for (int trace = 0; trace < 4; trace++)
                {
                    if (InBounds(c + ur + trace, r + ur + trace))
                    {
                        if (board[c + ur + trace, r + ur + trace] == oppositeColor)
                        {
                            addPossible = false;
                            break;
                        }
                    }
                    else
                    {
                        addPossible = false;
                        break;
                    }
                }

                if (addPossible)
                {
                    participatedPossibles += 1;
                }
            }

            // Angled up left scan
            // row and column +- 3 always same difference for row and column
            // row - when column + and row + when column -
            for (int ul = -3; ul < 1; ul++)
            {
                addPossible = true;

                for (int trace = 0; trace < 4; trace++)
                {
                    if (InBounds(c + ((-1) * (ul + trace)), r + ul + trace))
                    {
                        if (board[c + ((-1) * (ul + trace)), r + ul + trace] == oppositeColor)
                        {
                            addPossible = false;
                            break;
                        }
                    }
                    else
                    {
                        addPossible = false;
                        break;
                    }
                }

                if (addPossible)
                {
                    participatedPossibles += 1;
                }
            }

            //int colorMulti = (checkColor == red ? -1 : 1);
            // TODO possible don't multiply here since negamax search is multiplying also
            val += (0.25 * participatedPossibles) * checkColor;
        }


        private int AlphaBetaSearch(Node n)
        {

            double alpha = Double.MinValue;
            double beta = Double.MaxValue;

            int totalDepth = 7;

            // TODO change this based on the AI's color
            double val = MinValue(n, ref alpha, ref beta, ref totalDepth);

            int ret = -1;

            foreach (int openMove in GetOpenColumns(n.BoardDiscState))
            {
                if (EvaluateBoardState(GenerateBoardState(openMove, aiColor, n.BoardDiscState)) == val)
                {
                    ret = openMove;
                    break;
                }
            }

            return ret;
        }


        private double MaxValue(Node n, ref double alpha, ref double beta, ref int depth)
        {
            // TODO terminal check
            if (depth <= 0)
            {
                return EvaluateBoardState(n.BoardDiscState);
            }

            double ret = double.MinValue;
            int colorMoved = (n.ColorMoved == black ? red : black);

            foreach (int openMove in GetOpenColumns(n.BoardDiscState))
            {
                int[,] newState = GenerateBoardState(openMove, colorMoved, n.BoardDiscState);

                Node child = new Node(newState, openMove, colorMoved, 0.0);

                depth--;
                ret = Math.Max(ret, MinValue(child, ref alpha, ref beta, ref depth));

                if (ret >= beta)
                {
                    return ret;
                }

                alpha = Math.Max(alpha, ret);
            }

            return ret;
        }


        private double MinValue(Node n, ref double alpha, ref double beta, ref int depth)
        {
            // TODO terminal check
            if (depth <= 0)
            {
                return EvaluateBoardState(n.BoardDiscState);
            }

            double ret = double.MaxValue;
            int colorMoved = (n.ColorMoved == black ? red : black);

            foreach (int openMove in GetOpenColumns(n.BoardDiscState))
            {
                int[,] newState = GenerateBoardState(openMove, colorMoved, n.BoardDiscState);

                Node child = new Node(newState, openMove, colorMoved, 0.0);

                depth--;
                ret = Math.Min(ret, MaxValue(child, ref alpha, ref beta, ref depth));

                if (ret <= alpha)
                {
                    return ret;
                }

                beta = Math.Min(beta, ret);
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
        private void ScorePossibles_old(int[,] board, int checkColor, int c, int r, ref double val)
        {
            int tempCol = -1;
            int tempRow = -1;
            int colorMulti = 1;
            
            if (checkColor == red)
            {
                colorMulti = -1;
            }
            
            /*
             * m is a multiplier to stretch out 1, 2 and 3
             * spaces away from the current space being played.
             * cdiff and rdiff move in all eight possible directions
             * out from the current space summing up the space
             * values based on whether nearby spaces are empty
             * or have a same color disc.
             */
            for (int m = 1; m < 4; m++)
            {
                for (int cdiff = -1; cdiff < 2; cdiff++)
                {
                    for (int rdiff = -1; rdiff < 2; rdiff++)
                    {
                        tempCol = c + (cdiff * m);
                        tempRow = r + (rdiff * m);

                        if (InBounds(tempRow, tempCol))
                        {
                            if (board[tempRow, tempCol] == 0)
                            {
                                //val += (0.0625 * colorMulti);
                            }
                            else if (board[tempRow, tempCol] == checkColor)
                            {
                                // Lateral connections valued more than veritcal
                                // they shouldn't be it should be based on free space that could potentially become more scoring possibilities
                                if (rdiff == 0)
                                {
                                    val += (0.25 * colorMulti);
                                }
                                else
                                {
                                    val += (0.25 * colorMulti);
                                }

                                
                                // TODO heavily rewarding three in a row
                                // TODO this can be improved
                                // needs to weigh the opponents three more than its own
                                if (m < 3)
                                {
                                    tempCol = c + (cdiff * (m + 1));
                                    tempRow = r + (rdiff * (m + 1));

                                    if (InBounds(tempRow, tempCol))
                                    {
                                        if (board[tempRow, tempCol] == checkColor)
                                        {
                                            val += (0.5 * colorMulti);

                                            //if (checkColor != aiColor)
                                            //{
                                            //    val += (0.5 * colorMulti);
                                            //}
                                        }
                                    }
                                }
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
            if (treeBuilder != null && treeBuilder.WorkerSupportsCancellation)
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

            //while (buildTree)
            //{
            if (worker.CancellationPending == true)
            {
                e.Cancel = true;
                // was break; when loop was here
                return;
            }

            // TODO how to respond appropriately to a null root?
            // TODO how to build from the leafs once root as been established?
            // TODO make a method to enumerate children given a node call that recursively
            if (treeRoot != null)
            {
                if (treeRoot.Children.Count == 0)
                {
                    System.Console.WriteLine("starting to growchildren");
                    treeBuilder_GrowChildren(treeRoot, 4);
                    System.Console.WriteLine("check out tree root");
                }

            }
            //} // TODO this was a loop to continuously build tree. should the tree be restarted from the new node on tree trimming? might make sense
            // that would require the back ground worker to continually reset, and it should probably take in any Node type and not depend on treeRoot.
        }

        /// <summary>
        /// Builds out all possible child moves form a given
        /// parent board state.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="depth"></param>
        private void treeBuilder_GrowChildren(Node parent, int depth)
        {
            //int nextTurn = (parent.CurrentTurn == black ? red : black);
            // The color that moved resulting in the current position.
            int colorMoved = (parent.ColorMoved == black ? red : black);

            List<int> openColumns = GetOpenColumns(parent.BoardDiscState);

            foreach (int ix in openColumns)
            {
                int[,] newState = GenerateBoardState(ix, colorMoved, parent.BoardDiscState);
                double newScore = EvaluateBoardState(newState);
                
                Node child = new Node(newState, ix, colorMoved, newScore);
                parent.Children.Add(child);
            }

            // Build off of children if we haven't hit max depth yet.
            if (depth > 0)
            {
                for (int ic = 0; ic < parent.Children.Count; ic++)
                {
                    treeBuilder_GrowChildren(parent.Children[ic], depth - 1);
                }
            }

            //if (depth <= 0)
            //{
            //    Console.WriteLine("Reached max depth on branch.");
            //}
            // testing VS git integration
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
