using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Engine.Math;

namespace Engine
{
    class Resolver
    {
        public static void BroadPhase(GameObject a, GameObject b)
        {
            if (a.body == null || b.body == null)//collision resolving requires body to work
                return;

            if (a.isstatic && b.isstatic)//if gameobjects are static they cant collide
                return;

            //mask clipping
            if ((a.collider.CollisionMask & b.collider.CollisionMask) == 0)
                return;

            /*broad phase code here(aabb or obb or x&y axis projections)
            *
            *
            * 
            * 
            * 
            * 
            */

            NarrowPhase(a.collider, b.collider);
        }

        static void NarrowPhase(Collider a, Collider b)
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

        static void Resolve(Collider a, Collider b, CollisionInfo info)
        {
            Body a_body = a.gameobject.body;
            Body b_body = b.gameobject.body;

            float bouncity = Body.CalculateBouncity(a_body.Bouncity, b_body.Bouncity);
            float static_friction = Body.CalculateStaticFriction(a_body.StaticFriction, b_body.StaticFriction);
            float dynamic_friction = Body.CalculateDynamicFriction(a_body.DynamicFriction, b_body.DynamicFriction);

            int ContactPointCount = info.ContactPoints.Count;

            for(int i = 0; i < ContactPointCount; i++)
            {
                var contact = info.ContactPoints[i];

                Vector2 ra = contact.point - (a_body.CenterOfMass_local + a.gameobject.Position);
                Vector2 rb = contact.point - (b_body.CenterOfMass_local + b.gameobject.Position);

                Vector2 RelativeVelocity = (b_body.Velocity + Vector2.CrossProduct(b_body.AngularVelocity, rb))
                    - (a_body.Velocity + Vector2.CrossProduct(a_body.AngularVelocity, ra));

                float VelAlongNormal = (RelativeVelocity * contact.normal);

                if (VelAlongNormal > 0)
                    continue;

                //formula for impulse scalar
                float ImpulseScalar = (-(1 + bouncity) * VelAlongNormal) / (a_body.Inv_mass + b_body.Inv_mass
                + sqr(Vector2.CrossProduct(ra, contact.normal)) * a_body.Inv_MomentOfInertia
                + sqr(Vector2.CrossProduct(rb, contact.normal)) * b_body.Inv_MomentOfInertia);

                ImpulseScalar /= ContactPointCount;

                contact.ImpulseScalar = ImpulseScalar;

                Vector2 Impulse = contact.normal * ImpulseScalar;

                //applying impulse
                a_body.Velocity -= a_body.Inv_mass * Impulse;
                a_body.AngularVelocity -= a_body.Inv_MomentOfInertia * Math.Vector2.CrossProduct(ra, Impulse);

                b_body.Velocity += b_body.Inv_mass * Impulse;
                b_body.AngularVelocity += b_body.Inv_MomentOfInertia * Math.Vector2.CrossProduct(rb, Impulse);
                //**********************         

                info.ContactPoints[i] = contact;

                ra = contact.point - (a_body.CenterOfMass_local + a.gameobject.Position);
                rb = contact.point - (b_body.CenterOfMass_local + b.gameobject.Position);

                //friction
                RelativeVelocity = (b_body.Velocity + Vector2.CrossProduct(b_body.AngularVelocity, rb))
                - (a_body.Velocity + Vector2.CrossProduct(a_body.AngularVelocity, ra));

                Vector2 tangent = (RelativeVelocity - (RelativeVelocity * contact.normal)
                    * contact.normal).normalized;

                float jt = -(RelativeVelocity * tangent);
                jt /= a_body.Inv_mass + b_body.Inv_mass
                + sqr(Vector2.CrossProduct(ra, contact.normal)) * a_body.Inv_MomentOfInertia
                + sqr(Vector2.CrossProduct(rb, contact.normal)) * b_body.Inv_MomentOfInertia;
                jt /= ContactPointCount;

                Vector2 FrictionImpulse;

                if (abs(jt) < contact.ImpulseScalar * static_friction)
                {
                    FrictionImpulse = jt * tangent;
                }
                else
                {
                    FrictionImpulse = -contact.ImpulseScalar * tangent * dynamic_friction;
                }

                a_body.Velocity -= a_body.Inv_mass * FrictionImpulse;
                a_body.AngularVelocity -= a_body.Inv_MomentOfInertia * Vector2.CrossProduct(ra, FrictionImpulse);

                b_body.Velocity += b_body.Inv_mass * FrictionImpulse;
                b_body.AngularVelocity += b_body.Inv_MomentOfInertia * Vector2.CrossProduct(rb, FrictionImpulse);
            }

            foreach(var contact in info.ContactPoints)
            { 
                //positional correction
                
                float percent = 0.1f;
                float slop = 0.01f;

                Vector2 correction = ((max(contact.depth - slop, 0)
                / (a_body.Inv_mass + b_body.Inv_mass)) * contact.normal * percent ) / ContactPointCount;

                a.gameobject.Position -= a_body.Inv_mass * correction;
                b.gameobject.Position += b_body.Inv_mass * correction;
                
                //*******************
            }
        }
    }
}
