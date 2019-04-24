﻿using System;
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
            public float ImpulseScalar;

            public ContactPoint(Vector2 _point, Vector2 _normal, float _depth)
            {
                point = _point;
                normal = _normal;
                depth = _depth;
                ImpulseScalar = 0;
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

        struct ClosestEdgeInfo
        {
            public int start_index;
            public int end_index;
            public float sqr_distance;
            public Line edge;
        }

        const float tolerance = 0.0001f;

        public static CollisionInfo PolygonvsPolygon(Polygon a, Polygon b)
        {
            //GJK
            CollisionInfo info = new CollisionInfo();

            int GetFarthestPointIndex(Vector2[] polygon, Vector2 direction)
            {
                int index = 0;

                if (direction.y == 0)//x coord check
                {
                    for(int vert = 0; vert < polygon.Length; vert++)
                    {
                        if (sgn(polygon[vert].x - polygon[index].x) == sgn(direction.x))//if direction == right searching max(x) else min
                        {
                            index = vert;
                        }
                    }

                    return index;
                }

                float k = -direction.x / direction.y; //perpendicular direction angular coefficient
                //y = kx + b line equation. we find line equation for each point and b 
                //is the intersection point of this line with Oy. Extremum of b means that there is the 
                //ultimate point in the direction, perpendicular to this line i.e. "direction" vector

                for (int vert = 0; vert < polygon.Length; vert++)
                {
                    if (sgn((polygon[vert].y - polygon[vert].x * k) - (polygon[index].y - polygon[index].x * k)) ==
                        sgn(direction.y))//if top direction searching max(b) else min(b)
                    {
                        index = vert;
                    }
                }

                return index;
            }

            Vector2 GetFarthestPoint(Vector2[] polygon, Vector2 direction)
            {
                return polygon[GetFarthestPointIndex(polygon, direction)];
            }

            Vector2 GetFarthestPointInDifference(Vector2 direction)
            {
                //b should be mirrored about origin so GetFarthestPoint with -dir instread of dir
                return GetFarthestPoint(a.Verts, direction) - GetFarthestPoint(b.Verts, -direction);
            }

            Vector2 GetNormalToLineFacingOrigin(Line line)
            {
                Vector2 line_normal;

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

                        if (k < 0)
                            line_normal *= -1;
                    }
                }

                return line_normal;
            }

            Vector2 GetVertNearestToOrigin(Line line, Vector2[] a_poly, Vector2[] b_poly)
            {
                return GetFarthestPointInDifference(GetNormalToLineFacingOrigin(line));
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

                    float half_space_simplex_i = sgn(a_k * simplex[i].x + b_k * simplex[i].y + c_k);
                    float half_space_origin = sgn(c_k);


                    if (half_space_simplex_i == - half_space_origin)//if Simplex[i] and origin
                    //are in different half-spaces
                    {
                        return false;
                    }
                }

                return true;
            }

            float GetSqrDistanceFromOriginToLine(Line line)
            {
                //ax + by + c = 0 
                //distance = |c|/sqrt(a^2 + b^2)
                float a_k = line.start.y - line.end.y;
                float b_k = line.end.x - line.start.x;
                float c_k = Vector2.CrossProduct(line.start, line.end);

                return sqr(c_k) / (sqr(a_k) + sqr(b_k));
            }

            float GetSqrDistanceFromPointToLine(Vector2 point, Line line)
            {
                //ax + by + c = 0 
                //distance = |a * x1 + b * y1 + c| / sqrt(a ^ 2 + b ^ 2)
                float a_k = line.start.y - line.end.y;
                float b_k = line.end.x - line.start.x;
                float c_k = Vector2.CrossProduct(line.start, line.end);

                return sqr(a_k * point.x + b_k * point.y + c_k) / (sqr(a_k) + sqr(b_k));
            }

            bool PolygonContainsPoint(Vector2[] poly, Vector2 point)
            {
                for(int i = 0; i < poly.Length; i++)
                {
                    int j = (i + 1) % poly.Length;

                    float a_k = poly[i].y - poly[j].y;
                    float b_k = poly[j].x - poly[i].x;
                    float c_k = Vector2.CrossProduct(poly[i], poly[j]);

                    int k = (i + 2) % poly.Length;

                    float vert_halfspace = sgn(a_k * poly[k].x + b_k * poly[k].y + c_k);
                    float point_halfspace = sgn(a_k * point.x + b_k * point.y + c_k);

                    if (point_halfspace == -vert_halfspace)
                        return false;
                }

                return true;
            }

            Vector2 init_direction = new Vector2(1, 0);//may be any instread of Vector2.zero

            List<Vector2> Simplex = new List<Vector2>();
            Simplex.Add(GetFarthestPointInDifference(init_direction));
            Simplex.Add(GetFarthestPointInDifference(-init_direction));
            Simplex.Add(GetVertNearestToOrigin(new Line(Simplex[0], Simplex[1]), a.Verts, b.Verts));

            while(true)
            {
                if (SimplexContainsOrigin(Simplex))
                {
                    //EPA with Simplex
                    ClosestEdgeInfo GetClosestEdge(List<Vector2> simplex)
                    {
                        int closest_edge_index = 0;
                        float min_distance = float.MaxValue;

                        for (int i = 0; i < simplex.Count; i++)
                        {
                            int j = (i + 1) % simplex.Count;

                            float sqr_distance = GetSqrDistanceFromOriginToLine(new Line(Simplex[i], Simplex[j]));

                            if (sqr_distance < min_distance)
                            {
                                min_distance = sqr_distance;
                                closest_edge_index = i;
                            }

                        }

                        return new ClosestEdgeInfo()
                        {
                            sqr_distance = min_distance,
                            start_index = closest_edge_index,
                            end_index = (closest_edge_index + 1) % simplex.Count,
                            edge = new Line(simplex[closest_edge_index], simplex[(closest_edge_index + 1) % simplex.Count])
                        };
                    }

                    while (true)
                    {
                        ClosestEdgeInfo closest_info = GetClosestEdge(Simplex);
                        Vector2 normal = -GetNormalToLineFacingOrigin(closest_info.edge).normalized;
                        Vector2 new_vert = GetFarthestPointInDifference(normal);
                        float distance = new_vert * normal;

                        if (abs(distance - sqrt(closest_info.sqr_distance)) < tolerance)
                        { 
                            info.ContactPoints = new List<CollisionInfo.ContactPoint>();

                            bool flip = false;

                            int incident_edge_start = GetFarthestPointIndex(a.Verts, normal);
                            int incident_edge_end = (incident_edge_start + 1) % a.Verts.Length;

                            if (!(abs((a.Verts[incident_edge_start] - a.Verts[incident_edge_end]) * normal) < tolerance))
                            {
                                incident_edge_end = (incident_edge_start - 1 + a.Verts.Length) % a.Verts.Length;
                            }

                            if (!(abs((a.Verts[incident_edge_start] - a.Verts[incident_edge_end]) * normal) < tolerance))
                            {
                                flip = true;

                                incident_edge_start = GetFarthestPointIndex(b.Verts, -normal);
                                incident_edge_end = (incident_edge_start + 1) % b.Verts.Length;

                                if (!(abs((b.Verts[incident_edge_start] - b.Verts[incident_edge_end]) * normal) < tolerance))
                                {
                                    incident_edge_end = (incident_edge_start - 1 + b.Verts.Length) % b.Verts.Length;
                                }
                            }

                            Line incident_edge;

                            if(!flip)
                            {//a
                                incident_edge = new Line(a.Verts[incident_edge_start], a.Verts[incident_edge_end]);
                            }
                            else
                            {//b
                                incident_edge = new Line(b.Verts[incident_edge_start], b.Verts[incident_edge_end]);
                            }


                            if (!flip)
                            {//a
                                int b_farthest = GetFarthestPointIndex(b.Verts, -normal);
                                int b_farthest_left_neighb = (b_farthest + 1) % b.Verts.Length;
                                int b_farthest_right_neighb = (b_farthest - 1 + b.Verts.Length) % b.Verts.Length;

                                info.ContactPoints.Add(new CollisionInfo.ContactPoint()
                                {
                                    depth = distance,
                                    normal = normal,
                                    point = b.Verts[b_farthest] + normal * distance

                                });

                                bool a_contains_left = PolygonContainsPoint(a.Verts, b.Verts[b_farthest_left_neighb]);
                                bool a_contains_right = PolygonContainsPoint(a.Verts, b.Verts[b_farthest_right_neighb]);

                                if (a_contains_left && a_contains_right)
                                {
                                    float left_sqr_dist = GetSqrDistanceFromPointToLine(b.Verts[b_farthest_left_neighb], incident_edge);
                                    float right_sqr_dist = GetSqrDistanceFromPointToLine(b.Verts[b_farthest_right_neighb], incident_edge);
                                    int b_almostfarthest = (right_sqr_dist > left_sqr_dist) ? b_farthest_right_neighb : b_farthest_left_neighb;
                                    float b_almost_farthest_distance = sqrt(max(right_sqr_dist, left_sqr_dist));

                                    info.ContactPoints.Add(new CollisionInfo.ContactPoint()
                                    {
                                        depth = b_almost_farthest_distance,
                                        normal = normal,
                                        point = b.Verts[b_almostfarthest] + normal * b_almost_farthest_distance

                                    });
                                }
                                else if (!a_contains_left && !a_contains_right)
                                {
                                    return info;
                                }
                                else if (a_contains_left)
                                {
                                    float dist = sqrt(GetSqrDistanceFromPointToLine(b.Verts[b_farthest_left_neighb], incident_edge));

                                    info.ContactPoints.Add(new CollisionInfo.ContactPoint()
                                    {
                                        depth = dist,
                                        normal = normal,
                                        point = b.Verts[b_farthest_left_neighb] + normal * dist

                                    });
                                }
                                else
                                {
                                    float dist = sqrt(GetSqrDistanceFromPointToLine(b.Verts[b_farthest_right_neighb], incident_edge));

                                    info.ContactPoints.Add(new CollisionInfo.ContactPoint()
                                    {
                                        depth = dist,
                                        normal = normal,
                                        point = b.Verts[b_farthest_right_neighb] + normal * dist

                                    });
                                }

                                return info;
                            }
                            else
                            {//b
                                int a_farthest = GetFarthestPointIndex(a.Verts, normal);
                                int a_farthest_left_neighb = (a_farthest + 1) % a.Verts.Length;
                                int a_farthest_right_neighb = (a_farthest - 1 + a.Verts.Length) % a.Verts.Length;

                                info.ContactPoints.Add(new CollisionInfo.ContactPoint()
                                {
                                    depth = distance,
                                    normal = normal,
                                    point = a.Verts[a_farthest] + normal * distance

                                });

                                bool b_contains_left = PolygonContainsPoint(b.Verts, a.Verts[a_farthest_left_neighb]);
                                bool b_contains_right = PolygonContainsPoint(b.Verts, a.Verts[a_farthest_right_neighb]);

                                if (b_contains_left && b_contains_right)
                                {
                                    float left_sqr_dist = GetSqrDistanceFromPointToLine(a.Verts[a_farthest_left_neighb], incident_edge);
                                    float right_sqr_dist = GetSqrDistanceFromPointToLine(a.Verts[a_farthest_right_neighb], incident_edge);
                                    int a_almostfarthest = (right_sqr_dist > left_sqr_dist) ? a_farthest_right_neighb : a_farthest_left_neighb;
                                    float a_almost_farthest_distance = sqrt(max(right_sqr_dist, left_sqr_dist));

                                    info.ContactPoints.Add(new CollisionInfo.ContactPoint()
                                    {
                                        depth = a_almost_farthest_distance,
                                        normal = normal,
                                        point = a.Verts[a_almostfarthest] + normal * a_almost_farthest_distance

                                    });
                                }
                                else if (!b_contains_left && !b_contains_right)
                                {
                                    return info;
                                }
                                else if (b_contains_left)
                                {
                                    float dist = sqrt(GetSqrDistanceFromPointToLine(a.Verts[a_farthest_left_neighb], incident_edge));

                                    info.ContactPoints.Add(new CollisionInfo.ContactPoint()
                                    {
                                        depth = dist,
                                        normal = normal,
                                        point = a.Verts[a_farthest_left_neighb] + normal * dist

                                    });
                                }
                                else
                                {
                                    float dist = sqrt(GetSqrDistanceFromPointToLine(a.Verts[a_farthest_right_neighb], incident_edge));

                                    info.ContactPoints.Add(new CollisionInfo.ContactPoint()
                                    {
                                        depth = dist,
                                        normal = normal,
                                        point = a.Verts[a_farthest_right_neighb] + normal * dist

                                    });
                                }

                                return info;
                            }
                        }

                        Simplex.Insert(closest_info.end_index, new_vert);
                    }
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

                        if(((Simplex[k].x == Simplex[i].x) && (Simplex[k].y == Simplex[i].y))
                        || ((Simplex[k].x == Simplex[j].x) && (Simplex[k].y == Simplex[j].y)))
                        {
                            //no origin in minkovski difference
                            info.ContactPoints = new List<CollisionInfo.ContactPoint>();
                            return info;
                        }

                        break;
                    }
                }
            }
        }
    }

}
