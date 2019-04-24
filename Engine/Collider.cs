using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Engine.Math;

namespace Engine
{
    abstract class Collider
    {
        public int CollisionMask = 1;

        public GameObject gameobject;

        public abstract void UpdatePosAndRotation(Vector2 GameObjectPos, float rotation);
    }

    class Circle : Collider
    {
        public Vector2 Center { get; private set; }

        public float Radius { get; private set; }
                       
        public override void UpdatePosAndRotation(Vector2 GameObjectPos, float rotation)
        {
            Center = GameObjectPos;
        }

        public Circle(float r)
        {
            Radius = r;
        }
    }

    class Polygon : Collider
    {
        Vector2[] origin_verts;
        public Vector2[] Verts { get; private set; }

        public override void UpdatePosAndRotation(Vector2 GameObjectPos, float rotation)
        {
            Matrix2x2 rot_matrix = new Matrix2x2(rotation, Matrix2x2.MatrixCreationType.rotation_rad);

            for(int i = 0; i < Verts.Length; i++)
            {
                Verts[i] = rot_matrix * (origin_verts[i] - gameobject.body.CenterOfMass_local) + GameObjectPos + gameobject.body.CenterOfMass_local;
            }
        }

        public Polygon(Vector2[] vertices)
        {
            origin_verts = (Vector2[])vertices.Clone();
            Verts = (Vector2[])origin_verts.Clone();
        }
    }
}
