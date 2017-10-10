using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;

namespace Onderzoek_Bussimulatie
{
    class Simulation
    {
        // Static objects representing the bus, score and queue
        public static Bus currentBus;
        public static int totalScore;
        public static Queue waitingLine;

        public static int CompleteSimulation(int[] distribution, bool[] busSchedule)
        {
            // Reset values
            currentBus = null;
            totalScore = 0;
            waitingLine = new Queue();

            // Simulate for 60 timesteps
            Console.WriteLine("--- Simulating 60 timesteps ---");
            for (int t = 0; t < 60; t++)
                SimulateTimestep(t, distribution, busSchedule);

            // Add waiting time and other penalties to the final score
            foreach (Person p in waitingLine)
                totalScore += p.waitingTime + 200; // Waiting time of people left at the station + additional penalty

            // Display people remaining on the station
            Console.WriteLine("People Remaining: " + waitingLine.Count);
            return totalScore;
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
