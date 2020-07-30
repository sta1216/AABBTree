using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Beam
{
    public class OverlappingTest
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
        public void getOverlappingPairs_Test()
        {
            AABBTree tree = getInitData();
            Assert.NotNull(tree);

            List<AABBExternalNode> overlappingNodes = new List<AABBExternalNode>();
            var count = tree.getOverlappingPairs(overlappingNodes);
            Assert.Equal(2, count);
            Assert.Equal(overlappingNodes[0].Data, 1);
            Assert.Equal(overlappingNodes[1].Data, 2);
        }

        [Theory]
        [InlineData(-10, 10, 0, new int[] {})]
        [InlineData(0, 10, 2, new int[] { 1,2})]
        [InlineData(-40, 40, 3, new int[] { 1, 2, 3 })]
        public void getOverlappingNodes_Test(double min, double max, int count, int[] result)
        {
            AABBTree tree = getInitData();
            Assert.NotNull(tree);

            List<AABBExternalNode> overlappingNodes = new List<AABBExternalNode>();
            var queryExtents = new AABBBox(new Point3(min, min, min), new Point3(max, max, max));
            var actualCount = tree.getOverlappingNodes(queryExtents, overlappingNodes);
            Assert.Equal(count, actualCount);
            var actualRes = overlappingNodes.Select(n => (int)n.Data).ToList();
            actualRes.Sort();
            Assert.Equal(result, actualRes);
        }

        [Theory]
        [InlineData(10, 0, 0, new int[] { })]
        [InlineData(10, 0, 0, new int[] { })]
        public void getSphereOverlappingNodes_Test(double center, double radius, int count, int[] result)
        {
            AABBTree tree = getInitData();
            Assert.NotNull(tree);

            List<AABBExternalNode> overlappingNodes = new List<AABBExternalNode>();
            var actualCount = tree.getSphereOverlappingNodes(new Point3(center, center, center), 21, overlappingNodes);
            Assert.Equal(count, actualCount);
            var actualRes = overlappingNodes.Select(n => (int)n.Data).ToList();
            actualRes.Sort();
            Assert.Equal(result, actualRes);
        }
    }
}
