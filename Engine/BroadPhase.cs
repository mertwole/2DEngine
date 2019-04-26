using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Engine.NarrowPhase;

namespace Engine
{
    class BroadPhase
    {
        public static void Broad_Phase(GameObject a, GameObject b)
        {
            if (a.body == null || b.body == null)//collision resolving requires body to work
                return;

            if (a.isstatic && b.isstatic)//if gameobjects are static they cant collide
                return;

            if ((a.collider.CollisionMask & b.collider.CollisionMask) == 0)//mask clipping
                return;

            if (!Collisions.AABBvsAABB(a.collider.GetBoundingBox(), b.collider.GetBoundingBox()))//AABBs
                return;

            Narrow_Phase(a.collider, b.collider);
        }
    }
}
