using System;

namespace ConnectBot
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread] // TODO Should this be single threaded application? 
        // probably not based on how I'm trying to use the AI, does it need to be?
        static void Main()
        {
            using (var game = new ConnectGame())
                game.Run();
        }
    }
#endif
}
