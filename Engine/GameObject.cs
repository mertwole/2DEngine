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
        internal Collider collider;
        public Body body;

        public bool isstatic = false;

        public Vector2 Position;

        public float Rotation;

        public void AddCircleCollider(float radius)
        {
            collider = new Circle(radius);
            collider.gameobject = this;
        }

        public void AddPolygonCollider(Vector2[] verts)
        {
            collider = new Polygon(verts);
            collider.gameobject = this;
        }

        public void AddBody()
        {
            body = new Body();
        }

        //************test***************

        public static void Draw(PictureBox picturebox)
        {
            Bitmap img = new Bitmap(picturebox.Width, picturebox.Height);
            Graphics g = Graphics.FromImage(img);
            g.Clear(Color.Black);

            foreach (var go in Core.GetScene(0).GameObjects)
            {
                if (go.collider.GetType() == typeof(Circle))
                {
                    Circle coll = go.collider as Circle;

                    g.DrawEllipse(new Pen(Color.Green), new RectangleF(go.Position.x - coll.Radius, go.Position.y - coll.Radius, coll.Radius * 2, coll.Radius * 2));
                }
                else if(go.collider.GetType() == typeof(Polygon))
                {
                    Polygon coll = go.collider as Polygon;

                    g.DrawPolygon(new Pen(Color.Green), (coll.Verts.Select(vec => new PointF(vec.x, vec.y)).ToArray()));
                }

            }
          
            img.RotateFlip(RotateFlipType.RotateNoneFlipY);
            picturebox.Image = img;
        }

        //*******************************
    }
}
