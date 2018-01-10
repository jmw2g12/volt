using System;

namespace Volt
{
    class Evaluator
    {
        String strategy;
        public Evaluator(String strategy)
        {
            this.strategy = strategy;
        }
        public int Evaluate(Board board, int playerNum)
        {
            switch (strategy)
            {
                case "manhattan":
                    int result = DistToControlPoint(board.GetPlayerLocation(playerNum), board.controlLocations);
                    return result;
            }
            throw new Exception();
        }
        int DistToControlPoint(Coord playerLocation, Coord[] controlPoints)
        {
            int lowest = int.MaxValue;
            int value;
            foreach (Coord cp in controlPoints)
            {
                value = Coord.ManhattanDistance(cp, playerLocation);
                if (value < lowest)
                {
                    lowest = value;
                }
            }
            if (lowest == 0) return (int)Math.Round(1.5 * (float)(Globals.width + Globals.height));
            return (Globals.width + Globals.height) - lowest;
        }
    }
}