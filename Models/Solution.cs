using PhotoSlideshow.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PhotoSlideshow.Models
{
    public class Solution
    {
        public List<Slide> Slides { get; set; }

        public int InterestFactor { get; set; } = int.MinValue;
       
        private double PerturbationPercentage  = 0.1;
        public Solution()
        {
            this.Slides = new List<Slide>();
        }
        public Solution(List<Slide> Slides)
        {
            this.Slides = Slides;
        }
        public void GenerateRandomSolution(List<Photo> photos)
        {
            int slideId = 0;
            Random random = new Random();
            List<int> photosToSkip = new List<int>();
           // RemovePhoto(photos);
            while (photosToSkip.Count() < photos.Count())
            {
                int randomStart = random.Next(0, photos.Count() - 1);
                Photo photo = photos.Where(x => randomStart == x.Id).FirstOrDefault();
                ChangePositionOfPhoto(photos);
                List<Photo> photosToAdd = new List<Photo>()
                {
                    photo
                };
                
                
                if (photo.Orientation == Orientation.V)
                {
                    Photo secondPhoto = photos.FirstOrDefault(x => x.Id != photo.Id && x.Orientation.Equals(Orientation.V) && !photosToSkip.Contains(x.Id));
                    if (secondPhoto != null)
                    {
                        photosToAdd.Add(secondPhoto);
                        photosToSkip.Add(secondPhoto.Id);
                    }
                }

                photosToSkip.Add(photo.Id);
                this.Slides.Add(new Slide(slideId, photosToAdd));
                slideId++;
                DifferentPositionsSlides(Slides);
                ChangePositionFirstWithLast(Slides);
            }
        }

        //Operators
        public List<Photo> RemovePhoto(List<Photo> photo)
        {
            Random random = new Random();
            photo.RemoveAt(random.Next(0, photo.Count));
            return photo;
        } 
        public List<Photo> ChangePositionOfPhoto(List<Photo> photo)
        {
            for(int i=0;i<photo.Count-1; i++)
            {
                var temp = photo[i];
                photo[i] = photo[i + 1];
                photo[i + 1] = temp;
            }   
            return photo;
        }

        public List <Slide> DifferentPositionsSlides(List <Slide> slide)
        {
            for (int i = 0; i < slide.Count-1; i++)
            {
                var temp = slide[i];
                slide[i] = slide[i+1];
                slide[i+1] = temp;
            }
            return slide;
        }
        public List<Slide> ChangePositionFirstWithLast(List<Slide> slide)
        {
            var temp = slide[0];
            slide[0] = slide[slide.Count - 1];
            slide[slide.Count - 1] = temp;
            return slide; 
        }
        public List<Photo> Find(List<Photo> photos)
        {
            List<Photo> ph = new List<Photo>();
            for(int i=0; i<photos.Count-1; i++)
            {
                if((FindCommonTagsPhotos(photos[i], photos[i + 1])>=1))
                {
                    ph.Add(photos[i]);
                }
            }
            return ph;
        }
      
        public void GenerateSolutionWithHeuristic(List<Photo> photos)
        {
            int slideId = 0; 
            int miniPhotoToTake = photos.Count/2;

            for (int i = 0; i < photos.Count - 1; i++)
            {
                List<Photo> tempPhotos = new List<Photo>(photos.Skip(i * miniPhotoToTake).Take(miniPhotoToTake));
                int tempPhotosCount = tempPhotos.Count();
                int iterationCount = 0;

                while (iterationCount < tempPhotosCount)
                {
                    Photo photo;
                    if (iterationCount != 0 && i != 0)
                    {
                        photo = tempPhotos.OrderByDescending(x =>
                                            x.Tags.Where(t => !this.Slides.LastOrDefault().Tags.Contains(t)).Count() +
                                            x.Tags.Where(t => this.Slides.LastOrDefault().Tags.Contains(t)).Count() +
                                            this.Slides.LastOrDefault().Tags.Where(t => x.Tags.Contains(t)).Count())
                                        .FirstOrDefault();
                    }
                    else
                    {
                        photo = tempPhotos.FirstOrDefault();
                    }


                    List<Photo> photosToAdd = new List<Photo>()
                      {
                        photo
                      };

                    if (photo.Orientation == Orientation.V)
                    {
                        Photo secondPhoto = tempPhotos
                            .Where(x => x.Id != photo.Id && x.Orientation.Equals(Orientation.V))
                            .OrderByDescending(x =>
                                x.Tags.Where(t => !photo.Tags.Contains(t)).Count() +
                                x.Tags.Where(t => photo.Tags.Contains(t)).Count())
                            .FirstOrDefault();

                        if (secondPhoto != null)
                        {
                            photosToAdd.Add(secondPhoto);
                            tempPhotos.Remove(secondPhoto);

                            iterationCount++;
                        }
                    }

                    this.Slides.Add(new Slide(slideId, photosToAdd));
                    tempPhotos.Remove(photo);

                    iterationCount++;
                    slideId++;
                }
            }

        }
      
        public void Mutate1(List<Slide> slides)
        {
            Random rnd = new Random();
            for(int i = 0; i<10; i++)
            {
                int r1 = rnd.Next(0, slides.Count);
                a: int r2 = rnd.Next(0, slides.Count);

                if (r1 == r2)
                {
                    goto a;
                }
                else
                {
                    for (int j = 0; j < 2; j++)
                    {
                        var temp = slides[r1];
                        slides[r1] = slides[r2];
                        slides[r2] = temp;
                    }
                }
            }

        }

        public void HillClimbing(int numberOfIterations)
        {
            Random random = new Random();
            List<int> randomNumbers = new List<int>();
            for (int i = 0; i < this.Slides.Count(); i++)
            {
                randomNumbers.Add(i);
            }

            for (int i = 0; i < numberOfIterations; i++)
            {
                List<Slide> tempSolution = this.Slides;
                List<int> slidesToSwap = randomNumbers.OrderBy(x => random.Next()).Take(2).ToList();
                DifferentPositionsSlides(this.Slides);
                // Mutate(tempSolution, randomNumbers);

                int currentInterestFactor = CalculateInterestFactor(tempSolution);
                if (currentInterestFactor >= this.InterestFactor)
                {
                    this.Slides = new List<Slide>(tempSolution);
                    this.InterestFactor = currentInterestFactor;
                }
            }
        }

        public void HillClimbingWithAdditionalFeatures(int numberOfIterations)
        {
            Random random = new Random();
            List<int> randomNumbers = new List<int>();
            for (int i = 0; i < this.Slides.Count(); i++)
            {
                randomNumbers.Add(i);
            }

            for (int i = 0; i < numberOfIterations; i++)
            {
                List<Slide> tempSolution = DifferentPositionsSlides(this.Slides);
                List<int> slidesToSwap = randomNumbers.OrderBy(x => random.Next()).Take(2).ToList();

                Mutate1(tempSolution);

                int currentInterestFactor = CalculateInterestFactor(tempSolution);
                if (currentInterestFactor >= this.InterestFactor)
                {
                    this.Slides = new List<Slide>(tempSolution);
                    this.InterestFactor = currentInterestFactor;
                }
            }

        }
        public int CalculateInterestFactorPhoto(Photo photoA,Photo photoB)
        {
            int interestFactor = 0;
            
                int commonTags = FindCommonTagsPhoto(photoA, photoB);
                int slideAnotB = FindDifferenteTagsPhoto(photoA, photoB);
                int slideBnotA = FindDifferenteTagsPhoto(photoB, photoA);
                interestFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));
            
            return interestFactor;
        }

        public int CalculateInterestFactor(List<Slide> slides)
        {
            int interestFactor = 0;
            for (int i = 0; i < slides.Count - 1; i++)
            {
                int commonTags = FindCommonTags(slides[i], slides[i + 1]);
                int slideAnotB = FindDifferenteTags(slides[i], slides[i + 1]);
                int slideBnotA = FindDifferenteTags(slides[i + 1], slides[i]);
                interestFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));
            }
            return interestFactor;
        }
        public int FindCommonTagsPhotos(Photo PhotoA, Photo PhotoB)
        {
            return PhotoA.Tags.Where(x => PhotoB.Tags.Contains(x)).Count();
        }
        public int FindCommonTagsPhoto(Photo photoA, Photo photoB)
        {
            return photoA.Tags.Where(x => photoB.Tags.Contains(x)).Count();
        }
        
        public int FindCommonTags(Slide slideA, Slide slideB)
        {
            return slideA.Tags.Where(x => slideB.Tags.Contains(x)).Count();
        }
        public int FindDifferenteTagsPhoto(Photo photoA, Photo photoB)
        {
            return photoA.Tags.Where(x => !photoB.Tags.Contains(x)).Count();
        }
        public int FindDifferenteTags(Slide slideA, Slide slideB)
        {
            return slideA.Tags.Where(x => !slideB.Tags.Contains(x)).Count();
        }

        public void GenerateOutputFile(string filename)
        {
            using (StreamWriter file = new StreamWriter(new FileStream(filename, FileMode.CreateNew)))
            {
                file.WriteLine(this.Slides.Count);
                foreach (Slide slide in this.Slides)
                {
                    file.WriteLine($"{string.Join(" ", slide.Photos.Select(x => x.Id).ToList())}");
                }
            }
        }

     //ILS 
        public void IteratedLocalSearch(int totalIterations, long IdealSolution) //, Stopwatch stopwatch, int time
        {
            int fileToRead = 2;
            List<int> DistributionOfTime = GetRandomIterations(Convert.ToInt32(0.5 * totalIterations), totalIterations); // T
            Solution solution = new Solution();
            string[] files = Directory.GetFiles($"Samples", "*.txt");
            Random random = new Random();
            List<Slide> slides = new List<Slide>();
            Instance instance = Extensions.IO.ReadInput(files[fileToRead]);
            GenerateSolutionWithHeuristic(instance.Photos.ToList());
            //GenerateRandomSolution(instance.Photos.ToList());// S
                                     //instance.Photos.OrderBy(x => random.Next()).ToList()
            List<Slide> S = this.Slides;
            List <Slide> H = S; // H
            List <Slide> Best = S; // Best
            List <Slide> R;


            Random rnd = new Random();

            while (CalculateInterestFactor(Best) != IdealSolution && totalIterations > 0)
            {
                int CurrentIterationsPerDistribution = DistributionOfTime[rnd.Next(DistributionOfTime.Count)];

                while (CalculateInterestFactor(S) != IdealSolution && CurrentIterationsPerDistribution > 0 && totalIterations > 0)
                {
                     R = S;
                    Random ran = new Random();
                    List<int> randomNumbers = new List<int>();
                    for (int i = 0; i < this.Slides.Count(); i++)
                    {
                        randomNumbers.Add(i);
                    }

                    Mutate1(R);
                    

                    if (CalculateInterestFactor(R) > CalculateInterestFactor(S))
                        S = R;

                    CurrentIterationsPerDistribution--;
                    totalIterations--;
                }

                if (CalculateInterestFactor(S) > CalculateInterestFactor(Best))
                {
                    Best = S;
                }

                H = NewHomeBase(H, S);
                S = Perturb(H);

            }
            Console.WriteLine("Interes factor: " + CalculateInterestFactor(Best));

        }
        public List <Slide> NewHomeBase(List <Slide> H, List<Slide> S)
        {
            if (CalculateInterestFactor(S) >= CalculateInterestFactor(H))
                return SimulatedAnnealing(400, 0.1, S);
            else
                return SimulatedAnnealing(400, 0.1, H);
        }
        public List <Slide> Perturb(List <Slide> H)
        {
            int MutationCounter = Convert.ToInt32(PerturbationPercentage * H.Count);

            for (int i = 0; i < MutationCounter; i++)
            {
                List<int> randomNumbers = new List<int>();
                for (int ii = 0; ii < this.Slides.Count(); ii++)
                {
                    randomNumbers.Add(ii);
                }
                Mutate1(H);
            }

            return H;
        }
       
        public List<int> GetRandomIterations(int count, int totalIterations)
        {
            // min 30% max 70% e totaliterations
            Random random = new Random();
            int minValueOfIteration = (int)Math.Ceiling(0.1 * totalIterations);
            int maxValueOfIteration = (int)Math.Ceiling(0.9 * totalIterations);

            List<int> randomIterations = new List<int>();
            for (int i = 0; i < count; i++)
            {
                randomIterations.Add(random.Next(minValueOfIteration, maxValueOfIteration));
            }

            return randomIterations;
        }
        public List<Slide> SimulatedAnnealing(double temperature, double minDec, List<Slide> slides)
        {
            Random rnd = new Random();
            List<Slide> BestS = new List<Slide>();
            List<Slide> S = new List<Slide>();
            List<Slide> R = new List<Slide>();
            double e = 0.001;
            S = slides;
            BestS = S;
            while (temperature > 0)
            {
                R = S;
                Mutate1(R);
                if(CalculateInterestFactor(R)>=CalculateInterestFactor(S) || (rnd.Next(0,100) * e) < (Math.Exp(CalculateInterestFactor(R) - CalculateInterestFactor(S)) / temperature))
                {
                    S = R;
                    temperature *= minDec;
                }
                if(CalculateInterestFactor(S) >= CalculateInterestFactor(BestS)){
                    BestS = S;
                }
                var a = CalculateInterestFactor(R);
                var b = CalculateInterestFactor(S);
            }
            return BestS;
        }
       
    }
}
