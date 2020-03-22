using ConnectBot;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConnectBotTests
{
    [TestClass]
    public class BoardColumnTests
    {
        [TestMethod]
        public void GetBitColumn_ShouldReturnAccurateColumn_GivenFullColumn()
        {
            var boardColumn = new BoardColumn(0, 0, null, null, null);

            boardColumn.SetSpace(DiscColor.Black);
            boardColumn.SetSpace(DiscColor.Black);
            boardColumn.SetSpace(DiscColor.Black);

            boardColumn.SetSpace(DiscColor.Red);
            boardColumn.SetSpace(DiscColor.Red);
            boardColumn.SetSpace(DiscColor.Red);

            var bitColumn = boardColumn.GetBitColumn();

            Assert.IsTrue(BitBoardHelpers.CheckSingleBit(bitColumn.BlackDiscs, 0));
            Assert.IsTrue(BitBoardHelpers.CheckSingleBit(bitColumn.BlackDiscs, 1));
            Assert.IsTrue(BitBoardHelpers.CheckSingleBit(bitColumn.BlackDiscs, 2));
            Assert.IsFalse(BitBoardHelpers.CheckSingleBit(bitColumn.BlackDiscs, 3));
            Assert.IsFalse(BitBoardHelpers.CheckSingleBit(bitColumn.BlackDiscs, 4));
            Assert.IsFalse(BitBoardHelpers.CheckSingleBit(bitColumn.BlackDiscs, 5));

            Assert.IsFalse(BitBoardHelpers.CheckSingleBit(bitColumn.RedDiscs, 0));
            Assert.IsFalse(BitBoardHelpers.CheckSingleBit(bitColumn.RedDiscs, 1));
            Assert.IsFalse(BitBoardHelpers.CheckSingleBit(bitColumn.RedDiscs, 2));
            Assert.IsTrue(BitBoardHelpers.CheckSingleBit(bitColumn.RedDiscs, 3));
            Assert.IsTrue(BitBoardHelpers.CheckSingleBit(bitColumn.RedDiscs, 4));
            Assert.IsTrue(BitBoardHelpers.CheckSingleBit(bitColumn.RedDiscs, 5));
        }

        [TestMethod]
        public void GetBitColumn_ShouldReturnAccurateColumn_GivenPartiallyFullColumn()
        {
            var boardColumn = new BoardColumn(0, 0, null, null, null);

            boardColumn.SetSpace(DiscColor.Black);

            boardColumn.SetSpace(DiscColor.Red);

            var bitColumn = boardColumn.GetBitColumn();

            Assert.IsTrue(BitBoardHelpers.CheckSingleBit(bitColumn.BlackDiscs, 0));
            Assert.IsFalse(BitBoardHelpers.CheckSingleBit(bitColumn.BlackDiscs, 1));
            Assert.IsFalse(BitBoardHelpers.CheckSingleBit(bitColumn.BlackDiscs, 2));
            Assert.IsFalse(BitBoardHelpers.CheckSingleBit(bitColumn.BlackDiscs, 3));
            Assert.IsFalse(BitBoardHelpers.CheckSingleBit(bitColumn.BlackDiscs, 4));
            Assert.IsFalse(BitBoardHelpers.CheckSingleBit(bitColumn.BlackDiscs, 5));

            Assert.IsFalse(BitBoardHelpers.CheckSingleBit(bitColumn.RedDiscs, 0));
            Assert.IsTrue(BitBoardHelpers.CheckSingleBit(bitColumn.RedDiscs, 1));
            Assert.IsFalse(BitBoardHelpers.CheckSingleBit(bitColumn.RedDiscs, 2));
            Assert.IsFalse(BitBoardHelpers.CheckSingleBit(bitColumn.RedDiscs, 3));
            Assert.IsFalse(BitBoardHelpers.CheckSingleBit(bitColumn.RedDiscs, 4));
            Assert.IsFalse(BitBoardHelpers.CheckSingleBit(bitColumn.RedDiscs, 5));
        }
    }
}
