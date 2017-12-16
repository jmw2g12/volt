using System;
using System.Collections;
namespace Volt
{
    public static class Globals
    {
        public static int numPlayers = 4;
        public static int numMoves = 1;

        public static String[] shootDirs = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        public static int numShotOptions = shootDirs.Length;
        public static String[] shootTargets = { "HEAD", "BODY", "LARM", "RARM", "LLEG", "RLEG" };
        public static int[] targetRanks = { 5, 4, 2, 2, 3, 3 };
        public static String[] moveDirs = { "U", "R", "D", "L" };
        public static int numMoveOptions = moveDirs.Length;
        public static String[] moveTargets = { "1", "2", "3", "4", "5", "6" };


        public static int width = 9;
        public static int height = 9;
        public static int numHoles = 5;
        public static int numControlPoints = 1;
    }

    class Board
    {
        static ArrayList allMoves;
        Player[] players;
        public Coord[] holeLocations;
        public Coord[] controlLocations;

        public Board(Player[] players)
        {
            this.players = players;

            InitPlayerLocations();
            InitHoleLocations();
            SetControlLocations(true);

            GenAllMoves();
        }

        public Board Clone(){
            Player[] clonedPlayers = Player[Globals.numPlayers];
            for (int i = 0; i < Globals.numPlayers; i++){
                clonedPlayers[i] = players[i].Clone();
            }

            Board clonedBoard = new Board(clonedPlayers);

            Coord[] clonedHoleLocations = new Coord[Globals.numHoles];
            for (int i = 0; i < Globals.numHoles; i++){
                clonedHoleLocations[i] = holeLocations[i].Clone();
            }
            clonedBoard.holeLocations = clonedHoleLocations;

            Coord[] clonedControlLocations = new Coord[Globals.numControlPoints];
            for (int i = 0; i < Globals.numControlPoints; i++){
                clonedControlLocations[i] = controlLocations[i].Clone();
            }
            clonedBoard.controlLocations = clonedControlLocations;

            return clonedBoard;
        }

        public int[] GetSpaceAroundPlayer(Player player)
        {
            int[] result = new int[Globals.numMoveOptions];
            Coord start = player.location;
            Coord checkLocation;
            int idx = 0;
            int count;
            foreach (Coord transform in Coord.moveTransforms)
            {
                count = 0;
                checkLocation = start.Clone();
                checkLocation = Coord.Add(checkLocation, transform);
                while (IsOnBoard(checkLocation) && !IsHoleLocation(checkLocation) && !IsPlayerLocation(checkLocation))
                {
                    checkLocation = Coord.Add(checkLocation, transform);
                    count++;
                }
                result[idx] = count;
                idx++;
            }
            return result;
        }

        public bool[] GetShotsAroundPlayer(Player player)
        {
            bool[] result = new bool[Globals.numShotOptions];
            Coord start = player.location;
            Coord checkLocation;
            int idx = 0;
            bool count;
            foreach (Coord t in Coord.shotTransforms)
            {
                checkLocation = start.Clone();
                checkLocation = Coord.Add(checkLocation, t);
                count = IsOnBoard(checkLocation) ? true : false;
                result[idx] = count;
                idx++;
            }
            return result;
        }

        ArrayList FilterMovesByDir(ArrayList moves, String dir)
        {
            ArrayList filtered = new ArrayList();
            foreach (Move m in moves)
            {
                if (!m.dir.Equals(dir))
                {
                    filtered.Add(m);
                }
            }
            return filtered;
        }

        ArrayList FilterMovesBySpaces(ArrayList moves, int[] spaces)
        {
            ArrayList filtered = new ArrayList();
            foreach (Move m in moves)
            {
                if (m.shoot)
                {
                    filtered.Add(m);
                }
                else
                {
                    int dirIdx = Array.IndexOf(Globals.moveDirs, m.dir);
                    int limit = spaces[dirIdx];
                    if (int.Parse(m.target) <= limit) filtered.Add(m);
                }
            }
            return filtered;
        }

        public ArrayList GetPossibleMoves(Player p)
        {
            int[] space;
            bool[] shots;
            ArrayList possibleMoves = (ArrayList)allMoves.Clone();

            space = GetSpaceAroundPlayer(p);
            possibleMoves = FilterMovesBySpaces(possibleMoves, space);

            shots = GetShotsAroundPlayer(p);
            for (int i = 0; i < shots.Length; i++)
            {
                if (!shots[i]) possibleMoves = FilterMovesByDir(possibleMoves, Globals.shootDirs[i]);
            }

            return possibleMoves;
        }

        void GenAllMoves()
        {
            allMoves = new ArrayList();
            foreach (String d in Globals.shootDirs)
            {
                foreach (String t in Globals.shootTargets)
                {
                    allMoves.Add(new Move(d, t));
                }
            }
            foreach (String d in Globals.moveDirs)
            {
                foreach (String t in Globals.moveTargets)
                {
                    allMoves.Add(new Move(d, t));
                }
            }
        }

        public void Print()
        {
            Console.WriteLine("");
            for (int i = 0; i < Globals.numPlayers; i++)
            {
                Console.WriteLine("Player " + (i + 1) + " : Health=" + players[i].health + "   Score=" + players[i].score);
            }
            Console.WriteLine("");
            Console.Write("╔");
            for (int x = 0; x < Globals.width; x++)
            {
                Console.Write("═══");
            }
            Console.WriteLine("╗");
            for (int y = Globals.height - 1; y >= 0; y--)
            {
                Console.Write("║");
                for (int x = 0; x < Globals.width; x++)
                {
                    Coord cell = new Coord(x, y);
                    if (IsControlLocation(cell))
                    {
                        Console.Write(" § ");
                        continue;
                    }
                    if (cell.IsInArray(holeLocations))
                    {
                        Console.Write(" ▓ ");
                    }
                    else
                    {
                        int l = WhichPlayerLocation(cell);
                        if (l != -1)
                        {
                            Console.Write(" " + (l + 1) + " ");
                        }
                        else
                        {
                            Console.Write(" · "); //×·
                        }
                    }
                }
                Console.WriteLine("║");
            }
            Console.Write("╚");
            for (int x = 0; x < Globals.width; x++)
            {
                Console.Write("═══");
            }
            Console.WriteLine("╝");
        }

        int WhichPlayerLocation(Coord c)
        {
            for (int i = 0; i < Globals.numPlayers; i++)
            {
                if (c.Equals(players[i].location))
                {
                    return players[i].number;
                }
            }
            return -1;
        }

        bool IsHoleLocation(Coord c)
        {
            for (int i = 0; i < Globals.numHoles; i++)
            {
                if (holeLocations[i] != null && c.Equals(holeLocations[i]))
                {
                    return true;
                }
            }
            return false;
        }

        bool IsControlLocation(Coord c)
        {
            for (int i = 0; i < Globals.numControlPoints; i++)
            {
                if (controlLocations != null && controlLocations[i] != null && c.Equals(controlLocations[i]))
                {
                    return true;
                }
            }
            return false;
        }

        bool IsPlayerLocation(Coord c)
        {
            foreach (Player p in players)
            {
                if (c.Equals(p.location))
                {
                    return true;
                }
            }
            return false;
        }

        void InitHoleLocations()
        {
            Random rnd = new Random(3);
            holeLocations = new Coord[Globals.numHoles];

            Coord holeLocation;
            for (int i = 0; i < Globals.numHoles; i++)
            {
                do
                {
                    holeLocation = new Coord(rnd.Next(1, Globals.width - 1), rnd.Next(1, Globals.height - 1));
                } while (IsHoleLocation(holeLocation));
                holeLocations[i] = holeLocation;
            }
        }

        void SetControlLocations(bool initialisation)
        {
            Random rnd = new Random(0);
            controlLocations = new Coord[Globals.numControlPoints];

            Coord newControlLocation;
            for (int i = 0; i < Globals.numControlPoints; i++)
            {
                do
                {
                    if (initialisation)
                    {
                        newControlLocation = new Coord(rnd.Next(1, Globals.width - 1), rnd.Next(1, Globals.height - 1));
                    }
                    else
                    {
                        newControlLocation = new Coord(rnd.Next(0, Globals.width), rnd.Next(0, Globals.height));
                    }
                } while (IsHoleLocation(newControlLocation) || IsPlayerLocation(newControlLocation) || IsControlLocation(newControlLocation));
                controlLocations[i] = newControlLocation;
            }
        }

        void InitPlayerLocations()
        {
            for (int i = 0; i < Globals.numPlayers; i++)
            {
                PlayerToSpawn(players[i]);
            }
        }

        void PlayerToSpawn(Player player)
        {
            int halfWidth = (int)Math.Floor(Globals.width / 2.0);
            int halfHeight = (int)Math.Floor(Globals.height / 2.0);
            int rightSide = Globals.width - 1;
            int topSide = Globals.height - 1;

            if (player.number == 0)
            {
                player.location = new Coord(halfWidth, 0);
            }
            else if (player.number == 1)
            {
                player.location = new Coord(0, halfHeight);
            }
            else if (player.number == 2)
            {
                player.location = new Coord(halfWidth, topSide);
            }
            else if (player.number == 3)
            {
                player.location = new Coord(rightSide, halfHeight);
            }
        }

        public void ExecuteMoves(Move[,] moves)
        {
            Move[] round;
            for (int i = 0; i < Globals.numMoves; i++)
            {
                round = GetRankedRound(moves, i);
                for (int j = 0; j < Globals.numPlayers; j++)
                {
                    if (round[j].shoot)
                    {
                        Console.WriteLine("Player " + (round[j].player.number + 1) + " takes their turn... : shooting " + round[j].dir + " at their opponent's " + round[j].target);
                    }
                    else
                    {
                        Console.WriteLine("Player " + (round[j].player.number + 1) + " takes their turn... : moving " + round[j].dir + " " + round[j].target + " places");
                    }
                    MakeMove(round[j]);
                    Print();
                    Console.ReadKey();
                }
            }
            foreach (Player p in players)
            {
                if (IsControlLocation(p.location))
                {
                    Console.WriteLine("Player " + (p.number + 1) + " finished the round on the control point!");
                    p.score++;
                    SetControlLocations(false);
                }
            }
        }

        public void MakeMove(Move m){
            if (m.shoot){
                PlayerShoot(m);
            }else{
                MovePlayer(m);
            }
        }

        public void PlayerShoot(Move m)
        {
            Coord checkLocation = m.player.location.Clone();
            Coord transform;

            switch (m.dir)
            {
                case "N":
                    transform = Coord.N;
                    break;
                case "NE":
                    transform = Coord.NE;
                    break;
                case "E":
                    transform = Coord.E;
                    break;
                case "SE":
                    transform = Coord.SE;
                    break;
                case "S":
                    transform = Coord.S;
                    break;
                case "SW":
                    transform = Coord.SW;
                    break;
                case "W":
                    transform = Coord.W;
                    break;
                case "NW":
                    transform = Coord.NW;
                    break;
                default:
                    transform = null;
                    break;
            }

            do
            {
                checkLocation = Coord.Add(checkLocation, transform);
                int pl = WhichPlayerLocation(checkLocation);
                if (pl != -1)
                {
                    Console.WriteLine("Shot HIT player " + (pl + 1));
                    switch (m.target)
                    {
                        case "HEAD":
                            players[pl].health--;
                            if (players[pl].health == 0)
                            {
                                players[pl].health = 3;
                                PlayerToSpawn(players[pl]);
                                m.player.score++;
                            }
                            break;
                        case "LARM":
                            foreach (Move p in players[pl].program)
                            {
                                p.ArmHit(true);
                            }
                            break;
                        case "RARM":
                            foreach (Move p in players[pl].program)
                            {
                                p.ArmHit(false);
                            }
                            break;
                        case "LLEG":
                            foreach (Move p in players[pl].program)
                            {
                                p.LegHit(true);
                            }
                            break;
                        case "RLEG":
                            foreach (Move p in players[pl].program)
                            {
                                p.LegHit(false);
                            }
                            break;
                        case "BODY":
                            Coord pushedLoc = Coord.Add(players[pl].location, transform);
                            if (IsOnBoard(pushedLoc) && !IsPlayerLocation(pushedLoc))
                            {
                                if (IsHoleLocation(pushedLoc))
                                {
                                    players[pl].health--;
                                    if (players[pl].health <= 0)
                                    {
                                        players[pl].health = 3;
                                        players[pl].score--;
                                    }
                                    PlayerToSpawn(players[pl]);
                                    m.player.score++;
                                    Console.WriteLine("Player " + (m.player.number + 1) + " fell down a hole!");
                                }
                                else
                                {
                                    players[pl].location = pushedLoc;
                                }
                            }
                            break;
                    }
                    break;
                }
            } while (IsOnBoard(checkLocation));

        }

        public void MovePlayer(Move m)
        {
            Coord checkLocation = m.player.location.Clone();
            Coord transform = new Coord(0, 0);
            int spaces = int.Parse(m.target);

            switch (m.dir)
            {
                case "U":
                    transform = Coord.U;
                    break;
                case "R":
                    transform = Coord.R;
                    break;
                case "D":
                    transform = Coord.D;
                    break;
                case "L":
                    transform = Coord.L;
                    break;
            }

            while (spaces > 0)
            {
                checkLocation = Coord.Add(checkLocation, transform);
                if (IsHoleLocation(checkLocation))
                {
                    m.player.health--;
                    if (m.player.health <= 0)
                    {
                        m.player.health = 3;
                        m.player.score--;
                    }
                    PlayerToSpawn(m.player);
                    Console.WriteLine("Player " + (m.player.number + 1) + " fell down a hole!");
                    break;
                }
                else if (!IsOnBoard(checkLocation) || IsPlayerLocation(checkLocation))
                {
                    break;
                }
                spaces--;
                m.player.location = checkLocation;
            }

        }

        public Coord MoveToTransform(Move m)
        {
            int distance;
            distance = int.Parse(m.target);
            if (m.dir.Equals("U"))
            {
                return new Coord(0, distance);
            }
            else if (m.dir.Equals("R"))
            {
                return new Coord(distance, 0);
            }
            else if (m.dir.Equals("D"))
            {
                return new Coord(0, -distance);
            }
            else if (m.dir.Equals("L"))
            {
                return new Coord(-distance, 0);
            }
            return null;
        }

        public bool IsOnBoard(Coord c)
        {
            return (c.x >= 0) && (c.y >= 0) && (c.x < Globals.width) && (c.y < Globals.height);
        }

        Move[] GetRankedRound(Move[,] moves, int number)
        {
            Move[] round = new Move[Globals.numPlayers];
            for (int i = 0; i < Globals.numPlayers; i++)
            {
                round[i] = moves[i, number];
            }
            //shuffleMoves(round);  need to implement!
            //Array.Sort(round, delegate (Move x, Move y) { return x.rank - y.rank; });
            return round;
        }
    }

    class Coord
    {
        public int x;
        public int y;

        public static Coord N = new Coord(0, 1);
        public static Coord NE = new Coord(1, 1);
        public static Coord E = new Coord(1, 0);
        public static Coord SE = new Coord(1, -1);
        public static Coord S = new Coord(0, -1);
        public static Coord SW = new Coord(-1, -1);
        public static Coord W = new Coord(-1, 0);
        public static Coord NW = new Coord(-1, 1);

        public static Coord U = N;
        public static Coord R = E;
        public static Coord D = S;
        public static Coord L = W;

        public static Coord[] shotTransforms = { N, NE, E, SE, S, SW, W, NW };
        public static Coord[] moveTransforms = { U, R, D, L };

        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Coord c)
        {
            return (c.x == x) && (c.y == y);
        }

        public bool IsInArray(Coord[] c)
        {
            for (int i = 0; i < c.Length; i++)
            {
                if (Equals(c[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public Coord Clone()
        {
            return new Coord(x, y);
        }

        public static Coord Add(Coord a, Coord b)
        {
            return new Coord(a.x + b.x, a.y + b.y);
        }

        public static int manhattanDistance(Coord a, Coord b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }
    }

    class Volt
    {
        static void Main()
        {
            Player[] players = new Player[Globals.numPlayers];
            for (int i = 0; i < Globals.numPlayers; i++)
            {
                players[i] = new Player(i, (i != 0 ? "computer" : "human"));
            }

            Board board = new Board(players);

            PrintIntro();

            Move[,] programming = new Move[Globals.numPlayers, Globals.numMoves];
            Move[] temp = new Move[Globals.numMoves];

            while (true)
            {
                board.Print();
                for (int i = 0; i < Globals.numPlayers; i++)
                {
                    temp = players[i].MakeMove(board);
                    for (int j = 0; j < Globals.numMoves; j++)
                    {
                        programming[i, j] = temp[j];
                    }
                }
                Console.WriteLine("All programs locked in!");
                board.ExecuteMoves(programming);
            }
        }

        static void PrintIntro()
        {
            Console.WriteLine("Welcome to Volt!");
            Console.WriteLine("");
            Console.WriteLine("During the programming phase type:");
            Console.WriteLine("L, R, U or D followed by a number separated by a space to move that number of places left, right, up or down respectively.");
            Console.WriteLine("N, NE, E, SE, S, SW, W, NW followed by HEAD (damage), BODY (push), LARM (45 deg ccw), RARM (45 deg cw), LLEG (90 deg ccw) or RLEG (90 deg cw) to shoot.");
            Console.WriteLine("");
        }
    }

    class Move
    {
        public int rank;
        public bool shoot;
        public String dir;
        public String target;
        public Player player;

        public Move(Player player, String dir, String target)
        {
            this.player = player;
            this.dir = dir;
            this.target = target;
            if (Array.IndexOf(Globals.shootDirs, dir) != -1)
            {
                shoot = true;
                rank = Globals.targetRanks[Array.IndexOf(Globals.shootTargets, target)];
            }
            else
            {
                shoot = false;
                rank = int.Parse(target);
            }
        }
        public Move(String dir, String target)
        {
            this.dir = dir;
            this.target = target;
            if (Array.IndexOf(Globals.shootDirs, dir) != -1)
            {
                shoot = true;
                rank = Globals.targetRanks[Array.IndexOf(Globals.shootTargets, target)];
            }
            else
            {
                shoot = false;
                rank = int.Parse(target);
            }
        }

        public Move Clone()
        {
            return new Move(dir, target);
        }

        public void Print()
        {
            if (shoot)
            {
                Console.WriteLine("Shooting " + dir + " at opponent's " + target);
            }
            else
            {
                Console.WriteLine("Moving " + dir + " " + target + " spaces");
            }
        }

        public static bool IsValidInput(String s)
        {
            if (s.Split(' ').Length < 2) return false;
            String dir = s.Split(' ')[0];
            String target = s.Split(' ')[1];
            return ((ArrContainsStr(Globals.shootDirs, dir) && ArrContainsStr(Globals.shootTargets, target)) || (ArrContainsStr(Globals.moveDirs, dir) && ArrContainsStr(Globals.moveTargets, target)));
        }

        static bool ArrContainsStr(String[] arr, String s)
        {
            foreach (String t in arr)
            {
                if (t.Equals(s)) return true;
            }
            return false;
        }

        public void LegHit(bool left)
        {
            if (shoot) return;
            int idx = Array.IndexOf(Globals.moveDirs, dir);
            if (left)
            {   //ccw
                int newIdx = Modulo(idx - 1, Globals.moveDirs.Length);
                dir = Globals.moveDirs[newIdx];
            }
            else
            {   //cw
                int newIdx = Modulo(idx + 1, Globals.moveDirs.Length);
                dir = Globals.moveDirs[newIdx];
            }
        }

        public void ArmHit(bool left)
        { 
            if (!shoot) return;
            int idx = Array.IndexOf(Globals.shootDirs, dir);
            if (left)
            {   //ccw
                int newIdx = Modulo(idx - 1, Globals.shootDirs.Length);
                dir = Globals.shootDirs[newIdx];
            }
            else
            {   //cw
                int newIdx = Modulo(idx + 1, Globals.shootDirs.Length);
                dir = Globals.shootDirs[newIdx];
            }
        }

        int Modulo(int a, int b)
        {
            return a - b * (int)Math.Floor((double)a / (double)b);
        }
    }

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
                    if (error) Console.WriteLine("Instruction was not valid!");
                    Console.WriteLine("Please enter command #" + (i + 1));
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
                value = Coord.manhattanDistance(cp, location);
                if (value < lowest)
                {
                    lowest = value;
                }
            }
            return lowest;
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

        public Move[] MakeMove(Board board)
        {
            Console.WriteLine("Player " + (number + 1) + "'s programming phase...");
            switch (strategy)
            {
                case "human":
                    return HumanMove(board);
                case "computer":
                    return ComputerMove(board);
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

    class Node {
        public Board board;
        public int score;
        public int games; 
        public Node (Board board){
            this.board = board;
        }
    }
}