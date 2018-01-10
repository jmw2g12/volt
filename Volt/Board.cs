using System;
using System.Collections;

namespace Volt
{
    class Board
    {
        public static ArrayList allMoves = GenAllMoves();
        Player[] players;
        public Coord[] holeLocations;
        public Coord[] controlLocations;

        public bool supressOutput;

        public Board(Player[] players)
        {
            this.players = players;

            InitPlayerLocations();
            InitHoleLocations();
            SetControlLocations(true);
            supressOutput = true;
        }

        public Board(Player[] players, Coord[] holeLocations, Coord[] controlLocations)
        {
            this.players = players;
            this.holeLocations = holeLocations;
            this.controlLocations = controlLocations;
            supressOutput = true;
        }

        public Board Clone()
        {
            Player[] clonedPlayers = new Player[Globals.numPlayers];
            for (int i = 0; i < Globals.numPlayers; i++)
            {
                clonedPlayers[i] = players[i].Clone();
            }

            Coord[] clonedHoleLocations = new Coord[Globals.numHoles];
            for (int i = 0; i < Globals.numHoles; i++)
            {
                clonedHoleLocations[i] = holeLocations[i].Clone();
            }

            Coord[] clonedControlLocations = new Coord[Globals.numControlPoints];
            for (int i = 0; i < Globals.numControlPoints; i++)
            {
                clonedControlLocations[i] = controlLocations[i].Clone();
            }

            return new Board(clonedPlayers, clonedHoleLocations, clonedControlLocations);
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

        ArrayList FilterMovesOnly(ArrayList moves)
        {
            ArrayList filtered = new ArrayList();
            foreach (Move m in moves)
            {
                if (!m.shoot)
                {
                    filtered.Add(m);
                }
            }
            return filtered;
        }

        ArrayList FilterShotsOnly(ArrayList moves)
        {
            ArrayList filtered = new ArrayList();
            foreach (Move m in moves)
            {
                if (m.shoot)
                {
                    filtered.Add(m);
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

            foreach (Move m in possibleMoves)
            {
                m.player = p;
            }

            return possibleMoves;
        }

        public ArrayList[] GetAllPlayersPossibleMoves()
        {
            ArrayList[] result = new ArrayList[Globals.numPlayers];
            int idx = 0;
            foreach (Player p in players)
            {
                result[idx] = GetPossibleMoves(p);
                idx++;
            }
            return result;
        }

        static ArrayList GenAllMoves()
        {
            ArrayList allMovesAL = new ArrayList();
            foreach (String d in Globals.shootDirs)
            {
                foreach (String t in Globals.shootTargets)
                {
                    allMovesAL.Add(new Move(d, t));
                }
            }
            foreach (String d in Globals.moveDirs)
            {
                foreach (String t in Globals.moveTargets)
                {
                    allMovesAL.Add(new Move(d, t));
                }
            }
            return allMovesAL;
        }

        public void Print()
        {
            if (!Globals.supressAllOutput) Console.WriteLine("");
            for (int i = 0; i < Globals.numPlayers; i++)
            {
                if (!Globals.supressAllOutput) Console.WriteLine("Player " + (i + 1) + " : Health=" + players[i].health + "   Score=" + players[i].score);
            }
            if (!Globals.supressAllOutput) Console.WriteLine("");
            if (!Globals.supressAllOutput) Console.Write("╔");
            for (int x = 0; x < Globals.width; x++)
            {
                if (!Globals.supressAllOutput) Console.Write("═══");
            }
            if (!Globals.supressAllOutput) Console.WriteLine("╗");
            for (int y = Globals.height - 1; y >= 0; y--)
            {
                if (!Globals.supressAllOutput) Console.Write("║");
                for (int x = 0; x < Globals.width; x++)
                {
                    Coord cell = new Coord(x, y);
                    if (IsControlLocation(cell))
                    {
                        if (!Globals.supressAllOutput) Console.Write(" § ");
                        continue;
                    }
                    if (cell.IsInArray(holeLocations))
                    {
                        if (!Globals.supressAllOutput) Console.Write(" ▓ ");
                    }
                    else
                    {
                        int l = WhichPlayerLocation(cell);
                        if (l != -1)
                        {
                            if (!Globals.supressAllOutput) Console.Write(" " + (l + 1) + " ");
                        }
                        else
                        {
                            if (!Globals.supressAllOutput) Console.Write(" · "); //×·
                        }
                    }
                }
                if (!Globals.supressAllOutput) Console.WriteLine("║");
            }
            if (!Globals.supressAllOutput) Console.Write("╚");
            for (int x = 0; x < Globals.width; x++)
            {
                if (!Globals.supressAllOutput) Console.Write("═══");
            }
            if (!Globals.supressAllOutput) Console.WriteLine("╝");
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
                        if (!supressOutput && !Globals.supressAllOutput) Console.WriteLine("Player " + (round[j].player.number + 1) + " takes their turn... : shooting " + round[j].dir + " at their opponent's " + round[j].target);
                    }
                    else
                    {
                        if (!supressOutput && !Globals.supressAllOutput) Console.WriteLine("Player " + (round[j].player.number + 1) + " takes their turn... : moving " + round[j].dir + " " + round[j].target + " places");
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
                    if (!supressOutput && !Globals.supressAllOutput) Console.WriteLine("Player " + (p.number + 1) + " finished the round on the control point!");
                    p.score++;
                    SetControlLocations(false);
                }
            }
        }

        public void ExecuteMoves(Move[] round)
        {   //should be for tree traversal use only
            round = GetRankedRound(round);

            Move[] program = new Move[1];
            for (int j = 0; j < Globals.numPlayers; j++)
            {
                round[j].player = players[j];
                program[0] = round[j];
                players[j].program = program;
            }
            for (int i = 0; i < Globals.numPlayers; i++)
            {
                MakeMove(round[i]);
            }

            foreach (Player p in players)
            {
                if (IsControlLocation(p.location))
                {
                    p.score++;
                    SetControlLocations(false);
                }
            }
        }

        public void MakeMove(Move m)
        {
            if (m.shoot)
            {
                PlayerShoot(m);
            }
            else
            {
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
                    if (!supressOutput && !Globals.supressAllOutput) Console.WriteLine("Shot HIT player " + (pl + 1));
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
                                    if (!supressOutput && !Globals.supressAllOutput) Console.WriteLine("Player " + (m.player.number + 1) + " fell down a hole!");
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
                    if (!supressOutput && !Globals.supressAllOutput) Console.WriteLine("Player " + (m.player.number + 1) + " fell down a hole!");
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

        Move[] GetRankedRound(Move[] round)
        {
            //shuffleMoves(round);  need to implement!
            //Array.Sort(round, delegate (Move x, Move y) { return x.rank - y.rank; });
            return round;
        }

        public Coord GetPlayerLocation(int playerNumber)
        {
            return players[playerNumber].location;
        }
    }
}