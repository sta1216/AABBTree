using System;

namespace Beam
{
    //
    // tree node
    //
    public class AABBTreeNode
    {
        static int version = 1;
        public int escapeNodeOffset;
        public AABBExternalNode externalNode; // user data
        public AABBBox extents; // bounding box

        public AABBTreeNode(AABBBox extents, int escapeNodeOffset, AABBExternalNode externalNode)
        {
            this.escapeNodeOffset = escapeNodeOffset;
            this.externalNode = externalNode;
            this.extents = extents;
        }

        public bool isLeaf()
        {
            return this.externalNode != null;
        }

        public void reset(double minX, double minY, double minZ,
            double maxX, double maxY, double maxZ,
            int escapeNodeOffset, AABBExternalNode externalNode = null)
        {
            this.escapeNodeOffset = escapeNodeOffset;
            this.externalNode = externalNode;
            var oldExtents = this.extents;
            oldExtents[0] = minX;
            oldExtents[1] = minY;
            oldExtents[2] = minZ;
            oldExtents[3] = maxX;
            oldExtents[4] = maxY;
            oldExtents[5] = maxZ;
        }

        public void clear()
        {
            this.escapeNodeOffset = 1;
            this.externalNode = null;
            var oldExtents = this.extents;
            var maxNumber = double.MaxValue;
            oldExtents[0] = maxNumber;
            oldExtents[1] = maxNumber;
            oldExtents[2] = maxNumber;
            oldExtents[3] = -maxNumber;
            oldExtents[4] = -maxNumber;
            oldExtents[5] = -maxNumber;
        }
    }

    public class AABBExternalNode
    {
        public int spatialIndex { get; set; }
        public object Data { get; set; }

        public void Print()
        {
            Console.WriteLine($"Node: {Data}");
        }
    }

    public class AABBBox
    {
        private double[] box = new double[6];
        public double this[int index]
        {
            get { return box[index]; }
            set { box[index] = value; }
        }

        public AABBBox()
        {
            for (int i = 0; i < 6; i++)
            {
                box[i] = 0;
            }
        }

        public AABBBox(Point3 min, Point3 max)
        {
            box[0] = min.X;
            box[1] = min.Y;
            box[2] = min.Z;
            box[3] = max.X;
            box[4] = max.Y;
            box[5] = max.Z;
        }
    }
}
