using System;

namespace OrbItProcs {
#if WINDOWS || XBOX
    static class Program
    {
        static Game1 game;
        static void Main(string[] args)
        {
           game = new Game1();
                game.Run();
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

