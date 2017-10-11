using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onderzoek_Bussimulatie
{
    class RemoveBus : BuurtRuimte
    {
        private readonly int _oldplace;
        private int _newScore = 0;

        public RemoveBus(Solution s, int oldplace)
        {
            if(!s.busDistribution[oldplace])
                throw new Exception("no bus existed in this place ");

            _oldplace = oldplace;
        }

        public override void AcceptNewSolution(Solution solution)
        {
            solution.busDistribution[_oldplace] = false;
            solution.solutionScore = _newScore;
            solution.busCount--;
            solution.peopleRemaining = Simulation.waitingLine.Count;
        }

        public override int GetDifference(Solution solution)
        {
            bool[] newDistribution = new bool[solution.busDistribution.Length];

            solution.busDistribution.CopyTo(newDistribution, 0);
            newDistribution[_oldplace] = false;

            _newScore = Simulation.CompleteSimulation(solution.peopleDistribution, newDistribution);

            return _newScore - solution.solutionScore;
            //this.cost is simulate new situation cost
        }
    }
}
