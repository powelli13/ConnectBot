using System;
using System.Collections.Generic;
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

        const int NumRows = 6;
        const int NumColumns = 7;

        /// <summary>
        /// Multidimensional array of integers representing the board
        /// indexed by [column, row]. Lowest column index is left most 
        /// on the screen. Lowest row index is lowest in the column stack.
        /// </summary>
        protected int[,] GameDiscs = new int[NumColumns, NumRows];
        
        /// <summary>
        /// Stores the AIs disc color. 
        /// 1 is black, moves first with positive eval score.
        /// -1 is red, moves second with negative eval score.
        /// </summary>
        protected int AiColor { get; set; }
        protected int OpponentColor { get; set; }
        // TODO have best move variable updated as tree is explored? return when called upon?

        /// <summary>
        /// used for testing
        /// </summary>
        protected Random Rando { get; set; }

        /// <summary>
        /// Index of the current best column to move to
        /// updated as the tree is built.
        /// </summary>
        private int CurrentBestMoveColumn { get; set; }

        #region Class : Node
        /// <summary>
        /// Node of the move tree stores a board state, 
        /// turn disc and column for move that generated this state
        /// and evaluation score of the board state.
        /// </summary>
        private class Node
        {
            public int[,] BoardDiscState { get; set; }

            /// <summary>
            /// Color to next move at that node.
            /// </summary>
            public int ColorMoved { get; set; }

            /// <summary>
            /// Column that was moved in to generate this node.
            /// </summary>
            public int ColumnMoved { get; set; }

            public decimal PositionalScore { get; set; }

            public List<Node> Children { get; set; }
            
            public Node(int[,] board, int column, int turn, decimal score = 0.0m)
            {
                BoardDiscState = board;
                ColumnMoved = column;
                PositionalScore = score;
                ColorMoved = turn;// TODO will this pass the turn that just moved?
                Children = new List<Node>();
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
                boardState = new int[NumColumns, NumRows];
                colorToMove = LogicalBoardHelpers.DISC_COLOR_BLACK;

                for (int c = 0; c < NumColumns; c++)
                {
                    for (int r = 0; r < NumRows; r++)
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
                // TODO resolve bug where AI thinks it is moving successfully but does not move. 
                // the game should request moves from the AI until it gets a legal one.

                // TODO does move history need to hold color or can it be implied?
                if (boardState[column, NumRows - 1] == 0)
                {
                    int rowMoved = 0;

                    while (boardState[column, rowMoved] != 0)
                    {
                        rowMoved++;
                    }

                    boardState[column, rowMoved] = colorToMove;
                    colorToMove = LogicalBoardHelpers.ChangeTurnColor(colorToMove);
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
                    int rowRemoved = NumRows - 1;
                    int lastColumn = moveHistory.Pop();

                    while (boardState[lastColumn, rowRemoved] == 0)
                    {
                        rowRemoved--;
                    }

                    boardState[lastColumn, rowRemoved] = 0;
                    colorToMove = LogicalBoardHelpers.ChangeTurnColor(colorToMove);

                    return true;
                }

                return false;
            }
        }
        #endregion

        #region Class : ScoredMoveIndex
        /// <summary>
        /// Lightweight class used to return the score of a 
        /// position and the moved column that generated it.
        /// </summary>
        protected class ScoredMoveIndex
        {
            public decimal PositionScore { get; set; }

            public int MoveIndex { get; set; }

            public ScoredMoveIndex(decimal positionScore, int moveIndex)
            {
                PositionScore = positionScore;
                MoveIndex = moveIndex;
            }
        }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="color">Color for AI to play.</param>
        public ConnectAI(int color)
        {
            AiColor = color;
            OpponentColor = LogicalBoardHelpers.ChangeTurnColor(AiColor);
            // TODO should the root only ever start on the AI's turn? yeah probably

            Rando = new Random();
        }
        
        /// <summary>
        /// Update internal board state.
        /// </summary>
        /// <param name="newBoard">Array representing updated board state.</param>
        /// <param name="lastMove">Last move made on the new board. Negative one signifies first board update.</param>
        public void UpdateBoard(int[,] newBoard, int lastMove)
        {
            for (int c = 0; c < NumColumns; c++)
            {
                for (int r = 0; r < NumRows; r++)
                {
                    GameDiscs[c, r] = newBoard[c, r];
                }
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

        public async Task<int> Move()
        {
            // TODO left off here. because treeRoot doesn't change during update this will only ever return 3 
            // (best move at start)
            // update board needs to trim tree, update root and possilby start off background worker again.
            // check open columns
            // when player moves first AI is not taking that into account. tree should only start building on first update call i think

            //AISelfTest();

            // Look for wins before performing in depth searches
            int killerMove = await FindKillerMove(GameDiscs);

            if (killerMove != -1) 
                return killerMove;

            Node n = new Node(GameDiscs, 0, AiColor, 0.0m);
            
            int retMove = MinimaxCutoffSearch(n);

            return retMove;
        }

        protected async Task<int> FindKillerMove(int[,] boardState)
        {
            int move = -1;

            for (int i = 0; i < NumColumns; i++)
            {
                try
                {
                    // If the AI could win return that immediately.
                    var movedBoard = GenerateBoardState(i, AiColor, boardState);
                    var winner = LogicalBoardHelpers.CheckVictory(movedBoard);

                    if (winner == AiColor)
                        return i;

                    // If the opponent could win be sure to check the rest in case the AI could win
                    // at a column further down.
                    var oppMovedBoard = GenerateBoardState(i, OpponentColor, boardState);
                    var oppWinner = LogicalBoardHelpers.CheckVictory(oppMovedBoard);

                    if (oppWinner == OpponentColor)
                        move = i;
                }
                catch (InvalidOperationException _) { }
            }

            return move;
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
            int[,] newState = new int[NumColumns, NumRows];

            for (int c = 0; c < NumColumns; c++)
            {
                for (int r = 0; r < NumRows; r++)
                {
                    newState[c, r] = currState[c, r];
                }
            }

            bool moveWasLegal = false;
            for (int row = 0; row < NumRows; row++)
            {
                if (newState[moveColumn, row] == 0)
                {
                    newState[moveColumn, row] = discColor;
                    moveWasLegal = true;
                    break;
                }
            }

            if (!moveWasLegal) throw new InvalidOperationException($"Illegal move attempted for column {moveColumn}");

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

            for (int c = 0; c < NumColumns; c++)
            {
                if (board[c, NumRows - 1] == 0)
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
        protected decimal EvaluateBoardState(int[,] boardState)
        {
            /*
             * For both colors scan the entire board.
             * From each space reach out in scorable
             * directions adding on more value for free
             * spaces that could be used to score,
             * even more value for same color discs
             * and breaking on opponents blocking
             * dics.
             */
            var ret = 0.0m;

            for (int c = 0; c < NumColumns; c++)
            {
                for (int r = 0; r < NumRows; r++)
                {
                    if (boardState[c, r] != 0)
                    {
                        ret += ScorePossibles(boardState, boardState[c, r], c, r);
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Return the board positional score based on scoring
        /// potentials of both sides.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="checkColor"></param>
        /// <param name="discColumn"></param>
        /// <param name="discRow"></param>
        /// <param name="val"></param>
        private decimal ScorePossibles(int[,] board, int checkColor, int discColumn, int discRow)
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
             */

            // The total number of possible open scoring combinations 
            // that the checked disc participates.
            int participatedPossibles = 0;
            decimal participatedValue = 0.25m;
            int oppositeColor = LogicalBoardHelpers.ChangeTurnColor(checkColor);

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
                    if (InBounds(discColumn + ch + trace, discRow))
                    {
                        if (board[discColumn + ch + trace, discRow] == oppositeColor)
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
                    participatedPossibles++;
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
                    if (InBounds(discColumn, discRow + rh + trace))
                    {
                        if (board[discColumn, discRow + rh + trace] == oppositeColor)
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
                    participatedPossibles++;
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
                    if (InBounds(discColumn + ur + trace, discRow + ur + trace))
                    {
                        if (board[discColumn + ur + trace, discRow + ur + trace] == oppositeColor)
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
                    if (InBounds(discColumn + (-1 * (ul + trace)), discRow + ul + trace))
                    {
                        if (board[discColumn + (-1 * (ul + trace)), discRow + ul + trace] == oppositeColor)
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

            // TODO double check this if we ever move to a negamax algo
            return (participatedValue * participatedPossibles);
        }

        // TODO
        // Notes while debugging:
        // the terminal check is short circuiting those level evaluations and so the first
        // node at a terminal level will return it's score
        private int MinimaxCutoffSearch(Node node)
        {
            //decimal alpha = decimal.MinValue;
            //decimal beta = decimal.MaxValue;

            int maxDepth = 0;

            // TODO change this based on the AI's color
            //var scoredColumn = MinValue(n, ref alpha, ref beta, maxDepth);
            //var scoredColumn = MinValue(node, maxDepth);

            // for all actions return min value of the result of the action
            var minMove = decimal.MaxValue;
            var movedColumn = -1;

            // WOOHOO!! after more reading I finally got a much better understanding of
            // how a minimax search should be working.
            // left off here

            foreach (int openMove in GetOpenColumns(node.BoardDiscState))
            {
                int[,] newState = GenerateBoardState(openMove, AiColor, node.BoardDiscState);
                Node child = new Node(newState, openMove, AiColor);

                var maxMove = MinValue(child, maxDepth);

                if (maxMove.PositionScore < minMove)
                {
                    minMove = maxMove.PositionScore;
                    movedColumn = openMove;
                }
            }

            return movedColumn;
        }

        private ScoredMoveIndex MaxValue(Node n, ref decimal alpha, ref decimal beta, int depth)
        {
            decimal maxVal = decimal.MinValue;
            int ix = -1;
            int colorMoved = LogicalBoardHelpers.ChangeTurnColor(n.ColorMoved);
            
            // TODO terminal check
            if (depth <= 0)
            {
                decimal tempMaxVal = decimal.MinValue;

                foreach (int openTerminalMove in GetOpenColumns(n.BoardDiscState))
                {
                    int[,] newState = GenerateBoardState(openTerminalMove, colorMoved, n.BoardDiscState);
                    decimal newStateScore = EvaluateBoardState(newState);
                    Node child = new Node(newState, openTerminalMove, colorMoved, newStateScore);

                    Console.WriteLine($"Terminal Depth. Column: {openTerminalMove} Score: {newStateScore}");
                    if (tempMaxVal < newStateScore)
                    {
                        tempMaxVal = newStateScore;
                        ix = openTerminalMove;
                    }
                }

                Console.WriteLine($"Terminal returned. Column: {ix} Score: {tempMaxVal}");
                return new ScoredMoveIndex(tempMaxVal, ix);
            }

            foreach (int openMove in GetOpenColumns(n.BoardDiscState))
            {
                int[,] newState = GenerateBoardState(openMove, colorMoved, n.BoardDiscState);
                decimal newStateScore = EvaluateBoardState(newState);
                Node child = new Node(newState, openMove, colorMoved, newStateScore);

                Console.WriteLine($"Depth: {depth} Column: {openMove} Score: {newStateScore}");

                var minVal = MinValue(child, ref alpha, ref beta, depth - 1);

                if (minVal.PositionScore > maxVal)
                {
                    maxVal = minVal.PositionScore;
                    ix = openMove;
                }

                if (maxVal >= beta)
                {
                    return new ScoredMoveIndex(maxVal, ix);
                }

                alpha = Math.Max(alpha, maxVal);
            }

            return new ScoredMoveIndex(maxVal, ix);
        }

        private ScoredMoveIndex MinValue(Node n, ref decimal alpha, ref decimal beta, int depth)
        {
            decimal minVal = decimal.MaxValue;
            int ix = -1;
            int colorMoved = LogicalBoardHelpers.ChangeTurnColor(n.ColorMoved);

            if (depth <= 0)
            {
                decimal tempMinVal = decimal.MaxValue;

                foreach (int openTerminalMove in GetOpenColumns(n.BoardDiscState))
                {
                    int[,] newState = GenerateBoardState(openTerminalMove, colorMoved, n.BoardDiscState);
                    decimal newStateScore = EvaluateBoardState(newState);
                    Node child = new Node(newState, openTerminalMove, colorMoved, newStateScore);

                    Console.WriteLine($"Terminal Depth. Column: {openTerminalMove} Score: {newStateScore}");
                    if (tempMinVal > newStateScore)
                    {
                        tempMinVal = newStateScore;
                        ix = openTerminalMove;
                    }
                }

                Console.WriteLine($"Terminal returned. Column: {ix} Score: {tempMinVal}");
                return new ScoredMoveIndex(tempMinVal, ix);
            }


            foreach (int openMove in GetOpenColumns(n.BoardDiscState))
            {
                int[,] newState = GenerateBoardState(openMove, colorMoved, n.BoardDiscState);
                decimal newStateScore = EvaluateBoardState(newState);
                Node child = new Node(newState, openMove, colorMoved, newStateScore);

                Console.WriteLine($"Depth: {depth} Column: {openMove} Score: {newStateScore}");

                var maxMove = MaxValue(child, ref alpha, ref beta, depth - 1);

                if (maxMove.PositionScore < minVal)
                {
                    minVal = maxMove.PositionScore;
                    ix = openMove;
                }

                if (minVal <= alpha)
                {
                    return new ScoredMoveIndex(minVal, openMove);
                }

                beta = Math.Min(beta, minVal);
            }
            
            return new ScoredMoveIndex(minVal, ix);
        }

        private ScoredMoveIndex MaxValue(Node node, int depth)
        {
            // TODO make this check and stop terminal states as well
            if (depth <= 0)
            {
                decimal tempMaxVal = EvaluateBoardState(node.BoardDiscState);
                return new ScoredMoveIndex(tempMaxVal, node.ColumnMoved);
            }

            decimal maxVal = decimal.MinValue;
            int ix = -1;
            int colorMoved = LogicalBoardHelpers.ChangeTurnColor(node.ColorMoved);

            foreach (int openMove in GetOpenColumns(node.BoardDiscState))
            {
                int[,] newState = GenerateBoardState(openMove, colorMoved, node.BoardDiscState);
                Node child = new Node(newState, openMove, colorMoved);

                var minVal = MinValue(child, depth - 1);

                if (minVal.PositionScore > maxVal)
                {
                    maxVal = minVal.PositionScore;
                    ix = openMove;
                }
            }

            return new ScoredMoveIndex(maxVal, ix);
        }

        private ScoredMoveIndex MinValue(Node node, int depth)
        {
            if (depth <= 0)
            {
                var tempMinVal = EvaluateBoardState(node.BoardDiscState);
                return new ScoredMoveIndex(tempMinVal, node.ColumnMoved);
            }

            decimal minVal = decimal.MaxValue;
            int ix = -1;
            int colorMoved = LogicalBoardHelpers.ChangeTurnColor(node.ColorMoved);

            foreach (int openMove in GetOpenColumns(node.BoardDiscState))
            {
                int[,] newState = GenerateBoardState(openMove, colorMoved, node.BoardDiscState);
                Node child = new Node(newState, openMove, colorMoved);

                var maxMove = MaxValue(child, depth - 1);

                if (maxMove.PositionScore < minVal)
                {
                    minVal = maxMove.PositionScore;
                    ix = openMove;
                }
            }

            return new ScoredMoveIndex(minVal, ix);
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
    }
}
