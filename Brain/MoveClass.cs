using Chess.Logic;

namespace Chess.Brain
{
    public enum MoveClass
    {
        NORMAL, PAWN_MOVE, CAPTURE, CHECK, PROMOTION, CASTLE
    }

    public static class MoveClassifier//board must be in a state just before the move has been made
    {
        public static MoveClass ClassifyMove(Move move)
        {
            Board.MakeMove(move);
            PieceList allyPieces;
            PieceList enemyPieces;
            if (Board.toStay == Piece.WHITE)
            {
                allyPieces = Board.whitePieces;
                enemyPieces = Board.blackPieces;
            }
            else
            {
                allyPieces = Board.blackPieces;
                enemyPieces = Board.whitePieces;
            }

            ulong attacked = AttackMapper.MapAttacks(Board.toStay, allyPieces, enemyPieces);
            if ((attacked & enemyPieces.kingPosition) != 0)
            {
                return MoveClass.CHECK;
            }
            Board.UnMakeMove();

            if (move.MoveFlag == Move.Flag.Castling)
            {
                return MoveClass.CASTLE;
            }
            if(move.MoveFlag != Move.Flag.None && move.MoveFlag != Move.Flag.EnPassantCapture && move.MoveFlag != Move.Flag.PawnTwoForward)
            {
                return MoveClass.PROMOTION;
            }


            if(move.MoveFlag == Move.Flag.EnPassantCapture || ((1UL << move.TargetSquare) & enemyPieces.allPieces) != 0)
            {
                return MoveClass.CAPTURE;
            }

            if(((1UL << move.StartSquare) & allyPieces.pawns) != 0)
            {
                return MoveClass.PAWN_MOVE;
            }

            return MoveClass.NORMAL;
        }

        public static MoveClass ClassifyLastMove()
        {
            Move move = Board.moveHistory.Peek();
            Board.UnMakeMove();
            return ClassifyMove(move);
        }

        public static bool IsCapture(Move move)
        {
            ulong allPieces = Board.whitePieces.allPieces | Board.blackPieces.allPieces;
            return move.MoveFlag == Move.Flag.EnPassantCapture || (((1UL << move.TargetSquare) & allPieces) != 0);
        }
    }
}
