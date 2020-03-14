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
            ulong board = BitBoard.GetNewBoard();

            Assert.IsTrue(BitBoard.IsColumnOneOpen(board));
            Assert.IsTrue(BitBoard.IsColumnTwoOpen(board));
            Assert.IsTrue(BitBoard.IsColumnThreeOpen(board));
            Assert.IsTrue(BitBoard.IsColumnFourOpen(board));
            Assert.IsTrue(BitBoard.IsColumnFiveOpen(board));
            Assert.IsTrue(BitBoard.IsColumnSixOpen(board));
            Assert.IsTrue(BitBoard.IsColumnSevenOpen(board));
        }
    }
}
