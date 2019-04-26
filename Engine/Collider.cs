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

        public abstract AABB GetBoundingBox();
    }

    struct AABB
    {
        public Vector2 LeftBottom;
        public Vector2 RightTop;

        public AABB(Vector2 left_bottom, Vector2 right_top)
        {
            LeftBottom = left_bottom;
            RightTop = right_top;
        }
    }

    class Circle : Collider
    {
        public Vector2 Center { get; private set; }

        public float Radius { get; private set; }
                       
        public override void UpdatePosAndRotation(Vector2 GameObjectPos, float rotation)
        {
            Center = GameObjectPos;
        }

        public override AABB GetBoundingBox()
        {
            Vector2 r = new Vector2(Radius, Radius);
            return new AABB(Center - r, Center + r);
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

        AABB bounding_box = new AABB();
        bool bounding_box_actual;

        public override AABB GetBoundingBox()
        {
            if (bounding_box_actual)
                return bounding_box;

            float min_x = Verts[0].x;
            float max_x = Verts[0].x;
            float min_y = Verts[0].y;
            float max_y = Verts[0].y;

            foreach (var vert in Verts)
            {
                if (vert.x > max_x)
                    max_x = vert.x;
                else if (vert.x < min_x)
                    min_x = vert.x;

                if (vert.y > max_y)
                    max_y = vert.y;
                else if (vert.y < min_y)
                    min_y = vert.y;
            }

            bounding_box = new AABB(new Vector2(min_x, min_y), new Vector2(max_x, max_y));
            bounding_box_actual = true;

            return bounding_box;
        }

        public override void UpdatePosAndRotation(Vector2 GameObjectPos, float rotation)
        {
            if (rotation == 0)
            {
                Vector2 offset = GameObjectPos - (Verts[0] - origin_verts[0]);
                bounding_box.LeftBottom += offset;
                bounding_box.RightTop += offset;
            }
            else
              bounding_box_actual = false;

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
