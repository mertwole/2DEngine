using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using static Engine.Math;

namespace Engine
{
    public class GameObject
    {
        public Collider collider { get; internal set; }
        public Body body = new Body();

        public GameObject()
        {
            body.gameobject = this;
        }

        public bool isstatic {
            get { return is_static; }
            set
            {
                is_static = value;

                if(is_static)
                {
                    body.Mass = 0;
                    body.MomentOfInertia = 0;
                }
            }
        }
        bool is_static;

        public Vector2 Position;

        public float Rotation;

        public void AddCircleCollider(float radius)
        {
            collider = new Circle(radius);
            body.CenterOfMass_local = Vector2.zero;
            collider.gameobject = this;
        }

        public void AddPolygonCollider(Vector2[] verts)
        {
            collider = new Polygon(verts);
            body.CenterOfMass_local = GetPolyCOM(verts) - Position;
            collider.gameobject = this;
        }

        Vector2 GetPolyCOM(Vector2[] poly)
        {
            Vector2 com = Vector2.zero;

            for (int i = 0; i < poly.Length; i++)
            {
                com += poly[i];
            }

            com /= poly.Length;

            return com;
        }  
    }
}
