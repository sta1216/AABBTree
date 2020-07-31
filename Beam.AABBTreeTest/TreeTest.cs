using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Beam
{
    public class TreeTest
    {
        [Fact]
        public void AddTest()
        {
            var tree = new AABBTree(true);
            tree.add(new AABBExternalNode() { Data = 1 }, new AABBBox(new Point3(1, 1, 1), new Point3(10, 10, 10)));
            tree.finalize();
            Assert.Equal(1, tree.getNodes().Count);

            tree.add(new AABBExternalNode() { Data = 2 }, new AABBBox(new Point3(5, 5, 5), new Point3(15, 15, 15)));
            tree.finalize();
            Assert.Equal(3, tree.getNodes().Count);

            tree.clear();
            Assert.Equal(0, tree.getNodes().Count);
        }

        [Fact]
        public void RemoveTest()
        {
            var tree = new AABBTree(true);
            var node = new AABBExternalNode() { Data = 1 };
            tree.add(node, new AABBBox(new Point3(1, 1, 1), new Point3(10, 10, 10)));
            tree.add(new AABBExternalNode() { Data = 2 }, new AABBBox(new Point3(5, 5, 5), new Point3(15, 15, 15)));
            tree.finalize();
            Assert.Equal(3, tree.getNodes().Count);

            tree.remove(node);
            tree.finalize();
            Assert.Equal(1, tree.getNodes().Count);

            tree.clear();
            Assert.Equal(0, tree.getNodes().Count);
        }

        [Fact]
        public void UpdateTest()
        {
            var tree = new AABBTree(true);
            var node = new AABBExternalNode() { Data = 1 };
            tree.add(node, new AABBBox(new Point3(1, 1, 1), new Point3(10, 10, 10)));
            tree.add(new AABBExternalNode() { Data = 2 }, new AABBBox(new Point3(5, 5, 5), new Point3(15, 15, 15)));
            tree.finalize();
            Assert.Equal(3, tree.getNodes().Count);

            tree.update(node, new AABBBox(new Point3(20, 20, 20), new Point3(33, 33, 33)));
            tree.finalize();
            Assert.Equal(3, tree.getNodes().Count);

            var treeNode = tree.getNodes().FirstOrDefault(n => n.externalNode?.spatialIndex == node.spatialIndex);
            Assert.NotNull(treeNode);
            Assert.Equal(20, treeNode.extents[0]);
            Assert.Equal(20, treeNode.extents[1]);
            Assert.Equal(20, treeNode.extents[2]);
            Assert.Equal(33, treeNode.extents[3]);
            Assert.Equal(33, treeNode.extents[4]);
            Assert.Equal(33, treeNode.extents[5]);

            tree.clear();
            Assert.Equal(0, tree.getNodes().Count);
        }
    }
}
