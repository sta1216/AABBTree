using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Beam
{
    public class AABBTreeRayTestResult
    {
        public Point3 hitPoint;
        public Vector3 hitNormal;
        public double factor;
        public dynamic collisionObject; //?: PhysicsCollisionObject;
        public dynamic body; //?: PhysicsRigidBody;
    };

    public class AABBTreeRay
    {
        public Point3 origin;    // v3
        public Vector3 direction; // v3
        public double maxFactor;
    }

    public class AABBTreeRayTest
    {
        struct PriorityNode
        {
            public AABBTree tree;
            public int nodeIndex;
            public double distance;
        }

#if DEBUG
        private AABBTreeRayTestResult DefaultCallback(AABBTree tree, AABBExternalNode node, AABBTreeRay ray, double distance, double upperBound)
        {
            return new AABBTreeRayTestResult() { factor = Math.Min(distance, upperBound) };
        }

        public AABBTreeRayTestResult rayTest(AABBTree[] trees, AABBTreeRay ray)
        {
            return rayTest(trees, ray, DefaultCallback);
        }

#endif
        public AABBTreeRayTestResult rayTest(AABBTree[] trees,
                AABBTreeRay ray,
                Func<AABBTree, AABBExternalNode, AABBTreeRay, double, double, AABBTreeRayTestResult> callback)
        {
            //
            //
            // we traverse both trees at once
            // keeping a priority list of nodes to check next.

            // TODO: possibly implement priority list more effeciently?
            //       binary heap probably too much overhead in typical case.
            List<PriorityNode> priorityList = new List<PriorityNode>(); 
                                                              //current upperBound on distance to first intersection
                                                              //and current closest object properties
            AABBTreeRayTestResult minimumResult = null;

            var upperBound = ray.maxFactor;

            for (int i = 0; i < trees.Length; i += 1)
            {
                AABBTree tree = trees[i];
                if (tree.getNodeCount() != 0)
                {
                    upperBound = processNode(tree, ray, 0, upperBound, callback, ref priorityList, ref minimumResult);
                }
            }

            while (priorityList.Count != 0)
            {
                var nodeObj = priorityList.Last();
                priorityList.RemoveAt(priorityList.Count - 1);
                // A node inserted into priority list after this one may have
                // moved the upper bound.
                if (nodeObj.distance >= upperBound)
                {
                    continue;
                }

                var nodeIndex = nodeObj.nodeIndex;
                AABBTree tree = nodeObj.tree;
                var nodes = tree.getNodes();

                var node = nodes[nodeIndex];
                var maxIndex = nodeIndex + node.escapeNodeOffset;

                var childIndex = nodeIndex + 1;
                do
                {
                    upperBound = processNode(tree, ray, childIndex, upperBound, callback, ref priorityList, ref minimumResult);
                    childIndex += nodes[childIndex].escapeNodeOffset;
                }
                while (childIndex < maxIndex);
            }

            return minimumResult;
        }

        // evaluate distance factor to a node's extents from ray origin, along direction
        // use this to induce an ordering on which nodes to check.
        double? distanceExtents(AABBTreeRay ray, AABBBox extents, double upperBound)
        {
            var origin = ray.origin;
            var direction = ray.direction;

            // values used throughout calculations.
            var o0 = origin[0];
            var o1 = origin[1];
            var o2 = origin[2];
            var d0 = direction[0];
            var d1 = direction[1];
            var d2 = direction[2];
            var id0 = 1 / d0;
            var id1 = 1 / d1;
            var id2 = 1 / d2;

            var min0 = extents[0];
            var min1 = extents[1];
            var min2 = extents[2];
            var max0 = extents[3];
            var max1 = extents[4];
            var max2 = extents[5];

            // treat origin internal to extents as 0 distance.
            if (min0 <= o0 && o0 <= max0 &&
                min1 <= o1 && o1 <= max1 &&
                min2 <= o2 && o2 <= max2)
            {
                return 0.0;
            }

            double tmin, tmax;
            double tymin, tymax;
            double del;
            if (d0 >= 0)
            {
                // Deal with cases where d0 == 0
                del = (min0 - o0);
                tmin = ((del == 0) ? 0 : (del * id0));
                del = (max0 - o0);
                tmax = ((del == 0) ? 0 : (del * id0));
            }
            else
            {
                tmin = ((max0 - o0) * id0);
                tmax = ((min0 - o0) * id0);
            }

            if (d1 >= 0)
            {
                // Deal with cases where d1 == 0
                del = (min1 - o1);
                tymin = ((del == 0) ? 0 : (del * id1));
                del = (max1 - o1);
                tymax = ((del == 0) ? 0 : (del * id1));
            }
            else
            {
                tymin = ((max1 - o1) * id1);
                tymax = ((min1 - o1) * id1);
            }

            if ((tmin > tymax) || (tymin > tmax))
            {
                return null;
            }

            if (tymin > tmin)
            {
                tmin = tymin;
            }

            if (tymax < tmax)
            {
                tmax = tymax;
            }

            double tzmin, tzmax;
            if (d2 >= 0)
            {
                // Deal with cases where d2 == 0
                del = (min2 - o2);
                tzmin = ((del == 0) ? 0 : (del * id2));
                del = (max2 - o2);
                tzmax = ((del == 0) ? 0 : (del * id2));
            }
            else
            {
                tzmin = ((max2 - o2) * id2);
                tzmax = ((min2 - o2) * id2);
            }

            if ((tmin > tzmax) || (tzmin > tmax))
            {
                return null;
            }

            if (tzmin > tmin)
            {
                tmin = tzmin;
            }

            if (tzmax < tmax)
            {
                tmax = tzmax;
            }

            if (tmin < 0)
            {
                tmin = tmax;
            }

            if(0 <= tmin && tmin < upperBound)
            {
                return tmin;
            }

            return null;
        }

        //if node is a leaf, intersect ray with shape
        // otherwise insert node into priority list.
        double processNode(AABBTree tree, 
            AABBTreeRay ray, 
            int nodeIndex, 
            double upperBound, 
            Func<AABBTree, AABBExternalNode, AABBTreeRay, double, double, AABBTreeRayTestResult> callback, 
            ref List<PriorityNode> priorityList, 
            ref AABBTreeRayTestResult minimumResult)
        {
            var nodes = tree.getNodes();
            var node = nodes[nodeIndex];
            var distance = distanceExtents(ray, node.extents, upperBound);
            if (distance == null)
            {
                return upperBound;
            }

            if (node.externalNode != null)
            {
                var result = callback(tree, node.externalNode, ray, distance.Value, upperBound);
                if (result != null)
                {
                    minimumResult = result;
                    upperBound = result.factor;
                }
            }
            else
            {
                // TODO: change to binary search?
                var length = priorityList.Count;
                int i;
                for (i = 0; i < length; i += 1)
                {
                    var curObj = priorityList[i];
                    if (distance > curObj.distance)
                    {
                        break;
                    }
                }

				if(i>= length - 1){
					//insert node at index i
					priorityList.Add(new PriorityNode() {
						tree = tree,
						nodeIndex = nodeIndex,
						distance = distance.Value
					});
				} 
				else {
					//insert node at index i
					priorityList.Insert(i+1, new PriorityNode() {
						tree = tree,
						nodeIndex = nodeIndex,
						distance = distance.Value
					});
				}
            }

            return upperBound;
        }
    }
}
