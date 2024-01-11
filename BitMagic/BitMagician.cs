namespace Chess.BitMagic
{
    public static class BitMagician
    {
        public static int CountBits(ulong bitBoard)
        {
            int count = 0;
            while (bitBoard > 0)
            {
                bitBoard &= (bitBoard - 1);
                count++;
            }
            return count;
        }

        public static int GetBitIndex(ulong bitBoard)
        {
            ulong pointer = 1UL;
            for (int i = 0; i < 64; i++)
            {
                if ((bitBoard & pointer) != 0)
                {
                    return i;
                }
                pointer <<= 1;
            }

            return -1;
        }
    }
}
