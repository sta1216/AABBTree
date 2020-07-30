using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Beam
{

    //
    // AABBTree
    //
    public class AABBTree
    {
        int numNodesLeaf = 4;
        List<AABBTreeNode> nodes = new List<AABBTreeNode>();
        bool needsRebuild;
        bool needsRebound;
        int numAdds;
        int numUpdates;
        int numExternalNodes;
        int startUpdate;
        int endUpdate;
        bool highQuality;
        bool ignoreY;
        int[] nodesStack;

        public AABBTree(bool highQuality)
        {
            this.nodes = new List<AABBTreeNode>();
            this.needsRebuild = false;
            this.needsRebound = false;
            this.numAdds = 0;
            this.numUpdates = 0;
            this.numExternalNodes = 0;
            this.startUpdate = 0x7FFFFFFF;
            this.endUpdate = -0x7FFFFFFF;
            this.highQuality = highQuality;
            this.ignoreY = false;
            this.nodesStack = new int[32];
        }

        public void add(AABBExternalNode externalNode, AABBBox extents)
        {
            var endNode = this.getEndNodeIndex();
            externalNode.spatialIndex = endNode - 1;

            var node = new AABBTreeNode(new AABBBox(), 1, null);
            node.escapeNodeOffset = 1;
            node.externalNode = externalNode;
            var copyExtents = node.extents;
            copyExtents[0] = extents[0];
            copyExtents[1] = extents[1];
            copyExtents[2] = extents[2];
            copyExtents[3] = extents[3];
            copyExtents[4] = extents[4];
            copyExtents[5] = extents[5];

            this.nodes.Add(node);
            this.needsRebuild = true;
            this.numAdds += 1;
            this.numExternalNodes += 1;
        }

        public void remove(AABBExternalNode externalNode)
        {
            var index = externalNode.spatialIndex;
            if (index >= 0)
            {
                if (this.numExternalNodes > 1)
                {
                    var nodes = this.nodes;
                    Debug.Assert(nodes[index].externalNode == externalNode);
                    nodes[index].clear();

                    var endNode = this.getEndNodeIndex();
                    if ((index + 1) >= endNode)
                    {
                        while (nodes[endNode - 1].externalNode == null) // No leaf
                        {
                            endNode -= 1;
                        }

                        if(this.nodes.Count > endNode)
                        {
                            this.nodes.RemoveRange(endNode, this.nodes.Count - endNode);
                        }
                    }
                    else
                    {
                        this.needsRebuild = true;
                    }
                    this.numExternalNodes -= 1;
                }
                else
                {
                    this.clear();
                }

                externalNode.spatialIndex = -1;
            }
        }

        public AABBTreeNode findParent(int nodeIndex)
        {
            var nodes = this.nodes;
            var parentIndex = nodeIndex;
            int nodeDist = 0;
            AABBTreeNode parent;
            do
            {
                parentIndex -= 1;
                nodeDist += 1;
                parent = nodes[parentIndex];
            }
            while (parent.escapeNodeOffset <= nodeDist);
            return parent;
        }

        public void update(AABBExternalNode externalNode, AABBBox extents)
        {
            var index = externalNode.spatialIndex;
            if (index >= 0)
            {
                var min0 = extents[0];
                var min1 = extents[1];
                var min2 = extents[2];
                var max0 = extents[3];
                var max1 = extents[4];
                var max2 = extents[5];

                var needsRebuild = this.needsRebuild;
                var needsRebound = this.needsRebound;
                var nodes = this.nodes;
                var node = nodes[index];
                Debug.Assert(node.externalNode == externalNode);
                var nodeExtents = node.extents;

                var doUpdate = (needsRebuild ||
                                needsRebound ||
                                nodeExtents[0] > min0 ||
                                nodeExtents[1] > min1 ||
                                nodeExtents[2] > min2 ||
                                nodeExtents[3] < max0 ||
                                nodeExtents[4] < max1 ||
                                nodeExtents[5] < max2);

                nodeExtents[0] = min0;
                nodeExtents[1] = min1;
                nodeExtents[2] = min2;
                nodeExtents[3] = max0;
                nodeExtents[4] = max1;
                nodeExtents[5] = max2;

                if (doUpdate)
                {
                    if (!needsRebuild && 1 < nodes.Count)
                    {
                        this.numUpdates += 1;
                        if (this.startUpdate > index)
                        {
                            this.startUpdate = index;
                            if (this.endUpdate < index)
                            {
                                this.endUpdate = index;
                                if (!needsRebound)
                                {
                                    // force a rebound when things change too much
                                    if ((2 * this.numUpdates) > this.numExternalNodes)
                                    {
                                        this.needsRebound = true;
                                    }
                                    else
                                    {
                                        var parent = this.findParent(index);
                                        var parentExtents = parent.extents;
                                        if (parentExtents[0] > min0 ||
                                            parentExtents[1] > min1 ||
                                            parentExtents[2] > min2 ||
                                            parentExtents[3] < max0 ||
                                            parentExtents[4] < max1 ||
                                            parentExtents[5] < max2)
                                        {
                                            this.needsRebound = true;
                                        }
                                    }
                                }
                                else
                                {
                                    // force a rebuild when things change too much
                                    if (this.numUpdates > (3 * this.numExternalNodes))
                                    {
                                        this.needsRebuild = true;
                                        this.numAdds = this.numUpdates;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        this.add(externalNode, extents);
                    }
                }
            }
        }

        public bool needsFinalize()
        {
            return (this.needsRebuild || this.needsRebound);
        }

        public void finalize()
        {
            if (this.needsRebuild)
            {
                this.rebuild();
            }
            else if (this.needsRebound)
            {
                this.rebound();
            }
        }

        private void rebound()
        {
            var nodes = this.nodes;
            if (nodes.Count > 1)
            {
                var startUpdateNodeIndex = this.startUpdate;
                var endUpdateNodeIndex = this.endUpdate;

                var nodesStack = this.nodesStack;
                var numNodesStack = 0;
                var topNodeIndex = 0;
                for (; ; )
                {
                    var topNode = nodes[topNodeIndex];
                    var currentNodeIndex = topNodeIndex;
                    var currentEscapeNodeIndex = (topNodeIndex + topNode.escapeNodeOffset);
                    var nodeIndex = (topNodeIndex + 1); // First child
                    AABBTreeNode node;

                    do
                    {
                        node = nodes[nodeIndex];
                        var escapeNodeIndex = (nodeIndex + node.escapeNodeOffset);
                        if (nodeIndex < endUpdateNodeIndex)
                        {
                            if (node.externalNode == null) // No leaf
                            {
                                if (escapeNodeIndex > startUpdateNodeIndex)
                                {
                                    nodesStack[numNodesStack] = topNodeIndex;
                                    numNodesStack += 1;
                                    topNodeIndex = nodeIndex;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                        nodeIndex = escapeNodeIndex;
                    }
                    while (nodeIndex < currentEscapeNodeIndex);

                    if (topNodeIndex == currentNodeIndex)
                    {
                        nodeIndex = (topNodeIndex + 1); // First child
                        node = nodes[nodeIndex];

                        var extents = node.extents;
                        var minX = extents[0];
                        var minY = extents[1];
                        var minZ = extents[2];
                        var maxX = extents[3];
                        var maxY = extents[4];
                        var maxZ = extents[5];

                        nodeIndex = (nodeIndex + node.escapeNodeOffset);
                        while (nodeIndex < currentEscapeNodeIndex)
                        {
                            node = nodes[nodeIndex];
                            extents = node.extents;
                            /*jshint white: false*/
                            if (minX > extents[0]) { minX = extents[0]; }
                            if (minY > extents[1]) { minY = extents[1]; }
                            if (minZ > extents[2]) { minZ = extents[2]; }
                            if (maxX < extents[3]) { maxX = extents[3]; }
                            if (maxY < extents[4]) { maxY = extents[4]; }
                            if (maxZ < extents[5]) { maxZ = extents[5]; }
                            /*jshint white: true*/
                            nodeIndex = (nodeIndex + node.escapeNodeOffset);
                        }

                        extents = topNode.extents;
                        extents[0] = minX;
                        extents[1] = minY;
                        extents[2] = minZ;
                        extents[3] = maxX;
                        extents[4] = maxY;
                        extents[5] = maxZ;

                        endUpdateNodeIndex = topNodeIndex;

                        if (0 < numNodesStack)
                        {
                            numNodesStack -= 1;
                            topNodeIndex = nodesStack[numNodesStack];
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            this.needsRebuild = false;
            this.needsRebound = false;
            this.numAdds = 0;
            //this.numUpdates = 0;
            this.startUpdate = 0x7FFFFFFF;
            this.endUpdate = -0x7FFFFFFF;
        }


        private void rebuild()
        {
            if (this.numExternalNodes > 0)
            {
                var nodes = this.nodes;
                List<AABBTreeNode> buildNodes;
                int endNodeIndex;

                if (this.numExternalNodes == nodes.Count)
                {
                    buildNodes = nodes;
                    nodes = new List<AABBTreeNode>(nodes.Count);
                    this.nodes = nodes;
                }
                else
                {
                    buildNodes = new List<AABBTreeNode>(this.numExternalNodes);
                    endNodeIndex = this.getEndNodeIndex();
                    for (int n = 0; n < endNodeIndex; n += 1)
                    {
                        var currentNode = nodes[n];
                        if (currentNode.externalNode != null) // Is leaf
                        {
                            buildNodes.Add(currentNode);
                        }
                    }
                }

                AABBTreeNode rootNode;
                if (buildNodes.Count > 1)
                {
                    if (buildNodes.Count > this.numNodesLeaf && this.numAdds > 0)
                    {
                        if (this.highQuality)
                        {
                            new AABBTreeNodeSortHighQuality().Sort(buildNodes, this.numNodesLeaf);
                        }
                        else if (this.ignoreY)
                        {
                            new AABBTreeNodeSortNoY().Sort(buildNodes, this.numNodesLeaf);
                        }
                        else
                        {
                            new AABBTreeNodeSort().Sort(buildNodes, this.numNodesLeaf);
                        }
                    }

                    var predictedNumNodes = this._predictNumNodes(0, buildNodes.Count, 0);
                    this.nodes.Clear();
                    for (int i = 0; i < predictedNumNodes; i++)
                    {
                        this.nodes.Add(null);
                    }

                    this._recursiveBuild(buildNodes, 0, buildNodes.Count, 0);

                    endNodeIndex = nodes[0].escapeNodeOffset;
                    if (nodes.Count > endNodeIndex)
                    {
                        nodes.RemoveRange(endNodeIndex, nodes.Count - endNodeIndex);
                    }

                    // Check if we should take into account the Y coordinate
                    rootNode = nodes[0];
                    var extents = rootNode.extents;
                    var deltaX = (extents[3] - extents[0]);
                    var deltaY = (extents[4] - extents[1]);
                    var deltaZ = (extents[5] - extents[2]);
                    this.ignoreY = ((4 * deltaY) < (deltaX <= deltaZ ? deltaX : deltaZ));
                }
                else
                {
                    rootNode = buildNodes[0];
                    rootNode.externalNode.spatialIndex = 0;
                    this.nodes = new List<AABBTreeNode>() { rootNode };
                }
            }

            this.needsRebuild = false;
            this.needsRebound = false;
            this.numAdds = 0;
            this.numUpdates = 0;
            this.startUpdate = 0x7FFFFFFF;
            this.endUpdate = -0x7FFFFFFF;
        }

        private void _recursiveBuild(List<AABBTreeNode> buildNodes, int startIndex, int endIndex, int lastNodeIndex)
        {
            var nodes = this.nodes;
            var nodeIndex = lastNodeIndex;
            lastNodeIndex += 1;

            double minX, minY, minZ, maxX, maxY, maxZ;
            AABBBox extents;
            AABBTreeNode buildNode, lastNode;

            if ((startIndex + this.numNodesLeaf) >= endIndex)
            {
                buildNode = buildNodes[startIndex];
                extents = buildNode.extents;
                minX = extents[0];
                minY = extents[1];
                minZ = extents[2];
                maxX = extents[3];
                maxY = extents[4];
                maxZ = extents[5];

                buildNode.externalNode.spatialIndex = lastNodeIndex;
                this._replaceNode(nodes, lastNodeIndex, buildNode);

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
                    lastNodeIndex += 1;
                    buildNode.externalNode.spatialIndex = lastNodeIndex;
                    this._replaceNode(nodes, lastNodeIndex, buildNode);
                }

                lastNode = nodes[lastNodeIndex];
            }
            else
            {
                /* tslint:disable:no-bitwise */
                var splitPosIndex = ((startIndex + endIndex) >> 1);
                /* tslint:enable:no-bitwise */

                if ((startIndex + 1) >= splitPosIndex)
                {
                    buildNode = buildNodes[startIndex];
                    buildNode.externalNode.spatialIndex = lastNodeIndex;
                    this._replaceNode(nodes, lastNodeIndex, buildNode);
                }
                else
                {
                    this._recursiveBuild(buildNodes, startIndex, splitPosIndex, lastNodeIndex);
                }

                lastNode = nodes[lastNodeIndex];
                extents = lastNode.extents;
                minX = extents[0];
                minY = extents[1];
                minZ = extents[2];
                maxX = extents[3];
                maxY = extents[4];
                maxZ = extents[5];

                lastNodeIndex = (lastNodeIndex + lastNode.escapeNodeOffset);

                if ((splitPosIndex + 1) >= endIndex)
                {
                    buildNode = buildNodes[splitPosIndex];
                    buildNode.externalNode.spatialIndex = lastNodeIndex;
                    this._replaceNode(nodes, lastNodeIndex, buildNode);
                }
                else
                {
                    this._recursiveBuild(buildNodes, splitPosIndex, endIndex, lastNodeIndex);
                }

                lastNode = nodes[lastNodeIndex];
                extents = lastNode.extents;
                /*jshint white: false*/
                if (minX > extents[0]) { minX = extents[0]; }
                if (minY > extents[1]) { minY = extents[1]; }
                if (minZ > extents[2]) { minZ = extents[2]; }
                if (maxX < extents[3]) { maxX = extents[3]; }
                if (maxY < extents[4]) { maxY = extents[4]; }
                if (maxZ < extents[5]) { maxZ = extents[5]; }
                /*jshint white: true*/
            }

            var node = nodes[nodeIndex];
            if (node == null)
            {
                nodes[nodeIndex] = node = new AABBTreeNode(new AABBBox(), 1, null);
            }
            node.reset(minX, minY, minZ, maxX, maxY, maxZ,
                        (lastNodeIndex + lastNode.escapeNodeOffset - nodeIndex));
        }

        private void _replaceNode(List<AABBTreeNode> nodes, int nodeIndex, AABBTreeNode newNode)
        {
            nodes[nodeIndex] = newNode;
        }

        private int _predictNumNodes(int startIndex, int endIndex, int lastNodeIndex)
        {
            lastNodeIndex += 1;

            if ((startIndex + this.numNodesLeaf) >= endIndex)
            {
                lastNodeIndex += (endIndex - startIndex);
            }
            else
            {
                var splitPosIndex = ((startIndex + endIndex) >> 1);

                if ((startIndex + 1) >= splitPosIndex)
                {
                    lastNodeIndex += 1;
                }
                else
                {
                    lastNodeIndex = this._predictNumNodes(startIndex, splitPosIndex, lastNodeIndex);
                }

                if ((splitPosIndex + 1) >= endIndex)
                {
                    lastNodeIndex += 1;
                }
                else
                {
                    lastNodeIndex = this._predictNumNodes(splitPosIndex, endIndex, lastNodeIndex);
                }
            }

            return lastNodeIndex;
        }

        public int getVisibleNodes(Plane3[] planes, List<AABBExternalNode> visibleNodes)
        {
            var numVisibleNodes = 0;
            if (this.numExternalNodes > 0)
            {
                var nodes = this.nodes;
                var endNodeIndex = this.getEndNodeIndex();
                var numPlanes = planes.Length;
                AABBTreeNode node;
                AABBBox extents;
                int endChildren;
                double n0, n1, n2, p0, p1, p2;
                bool isInside;
                int n;
                Plane3 plane;
                double d0, d1, d2, distance;
                var nodeIndex = 0;

                for (; ; )
                {
                    node = nodes[nodeIndex];
                    extents = node.extents;
                    n0 = extents[0];
                    n1 = extents[1];
                    n2 = extents[2];
                    p0 = extents[3];
                    p1 = extents[4];
                    p2 = extents[5];
                    //isInsidePlanesAABB
                    isInside = true;
                    n = 0;
                    do
                    {
                        plane = planes[n];
                        d0 = plane[0];
                        d1 = plane[1];
                        d2 = plane[2];
                        distance = (d0 * (d0 < 0 ? n0 : p0) + d1 * (d1 < 0 ? n1 : p1) + d2 * (d2 < 0 ? n2 : p2));
                        if (distance < plane[3])
                        {
                            isInside = false;
                            break;
                        }
                        n += 1;
                    }
                    while (n < numPlanes);
                    if (isInside)
                    {
                        if (node.externalNode != null) // Is leaf
                        {
                            visibleNodes.Add(node.externalNode);
                            numVisibleNodes += 1;
                            nodeIndex += 1;
                            if (nodeIndex >= endNodeIndex)
                            {
                                break;
                            }
                        }
                        else
                        {
                            //isFullyInsidePlanesAABB
                            isInside = true;
                            n = 0;
                            do
                            {
                                plane = planes[n];
                                d0 = plane[0];
                                d1 = plane[1];
                                d2 = plane[2];
                                distance = (d0 * (d0 > 0 ? n0 : p0) + d1 * (d1 > 0 ? n1 : p1) + d2 * (d2 > 0 ? n2 : p2));
                                if (distance < plane[3])
                                {
                                    isInside = false;
                                    break;
                                }
                                n += 1;
                            }
                            while (n < numPlanes);
                            if (isInside)
                            {
                                endChildren = (nodeIndex + node.escapeNodeOffset);
                                nodeIndex += 1;
                                do
                                {
                                    node = nodes[nodeIndex];
                                    if (node.externalNode != null) // Is leaf
                                    {
                                        visibleNodes.Add(node.externalNode);
                                        numVisibleNodes += 1;
                                    }
                                    nodeIndex += 1;
                                }
                                while (nodeIndex < endChildren);
                                if (nodeIndex >= endNodeIndex)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                nodeIndex += 1;
                            }
                        }
                    }
                    else
                    {
                        nodeIndex += node.escapeNodeOffset;
                        if (nodeIndex >= endNodeIndex)
                        {
                            break;
                        }
                    }
                }
            }
            return numVisibleNodes;
        }

        public int getOverlappingNodes(AABBBox queryExtents, List<AABBExternalNode> overlappingNodes)
        {
            if (this.numExternalNodes > 0)
            {
                var queryMinX = queryExtents[0];
                var queryMinY = queryExtents[1];
                var queryMinZ = queryExtents[2];
                var queryMaxX = queryExtents[3];
                var queryMaxY = queryExtents[4];
                var queryMaxZ = queryExtents[5];
                var nodes = this.nodes;
                var endNodeIndex = this.getEndNodeIndex();
                AABBTreeNode node;
                AABBBox extents;
                int endChildren;
                var numOverlappingNodes = 0;
                var nodeIndex = 0;
                for (; ; )
                {
                    node = nodes[nodeIndex];
                    extents = node.extents;
                    var minX = extents[0];
                    var minY = extents[1];
                    var minZ = extents[2];
                    var maxX = extents[3];
                    var maxY = extents[4];
                    var maxZ = extents[5];
                    if (queryMinX <= maxX &&
                        queryMinY <= maxY &&
                        queryMinZ <= maxZ &&
                        queryMaxX >= minX &&
                        queryMaxY >= minY &&
                        queryMaxZ >= minZ)
                    {
                        if (node.externalNode != null) // Is leaf
                        {
                            overlappingNodes.Add(node.externalNode);
                            numOverlappingNodes += 1;
                            nodeIndex += 1;
                            if (nodeIndex >= endNodeIndex)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (queryMaxX >= maxX &&
                                queryMaxY >= maxY &&
                                queryMaxZ >= maxZ &&
                                queryMinX <= minX &&
                                queryMinY <= minY &&
                                queryMinZ <= minZ)
                            {
                                endChildren = (nodeIndex + node.escapeNodeOffset);
                                nodeIndex += 1;
                                do
                                {
                                    node = nodes[nodeIndex];
                                    if (node.externalNode != null) // Is leaf
                                    {
                                        overlappingNodes.Add(node.externalNode);
                                        numOverlappingNodes += 1;
                                    }
                                    nodeIndex += 1;
                                }
                                while (nodeIndex < endChildren);
                                if (nodeIndex >= endNodeIndex)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                nodeIndex += 1;
                            }
                        }
                    }
                    else
                    {
                        nodeIndex += node.escapeNodeOffset;
                        if (nodeIndex >= endNodeIndex)
                        {
                            break;
                        }
                    }
                }
                return numOverlappingNodes;
            }
            else
            {
                return 0;
            }
        }

        public int getSphereOverlappingNodes(Point3 center, double radius, List<AABBExternalNode> overlappingNodes)
        {
            if (this.numExternalNodes > 0)
            {
                var radiusSquared = (radius * radius);
                var centerX = center[0];
                var centerY = center[1];
                var centerZ = center[2];
                var nodes = this.nodes;
                var endNodeIndex = this.getEndNodeIndex();
                AABBTreeNode node;
                AABBBox extents;
                var numOverlappingNodes = 0;
                var nodeIndex = 0;
                for (; ; )
                {
                    node = nodes[nodeIndex];
                    extents = node.extents;
                    var minX = extents[0];
                    var minY = extents[1];
                    var minZ = extents[2];
                    var maxX = extents[3];
                    var maxY = extents[4];
                    var maxZ = extents[5];
                    double totalDistance = 0;
                    double sideDistance;
                    if (centerX < minX)
                    {
                        sideDistance = (minX - centerX);
                        totalDistance += (sideDistance * sideDistance);
                    }
                    else if (centerX > maxX)
                    {
                        sideDistance = (centerX - maxX);
                        totalDistance += (sideDistance * sideDistance);
                    }
                    if (centerY < minY)
                    {
                        sideDistance = (minY - centerY);
                        totalDistance += (sideDistance * sideDistance);
                    }
                    else if (centerY > maxY)
                    {
                        sideDistance = (centerY - maxY);
                        totalDistance += (sideDistance * sideDistance);
                    }
                    if (centerZ < minZ)
                    {
                        sideDistance = (minZ - centerZ);
                        totalDistance += (sideDistance * sideDistance);
                    }
                    else if (centerZ > maxZ)
                    {
                        sideDistance = (centerZ - maxZ);
                        totalDistance += (sideDistance * sideDistance);
                    }
                    if (totalDistance <= radiusSquared)
                    {
                        nodeIndex += 1;
                        if (node.externalNode != null) // Is leaf
                        {
                            overlappingNodes.Add(node.externalNode);
                            numOverlappingNodes++;
                            if (nodeIndex >= endNodeIndex)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        nodeIndex += node.escapeNodeOffset;
                        if (nodeIndex >= endNodeIndex)
                        {
                            break;
                        }
                    }
                }

                return numOverlappingNodes;
            }

            return 0;
        }

        public int getOverlappingPairs(List<AABBExternalNode> overlappingPairs)
        {
            if (this.numExternalNodes > 0)
            {
                var nodes = this.nodes;
                var endNodeIndex = this.getEndNodeIndex();
                AABBTreeNode currentNode;
                AABBExternalNode currentExternalNode;
                AABBTreeNode node;
                AABBBox extents;
                var numInsertions = 0;
                int currentNodeIndex = 0;
                int nodeIndex;
                for (; ; )
                {
                    currentNode = nodes[currentNodeIndex];
                    while (currentNode.externalNode == null) // No leaf
                    {
                        currentNodeIndex += 1;
                        currentNode = nodes[currentNodeIndex];
                    }

                    currentNodeIndex += 1;
                    if (currentNodeIndex < endNodeIndex)
                    {
                        currentExternalNode = currentNode.externalNode;
                        extents = currentNode.extents;
                        var minX = extents[0];
                        var minY = extents[1];
                        var minZ = extents[2];
                        var maxX = extents[3];
                        var maxY = extents[4];
                        var maxZ = extents[5];

                        nodeIndex = currentNodeIndex;
                        for (; ; )
                        {
                            node = nodes[nodeIndex];
                            extents = node.extents;
                            if (minX <= extents[3] &&
                                minY <= extents[4] &&
                                minZ <= extents[5] &&
                                maxX >= extents[0] &&
                                maxY >= extents[1] &&
                                maxZ >= extents[2])
                            {
                                nodeIndex += 1;
                                if (node.externalNode != null) // Is leaf
                                {
                                    overlappingPairs.Add(currentExternalNode);
                                    overlappingPairs.Add(node.externalNode);
                                    numInsertions += 2;
                                    if (nodeIndex >= endNodeIndex)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                nodeIndex += node.escapeNodeOffset;
                                if (nodeIndex >= endNodeIndex)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                return numInsertions;
            }
            else
            {
                return 0;
            }
        }

        public AABBBox getExtents()
        {
            return (0 < this.nodes.Count ? this.nodes[0].extents : null);
        }

        public AABBTreeNode getRootNode()
        {
            return this.nodes[0];
        }

        public IList<AABBTreeNode> getNodes()
        {
            return this.nodes;
        }

        public int getEndNodeIndex()
        {
            return this.nodes.Count;
        }

        public void clear()
        {
            this.nodes.Clear();
            this.needsRebuild = false;
            this.needsRebound = false;
            this.numAdds = 0;
            this.numUpdates = 0;
            this.numExternalNodes = 0;
            this.startUpdate = 0x7FFFFFFF;
            this.endUpdate = -0x7FFFFFFF;
            this.ignoreY = false;
        }
    }
}
