using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Engine.Math;

namespace Engine
{
    public class Scene
    {
        public List<GameObject> GameObjects = new List<GameObject>();

        public void Tick(float dt)
        {
            foreach (GameObject go in GameObjects)
            {
                go.collider.UpdatePosAndRotation(go.Position, go.Rotation);
            }

            ResolveCollisions();
            ApplyGravity();
            ApplyForces(dt);

            ClearForces();
        }

        void ApplyForces(float dt)
        {
            foreach(GameObject go in GameObjects)
            {
                if(!go.isstatic)
                {                  
                    go.body.Velocity += go.body.Inv_mass * go.body.Force * dt;
                    go.Position += go.body.Velocity * dt;

                    go.body.AngularVelocity += go.body.Torque * (1 / go.body.MomentOfInertia) * dt;
                    go.Rotation += go.body.AngularVelocity * dt;

                    go.collider.UpdatePosAndRotation(go.Position, go.Rotation);
                }
            }
        }

        void ClearForces()
        {
            foreach(var go in GameObjects)
            {
                go.body.Force = Vector2.zero;
                go.body.Torque = 0;
            }
        }

        void ApplyGravity()
        {
            foreach(GameObject go in GameObjects)
            {
                if (!go.isstatic)
                {
                    go.body.Force += new Vector2(0, -10);
                }
            }
        }

        void ResolveCollisions()
        {
            for(int a = 0; a < GameObjects.Count - 1; a++)
            {
                for(int b = a + 1; b < GameObjects.Count; b++)
                {
                    BroadPhase(GameObjects[a], GameObjects[b]);//all pairs without repeats
                }
            }
        }

        void BroadPhase(GameObject a, GameObject b)
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

        void NarrowPhase(Collider a, Collider b)
        {
            CollisionInfo info = GetCollisionInfo(a, b);

            if (info.ContactPoints.Count == 0)
                return;

            Resolve(a, b, info);
        }

        CollisionInfo GetCollisionInfo(Collider a, Collider b)
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

        void Resolve(Collider a, Collider b, CollisionInfo info)
        {
            Body a_body = a.gameobject.body;
            Body b_body = b.gameobject.body;

            float bouncity = Body.CalculateBouncity(a_body.Bouncity, b_body.Bouncity);
            float static_friction = Body.CalculateStaticFriction(a_body.StaticFriction, b_body.StaticFriction);
            float dynamic_friction = Body.CalculateDynamicFriction(a_body.DynamicFriction, b_body.DynamicFriction);

            int ContactPointCount = info.ContactPoints.Count;

            foreach (var contact in info.ContactPoints)
            {
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

                Vector2 Impulse = contact.normal * ImpulseScalar;

                //applying impulse
                a_body.Velocity -= a_body.Inv_mass * Impulse;
                a_body.AngularVelocity -= a_body.Inv_MomentOfInertia * Vector2.CrossProduct(ra, Impulse);

                b_body.Velocity += b_body.Inv_mass * Impulse;
                b_body.AngularVelocity += b_body.Inv_MomentOfInertia * Vector2.CrossProduct(rb, Impulse);
                //**********************         

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

                if (abs(jt) < ImpulseScalar * dynamic_friction)
                {
                    FrictionImpulse = jt * tangent;
                }
                else
                {
                    FrictionImpulse = - ImpulseScalar * tangent * dynamic_friction;
                }

                a_body.Velocity -= a_body.Inv_mass * FrictionImpulse;
                a_body.AngularVelocity -= a_body.Inv_MomentOfInertia * Vector2.CrossProduct(ra, FrictionImpulse);

                b_body.Velocity += b_body.Inv_mass * FrictionImpulse;
                b_body.AngularVelocity += b_body.Inv_MomentOfInertia * Vector2.CrossProduct(rb, FrictionImpulse);

                //********

                //positional correction

                float percent = 0.9f;
                float slop = 0.1f;

                Vector2 correction = (max(contact.depth - slop, 0)
                / (a_body.Inv_mass + b_body.Inv_mass)) * contact.normal * percent;

                a.gameobject.Position -= a_body.Inv_mass * correction;
                b.gameobject.Position += b_body.Inv_mass * correction;
                
                //*******************
                
            }
        }

    }
}
