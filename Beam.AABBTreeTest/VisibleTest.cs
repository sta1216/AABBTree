using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Beam
{
    public class VisibleTest
    {
        private AABBTree getInitData()
        {
            var tree = new AABBTree(true);
            tree.add(new AABBExternalNode() { Data = 1 }, new AABBBox(new Point3(1, 1, 1), new Point3(10, 10, 10)));
            tree.add(new AABBExternalNode() { Data = 2 }, new AABBBox(new Point3(5, 5, 5), new Point3(15, 15, 15)));
            tree.add(new AABBExternalNode() { Data = 3 }, new AABBBox(new Point3(20, 20, 20), new Point3(30, 30, 30)));
            tree.add(new AABBExternalNode() { Data = 4 }, new AABBBox(new Point3(40, 40, 40), new Point3(50, 50, 50)));
            tree.add(new AABBExternalNode() { Data = 5 }, new AABBBox(new Point3(51, 51, 51), new Point3(60, 60, 60)));

            tree.finalize();

            return tree;
        }

        [Theory]
        [InlineData(0, 0, -1, 0, new int[] { })]
        [InlineData(0, 0, -1, -4, new int[] { 1 })]
        [InlineData(0, 0, 1, -4, new int[] { 1, 2, 3, 4, 5 })]
        //[InlineData(0, 0, 1, 100, new int[] { 4, 5 })] ”–Œ Ã‚
        public void getOverlappingNodes_Test(double a, double b, double c, double d, int[] result)
        {
            AABBTree tree = getInitData();
            Assert.NotNull(tree);

            List<AABBExternalNode> resultNodes = new List<AABBExternalNode>();
            var plane = new Plane3(a, b, c, d);
            var actualCount = tree.getVisibleNodes(new Plane3[] { plane }, resultNodes);
            Assert.Equal(result.Length, actualCount);
            var actualRes = resultNodes.Select(n => (int)n.Data).ToList();
            actualRes.Sort();
            Assert.Equal(result, actualRes);
        }
    }
}
