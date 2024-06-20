using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingGen
{
    public static class GRingSimplify
    {
        internal class GRingNode
        {
            double x;
            double y;
            GRingNode next;
            GRingNode pre;
        }
        internal class GRing
        {
            GRingNode beginNode;
            Queue<GRingNode> nodes;
            
        }
    }
}
