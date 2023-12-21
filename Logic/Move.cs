namespace Chess.Logic
{
    //this is heavily based off of Sebastian LAgue's implementation https://github.com/SebLague/Chess-Coding-Adventure/tree/Chess-V1-Unity

    //move is storad as a ushort
    public class Move
    {
        public readonly ushort value = 0;

        public readonly struct Flag
        {
            public const int None = 0;
            public const int EnPassantCapture = 1;
            public const int Castling = 2;
            public const int PawnTwoForward = 3;
            public const int PromoteToQueen = 4;
            public const int PromoteToKnight = 5;
            public const int PromoteToRook = 6;
            public const int PromoteToBishop = 7;
        }

        const ushort startSquareMask = 0b0000000000111111;
        const ushort targetSquareMask = 0b0000111111000000;
        const ushort flagMask = 0b1111000000000000;



        public Move(int startSquare, int targetSquare)
        {
            value |= (ushort)(startSquare | targetSquare << 6);
        }
        public Move(int startSquare, int targetSquare, int flag)
        {
            value |= (ushort)(startSquare | targetSquare << 6 | flag << 12);
        }

        public int StartSquare
        {
            get
            {
                return value & startSquareMask;
            }
        }

        public int TargetSquare
        {
            get
            {
                return (value & targetSquareMask) >> 6;
            }
        }

        public int MoveFlag
        {
            get
            {
                return (value & flagMask) >> 12;
            }
        }

        public override string ToString()
        {
            int startRow = StartSquare / 8 + 1;
            int startCol = StartSquare % 8;

            int targetRow = TargetSquare / 8 + 1;
            int targetCol = TargetSquare % 8;

            char startFile = (char)('a' + startCol);
            char targetFile = (char)('a' + targetCol);
            return startFile.ToString() + startRow.ToString() + targetFile.ToString() + targetRow.ToString();
        }
    }
}
