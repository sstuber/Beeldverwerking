using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onderzoek_Bussimulatie
{
    class AddBus : BuurtRuimte
    {
        private readonly int _newBusPlace;
        private int _newScore = 0;


        public AddBus(Solution s, int newPlace)
        {
            if (s.busDistribution[newPlace])
                throw new Exception("bus already existed in new place");

            _newBusPlace = newPlace;
        }


        public override void AcceptNewSolution(Solution solution)
        {
            solution.busDistribution[_newBusPlace] = true;
            solution.solutionScore = _newScore;
            solution.busCount++;
            solution.peopleRemaining = Simulation.waitingLine.Count;
        }

        public override int GetDifference(Solution solution)
        {
            bool[] newDistribution = new bool[solution.busDistribution.Length];

            solution.busDistribution.CopyTo(newDistribution,0);
            newDistribution[_newBusPlace] = true;

            _newScore = Simulation.CompleteSimulation(solution.peopleDistribution, newDistribution);

            return _newScore - solution.solutionScore;
            //this.cost is simulate new situation cost
        }
    }
}
