using Chess.gui;
using Chess.Logic;

namespace Chess.Abstracts
{
    public interface IPlayer
    {
        Move ChooseMove();

        PieceImage GetPiece(uint piece, int fields);

        void RemovePiece(PieceImage piece);
    }
}
