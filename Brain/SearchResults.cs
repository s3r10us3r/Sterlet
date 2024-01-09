using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Brain
{
    //this class serves only for analysis of the engine by me
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
