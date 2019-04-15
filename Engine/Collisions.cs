using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Engine.Math;

namespace Engine
{
    class CollisionInfo
    {
        public struct ContactPoint
        {
            public Vector2 point;
            public Vector2 normal;
            public float depth;

            public ContactPoint(Vector2 _point, Vector2 _normal, float _depth)
            {
                point = _point;
                normal = _normal;
                depth = _depth;
            }
        }

        public List<ContactPoint> ContactPoints = new List<ContactPoint>();
    }

    class Collisions
    {
        public static CollisionInfo CirclevsCircle(Circle a, Circle b)
        {
            CollisionInfo info = new CollisionInfo();

            if ((a.Center - b.Center).sqrMagnitude <= sqr(a.Radius + b.Radius))//if distance between centers <= Ra + Rb
            {
                float distance = (a.Center - b.Center).magnitude;

                var ContactPoint = new CollisionInfo.ContactPoint();
                ContactPoint.depth = a.Radius + b.Radius - distance; 
                ContactPoint.normal = (b.Center - a.Center) / distance;
                ContactPoint.point = a.Center + ContactPoint.normal * a.Radius;

                info.ContactPoints.Add(ContactPoint);
            }

            return info;
        }

        public static CollisionInfo CirclevsPolygon(Circle a, Polygon b)
        {
            return new CollisionInfo();
        }

        public static CollisionInfo PolygonvsCircle(Polygon a, Circle b)
        {
            return new CollisionInfo();
        }

        public static CollisionInfo PolygonvsPolygon(Polygon a, Polygon b)
        {
            CollisionInfo info = new CollisionInfo();



            return info;
        }
    }

}
