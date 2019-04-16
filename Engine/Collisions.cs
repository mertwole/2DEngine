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

        struct Line
        {
            public Vector2 start;
            public Vector2 end;

            public Line(Vector2 _start, Vector2 _end)
            {
                start = _start;
                end = _end;
            }
        }

        public static CollisionInfo PolygonvsPolygon(Polygon a, Polygon b)
        {
            CollisionInfo info = new CollisionInfo();

            Vector2 GetFarthestPoint(Vector2[] polygon, Vector2 direction)
            {
                Vector2 out_vector;

                if (direction.y == 0)//x coord check
                {
                    out_vector = polygon[0];

                    foreach(var vert in polygon)
                    {
                        if(sgn(vert.x - out_vector.x) == sgn(direction.x))//if direction == right searching max(x) else min
                        {
                            out_vector = vert;
                        }
                    }

                    return out_vector;
                }

                float k = -direction.x / direction.y; //perpendicular direction angular coefficient
                //y = kx + b line equation. we find line equation for each point and b 
                //is the intersection point of this line with Oy. Extremum of b means that there is the 
                //ultimate point in the direction, perpendicular to this line i.e. "direction" vector
                out_vector = polygon[0];

                foreach(var vert in polygon)
                {
                    if(sgn((vert.y - vert.x * k) - (out_vector.y - out_vector.x * k)) == 
                        sgn(direction.y))//if top direction searching max(b) else min(b)
                    {
                        out_vector = vert;
                    }
                }

                return out_vector;

            }

            Vector2 GetVertNearestToOrigin(Line line, Vector2[] a_poly, Vector2[] b_poly)
            {
                Vector2 line_normal;//vector perpendicular to line & facing to origin

                //get origin half space about line
                if (line.start.x == line.end.x)
                {
                    //vert line
                    line_normal = line.start.x > 0 ? new Vector2(-1, 0) : new Vector2(1, 0);
                }
                else
                {
                    //y = kx + t
                    //line intersects Oy(at (0, b))
                    float k = (line.start.y - line.end.y) / (line.start.x - line.end.x);
                    float t = line.start.y - k * line.start.x;

                    if (k == 0)
                    {
                        //k_normal = -1 / k , -1 / 0 = NAN
                        line_normal = t > 0 ? new Vector2(0, -1) : new Vector2(0, 1);
                    }
                    else
                    {
                        //k != 0
                        float k_normal = -1 / k;

                        line_normal = t > 0 ? new Vector2(1, k_normal) : new Vector2(-1, -k_normal);
                    }
                }

                return GetFarthestPoint(a_poly, line_normal) - GetFarthestPoint(b_poly, -line_normal);
            }

            bool SimplexContainsOrigin(List<Vector2> simplex)
            {
                for (int i = 0; i < 3; i++)
                {
                    int j = (i + 1) % 3;
                    int k = (i + 2) % 3;

                    //side jk third vert is i
                    //a_k * x + b_k * y + c_k = 0
                    float a_k = simplex[j].y - simplex[k].y;
                    float b_k = simplex[k].x - simplex[j].x;
                    float c_k = Vector2.CrossProduct(simplex[j], simplex[k]);

                    if (sgn(a_k * simplex[i].x + b_k * simplex[i].y + c_k) != sgn(c_k))//if Simplex[i] and origin
                    //are in different half-spaces
                    {
                        return false;
                    }
                }

                return true;
            }

            Vector2 init_direction = new Vector2(1, 0);//may be any instread of Vector2.zero

            //mirroring b about origin(to get minkovski differense instread of summ)
            //so GetFarthestPoint must get -dir instread of dir

            List<Vector2> Simplex = new List<Vector2>();
            Simplex.Add(GetFarthestPoint(a.Verts, init_direction) - GetFarthestPoint(b.Verts, -init_direction));
            Simplex.Add(GetFarthestPoint(a.Verts, -init_direction) - GetFarthestPoint(b.Verts, init_direction));
            Simplex.Add(GetVertNearestToOrigin(new Line(Simplex[0], Simplex[1]), a.Verts, b.Verts));

            while(true)
            {
                if (SimplexContainsOrigin(Simplex))
                {
                    //collision detected

                    break;
                }
                
                //determine nearest side to origin
                //
                //if we have triangle the nearest side to origin is side such that another vertex of simplex
                //and origin lying in different half-spaces relatively this side
                //
                //ax + by + c = 0 
                //if sgn(a * dot0.x + b * dot0.y + c) == - sgn(a * dot1.x + b * dot1.y + c) then dot 0 and dot 1
                //lying on different sides of line ax + by + c = 0

                for(int i = 0; i < 3; i++)
                {
                    int j = (i + 1) % 3;
                    int k = (i + 2) % 3;

                    //side ij third vert is k

                    float a_k = Simplex[i].y - Simplex[j].y;
                    float b_k = Simplex[j].x - Simplex[i].x;                  
                    float c_k = Vector2.CrossProduct(Simplex[i], Simplex[j]);

                    if(sgn(a_k * Simplex[k].x + b_k * Simplex[k].y + c_k) == - sgn(c_k))
                    {
                        //needed side
                        Simplex[k] = GetVertNearestToOrigin(new Line(Simplex[i], Simplex[j]), a.Verts, b.Verts);

                        if(Simplex[k] == Simplex[i] || Simplex[k] == Simplex[j])
                        {
                            //no origin in minkovski difference
                            info.ContactPoints = new List<CollisionInfo.ContactPoint>();
                            return info;
                        }

                        break;
                    }
                }
            }

            //collision detected


            return info;
        }
    }

}
