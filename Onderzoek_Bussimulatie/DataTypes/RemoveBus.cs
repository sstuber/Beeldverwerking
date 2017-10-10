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

        }

        public override int GetDifference(Solution solution)
        {
            //this.cost is simulate new situation cost
            throw new NotImplementedException();
        }
    }
}
