using System;

namespace Volt
{
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
                if (!Globals.supressAllOutput) Console.WriteLine("Shooting " + dir + " at opponent's " + target);
            }
            else
            {
                if (!Globals.supressAllOutput) Console.WriteLine("Moving " + dir + " " + target + " spaces");
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
}