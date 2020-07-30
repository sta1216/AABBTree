using System;
using System.Collections.Generic;
using System.Text;

namespace Beam
{
    class AABBTreeNodeSort : AABBTreeNodeSortBase
    {
        double getkeyXfn(AABBTreeNode node)
        {
            var extents = node.extents;
            return (extents[0] + extents[3]);
        }

        double getkeyYfn(AABBTreeNode node)
        {
            var extents = node.extents;
            return (extents[1] + extents[4]);
        }

        double getkeyZfn(AABBTreeNode node)
        {
            var extents = node.extents;
            return (extents[2] + extents[5]);
        }

        double getreversekeyXfn(AABBTreeNode node)
        {
            var extents = node.extents;
            return -(extents[0] + extents[3]);
        }

        double getreversekeyYfn(AABBTreeNode node)
        {
            var extents = node.extents;
            return -(extents[1] + extents[4]);
        }

        double getreversekeyZfn(AABBTreeNode node)
        {
            var extents = node.extents;
            return -(extents[2] + extents[5]);
        }

       protected override void sortNodesRecursive(List<AABBTreeNode> nodes, int startIndex, int endIndex, int axis)
        {
            var splitNodeIndex = ((startIndex + endIndex) >> 1);

            if (axis == 0)
            {
                if (reverse)
                {
                    nthElement(nodes, startIndex, splitNodeIndex, endIndex, getreversekeyXfn);
                }
                else
                {
                    nthElement(nodes, startIndex, splitNodeIndex, endIndex, getkeyXfn);
                }
            }
            else if (axis == 2)
            {
                if (reverse)
                {
                    nthElement(nodes, startIndex, splitNodeIndex, endIndex, getreversekeyZfn);
                }
                else
                {
                    nthElement(nodes, startIndex, splitNodeIndex, endIndex, getkeyZfn);
                }
            }
            else //if (axis === 1)
            {
                if (reverse)
                {
                    nthElement(nodes, startIndex, splitNodeIndex, endIndex, getreversekeyYfn);
                }
                else
                {
                    nthElement(nodes, startIndex, splitNodeIndex, endIndex, getkeyYfn);
                }
            }

            if (axis == 0)
            {
                axis = 2;
            }
            else if (axis == 2)
            {
                axis = 1;
            }
            else //if (axis === 1)
            {
                axis = 0;
            }

            reverse = !reverse;

            if ((startIndex + numNodesLeaf) < splitNodeIndex)
            {
                sortNodesRecursive(nodes, startIndex, splitNodeIndex, axis);
            }

            if ((splitNodeIndex + numNodesLeaf) < endIndex)
            {
                sortNodesRecursive(nodes, splitNodeIndex, endIndex, axis);
            }
        }
    }
}
