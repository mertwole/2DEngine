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
                    BroadPhase.Broad_Phase(GameObjects[a], GameObjects[b]);//all pairs without repeats
                }
            }
        }

        

    }
}
