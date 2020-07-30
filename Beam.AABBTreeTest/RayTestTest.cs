using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Beam
{
    public class RayTestTest
    {
        private AABBTree getInitData()
        {
            var tree = new AABBTree(true);
            tree.add(new AABBExternalNode() { Data = 1 }, new AABBBox(new Point3(0, 0, 0), new Point3(10, 10, 10)));
            tree.add(new AABBExternalNode() { Data = 2 }, new AABBBox(new Point3(5, 5, 5), new Point3(15, 15, 15)));
            tree.add(new AABBExternalNode() { Data = 3 }, new AABBBox(new Point3(20, 20, 20), new Point3(30, 30, 30)));
            tree.add(new AABBExternalNode() { Data = 4 }, new AABBBox(new Point3(40, 40, 40), new Point3(50, 50, 50)));
            tree.add(new AABBExternalNode() { Data = 5 }, new AABBBox(new Point3(50, 50, 50), new Point3(60, 60, 60)));

            tree.finalize();

            return tree;
        }

        [Fact]
        public void rayTest_Test()
        {
            AABBTree tree = getInitData();
            Assert.NotNull(tree);

            var ray = new AABBTreeRay() { direction = new Vector3(1, 1, 1), origin = new Point3(), maxFactor = 30 };
            var testRes = new AABBTreeRayTest().rayTest(new AABBTree[] { tree }, ray);
        }
    }
}
