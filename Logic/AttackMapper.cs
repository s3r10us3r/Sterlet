using System;

namespace Chess.Logic
{
    public static class AttackMapper
    {
        private static ulong attackMap;
        private static ulong enemyKing;

        private static readonly ulong AFile = 0x101010101010101;
        private static readonly ulong HFile = AFile << 7;

        //this violates oop in a very awful way but I'm lazy
        public static ulong NWSESliderAttacks;
        public static ulong NESWSliderAttacks;
        public static ulong NSSliderAttacks;
        public static ulong EWSliderAttacks;

        public static ulong MapAttacks(uint color, PieceList pieces, PieceList enemyPieces)
        {

            enemyKing = enemyPieces.kingPosition;

            attackMap = 0;


            MapKingAttacks(pieces.kingPosition);
            if (pieces.orthogonalSliders != 0)
            {
                MapOrthogonalAttacks(pieces.orthogonalSliders);
            }

            if (pieces.diagonalSliders != 0)
            {
                MapDiagonallAttacks(pieces.diagonalSliders);
            }

            if (pieces.pawns != 0)
            {
                MapPawnAttacks(pieces.pawns, color);
            }

            if (pieces.knights != 0)
            {
                MapKnightAttacks(pieces.knights);
            }

            return attackMap;
        }

        public static void PrintBitBoard(ulong bitBoard)
        {
            for (int i = 7; i >= 0; i--)
            {
                for (int j = 0; j < 8; j++)
                {
                    ulong bit = 1UL << i * 8 + j;
                    if ((bit & bitBoard) != 0)
                    {
                        Console.Write('1');
                    }
                    else
                    {
                        Console.Write('0');
                    }
                }
                Console.Write('\n');
            }

            Console.WriteLine();
        }


        public static void MapPawnAttacks(ulong pawns, uint color)
        {
            if (color == Piece.WHITE)
            {
                attackMap |= (pawns & ~AFile) << 7;
                attackMap |= (pawns & ~HFile) << 9;
            }
            else
            {
                attackMap |= (pawns & ~AFile) >> 9;
                attackMap |= (pawns & ~HFile) >> 7;
            }

        }

        public static ulong GetPawnAttacks(ulong pawns, uint color)
        {
            ulong attackMap = 0;
            if (color == Piece.WHITE)
            {
                attackMap |= (pawns & ~AFile) << 7;
                attackMap |= (pawns & ~HFile) << 9;
            }
            else
            {
                attackMap |= (pawns & ~AFile) >> 9;
                attackMap |= (pawns & ~HFile) >> 7;
            }
            return attackMap;
        }

        public static void MapDiagonallAttacks(ulong diagonalls)
        {
            ulong allPieces = (Board.blackPieces.allPieces | Board.whitePieces.allPieces) ^ enemyKing;

            NWSESliderAttacks = 0;
            NESWSliderAttacks = 0;

            ulong diagonalMap = diagonalls;
            //N+W
            while (diagonalMap != 0)
            {
                diagonalMap &= ~AFile;
                diagonalMap <<= 7;
                attackMap |= diagonalMap;
                NWSESliderAttacks |= diagonalMap;
                diagonalMap &= ~allPieces;
            }

            diagonalMap = diagonalls;
            //S+W
            while (diagonalMap != 0)
            {
                diagonalMap &= ~AFile;
                diagonalMap >>= 9;
                attackMap |= diagonalMap;
                NESWSliderAttacks |= diagonalMap;
                diagonalMap &= ~allPieces;
            }

            diagonalMap = diagonalls;
            //N+E
            while (diagonalMap != 0)
            {
                diagonalMap &= ~HFile;
                diagonalMap <<= 9;
                attackMap |= diagonalMap;
                NESWSliderAttacks |= diagonalMap;
                diagonalMap &= ~allPieces;
            }

            diagonalMap = diagonalls;
            //S+E
            while (diagonalMap != 0)
            {
                diagonalMap &= ~HFile;
                diagonalMap >>= 7;
                attackMap |= diagonalMap;
                NWSESliderAttacks |= diagonalMap;
                diagonalMap &= ~allPieces;
            }
        }

        public static void MapOrthogonalAttacks(ulong orthogonals)
        {
            ulong allPiecesMask = ~((Board.blackPieces.allPieces | Board.whitePieces.allPieces) ^ enemyKing);
            NSSliderAttacks = 0;
            EWSliderAttacks = 0;
            ulong orthogonalMap = orthogonals;

            //N
            while (orthogonalMap != 0)
            {
                orthogonalMap <<= 8;
                attackMap |= orthogonalMap;
                NSSliderAttacks |= orthogonalMap;
                orthogonalMap &= allPiecesMask;
            }

            //S
            orthogonalMap = orthogonals;
            while (orthogonalMap != 0)
            {
                orthogonalMap >>= 8;
                attackMap |= orthogonalMap;
                NSSliderAttacks |= orthogonalMap;
                orthogonalMap &= allPiecesMask;
            }

            //W
            orthogonalMap = orthogonals;
            while (orthogonalMap != 0)
            {
                orthogonalMap &= ~AFile;
                orthogonalMap >>= 1;
                attackMap |= orthogonalMap;
                EWSliderAttacks |= orthogonalMap;
                orthogonalMap &= allPiecesMask;
            }

            //E
            orthogonalMap = orthogonals;
            while (orthogonalMap != 0)
            {
                orthogonalMap &= ~HFile;
                orthogonalMap <<= 1;
                attackMap |= orthogonalMap;
                EWSliderAttacks |= orthogonalMap;
                orthogonalMap &= allPiecesMask;
            }
        }


        public static ulong MapNWSliderAttacks(ulong king)
        {
            ulong allPieces = (Board.blackPieces.allPieces | Board.whitePieces.allPieces) ^ enemyKing;

            ulong kingAttacks = 0;

            ulong diagonalMap = king;
            //N+W
            while (diagonalMap != 0)
            {
                diagonalMap &= ~AFile;
                diagonalMap <<= 7;
                kingAttacks |= diagonalMap;
                diagonalMap &= ~allPieces;
            }

            return kingAttacks;
        }

        public static ulong MapSESliderAttacks(ulong king)
        {
            ulong allPieces = (Board.blackPieces.allPieces | Board.whitePieces.allPieces) ^ enemyKing;

            ulong kingAttacks = 0;
            ulong diagonalMap = king;
            //S+E
            while (diagonalMap != 0)
            {
                diagonalMap &= ~HFile;
                diagonalMap >>= 7;
                kingAttacks |= diagonalMap;
                diagonalMap &= ~allPieces;
            }

            return kingAttacks;
        }


        public static ulong MapSWSliderAttacks(ulong king)
        {
            ulong allPieces = (Board.blackPieces.allPieces | Board.whitePieces.allPieces) ^ enemyKing;

            ulong kingAttacks = 0;

            ulong diagonalMap = king;

            while (diagonalMap != 0)
            {
                diagonalMap &= ~AFile;
                diagonalMap >>= 9;
                kingAttacks |= diagonalMap;
                diagonalMap &= ~allPieces;
            }

            return kingAttacks;
        }


        public static ulong MapNESliderAttacks(ulong king)
        {
            ulong allPieces = (Board.blackPieces.allPieces | Board.whitePieces.allPieces) ^ enemyKing;

            ulong kingAttacks = 0;

            ulong diagonalMap = king;


            while (diagonalMap != 0)
            {
                diagonalMap &= ~HFile;
                diagonalMap <<= 9;
                kingAttacks |= diagonalMap;
                diagonalMap &= ~allPieces;
            }

            return kingAttacks;
        }

        public static ulong MapNSliderAttacks(ulong king)
        {
            ulong allPiecesMask = ~((Board.blackPieces.allPieces | Board.whitePieces.allPieces) ^ enemyKing);

            ulong orthogonalMap = king;
            ulong kingAttack = 0;

            while (orthogonalMap != 0)
            {
                orthogonalMap <<= 8;
                kingAttack |= orthogonalMap;
                orthogonalMap &= allPiecesMask;
            }
            return kingAttack;
        }

        public static ulong MapSSliderAttacks(ulong king)
        {
            ulong allPiecesMask = ~((Board.blackPieces.allPieces | Board.whitePieces.allPieces) ^ enemyKing);

            ulong orthogonalMap = king;
            ulong kingAttack = 0;

            while (orthogonalMap != 0)
            {
                orthogonalMap >>= 8;
                kingAttack |= orthogonalMap;
                orthogonalMap &= allPiecesMask;
            }

            return kingAttack;
        }

        public static ulong MapWSliderAttacks(ulong king)
        {
            ulong allPiecesMask = ~((Board.blackPieces.allPieces | Board.whitePieces.allPieces) ^ enemyKing);

            ulong attacks = 0;

            ulong orthogonalMap = king;
            //W
            while (orthogonalMap != 0)
            {
                orthogonalMap &= ~AFile;
                orthogonalMap >>= 1;
                attacks |= orthogonalMap;
                orthogonalMap &= allPiecesMask;
            }
            return attacks;
        }

        public static ulong MapESliderAttacks(ulong king)
        {
            ulong allPiecesMask = ~((Board.blackPieces.allPieces | Board.whitePieces.allPieces) ^ enemyKing);

            ulong attacks = 0;

            ulong orthogonalMap = king;
            while (orthogonalMap != 0)
            {
                orthogonalMap &= ~HFile;
                orthogonalMap <<= 1;
                attacks |= orthogonalMap;
                orthogonalMap &= allPiecesMask;
            }

            return attacks;
        }

        public static void MapKnightAttacks(ulong knights)
        {
            ulong knightsMap = knights;

            ulong BFile = AFile << 1;
            ulong GFile = HFile >> 1;

            knightsMap &= ~HFile;
            //N+N+E
            attackMap |= knightsMap << 17;
            //S+S+E
            attackMap |= knightsMap >> 15;

            knightsMap = knights & ~AFile;
            //N+N+W
            attackMap |= knightsMap << 15;
            //S+S+W
            attackMap |= knightsMap >> 17;

            knightsMap = knights & ~(AFile | BFile);
            //N+W+W
            attackMap |= knightsMap << 6;
            //S+W+W
            attackMap |= knightsMap >> 10;

            knightsMap = knights & ~(GFile | HFile);
            //N+E+E
            attackMap |= knightsMap << 10;
            //S+E+E
            attackMap |= knightsMap >> 6;
        }

        public static void MapKingAttacks(ulong king)
        {
            ulong kingMap = king & ~AFile;

            attackMap |= kingMap << 7;
            attackMap |= kingMap >> 9;
            attackMap |= kingMap >> 1;

            kingMap = king & ~HFile;
            attackMap |= kingMap >> 7;
            attackMap |= kingMap << 9;
            attackMap |= kingMap << 1;

            attackMap |= king << 8;
            attackMap |= king >> 8;
        }
    }
}
