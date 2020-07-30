using System;
using System.Collections.Generic;

namespace Beam
{
    /// <summary>
    /// tree node sort algorithm base class
    /// </summary>
    public abstract class AABBTreeNodeSortBase
    {
        protected bool reverse = false;
        protected int numNodesLeaf = 0;

        public void Sort(List<AABBTreeNode> nodes, int numNodesLeaf)
        {
            var numNodes = nodes.Count;
            this.reverse = false;
            this.numNodesLeaf = numNodesLeaf;

            sortNodesRecursive(nodes, 0, numNodes, 0);
        }

        protected abstract void sortNodesRecursive(List<AABBTreeNode> nodes, int startIndex, int endIndex, int axis);

        double medianFn(double a, double b, double c)
        {
            if (a < b)
            {
                if (b < c)
                {
                    return b;
                }
                else if (a < c)
                {
                    return c;
                }
                else
                {
                    return a;
                }
            }
            else if (a < c)
            {
                return a;
            }
            else if (b < c)
            {
                return c;
            }
            return b;
        }

        void insertionSortFn(List<AABBTreeNode> nodes,
            int first,
            int last,
            Func<AABBTreeNode, double> getkey)
        {
            var sorted = (first + 1);
            while (sorted != last)
            {
                var tempNode = nodes[sorted];
                var tempKey = getkey(tempNode);

                var next = sorted;
                var current = (sorted - 1);

                while (next != first && tempKey < getkey(nodes[current]))
                {
                    nodes[next] = nodes[current];
                    next -= 1;
                    current -= 1;
                }

                if (next != sorted)
                {
                    nodes[next] = tempNode;
                }

                sorted += 1;
            }
        }

        protected void nthElement(List<AABBTreeNode> nodes, int first, int nth, int last, Func<AABBTreeNode, double> getkey)
        {
            while ((last - first) > 8)
            {
                var midValue = medianFn(getkey(nodes[first]),
                    getkey(nodes[first + ((last - first) >> 1)]),
                    getkey(nodes[last - 1]));

                var firstPos = first;
                var lastPos = last;
                int midPos;
                for (; ; firstPos += 1)
                {
                    while (getkey(nodes[firstPos]) < midValue)
                    {
                        firstPos += 1;
                    }

                    do
                    {
                        lastPos -= 1;
                    }
                    while (midValue < getkey(nodes[lastPos]));

                    if (firstPos >= lastPos)
                    {
                        midPos = firstPos;
                        break;
                    }
                    else
                    {
                        var temp = nodes[firstPos];
                        nodes[firstPos] = nodes[lastPos];
                        nodes[lastPos] = temp;
                    }
                }

                if (midPos <= nth)
                {
                    first = midPos;
                }
                else
                {
                    last = midPos;
                }
            }

            insertionSortFn(nodes, first, last, getkey);
        }
    }
}
