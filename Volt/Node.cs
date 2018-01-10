using System;
using System.Collections;

namespace Volt
{
    class Node
    {
        public Board board;
        public int score;
        public int games;
        public int depth;
        public int playerNumber;
        public Node parent;
        static int maxDepth = 1;
        static float tuneableParam = 1;
        public int totalPlays;
        public ArrayList[] possibleMoves;
        public int numPossibleMoves;
        public int[] moveScores;
        public int[] movePlays;
        public string banditStrategy;
        public Random rnd = new Random();

        public bool isRoot = true;

        public Node(Board board, int playerNumber, string banditStrategy)
        {
            this.board = board;
            this.possibleMoves = board.GetAllPlayersPossibleMoves();
            this.numPossibleMoves = possibleMoves[playerNumber].Count;
            this.moveScores = new int[numPossibleMoves];
            this.movePlays = new int[numPossibleMoves];
            this.playerNumber = playerNumber;
            this.banditStrategy = banditStrategy;
            this.depth = 0;
        }

        Node(Board board, Node n)
        { // Make child, should be only used within class
            this.board = board;
            this.score = n.score;
            this.games = n.games;
            this.depth = n.depth + 1;
            this.playerNumber = n.playerNumber;
            this.parent = n;
            this.possibleMoves = board.GetAllPlayersPossibleMoves();
            isRoot = false;
        }

        public Move RunMCTS(int limit)
        {
            limit += numPossibleMoves;
            totalPlays = 0;
            for (int i = 0; i < numPossibleMoves; i++)
            {
                //playout(move[i])
                int moveScore = Playout((Move)possibleMoves[playerNumber][i]);
                totalPlays++;
                //update average score in moveScores[index of random]
                moveScores[i] += moveScore;
                movePlays[i]++;
                //if (!Globals.supressAllOutput) Console.WriteLine();
            }

            int idx;
            while (totalPlays < limit)
            {
                idx = IdxFromBanditStrategy();

                Move m = (Move)possibleMoves[playerNumber][idx];
                int moveScore = Playout(m);
                totalPlays++;
                moveScores[idx] += moveScore;
                movePlays[idx]++;
                //if (!Globals.supressAllOutput) Console.WriteLine();
            }

            /* For surface level analysis of move choice
            for (int i = 0; i < numPossibleMoves; i++){
                if (!Globals.supressAllOutput) Console.WriteLine("");
                ((Move)possibleMoves[playerNumber][i]).Print();
                if (!Globals.supressAllOutput) Console.WriteLine("scores: " + moveScores[i]);
                if (!Globals.supressAllOutput) Console.WriteLine("plays: " + movePlays[i]);
                if (!Globals.supressAllOutput) Console.WriteLine("score / plays = " + (float)((float)moveScores[i] / (float)movePlays[i]) + (GetBestMove() == i ? " ***BEST*** " : ""));
            }
            */

            return ((Move)possibleMoves[playerNumber][GetBestMove()]).Clone();
        }

        int IdxFromBanditStrategy()
        {
            switch (banditStrategy)
            {
                case "ucb":
                    return IdxFromUCB();
                case "egreedy":
                    return IdxFromEpsilonGreedy();
                default:
                    return -1;
            }
        }

        int IdxFromUCB()
        {
            double bestUCB = double.MinValue;
            int bestUCBIdx = 0;
            double UCB;
            for (int i = 0; i < numPossibleMoves; i++)
            {
                UCB = moveScores[i] + tuneableParam * Math.Sqrt(Math.Log(totalPlays) / movePlays[i]);
                if (UCB > bestUCB)
                {
                    bestUCB = UCB;
                    bestUCBIdx = i;
                }
            }
            return bestUCBIdx;
        }

        int IdxFromEpsilonGreedy()
        {
            if (rnd.NextDouble() < Globals.epsilon)
            {
                return rnd.Next(numPossibleMoves);
            }
            else
            {
                return GetBestMove();
            }
        }

        int GetBestMove()
        {
            float bestAvgScore = float.MinValue;
            int bestIdx = 0;
            float moveScore;
            for (int i = 0; i < numPossibleMoves; i++)
            {
                moveScore = (float)moveScores[i] / (float)movePlays[i];
                if (moveScore > bestAvgScore)
                {
                    bestAvgScore = moveScore;
                    bestIdx = i;
                }
            }
            return bestIdx;
        }

        public int Playout(Move m)
        {
            if (depth < maxDepth)
            {
                Node child = MakeMoveWithRandomRound(m);
                return child.Playout();
            }
            else
            {
                return EvaluateLeaf();
            }
        }

        public int Playout()
        {
            if (depth < maxDepth)
            {
                Node child = RandomRound();
                return child.Playout();
            }
            else
            {
                return EvaluateLeaf();
            }
        }

        public Move PlayerMoveFromIdx(int idx)
        {
            Move m = (Move)possibleMoves[playerNumber][idx];

            return m;
        }

        public Move[] GetRandomMoves()
        {
            Move[] moves = new Move[Globals.numPlayers];
            for (int i = 0; i < Globals.numPlayers; i++)
            {
                bool shoot = rnd.Next(3) == 0; // 33% chance
                do
                {
                    moves[i] = ((Move)possibleMoves[i][rnd.Next(possibleMoves[i].Count)]).Clone();
                } while (moves[i].shoot != shoot);
            }
            return moves;
        }

        public Node MakeMoveWithRandomRound(Move m)
        {
            Move[] round = GetRandomMoves();
            round[playerNumber] = m.Clone();

            Board nextBoard = board.Clone();
            nextBoard.ExecuteMoves(round);

            Node nextNode = new Node(nextBoard, this);
            return nextNode;
        }

        public Node RandomRound()
        {

            Move[] round = GetRandomMoves();

            Board nextBoard = board.Clone();

            nextBoard.ExecuteMoves(round);

            Node nextNode = new Node(nextBoard, this);
            return nextNode;
        }

        public int EvaluateLeaf()
        {
            Evaluator e = new Evaluator("manhattan");
            int newScore = e.Evaluate(board, playerNumber);
            return newScore;
        }

        public void BackPropagate(int score, int games)
        {
            if (!isRoot)
            {
                this.score = score;
                this.games = games;
                parent.BackPropagate(score, games);
            }
        }
    }
}