using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomTest
{
    class Program
    {
        static void Main(string[] args)
        {

            Random rnd = new Random();

            int testLength = 10000;

            int amountofbuckets = 1000;

            int[] buckets = new int[amountofbuckets];

            Console.WriteLine("start generating");

            for (int i = 0; i < testLength * amountofbuckets; i++)
            {
                int index = (int) (rnd.NextDouble()* amountofbuckets);
                buckets[index]++;
            }

            Console.WriteLine("start printing");

            for (int i = 0; i < buckets.Length; i++)
            {
                Console.WriteLine("bucket {0} bevat {1} aantal" ,i,buckets[i]);
            }

            Console.ReadLine();
        }
    }
}
