using PhotoSlideshow.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

namespace PhotoSlideshow
{
    class Program
    {
        static void Main(string[] args)
        {
            
            int fileToRead = 2;
            int time = 5;
            int numberOfIterations = 500;
            Stopwatch stopw = new Stopwatch();

            Random random = new Random();
            Solution solution = new Solution();

            string[] files = Directory.GetFiles($"Samples", "*.txt");

            List<Slide> slides = new List<Slide>();
            Instance instance = Extensions.IO.ReadInput(files[fileToRead]);

            Console.WriteLine($"Number of photos: {instance.NumberOfPhotos}\n");

            // stopw.Start();
            solution.IteratedLocalSearch(numberOfIterations, 50);
           // stopw.Stop();
            //solution.SimulatedAnnealing(100, 0.1, slides);

        
            solution.GenerateOutputFile($"{Path.GetFileNameWithoutExtension(files[fileToRead])}_result_{DateTime.Now.Ticks}.txt");

            Console.WriteLine($"Number of slides: { solution.Slides.Count() }\n");

            Console.ReadKey();
           
        }
    }
}
