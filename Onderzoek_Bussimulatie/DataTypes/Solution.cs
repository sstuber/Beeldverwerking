using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onderzoek_Bussimulatie
{
    class Solution
    {
        public int solutionScore = 0;
        public int[] peopleDistribution;

        public Solution(int[] peopleDistribution)
        {
            this.peopleDistribution = peopleDistribution;
        }


        public Solution Copy()
        {
            return this;
        }
    }
}
