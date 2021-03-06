﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onderzoek_Bussimulatie
{
    class Solution
    {
        public int solutionScore = 0;
        public int busCount = 0;
        public int peopleRemaining = 0;

        public int[] peopleDistribution;
        public bool[] busDistribution;

        public Solution(int[] peopleDistribution, bool[] startSolution)
        {
            this.peopleDistribution = peopleDistribution;
            busDistribution = startSolution;

            for (int i =0; i< busDistribution.Length;i++)
                if (busDistribution[i])
                    busCount++;
        }


        public Solution Copy()
        {
            var newPeopleDistribution = (int[])peopleDistribution.Clone();
            var newbusDistribution = (bool[])busDistribution.Clone();

            var solution = new Solution(newPeopleDistribution, newbusDistribution)
            {
                solutionScore = solutionScore,
                peopleRemaining = peopleRemaining
            };

            return solution;
        }
    }
}
