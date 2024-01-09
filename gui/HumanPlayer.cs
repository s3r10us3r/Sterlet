using Chess.Abstracts;
using Chess.Logic;
using System.Collections.Generic;
using System.Threading;

namespace Chess.gui
{
    public class HumanPlayer : IPlayer
    {
        private HashSet<PieceImage> pieces = new HashSet<PieceImage>();
        private Move chosenMove = null;
        public List<Move> AvailableMoves { get; private set; }

        public Move ChooseMove()
        {
            chosenMove = null;

            AvailableMoves = MoveGenerator.GenerateMoves();

            foreach (PlayerControlledPiece piece in pieces)
            {
                piece.UnlockDragging();
            }

            while (chosenMove == null)
            {
                Thread.Sleep(30);
            }

            foreach (PlayerControlledPiece piece in pieces)
            {
                piece.LockDragging();
            }
            
            return chosenMove;
        }

        public PieceImage GetPiece(uint piece, int field)
        {
            PlayerControlledPiece pieceImage = new PlayerControlledPiece(piece, field, this);
            pieces.Add(pieceImage);
            return pieceImage;
        }

        public void RemovePiece(PieceImage piece)
        {
            pieces.Remove(piece);
        }

        public void SetChosenMove(Move move)
        {
            chosenMove = move;
        }
    }
}
