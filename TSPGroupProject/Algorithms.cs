using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TSP
{
    class Algorithms
    {

        public ArrayList RandomSolution(City[] Cities)
        {
            ArrayList randomRoute = new ArrayList();
            List<int> remainingCities = new List<int>();
            int currCity = -1;
            int prevCity = -1;
            bool notConnected = true;
            double dist = -1;

            remainingCities = initRemaining(remainingCities, Cities);

            while (notConnected)
            {
                while (remainingCities.Count > 0)
                {
                    if (remainingCities.Count > 1)
                    {                        
                        currCity = getUniqueRandom(Cities.Length, remainingCities);

                        if (prevCity != -1)
                        {
                            dist = Cities[prevCity].costToGetTo(Cities[currCity]);

                            if (!double.IsInfinity(dist))
                            {
                                randomRoute.Add(Cities[currCity]);
                                remainingCities.Remove(currCity);
                                prevCity = currCity;
                            }
                        }
                        else
                        {
                            randomRoute.Add(Cities[currCity]);
                            remainingCities.Remove(currCity);
                            prevCity = currCity;
                        }
                    }
                    else
                    {
                        // no need to get a random number if only 1 city is left in the list, just return the remaining city
                        currCity = remainingCities[remainingCities.Count - 1];
                        dist = Cities[prevCity].costToGetTo(Cities[currCity]);

                        if (!double.IsInfinity(dist))
                        {                        
                            randomRoute.Add(Cities[currCity]);
                            remainingCities.Remove(currCity);
                            prevCity = currCity;
                        }
                        else
                        {
                            // Next to last city cannot connect to last city.  Need to reset and try again to get a connected route.
                            randomRoute.Clear();
                            remainingCities.Clear();
                            currCity = -1;
                            prevCity = -1;
                            remainingCities = initRemaining(remainingCities, Cities);
                        }
                    }
                }

                dist = Cities[currCity].costToGetTo((City)randomRoute[0]);

                if (!double.IsInfinity(dist))
                {
                    notConnected = false;
                }
                else
                {
                    // Last city cannot connect to first city.  Need to reset and try again to get a connected route.
                    randomRoute.Clear();
                    remainingCities.Clear();
                    currCity = -1;
                    prevCity = -1;
                    remainingCities = initRemaining(remainingCities, Cities);
                }
            }

            return randomRoute;
        }


        public List<int> initRemaining(List<int> remainingCities, City[] Cities)
        {
            //initialize the remaining cities array with all the cities
            for (int i = 0; i < Cities.Length; i++)
            {
                remainingCities.Add(i);
            }

            return remainingCities;
        }


        public ArrayList GreedySolution(City[] Cities)
        {
            ArrayList greedyRoute = new ArrayList();

            for (int w = 0; w < Cities.Length; w++)
            {
                greedyRoute.Clear();
                List<int> remainingCities = new List<int>();

                //initialize the remaining cities array with all the cities
                for (int i = 0; i < Cities.Length; i++)
                {
                    remainingCities.Add(i);
                }

                int nextCity = w;
                City firstCity = Cities[w];
                City currCity = firstCity;

                while (remainingCities.Count > 0)
                {
                    greedyRoute.Add(Cities[nextCity]);
                    remainingCities.Remove(nextCity);

                    double bestDist = double.PositiveInfinity;
                    int bestCity = -1;

                    foreach (int city in remainingCities)
                    {
                        double dist = Cities[nextCity].costToGetTo(Cities[city]);

                        if (bestDist > dist)
                        {
                            bestCity = city;
                            bestDist = dist;
                            currCity = Cities[bestCity];
                        }
                    }

                    nextCity = bestCity;
                }

                if ((currCity.costToGetTo(firstCity) != double.PositiveInfinity) && (greedyRoute.Count == Cities.Length))
                {
                    // Solution found, stop looping
                    break;
                }
            }

            return greedyRoute;
        }


        public ArrayList BranchAndBound(City[] Cities, ArrayList Route, double BSSF)
        { 
            Node startNode = new Node(Cities);
            startNode.initStaticMembers();
            PriorityQueue pQ = new PriorityQueue(startNode);
            ArrayList bbRoute = null;
            Stopwatch timer = new Stopwatch();
            timer.Start();

            //while ((pQ.Size() > 0) && (pQ.Peek() < BSSF) && (timer.Elapsed.TotalMinutes < 10))
            while ((pQ.Size() > 0) && (pQ.Peek() < BSSF) && (timer.Elapsed.TotalSeconds < 30))
            {
                // Keep track of the largest size of the queue
                if (pQ.Size() > Node.maxNodesCreated)
                {
                    Node.maxNodesCreated = pQ.Size();
                }

                startNode = pQ.DeleteMinimum();
                //startNode.matrix2Table(); 

                // Check for a solution
                if (startNode.includedEdges == Cities.Length)
                {
                    ArrayList tempRoute = new ArrayList();

                    if (!startNode.exited.Contains(-1))
                    {
                        int index = 0;

                        while (tempRoute.Count < Cities.Length)
                        {
                            tempRoute.Add(Cities[startNode.exited[index]]);
                            index = startNode.exited[index];
                        }

                        BSSF = startNode.lowerBound;
                        bbRoute = tempRoute;
                        Node.numSolutions++;
                    }
                }

                Node incNode = new Node(startNode);
                Node excNode = new Node(startNode);
                Node maxInclude = null;
                Node maxExclude = null;

                double maxDiff = -1;
                double diff = 0;
                int maxRow = 0;
                int maxCol = 0;

                for (int row = 0; row < startNode.matrixSize; row++)
                {
                    for (int col = 0; col < startNode.matrixSize; col++)
                    {
                        if (startNode.rcMatrix[row, col] == 0)
                        {
                            Node includeNode = new Node(incNode, true, row, col);
                            Node excludeNode = new Node(excNode, false, row, col);                            
                            diff = excludeNode.lowerBound - includeNode.lowerBound;
                            Node.numStatesCreated += 2;

                            if (diff > maxDiff)
                            {
                                maxDiff = diff;
                                maxRow = row;
                                maxCol = col;
                                maxInclude = new Node(includeNode);
                                maxExclude = new Node(excludeNode);
                            }                            
                        }
                    }
                }

                if (maxInclude != null && maxInclude.lowerBound < BSSF)
                {
                    pQ.Insert(maxInclude);
                }
                else
                {
                    Node.pruneCount++;
                }

                if (maxExclude != null && maxExclude.lowerBound < BSSF)
                {
                    pQ.Insert(maxExclude);
                }
                else
                {
                    Node.pruneCount++;
                }
            }

            timer.Stop();
            Node.timeElapsed = timer.ElapsedMilliseconds;

            if (bbRoute == null)
            {
                return Route;
            }
            else
            {
                // Add the rest of the queue to the pruned count
                Node.pruneCount += pQ.Size();
                return bbRoute;
            }
        }


        public ArrayList SimulatedAnnealingSolution(City[] Cities, ArrayList simAnnealRoute)
        {
            //ArrayList simAnnealRoute = GreedySolution(Cities);
            ArrayList nextRoute = new ArrayList();
            Random random = new Random(DateTime.Now.Millisecond);

            int iteration = -1;
            //double temperature = 50000.0;
            double temperature = Program.MainForm.saMaxTemp;
            double deltaDistance = 0;
            double coolingRate = 0.9999;
            double absTemperature = 0.00001;
            double distance = getCost(simAnnealRoute);

            while (temperature > absTemperature)
            {
                nextRoute = getNextRoute(simAnnealRoute, Cities);
                deltaDistance = getCost(nextRoute) - distance;

                if (deltaDistance < 0 || (distance > 0 && Math.Exp(-deltaDistance / temperature) > random.NextDouble()))
                {
                    for (int i = 0; i < nextRoute.Count; i++)
                    {
                        simAnnealRoute[i] = nextRoute[i];
                    }

                    distance = deltaDistance + distance;
                }

                temperature *= coolingRate;
                iteration++;
            }

            return simAnnealRoute;
        }


        private ArrayList getNextRoute(ArrayList prev, City[] Cities)
        {
            ArrayList newRoute = new ArrayList(prev);

            List<int> remainingCities = new List<int>();

            //initialize the remaining cities array with all the cities
            for (int i = 0; i < Cities.Length; i++)
            {
                remainingCities.Add(i);
            }

            int firstCity = new Random(DateTime.Now.Millisecond).Next(Cities.Length);

            remainingCities.Remove(firstCity);

            int secondCity = getUniqueRandom(Cities.Length, remainingCities);

            City temp = (City)newRoute[firstCity];
            newRoute[firstCity] = newRoute[secondCity];
            newRoute[secondCity] = temp;

            return newRoute;
        }


        private double getCost(ArrayList route)
        {
            ProblemAndSolver.TSPSolution r = new ProblemAndSolver.TSPSolution(route);

            return r.costOfRoute();
        }

        /// <summary>
        /// Gets a unique random number up to the original set size.  Ensures the number is in the current set.  
        /// </summary>
        /// <param name="max">The maximum value from which a random number can be selected.</param>
        /// <param name="remainingCities">A list of city indexes that can still be selected.</param>
        /// <returns>A random integer.</returns>
        private int getUniqueRandom(int max, List<int> remainingCities)
        {
            int r = 0;
            Random random = new Random(DateTime.Now.Millisecond);
            bool gimmeNother = true;

            while (gimmeNother)
            {
                r = random.Next(max);

                // See if the current random number (r) is in the list (has not already been used).
                // If it is in the list it is valid, else get another.
                if (remainingCities.Contains(r))
                {
                    gimmeNother = false;
                }
            }

            return r;
        }

    }
}
