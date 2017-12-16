using System;
using System.Collections;
namespace HelloWorld
{
    class Board
    {
        // Coordinate transforms
        Coord N;
        Coord NE;
        Coord E;
        Coord SE;
        Coord S;
        Coord SW;
        Coord W;
        Coord NW;

        Coord U;
        Coord R;
        Coord D;
        Coord L;

        Coord[] shotTransforms;
        Coord[] moveTransforms;

        int nMoves = 1;

        String[] shootDirs = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        String[] shootTargets = { "HEAD", "BODY", "LARM", "RARM", "LLEG", "RLEG" };
        String[] moveDirs = { "U", "R", "D", "L" };
        String[] moveTargets = { "1", "2", "3", "4", "5", "6" };

        int width = 9;
        int height = 9;
        int numHoles = 5;
        int numPlayers;

        Player[] players;
        Coord[] holeLocations;
        public Coord controlLocation;

        public ArrayList allMoves = new ArrayList();

        public Board(Player[] players)
        {
            this.players = players;
            numPlayers = players.Length;
            holeLocations = new Coord[numHoles];

            initPlayerLocations();
            initHoleLocations();
            setControlLocation(true);

            genAllMoves();

            N = new Coord(0, 1);
            NE = new Coord(1, 1);
            E = new Coord(1, 0);
            SE = new Coord(1, -1);
            S = new Coord(0, -1);
            SW = new Coord(-1, -1);
            W = new Coord(-1, 0);
            NW = new Coord(-1, 1);

            U = N;
            R = E;
            D = S;
            L = W;

            shotTransforms = new Coord[] { N, NE, E, SE, S, SW, W, NW };
            moveTransforms = new Coord[] { N, E, S, W };
        }

        public int[] getSpaceAroundPlayer(Player player)
        {
            int[] result = new int[moveTransforms.Length];
            Coord start = player.location;
            Coord loc;
            int idx = 0;
            int count;
            foreach (Coord t in moveTransforms)
            {
                count = 0;
                loc = start.clone();
                loc = Coord.add(loc, t);
                while (isOnBoard(loc.x, loc.y) && !isHoleLocation(loc) && !isPlayerLocation(loc))
                {
                    loc = Coord.add(loc, t);
                    count++;
                }
                result[idx] = count;
                idx++;
            }
            return result;
        }
        public int[] getShotsAroundPlayer(Player player)
        {
            int[] result = new int[shotTransforms.Length];
            Coord start = player.location;
            Coord loc;
            int idx = 0;
            int count;
            foreach (Coord t in shotTransforms)
            {
                loc = start.clone();
                loc = Coord.add(loc, t);
                count = isOnBoard(loc.x, loc.y) ? 1 : 0;
                result[idx] = count;
                idx++;
            }
            return result;
        }

        public ArrayList filterMovesByDir(ArrayList moves, String dir)
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

        public ArrayList filterMovesBySpaces(ArrayList moves, int[] spaces)
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
                    //{N, E, S, W} -> 0, 1, 2, 3
                    int dirIdx = Array.IndexOf(moveDirs, m.dir);

                    //limit in dir
                    int limit = spaces[dirIdx];

                    //target > limit?
                    if (int.Parse(m.target) <= limit) filtered.Add(m);
                }
            }
            return filtered;
        }

        public ArrayList getPossibleMoves(Player p)
        {
            int[] space;
            string[] shotDirs = new string[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
            ArrayList possMoves = (ArrayList)allMoves.Clone();

            space = getSpaceAroundPlayer(p);
            possMoves = filterMovesBySpaces(possMoves, space);

            space = getShotsAroundPlayer(p);
            for (int i = 0; i < space.Length; i++)
            {
                if (space[i] == 0) possMoves = filterMovesByDir(possMoves, shotDirs[i]);
            }

            return possMoves;
        }

        void genAllMoves()
        {

            foreach (String d in shootDirs)
            {
                foreach (String t in shootTargets)
                {
                    allMoves.Add(new Move(d, t));
                }
            }
            foreach (String d in moveDirs)
            {
                foreach (String t in moveTargets)
                {
                    allMoves.Add(new Move(d, t));
                }
            }
        }

        public void print()
        {
            for (int i = 0; i < numPlayers; i++)
            {
                Console.WriteLine("Player " + (i + 1) + " : Health=" + players[i].health + "   Score=" + players[i].score);
            }
            Console.WriteLine("");
            Console.Write("╔");
            for (int x = 0; x < width; x++)
            {
                Console.Write("═══");
            }
            Console.WriteLine("╗");
            for (int y = height - 1; y >= 0; y--)
            {
                Console.Write("║");
                for (int x = 0; x < width; x++)
                {
                    Coord cell = new Coord(x, y);
                    if (cell.isSame(controlLocation))
                    {
                        Console.Write(" § ");
                        continue;
                    }
                    if (cell.isInArray(holeLocations))
                    {
                        Console.Write(" ▓ ");
                    }
                    else
                    {
                        int l = whichPlayerLocation(cell);
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
            for (int x = 0; x < width; x++)
            {
                Console.Write("═══");
            }
            Console.WriteLine("╝");
        }

        int whichPlayerLocation(Coord c)
        {
            for (int i = 0; i < numPlayers; i++)
            {
                if (c.isSame(players[i].location))
                {
                    return players[i].number;
                }
            }
            return -1;
        }

        bool isHoleLocation(Coord c)
        {
            for (int i = 0; i < numHoles; i++)
            {
                if (holeLocations[i] != null && c.isSame(holeLocations[i]))
                {
                    return true;
                }
            }
            return false;
        }

        bool isPlayerLocation(Coord c)
        {
            foreach (Player p in players)
            {
                if (c.isSame(p.location))
                {
                    return true;
                }
            }
            return false;
        }

        void initHoleLocations()
        {
            Random rnd = new Random(3);
            Coord holeLoc;
            for (int i = 0; i < numHoles; i++)
            {
                do
                {
                    holeLoc = new Coord(rnd.Next(1, width - 1), rnd.Next(1, height - 1));
                } while (isHoleLocation(holeLoc));
                holeLocations[i] = holeLoc;
            }
        }

        void setControlLocation(bool initialisation)
        {
            Random rnd = new Random(0);
            Coord controlLoc;
            do
            {
                if (initialisation)
                {
                    controlLoc = new Coord(rnd.Next(1, width - 1), rnd.Next(1, height - 1));
                }
                else
                {
                    controlLoc = new Coord(rnd.Next(0, width), rnd.Next(0, height));
                }
            } while (isHoleLocation(controlLoc) || isPlayerLocation(controlLoc));
            controlLocation = controlLoc;
        }

        void initPlayerLocations()
        {
            for (int i = 0; i < numPlayers; i++)
            {
                playerToSpawn(players[i]);
            }
        }

        void playerToSpawn(Player player)
        {
            int w = (int)Math.Floor(width / 2.0);
            int h = (int)Math.Floor(height / 2.0);
            int r = width - 1;
            int t = height - 1;

            if (player.number == 0)
            {
                player.location = new Coord(w, 0);
            }
            else if (player.number == 1)
            {
                player.location = new Coord(0, h);
            }
            else if (player.number == 2)
            {
                player.location = new Coord(w, t);
            }
            else if (player.number == 3)
            {
                player.location = new Coord(r, h);
            }
        }

        public void executeMoves(Move[,] moves)
        {
            Move[] round;
            for (int i = 0; i < nMoves; i++)
            {
                round = getRankedRound(moves, i);
                for (int j = 0; j < numPlayers; j++)
                {
                    Console.WriteLine("moving player " + (j+1));
                    if (round[j].shoot)
                    {
                        Console.WriteLine("Player " + (round[j].player.number + 1) + " takes their turn... : shooting " + round[j].dir + " at their opponent's " + round[j].target);
                        playerShoot(round[j]);
                    }
                    else
                    {
                        Console.WriteLine("Player " + (round[j].player.number + 1) + " takes their turn... : moving " + round[j].dir + " " + round[j].target + " places");
                        movePlayer(round[j]);
                    }
                    print();
                    Console.ReadKey();
                }
            }
            foreach (Player p in players)
            {
                if (p.location.isSame(controlLocation))
                {
                    Console.WriteLine("Player " + (p.number + 1) + " finished the round on the control point!");
                    p.score++;
                    setControlLocation(false);
                }
            }
        }

        public void playerShoot(Move m)
        {
            Coord nextLoc = m.player.location.clone();
            Coord transform = new Coord(0, 0);

            switch (m.dir)
            {
                case "N":
                    transform = N;
                    break;
                case "NE":
                    transform = NE;
                    break;
                case "E":
                    transform = E;
                    break;
                case "SE":
                    transform = SE;
                    break;
                case "S":
                    transform = S;
                    break;
                case "SW":
                    transform = SW;
                    break;
                case "W":
                    transform = W;
                    break;
                case "NW":
                    transform = NW;
                    break;
            }

            do
            {
                nextLoc = Coord.add(nextLoc, transform);
                int pl = whichPlayerLocation(nextLoc);
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
                                playerToSpawn(players[pl]);
                                m.player.score++;
                            }
                            break;
                        case "LARM":
                            foreach (Move p in players[pl].program)
                            {
                                p.armHit(true);
                            }
                            break;
                        case "RARM":
                            foreach (Move p in players[pl].program)
                            {
                                p.armHit(false);
                            }
                            break;
                        case "LLEG":
                            foreach (Move p in players[pl].program)
                            {
                                p.legHit(true);
                            }
                            break;
                        case "RLEG":
                            foreach (Move p in players[pl].program)
                            {
                                p.legHit(false);
                            }
                            break;
                        case "BODY":
                            Coord pushedLoc = Coord.add(players[pl].location, transform);
                            if (isOnBoard(pushedLoc.x, pushedLoc.y) && !isPlayerLocation(pushedLoc))
                            {
                                if (isHoleLocation(pushedLoc))
                                {
                                    players[pl].health--;
                                    if (players[pl].health <= 0)
                                    {
                                        players[pl].health = 3;
                                        players[pl].score--;
                                    }
                                    playerToSpawn(players[pl]);
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
            } while (isOnBoard(nextLoc.x, nextLoc.y));

        }

        public void movePlayer(Move m)
        {
            Coord nextLoc = m.player.location.clone();
            Coord transform = new Coord(0, 0);
            int spaces = int.Parse(m.target);

            switch (m.dir)
            {
                case "U":
                    transform = U;
                    break;
                case "R":
                    transform = R;
                    break;
                case "D":
                    transform = D;
                    break;
                case "L":
                    transform = L;
                    break;
            }

            while (spaces > 0)
            {
                nextLoc = Coord.add(nextLoc, transform);
                if (isHoleLocation(nextLoc))
                {
                    m.player.health--;
                    if (m.player.health <= 0)
                    {
                        m.player.health = 3;
                        m.player.score--;
                    }
                    playerToSpawn(m.player);
                    Console.WriteLine("Player " + (m.player.number + 1) + " fell down a hole!");
                    break;
                }
                else if (!isOnBoard(nextLoc.x, nextLoc.y) || isPlayerLocation(nextLoc))
                {
                    break;
                }
                spaces--;
                m.player.location = nextLoc;
            }

        }

        public Coord moveToTransform(Move m)
        {
            int dist;
            dist = int.Parse(m.target);
            if (m.dir.Equals("U"))
            {
                return new Coord(0, dist);
            }
            else if (m.dir.Equals("R"))
            {
                return new Coord(dist, 0);
            }
            else if (m.dir.Equals("D"))
            {
                return new Coord(0, -dist);
            }
            else if (m.dir.Equals("L"))
            {
                return new Coord(-dist, 0);
            }
            return null;
        }

        public bool isOnBoard(int x, int y)
        {
            return (x >= 0) && (y >= 0) && (x < width) && (y < height);
        }

        Move[] getRankedRound(Move[,] moves, int number)
        {
            Move[] round = new Move[numPlayers];
            for (int i = 0; i < numPlayers; i++)
            {
                round[i] = moves[i, number];
            }
            //shuffleMoves(round);  need to implement!
            Array.Sort(round, delegate (Move x, Move y) { return x.rank - y.rank; });
            return round;
        }
    }

    class Coord
    {
        public int x;
        public int y;

        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool isSame(Coord c)
        {
            return (c.x == x) && (c.y == y);
        }

        public bool isInArray(Coord[] c)
        {
            for (int i = 0; i < c.Length; i++)
            {
                if (isSame(c[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public int idxInArray(Coord[] c)
        {
            Console.WriteLine("size = " + c.Length);
            for (int i = 0; i < c.Length; i++)
            {
                if (isSame(c[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public Coord clone()
        {
            return new Coord(x, y);
        }

        public static Coord add(Coord a, Coord b)
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
            int nPlayers = 4;
            int nMoves = 1; //change back to 3

            Player[] players = new Player[nPlayers];
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = new Player(i, (i != 0 ? "computer" : "human"));
            }

            Board b = new Board(players);

            for (int i = 0; i < players.Length; i++)
            {
                players[i].setBoard(b);
            }

            printIntro();
            b.print();

            Move[,] programming = new Move[nPlayers, nMoves];
            Move[] temp = new Move[nMoves];

            while (true)
            {
                for (int i = 0; i < nPlayers; i++)
                {
                    temp = players[i].makeMove();
                    for (int j = 0; j < nMoves; j++)
                    {
                        programming[i, j] = temp[j];
                    }
                }
                Console.WriteLine("All programs locked in!");
                b.executeMoves(programming);
            }
        }
        static void printIntro()
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
        static String[] shootDirs = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        static String[] shootTargets = { "HEAD", "BODY", "LARM", "RARM", "LLEG", "RLEG" };
        static int[] targetRanks = { 5, 4, 2, 2, 3, 3 };
        static String[] moveDirs = { "U", "R", "D", "L" };
        static String[] moveTargets = { "1", "2", "3", "4", "5", "6" };

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
            if (Array.IndexOf(shootDirs, dir) != -1)
            {
                shoot = true;
                rank = targetRanks[Array.IndexOf(shootTargets, target)];
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
            if (Array.IndexOf(shootDirs, dir) != -1)
            {
                shoot = true;
                rank = targetRanks[Array.IndexOf(shootTargets, target)];
            }
            else
            {
                shoot = false;
                rank = int.Parse(target);
            }
        }

        public Move clone(){
            return new Move(dir, target);
        }

        public void print(){
            if (shoot)
            {
                Console.WriteLine("Shooting " + dir + " at opponent's " + target);
            }
            else
            {
                Console.WriteLine("Moving " + dir + " " + target + " spaces");
            }            
        }

        public static bool isValidInput(String s)
        {
            if (s.Split(' ').Length < 2) return false;
            String dir = s.Split(' ')[0];
            String target = s.Split(' ')[1];
            return ((arrContainsStr(shootDirs, dir) && arrContainsStr(shootTargets, target)) || (arrContainsStr(moveDirs, dir) && arrContainsStr(moveTargets, target)));
        }

        static bool arrContainsStr(String[] arr, String s)
        {
            foreach (String t in arr)
            {
                if (t.Equals(s)) return true;
            }
            return false;
        }

        public void legHit(bool left)
        {  //moves movements
            if (shoot) return;
            int idx = Array.IndexOf(moveDirs, dir);
            if (left)
            {  //ccw
                int newIdx = mod(idx - 1, moveDirs.Length);
                dir = moveDirs[newIdx];
            }
            else
            {      //cw
                int newIdx = mod(idx + 1, moveDirs.Length);
                dir = moveDirs[newIdx];
            }
        }

        public void armHit(bool left)
        {  //moves shots
            if (!shoot) return;
            int idx = Array.IndexOf(shootDirs, dir);
            if (left)
            {  //ccw
                int newIdx = mod(idx - 1, shootDirs.Length);
                dir = shootDirs[newIdx];
            }
            else
            {      //cw
                int newIdx = mod(idx + 1, shootDirs.Length);
                dir = shootDirs[newIdx];
            }
        }

        int mod(int a, int b)
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
        Board board;
        public Move[] program;

        public Player(int number, String strategy)
        {
            this.number = number;
            this.strategy = strategy;
        }

        public void setBoard(Board b)
        {
            board = b;
        }

        public Move[] humanMove()
        {
            int nMoves = 1;
            Move[] moves = new Move[nMoves];

            String line;
            bool error;
            for (int i = 0; i < nMoves; i++)
            {
                error = false;
                do
                {
                    if (error) Console.WriteLine("Instruction was not valid!");
                    Console.WriteLine("Please enter command #" + (i + 1));
                    line = Console.ReadLine().ToUpper();
                    error = true;
                } while (!Move.isValidInput(line));
                moves[i] = new Move(this, line.Split(' ')[0], line.Split(' ')[1]);
            }
            program = moves;
            return moves;
        }

        public int controlEvalFunction(Coord loc, Coord control)
        {
            return Coord.manhattanDistance(control, loc);
        }

        public Move[] computerMove()
        {
            int nMoves = 1;
            Move[] moves = new Move[nMoves];
            ArrayList possible = board.getPossibleMoves(this);
            int currentBestVal = int.MaxValue;
            Move currentBestMove = (Move)possible[0];
            int value;

            foreach (Move n in possible)
            {
                if (n.shoot) continue;
                Console.Write("testing : ");
                n.print();
                Coord transform = board.moveToTransform(n);
                value = controlEvalFunction(Coord.add(transform, location), board.controlLocation);
                Console.WriteLine("value = " + value);
                if (value < currentBestVal){
                    Console.WriteLine("new best!");
                    currentBestMove = n;
                    currentBestVal = value;
                }
            }
            moves[0] = currentBestMove.clone();
            moves[0].player = this;

            return moves;
        }

        public Move[] makeMove()
        {
            Console.WriteLine("Player " + (number + 1) + "'s programming phase...");
            switch (strategy)
            {
                case "human":
                    return humanMove();
                case "computer":
                    return computerMove();
            }
            return null;
        }

    }
}