
using System;
using System.IO;
using Newtonsoft.Json;

namespace Volt
{
    class Volt
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Globals.supressAllOutput = true;
            }
            Player[] players = new Player[Globals.numPlayers];
            //players = ParsePlayers(args);

            for (int i = 0; i < Globals.numPlayers; i++)
            {
                if (args.Length == 0)
                {
                    players[i] = new Player(i, (i != 0 ? "mctsegreedy" : "human"));
                }
                else
                {
                    players[i] = new Player(i, "mcts" + args[i]);
                }
            }

            Board board = new Board(players);
            board.supressOutput = false;

            /*Node n = new Node(board, 0);

            long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            n.RunMCTS(10000);
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (!Globals.supressAllOutput) Console.WriteLine("Elapsed time : " + (endTime-startTime));
            */


            PrintIntro();

            Move[,] programming = new Move[Globals.numPlayers, Globals.numMoves];
            Move[] temp = new Move[Globals.numMoves];

            int count = 0;
            while (count <= Globals.maxTurns)
            {
                board.Print();
                for (int i = 0; i < Globals.numPlayers; i++)
                {
                    temp = players[i].ChooseMove(board.Clone());
                    for (int j = 0; j < Globals.numMoves; j++)
                    {
                        programming[i, j] = temp[j];
                    }
                }
                if (!Globals.supressAllOutput) Console.WriteLine("All programs locked in!");
                board.ExecuteMoves(programming);
                count++;
            }
        }

        static Player[] ParsePlayers(string[] args)
        {
            Console.WriteLine("in parseplayers");
            Player[] players = new Player[Globals.numPlayers];

            StreamReader s = File.OpenText("../../voltplayers.json");
            string input = s.ReadToEnd();

            dynamic result = JsonConvert.DeserializeObject(input);
            string a = result.players[0].strategy.type;
            Console.WriteLine(a);

            Console.WriteLine("leaving parseplayers");
            return players;
        }

        static void PrintIntro()
        {
            if (!Globals.supressAllOutput) Console.WriteLine("Welcome to Volt!");
            if (!Globals.supressAllOutput) Console.WriteLine("");
            if (!Globals.supressAllOutput) Console.WriteLine("During the programming phase type:");
            if (!Globals.supressAllOutput) Console.WriteLine("L, R, U or D followed by a number separated by a space to move that number of places left, right, up or down respectively.");
            if (!Globals.supressAllOutput) Console.WriteLine("N, NE, E, SE, S, SW, W, NW followed by HEAD (damage), BODY (push), LARM (45 deg ccw), RARM (45 deg cw), LLEG (90 deg ccw) or RLEG (90 deg cw) to shoot.");
            if (!Globals.supressAllOutput) Console.WriteLine("");
        }
    }
}