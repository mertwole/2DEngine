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
        internal static void Resolve(Collider a, Collider b, CollisionInfo info)
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

                float denominator = a_body.Inv_mass + b_body.Inv_mass
                + sqr(Vector2.CrossProduct(ra, contact.normal)) * a_body.Inv_MomentOfInertia
                + sqr(Vector2.CrossProduct(rb, contact.normal)) * b_body.Inv_MomentOfInertia;
                denominator *= ContactPointCount;

                //formula for impulse scalar
                float ImpulseScalar = (-(1 + bouncity) * VelAlongNormal) / denominator;

                Vector2 Impulse = contact.normal * ImpulseScalar;

                //applying impulse
                a_body.Velocity -= a_body.Inv_mass * Impulse;
                a_body.AngularVelocity -= a_body.Inv_MomentOfInertia * Vector2.CrossProduct(ra, Impulse);

                b_body.Velocity += b_body.Inv_mass * Impulse;
                b_body.AngularVelocity += b_body.Inv_MomentOfInertia * Vector2.CrossProduct(rb, Impulse);
                //**********************         

                //friction
                //re-count relative velocity
                RelativeVelocity = (b_body.Velocity + Vector2.CrossProduct(b_body.AngularVelocity, rb))
                - (a_body.Velocity + Vector2.CrossProduct(a_body.AngularVelocity, ra));

                Vector2 tangent = (RelativeVelocity - (RelativeVelocity * contact.normal)
                    * contact.normal).normalized;

                float jt = -(RelativeVelocity * tangent) / denominator;

                Vector2 FrictionImpulse;

                if (abs(jt) < ImpulseScalar * static_friction)
                    FrictionImpulse = jt * tangent;
                else
                    FrictionImpulse = -ImpulseScalar * tangent * dynamic_friction;

                //applying friction impulse
                a_body.Velocity -= a_body.Inv_mass * FrictionImpulse;
                a_body.AngularVelocity -= a_body.Inv_MomentOfInertia * Vector2.CrossProduct(ra, FrictionImpulse);

                b_body.Velocity += b_body.Inv_mass * FrictionImpulse;
                b_body.AngularVelocity += b_body.Inv_MomentOfInertia * Vector2.CrossProduct(rb, FrictionImpulse);
            }

            foreach(var contact in info.ContactPoints)
            { 
                //positional correction
                
                float percent = 0.6f;
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
