using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onderzoek_Bussimulatie
{
    class SimulatedAnnealing
    {
        Solution solution;
        Solution bestSolution;
        readonly BuurtRuimteZoeker zoeker;
        double _maxQ, t, alpha;
        // teller counts how many "reject Worse Solution" cases happen simultaneously
        int teller;
        double qCounter;
        long iteraties; // long want kan vaak zijn;

        public SimulatedAnnealing()
        {
            this.zoeker = new BuurtRuimteZoeker();
            _maxQ = 50000;
            t = 3000;
            alpha = 0.99;

            qCounter = _maxQ;
            iteraties = 1;

        }
        public SimulatedAnnealing(int t)
        {
           // this.zoeker = new BuurtRuimteZoeker();
            _maxQ = 50000;
            this.t = t;
            alpha = 0.99;

            qCounter = _maxQ;
            iteraties = 1;

        }

        public Solution SimulateThis(Solution solution)
        {
          /*  s.bestOrderlijst = null;
            s.bestOverCapacity = null;
            s.bestOverTime = null;*/
            /* if (bestSolution != null)
             {
                 if (s.score <= bestSolution.score)
                     bestSolution = s;
             }
             else
                 bestSolution = s;*/

            bestSolution = solution.Copy();

            while (!StopCondition(solution)) // als aan de conditie niet voldaan is
            {
               BuurtRuimte r = zoeker.ZoekBuurtRuimte(solution);

                int cost = r.GetDifference(solution);

                if (cost <= 0)
                { // buurtruimte is beter? accepteer
                    r.AcceptNewSolution(solution);
                    if (solution.solutionScore <= bestSolution.solutionScore)
                        bestSolution = solution;
                    aftermath(r);
                    teller = 0;
                    continue;

                }

                // accepteren we minder? nee? dan gaan we door met de volgende iteratie
                if (!AcceptWorse(cost))
                {
                    aftermath(r);
                    teller++;
                    continue;
                }

                if (solution.solutionScore <= bestSolution.solutionScore)
                    bestSolution = bestSolution.Copy();

                // we accepteren? we voegen samen
                r.AcceptNewSolution(solution);
                aftermath(r);
                teller = 0;
            }


            return bestSolution;
        }
        // zijn we d'r al? 
        private bool StopCondition(Solution s)
        {
            if (iteraties > 50000000 /*&& ( Program.overTime.Count == 0 && Program.overCapacity.Count == 0)*/)
                return true;

            if (teller > 10000)
                return true;
            else
                return false;
        }

        private bool AcceptWorse(int cost)
        {
            double chance = Math.Exp(-cost / t);
            if (BuurtRuimteZoeker.rnd.NextDouble() < chance)
                return true;
            return false;
        }


        // dingen als aantal iteraties bij houden en iets met q en t doen
        private void aftermath(BuurtRuimte r)
        {
            qCounter--;

            // moeten we t verlagen? 
            if (qCounter <= 0)
            {
                t *= alpha;
                qCounter = _maxQ;
            }

            iteraties++;

            if (iteraties % 10000000 == 0)
                Console.WriteLine(iteraties);
        }
    }
}
