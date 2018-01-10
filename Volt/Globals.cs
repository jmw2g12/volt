using System;

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

        public static bool supressAllOutput = false;

        public static int width = 9;
        public static int height = 9;
        public static int numHoles = 5;
        public static int numControlPoints = 1;

        public static int MCTSLimit = 20000;
        public static float epsilon = 0.66F;

        public static int maxTurns = 10;
    }
}