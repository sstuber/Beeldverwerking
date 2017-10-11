using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onderzoek_Bussimulatie.DataTypes
{
    class MoveBus : BuurtRuimte
    {
        private int _oldPlace;
        private int _newPlace;
        private int _newScore;

        public MoveBus(Solution solution, int oldPlace, int newPlace)
        {
            if (!solution.busDistribution[oldPlace])
                throw new Exception("no bus existed in this place ");

            if (solution.busDistribution[newPlace])
                throw new Exception("bus already existed in new place");

            _oldPlace = oldPlace;
            _newPlace = newPlace;
        }

        public override void AcceptNewSolution(Solution solution)
        {

            solution.busDistribution[_oldPlace] = false;
            solution.busDistribution[_newPlace] = true;
            solution.solutionScore = _newScore;
            solution.peopleRemaining = Simulation.waitingLine.Count;
        }

        public override int GetDifference(Solution solution)
        {
            bool[] newDistribution = (bool[]) solution.busDistribution.Clone();
            newDistribution[_oldPlace] = false;
            newDistribution[_newPlace] = true;

            _newScore = Simulation.CompleteSimulation(solution.peopleDistribution, newDistribution);

            return _newScore - solution.solutionScore;
        }
    }
}
