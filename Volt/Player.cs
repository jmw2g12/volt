using System;
using System.Collections;

namespace Volt
{
    class Player
    {
        public int number;
        public string strategy;
        public int score = 0;
        public int health = 3;
        public Coord location;
        public Move[] program;

        public Player(int number, string strategy)
        {
            this.number = number;
            this.strategy = strategy;
        }

        public Move[] HumanMove(Board board)
        {
            Move[] moves = new Move[Globals.numMoves];

            String line;
            bool error;
            for (int i = 0; i < Globals.numMoves; i++)
            {
                error = false;
                do
                {
                    if (error && !Globals.supressAllOutput) Console.WriteLine("Instruction was not valid!");
                    if (!Globals.supressAllOutput) Console.WriteLine("Please enter command #" + (i + 1));
                    line = Console.ReadLine().ToUpper();
                    error = true;
                } while (!Move.IsValidInput(line));
                moves[i] = new Move(this, line.Split(' ')[0], line.Split(' ')[1]);
            }
            program = moves;
            return moves;
        }

        public int ControlPointEvalFunction(Coord location, Coord[] controlPoints)
        {
            int lowest = int.MaxValue;
            int value;
            foreach (Coord cp in controlPoints)
            {
                value = Coord.ManhattanDistance(cp, location);
                if (value < lowest)
                {
                    lowest = value;
                }
            }
            return lowest;
        }

        public Move[] MCTSMove(Board board, String banditStrategy)
        {
            Move[] moves = new Move[Globals.numMoves];
            Node n = new Node(board.Clone(), number, banditStrategy);
            moves[0] = n.RunMCTS(Globals.MCTSLimit);
            moves[0].player = this;
            program = moves;
            return moves;
        }

        public Move[] ComputerMove(Board board)
        {
            Move[] moves = new Move[Globals.numMoves];
            ArrayList possible = board.GetPossibleMoves(this);
            int currentBestVal = int.MaxValue;
            Move currentBestMove = (Move)possible[0];
            int value;

            foreach (Move n in possible)
            {
                if (n.shoot) continue;
                Coord transform = board.MoveToTransform(n);
                value = ControlPointEvalFunction(Coord.Add(transform, location), board.controlLocations);
                if (value < currentBestVal)
                {
                    currentBestMove = n;
                    currentBestVal = value;
                }
            }
            //only works for one move!
            moves[0] = currentBestMove.Clone();
            moves[0].player = this;

            program = moves;
            return moves;
        }

        public Move[] ChooseMove(Board board)
        {
            if (!Globals.supressAllOutput) Console.WriteLine("Player " + (number + 1) + "'s programming phase...");
            switch (strategy)
            {
                case "human":
                    return HumanMove(board);
                case "computer":
                    return ComputerMove(board);
                case "mctsucb":
                    return MCTSMove(board, "ucb");
                case "mctsegreedy":
                    return MCTSMove(board, "egreedy");
            }
            return null;
        }

        public Player Clone()
        {
            Player cloned = new Player(number, strategy);
            cloned.score = score;
            cloned.health = health;
            if (location != null) cloned.location = location.Clone();
            if (program != null)
            {
                Move[] moves = new Move[Globals.numMoves];
                for (int i = 0; i < Globals.numMoves; i++)
                {
                    if (program[i] != null) moves[i] = program[i].Clone();
                }
            }
            return cloned;
        }
    }
}