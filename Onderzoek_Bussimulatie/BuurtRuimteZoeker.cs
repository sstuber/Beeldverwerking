using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Onderzoek_Bussimulatie.DataTypes;

namespace Onderzoek_Bussimulatie
{
    class BuurtRuimteZoeker
    {
        public static Random rnd = new Random();

        public BuurtRuimte ZoekBuurtRuimte(Solution solution)
        {
            double randomDouble = rnd.NextDouble();
            BuurtRuimte ruimte;

            if (randomDouble < 0.5)
                ruimte = AddBusBuurtRuimte(solution);
            else
                ruimte = RemoveBusBuurtRuimte(solution); 
            return ruimte;
        }

        public BuurtRuimte AddBusBuurtRuimte(Solution s)
        {
            int newBusPlace = rnd.Next(s.peopleDistribution.Length);


            int counter = 0;

            while (s.busDistribution[newBusPlace] ||
                (newBusPlace + 1 < s.peopleDistribution.Length && s.busDistribution[newBusPlace + 1]) ||
                (newBusPlace > 0 && s.busDistribution[newBusPlace -1]))
            {
                newBusPlace = rnd.Next(s.peopleDistribution.Length);

                counter ++;

                if (counter > 10 )
                    return new Nullruimte();
            }

            return new AddBus(s, newBusPlace);
        }

        public BuurtRuimte RemoveBusBuurtRuimte(Solution s)
        {
            List<int> busPlaces = new List<int>();

            for (int i = 0; i < s.busDistribution.Length; i++)
                if (s.busDistribution[i])
                    busPlaces.Add(i);

            if (busPlaces.Count == 0)
                return new Nullruimte();

            int oldplaceIndex = rnd.Next(busPlaces.Count);
            int oldplace = busPlaces[oldplaceIndex];

            return new RemoveBus(s, oldplace);
        }

    }
}
