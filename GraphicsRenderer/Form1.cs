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

            scene.GameObjects.Add(new GameObject());
            GameObject platform = scene.GameObjects[scene.GameObjects.Count - 1];
            platform.AddPolygonCollider(new Vector2[]
            {
                new Vector2(10, 10),
                new Vector2(10, 50),
                new Vector2(500, 50),
                new Vector2(500, 10)
            });
            platform.isstatic = true;
            platform.AddBody();
            platform.body.MomentOfInertia = 0;
            platform.body.Mass = 0;
            platform.body.StaticFriction = 1.8f;
            platform.body.DynamicFriction = 1.8f;
            platform.body.Bouncity = 0;
            
            scene.GameObjects.Add(new GameObject());
            GameObject wall_left = scene.GameObjects[scene.GameObjects.Count - 1];
            wall_left.AddPolygonCollider(new Vector2[]
            {
                new Vector2(500, 200),
                new Vector2(500, 50),
                new Vector2(510, 50),
                new Vector2(510, 200)
            });
            wall_left.isstatic = true;
            wall_left.AddBody();
            wall_left.body.MomentOfInertia = 0;
            wall_left.body.Mass = 0;
            wall_left.body.StaticFriction = 0.2f;
            wall_left.body.DynamicFriction = 0.2f;

            scene.GameObjects.Add(new GameObject());
            GameObject wall_right = scene.GameObjects[scene.GameObjects.Count - 1];
            wall_right.AddPolygonCollider(new Vector2[]
            {
                new Vector2(0, 200),
                new Vector2(0, 10),
                new Vector2(10, 10),
                new Vector2(10, 200)
            });
            wall_right.isstatic = true;
            wall_right.AddBody();
            wall_right.body.MomentOfInertia = 0;
            wall_right.body.Mass = 0;
            wall_right.body.StaticFriction = 0.2f;
            wall_right.body.DynamicFriction = 0.2f;
            
            //*******
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GameObject.Draw(pictureBox1);

            Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Core.GetScene(0).Tick(0.01f);

            GameObject.Draw(pictureBox1);

            Invalidate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                Core.GetScene(0).Tick(0.01f);
            }
            
            GameObject.Draw(pictureBox1);

            Invalidate();
        }

        bool poly_creating = false;
        List<Vector2> new_poly = new List<Vector2>();

        private void button4_Click(object sender, EventArgs e)
        {
            if(!poly_creating)
            {
                new_poly = new List<Vector2>();
            }
            else
            {
                scene.GameObjects.Add(new GameObject());
                GameObject poly = scene.GameObjects[scene.GameObjects.Count - 1];
                poly.AddPolygonCollider(new_poly.ToArray());
                poly.Position = Vector2.zero;
                poly.isstatic = false;
                poly.AddBody();
                poly.body.MomentOfInertia = 100;
                poly.body.Mass = 1;
                poly.body.DynamicFriction = 0;
                poly.body.StaticFriction = 0;
                poly.body.Bouncity = 0;
                poly.body.CenterOfMass_local = GetPolyCOM(new_poly);

                float moment_inertia = 0;

                for(int i = 0; i < new_poly.Count; i++)
                {
                    moment_inertia += (poly.body.CenterOfMass_local - new_poly[i]).sqrMagnitude;
                }

                poly.body.MomentOfInertia = moment_inertia / 5;

            }

            poly_creating = !poly_creating;
        }

        Vector2 GetPolyCOM(List<Vector2> poly)
        {
            Vector2 com = Vector2.zero;

            for(int i = 0; i < poly.Count; i++)
            {
                com += poly[i];
            }

            com /= poly.Count;

            return com;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (poly_creating)
            {
                new_poly.Add(new Vector2(e.X, pictureBox1.Height - e.Y));
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            scene.GameObjects.Add(new GameObject());
            GameObject box = scene.GameObjects[scene.GameObjects.Count - 1];
            box.AddPolygonCollider(new Vector2[]
            {
                new Vector2(160, 170),
                new Vector2(160, 110),
                new Vector2(100, 110)
            });
            box.Position = Vector2.zero;
            box.isstatic = false;
            box.AddBody();
            box.body.MomentOfInertia = 100;
            box.body.Mass = 1;
            box.body.DynamicFriction = 0;
            box.body.StaticFriction = 0;
            box.body.Bouncity = 0;
            box.body.CenterOfMass_local = GetPolyCOM(new Vector2[]
            {
                new Vector2(160, 170),
                new Vector2(160, 110),
                new Vector2(100, 110)
            }.ToList());
        }
    }
}
