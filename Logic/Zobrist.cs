using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Logic
{
    public static class Zobrist
    {
        public static ulong blackToMove;
        public static ulong[] enPassantFiles = new ulong[8];
        public static ulong whiteQueenSideCastle;
        public static ulong blackQueenSideCastle;
        public static ulong whiteKingSideCastle;
        public static ulong blackKingSideCastle;
        public static ulong lastEnPassant = 0;

        public static Keys whiteKeys;
        public static Keys blackKeys;
        static Zobrist()
        {
            Random rand = new Random(325519);

            whiteKeys = new Keys(rand);
            blackKeys = new Keys(rand);
            for (int i = 0; i < 8; i++)
            {
                enPassantFiles[i] = NextUlong(rand);
            }

            blackToMove = NextUlong(rand);
            whiteKingSideCastle = NextUlong(rand);
            blackKingSideCastle = NextUlong(rand);
            whiteQueenSideCastle = NextUlong(rand);
            blackQueenSideCastle = NextUlong(rand);
        }

        public static ulong HashCurrentPosition()
        {
            ulong hash = 0;
            for(int i = 0; i < 64; i++)
            {
                uint color = Piece.GetColor(Board.board[i]);
                hash ^= GetPieceNum(Board.board[i], i, color == Piece.WHITE ? whiteKeys : blackKeys);
            }

            if (Board.toMove == Piece.BLACK)
            {
                hash ^= blackToMove;
            }

            int enPassantFile = (int)(((Board.currentGameState & Board.enPassantMask) >> 4) - 1);
            if (enPassantFile > -1)
            {
                hash ^= enPassantFiles[enPassantFile];
                lastEnPassant = enPassantFiles[enPassantFile];
            }

            uint gameState = Board.currentGameState;
            if ((gameState & Board.blackKingsideCastleMask) != 0)
            {
                hash ^= blackKingSideCastle;
            }
            if ((gameState & Board.blackQueensideCastleMask) != 0)
            {
                hash ^= blackQueenSideCastle;
            }
            if ((gameState & Board.whiteKingsideCastleMask) != 0)
            {
                hash ^= whiteKingSideCastle;
            }
            if ((gameState & Board.whiteQueensideCastleMask) != 0)
            {
                hash ^= whiteQueenSideCastle;
            }

            return hash;
        }

        public static ulong HashOutWhiteKingsideCastleMask(ulong hash)
        {
            hash ^= whiteKingSideCastle;
            whiteKingSideCastle = 0;
            return hash;
        }
        public static ulong HashOutWhiteQueensideCastleMask(ulong hash)
        {
            hash ^= whiteQueenSideCastle;
            whiteQueenSideCastle = 0;
            return hash;
        }
        public static ulong HashOutBlackKingsideCastleMask(ulong hash)
        {
            hash ^= blackKingSideCastle;
            blackKingSideCastle = 0;
            return hash;
        }
        public static ulong HashOutBlackQueensideCastleMask(ulong hash)
        {
            hash ^= blackQueenSideCastle;
            blackQueenSideCastle = 0;
            return hash;
        }


        private static ulong NextUlong(Random rand)
        {
            byte[] buffer = new byte[8];
            rand.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static ulong GetPieceNum(uint piece, int i, Keys keys)
        {
            uint pieceType = Piece.GetPiece(piece);

            switch(pieceType)
            {
                case Piece.PAWN:
                    return keys.pawns[i];
                case Piece.KNIGHT:
                    return keys.knights[i];
                case Piece.BISHOP:
                    return keys.bishops[i];
                case Piece.KING:
                    return keys.kings[i];
                case Piece.QUEEN:
                    return keys.queens[i];
                case Piece.ROOK:
                    return keys.rooks[i];
                default:
                    return 0;
            }
        }
    }

    public class Keys
    {
        public ulong[] pawns = new ulong[64];
        public ulong[] bishops = new ulong[64];
        public ulong[] knights = new ulong[64];
        public ulong[] rooks = new ulong[64];
        public ulong[] queens = new ulong[64];
        public ulong[] kings = new ulong[64];
        public Keys(Random rand)
        {
            for (int i = 0; i < 64; i++)
            {
                pawns[i] = NextUlong(rand);
                bishops[i] = NextUlong(rand);
                knights[i] = NextUlong(rand);
                queens[i] = NextUlong(rand);
                kings[i] = NextUlong(rand);
                rooks[i] = NextUlong(rand);
            }
        }

        private static ulong NextUlong(Random rand)
        {
            byte[] buffer = new byte[8];
            rand.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }
    }

}
