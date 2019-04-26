using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Engine.Resolver;

namespace Engine
{
    class NarrowPhase
    {
        public static void Narrow_Phase(Collider a, Collider b)
        {
            CollisionInfo info = GetCollisionInfo(a, b);

            if (info.ContactPoints.Count == 0)
                return;

            Resolve(a, b, info);
        }

        static CollisionInfo GetCollisionInfo(Collider a, Collider b)
        {
            //all variants of collision
            //#####################
            if (a.GetType() == typeof(Circle) && b.GetType() == typeof(Circle))
            {
                return Collisions.CirclevsCircle(a as Circle, b as Circle);
            }
            //#####################
            if (a.GetType() == typeof(Polygon) && b.GetType() == typeof(Circle))
            {
                return Collisions.PolygonvsCircle(a as Polygon, b as Circle);
            }
            //#####################
            if (a.GetType() == typeof(Circle) && b.GetType() == typeof(Polygon))
            {
                return Collisions.CirclevsPolygon(a as Circle, b as Polygon);
            }
            //#####################
            if (a.GetType() == typeof(Polygon) && b.GetType() == typeof(Polygon))
            {
                return Collisions.PolygonvsPolygon(a as Polygon, b as Polygon);
            }
            //#####################
            return null;
        }
    }
}
