namespace ConnectBot
{
    /*
    * Below is a map of indices on the ulong bits with where 
    * on the board they represent. Each ulong property on a
    * BitBoard only saves the discs of that color.
    * To find the available space in a column OR the two
    * discs sets together.
    * 
    * 5  11  17  23  29  35  41
    * 4  10  16  22  28  34  40
    * 3   9  15  21  27  33  39
    * 2   8  14  20  26  32  38
    * 1   7  13  19  25  31  37
    * 0   6  12  18  24  30  36
    * 
    * To move over a column add 7.
    */
    /// <summary>
    /// A struct used to store the state of the board as
    /// two ulongs. One for each column of disc. The 1 bits
    /// for a color represent a disc there. The lower left
    /// space is the first bit and it counts up as it moves
    /// up the columns. 
    /// </summary>
    public class BitBoard
    {
        public ulong RedDiscs { get; set; }
        public ulong BlackDiscs { get; set; }
        public ulong FullBoard { get
            {
                return RedDiscs | BlackDiscs;
            }
        }

        public BitBoard(ulong redDiscs, ulong blackDiscs)
        {
            RedDiscs = redDiscs;
            BlackDiscs = blackDiscs;
        }
    }
}
