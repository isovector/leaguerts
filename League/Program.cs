using System;

namespace LeagueEngine {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args) {
            using (League game = new League()) {
                game.Run();
            }
        }
    }
}

