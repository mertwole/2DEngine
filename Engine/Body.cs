using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Engine.Math;

namespace Engine
{
    public class Body
    {
        public delegate float Calculation(float x, float y);

        public static Calculation CalculateBouncity = (x, y) => (x + y) / 2;
        public static Calculation CalculateStaticFriction = (x, y) => (x + y) / 2;
        public static Calculation CalculateDynamicFriction = (x, y) => (x + y) / 2;

        public GameObject gameobject;

        public float Bouncity = 0;

        public float StaticFriction = 0;
        public float DynamicFriction = 0;

        float mass;// 0 means infinity
        float inv_mass;
        public float Inv_mass { get { return inv_mass; } }
        public float Mass
        {
            get { return mass; }
            set
            {
                mass = value;
                inv_mass = (mass == 0 ? 0 : 1 / mass); // 1/infinity == 0
            }
        }

        public void SetAutoMass(float density)
        {
            float area = 0;

            if (gameobject.collider.GetType() == typeof(Circle))
                area = PI * sqr((gameobject.collider as Circle).Radius);
            else if (gameobject.collider.GetType() == typeof(Polygon))
                area = GetPolyArea((gameobject.collider as Polygon).Verts);

            Mass = area * density / 1000;
        }

        float GetPolyArea(Vector2[] poly)
        {
            float area = 0;

            for (int i = 0; i < poly.Length; i++)
            {
                area += Vector2.CrossProduct(poly[i], poly[(i + 1) % poly.Length]);
            }

            return abs(area) / 2;
        }

        public void SetAutoMomentOfInertia(float multiplier)
        {
            var collider = gameobject.collider;

            float moment_inertia = 0;

            if (collider.GetType() == typeof(Circle))
            {
                moment_inertia = sqr((collider as Circle).Radius);
            }
            else if(collider.GetType() == typeof(Polygon))
            {
                var poly = collider as Polygon;

                for (int i = 0; i < poly.Verts.Length; i++)
                {
                    moment_inertia += (CenterOfMass_local - poly.Verts[i]).sqrMagnitude;
                }

                moment_inertia /= poly.Verts.Length;
            }

            MomentOfInertia = moment_inertia * multiplier;         
        }

        internal Vector2 CenterOfMass_local;

        public Vector2 Force;
        public Vector2 Velocity;

        public float AngularVelocity;
        public float Torque;

        float momentofinertia;// 0 means infinity
        float inv_momentofinertia;
        public float Inv_MomentOfInertia { get { return inv_momentofinertia; } }
        public float MomentOfInertia
        {
            get { return momentofinertia; }
            set
            {
                momentofinertia = value;
                inv_momentofinertia = (momentofinertia == 0 ? 0 : 1 / momentofinertia); // 1/infinity == 0
            }
        }
    }
}
