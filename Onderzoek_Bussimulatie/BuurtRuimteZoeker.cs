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
        public int busOffset = 2;

        public BuurtRuimte ZoekBuurtRuimte(Solution solution)
        {
            double randomDouble = rnd.NextDouble();
            BuurtRuimte ruimte;

            if (randomDouble < 0.33)
                ruimte = AddBusBuurtRuimte(solution);
            else if (randomDouble < 0.66)
                ruimte = RemoveBusBuurtRuimte(solution);
            else
                ruimte = MoveBusBuurtRuimte(solution);
            return ruimte;
        }

        public BuurtRuimte MoveBusBuurtRuimte(Solution s)
        {
            // remove bus code
            List<int> busPlaces = new List<int>();

            for (int i = 0; i < s.busDistribution.Length; i++)
                if (s.busDistribution[i])
                    busPlaces.Add(i);

            if (busPlaces.Count == 0)
                return new Nullruimte();

            int oldplaceIndex = rnd.Next(busPlaces.Count);
            int oldplace = busPlaces[oldplaceIndex];

            int newBusPlace = rnd.Next(s.busDistribution.Length);

            // add bus code
            int counter = 0;

            while (IsInCorrectBusPlace(newBusPlace, busOffset, s.busDistribution))
            {
                newBusPlace = rnd.Next(s.peopleDistribution.Length);

                counter++;

                if (counter > 10)
                    return new Nullruimte();
            }

            return new MoveBus(s, oldplace, newBusPlace);
        }

        public BuurtRuimte AddBusBuurtRuimte(Solution s)
        {
            int newBusPlace = rnd.Next(s.busDistribution.Length);

            int counter = 0;

            while (IsInCorrectBusPlace(newBusPlace, busOffset, s.busDistribution))
            {
                newBusPlace = rnd.Next(s.busDistribution.Length);

                counter ++;

                if (counter > 10 )
                    return new Nullruimte();
            }

            return new AddBus(s, newBusPlace);
        }

        private bool IsInCorrectBusPlace(int newPlace, int offset,  bool[] busPlaces)
        {
            int minimum = newPlace - offset > 0 ? newPlace - offset : 0;
            int maximum = newPlace + offset + 1  < busPlaces.Length ? newPlace + offset + 1 : busPlaces.Length;

            for(int i = minimum; i < maximum; i++)
                if (busPlaces[i])
                    return true;

            return false;
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
