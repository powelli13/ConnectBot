using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConnectBot
{
    public class ConnectAI
    {
        /// <summary>
        /// Multidimensional array of integers representing the board
        /// indexed by [column, row]. Lowest column index is left most 
        /// on the screen. Lowest row index is lowest in the column stack.
        /// </summary>
        protected DiscColor[,] GameDiscs = new DiscColor[LogicalBoardHelpers.NUM_COLUMNS, LogicalBoardHelpers.NUM_ROWS];

        protected DiscColor AiColor { get; set; }
        protected DiscColor OpponentColor { get; set; }

        /// <summary>
        /// used for testing
        /// </summary>
        protected Random Rando { get; set; }

        /// <summary>
        /// Used for counting the total number of nodes that were explored
        /// during a given search iteration. Used for diagnostics and performance measuring.
        /// </summary>
        class NodeCounter
        {
            public int TotalNodes { get; private set; }

            public NodeCounter()
            {
                TotalNodes = 0;
            }

            public void Increment()
            {
                TotalNodes++;
            }
        }

        /// <summary>
        /// Used to store the window edges Alpha and Beta
        /// when performing an Alpha Beta search.
        /// </summary>
        private class AlphaBeta
        {
            public decimal Alpha { get; set; }
            public decimal Beta { get; set; }

            public AlphaBeta(
                decimal alpha = decimal.MinValue, 
                decimal beta = decimal.MaxValue)
            {
                Alpha = alpha;
                Beta = beta;
            }
        }

        #region Class : Node
        /// <summary>
        /// Node of the move tree stores a board state, 
        /// turn disc and column for move that generated this state
        /// and evaluation score of the board state.
        /// </summary>
        private class Node
        {
            public DiscColor[,] BoardDiscState { get; set; }

            /// <summary>
            /// Color to next move at that node.
            /// </summary>
            public DiscColor ColorMoved { get; set; }

            /// <summary>
            /// Column that was moved in to generate this node.
            /// </summary>
            public int ColumnMoved { get; set; }

            public decimal PositionalScore { get; set; }

            public List<Node> Children { get; set; }
            
            public Node(DiscColor[,] board, int column, DiscColor turn, decimal score = 0.0m)
            {
                BoardDiscState = board;
                ColumnMoved = column;
                PositionalScore = score;
                ColorMoved = turn;// TODO will this pass the turn that just moved?
                Children = new List<Node>();
            }
        }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="color">Color for AI to play.</param>
        public ConnectAI(DiscColor color)
        {
            AiColor = color;
            OpponentColor = LogicalBoardHelpers.ChangeTurnColor(AiColor);

            Rando = new Random();
        }
        
        /// <summary>
        /// Update internal board state.
        /// </summary>
        /// <param name="newBoard">Array representing updated board state.</param>
        /// <param name="lastMove">Last move made on the new board. Negative one signifies first board update.</param>
        public void UpdateBoard(DiscColor[,] newBoard, int lastMove)
        {
            for (int c = 0; c < LogicalBoardHelpers.NUM_COLUMNS; c++)
            {
                for (int r = 0; r < LogicalBoardHelpers.NUM_ROWS; r++)
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
            //AISelfTest();

            // Look for wins before performing in depth searches
            int killerMove = FindKillerMove(GameDiscs);

            if (killerMove != -1) 
                return killerMove;

            Node n = new Node(GameDiscs, 0, AiColor, 0.0m);
            
            int retMove = MinimaxCutoffSearch(n);

            return retMove;
        }

        protected int FindKillerMove(DiscColor[,] boardState)
        {
            int move = -1;

            foreach (var openColumn in LogicalBoardHelpers.GetOpenColumns(boardState))
            {
                // If the AI could win return that immediately.
                var movedBoard = GenerateBoardState(openColumn, AiColor, boardState);
                var winner = LogicalBoardHelpers.CheckVictory(movedBoard);

                if (winner == AiColor)
                    return openColumn;

                // If the opponent could win be sure to check the rest in case the AI could win
                // at a column further down.
                var oppMovedBoard = GenerateBoardState(openColumn, OpponentColor, boardState);
                var oppWinner = LogicalBoardHelpers.CheckVictory(oppMovedBoard);

                if (oppWinner == OpponentColor)
                    move = openColumn;
            }

            return move;
        }

        protected DiscColor[,] GenerateBoardState(int moveColumn, DiscColor discColor, DiscColor[,] currentBoard)
        {
            DiscColor[,] newState = new DiscColor[LogicalBoardHelpers.NUM_COLUMNS, LogicalBoardHelpers.NUM_ROWS];

            for (int c = 0; c < LogicalBoardHelpers.NUM_COLUMNS; c++)
            {
                for (int r = 0; r < LogicalBoardHelpers.NUM_ROWS; r++)
                {
                    newState[c, r] = currentBoard[c, r];
                }
            }

            bool moveWasLegal = false;
            for (int row = 0; row < LogicalBoardHelpers.NUM_ROWS; row++)
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
        /// Evalues the board state and returns a real number score
        /// postive numbers favor black, negative favor red.
        /// Every disc is worth one. Adjacent discs of the same color 
        /// add .125 if adjacent discs help to form a line they are
        /// worth .25
        /// </summary>
        /// <param name="boardState"></param>
        /// <returns></returns>
        /// TODO there has to be a better way to structure board than all this rigorous checking
        protected decimal EvaluateBoardState(DiscColor[,] boardState)
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

            for (int c = 0; c < LogicalBoardHelpers.NUM_COLUMNS; c++)
            {
                for (int r = 0; r < LogicalBoardHelpers.NUM_ROWS; r++)
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
        private decimal ScorePossibles(DiscColor[,] board, DiscColor checkColor, int discColumn, int discRow)
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
            DiscColor oppositeColor = LogicalBoardHelpers.ChangeTurnColor(checkColor);

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
                    if (LogicalBoardHelpers.InBounds(discColumn + ch + trace, discRow))
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
                    if (LogicalBoardHelpers.InBounds(discColumn, discRow + rh + trace))
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
                    if (LogicalBoardHelpers.InBounds(discColumn + ur + trace, discRow + ur + trace))
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
                    if (LogicalBoardHelpers.InBounds(discColumn + (-1 * (ul + trace)), discRow + ul + trace))
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

        /// <summary>
        /// Min max searching algorithm with defined cutoff depth.
        /// </summary>
        /// <param name="node">Node representing the current board state.</param>
        /// <returns>The column that will be moved played in.</returns>
        private int MinimaxCutoffSearch(Node node)
        {
            int maxDepth = 7;

            // TODO change this based on the AI's color
            // for all actions return min value of the result of the action
            var minimumMoveValue = decimal.MaxValue;
            var movedColumn = -1;
            var alphaBeta = new AlphaBeta();
            var nodeCounter = new NodeCounter();

            foreach (int openMove in LogicalBoardHelpers.GetOpenColumns(node.BoardDiscState))
            {
                DiscColor[,] newState = GenerateBoardState(openMove, AiColor, node.BoardDiscState);
                Node child = new Node(newState, openMove, AiColor);

                var openMoveValue = MinValue(child, maxDepth, alphaBeta, nodeCounter);

                if (openMoveValue < minimumMoveValue)
                {
                    minimumMoveValue = openMoveValue;
                    movedColumn = openMove;
                }
            }

            Console.WriteLine($"{nodeCounter.TotalNodes} were explored.");

            return movedColumn;
        }

        private decimal MaxValue(
            Node node, 
            int depth, 
            AlphaBeta alphaBeta,
            NodeCounter nodeCounter)
        {
            nodeCounter.Increment();

            var openColumns = LogicalBoardHelpers.GetOpenColumns(node.BoardDiscState);

            // TODO make this check and stop terminal states as well
            if (depth <= 0 ||
                // this should represent a drawn game, we may want to
                // return something other than an evaluation here
                openColumns.Count == 0)
            {
                return EvaluateBoardState(node.BoardDiscState);
            }

            decimal maximumMoveValue = decimal.MinValue;
            DiscColor colorMoved = LogicalBoardHelpers.ChangeTurnColor(node.ColorMoved);

            foreach (int openMove in openColumns)
            {
                var newState = GenerateBoardState(openMove, colorMoved, node.BoardDiscState);
                var child = new Node(newState, openMove, colorMoved);

                maximumMoveValue = Math.Max(
                    maximumMoveValue,
                    MinValue(child, depth - 1, alphaBeta, nodeCounter));

                if (maximumMoveValue >= alphaBeta.Beta)
                    return maximumMoveValue;

                alphaBeta.Alpha = Math.Max(alphaBeta.Alpha, maximumMoveValue);
            }

            return maximumMoveValue;
        }

        private decimal MinValue(
            Node node, 
            int depth,
            AlphaBeta alphaBeta,
            NodeCounter nodeCounter)
        {
            nodeCounter.Increment();

            var openColumns = LogicalBoardHelpers.GetOpenColumns(node.BoardDiscState);

            if (depth <= 0 ||
                openColumns.Count == 0)
            {
                return EvaluateBoardState(node.BoardDiscState);
            }

            decimal minimumMoveValue = decimal.MaxValue;
            DiscColor colorMoved = LogicalBoardHelpers.ChangeTurnColor(node.ColorMoved);

            foreach (int openMove in openColumns)
            {
                var newState = GenerateBoardState(openMove, colorMoved, node.BoardDiscState);
                var child = new Node(newState, openMove, colorMoved);

                minimumMoveValue = Math.Min(
                    minimumMoveValue,
                    MaxValue(child, depth - 1, alphaBeta, nodeCounter));

                if (minimumMoveValue <= alphaBeta.Alpha)
                    return minimumMoveValue;

                alphaBeta.Beta = Math.Min(alphaBeta.Beta, minimumMoveValue);
            }

            return minimumMoveValue;
        }
    }
}
