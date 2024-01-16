namespace Chess.Brain
{
    //this class serves only for analysis of the engine
    public class SearchResults
    {
        public readonly int depthSearched;
        public readonly int eval;
        public readonly int naiveEval;

        public SearchResults(int depth, int ev, int naive)
        {
            depthSearched = depth;
            eval = ev;
            naiveEval = naive;
        }
    }
}
