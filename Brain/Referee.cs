using Chess.BitMagic;
using Chess.Logic;
using System.Collections.Generic;

namespace Chess.Brain
{
    //this class is used in GUI
    public static class Referee
    {
        public static GameState DetermineGameState(List<Move> moves)
        {
            PieceList toMove;
            PieceList toStay;

            if (Board.repetitionTable[Board.hash] >= 3)
            {
                return GameState.DRAW_BY_REPETITION;
            }

            if(Board.toMove == Piece.WHITE)
            {
                toMove = Board.whitePieces;
                toStay = Board.blackPieces;
            }
            else
            {
                toMove = Board.blackPieces;
                toStay = Board.whitePieces;
            }

            ulong attackMap = AttackMapper.MapAttacks(Board.toStay, toStay, toMove);

            if (moves.Count == 0)
            {
                return (attackMap & toMove.kingPosition) != 0 ? GameState.WIN : GameState.DRAW_BY_STALEMATE;
            }


            int whitePieceCount = BitMagician.CountBits(Board.whitePieces.allPieces);
            int blackPieceCount = BitMagician.CountBits(Board.blackPieces.allPieces);

            if (whitePieceCount == 1 && blackPieceCount == 1)
            {
                return GameState.DRAW_BY_MATERIAL;
            }
            if (whitePieceCount == 2 && blackPieceCount == 2)
            {
                ulong allOrthogonalSliders = toMove.orthogonalSliders | toStay.orthogonalSliders;
                ulong allPawns = toMove.pawns | toStay.pawns;
                if(allOrthogonalSliders == allPawns)
                {
                    return GameState.DRAW_BY_MATERIAL;
                }

            }

            uint halfMoves = Board.currentGameState >> 14;
            return halfMoves >= 50 ? GameState.DRAW_BY_HALFMOVES : GameState.ONGOING;
        }
    }

    public enum GameState
    {
        ONGOING, WIN, DRAW_BY_MATERIAL, DRAW_BY_STALEMATE, DRAW_BY_REPETITION, DRAW_BY_HALFMOVES
    }
}
