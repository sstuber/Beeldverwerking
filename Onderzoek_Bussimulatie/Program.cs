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
            for (int i = 31; i < 61; i += 5)
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

            // Calculate variables used in the distribution
            int counter = 0;
            int factor = (int)(60 / peaks);
            int difference = (int)(60 - peaks);
            if (difference > 0 && factor == 1)
            {
                // Set variables
                int remainder = (int) (peaks - difference);
                int mainDivider = (difference > remainder) ? 2 : 1;
                int groupSize = (mainDivider == 2) ? (difference/remainder) : (remainder/difference);
                int groupTotals = (mainDivider == 2) ? remainder : difference;
                int finalSize = (mainDivider == 2)
                    ? groupSize + (difference - (groupSize*remainder))
                    : groupSize + (remainder - (groupSize*difference));

                // Fill the array
                int diffCounter = 0;
                int groupCounter = 0;
                int completeGroups = 0;
                for (int i = 0; i < peaks; i++)
                {
                    if (completeGroups >= groupTotals - 1)
                        groupSize = finalSize;

                    if (i == (int) peaks - 1)
                        distribution[counter] = finalPeak;
                    else
                        distribution[counter] = peakHeight;

                    Console.WriteLine("Peak " + i + " on timestep " + counter + " contains " + distribution[counter] + " people");

                    // Improve distribution of large amount of peaks
                    if (mainDivider == 2) // For rounding down
                    {
                        if (groupCounter < groupSize && diffCounter < difference)
                        {
                            counter += factor + 1;
                            diffCounter++;
                        }
                        else
                        {
                            counter += factor;
                            groupCounter = -1;
                            completeGroups++;
                        }
                    }
                    else // For rounding up
                    {
                        if (groupCounter < groupSize - 1 && diffCounter < remainder)
                        {
                            counter += factor;
                            diffCounter++;
                        }
                        else
                        {
                            counter += factor + 1;
                            groupCounter = -1;
                            completeGroups++;
                        }
                    }
                    groupCounter++;
                }
            }
            else
            {
                // Use general distribution for other 
                for (int i = 0; i < peaks; i++)
                {
                    if (i == (int)peaks - 1)
                        distribution[counter] = finalPeak;
                    else
                        distribution[counter] = peakHeight;

                    Console.WriteLine("Peak " + i + " on timestep " + counter + " contains " + distribution[counter] + " people");
                    counter += (int)(60 / peaks);
                }
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

