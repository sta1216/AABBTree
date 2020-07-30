using System;
using System.Collections.Generic;

namespace Beam
{
    class AABBTreeNodeSortHighQuality : AABBTreeNodeSortBase
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

        double getkeyXZfn(AABBTreeNode node)
        {
            var extents = node.extents;
            return (extents[0] + extents[2] + extents[3] + extents[5]);
        }

        double getkeyZXfn(AABBTreeNode node)
        {
            var extents = node.extents;
            return (extents[0] - extents[2] + extents[3] - extents[5]);
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

        double getreversekeyXZfn(AABBTreeNode node)
        {
            var extents = node.extents;
            return -(extents[0] + extents[2] + extents[3] + extents[5]);
        }

        double getreversekeyZXfn(AABBTreeNode node)
        {
            var extents = node.extents;
            return -(extents[0] - extents[2] + extents[3] - extents[5]);
        }

        protected override void sortNodesRecursive(List<AABBTreeNode> nodes, int startIndex, int endIndex, int axis)
        {
            var splitNodeIndex = ((startIndex + endIndex) >> 1);

            nthElement(nodes, startIndex, splitNodeIndex, endIndex, getkeyXfn);
            var sahX = (calculateSAH(nodes, startIndex, splitNodeIndex) +
                calculateSAH(nodes, splitNodeIndex, endIndex));

            nthElement(nodes, startIndex, splitNodeIndex, endIndex, getkeyYfn);
            var sahY = (calculateSAH(nodes, startIndex, splitNodeIndex) +
                calculateSAH(nodes, splitNodeIndex, endIndex));

            nthElement(nodes, startIndex, splitNodeIndex, endIndex, getkeyZfn);
            var sahZ = (calculateSAH(nodes, startIndex, splitNodeIndex) +
                calculateSAH(nodes, splitNodeIndex, endIndex));

            nthElement(nodes, startIndex, splitNodeIndex, endIndex, getkeyXZfn);
            var sahXZ = (calculateSAH(nodes, startIndex, splitNodeIndex) +
                calculateSAH(nodes, splitNodeIndex, endIndex));

            nthElement(nodes, startIndex, splitNodeIndex, endIndex, getkeyZXfn);
            var sahZX = (calculateSAH(nodes, startIndex, splitNodeIndex) +
                calculateSAH(nodes, splitNodeIndex, endIndex));

            if (sahX <= sahY &&
                sahX <= sahZ &&
                sahX <= sahXZ &&
                sahX <= sahZX)
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
            else if (sahZ <= sahY &&
                sahZ <= sahXZ &&
                sahZ <= sahZX)
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
            else if (sahY <= sahXZ &&
                sahY <= sahZX)
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
            else if (sahXZ <= sahZX)
            {
                if (reverse)
                {
                    nthElement(nodes, startIndex, splitNodeIndex, endIndex, getreversekeyXZfn);
                }
                else
                {
                    nthElement(nodes, startIndex, splitNodeIndex, endIndex, getkeyXZfn);
                }
            }
            else //if (sahZX <= sahXZ)
            {
                if (reverse)
                {
                    nthElement(nodes, startIndex, splitNodeIndex, endIndex, getreversekeyZXfn);
                }
                else
                {
                    nthElement(nodes, startIndex, splitNodeIndex, endIndex, getkeyZXfn);
                }
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

        double calculateSAH(List<AABBTreeNode> buildNodes, int startIndex, int endIndex)
        {
            AABBTreeNode buildNode;
            AABBBox extents;
            double minX, minY, minZ, maxX, maxY, maxZ;

            buildNode = buildNodes[startIndex];
            extents = buildNode.extents;
            minX = extents[0];
            minY = extents[1];
            minZ = extents[2];
            maxX = extents[3];
            maxY = extents[4];
            maxZ = extents[5];

            for (var n = (startIndex + 1); n < endIndex; n += 1)
            {
                buildNode = buildNodes[n];
                extents = buildNode.extents;
                /*jshint white: false*/
                if (minX > extents[0]) { minX = extents[0]; }
                if (minY > extents[1]) { minY = extents[1]; }
                if (minZ > extents[2]) { minZ = extents[2]; }
                if (maxX < extents[3]) { maxX = extents[3]; }
                if (maxY < extents[4]) { maxY = extents[4]; }
                if (maxZ < extents[5]) { maxZ = extents[5]; }
                /*jshint white: true*/
            }

            return ((maxX - minX) + (maxY - minY) + (maxZ - minZ));
        }
    }
}
