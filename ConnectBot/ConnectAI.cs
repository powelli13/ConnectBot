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
            int maxDepth = 7;

            var openMoveValue = 0.0m;
            var movedColumn = -1;

            var alpha = decimal.MinValue;
            var beta = decimal.MaxValue;

            var nodeCounter = new NodeCounter();
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            if (AiColor == DiscColor.Red)
            {
                (openMoveValue, movedColumn) = MinValue(board, maxDepth, alpha, beta, nodeCounter, AiColor);
            }
            else
            {
                (openMoveValue, movedColumn) = MaxValue(board, maxDepth, alpha, beta, nodeCounter, AiColor);
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

        private (decimal BoardScore, int Column) MaxValue(
            BitBoard board,
            int depth,
            decimal alpha,
            decimal beta,
            NodeCounter nodeCounter,
            DiscColor movingColor)
        {
            nodeCounter.Increment();

            // TODO double check if -1 should be used
            // I didn't see any problems with this during some test games, may require more vetting though
            var openColumns = GetOpenColumns(in board);

            // Drawn game
            if (openColumns.Count == 0)
                return (0.0m, -1);

            if (depth == 0 ||
                CheckVictory(in board) != DiscColor.None)
            {
                return (EvaluateBoardState(in board, movingColor), -1);
            }

            // Win and return immediately if possible
            var winningMove = FindKillerMove(in board, movingColor);
            if (winningMove.HasWinner &&
                winningMove.Winner == movingColor)
            {
                var winningBoard = BitBoardMove(in board, winningMove.Column, movingColor);
                var winningScore = EvaluateBoardState(in winningBoard, movingColor);

                return (winningScore, winningMove.Column);
            }

            // Stop the opponent from winning if possible
            var oppWinningMove = FindKillerMove(in board, AiColor);
            if (oppWinningMove.HasWinner)
            {
                var stopWinningBoard = BitBoardMove(in board, oppWinningMove.Column, OpponentColor);
                var stopWinningScore = EvaluateBoardState(in stopWinningBoard, movingColor);

                return (stopWinningScore, oppWinningMove.Column);
            }

            decimal maximumMoveValue = decimal.MinValue;
            int movedColumn = -1;

            foreach (int openMove in openColumns)
            {
                var newState = BitBoardMove(in board, openMove, movingColor);

                var childMinValue = MinValue(newState, depth - 1, alpha, beta, nodeCounter, ChangeTurnColor(movingColor)).BoardScore;
                if (childMinValue > maximumMoveValue)
                {
                    maximumMoveValue = childMinValue;
                    movedColumn = openMove;
                }

                if (maximumMoveValue >= beta)
                    return (maximumMoveValue, openMove);

                alpha = Math.Max(alpha, maximumMoveValue);
            }

            return (maximumMoveValue, movedColumn);
        }

        private (decimal BoardScore, int Column) MinValue(
            BitBoard board,
            int depth,
            decimal alpha,
            decimal beta,
            NodeCounter nodeCounter,
            DiscColor movingColor)
        {
            nodeCounter.Increment();

            var openColumns = GetOpenColumns(in board);

            // TODO I think these columns are safe but not certain
            // I didn't see any problems with this during some test games, may require more vetting though
            // Drawn game
            if (openColumns.Count == 0)
                return (0.0m, -1);

            if (depth == 0 ||
                CheckVictory(in board) != DiscColor.None)
            {
                return (EvaluateBoardState(in board, movingColor), -1);
            }

            // Win and return immediately if possible
            var winningMove = FindKillerMove(in board, movingColor);
            if (winningMove.HasWinner &&
                winningMove.Winner == movingColor)
            {
                var winningBoard = BitBoardMove(in board, winningMove.Column, movingColor);
                var winningScore = EvaluateBoardState(in winningBoard, movingColor);

                return (winningScore, winningMove.Column);
            }

            // Stop the opponent from winning if possible
            var oppWinningMove = FindKillerMove(in board, OpponentColor);
            if (oppWinningMove.HasWinner)
            {
                var stopWinningBoard = BitBoardMove(in board, oppWinningMove.Column, AiColor);
                var stopWinningScore = EvaluateBoardState(in stopWinningBoard, movingColor);

                return (stopWinningScore, oppWinningMove.Column);
            }

            decimal minimumMoveValue = decimal.MaxValue;
            int movedColumn = -1;

            foreach (int openMove in openColumns)
            {
                var newState = BitBoardMove(in board, openMove, movingColor);

                var childMaxValue = MaxValue(newState, depth - 1, alpha, beta, nodeCounter, ChangeTurnColor(movingColor)).BoardScore;

                if (childMaxValue < minimumMoveValue)
                {
                    minimumMoveValue = childMaxValue;
                    movedColumn = openMove;
                }

                if (minimumMoveValue <= alpha)
                    return (minimumMoveValue, openMove);

                beta = Math.Min(beta, minimumMoveValue);
            }

            return (minimumMoveValue, movedColumn);
        }
    }
}
