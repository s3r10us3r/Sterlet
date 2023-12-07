using System;

namespace Chess.Logic
{
    public class PieceList
    {
        public ulong allPieces = 0;
        public ulong diagonalSliders = 0;
        public ulong orthogonalSliders = 0;
        public ulong pawns = 0;
        public ulong knights = 0;
        public ulong kingPosition = 0;

        public PieceList(uint[] board, uint color)
        {
            for (int i = 0; i < 64; i++)
            {
                uint piece = board[i];
                if (Piece.GetColor(piece) == color)
                {
                    uint pieceType = Piece.GetPiece(piece);
                    ulong positionBit = 1UL << i;
                    switch (pieceType)
                    {
                        case Piece.PAWN:
                            pawns |= positionBit;
                            break;
                        case Piece.KNIGHT:
                            knights |= positionBit;
                            break;
                        case Piece.QUEEN:
                            diagonalSliders |= positionBit;
                            orthogonalSliders |= positionBit;
                            break;
                        case Piece.ROOK:
                            orthogonalSliders |= positionBit;
                            break;
                        case Piece.BISHOP:
                            diagonalSliders |= positionBit;
                            break;
                        case Piece.KING:
                            kingPosition |= positionBit;
                            break;
                    }
                    allPieces = diagonalSliders | orthogonalSliders | pawns | knights | kingPosition;
                }
            }
        }


        public void printAllBitboards()
        {
            Console.WriteLine("PIECES:");
            printBitBoard(pawns);
            printBitBoard(knights);
            printBitBoard(diagonalSliders);
            printBitBoard(orthogonalSliders);
            printBitBoard(kingPosition);
            printBitBoard(allPieces);
        }

        public void RemovePieceAtField(int field, uint pieceType)
        {
            ulong fieldBit = 1UL << field;
            switch (pieceType)
            {
                case Piece.PAWN:
                    pawns ^= fieldBit;
                    break;
                case Piece.KNIGHT:
                    knights ^= fieldBit;
                    break;
                case Piece.QUEEN:
                    diagonalSliders ^= fieldBit;
                    orthogonalSliders ^= fieldBit;
                    break;
                case Piece.ROOK:
                    orthogonalSliders ^= fieldBit;
                    break;
                case Piece.BISHOP:
                    diagonalSliders ^= fieldBit;
                    break;
                case Piece.KING:
                    kingPosition ^= fieldBit;
                    break;
            }

            allPieces ^= fieldBit;
        }

        public void AddPieceAtField(int field, uint pieceType)
        {
            ulong fieldBit = 1UL << field;
            switch (pieceType)
            {
                case Piece.PAWN:
                    pawns |= fieldBit;
                    break;
                case Piece.KNIGHT:
                    knights |= fieldBit;
                    break;
                case Piece.QUEEN:
                    diagonalSliders |= fieldBit;
                    orthogonalSliders |= fieldBit;
                    break;
                case Piece.ROOK:
                    orthogonalSliders |= fieldBit;
                    break;
                case Piece.BISHOP:
                    diagonalSliders |= fieldBit;
                    break;
                case Piece.KING:
                    kingPosition |= fieldBit;
                    break;
            }

            allPieces |= fieldBit;
        }

        public void MovePieceToField(int start, int target, uint pieceType)
        {
            ulong negatedStart = ~(1UL << start);
            ulong targetBit = 1UL << target;

            switch (pieceType)
            {
                case Piece.PAWN:
                    pawns &= negatedStart;
                    pawns |= targetBit;
                    break;
                case Piece.KNIGHT:
                    knights &= negatedStart;
                    knights |= targetBit;
                    break;
                case Piece.QUEEN:
                    diagonalSliders &= negatedStart;
                    orthogonalSliders &= negatedStart;
                    diagonalSliders |= targetBit;
                    orthogonalSliders |= targetBit;
                    break;
                case Piece.ROOK:
                    orthogonalSliders &= negatedStart;
                    orthogonalSliders |= targetBit;
                    break;
                case Piece.BISHOP:
                    diagonalSliders &= negatedStart;
                    diagonalSliders |= targetBit;
                    break;
                case Piece.KING:
                    kingPosition &= negatedStart;
                    kingPosition |= targetBit;
                    break;
            }

            allPieces &= negatedStart;
            allPieces |= targetBit;
        }


        private void printBitBoard(ulong bitBoard)
        {
            for (int i = 7; i >= 0; i--)
            {
                for (int j = 0; j < 8; j++)
                {
                    ulong bit = 1UL << i * 8 + j;
                    if ((bit & bitBoard) != 0)
                        Console.Write('1');
                    else
                        Console.Write('0');
                }
                Console.Write('\n');
            }

            Console.WriteLine();
        }
    }
}
