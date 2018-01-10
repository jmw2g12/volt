using System;

namespace Volt
{
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

        public static int ManhattanDistance(Coord a, Coord b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        public override string ToString()
        {
            return x + ", " + y;
        }
    }
}