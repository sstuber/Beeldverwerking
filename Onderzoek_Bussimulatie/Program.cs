using System;

// Onderzoeksmethoden
// Olaf Kampers - 4255194
// Stan van Meerendonk - 4284763

namespace Onderzoek_Bussimulatie
{
    class BusSimulation
    {
        static void Main(string[] args)
        {
            // Distribute total people over selected amount of peaks
            for (int i = 31; i < 60; i += 5)
            {
                double peaks = i;
                double totalPeople = 4000; // Total people in simulation
                int[] distribution = DistributePeople(totalPeople, peaks);
                // Represents the schedule for the busses
                // TRUE = new bus arrives at timestep index
                // FALSE = nothing happens
                bool[] busSchedule = CreateInitialSchedule();

                SimulatedAnnealing annealing = new SimulatedAnnealing();

                var solution = annealing.SimulateThis(distribution, busSchedule);
                // Display results of the final solution
                Console.WriteLine("Peaks: " + peaks + ", Score: " + solution.solutionScore + ", Total busses: " + solution.busCount + ", Remaining people: " + solution.peopleRemaining);
            }
            
            Console.ReadLine();
        }

        // Function that distributes people according to defined peaks
        static int[] DistributePeople(double totalPeople, double peaks)
        {
            int[] distribution = new int[60]; // Array containing the distribution of people
            double averagePeak = totalPeople / peaks;
            int peakHeight = (int)Math.Ceiling(averagePeak); // Average people in peak
            int finalPeak = (int)(peakHeight + (totalPeople - peakHeight * peaks)); // Adjust final peak

            int counter = 0;
            int factor = (int)(60 / peaks);
            int difference = (int)(60 - peaks) - 1;
            int diffCounter = 0;
            for (int i = 0; i < peaks; i++)
            {
                if (i == (int)peaks - 1)
                    distribution[counter] = finalPeak;
                else
                    distribution[counter] = peakHeight;

                Console.WriteLine("Peak " + i + " on timestep " + counter + " contains " + distribution[counter] + " people");

                // Improve distribution of large amount of peaks
                if (factor == 1 && i%3 != 0 && diffCounter < difference)
                {
                    counter += 2;
                    diffCounter++;
                }
                else
                    counter += factor;
            }
            return distribution;
        }

        // Creates initial schedule used in the simulated annealing
        static bool[] CreateInitialSchedule()
        {
            // Initial schedule only contains 1 bus at timeStep 0
            bool[] busSchedule = new bool[60];
            busSchedule[0] = true;
            for (int k = 1; k < busSchedule.Length; k++)
                busSchedule[k] = false;

            return busSchedule;
        }
    }
}

