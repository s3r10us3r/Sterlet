using Chess.BitMagic;
using System.Collections.Generic;

namespace Chess.Logic
{
    static public class MoveGenerator
    {
        private static readonly int N = 8, S = -8, E = 1, W = -1;
        private static readonly ulong whiteKingsideCastleFields = 96;
        private static readonly ulong whiteQueensideCastleFields = 12;
        private static readonly ulong blackKingsideCastleFields = whiteKingsideCastleFields << 56;
        private static readonly ulong blackQueensideCastleFields = whiteQueensideCastleFields << 56;

        public static ulong enemyAttackMap;
        public static ulong allyAttackMap;
        private static ulong possibleFields = 0xFFFFFFFFFFFFFFFF;
        private static ulong enemyMask;
        private static ulong allyMask;

        private static ulong canMoveInNESW = 0;
        private static ulong canMoveInNWSE = 0;
        private static ulong canMoveInNS = 0;
        private static ulong canMoveInWE = 0;

        private static ulong canPawnCaptureEast = 0;
        private static ulong canPawnCaptureWest = 0;

        private static readonly ulong AFile = 0x101010101010101;
        private static readonly ulong HFile = AFile << 7;
        private static readonly ulong AFileMask = ~AFile;
        private static readonly ulong HFileMask = ~HFile;

        private static int kingField;
        private static int pawnDirection;
        private static int enPassantFile = -1;
        private static int enPassantField;

        private static ulong kingN;
        private static ulong kingS;
        private static ulong kingE;
        private static ulong kingW;
        private static ulong kingNW;
        private static ulong kingNE;
        private static ulong kingSW;
        private static ulong kingSE;

        private static PieceList pieces;
        private static PieceList enemyPieces;


        public static List<Move> GenerateMoves()
        {
            enPassantFile = (int)((Board.currentGameState & Board.enPassantMask) >> 4) - 1;

            possibleFields = 0xFFFFFFFFFFFFFFFF;
            if (Board.toMove == Piece.WHITE)
            {
                enemyAttackMap = AttackMapper.MapAttacks(Piece.BLACK, Board.blackPieces, Board.whitePieces);
                pieces = Board.whitePieces;
                enemyPieces = Board.blackPieces;
                pawnDirection = N;
                enPassantField = 40 + enPassantFile;
            }
            else
            {
                enemyAttackMap = AttackMapper.MapAttacks(Piece.WHITE, Board.whitePieces, Board.blackPieces);
                pieces = Board.blackPieces;
                enemyPieces = Board.whitePieces;
                pawnDirection = S;
                enPassantField = 16 + enPassantFile;
            }

            FindPins(pieces.kingPosition);

            if ((pieces.kingPosition & enemyAttackMap) != 0)
            {
                kingField = -1;
                ulong testBit = 1;
                for (int i = 0; i < 64; i++)
                {
                    if ((pieces.kingPosition & testBit) != 0)
                    {
                        kingField = i;
                        break;
                    }
                    testBit <<= 1;
                }


                ulong diagonallAttackers = 0;
                ulong orthogonalAttackers = 0;
                ulong knightAttackers = 0;
                ulong pawnAttackers = 0;
                diagonallAttackers |= FindDiagonallsThatMoveToField(kingField, enemyPieces.diagonalSliders);
                orthogonalAttackers |= FindOrthogonalsThatMoveToField(kingField, enemyPieces.orthogonalSliders);
                knightAttackers |= FindKnightsThatMoveToField(kingField, enemyPieces.knights);
                pawnAttackers |= FindPawnsThatCaptureOnField(kingField, enemyPieces.pawns, Board.toStay);

                ulong allAttackers = diagonallAttackers | orthogonalAttackers | knightAttackers | pawnAttackers;
                int attackersNum = BitMagician.CountBits(allAttackers);

                if (attackersNum > 1)
                {
                    return GenerateDoubleCheckMoves(kingField);
                }
                else
                {
                    return GenerateCheckMoves(allAttackers);
                }
            }
            else
            {
                return GenerateUnCheckedMoves();
            }
        }

        private static List<Move> GenerateUnCheckedMoves()
        {
            List<Move> moveList = new List<Move>();
            allyMask = ~pieces.allPieces;
            enemyMask = ~enemyPieces.allPieces;


            ulong squareBitboard = 1UL;

            for (int i = 0; i < 64; i++)
            {
                if ((squareBitboard & pieces.allPieces) != 0)
                {
                    if ((squareBitboard & pieces.orthogonalSliders) != 0)
                    {
                        GenerateOrthogonalSliderMoves(i, moveList);
                    }
                    if ((squareBitboard & pieces.diagonalSliders) != 0)
                    {
                        GenerateDiagonalSliderMoves(i, moveList);
                    }
                    else if ((squareBitboard & pieces.kingPosition) != 0)
                    {
                        GenerateKingMoves(i, moveList);
                    }
                    else if ((squareBitboard & pieces.pawns) != 0)
                    {
                        GeneratePawnMoves(i, moveList);
                    }
                    else if ((squareBitboard & pieces.knights) != 0)
                    {
                        GenerateKnightMoves(i, moveList);
                    }
                }
                squareBitboard <<= 1;
            }

            return moveList;
        }

        private static List<Move> GenerateCheckMoves(ulong attacker)
        {
            if ((attacker & (enemyPieces.pawns | enemyPieces.knights)) != 0)
            {
                possibleFields &= attacker;
            }
            else if ((attacker & kingN) != 0)
            {
                possibleFields &= (AttackMapper.NSSliderAttacks & kingN & ~pieces.allPieces) | attacker;
            }
            else if ((attacker & kingS) != 0)
            {
                possibleFields &= (AttackMapper.NSSliderAttacks & kingS & ~pieces.allPieces) | attacker;
            }
            else if ((attacker & kingE) != 0)
            {
                possibleFields &= (AttackMapper.EWSliderAttacks & kingE & ~pieces.allPieces) | attacker;
            }
            else if ((attacker & kingW) != 0)
            {
                possibleFields &= (AttackMapper.EWSliderAttacks & kingW & ~pieces.allPieces) | attacker;
            }
            else if ((attacker & kingNW) != 0)
            {
                possibleFields &= (AttackMapper.NWSESliderAttacks & kingNW & ~pieces.allPieces) | attacker;
            }
            else if ((attacker & kingSE) != 0)
            {
                possibleFields &= (AttackMapper.NWSESliderAttacks & kingSE & ~pieces.allPieces) | attacker;
            }
            else if ((attacker & kingNE) != 0)
            {
                possibleFields &= (AttackMapper.NESWSliderAttacks & kingNE & ~pieces.allPieces) | attacker;
            }
            else
            {
                possibleFields &= (AttackMapper.NESWSliderAttacks & kingSW & ~pieces.allPieces) | attacker;
            }

            return GenerateUnCheckedMoves();
        }

        private static List<Move> GenerateDoubleCheckMoves(int kingPosition)
        {
            List<Move> moveList = new List<Move>();
            GenerateKingMoves(kingPosition, moveList);
            return moveList;
        }

        private static void GeneratePawnMoves(int field, List<Move> moveList)
        {
            int moveField = field + pawnDirection;
            ulong moveBitboard = 1UL << moveField;
            ulong fieldBitboard = 1UL << field;

            int rankBeforePromotion = Board.toMove == Piece.WHITE ? 6 : 1;
            int startRank = Board.toMove == Piece.WHITE ? 1 : 6;

            int currentRank = field / 8;
            if ((fieldBitboard & canMoveInNS) != 0 && (moveBitboard & (pieces.allPieces | enemyPieces.allPieces)) == 0)
            {
                if ((moveBitboard & possibleFields) != 0)
                {
                    if (currentRank == rankBeforePromotion)
                    {
                        moveList.Add(new Move(field, moveField, Move.Flag.PromoteToBishop));
                        moveList.Add(new Move(field, moveField, Move.Flag.PromoteToQueen));
                        moveList.Add(new Move(field, moveField, Move.Flag.PromoteToRook));
                        moveList.Add(new Move(field, moveField, Move.Flag.PromoteToKnight));
                    }
                    else
                    {
                        moveList.Add(new Move(field, moveField));
                    }
                }

                if (currentRank == startRank)
                {
                    int doubleMove = field + 2 * pawnDirection;
                    ulong doubleMoveBitboard = 1UL << doubleMove;

                    if ((doubleMoveBitboard & (pieces.allPieces | enemyPieces.allPieces)) == 0 && (doubleMoveBitboard & possibleFields) != 0)
                    {
                        moveList.Add(new Move(field, doubleMove, Move.Flag.PawnTwoForward));
                    }
                }


            }

            int capturedPawnEnPassantField = enPassantField - pawnDirection;
            ulong capturedPwnEnPassantBitboard = 1UL << capturedPawnEnPassantField;

            if (field % 8 != 0)
            {
                int captureField = field + pawnDirection + W;
                ulong captureFieldBitboard = 1UL << captureField;
                if ((captureFieldBitboard & enemyPieces.allPieces) != 0 && (captureFieldBitboard & possibleFields) != 0 && (fieldBitboard & canPawnCaptureWest) != 0)
                {
                    if (currentRank == rankBeforePromotion)
                    {
                        moveList.Add(new Move(field, captureField, Move.Flag.PromoteToBishop));
                        moveList.Add(new Move(field, captureField, Move.Flag.PromoteToQueen));
                        moveList.Add(new Move(field, captureField, Move.Flag.PromoteToRook));
                        moveList.Add(new Move(field, captureField, Move.Flag.PromoteToKnight));
                    }
                    else
                    {
                        moveList.Add(new Move(field, captureField));
                    }
                }
                else if (captureField == enPassantField && (fieldBitboard & canPawnCaptureWest) != 0 && ((captureFieldBitboard | capturedPwnEnPassantBitboard) & possibleFields) != 0)
                {
                    if (EnPassantTest(field, enPassantField - pawnDirection))
                    {
                        moveList.Add(new Move(field, captureField, Move.Flag.EnPassantCapture));
                    }
                }
            }

            if (field % 8 != 7)
            {
                int captureField = field + pawnDirection + E;
                ulong captureFieldBitboard = 1UL << captureField;
                if ((captureFieldBitboard & enemyPieces.allPieces) != 0 && (captureFieldBitboard & possibleFields) != 0 && (fieldBitboard & canPawnCaptureEast) != 0)
                {
                    if (currentRank == rankBeforePromotion)
                    {
                        moveList.Add(new Move(field, captureField, Move.Flag.PromoteToBishop));
                        moveList.Add(new Move(field, captureField, Move.Flag.PromoteToQueen));
                        moveList.Add(new Move(field, captureField, Move.Flag.PromoteToRook));
                        moveList.Add(new Move(field, captureField, Move.Flag.PromoteToKnight));
                    }
                    else
                    {
                        moveList.Add(new Move(field, captureField));
                    }
                }
                else if (enPassantFile != -1 && captureField == enPassantField && (fieldBitboard & canPawnCaptureEast) != 0 && ((captureFieldBitboard | capturedPwnEnPassantBitboard) & possibleFields) != 0)
                {
                    if (EnPassantTest(field, enPassantField - pawnDirection))
                    {
                        moveList.Add(new Move(field, captureField, Move.Flag.EnPassantCapture));
                    }
                }
            }
        }


        private static void GenerateKnightMoves(int field, List<Move> moveList)
        {
            ulong fieldBit = 1UL << field;
            if ((fieldBit & canMoveInNESW & canMoveInNWSE & canMoveInNS & canMoveInWE) == 0)
                return;


            ulong knightMoves = PreComputations.KnightMoves[field] & possibleFields & ~pieces.allPieces;

            ulong bitPointer = 1UL;

            for (int i = 0; i < 64; i++)
            {
                if ((knightMoves & bitPointer) != 0)
                {
                    moveList.Add(new Move(field, i));
                }
                bitPointer <<= 1;
            }
        }

        private static void GenerateOrthogonalSliderMoves(int field, List<Move> movelist)
        {
            ulong fieldBit = 1UL << field;

            int i;
            ulong iBit;

            if ((fieldBit & canMoveInNS) != 0)
            {
                //N
                i = field + 8;
                iBit = fieldBit << 8;
                iBit &= allyMask;
                while (iBit != 0)
                {
                    if ((iBit & possibleFields) != 0)
                        movelist.Add(new Move(field, i));
                    iBit &= enemyMask;
                    i += 8;
                    iBit <<= 8;
                    iBit &= allyMask;
                }

                //S
                i = field - 8;
                iBit = fieldBit >> 8;
                iBit &= allyMask;
                while (iBit != 0)
                {
                    if ((iBit & possibleFields) != 0)
                        movelist.Add(new Move(field, i));
                    iBit &= enemyMask;
                    i -= 8;
                    iBit >>= 8;
                    iBit &= allyMask;
                }
            }

            if ((fieldBit & canMoveInWE) != 0)
            {
                //W
                i = field - 1;
                iBit = (fieldBit & AFileMask) >> 1;
                ulong wMask = enemyMask & AFileMask;
                iBit &= allyMask;
                while (iBit != 0)
                {
                    if ((iBit & possibleFields) != 0)
                        movelist.Add(new Move(field, i));
                    iBit &= wMask;
                    i -= 1;
                    iBit >>= 1;
                    iBit &= allyMask;
                }

                //E
                i = field + 1;
                iBit = (fieldBit & HFileMask) << 1;
                ulong eMask = enemyMask & HFileMask;
                iBit &= allyMask;
                while (iBit != 0)
                {
                    if ((iBit & possibleFields) != 0)
                        movelist.Add(new Move(field, i));
                    iBit &= eMask;
                    i += 1;
                    iBit <<= 1;
                    iBit &= allyMask;
                }
            }
        }


        public static void GenerateDiagonalSliderMoves(int field, List<Move> moveList)
        {
            ulong fieldBit = 1UL << field;
            int i;
            ulong iBit;
            ulong eMask = enemyMask & HFileMask;
            ulong wMask = enemyMask & AFileMask;

            if ((fieldBit & canMoveInNESW) != 0)
            {
                //NE
                i = field + 9;
                iBit = (fieldBit & HFileMask) << 9;
                iBit &= allyMask;
                while (iBit != 0)
                {
                    if ((iBit & possibleFields) != 0)
                        moveList.Add(new Move(field, i));
                    iBit &= eMask;
                    i += 9;
                    iBit <<= 9;
                    iBit &= allyMask;
                }

                //SW
                i = field - 9;
                iBit = (fieldBit & AFileMask) >> 9;
                iBit &= allyMask;
                while (iBit != 0)
                {
                    if ((iBit & possibleFields) != 0)
                        moveList.Add(new Move(field, i));
                    iBit &= wMask;
                    i -= 9;
                    iBit >>= 9;
                    iBit &= allyMask;
                }
            }

            if ((fieldBit & canMoveInNWSE) != 0)
            {
                //NW
                i = field + 7;
                iBit = (fieldBit & AFileMask) << 7;
                iBit &= allyMask;
                while (iBit != 0)
                {
                    if ((iBit & possibleFields) != 0)
                        moveList.Add(new Move(field, i));
                    iBit &= wMask;
                    i += 7;
                    iBit <<= 7;
                    iBit &= allyMask;
                }

                //SE
                i = field - 7;
                iBit = (fieldBit & HFileMask) >> 7;
                iBit &= allyMask;
                while (iBit != 0)
                {
                    if ((iBit & possibleFields) != 0)
                        moveList.Add(new Move(field, i));
                    iBit &= eMask;
                    i -= 7;
                    iBit >>= 7;
                    iBit &= allyMask;
                }
            }
        }


        private static void GenerateKingMoves(int field, List<Move> moveList)
        {
            ulong moveMap = PreComputations.KingMoves[field] & ~(pieces.allPieces | enemyAttackMap);
            ulong bitPointer = 1UL;
            for (int i = 0; i < 64; i++)
            {
                if ((moveMap & bitPointer) != 0)
                {
                    moveList.Add(new Move(field, i));
                }
                bitPointer <<= 1;
            }

            if (Board.toMove == Piece.WHITE)
            {
                if ((Board.currentGameState & Board.whiteKingsideCastleMask) != 0 && (enemyAttackMap & (whiteKingsideCastleFields | pieces.kingPosition)) == 0 && ((pieces.allPieces | enemyPieces.allPieces) & whiteKingsideCastleFields) == 0)
                {
                    Move move = new Move(field, field + 2 * E, Move.Flag.Castling);
                    moveList.Add(move);
                }
                if ((Board.currentGameState & Board.whiteQueensideCastleMask) != 0 && (enemyAttackMap & (whiteQueensideCastleFields | pieces.kingPosition)) == 0 && ((pieces.allPieces | enemyPieces.allPieces) & (whiteQueensideCastleFields + 2)) == 0)
                {
                    Move move = new Move(field, field + 2 * W, Move.Flag.Castling);
                    moveList.Add(move);
                }
            }
            else
            {
                if ((Board.currentGameState & Board.blackKingsideCastleMask) != 0 && (enemyAttackMap & (blackKingsideCastleFields | pieces.kingPosition)) == 0 && ((pieces.allPieces | enemyPieces.allPieces) & blackKingsideCastleFields) == 0)
                {
                    Move move = new Move(field, field + 2 * E, Move.Flag.Castling);
                    moveList.Add(move);
                }
                if ((Board.currentGameState & Board.blackQueensideCastleMask) != 0 && (enemyAttackMap & (blackQueensideCastleFields | pieces.kingPosition)) == 0 && ((pieces.allPieces | enemyPieces.allPieces) & (blackQueensideCastleFields + (2UL << 56))) == 0)
                {
                    Move move = new Move(field, field + 2 * W, Move.Flag.Castling);
                    moveList.Add(move);
                }
            }
        }

        private static void FindPins(ulong kingPosition)
        {
            canMoveInWE = pieces.allPieces;
            canMoveInNESW = pieces.allPieces;
            canMoveInNS = pieces.allPieces;
            canMoveInNWSE = pieces.allPieces;

            ulong testBit = 1UL;
            for (int i = 0; i < 64; i++)
            {
                if ((testBit & kingPosition) != 0)
                {
                    kingField = i;
                    break;
                }
                testBit <<= 1;
            }

            //these are saved for later usage if the king happends to be in check
            kingN = AttackMapper.MapNSliderAttacks(kingPosition);
            kingS = AttackMapper.MapSSliderAttacks(kingPosition);
            kingE = AttackMapper.MapESliderAttacks(kingPosition);
            kingW = AttackMapper.MapWSliderAttacks(kingPosition);
            kingNW = AttackMapper.MapNWSliderAttacks(kingPosition);
            kingNE = AttackMapper.MapNESliderAttacks(kingPosition);
            kingSW = AttackMapper.MapSWSliderAttacks(kingPosition);
            kingSE = AttackMapper.MapSESliderAttacks(kingPosition);


            ulong kingNS = 0;
            ulong kingWE = 0;
            ulong kingNWSE = 0;
            ulong kingNESW = 0;

            if (enemyPieces.orthogonalSliders != 0)
            {
                kingNS = kingS | kingN;
                kingWE = kingW | kingE;
            }
            if (enemyPieces.diagonalSliders != 0)
            {
                kingNWSE = kingNW | kingSE;
                kingNESW = kingNE | kingSW;
            }

            ulong blockedNESW = kingNESW & AttackMapper.NESWSliderAttacks;
            ulong blockedNS = kingNS & AttackMapper.NSSliderAttacks;

            ulong blockedWE = kingWE & AttackMapper.EWSliderAttacks;
            ulong blockedNWSE = kingNWSE & AttackMapper.NWSESliderAttacks;

            canMoveInNESW ^= blockedNS | blockedWE | blockedNWSE;
            canMoveInNS ^= blockedWE | blockedNWSE | blockedNESW;
            canMoveInWE ^= blockedNS | blockedNWSE | blockedNESW;
            canMoveInNWSE ^= blockedNESW | blockedNS | blockedWE;

            canPawnCaptureEast = Board.toMove == Piece.WHITE ? canMoveInNESW : canMoveInNWSE;
            canPawnCaptureWest = Board.toMove == Piece.WHITE ? canMoveInNWSE : canMoveInNESW;
        }

        private static ulong FindPawnsThatCaptureOnField(int field, ulong pawns, uint color)
        {
            ulong result = 0;

            ulong fieldBitBoard = 1UL << field;

            int direction = color == Piece.WHITE ? S : N;
            int eastField = field + direction + E;
            int westField = field + direction + W;
            if ((fieldBitBoard & HFile) == 0)
                result |= 1UL << eastField;
            if ((fieldBitBoard & AFile) == 0)
                result |= 1UL << westField;
            return result & pawns;
        }

        private static ulong FindKnightsThatMoveToField(int field, ulong knights)
        {
            return PreComputations.KnightMoves[field] & knights;
        }

        private static ulong FindOrthogonalsThatMoveToField(int field, ulong orthogonals)
        {
            ulong result = 0;

            ulong fieldBit = 1UL << field;
            result |= AttackMapper.MapNSliderAttacks(fieldBit);
            result |= AttackMapper.MapSSliderAttacks(fieldBit);
            result |= AttackMapper.MapESliderAttacks(fieldBit);
            result |= AttackMapper.MapWSliderAttacks(fieldBit);

            return result & orthogonals;
        }

        private static ulong FindDiagonallsThatMoveToField(int field, ulong diagonalls)
        {
            ulong result = 0;

            ulong fieldBit = 1UL << field;
            result |= AttackMapper.MapNESliderAttacks(fieldBit);
            result |= AttackMapper.MapNWSliderAttacks(fieldBit);
            result |= AttackMapper.MapSESliderAttacks(fieldBit);
            result |= AttackMapper.MapSWSliderAttacks(fieldBit);
            return result & diagonalls;
        }

        public static bool EnPassantTest(int field, int enPassantField)
        {
            int leftField = enPassantField < field ? enPassantField - 1 : field - 1;
            int rightField = enPassantField > field ? enPassantField + 1 : field + 1;

            ulong leftSideBit = 1UL << leftField;
            ulong rightSideBit = 1UL << rightField;

            ulong allPieces = enemyPieces.allPieces | pieces.allPieces;

            while (true)
            {
                if (leftField % 8 == 7)
                {
                    return true;
                }
                if ((leftSideBit & allPieces) != 0)
                {
                    break;
                }
                leftField--;
                leftSideBit >>= 1;
            }
            while (true)
            {
                if (rightField % 8 == 0)
                {
                    return true;
                }
                if ((rightSideBit & allPieces) != 0)
                {
                    break;
                }
                rightField++;
                rightSideBit <<= 1;
            }

            return ((leftSideBit & pieces.kingPosition) == 0 || (rightSideBit & enemyPieces.orthogonalSliders) == 0) && ((rightSideBit & pieces.kingPosition) == 0 || (leftSideBit & enemyPieces.orthogonalSliders) == 0);
        }
    }
}
