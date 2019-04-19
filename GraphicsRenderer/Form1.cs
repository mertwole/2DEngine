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
        public Form1()
        {
            InitializeComponent();

            //testbed

            Core.CreateScene(0);
            Scene scene = Core.GetScene(0);
            /*
            scene.GameObjects.Add(new GameObject());
            GameObject circle0 = scene.GameObjects[0];
            circle0.AddCircleCollider(20);
            circle0.Position = new Vector2(30, 30);
            circle0.isstatic = true;
            circle0.AddBody();
            circle0.body.MomentOfInertia = 1;
            circle0.body.Mass = 0;

            scene.GameObjects.Add(new GameObject());
            GameObject circle1 = scene.GameObjects[1];
            circle1.AddCircleCollider(20);
            circle1.Position = new Vector2(32, 80);
            circle1.isstatic = false;
            circle1.AddBody();
            circle1.body.MomentOfInertia = 1;
            circle1.body.Mass = 1;
            */
            scene.GameObjects.Add(new GameObject());
            GameObject box = scene.GameObjects[0];
            box.AddPolygonCollider(new Vector2[]
            {
                new Vector2(30, 30),
                new Vector2(30, -30),
                new Vector2(-30, -30),
                new Vector2(-30, 30)
            });
            box.Position = new Vector2(130, 140);
            box.isstatic = false;
            box.AddBody();
            box.body.MomentOfInertia = 100;
            box.body.Mass = 1;
            box.body.DynamicFriction = 0.7f;
            box.body.StaticFriction = 0.7f;
            //box.body.CenterOfMass_local = new Vector2(10, -10);

            scene.GameObjects.Add(new GameObject());
            GameObject platform = scene.GameObjects[1];
            platform.AddPolygonCollider(new Vector2[]
            {
                new Vector2(200, 0),
                new Vector2(200, 90),
                new Vector2(80, 70),
                new Vector2(0, 10)
            });
            platform.isstatic = true;
            platform.AddBody();
            platform.body.MomentOfInertia = 0;
            platform.body.Mass = 0;      

            //*******
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GameObject.Draw(pictureBox1);

            Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Core.GetScene(0).Tick(0.05f);
            GameObject.Draw(pictureBox1);

            Invalidate();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Core.GetScene(0).Tick(0.05f);
            GameObject.Draw(pictureBox1);

            Invalidate();
        }
    }
}
