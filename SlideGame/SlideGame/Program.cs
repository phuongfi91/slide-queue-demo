using System;

namespace SlideGame
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SlideGame game = new SlideGame())
            {
                game.Run();
            }
        }
    }
#endif
}

