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
        public bool[] busDistribution;

        public Solution(int[] peopleDistribution, bool[] startSolution)
        {
            this.peopleDistribution = peopleDistribution;
            busDistribution = startSolution;
        }


        public Solution Copy()
        {
            var newPeopleDistribution = (int[])peopleDistribution.Clone();
            var newbusDistribution = (bool[])busDistribution.Clone();

            var solution = new Solution(newPeopleDistribution, newbusDistribution)
            {
                solutionScore = solutionScore
            };

            return solution;
        }
    }
}
