using System;

namespace OrbItProcs {
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        static Game1 game;
        static void Main(string[] args)
        {
            
           //using (Game1 game = new Game1())
           //{
           //    game.Run();
           //}
            //using (
            game = new Game1();//)
            //{
                game.Run();
            //
            //
            Console.WriteLine("....guys?");
        }

        public static Game1 getGame()
        {
            return game;
        }
        public static Room getRoom()
        {
            return game.room;
        }
    }
#endif
}

