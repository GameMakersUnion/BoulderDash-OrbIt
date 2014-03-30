using System;

namespace OrbItProcs {
#if WINDOWS || XBOX
    static class Program
    {
        static void Main(string[] args)
        {
            Game1.Start();
        }
    }
#endif
}