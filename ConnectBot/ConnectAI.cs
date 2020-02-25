﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ConnectBot.LogicalBoardHelpers;

namespace ConnectBot
{
    public class ConnectAI
    {
        /// <summary>
        /// Multidimensional array of integers representing the board
        /// indexed by [column, row]. Lowest column index is left most 
        /// on the screen. Lowest row index is lowest in the column stack.
        /// </summary>
        protected DiscColor[,] GameDiscs = new DiscColor[NUM_COLUMNS, NUM_ROWS];

        protected DiscColor AiColor { get; set; }
        protected DiscColor OpponentColor { get; set; }

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
        class AlphaBeta
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

        /// <summary>
        /// Node of the move tree stores a board state, 
        /// turn disc and column for move that generated this state
        /// and evaluation score of the board state.
        /// </summary>
        class Node
        {
            public DiscColor[,] BoardDiscState { get; set; }

            /// <summary>
            /// Color that moved last to create the current state in the Node.
            /// </summary>
            public DiscColor ColorLastMoved { get; set; }

            public DiscColor CurrentTurn { get; private set; }

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
                ColorLastMoved = turn;
                CurrentTurn = ChangeTurnColor(turn);
                Children = new List<Node>();
            }
        }

        /// <summary>
        /// Holds a killer (winning) move column and the
        /// disc color that move would benefit.
        /// </summary>
        class KillerMove
        {
            public bool HasWinner => Column != -1;
            public int Column { get; set; }
            public DiscColor Color { get; set; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="color">Color for AI to play.</param>
        public ConnectAI(DiscColor color)
        {
            AiColor = color;
            OpponentColor = ChangeTurnColor(AiColor);
        }
        
        /// <summary>
        /// Update internal board state.
        /// </summary>
        /// <param name="newBoard">Array representing updated board state.</param>
        /// <param name="lastMove">Last move made on the new board. Negative one signifies first board update.</param>
        public void UpdateBoard(DiscColor[,] newBoard, int lastMove)
        {
            for (int c = 0; c < NUM_COLUMNS; c++)
            {
                for (int r = 0; r < NUM_ROWS; r++)
                {
                    GameDiscs[c, r] = newBoard[c, r];
                }
            }
        }

        public async Task<int> Move()
        {
            //AISelfTest();

            // Look for wins before performing in depth searches
            var aiWinningMove = FindKillerMove(GameDiscs, AiColor);

            if (aiWinningMove.HasWinner) 
                return aiWinningMove.Column;

            // TODO consider refactoring FindKillerMove so that we only need to call once
            // Ensure we block the opponents winning moves
            var opponentWinningMove = FindKillerMove(GameDiscs, OpponentColor);

            if (opponentWinningMove.HasWinner)
                return opponentWinningMove.Column;

            Node n = new Node(GameDiscs, 0, AiColor, 0.0m);
            
            int retMove = MinimaxCutoffSearch(n);

            return retMove;
        }

        KillerMove FindKillerMove(DiscColor[,] boardState, DiscColor checkColor)
        {
            foreach (var openColumn in GetOpenColumns(boardState))
            {
                var movedBoard = GenerateBoardState(openColumn, checkColor, boardState);
                
                if (CheckVictory(movedBoard) == checkColor)
                    return new KillerMove(){
                        Column = openColumn,
                        Color = checkColor
                    };
            }

            return new KillerMove()
            {
                Column = -1,
                Color = DiscColor.None
            };
        }

        protected DiscColor[,] GenerateBoardState(int moveColumn, DiscColor discColor, DiscColor[,] currentBoard)
        {
            DiscColor[,] newState = new DiscColor[NUM_COLUMNS, NUM_ROWS];

            for (int c = 0; c < NUM_COLUMNS; c++)
            {
                for (int r = 0; r < NUM_ROWS; r++)
                {
                    newState[c, r] = currentBoard[c, r];
                }
            }

            bool moveWasLegal = false;
            for (int row = 0; row < NUM_ROWS; row++)
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
            // TODO adjust when moving to negamax
            var redPossiblesValue = CountAllPossibles(boardState, DiscColor.Red);
            var blackPossiblesValue = CountAllPossibles(boardState, DiscColor.Black);

            return blackPossiblesValue + (redPossiblesValue * -1.0m);
        }

        decimal CountAllPossibles(DiscColor[,] boardState, DiscColor checkColor)
        {
            decimal ret = PossibleHorizontals(boardState, checkColor);
            ret += PossibleVerticals(boardState, checkColor);
            ret += PossibleDiagonalRising(boardState, checkColor);
            ret += PossibleDiagonalDescending(boardState, checkColor);

            return ret;
        }

        decimal PossibleHorizontals(DiscColor[,] boardState, DiscColor checkColor)
        {
            decimal ret = 0.0m;

            for (int r = 0; r < NUM_ROWS; r++)
            {
                for (int trace = 0; trace < NUM_COLUMNS - 3; trace++)
                {
                    if (IsFourScorable(
                        checkColor,
                        boardState[trace, r],
                        boardState[trace + 1, r],
                        boardState[trace + 2, r],
                        boardState[trace + 3, r]))
                    {
                        ret += PossibleFourValue(
                            boardState[trace, r],
                            boardState[trace + 1, r],
                            boardState[trace + 2, r],
                            boardState[trace + 3, r]);
                    }
                }
            }


            return ret;
        }

        decimal PossibleVerticals(DiscColor[,] boardState, DiscColor checkColor)
        {
            decimal ret = 0.0m;

            for (int c = 0; c < NUM_COLUMNS; c++)
            {
                for (int trace = 0; trace < NUM_ROWS - 3; trace++)
                {
                    if (IsFourScorable(
                        checkColor,
                        boardState[c, trace],
                        boardState[c, trace + 1],
                        boardState[c, trace + 2],
                        boardState[c, trace + 3]))
                    {
                        ret += PossibleFourValue(
                            boardState[c, trace],
                            boardState[c, trace + 1],
                            boardState[c, trace + 2],
                            boardState[c, trace + 3]);
                    }
                }
            }

            return ret;
        }

        decimal PossibleDiagonalRising(DiscColor[,] boardState, DiscColor checkColor)
        {
            decimal ret = 0.0m;

            // for the first four columns
            // for the first three rows
            // statically check up to the right
            // note that highest index of a row is at the top
            for (int c = 0; c < NUM_COLUMNS - 3; c++)
            {
                for (int r = 0; r < NUM_ROWS - 3; r++)
                {
                    if (IsFourScorable(
                        checkColor,
                        boardState[c, r],
                        boardState[c + 1, r + 1],
                        boardState[c + 2, r + 2],
                        boardState[c + 3, r + 3]))
                    {
                        ret += PossibleFourValue(
                            boardState[c, r],
                            boardState[c + 1, r + 1],
                            boardState[c + 2, r + 2],
                            boardState[c + 3, r + 3]);
                    }
                }
            }

            return ret;
        }

        decimal PossibleDiagonalDescending(DiscColor[,] boardState, DiscColor checkColor)
        {
            decimal ret = 0.0m;

            // for first four columns
            // for top 3 rows
            // statically check down right
            for (int c = 0; c < NUM_COLUMNS - 3; c++)
            {
                for (int r = NUM_ROWS - 1; r > NUM_ROWS - 4; r--)
                {
                    if (IsFourScorable(
                        checkColor,
                        boardState[c, r],
                        boardState[c + 1, r - 1],
                        boardState[c + 2, r - 2],
                        boardState[c + 3, r - 3]))
                    {
                        ret += PossibleFourValue(
                            boardState[c, r],
                            boardState[c + 1, r - 1],
                            boardState[c + 2, r - 2],
                            boardState[c + 3, r - 3]);
                    }
                }
            }

            return ret;
        }

        bool IsFourScorable(
            DiscColor friendlyColor,
            DiscColor first,
            DiscColor second,
            DiscColor third,
            DiscColor fourth)
        {
            DiscColor opponentColor = ChangeTurnColor(friendlyColor);

            // there is an opponent disc blocking
            if (first == opponentColor ||
                second == opponentColor ||
                third == opponentColor ||
                fourth == opponentColor)
                return false;

            // there are no friendly discs
            if (first == DiscColor.None &&
                second == DiscColor.None &&
                third == DiscColor.None &&
                fourth == DiscColor.None)
                return false;

            return true;
        }

        // TODO the problem with this approach is that it doesn't
        // take into account open spaces at the ends of a connection.
        decimal PossibleFourValue(
            DiscColor first,
            DiscColor second,
            DiscColor third,
            DiscColor fourth)
        {
            int discCount = 0;

            if (first != DiscColor.None)
                discCount++;

            if (second != DiscColor.None)
                discCount++;

            if (third != DiscColor.None)
                discCount++;

            if (fourth != DiscColor.None)
                discCount++;

            switch (discCount)
            {
                case 1:
                    return 0.2m;
                case 2:
                    return 0.6m;
                case 3:
                    return 1.2m;
                // TODO is this needed with killer move checking?
                // TODO consider using some extremely large value that isn't max/min
                // for victory states and when they are discovered through this evaluation
                case 4:
                    return 1000.0m;
                default:
                    return 0.0m;
            }
        }

        // TODO consider changing this if negamax is used
        decimal EndgameStateScore(DiscColor winner)
        {
            if (winner == DiscColor.None) 
                throw new ArgumentException("Disc color to check cannot be None.", nameof(winner));

            if (winner == DiscColor.Black)
                return Decimal.MaxValue - 1.0m;

            return Decimal.MinValue + 1.0m;
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

            foreach (int openMove in GetOpenColumns(node.BoardDiscState))
            {
                DiscColor[,] newState = GenerateBoardState(openMove, AiColor, node.BoardDiscState);
                Node child = new Node(newState, openMove, AiColor);
                
                // TODO improve this use of a bool
                //bool opponentWinning = false;

                var openMoveValue = MinValue(child, maxDepth, alphaBeta, nodeCounter);
                Console.WriteLine($"Column {openMove} had a score of {openMoveValue}.");

                // TODO do more reading because this feels unnecessary. the danger of 
                // an opponent having an imminent win should be able to be captured in the heuristic

                // final depth search to ensure that a move doesn't leave the 
                // opponent with an opportunity to win
                var killer = FindKillerMove(child.BoardDiscState, OpponentColor);

                if (killer.HasWinner)
                {
                    // TODO still could probably be improved. this reflects
                    // the worst possible move but will ensure that the bot
                    // moves when every move gives the opponent a win
                    openMoveValue = decimal.MaxValue - 1.0m;
                }

                if (openMoveValue < minimumMoveValue)
                {
                    minimumMoveValue = openMoveValue;
                    movedColumn = openMove;
                }
            }

            Console.WriteLine($"Column {movedColumn} was chosen.");
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

            var openColumns = GetOpenColumns(node.BoardDiscState);

            if (depth <= 0 ||
                // TODO this should represent a drawn game, we may want to
                // return something other than an evaluation here
                openColumns.Count == 0)
            {
                return EvaluateBoardState(node.BoardDiscState);
            }

            // TODO verify that these return values are what are wanted
            var possibleWinner = CheckVictory(node.BoardDiscState);

            if (possibleWinner != DiscColor.None)
                return EndgameStateScore(possibleWinner);

            decimal maximumMoveValue = decimal.MinValue;

            foreach (int openMove in openColumns)
            {
                var newState = GenerateBoardState(openMove, node.CurrentTurn, node.BoardDiscState);
                var child = new Node(newState, openMove, node.CurrentTurn);

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

            var openColumns = GetOpenColumns(node.BoardDiscState);

            if (depth <= 0 ||
                openColumns.Count == 0)
            {
                return EvaluateBoardState(node.BoardDiscState);
            }

            // TODO verify that these return values are what are wanted
            var possibleWinner = CheckVictory(node.BoardDiscState);

            if (possibleWinner != DiscColor.None)
                return EndgameStateScore(possibleWinner);

            decimal minimumMoveValue = decimal.MaxValue;

            foreach (int openMove in openColumns)
            {
                var newState = GenerateBoardState(openMove, node.CurrentTurn, node.BoardDiscState);
                var child = new Node(newState, openMove, node.CurrentTurn);

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
