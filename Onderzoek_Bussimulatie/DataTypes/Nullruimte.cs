using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onderzoek_Bussimulatie.DataTypes
{
    class Nullruimte :BuurtRuimte
    {
        public override void AcceptNewSolution(Solution solution)
        {
            return;
            
        }

        public override int GetDifference(Solution solution)
        {
            return -1;
        }
    }
}
