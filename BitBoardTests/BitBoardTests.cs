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

            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);

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

            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);

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

            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);

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

            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Black);

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

            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Red);

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

            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(board, column, DiscColor.Black);

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

            board = BitBoardHelpers.BitBoardMove(board, firstColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, secondColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, thirdColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, fourthColumn, DiscColor.Red);

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

            board = BitBoardHelpers.BitBoardMove(board, firstColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(board, secondColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(board, thirdColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(board, fourthColumn, DiscColor.Black);

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

            board = BitBoardHelpers.BitBoardMove(board, firstColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, secondColumn, DiscColor.Red);
            board = BitBoardHelpers.BitBoardMove(board, thirdColumn, DiscColor.Red);

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

            board = BitBoardHelpers.BitBoardMove(board, firstColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(board, secondColumn, DiscColor.Black);
            board = BitBoardHelpers.BitBoardMove(board, thirdColumn, DiscColor.Black);

            var winner = BitBoardHelpers.CheckVictory(board);

            Assert.AreEqual(DiscColor.None, winner);
        }
    }
}
