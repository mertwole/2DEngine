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

        public static Calculation CalculateBouncity = (x, y) => x + y / 2;
        public static Calculation CalculateStaticFriction = (x, y) => x + y / 2;
        public static Calculation CalculateDynamicFriction = (x, y) => x + y / 2;

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

        public Vector2 CenterOfMass_local;

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
