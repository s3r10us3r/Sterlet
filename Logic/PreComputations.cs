namespace Chess.Logic
{
    static class PreComputations
    {
        public static ulong[] KnightMoves;
        public static ulong[] SliderMoves;
        public static ulong[] KingMoves;
        public static readonly int N = 8, S = -8, E = 1, W = -1;

        static PreComputations()
        {
            GenerateKnightMoves();
            GenerateSliderMoves();
            GenerateKingMoves();
        }

        private static void GenerateKnightMoves()
        {
            KnightMoves = new ulong[64];
            for (int i = 0; i < 64; i++)
            {
                ulong allMoves = 0;

                if (i / 8 < 6)
                {
                    if (i % 8 < 7)
                    {
                        allMoves |= 1UL << (i + N + N + E);
                    }
                    if (i % 8 > 0)
                    {
                        allMoves |= 1UL << (i + N + N + W);
                    }
                }

                if (i / 8 > 1)
                {
                    if (i % 8 < 7)
                    {
                        allMoves |= 1UL << (i + S + S + E);
                    }
                    if (i % 8 > 0)
                    {
                        allMoves |= 1UL << (i + S + S + W);
                    }
                }
                if (i % 8 < 6)
                {
                    if (i / 8 < 7)
                    {
                        allMoves |= 1UL << (i + E + E + N);
                    }
                    if (i / 8 > 0)
                    {
                        allMoves |= 1UL << (i + E + E + S);
                    }
                }
                if (i % 8 > 1)
                {
                    if (i / 8 < 7)
                    {
                        allMoves |= 1UL << (i + W + W + N);
                    }
                    if (i / 8 > 0)
                    {
                        allMoves |= 1UL << (i + W + W + S);
                    }
                }

                KnightMoves[i] = allMoves;
            }
        }

        private static void GenerateSliderMoves()
        {
            SliderMoves = new ulong[64];
            for (int i = 0; i < 64; i++)
            {
                ulong allMoves = 0;


                int j = i + N;
                while (j < 64)
                {
                    allMoves |= 1UL << j;
                    j += N;
                }

                j = i + S;
                while (j >= 0)
                {
                    allMoves |= 1UL << j;
                    j += S;
                }

                j = i + E;
                while (j % 8 != 0)
                {
                    allMoves |= 1UL << j;
                    j += E;
                }

                j = i + W;
                while (j % 8 != 7 && j >= 0)
                {
                    allMoves |= 1UL << j;
                    j += W;
                }

                j = i + N + E;
                while (j % 8 != 0 && j < 64)
                {
                    allMoves |= 1UL << j;
                    j += N + E;
                }

                j = i + N + W;
                while (j % 8 != 7 && j < 64)
                {
                    allMoves |= 1UL << j;
                    j += N + W;
                }

                j = i + S + E;
                while (j % 8 != 0 && j >= 0)
                {
                    allMoves |= 1UL << j;
                    j += S + E;
                }

                j = i + S + W;
                while (j % 8 != 7 && j >= 0)
                {
                    allMoves |= 1UL << j;
                    j += S + W;
                }
                SliderMoves[i] = allMoves;
            }

        }


        private static void GenerateKingMoves()
        {
            KingMoves = new ulong[64];

            for (int i = 0; i < 64; i++)
            {
                ulong allKingMoves = 0;
                if (i / 8 < 7)
                {
                    allKingMoves |= 1UL << (i + N);

                    if (i % 8 > 0)
                    {
                        allKingMoves |= 1UL << (i + N + W);
                    }

                    if (i % 8 < 7)
                    {
                        allKingMoves |= 1UL << (i + N + E);
                    }
                }

                if (i / 8 > 0)
                {
                    allKingMoves |= 1UL << (i + S);

                    if (i % 8 > 0)
                    {
                        allKingMoves |= 1UL << (i + S + W);
                    }

                    if (i % 8 < 7)
                    {
                        allKingMoves |= 1UL << (i + S + E);
                    }
                }

                if (i % 8 > 0)
                {
                    allKingMoves |= 1UL << (i + W);
                }

                if (i % 8 < 7)
                {
                    allKingMoves |= 1UL << (i + E);
                }

                KingMoves[i] = allKingMoves;
            }
        }
    }
}
