using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Beam
{
    public class OverlappingTest
    {
        private AABBTree getInitData1()
        {
            var tree = new AABBTree(true);
            tree.add(new AABBExternalNode() { Data = 1 }, new AABBBox(new Point3(0, 0, 0), new Point3(10, 10, 10)));
            tree.add(new AABBExternalNode() { Data = 2 }, new AABBBox(new Point3(5, 5, 5), new Point3(15, 15, 15)));
            tree.add(new AABBExternalNode() { Data = 3 }, new AABBBox(new Point3(20, 20, 20), new Point3(30, 30, 30)));
            tree.add(new AABBExternalNode() { Data = 4 }, new AABBBox(new Point3(40, 40, 40), new Point3(50, 50, 50)));
            tree.add(new AABBExternalNode() { Data = 5 }, new AABBBox(new Point3(51, 51, 51), new Point3(60, 60, 60)));

            tree.finalize();

            return tree;
        }

        [Fact]
        public void getOverlappingPairs_Test()
        {
            AABBTree tree = getInitData1();
            Assert.NotNull(tree);

            List<AABBExternalNode> overlappingNodes = new List<AABBExternalNode>();
            var count = tree.getOverlappingPairs(overlappingNodes);
            Assert.Equal(2, count);
            Assert.Equal(1, overlappingNodes[0].Data);
            Assert.Equal(2, overlappingNodes[1].Data);
        }

        [Theory]
        [InlineData(-10, 10, new int[] {1,2})]
        [InlineData(0, 10, new int[] { 1,2})]
        [InlineData(-35, 35, new int[] { 1, 2, 3 })]
        [InlineData(-35, -15, new int[] { })]
        [InlineData(75, 95, new int[] { })]
        [InlineData(0, 95, new int[] {1,2,3,4,5 })]
        public void getOverlappingNodes_Test(double min, double max, int[] result)
        {
            AABBTree tree = getInitData1();
            Assert.NotNull(tree);

            List<AABBExternalNode> overlappingNodes = new List<AABBExternalNode>();
            var queryExtents = new AABBBox(new Point3(min, min, min), new Point3(max, max, max));
            var actualCount = tree.getOverlappingNodes(queryExtents, overlappingNodes);
            Assert.Equal(result.Length, actualCount);
            var actualRes = overlappingNodes.Select(n => (int)n.Data).ToList();
            actualRes.Sort();
            Assert.Equal(result, actualRes);
        }

        [Theory]
        [InlineData(12, 0, new int[] { 2 })]
        [InlineData(12, 5, new int[] { 1, 2 })]
        [InlineData(12, 15, new int[] { 1, 2, 3})]
        [InlineData(-12, 10, new int[] { })]
        [InlineData(-12, 200, new int[] { 1, 2, 3,4,5 })]
        public void getSphereOverlappingNodes_Test(double center, double radius, int[] result)
        {
            AABBTree tree = getInitData1();
            Assert.NotNull(tree);

            List<AABBExternalNode> overlappingNodes = new List<AABBExternalNode>();
            var actualCount = tree.getSphereOverlappingNodes(new Point3(center, center, center), radius, overlappingNodes);
            Assert.Equal(result.Length, actualCount);
            var actualRes = overlappingNodes.Select(n => (int)n.Data).ToList();
            actualRes.Sort();
            Assert.Equal(result, actualRes);
        }
    }
}
