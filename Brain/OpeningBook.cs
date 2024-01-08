using Chess.Logic;
using System;
using System.Collections.Generic;
using System.IO;

namespace Chess.Brain
{
    public class OpeningBook
    {
        private List<MoveNode> possibleMoves;
        Random rand;

        public OpeningBook(string input)
        {
            possibleMoves = new List<MoveNode>();
            rand = new Random();

            List<string[]> allGames = new List<string[]>();
            HashSet<string> startingMoves = new HashSet<string>();

            using(StreamReader sr = new StreamReader(input))
            {
                string line;
                while((line = sr.ReadLine()) != null)
                {
                    string[] game = line.Split(' ');
                    allGames.Add(game);
                    startingMoves.Add(game[0]);
                }
            }
            Console.WriteLine($"first move opening nums {startingMoves.Count}");
            foreach(string startingMove in startingMoves)
            {
                possibleMoves.Add(new MoveNode(allGames, 0, startingMove));
            }
        }

        public void MakeMove(Move move)
        {
            List<MoveNode> possible = new List<MoveNode>();
            string moveString = move.ToString();

            foreach (MoveNode moveNode in possibleMoves)
            {
                if (moveNode.moveString == moveString)
                {
                    possible.Add(moveNode);
                }
            }

            if(possible.Count == 0)
            {
                possibleMoves = null;
                return;
            }

            int randomIndex = rand.Next(possible.Count);
            possibleMoves = possible[randomIndex].Offspring;
        }

        public string GetNextMove()
        {
            if(possibleMoves == null || possibleMoves.Count == 0)
            {
                return null;
            }
            int randomIndex = rand.Next(possibleMoves.Count);
            return possibleMoves[randomIndex].moveString;
        }
    }

    class MoveNode
    {
        public readonly string moveString;
        public List<MoveNode> Offspring { get; }

        public MoveNode(List<string[]> games, int depth, string move)
        {
            moveString = move;
            if(depth == games[0].Length - 1)
            {
                Offspring = null;
            }
            else
            {
                List<string[]> gamesWithMe = new List<string[]>();
                HashSet<string> lines = new HashSet<string>();
                foreach(string[] game in games)
                {
                    if(game[depth] == move)
                    {
                        gamesWithMe.Add(game);
                        lines.Add(game[depth + 1]);
                    }
                }
                //Console.WriteLine(lines.Count);
                Offspring = new List<MoveNode>();

                foreach(string line in lines)
                {
                    Offspring.Add(new MoveNode(gamesWithMe, depth + 1, line));
                }
            }
        }
    }
}
