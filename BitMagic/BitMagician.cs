using System;

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

        public static void PrintBitBoard(ulong bitBoard)
        {
            for (int i = 7; i >= 0; i--)
            {
                for (int j = i * 8; j < (i * 8) + 8; j++)
                {
                    if ((bitBoard & (1UL << j)) != 0)
                    {
                        Console.Write("1");
                    }
                    else
                    {
                        Console.Write("0");
                    }
                }

                Console.Write("\n");
            }
        }
    }
}
