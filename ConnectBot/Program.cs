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
        static void Main()
        {
            using (var game = new ConnectGame())
                game.Run();
        }
    }
#endif
}
