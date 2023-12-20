using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess.Logic
{
    public static class Board
    {
        public static readonly string STARTING_POSITION = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public static readonly int N = 8, S = -8, E = 1, W = -1;

        public static readonly uint whiteKingsideCastleMask = 1;
        public static readonly uint whiteQueensideCastleMask = 0b10;
        public static readonly uint blackKingsideCastleMask = 0b100;
        public static readonly uint blackQueensideCastleMask = 0b1000;
        public static readonly uint enPassantMask = 0b11110000;
        public static readonly uint capturedPieceMask = 0b11111100000000;
        public static readonly uint moveCounterMask = ~0b11111111111111u;

        public static uint[] board =
            {
               Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE,
               Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE,
               Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE,
               Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE,
               Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE,
               Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE,
               Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE,
               Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE
            };

        public static uint toMove;
        public static uint toStay;

        //bit 0 white kingside castle
        //bit 1 white queenside castle
        //bit 2 black kingside castle
        //bit 3 black queenside castle

        //4-7 store enpassant file from 1 to 8 so 0 is none
        //bits 8-13 captured piece
        //bits 14+ 50 move counter

        public static Stack<uint> gameStateHistory = new Stack<uint>();
        public static uint currentGameState = 0;

        //these sets contain all positions of white/black pieces
        public static PieceList whitePieces;
        public static PieceList blackPieces;



        //this holds a single bit in a 64 bit number indicating king's position
        public static ulong whiteKingPosition;
        public static ulong blackKingPosition;

        public static ulong hash;
        public static Stack<ulong> hashStack = new Stack<ulong>();

        public static Stack<Move> moveHistory = new Stack<Move>();

        public static void ReadFEN(string fenString)
        {

            board = new uint[]
            {
                Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE,
               Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE,
               Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE,
               Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE,
               Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE,
               Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE,
               Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE,
               Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE, Piece.NONE
            };
            currentGameState = 0;
            whitePieces = null;
            blackPieces = null;
            gameStateHistory = new Stack<uint>();
            moveHistory = new Stack<Move>();

            char[] delimeters = { ' ' };
            string[] elements = fenString.Split(delimeters, StringSplitOptions.RemoveEmptyEntries);

            delimeters[0] = '/';
            string[] rows = elements[0].Split(delimeters, StringSplitOptions.RemoveEmptyEntries);
            SetPosition(rows);
            ValidateSides(elements[1]);
            ValidateCastles(elements[2]);
            SetEnPassant(elements[3]);


            whitePieces = new PieceList(board, Piece.WHITE);
            blackPieces = new PieceList(board, Piece.BLACK);

            hash = Zobrist.HashCurrentPosition();
        }

        public static void MakeMove(Move move)
        {
            hashStack.Push(hash);

            if ((currentGameState & enPassantMask) != 0)
            {
                int prevEnPassantFile = (int)((currentGameState & enPassantMask) >> 4) - 1;
                hash ^= Zobrist.enPassantFiles[prevEnPassantFile];
            }

            uint newGameState = 0;
            newGameState |= currentGameState & (whiteKingsideCastleMask | whiteQueensideCastleMask | blackKingsideCastleMask | blackQueensideCastleMask);
            uint color = toMove;
            PieceList pieceList;
            PieceList enemyPieceList;

            Keys toMoveKeys;
            Keys toStayKeys;
            hash ^= Zobrist.blackToMove;
            if (color == Piece.WHITE)
            {
                toMoveKeys = Zobrist.whiteKeys;
                toStayKeys = Zobrist.blackKeys;
                pieceList = whitePieces;
                enemyPieceList = blackPieces;
            }
            else
            {
                toMoveKeys = Zobrist.blackKeys;
                toStayKeys = Zobrist.whiteKeys;
                pieceList = blackPieces;
                enemyPieceList = whitePieces;
            }

            int start = move.StartSquare;
            int target = move.TargetSquare;

            uint piece = board[move.StartSquare];
            uint pieceType = Piece.GetPiece(piece);

            bool captureWasMade = false;

            int flag = move.MoveFlag;

            if (start == 0 || target == 0)
            {
                newGameState &= ~whiteQueensideCastleMask;
                hash = Zobrist.HashOutWhiteQueensideCastleMask(hash);
            }
            if (start == 7 || target == 7)
            {
                newGameState &= ~whiteKingsideCastleMask;
                hash = Zobrist.HashOutWhiteKingsideCastleMask(hash);
            }
            if (start == 56 || target == 56)
            {
                newGameState &= ~blackQueensideCastleMask;
                hash = Zobrist.HashOutBlackQueensideCastleMask(hash);
            }
            if (start == 63 || target == 63)
            {
                newGameState &= ~blackKingsideCastleMask;
                hash = Zobrist.HashOutBlackKingsideCastleMask(hash);
            }

            hash ^= Zobrist.GetPieceNum(piece, start, toMoveKeys);
            hash ^= Zobrist.GetPieceNum(piece, target, toMoveKeys);
            switch (flag)
            {
                case Move.Flag.None:
                    pieceList.MovePieceToField(start, target, pieceType);
                    if (board[target] != Piece.NONE)
                    {
                        uint enemyPiece = board[target];
                        enemyPieceList.RemovePieceAtField(target, Piece.GetPiece(enemyPiece));
                        newGameState |= enemyPiece << 8;
                        captureWasMade = true;

                        hash ^= Zobrist.GetPieceNum(enemyPiece, target, toStayKeys);
                    }
                    if (pieceType == Piece.KING)
                    {
                        if (color == Piece.WHITE)
                        {
                            newGameState &= ~whiteKingsideCastleMask;
                            newGameState &= ~whiteQueensideCastleMask;
                            hash = Zobrist.HashOutWhiteKingsideCastleMask(hash);
                            hash = Zobrist.HashOutWhiteQueensideCastleMask(hash);
                        }
                        else
                        {
                            newGameState &= ~blackKingsideCastleMask;
                            newGameState &= ~blackQueensideCastleMask;
                            hash = Zobrist.HashOutBlackKingsideCastleMask(hash);
                            hash = Zobrist.HashOutBlackQueensideCastleMask(hash);
                        }
                    }
                    board[start] = Piece.NONE;
                    board[target] = piece;
                    break;
                case Move.Flag.PawnTwoForward:
                    pieceList.MovePieceToField(start, target, pieceType);
                    uint enPassantFile = (uint)target % 8;
                    newGameState |= (enPassantFile + 1) << 4;
                    hash ^= Zobrist.enPassantFiles[enPassantFile];
                    board[start] = Piece.NONE;
                    board[target] = piece;
                    break;
                case Move.Flag.EnPassantCapture:
                    int rankDelta = color == Piece.WHITE ? -8 : 8;
                    int capturedPawnField = target + rankDelta;
                    enemyPieceList.RemovePieceAtField(capturedPawnField, Piece.PAWN);
                    pieceList.MovePieceToField(start, target, pieceType);
                    newGameState |= (Piece.PAWN + color) << 8;
                    board[start] = Piece.NONE;
                    board[target] = piece;
                    board[capturedPawnField] = Piece.NONE;
                    hash ^= Zobrist.GetPieceNum(Piece.PAWN, target, toStayKeys);
                    break;
                case Move.Flag.Castling:
                    int rookStart;
                    int rookTarget;
                    if (color == Piece.WHITE)
                    {
                        newGameState &= ~whiteKingsideCastleMask;
                        hash = Zobrist.HashOutWhiteKingsideCastleMask(hash);
                        newGameState &= ~whiteQueensideCastleMask;
                        hash = Zobrist.HashOutWhiteQueensideCastleMask(hash);
                    }
                    else
                    {
                        newGameState &= ~blackKingsideCastleMask;
                        hash = Zobrist.HashOutBlackKingsideCastleMask(hash);
                        newGameState &= ~blackQueensideCastleMask;
                        hash = Zobrist.HashOutBlackQueensideCastleMask(hash);
                    }
                    if (target > start)
                    {
                        rookStart = start + 3;
                        rookTarget = start + 1;
                    }
                    else
                    {
                        rookStart = start - 4;
                        rookTarget = start - 1;
                    }
                    hash ^= toMoveKeys.rooks[rookStart];
                    hash ^= toMoveKeys.rooks[rookTarget];
                    pieceList.MovePieceToField(start, target, pieceType);
                    pieceList.MovePieceToField(rookStart, rookTarget, Piece.ROOK);
                    uint rook = board[rookStart];
                    board[start] = Piece.NONE;
                    board[target] = piece;
                    board[rookStart] = Piece.NONE;
                    board[rookTarget] = rook;
                    break;
                case Move.Flag.PromoteToBishop:
                    if (board[target] != Piece.NONE)
                    {
                        uint enemyPiece = board[target];
                        enemyPieceList.RemovePieceAtField(target, Piece.GetPiece(enemyPiece));
                        newGameState |= enemyPiece << 8;
                    }
                    pieceList.RemovePieceAtField(start, Piece.PAWN);
                    hash ^= toMoveKeys.pawns[target];
                    hash ^= toMoveKeys.bishops[target];
                    pieceList.AddPieceAtField(target, Piece.BISHOP);
                    board[start] = Piece.NONE;
                    board[target] = Piece.BISHOP + color;
                    break;
                case Move.Flag.PromoteToRook:
                    if (board[target] != Piece.NONE)
                    {
                        uint enemyPiece = board[target];
                        enemyPieceList.RemovePieceAtField(target, Piece.GetPiece(enemyPiece));
                        newGameState |= enemyPiece << 8;
                    }
                    pieceList.RemovePieceAtField(start, Piece.PAWN);
                    pieceList.AddPieceAtField(target, Piece.ROOK);

                    hash ^= toMoveKeys.pawns[target];
                    hash ^= toMoveKeys.rooks[target];

                    board[start] = Piece.NONE;
                    board[target] = Piece.ROOK + color;
                    break;
                case Move.Flag.PromoteToQueen:
                    if (board[target] != Piece.NONE)
                    {
                        uint enemyPiece = board[target];
                        enemyPieceList.RemovePieceAtField(target, Piece.GetPiece(enemyPiece));
                        newGameState |= enemyPiece << 8;
                    }
                    pieceList.RemovePieceAtField(start, Piece.PAWN);
                    pieceList.AddPieceAtField(target, Piece.QUEEN);

                    hash ^= toMoveKeys.pawns[target];
                    hash ^= toMoveKeys.queens[target];

                    board[start] = Piece.NONE;
                    board[target] = Piece.QUEEN + color;
                    break;
                case Move.Flag.PromoteToKnight:
                    if (board[target] != Piece.NONE)
                    {
                        uint enemyPiece = board[target];
                        enemyPieceList.RemovePieceAtField(target, Piece.GetPiece(enemyPiece));
                        newGameState |= enemyPiece << 8;
                    }
                    pieceList.RemovePieceAtField(start, Piece.PAWN);
                    pieceList.AddPieceAtField(target, Piece.KNIGHT);

                    hash ^= toMoveKeys.pawns[target];
                    hash ^= toMoveKeys.knights[target];

                    board[start] = Piece.NONE;
                    board[target] = Piece.KNIGHT + color;
                    break;
            }

            uint moveCounter = currentGameState >> 14;
            if (pieceType == Piece.PAWN || captureWasMade)
                moveCounter = 0;
            else
                moveCounter++;

            newGameState |= moveCounter << 14;

            gameStateHistory.Push(currentGameState);
            moveHistory.Push(move);
            currentGameState = newGameState;
            ChangeSides();
        }

        public static void UnMakeMove()
        {
            Move move = moveHistory.Pop();
            uint color = toStay;
            int target = move.TargetSquare;
            int start = move.StartSquare;
            uint piece = board[target];
            uint pieceType = Piece.GetPiece(piece);

            PieceList pieces;
            PieceList enemyPieces;

            uint capturedPiece = (currentGameState & capturedPieceMask) >> 8;
            uint capturedPieceType = Piece.GetPiece(capturedPiece);
            if (color == Piece.WHITE)
            {
                pieces = whitePieces;
                enemyPieces = blackPieces;
            }
            else
            {
                pieces = blackPieces;
                enemyPieces = whitePieces;
            }
            int moveFlag = move.MoveFlag;

            if (moveFlag == Move.Flag.None || moveFlag == Move.Flag.PawnTwoForward)
            {
                if (capturedPiece != Piece.NONE)
                {
                    enemyPieces.AddPieceAtField(target, capturedPieceType);
                }
                board[target] = capturedPiece;
                pieces.MovePieceToField(target, start, pieceType);
                board[start] = piece;
            }
            else if (moveFlag == Move.Flag.Castling)
            {
                int startRookSquare;
                int targetRookSquare;
                if (target > start)
                {
                    startRookSquare = target + 1;
                    targetRookSquare = start + 1;
                }
                else
                {
                    startRookSquare = target - 2;
                    targetRookSquare = start - 1;
                }
                pieces.MovePieceToField(target, start, pieceType);
                pieces.MovePieceToField(targetRookSquare, startRookSquare, Piece.ROOK);
                uint rook = board[targetRookSquare];
                board[target] = Piece.NONE;
                board[start] = piece;
                board[targetRookSquare] = Piece.NONE;
                board[startRookSquare] = rook;
            }
            else if (moveFlag == Move.Flag.EnPassantCapture)
            {
                int rankDelta = color == Piece.WHITE ? -8 : 8;
                int capturedPawnSquare = target + rankDelta;
                uint capturedPawn = toMove + Piece.PAWN;
                board[capturedPawnSquare] = capturedPawn;
                board[target] = Piece.NONE;
                board[start] = piece;
                pieces.MovePieceToField(target, start, pieceType);
                enemyPieces.AddPieceAtField(capturedPawnSquare, Piece.PAWN);
            }
            else //promotions
            {
                if (capturedPiece != Piece.NONE)
                {
                    enemyPieces.AddPieceAtField(target, capturedPieceType);
                }
                uint promotedPiece = board[target];
                pieces.RemovePieceAtField(target, Piece.GetPiece(promotedPiece));
                pieces.AddPieceAtField(start, Piece.PAWN);
                board[target] = capturedPiece;
                board[start] = Piece.PAWN + color;
            }

            currentGameState = gameStateHistory.Pop();
            hash = hashStack.Pop();
            ChangeSides();
        }

        public static void ChangeSides()
        {
            uint wasMoving = toMove;
            toMove = toStay;
            toStay = wasMoving;
        }



        private static void SetPosition(string[] rows)
        {
            int i = 56;

            foreach (string row in rows)
            {
                foreach (char c in row)
                {
                    if (c >= '0' && c <= '8')
                    {
                        i += c - '0';
                    }
                    else
                    {
                        uint piece = CharToPiece(c);
                        board[i] = piece;
                        i++;
                    }
                }

                i -= 16;
            }
        }

        private static void ValidateSides(string sideString)
        {
            if (sideString == "w")
            {
                toMove = Piece.WHITE;
                toStay = Piece.BLACK;
            }
            else if (sideString == "b")
            {
                toMove = Piece.BLACK;
                toStay = Piece.WHITE;
            }
            else
            {
                throw new ArgumentException("INVALID FEN LITERAL");
            }
        }

        private static void ValidateCastles(string castleString)
        {

            if (castleString.Contains('K'))
            {
                currentGameState |= whiteKingsideCastleMask;
            }
            if (castleString.Contains('Q'))
            {
                currentGameState |= whiteQueensideCastleMask;
            }
            if (castleString.Contains('k'))
            {
                currentGameState |= blackKingsideCastleMask;
            }
            if (castleString.Contains('q'))
            {
                currentGameState |= blackQueensideCastleMask;
            }
        }

        private static void SetEnPassant(string enPassantString)
        {
            if (enPassantString != "-")
            {
                uint enPassantField = FieldStringToNumber(enPassantString);
                uint enPassantFile = enPassantField % 8 + 1;
                currentGameState |= (enPassantFile << 4);
            }
        }

        private static uint CharToPiece(char piece)
        {
            uint color = char.IsUpper(piece) ? Piece.WHITE : Piece.BLACK;

            piece = char.ToLower(piece);
            uint pieceValue;
            switch (piece)
            {
                case 'r':
                    pieceValue = Piece.ROOK;
                    break;
                case 'b':
                    pieceValue = Piece.BISHOP;
                    break;
                case 'n':
                    pieceValue = Piece.KNIGHT;
                    break;
                case 'p':
                    pieceValue = Piece.PAWN;
                    break;
                case 'k':
                    pieceValue = Piece.KING;
                    break;
                case 'q':
                    pieceValue = Piece.QUEEN;
                    break;
                default:
                    throw new ArgumentException("INVALID FEN LITERAL");
            }

            return pieceValue + color;
        }

        private static uint FieldStringToNumber(string fieldString)
        {
            char colChar = fieldString[0];
            uint col = (uint)(colChar - 'a');
            uint row = (uint)(fieldString[1] - '1');

            return row * 8 + col;
        }
        public static string getFENstring()
        {
            string fenString = "";
            for (int row = 7; row >= 0; row--)
            {
                int emptyCount = 0;
                for(int col = 0; col < 8; col++)
                {
                    int field = row * 8 + col;
                    if (board[field] != Piece.NONE)
                    {
                        if (emptyCount != 0)
                        {
                            fenString += emptyCount.ToString();
                            emptyCount = 0;
                        }
                        fenString += getPieceString(board[field]);
                    }
                    else
                    {
                        emptyCount++;
                    }
                }
                if (emptyCount != 0)
                {
                    fenString += emptyCount.ToString();
                }
                else if (fenString[fenString.Length - 1] == '/')
                {
                    fenString += "8";
                }
                if (row != 0)
                    fenString += "/";
            }

            fenString += " ";
            if (toMove == Piece.WHITE)
                fenString += "w";
            else
                fenString += "b";

            fenString += " ";

            bool castleAvailable = false;
            if ((whiteKingsideCastleMask & currentGameState) != 0)
            {
                fenString += "K";
                castleAvailable = true;
            }
            if ((whiteQueensideCastleMask & currentGameState) != 0)
            {
                fenString += "Q";
                castleAvailable = true;
            }
            if ((blackKingsideCastleMask & currentGameState) != 0)
            {
                fenString += "k";
                castleAvailable = true;
            }
            if ((blackQueensideCastleMask & currentGameState) != 0)
            {
                fenString += "q";
                castleAvailable = true;
            }
            if(!castleAvailable)
            {
                fenString += "-";
            }

            fenString += " ";

            if ((currentGameState & enPassantMask) != 0)
            {
                int enPassantFile = (int)((currentGameState & enPassantMask) >> 4) - 1;
                fenString += (char)('a' + enPassantFile);
                if(toMove == Piece.WHITE)
                {
                    fenString += "6";
                }
                else
                {
                    fenString += "3";
                }
            }
            else
            {
                fenString += "-";
            }

            return fenString;
        }

        private static string getPieceString(uint piece)
        {
            string pieceString = "";
            uint pieceType = Piece.GetPiece(piece);
            switch(pieceType)
            {
                case Piece.PAWN:
                    pieceString = "p";
                    break;
                case Piece.KNIGHT:
                    pieceString = "n";
                    break;
                case Piece.KING:
                    pieceString = "k";
                    break;
                case Piece.QUEEN:
                    pieceString = "q";
                    break;
                case Piece.BISHOP:
                    pieceString = "b";
                    break;
                case Piece.ROOK:
                    pieceString = "r";
                    break;
            }
            uint color = Piece.GetColor(piece);
            if (color == Piece.WHITE)
            {
                pieceString = pieceString.ToUpper();
            }
            return pieceString;
        }
    }
}
