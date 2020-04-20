using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static ConnectBot.BitBoardHelpers;
using static ConnectBot.LogicalBoardHelpers;

namespace ConnectBot
{
    public class ConnectAI
    {
        protected BitBoard GameBoard { get; set; }

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
        /// Constructor.
        /// </summary>
        /// <param name="color">Color for AI to play.</param>
        public ConnectAI(DiscColor color)
        {
            AiColor = color;
            OpponentColor = ChangeTurnColor(AiColor);
        }

        public void UpdateBoard(BitBoard board)
        {
            GameBoard = board;
        }

        readonly struct ScoredReturnColumn
        {
            public readonly int Column;
            public readonly decimal Score;

            public ScoredReturnColumn(int col, decimal score)
            {
                Column = col;
                Score = score;
            }
        }

        public async Task<int> Move()
        {
            // Look for wins before performing in depth searches
            var aiWinningMove = FindKillerMove(GameBoard, AiColor);

            if (aiWinningMove.HasWinner)
                return aiWinningMove.Column;

            // Ensure we block the opponents winning moves
            var opponentWinningMove = FindKillerMove(GameBoard, OpponentColor);

            if (opponentWinningMove.HasWinner)
                return opponentWinningMove.Column;

            int retMove = MinimaxCutoffSearch(GameBoard);

            return retMove;
        }

        public decimal EndGameScore(DiscColor disc, int depth)
        {
            // what to return here!?!?
            if (disc == DiscColor.None)
                throw new ArgumentException("The disc cannot be None", nameof(disc));

            if (disc == DiscColor.Black)
                return 1000000.0m;

            return -1000000.0m;
        }

        /// <summary>
        /// Min max searching algorithm with defined cutoff depth.
        /// </summary>
        /// <returns>The column that will be moved played in.</returns>
        private int MinimaxCutoffSearch(BitBoard board)
        {
            int maxDepth = 12;

            // TODO change this based on the AI's color when color selection menu is used
            // for all actions return min value of the result of the action
            var minimumMoveValue = decimal.MaxValue;
            var maximumMoveValue = decimal.MinValue;
            var openMoveValue = 0.0m;
            var movedColumn = -1;
            var alphaBeta = new AlphaBeta();
            var nodeCounter = new NodeCounter();
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (int openMove in GetOpenColumns(in board))
            {
                var newState = BitBoardMove(in board, openMove, AiColor);

                // prompt for opponents first move in the searching
                if (AiColor == DiscColor.Red)
                {
                    openMoveValue = MaxValue(newState, maxDepth, alphaBeta, nodeCounter, OpponentColor);

                    Console.WriteLine($"Column {openMove} had a score of {openMoveValue}.");

                    if (openMoveValue < minimumMoveValue)
                    {
                        minimumMoveValue = openMoveValue;
                        movedColumn = openMove;
                    }
                }
                else
                {
                    openMoveValue = MinValue(newState, maxDepth, alphaBeta, nodeCounter, OpponentColor);

                    Console.WriteLine($"Column {openMove} had a score of {openMoveValue}.");

                    if (openMoveValue > maximumMoveValue)
                    {
                        maximumMoveValue = openMoveValue;
                        movedColumn = openMove;
                    }
                }
            }

            stopWatch.Stop();
            var elapsed = stopWatch.Elapsed;

            var elapsedTime = String.Format("Searched for {0:00}.{1:0000} seconds",
                elapsed.Seconds, elapsed.Milliseconds);
            Console.WriteLine(elapsedTime);

            Console.WriteLine($"Column {movedColumn} was chosen.");
            Console.WriteLine($"{nodeCounter.TotalNodes} were explored.");

            return movedColumn;
        }

        private decimal MaxValue(
            BitBoard board,
            int depth,
            AlphaBeta alphaBeta,
            NodeCounter nodeCounter,
            DiscColor movingColor)
        {
            nodeCounter.Increment();

            var openColumns = GetOpenColumns(in board);
            
            // Drawn game
            if (openColumns.Count == 0)
                return 0.0m;

            if (depth == 0 ||
                CheckVictory(in board) != DiscColor.None)
            {
                return EvaluateBoardState(in board, movingColor);
            }

            // Win and return immediately if possible
            var winningMove = FindKillerMove(in board, movingColor);
            if (winningMove.HasWinner &&
                winningMove.Winner == movingColor)
            {
                var winningBoard = BitBoardMove(in board, winningMove.Column, movingColor);
                var winningScore = EvaluateBoardState(in winningBoard, movingColor);

                alphaBeta.Alpha = Math.Max(alphaBeta.Alpha, winningScore);

                return winningScore;
            }

            // Stop the opponent from winning if possible
            var oppWinningMove = FindKillerMove(in board, AiColor);
            if (oppWinningMove.HasWinner)
            {
                var stopWinningBoard = BitBoardMove(in board, oppWinningMove.Column, OpponentColor);
                var stopWinningScore = EvaluateBoardState(in stopWinningBoard, movingColor);

                alphaBeta.Alpha = Math.Max(alphaBeta.Alpha, stopWinningScore);

                return stopWinningScore;
            }

            decimal maximumMoveValue = decimal.MinValue;

            foreach (int openMove in openColumns)
            {
                var newState = BitBoardMove(in board, openMove, movingColor);

                maximumMoveValue = Math.Max(
                    maximumMoveValue,
                    MinValue(newState, depth - 1, alphaBeta, nodeCounter, ChangeTurnColor(movingColor)));

                if (maximumMoveValue >= alphaBeta.Beta)
                    return maximumMoveValue;

                alphaBeta.Alpha = Math.Max(alphaBeta.Alpha, maximumMoveValue);
            }

            return maximumMoveValue;
        }

        private decimal MinValue(
            BitBoard board,
            int depth,
            AlphaBeta alphaBeta,
            NodeCounter nodeCounter,
            DiscColor movingColor)
        {
            nodeCounter.Increment();

            var openColumns = GetOpenColumns(in board);
            
            // Drawn game
            if (openColumns.Count == 0)
                return 0.0m;

            if (depth == 0 ||
                CheckVictory(in board) != DiscColor.None)
            {
                return EvaluateBoardState(in board, movingColor);
            }

            // Win and return immediately if possible
            var winningMove = FindKillerMove(in board, movingColor);
            if (winningMove.HasWinner &&
                winningMove.Winner == movingColor)
            {
                var winningBoard = BitBoardMove(in board, winningMove.Column, movingColor);
                var winningScore = EvaluateBoardState(in winningBoard, movingColor);

                alphaBeta.Beta = Math.Min(alphaBeta.Beta, winningScore);

                return winningScore;
            }

            // Stop the opponent from winning if possible
            var oppWinningMove = FindKillerMove(in board, OpponentColor);
            if (oppWinningMove.HasWinner)
            {
                var stopWinningBoard = BitBoardMove(in board, oppWinningMove.Column, AiColor);
                var stopWinningScore = EvaluateBoardState(in stopWinningBoard, movingColor);

                alphaBeta.Beta = Math.Min(alphaBeta.Beta, stopWinningScore);

                return stopWinningScore;
            }

            decimal minimumMoveValue = decimal.MaxValue;

            foreach (int openMove in openColumns)
            {
                var newState = BitBoardMove(in board, openMove, movingColor);

                minimumMoveValue = Math.Min(
                    minimumMoveValue,
                    MaxValue(newState, depth - 1, alphaBeta, nodeCounter, ChangeTurnColor(movingColor)));

                if (minimumMoveValue <= alphaBeta.Alpha)
                    return minimumMoveValue;

                alphaBeta.Beta = Math.Min(alphaBeta.Beta, minimumMoveValue);
            }

            return minimumMoveValue;
        }
    }
}
