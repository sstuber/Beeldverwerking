using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onderzoek_Bussimulatie
{
    abstract class BuurtRuimte
    {
        /*  elke variant van deze klas bevat de verandering
         *  in de constructor moet je dus alle informatie die de buurtruimte
         *  is weten door te geven. 
         */
        public abstract void AcceptNewSolution(Solution solution);

        // geeft delta cost. Negative kost is beter
        public abstract int GetDifference(Solution solution);

       // public abstract bool IsOverTime();
    }
}
