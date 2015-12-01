using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    class Node
    {
        public double[,] rcMatrix;
        public double lowerBound = 0;
        public int matrixSize;
        public int includedEdges = 0;
        public List<int> entered;
        public List<int> exited;

        public static Node bssfNode = null;
        public static double bssfVal = 0;
        public static int pruneCount = 0;
        public static int bssfUpdate = 0;
        public static int numStatesCreated = 0;
        public static int numSolutions = 0;
        public static long timeElapsed = 0;
        public static int maxNodesCreated = 0;


        public void initStaticMembers()
        {
            bssfNode = null;
            bssfVal = 0;
            pruneCount = 0;
            bssfUpdate = 0;
            numStatesCreated = 0;
            numSolutions = 0;
            timeElapsed = 0;
            maxNodesCreated = 0;
        }


        /// <summary>
        /// Create a deep copy of a node (state).
        /// </summary>
        /// <param name="parent"></param>
        public Node(Node parent)
        {
            rcMatrix = new double[parent.matrixSize, parent.matrixSize];
            Array.Copy(parent.rcMatrix, rcMatrix, parent.rcMatrix.Length);
            entered = new List<int>(parent.entered);
            exited = new List<int>(parent.exited);
            lowerBound = parent.lowerBound;
            matrixSize = parent.matrixSize;
            includedEdges = parent.includedEdges;
        }


        /// <summary>
        /// Create an include or exclude node (state).
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="include"></param>
        /// <param name="exitCity"></param>
        /// <param name="enterCity"></param>
        public Node(Node parent, bool include, int exitCity, int enterCity)
        {            
            // get all values from prev
            rcMatrix = new double[parent.matrixSize, parent.matrixSize];
            Array.Copy(parent.rcMatrix, rcMatrix, parent.rcMatrix.Length);
            entered = new List<int>(parent.entered);
            exited = new List<int>(parent.exited);
            lowerBound = parent.lowerBound;
            matrixSize = parent.matrixSize;

            switch (include)
            {
                //---------------------------------------------------------------
                // Include
                //---------------------------------------------------------------
                case true:
                    // eliminate edges based upon entered and exited, rows = exited, cols = entered, single spot = swap exit and enter
                    for (int i = 0; i < matrixSize; i++)
                    {
                        rcMatrix[exitCity, i] = double.PositiveInfinity;
                        rcMatrix[i, enterCity] = double.PositiveInfinity;
                    }

                    rcMatrix[enterCity, exitCity] = double.PositiveInfinity;
                    includedEdges = parent.includedEdges + 1;

                    if ((includedEdges == matrixSize - 1) || (includedEdges == matrixSize))
                    {
                        // update entered and exited arrays
                        entered[enterCity] = exitCity;
                        exited[exitCity] = enterCity;
                    }
                    else
                    {
                        // check for premature cycles
                        deleteEdges(exitCity, enterCity);
                    }
                    break;

                //---------------------------------------------------------------
                // Exclude
                //---------------------------------------------------------------
                case false:
                    rcMatrix[exitCity, enterCity] = double.PositiveInfinity;
                    includedEdges = parent.includedEdges;
                    break;
            }

            reduceMatrix();
            //matrix2Table();
        }


        /// <summary>
        /// Create the initial node (state) from the list of cities.
        /// </summary>
        /// <param name="Cities"></param>
        public Node(City[] Cities)
        {
            matrixSize = Cities.Length;
            entered = new List<int>(matrixSize);
            exited = new List<int>(matrixSize);
            rcMatrix = new double[matrixSize, matrixSize];

            for (int i = 0; i < matrixSize; i++)
            {
                entered.Add(-1);
                exited.Add(-1);
            }

            for (int row = 0; row < matrixSize; row++)
            {
                for (int col = 0; col < matrixSize; col++)
                {
                    rcMatrix[row, col] = Cities[row].costToGetTo(Cities[col]);
                }
            }

            for (int i = 0; i < matrixSize; i++)
            {
                rcMatrix[i, i] = double.PositiveInfinity;
            }

            reduceMatrix();
            //matrix2Table();
        }


        public void reduceMatrix()
        {
            double lowest = double.PositiveInfinity;

            for (int row = 0; row < matrixSize; row++)
            {
                if (canReduceRow(row))
                {
                    lowest = getLowestCostInRow(row);

                    for (int col = 0; col < matrixSize; col++)
                    {
                        rcMatrix[row, col] -= lowest;
                    }

                    lowerBound += lowest;
                }
            }

            for (int col = 0; col < matrixSize; col++)
            {                
                if (canReduceCol(col))
                {
                    lowest = getLowestCostInCol(col);

                    for (int row = 0; row < matrixSize; row++)
                    {
                        rcMatrix[row, col] -= lowest;
                    }

                    lowerBound += lowest;
                }
            }
        }        


        public bool canReduceRow(int row)
        {
            int infinityCount = 0;

            for (int i = 0; i < matrixSize; i++)
            {
                if (rcMatrix[row, i] == 0)
                {
                    return false;
                }

                if (double.IsPositiveInfinity(rcMatrix[row, i]))
                {
                    infinityCount++;
                }                
            }

            if (infinityCount == matrixSize)
            {
                return false;
            }
            else
            {
                return true;
            }            
        }        


        public bool canReduceCol(int col)
        {
            int infinityCount = 0;

            for (int i = 0; i < matrixSize; i++)
            {
                if (rcMatrix[i, col] == 0)
                {
                    return false;
                }

                if (double.IsPositiveInfinity(rcMatrix[i, col]))
                {
                    infinityCount++;
                } 
            }

            if (infinityCount == matrixSize)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        public double getLowestCostInRow(int row)
        {
            double lowest = double.PositiveInfinity;

            for (int i = 0; i < matrixSize; i++)
            {
                if (rcMatrix[row, i] < lowest)
                {
                    lowest = rcMatrix[row, i];
                }
            }

            return lowest;
        }


        public double getLowestCostInCol(int col)
        {
            double lowest = double.PositiveInfinity;

            for (int i = 0; i < matrixSize; i++)
            {
                if (rcMatrix[i, col] < lowest)
                {
                    lowest = rcMatrix[i, col];
                }
            }

            return lowest;
        }


        public void deleteEdges(int exit, int enter)
        {
            entered[enter] = exit;
            exited[exit] = enter;

            int start = exit;
            int end = enter;
            int partialPathLength = 0;

            // The new edge may be part of a partial solution. Go to the end of that solution.
            while (exited[end] != -1)
            {
                end = exited[end];
                partialPathLength++;
            }
            
            // Similarly, go to the start of the new partial solution.
            while (entered[start] != -1)
            {
                start = entered[start];
                partialPathLength++;
            }

            // Delete the edges that would make partial cycles, unless we’re ready to finish the tour
            if (partialPathLength < matrixSize - 1)
            {
                while (start != enter)
                {
                    rcMatrix[end, start] = double.PositiveInfinity;
                    rcMatrix[enter, start] = double.PositiveInfinity;
                    start = exited[start];
                }
            }
        }
        

        /// <summary>
        /// Creates a DataTable from the matrix.  Useful for visualizing the matrix and debugging.
        /// </summary>
        public void matrix2Table() 
        {
            DataTable matrixTable = new DataTable();
                        
            for (int col = 0; col < matrixSize; col++)
            {
                DataColumn tablecol = new DataColumn();
                tablecol.ColumnName = col.ToString();
                matrixTable.Columns.Add(tablecol);
            }

            for (int row = 0; row < matrixSize; row++)
            {
                DataRow tablerow = matrixTable.NewRow();

                for (int col = 0; col < matrixSize; col++)
                {                   
                    tablerow[col] = rcMatrix[row, col];
                }

                matrixTable.Rows.Add(tablerow);
            }
        }


        

    }
}
