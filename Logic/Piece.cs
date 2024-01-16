namespace Chess.Logic
{
    public static class Piece
    {
        public const uint WALL = 7;
        public const uint NONE = 0;
        public const uint PAWN = 1;
        public const uint BISHOP = 2;
        public const uint KNIGHT = 3;
        public const uint ROOK = 4;
        public const uint QUEEN = 5;
        public const uint KING = 6;

        public const uint WHITE = 16;
        public const uint BLACK = 24;


        public static uint GetColor(uint piece)
        {
            return piece & 24u;
        }

        public static uint GetPiece(uint piece)
        {
            return piece & 7u;
        }
    }
}
