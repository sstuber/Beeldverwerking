using System;
using System.Collections;

// Onderzoeksmethoden
// Olaf Kampers - 4255194
// Stan van Meerendonk - 4284763

namespace Onderzoek_Bussimulatie
{
    class BusSimulation
    {
        // Static objects representing the bus, score and queue
        public static Bus currentBus = null;
        public static int totalBus = 0;
        public static int totalScore = 0;
        public static Queue waitingLine = new Queue();

        static void Main(string[] args)
        {
            // Distribute total people over selected amount of peaks
            Console.WriteLine("Select amount of peaks in an hour: ");
            int peaks = int.Parse(Console.ReadLine());

            int totalPeople = 4000; // Total people in simulation
            int[] distribution = new int[60]; // array containing the distribution of people
            int peakHeight = totalPeople / peaks;
            int finalPeak = peakHeight + (totalPeople - peakHeight * peaks);

            int counter = 0;
            for (int i = 0; i < peaks; i++)
            {
                if (i == peaks - 1)
                    distribution[counter] = finalPeak;
                else
                    distribution[counter] = peakHeight;

                Console.WriteLine("Peak " + i + " on timestep " + counter + " contains " + distribution[counter] + " people");
                counter += 60 / peaks;
            }

            // Represents the schedule for the busses
            // TRUE = new bus arrives at timestep of the index
            // FALSE = nothing happens
            // Start Solution: only one bus arrives (at timestep 0)
            // -- TO ADD: Simulated Annealing for creation of optimal schedule
            bool[] busSchedule = new bool[60];
            busSchedule[0] = true;
            for (int k = 1; k < busSchedule.Length; k++)
                busSchedule[k] = false;

            // Simulate for 60 timesteps
            Console.WriteLine("--- Simulating 60 timesteps ---");
            for (int t = 0; t < 60; t++)
                SimulateTimestep(t, distribution, busSchedule);

            // Add waiting time and other penalties to the final score
            foreach (Person p in waitingLine)
                totalScore += p.waitingTime + 200; // Waiting time of people left at the station + additional penalty

            Console.WriteLine("People Remaining: " + waitingLine.Count);
            Console.WriteLine("Busses in simulation: " + totalBus);
            Console.WriteLine("Score: " + totalScore);
            Console.ReadLine();
        }

        // Simulates one timestep
        static void SimulateTimestep(int timeStep, int[] distribution, bool[] busSchedule)
        {
            // Add people to the queue according to the distribution
            if (distribution[timeStep] != null)
            {
                for (int i = 0; i < distribution[timeStep]; i++)
                    waitingLine.Enqueue(new Person());
            }

            // Check if the bus has to leave
            if (currentBus != null && currentBus.waitingTime == 2)
                currentBus = null; // Bus leaves

            // Check if new bus is scheduled to arrive
            if (busSchedule[timeStep])
            {
                currentBus = new Bus();
                totalBus++;
                totalScore += 1000; // Cost for new bus is added to the total score
            }

            // Remove people from the queue
            if (currentBus != null)
            {
                while (currentBus.peopleInBus < currentBus.capacity && waitingLine.Count > 0)
                {
                    // Remove person from queue, increase bus counter and score
                    Person p = (Person)waitingLine.Dequeue();
                    totalScore += p.waitingTime;
                    currentBus.peopleInBus++;
                }

                currentBus.waitingTime++; // Increase waiting time of the bus
            }

            IncreaseWaitingTime(); // Increase the waiting time of people remaining
        }

        // Increases the waiting time of every person in the queue
        static void IncreaseWaitingTime()
        {
            foreach (Person p in waitingLine)
                p.waitingTime++;
        }
    }
}

