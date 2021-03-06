﻿using System;
using System.Runtime.InteropServices;

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
            for (int i = 1; i < 61; i += 1)
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
            double averagePeak = totalPeople / peaks;
            int peakHeight = (int)Math.Ceiling(averagePeak); // Average people in peak
            int finalPeak = (int)(peakHeight + (totalPeople - peakHeight * peaks)); // Adjust final peak
            return FixedDistribution.GetDistribution(peaks, peakHeight, finalPeak);
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

