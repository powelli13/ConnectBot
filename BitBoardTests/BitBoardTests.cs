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

        [TestMethod]
        public void Move_ShouldReturnFullColumn_GivenSixMovesInSameColumn()
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.Move(board, 0, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 0, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 0, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 0, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 0, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 0, DiscColor.Red);

            Assert.IsFalse(BitBoardHelpers.IsColumnOpen(board, 0));
        }

        [TestMethod]
        public void Move_ShouldReturnOpenColumn_GivenFiveMovesInSameColumn()
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.Move(board, 0, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 0, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 0, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 0, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 0, DiscColor.Red);

            Assert.IsTrue(BitBoardHelpers.IsColumnOpen(board, 0));
        }

        [TestMethod]
        public void Move_ShouldReturnOpenColumn_GivenFiveMovesInLastColumn()
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.Move(board, 6, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 6, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 6, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 6, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 6, DiscColor.Red);

            Assert.IsTrue(BitBoardHelpers.IsColumnOpen(board, 6));
        }

        [TestMethod]
        public void Move_ShouldReturnFullColumn_GivenSixMovesInLastColumn()
        {
            var board = BitBoardHelpers.GetNewBoard();

            board = BitBoardHelpers.Move(board, 6, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 6, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 6, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 6, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 6, DiscColor.Red);
            board = BitBoardHelpers.Move(board, 6, DiscColor.Red);

            Assert.IsFalse(BitBoardHelpers.IsColumnOpen(board, 6));
        }
    }
}
