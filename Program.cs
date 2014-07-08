using System;

namespace OSDC
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (GameEx game = new GameEx())
            {
                game.Run();
            }
        }
    }
}

