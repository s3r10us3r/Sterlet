using Chess.gui;
using Chess.Logic;

namespace Chess.Abstracts
{
    public abstract class Player
    {
        public abstract Move ChooseMove();

        public abstract PieceImage GetPiece(uint piece, int fields);

        public abstract void RemovePiece(PieceImage piece);
    }
}
