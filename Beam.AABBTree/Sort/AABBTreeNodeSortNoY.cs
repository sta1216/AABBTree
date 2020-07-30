using System;
using System.Collections.Generic;

namespace Beam
{
    class AABBTreeNodeSortNoY : AABBTreeNodeSortBase
    {
        double getkeyXfn(AABBTreeNode node)
        {
            var extents = node.extents;
            return (extents[0] + extents[3]);
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
            else //if (axis === 2)
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

            if (axis == 0)
            {
                axis = 2;
            }
            else //if (axis === 2)
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
