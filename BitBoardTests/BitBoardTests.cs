using System;
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
        public void Move_ShouldReturnFullColumn_GivenSixMovesInSameColumn(int column)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.Move(board, column, DiscColor.Red);
            board = BitBoardHelpers.Move(board, column, DiscColor.Red);
            board = BitBoardHelpers.Move(board, column, DiscColor.Red);
            board = BitBoardHelpers.Move(board, column, DiscColor.Red);
            board = BitBoardHelpers.Move(board, column, DiscColor.Red);
            board = BitBoardHelpers.Move(board, column, DiscColor.Red);

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
        public void Move_ShouldReturnOpenColumn_GivenFiveMovesInSameColumn(int column)
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.Move(board, column, DiscColor.Red);
            board = BitBoardHelpers.Move(board, column, DiscColor.Red);
            board = BitBoardHelpers.Move(board, column, DiscColor.Red);
            board = BitBoardHelpers.Move(board, column, DiscColor.Red);
            board = BitBoardHelpers.Move(board, column, DiscColor.Red);

            Assert.IsTrue(BitBoardHelpers.IsColumnOpen(board, column));
        }
    }
}
