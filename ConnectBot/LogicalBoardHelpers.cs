using System;
using System.Collections.Generic;

namespace ConnectBot
{
    /// <summary>
    /// Stores game disc colors and a value for empty spaces. 
    /// Black moves first.
    /// </summary>
    public enum DiscColor
    {
        Red = -1,
        None = 0,
        Black = 1
    }

    /// <summary>
    /// Common methods to perform on a 6x7 array of integers
    /// that represents the logical board state.
    /// </summary>
    public static class LogicalBoardHelpers
    {
        public static int NUM_ROWS = 6;
        public static int NUM_COLUMNS = 7;

        public static DiscColor ChangeTurnColor(DiscColor color)
        {
            if (color != DiscColor.Red &&
                color != DiscColor.Black)
                throw new ArgumentException("Must pass in a valid disc color.", nameof(color));

            return color == DiscColor.Red 
                ? DiscColor.Black 
                : DiscColor.Red;
        }
    }
}
