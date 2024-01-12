using Chess.Logic;
using System;
using System.Collections.Generic;
using System.IO;

namespace Chess.Brain
{
    public class OpeningBook
    {
        //stores all moves found in the opening book available in this continuation
        private List<MoveNode> possibleMoves;
        private Random rand;

        //constructs the opening tree
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
            foreach(string startingMove in startingMoves)
            {
                possibleMoves.Add(new MoveNode(allGames, 0, startingMove));
            }
        }

        //By making a move we narrow down all of the possible moves to possible continuations of the current line
        public void MakeMove(Move move)
        {
            string moveString = move.ToString();

            foreach (MoveNode moveNode in possibleMoves)
            {
                if (moveNode.moveString == moveString)
                {
                    possibleMoves = moveNode.Offspring;
                    return;
                }
            }

            possibleMoves = null;

        }

        //randomly chooses moves from possible continuations
        public string GetNextMove()
        {
            if (possibleMoves == null || possibleMoves.Count == 0)
            {
                return null;
            }
            List<MoveNode> movesWeighted = new List<MoveNode>();
            foreach (MoveNode move in possibleMoves)
            {
                for(int i = 0; i < move.weight; i++)
                {
                    movesWeighted.Add(move);
                }
            }
            int randomIndex = rand.Next(movesWeighted.Count);
            return movesWeighted[randomIndex].moveString;
        }
    }

    //this class could as well be internal to OpeningBook, it represents a move with its string representation, and possible continuations
    //there can be many moves with the same string that occur in diffrent possitions and so have diffrent offspring
    class MoveNode
    {
        public readonly int weight;
        public readonly string moveString;
        public List<MoveNode> Offspring { get; }

        //Moves are constructed from all continuations possible from a given position this is recursive and by running it for all possible ply 1 moves we create a tree of all opening lines 
        //which is later traversed in the opening book class
        public MoveNode(List<string[]> games, int depth, string move)
        {
            moveString = move;
            if (depth == games[0].Length - 1)
            {
                weight = 1;
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

                Offspring = new List<MoveNode>();

                foreach (string line in lines)
                {
                    Offspring.Add(new MoveNode(gamesWithMe, depth + 1, line));
                }

                weight = 1 + Offspring.Count;
            }
        }
    }
}
