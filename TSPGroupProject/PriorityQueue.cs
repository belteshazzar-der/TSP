using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace TSP
{
    /// <summary>
    /// A min-heap implementation of a priority queue.
    /// </summary>
    class PriorityQueue
    {

        public List<Node> heap;


       /// <summary>
       /// 
       /// </summary>
       /// <param name="node"></param>
        public PriorityQueue(Node node)
        {
            heap = new List<Node>();
            Insert(node);
        }


        /// <summary>
        /// Rebalances the heap by comparing and bubbling values up until all child values are less than or equal to their parent value.
        /// </summary>
        /// <param name="childIndex">The index of the node in the heap to start with.</param>
        private void BubbleUp(int childIndex)
        {
            while (childIndex > 0)
            {
                int parentIndex = (childIndex - 1) / 2;

                if (heap[childIndex].lowerBound >= heap[parentIndex].lowerBound)
                {
                    break; // child item >= parent so we're done
                }

                Node tempNode = heap[childIndex];
                heap[childIndex] = heap[parentIndex];
                heap[parentIndex] = tempNode;

                childIndex = parentIndex;
            }
        }


        /// <summary>
        /// Insert a node into the heap, then rebalance the heap.
        /// </summary>
        /// <param name="node">The node to insert.</param>
        public void Insert(Node node)
        {
            heap.Add(node);
            BubbleUp(heap.Count - 1);
        }       


        /// <summary>
        /// Pop the top node from the heap, then rebalance the heap by sifting down.
        /// </summary>
        /// <returns>A node with the highest priority (lowest edge cost).</returns>
        public Node DeleteMinimum()
        {
            // size of queue cannot be 0
            int parentIndex = 0;                                            // start at front of the queue (top of the heap)
            int lastIndex = heap.Count - 1;                                 // last index (before removal)
            Node minItem = heap[parentIndex];                               // retrieve the minimum node (highest priority node)
            heap[parentIndex] = heap[lastIndex];                            // replace the minimum node (top of the heap) with the last
            heap.RemoveAt(lastIndex);                                       // remove the last node (copied to the front)
            lastIndex--;                                                    // last index (after removal)

            // balance the heap by settling the top value down
            while (true)
            {
                int childIndex = 2 * parentIndex + 1; // left child index of parent

                if (childIndex > lastIndex)
                {
                    break;  // no children, so done
                }

                int rightChild = childIndex + 1;     // right child

                //if there is a right child (childIndex + 1) and it is smaller than left child, use the right child instead
                if ((rightChild <= lastIndex) && (heap[rightChild].lowerBound < heap[childIndex].lowerBound))
                {
                    childIndex = rightChild;
                }

                if (heap[parentIndex].lowerBound <= heap[childIndex].lowerBound)
                {
                    break; // parent is <= smallest child, so done
                }

                Node tempNode = heap[parentIndex];
                heap[parentIndex] = heap[childIndex];
                heap[childIndex] = tempNode;

                parentIndex = childIndex;
            }

            // now that rebalance of the heap is done, return the minimum item (original top of the heap)
            return minItem;
        }       


        /// <summary>
        /// Looks at the value (lower bound) of the node at the top of the heap.
        /// </summary>
        /// <returns></returns>
        public double Peek()
        {            
            return heap[0].lowerBound;
        }


        /// <summary>
        /// Returns the size of the priority queue (heap).
        /// </summary>
        /// <returns></returns>
        public int Size()
        {
            return heap.Count;
        }


    }
}
