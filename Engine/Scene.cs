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

        public Vector2 Gravity = new Vector2(0, -10);

        struct DestroyBounds
        {
            public bool DestroyPoint(Vector2 point)
            {
                if (left != null && point.x < left)
                    return true;

                if (right != null && point.x > right)
                    return true;

                if (top != null && point.y > top)
                    return true;

                if (point.y < bottom)
                    return true;

                return false;
            }

            public DestroyBounds(float? left, float? right, float? top, float bottom)
            {
                this.left = left;
                this.right = right;
                this.top = top;
                this.bottom = bottom;
            }

            float? left;
            float? right;
            float? top;
            float bottom;

        }

        DestroyBounds destroybounds = new DestroyBounds();

        public void SetDestroyBounds(float? left, float? right, float? top, float bottom)
        {
            destroybounds = new DestroyBounds(left, right, top, bottom);
        }

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

            DestroyObjectsOutsideDestroybounds();
        }

        void ApplyForces(float dt)
        {
            foreach(GameObject go in GameObjects)
            {
                if(!go.isstatic)
                {                  
                    go.body.Velocity += go.body.Inv_mass * go.body.Force * dt;
                    go.Position += go.body.Velocity * dt;

                    go.body.AngularVelocity += go.body.Torque * go.body.Inv_MomentOfInertia * dt;
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
                    go.body.Force += Gravity * go.body.Mass;
                }
            }
        }

        void ResolveCollisions()
        {
            for(int a = 0; a < GameObjects.Count - 1; a++)
            {
                for(int b = a + 1; b < GameObjects.Count; b++)
                {
                    BroadPhase.Broad_Phase(GameObjects[a], GameObjects[b]);//all pairs without repeats
                }
            }
        }

        void DestroyObjectsOutsideDestroybounds()
        {
            for(int i = 0; i < GameObjects.Count; i++)
            {
                if (!GameObjects[i].isstatic && destroybounds.DestroyPoint(GameObjects[i].Position))
                    GameObjects.RemoveAt(i);
            }
        }

    }
}
