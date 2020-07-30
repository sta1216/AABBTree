﻿using System;
using System.Collections.Generic;

namespace Beam
{
    class Program
    {
        static void Main(string[] args)
        {
            var tree = new AABBTree(true);
            tree.add(new AABBExternalNode() { Data = 1 }, new AABBBox(new Point3(0, 0, 0), new Point3(10, 10, 10)));
            tree.add(new AABBExternalNode() { Data = 2 }, new AABBBox(new Point3(5, 5, 5), new Point3(15, 15, 15)));
            tree.add(new AABBExternalNode() { Data = 3 }, new AABBBox(new Point3(20, 20, 20), new Point3(30, 30, 30)));
            tree.add(new AABBExternalNode() { Data = 4 }, new AABBBox(new Point3(40, 40, 40), new Point3(50, 50, 50)));
            var node = new AABBExternalNode() { Data = 6 };
            var box = new AABBBox(new Point3(50, 50, 50), new Point3(60, 60, 60));
            tree.add(node, box);
            var lastNode = new AABBExternalNode() { Data = 5 };
            tree.add(lastNode, new AABBBox(new Point3(50, 50, 50), new Point3(60, 60, 60)));

            tree.finalize();

            // remove 
            tree.remove(node);
            tree.finalize();

            // get parent
            var parent = tree.findParent(lastNode.spatialIndex);

            // get all the overlapping pairs
            List<AABBExternalNode> overlappingNodes = new List<AABBExternalNode>();
            var count = tree.getOverlappingPairs(overlappingNodes);
            Console.WriteLine($"Node Count:{count}");
            overlappingNodes.ForEach(node=>node.Print());
            overlappingNodes.Clear();

            // find overlapping nodes by bounding box
            var queryExtents = new AABBBox(new Point3(-40, -40, -40), new Point3(40, 40, 40));
            count = tree.getOverlappingNodes(queryExtents, overlappingNodes);
            Console.WriteLine($"Node Count:{count}");
            overlappingNodes.ForEach(node => node.Print());
            overlappingNodes.Clear();

            // find overlapping nodes by sphere
            count = tree.getSphereOverlappingNodes(new Point3(20, 20, 20), 21, overlappingNodes);
            Console.WriteLine($"Node Count:{count}");
            overlappingNodes.ForEach(node => node.Print());
            overlappingNodes.Clear();

            // find nodes by planes
            var normal = new Vector3(1, 1, 1);
            normal.normalize();
            var plane = new Plane3(normal.X, normal.Y, normal.Z, -20);
			normal.negative();
            var plane2 = new Plane3(normal.X, normal.Y, normal.Z, -60);
            count = tree.getVisibleNodes(new Plane3[] { plane, plane2 }, overlappingNodes);
            Console.WriteLine($"Node Count:{count}");
            overlappingNodes.ForEach(node => node.Print());
            overlappingNodes.Clear();

            // TODO: ray test
            var ray = new AABBTreeRay() { direction = new Vector3(1, 1, 1), origin = new Point3(), maxFactor = 30 };
            var testRes = new AABBTreeRayTest().rayTest(new AABBTree[] { tree }, ray);

            tree.clear();
        }
    }
}
