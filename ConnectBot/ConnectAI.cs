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
            //var aiWinningMove = FindKillerMove(GameBoard, AiColor);

            //if (aiWinningMove.HasWinner)
            //    return aiWinningMove.Column;

            //// Ensure we block the opponents winning moves
            //var opponentWinningMove = FindKillerMove(GameBoard, OpponentColor);

            //if (opponentWinningMove.HasWinner)
            //    return opponentWinningMove.Column;

            //int retMove = MinimaxCutoffSearch(GameBoard);

            var alpha = decimal.MinValue;
            var beta = decimal.MaxValue;


            var scoredRetMove = AlphaBetaSearch(GameBoard, 5, ref alpha, ref beta, AiColor == DiscColor.Black);

            Console.WriteLine($"Best Score found was: {scoredRetMove.Score}");

            return scoredRetMove.Column;
            //return retMove;
        }

        public decimal EndGameScore(DiscColor disc, int depth)
        {
            if (disc == DiscColor.None)
                throw new ArgumentException("The disc cannot be None", nameof(disc));

            if (disc == DiscColor.Black)
                return 1000000.0m;

            return -1000000.0m;
            //if (disc == DiscColor.Black)
            //    return 42.00m - depth;

            //return -42.00m + depth;
        }


        /*
         function alphabeta(node, depth, α, β, maximizingPlayer) is
            if depth = 0 or node is a terminal node then
                return the heuristic value of node
            if maximizingPlayer then
                value := −∞
                for each child of node do
                    value := max(value, alphabeta(child, depth − 1, α, β, FALSE))
                    α := max(α, value)
                    if α ≥ β then
                        break (* β cut-off *)
                return value
            else
                value := +∞
                for each child of node do
                    value := min(value, alphabeta(child, depth − 1, α, β, TRUE))
                    β := min(β, value)
                    if α ≥ β then
                        break (* α cut-off *)
                return value
         */
        private ScoredReturnColumn AlphaBetaSearch(BitBoard board, int depth, ref decimal alpha, ref decimal beta, bool maximizingPlayer)
        {
            var openColumns = GetOpenColumns(in board);
            var winner = CheckVictory(in board);

            if (depth == 0 ||
                openColumns.Count == 0 ||
                winner != DiscColor.None)
            {
                if (maximizingPlayer)
                    return new ScoredReturnColumn(-1, EvaluateBoardState(in board, DiscColor.Black));
                else
                    return new ScoredReturnColumn(-1, EvaluateBoardState(in board, DiscColor.Red));
            }
            
            if (maximizingPlayer)
            {
                var value = decimal.MinValue;
                var bestCol = -1;

                foreach (var openColumn in openColumns)
                {
                    var newBoard = BitBoardMove(in board, openColumn, DiscColor.Black);
                    var newBest = Math.Max(value, AlphaBetaSearch(newBoard, depth - 1, ref alpha, ref beta, false).Score);

                    if (newBest > value)
                    {
                        value = newBest;
                        bestCol = openColumn;
                    }

                    alpha = Math.Max(alpha, value);

                    if (alpha >= beta)
                        break;
                }

                return new ScoredReturnColumn(bestCol, value);
            }
            else
            {
                var value = decimal.MaxValue;
                var bestCol = -1;

                foreach (var openColumn in openColumns)
                {
                    var newBoard = BitBoardMove(in board, openColumn, DiscColor.Red);
                    var newBest = Math.Min(value, AlphaBetaSearch(newBoard, depth - 1, ref alpha, ref beta, true).Score);

                    if (newBest < value)
                    {
                        value = newBest;
                        bestCol = openColumn;
                    }

                    beta = Math.Min(beta, value);

                    if (alpha >= beta)
                        break;
                }

                return new ScoredReturnColumn(bestCol, value);
            }
        }

        /// <summary>
        /// Min max searching algorithm with defined cutoff depth.
        /// </summary>
        /// <returns>The column that will be moved played in.</returns>
        private int MinimaxCutoffSearch(BitBoard board)
        {
            int maxDepth = 5;

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
            //var openColumns = GetOptimalColumns(in board, OpponentColor);

            // Drawn game
            if (openColumns.Count == 0)
                return 0.0m;

            // TODO is this obsolete then?
            var winner = CheckVictory(in board);
            if (winner != DiscColor.None)
            {

                return EndGameScore(winner, depth);
                //return EvaluateBoardState(in board, movingColor);
            }

            if (depth <= 0)
                // TODO this should represent a drawn game, we may want to
                // return something other than an evaluation here
                //openColumns.Count == 0)
            {
                //throw new InvalidOperationException("Should not reach depth");
                var retScore = EvaluateBoardState(in board, movingColor);
                //Console.WriteLine($"DEPTH: Evaluated at depth {depth} for board with Score {retScore}:");
                //Console.WriteLine(GetPrettyPrint(in board));
                return retScore;
            }


            // Win and return immediately if possible
            //var winningMove = FindKillerMove(in board, movingColor);
            //if (winningMove.HasWinner &&
            //    winningMove.Winner == movingColor)
            //{
            //    var winningBoard = BitBoardMove(in board, winningMove.Column, movingColor);
            //    //var winningScore = EvaluateBoardState(in winningBoard);
            //    var winningScore = EndGameScore(winningMove.Winner, depth);

            //    alphaBeta.Alpha = Math.Max(alphaBeta.Alpha, winningScore);

            //    //Console.WriteLine($"WINNER: Evaluated at depth {depth} for board with Score {winningScore}:");
            //    //Console.WriteLine(GetPrettyPrint(in winningBoard));
                
            //    return winningScore;
            //}

            //// Stop the opponent from winning if possible
            //var oppWinningMove = FindKillerMove(in board, AiColor);
            //if (oppWinningMove.HasWinner)
            //{
            //    var stopWinningBoard = BitBoardMove(in board, oppWinningMove.Column, OpponentColor);
            //    //var stopWinningScore = EvaluateBoardState(in stopWinningBoard);
            //    var stopWinningScore = EndGameScore(oppWinningMove.Winner, depth);

            //    alphaBeta.Alpha = Math.Max(alphaBeta.Alpha, stopWinningScore);

            //    return stopWinningScore;
            //}

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
            //var openColumns = GetOptimalColumns(in board, AiColor);

            // Drawn game
            if (openColumns.Count == 0)
                return 0.0m;

            var winner = CheckVictory(in board);
            if (winner != DiscColor.None)
            {

                return EndGameScore(winner, depth);
                //return EvaluateBoardState(in board, movingColor);
            }

            if (depth <= 0)
                //openColumns.Count == 0)
            {
                //throw new InvalidOperationException("Should not reach depth");
                var retScore = EvaluateBoardState(in board, movingColor);
                //Console.WriteLine($"DEPTH: Evaluated at depth {depth} for board with Score {retScore}:");
                //Console.WriteLine(GetPrettyPrint(in board));
                return retScore;
            }


            // Win and return immediately if possible
            //var winningMove = FindKillerMove(in board, movingColor);
            //if (winningMove.HasWinner &&
            //    winningMove.Winner == movingColor)
            //{
            //    var winningBoard = BitBoardMove(in board, winningMove.Column, movingColor);
            //    //var winningScore = EvaluateBoardState(in winningBoard);
            //    var winningScore = EndGameScore(winningMove.Winner, depth);

            //    alphaBeta.Beta = Math.Min(alphaBeta.Beta, winningScore);

            //    //Console.WriteLine($"Evaluated at depth {depth} for board with Score {winningScore}:");
            //    //Console.WriteLine(GetPrettyPrint(in winningBoard));

            //    return winningScore;
            //}

            //// Stop the opponent from winning if possible
            //var oppWinningMove = FindKillerMove(in board, OpponentColor);
            //if (oppWinningMove.HasWinner)
            //{
            //    var stopWinningBoard = BitBoardMove(in board, oppWinningMove.Column, AiColor);
            //    //var stopWinningScore = EvaluateBoardState(in stopWinningBoard);
            //    var stopWinningScore = EndGameScore(oppWinningMove.Winner, depth);

            //    alphaBeta.Beta = Math.Min(alphaBeta.Beta, stopWinningScore);

            //    return stopWinningScore;
            //}

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
