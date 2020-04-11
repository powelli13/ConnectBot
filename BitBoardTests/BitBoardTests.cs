using ConnectBot;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConnectBotTests
{
    [TestClass]
    public class BitBoardTests
    {
        [TestMethod]
        public void GetNewBoard_ShouldReturnBoardWithAllOpenColumns()
        {
            var board = BitBoardHelpers.GetNewBoard();

            Assert.IsTrue(BitBoardHelpers.IsColumnOpen(board, 0));
            Assert.IsTrue(BitBoardHelpers.IsColumnOpen(board, 1));
            Assert.IsTrue(BitBoardHelpers.IsColumnOpen(board, 2));
            Assert.IsTrue(BitBoardHelpers.IsColumnOpen(board, 3));
            Assert.IsTrue(BitBoardHelpers.IsColumnOpen(board, 4));
            Assert.IsTrue(BitBoardHelpers.IsColumnOpen(board, 5));
            Assert.IsTrue(BitBoardHelpers.IsColumnOpen(board, 6));
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]
        public void BitBoardMove_ShouldReturnFullColumn_GivenSixMovesInSameColumn(int column)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);

            Assert.IsFalse(BitBoardHelpers.IsColumnOpen(board, column));
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]
        public void BitBoardMove_ShouldReturnOpenColumn_GivenFiveMovesInSameColumn(int column)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);

            Assert.IsTrue(BitBoardHelpers.IsColumnOpen(board, column));
        }

        [TestMethod]
        public void PossibleHorizontals_ShouldReturnZero_GivenEmptyBoard()
        {
            var board = BitBoardHelpers.GetNewBoard();

            Assert.AreEqual(0.0m, BitBoardHelpers.ScorePossibleHorizontals(board, DiscColor.Red));
            Assert.AreEqual(0.0m, BitBoardHelpers.ScorePossibleHorizontals(board, DiscColor.Black));
        }

        [TestMethod]
        public void PossibleVerticals_ShouldReturnZero_GivenEmptyBoard()
        {
            var board = BitBoardHelpers.GetNewBoard();

            Assert.AreEqual(0.0m, BitBoardHelpers.ScorePossibleVerticals(board, DiscColor.Red));
            Assert.AreEqual(0.0m, BitBoardHelpers.ScorePossibleVerticals(board, DiscColor.Black));
        }

        [TestMethod]
        public void PossibleRisingDiagonals_ShouldReturnZero_GivenEmptyBoard()
        {
            var board = BitBoardHelpers.GetNewBoard();

            Assert.AreEqual(0.0m, BitBoardHelpers.ScorePossibleRisingDiagonals(board, DiscColor.Red));
            Assert.AreEqual(0.0m, BitBoardHelpers.ScorePossibleRisingDiagonals(board, DiscColor.Black));
        }

        [TestMethod]
        public void PossibleFallingDiagonals_ShouldReturnZero_GivenEmptyBoard()
        {
            var board = BitBoardHelpers.GetNewBoard();

            Assert.AreEqual(0.0m, BitBoardHelpers.ScorePossibleFallingDiagonals(board, DiscColor.Red));
            Assert.AreEqual(0.0m, BitBoardHelpers.ScorePossibleFallingDiagonals(board, DiscColor.Black));
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]
        public void CheckVictory_ShouldReturnRed_GivenRedWinningColumn(int column)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);

            var winner = BitBoardHelpers.CheckVictory(board);

            Assert.AreEqual(DiscColor.Red, winner);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]
        public void CheckVictory_ShouldReturnBlack_GivenBlackWinningColumn(int column)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Black);

            var winner = BitBoardHelpers.CheckVictory(board);

            Assert.AreEqual(DiscColor.Black, winner);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]
        public void CheckVictory_ShouldReturnNone_GivenThreeRedMovesInColumn(int column)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Red);

            var winner = BitBoardHelpers.CheckVictory(board);

            Assert.AreEqual(DiscColor.None, winner);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]
        public void CheckVictory_ShouldReturnNone_GivenThreeBlackMovesInColumn(int column)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, column, DiscColor.Black);

            var winner = BitBoardHelpers.CheckVictory(board);

            Assert.AreEqual(DiscColor.None, winner);
        }

        [DataTestMethod]
        [DataRow(0, 1, 2, 3)]
        [DataRow(1, 2, 3, 4)]
        [DataRow(2, 3, 4, 5)]
        [DataRow(3, 4, 5, 6)]
        public void CheckVictory_ShouldReturnRed_GivenFourRedMovesInRow(
            int firstColumn,
            int secondColumn,
            int thirdColumn,
            int fourthColumn)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, firstColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, secondColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, thirdColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, fourthColumn, DiscColor.Red);

            var winner = BitBoardHelpers.CheckVictory(board);

            Assert.AreEqual(DiscColor.Red, winner);
        }

        [DataTestMethod]
        [DataRow(0, 1, 2, 3)]
        [DataRow(1, 2, 3, 4)]
        [DataRow(2, 3, 4, 5)]
        [DataRow(3, 4, 5, 6)]
        public void CheckVictory_ShouldReturnBlack_GivenFourBlackMovesInRow(
            int firstColumn,
            int secondColumn,
            int thirdColumn,
            int fourthColumn)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, firstColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, secondColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, thirdColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, fourthColumn, DiscColor.Black);

            var winner = BitBoardHelpers.CheckVictory(board);

            Assert.AreEqual(DiscColor.Black, winner);
        }

        [DataTestMethod]
        [DataRow(0, 1, 2)]
        [DataRow(1, 2, 3)]
        [DataRow(2, 3, 4)]
        [DataRow(3, 4, 5)]
        [DataRow(4, 5, 6)]
        public void CheckVictory_ShouldReturnNone_GivenThreeRedMovesInRow(
            int firstColumn,
            int secondColumn,
            int thirdColumn)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, firstColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, secondColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, thirdColumn, DiscColor.Red);

            var winner = BitBoardHelpers.CheckVictory(board);

            Assert.AreEqual(DiscColor.None, winner);
        }

        [DataTestMethod]
        [DataRow(0, 1, 2)]
        [DataRow(1, 2, 3)]
        [DataRow(2, 3, 4)]
        [DataRow(3, 4, 5)]
        [DataRow(4, 5, 6)]
        public void CheckVictory_ShouldReturnNone_GivenThreeBlackMovesInRow(
            int firstColumn,
            int secondColumn,
            int thirdColumn)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, firstColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, secondColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, thirdColumn, DiscColor.Black);

            var winner = BitBoardHelpers.CheckVictory(board);

            Assert.AreEqual(DiscColor.None, winner);
        }

        [DataTestMethod]
        [DataRow(0, 6)]
        [DataRow(1, 5)]
        [DataRow(2, 4)]
        [DataRow(4, 2)]
        [DataRow(5, 1)]
        [DataRow(6, 0)]
        public void EvaluateBoardState_ShouldReturnZero_GivenSymmetricSingleDiscs(
            int redColumn,
            int blackColumn)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, redColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, blackColumn, DiscColor.Black);

            var actualScore = BitBoardHelpers.EvaluateBoardState(in board);

            Assert.AreEqual(0.0m, actualScore);
        }

        [DataTestMethod]
        [DataRow(0, 6)]
        [DataRow(1, 5)]
        [DataRow(2, 4)]
        [DataRow(4, 2)]
        [DataRow(5, 1)]
        [DataRow(6, 0)]
        public void EvaluateBoardState_ShouldReturnZero_GivenSymmetricDoubleDiscs(
            int redColumn,
            int blackColumn)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, redColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, redColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, blackColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, blackColumn, DiscColor.Black);

            var actualScore = BitBoardHelpers.EvaluateBoardState(in board);

            Assert.AreEqual(0.0m, actualScore);
        }

        [DataTestMethod]
        [DataRow(0, 6)]
        [DataRow(1, 5)]
        [DataRow(2, 4)]
        [DataRow(4, 2)]
        [DataRow(5, 1)]
        [DataRow(6, 0)]
        public void EvaluateBoardState_ShouldReturnZero_GivenSymmetricTripleDiscs(
            int redColumn,
            int blackColumn)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, redColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, redColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, redColumn, DiscColor.Red);

            board = BitBoardHelpers.BitBoardMove(in board, blackColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, blackColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, blackColumn, DiscColor.Black);

            var actualScore = BitBoardHelpers.EvaluateBoardState(in board);

            Assert.AreEqual(0.0m, actualScore);
        }

        [DataTestMethod]
        [DataRow(0, 6)]
        [DataRow(1, 5)]
        [DataRow(2, 4)]
        [DataRow(4, 2)]
        [DataRow(5, 1)]
        [DataRow(6, 0)]
        public void EvaluateBoardState_ShouldReturnZero_GivenSymmetricFourDiscs(
            int redColumn,
            int blackColumn)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, redColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, redColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, redColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, redColumn, DiscColor.Red);

            board = BitBoardHelpers.BitBoardMove(in board, blackColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, blackColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, blackColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, blackColumn, DiscColor.Black);

            var actualScore = BitBoardHelpers.EvaluateBoardState(in board);

            Assert.AreEqual(0.0m, actualScore);
        }

        [DataTestMethod]
        [DataRow(0, 1, 5, 6)]
        [DataRow(1, 2, 4, 5)]
        [DataRow(4, 5, 1, 2)]
        [DataRow(5, 6, 0, 1)]
        public void EvaluateBoardState_ShouldReturnZero_GivenSymmetricRowPairs(
            int redFirst,
            int redSecond,
            int blackFirst,
            int blackSecond)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, redFirst, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, redSecond, DiscColor.Red);

            board = BitBoardHelpers.BitBoardMove(in board, blackFirst, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, blackSecond, DiscColor.Black);

            var actualScore = BitBoardHelpers.EvaluateBoardState(in board);

            Assert.AreEqual(0.0m, actualScore);
        }

        [DataTestMethod]
        [DataRow(0, 6)]
        [DataRow(1, 5)]
        [DataRow(2, 4)]
        [DataRow(4, 2)]
        [DataRow(5, 1)]
        [DataRow(6, 0)]
        public void EvaluateBoardState_ShouldReturnZero_GivenOpposingStacks(
            int firstColumn,
            int secondColumn)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, firstColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, firstColumn, DiscColor.Black);

            board = BitBoardHelpers.BitBoardMove(in board, secondColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, secondColumn, DiscColor.Red);

            var actualScore = BitBoardHelpers.EvaluateBoardState(in board);

            Assert.AreEqual(0.0m, actualScore);
        }

        [DataTestMethod]
        [DataRow(0, 1, 2, 3)]
        [DataRow(1, 2, 3, 4)]
        [DataRow(2, 3, 4, 5)]
        [DataRow(3, 4, 5, 6)]
        [DataRow(2, 3, 0, 1)]
        [DataRow(3, 4, 1, 2)]
        [DataRow(4, 5, 2, 3)]
        [DataRow(5, 6, 3, 4)]
        [DataRow(0, 1, 5, 6)]
        [DataRow(0, 1, 3, 4)]
        [DataRow(0, 2, 1, 3)]
        [DataRow(1, 3, 0, 2)]
        [DataRow(2, 4, 3, 1)]
        [DataRow(2, 4, 3, 5)]
        [DataRow(3, 5, 4, 6)]
        public void CheckVictory_ShouldReturnNone_GivenNonWinningFirstRowHorizontals(
            int blackOne, int blackTwo,
            int redOne, int redTwo)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.BitBoardMove(in board, blackOne, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, blackTwo, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, redOne, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, redTwo, DiscColor.Red);

            var winner = BitBoardHelpers.CheckVictory(in board);

            Assert.AreEqual(DiscColor.None, winner);
        }

        [DataTestMethod]
        [DataRow(0, 1, 2, 3)]
        [DataRow(1, 2, 3, 4)]
        [DataRow(2, 3, 4, 5)]
        [DataRow(3, 4, 5, 6)]
        [DataRow(2, 3, 0, 1)]
        [DataRow(3, 4, 1, 2)]
        [DataRow(4, 5, 2, 3)]
        [DataRow(5, 6, 3, 4)]
        [DataRow(0, 1, 5, 6)]
        [DataRow(0, 1, 3, 4)]
        [DataRow(0, 2, 1, 3)]
        [DataRow(1, 3, 0, 2)]
        [DataRow(2, 4, 3, 1)]
        [DataRow(2, 4, 3, 5)]
        [DataRow(3, 5, 4, 6)]
        public void CheckVictory_ShouldReturnNone_GivenNonWinningSecondRowHorizontals(
            int blackOne, int blackTwo,
            int redOne, int redTwo)
        {
            var board = BitBoardHelpers.GetNewBoard();

            // Fill first row with non winners
            board = BitBoardHelpers.BitBoardMove(in board, 0, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, 1, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, 2, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, 3, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, 4, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, 5, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, 6, DiscColor.Black);
            
            board = BitBoardHelpers.BitBoardMove(in board, blackOne, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, blackTwo, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, redOne, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, redTwo, DiscColor.Red);

            var winner = BitBoardHelpers.CheckVictory(in board);

            Assert.AreEqual(DiscColor.None, winner);
        }


        [DataTestMethod]
        [DataRow(0, 1, 2, 3)]
        [DataRow(1, 2, 3, 4)]
        [DataRow(2, 3, 4, 5)]
        [DataRow(3, 4, 5, 6)]
        [DataRow(2, 3, 0, 1)]
        [DataRow(3, 4, 1, 2)]
        [DataRow(4, 5, 2, 3)]
        [DataRow(5, 6, 3, 4)]
        [DataRow(0, 1, 5, 6)]
        [DataRow(0, 1, 3, 4)]
        [DataRow(0, 2, 1, 3)]
        [DataRow(1, 3, 0, 2)]
        [DataRow(2, 4, 3, 1)]
        [DataRow(2, 4, 3, 5)]
        [DataRow(3, 5, 4, 6)]
        public void CheckVictory_ShouldReturnNone_GivenNonWinningThirdRowHorizontals(
            int blackOne, int blackTwo,
            int redOne, int redTwo)
        {
            var board = BitBoardHelpers.GetNewBoard();

            // Fill first row with non winners
            board = BitBoardHelpers.BitBoardMove(in board, 0, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, 1, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, 2, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, 3, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, 4, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, 5, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, 6, DiscColor.Black);

            // Fill second row with non winners
            board = BitBoardHelpers.BitBoardMove(in board, 0, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, 1, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, 2, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, 3, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, 4, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, 5, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, 6, DiscColor.Red);

            board = BitBoardHelpers.BitBoardMove(in board, blackOne, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, blackTwo, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(in board, redOne, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(in board, redTwo, DiscColor.Red);

            var winner = BitBoardHelpers.CheckVictory(in board);

            Assert.AreEqual(DiscColor.None, winner);
        }
    }
}
