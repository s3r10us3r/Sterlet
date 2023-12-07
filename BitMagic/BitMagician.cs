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
    }
}
