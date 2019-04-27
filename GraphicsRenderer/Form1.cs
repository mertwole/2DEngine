using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine;
using static Engine.Math;

namespace GraphicsRenderer
{
    public partial class Form1 : Form
    {
        Scene scene;

        public Form1()
        {
            InitializeComponent();

            //testbed
            Core.CreateScene(0);
            scene = Core.GetScene(0);
            scene.SetDestroyBounds(null, null, null, -10000);

            GameObject platform = new GameObject();
            platform.AddPolygonCollider(new Vector2[]
            {
                new Vector2(10, 10),
                new Vector2(10, 50),
                new Vector2(700, 50),
                new Vector2(700, 10)
            });
            platform.isstatic = true;
            scene.GameObjects.Add(platform);
        /*
            GameObject wall_left = new GameObject();
            wall_left.AddPolygonCollider(new Vector2[]
            {
                new Vector2(700, 200),
                new Vector2(700, 50),
                new Vector2(710, 50),
                new Vector2(710, 200)
            });
            wall_left.isstatic = true;
            scene.GameObjects.Add(wall_left);

  
            GameObject wall_right = new GameObject();
            wall_right.AddPolygonCollider(new Vector2[]
            {
                new Vector2(0, 200),
                new Vector2(0, 10),
                new Vector2(10, 10),
                new Vector2(10, 200)
            });
            wall_right.isstatic = true;
            scene.GameObjects.Add(wall_right);
            */
            //*******
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button3.Text = timer1.Enabled ? "enable ticks" : "disable ticks";

            timer1.Enabled = !timer1.Enabled;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            for(int i = 0; i < 5; i++)
            Core.GetScene(0).Tick(0.02f);

            Invalidate();
        }

        void Draw_Scene()
        {
            foreach (var go in Core.GetScene(0).GameObjects)
            {
                go.collider.UpdatePosAndRotation(go.Position, go.Rotation);
            }

            Bitmap img = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(img);
            g.Clear(Color.Black);

            foreach (var go in Core.GetScene(0).GameObjects)
            {
                if (go.collider.GetType() == typeof(Circle))
                {
                    Circle coll = go.collider as Circle;

                    g.DrawEllipse(new Pen(Color.Green), new RectangleF(go.Position.x - coll.Radius, go.Position.y - coll.Radius, coll.Radius * 2, coll.Radius * 2));
                }
                else if (go.collider.GetType() == typeof(Polygon))
                {
                    Polygon coll = go.collider as Polygon;

                    g.DrawPolygon(new Pen(Color.Green), (coll.Verts.Select(vec => new PointF(vec.x, vec.y)).ToArray()));
                }

            }

            img.RotateFlip(RotateFlipType.RotateNoneFlipY);
            pictureBox1.Image = img;
        }

        bool poly_creating = false;
        List<Vector2> new_poly = new List<Vector2>();

        private void button4_Click(object sender, EventArgs e)
        {
            if(!poly_creating)
            {
                new_poly = new List<Vector2>();
                button4.Text = "finish poly creating";
            }
            else
            {
                scene.GameObjects.Add(new GameObject());
                GameObject poly = scene.GameObjects[scene.GameObjects.Count - 1];

                Vector2[] verts = GetConvexHull(new_poly.ToArray());

                poly.AddPolygonCollider(verts);

                poly.Position = Vector2.zero;
                poly.isstatic = false;
                poly.body.SetAutoMass(1);
                poly.body.DynamicFriction = 0;
                poly.body.StaticFriction = 0;
                poly.body.Bouncity = 0;
                poly.body.SetAutoMomentOfInertia(1);

                button4.Text = "start poly creating";
            }

            poly_creating = !poly_creating;
        }

        Random rand = new Random();

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if(crt_rnd_shps)
            {             
                if(rand.Next(4) == 0)
                {
                    //circle                   
                    GameObject circle = new GameObject();
                    circle.AddCircleCollider(rand.Next(50) + 3);
                    circle.Position = new Vector2(e.X, pictureBox1.Height - e.Y);
                    circle.body.SetAutoMass(1);
                    circle.body.SetAutoMomentOfInertia(1);
                    scene.GameObjects.Add(circle);
                }
                else
                {
                    //polygon
                    int vert_count = rand.Next(50) + 5;
                    Vector2[] verts = new Vector2[vert_count];
                    for(int i = 0; i < vert_count; i++)
                    {
                        verts[i] = new Vector2(e.X + (rand.Next(100) - 50), (pictureBox1.Height - e.Y) + (rand.Next(100) - 50));
                    }

                    verts = GetConvexHull(verts);                   

                    if(verts.Length <= 2)
                    {
                        return;
                    }

                    GameObject poly = new GameObject();
                    poly.AddPolygonCollider(verts);
                    poly.Position = Vector2.zero;
                    poly.body.SetAutoMass(1);
                    poly.body.SetAutoMomentOfInertia(1);
                    scene.GameObjects.Add(poly);
                }
                return;
            }

            if (poly_creating)
            {
                new_poly.Add(new Vector2(e.X, pictureBox1.Height - e.Y));
                return;
            }

            if(circle_creating)
            {
                if(circle_point_0 == Vector2.zero)
                {
                    circle_point_0 = new Vector2(e.X, pictureBox1.Height - e.Y);
                }
                else
                {
                    circle_point_1 = new Vector2(e.X, pictureBox1.Height - e.Y);
                }
                return;
            }
        }

        bool circle_creating = false;
        Vector2 circle_point_0 = Vector2.zero;
        Vector2 circle_point_1 = Vector2.zero;

        private void button5_Click(object sender, EventArgs e)
        {
            if(!circle_creating)
            {
                circle_creating = true;

                circle_point_0 = Vector2.zero;
                circle_point_1 = Vector2.zero;

                button5.Text = "finish circle creating";
            }
            else
            {        
                GameObject circle = new GameObject();
                circle.AddCircleCollider((circle_point_0 - circle_point_1).magnitude);
                circle.Position = circle_point_0;
                circle.body.SetAutoMass(1);
                scene.GameObjects.Add(circle);

                circle_creating = false;

                button5.Text = "start circle creating";
            }
        }



        Vector2[] GetConvexHull(Vector2[] new_poly)
        {
            return ConvexHull((new_poly.ToList().ConvertAll((x)=>new Point((int)x.x, (int)x.y))).ToArray()).ToList().ConvertAll((x)=>new Vector2(x.X, x.Y)).ToArray();
        }

        public static IEnumerable<Point> ConvexHull(Point[] points)
        {
            var upper = new List<Point>();
            var lower = new List<Point>();

            points = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToArray();

            upper.AddRange(points.Take(2));
            for (int i = 2; i < points.Count(); i++)
            {
                upper.Add(points[i]);
                while (upper.Count() > 2 && !IsRightTurn(upper.ElementAt(upper.Count - 1),
                upper.ElementAt(upper.Count - 2), upper.ElementAt(upper.Count - 3)))
                {
                    upper.RemoveAt(upper.Count() - 2);
                }
            }

            lower.Add(points.ElementAt(points.Count() - 1));
            lower.Add(points.ElementAt(points.Count() - 2));
            for (int i = points.Count() - 2; i >= 0; i--)
            {
                lower.Add(points.ElementAt(points.Count() - 1));
                while (lower.Count() > 2 && !IsRightTurn(lower.ElementAt(lower.Count - 1),
                lower.ElementAt(lower.Count - 2), lower.ElementAt(lower.Count - 3)))
                {
                    lower.RemoveAt(lower.Count() - 2);
                }
            }
            lower.RemoveAt(lower.Count() - 1);
            lower.RemoveAt(0);

            return upper.Concat(lower);
        }
        public static bool IsRightTurn(Point a, Point b, Point c)
        {
            var seg = new LineSegment(a, b);
            return WherePoint(seg, c) < 0;
        }
        public static int WherePoint(LineSegment vector, Point p)
        {
            return (vector.Q.X - vector.P.X) * (p.Y - vector.P.Y)
            - (vector.Q.Y - vector.P.Y) * (p.X - vector.P.X);
        }
        public class LineSegment
        {
            public Point P { get; private set; }
            public Point Q { get; private set; }

            public LineSegment(Point p, Point q)
            {
                this.P = p;
                this.Q = q;
            }
        }
        public class PointNamed
        {
            public int X { get; set; }
            public int Y { get; set; }
            public string Name { get; set; }
            public Point Point;

            public PointNamed(string name, Point point)
            {
                this.Name = name;
                this.X = point.X;
                this.Y = point.Y;
                this.Point = point;
            }
        }
















        bool crt_rnd_shps;

        private void button1_Click(object sender, EventArgs e)
        {
            crt_rnd_shps = !crt_rnd_shps;

            button1.Text = crt_rnd_shps ? "creatingRandomShapes on" : "creatingRandomShapes off";
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if(draw)
            Draw_Scene();
        }

        bool draw;

        private void button2_Click(object sender, EventArgs e)
        {
            draw = !draw;

            button2.Text = draw ? "disable draw" : "enable draw";
        }
    }
}
