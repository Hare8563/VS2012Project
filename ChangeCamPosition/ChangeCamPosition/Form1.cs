using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpGL;
using SharpGL.SceneGraph.Cameras;
using SharpGL.SceneGraph.Collections;

namespace ChangeCamPosition
{
    public partial class Form1 : Form
    {
        float[] position = new float[3];
        float[] target = new float[3];
        float[] vector = new float[3];


        public Form1()
        {
            InitializeComponent();
        }

        private void gl_Draw(object sender, SharpGL.RenderEventArgs args)
        {

            OpenGL gl = openGLControl1.OpenGL;

            //  Clear the color and depth buffer.
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            
            {//For camera control
                gl.MatrixMode(SharpGL.Enumerations.MatrixMode.Projection);
                gl.LoadIdentity();
                gl.Viewport(0, 0, openGLControl1.Width, openGLControl1.Height);

                //  Create a perspective transformation.
                //gl.Perspective(60.0f, (double)Width / (double)Height, 0.01, 1000.0);
                gl.Ortho(-openGLControl1.Width / 2, openGLControl1.Width / 2, -openGLControl1.Height / 2, openGLControl1.Height, 0.01, 1000.0);
                //  Use the 'look at' helper function to position and aim the camera.
                gl.LookAt(position[0], position[1], position[2], target[0], target[1], target[2], vector[0], vector[1], vector[2]);
            }



            gl.MatrixMode(SharpGL.Enumerations.MatrixMode.Modelview);
            gl.LoadIdentity();

            drawGrid();

            //  Draw a coloured pyramid.
            gl.Begin(OpenGL.GL_TRIANGLE_STRIP);
            gl.Color(1.0F, 0.0F, 0.0F);
            gl.Vertex(target[0], target[1] - 20, target[2] - 20);
            gl.Color(0.0F, 1.0F, 0.0F);
            gl.Vertex(target[0], target[1] + 20, target[2] - 20);
            gl.Color(0.0F, 0.0F, 1.0F);
            gl.Vertex(target[0], target[1] - 20, target[2] + 20);
            gl.Color(1.0F, 1.0F, 0.0F);
            gl.Vertex(target[0], target[1] + 20, target[2] + 20);
            gl.End();

        }


        private void drawGrid()
        {
            OpenGL gl = openGLControl1.OpenGL;

            // z-axis
            gl.Begin(OpenGL.GL_LINES);
            {
                gl.LineWidth(5.0f);
                gl.Color(0.0, 0.0, 1.0);
                gl.Vertex(0, 0, -250.0);
                gl.Vertex(0, 0, 250.0);
            }
            gl.End();

            for (int x=-5; x<=5; x++) {
                gl.Begin(OpenGL.GL_LINES);
                {
                    gl.Color(1.0, 1.0, 1.0);
                    gl.Vertex(x, 0, -5.0);
                    gl.Vertex(x, 0, 5.0);
                }
                gl.End();
            }

            // x-axis
            gl.Begin(OpenGL.GL_LINES);
            {
                gl.LineWidth(5.0f);
                gl.Color(1.0, 0.0, 0.0);
                gl.Vertex(-250, 0, 0);
                gl.Vertex(250, 0, 0);
            }
            gl.End();

            for (int z=-5; z<=5; z++) {
                gl.Begin(OpenGL.GL_LINES);
                {
                    gl.Color(1.0, 1.0, 1.0);
                    gl.Vertex(-5.0, 0, z);
                    gl.Vertex(5.0, 0, z);
                }
                gl.End();
            }

            // y-axis
            gl.Begin(OpenGL.GL_LINES);
            {
                gl.LineWidth(5.0f);
                gl.Color(0.0, 1.0, 0.0);
                gl.Vertex(0, -250, 0);
                gl.Vertex(0, 250, 0);
            }
            gl.End();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            

            position[0] = float.Parse(textBox1.Text);
            position[1] = float.Parse(textBox2.Text);
            position[2] = float.Parse(textBox3.Text);

            target[0] = float.Parse(textBox4.Text);
            target[1] = float.Parse(textBox5.Text);
            target[2] = float.Parse(textBox6.Text);

            vector[0] = float.Parse(textBox7.Text);
            vector[1] = float.Parse(textBox8.Text);
            vector[2] = float.Parse(textBox9.Text);

            
            
            //  Set the modelview matrix.

        }

        private void gl_init(object sender, EventArgs e)
        {
            position[0] = -50;
            position[1] = 0;
            position[2] = 100;

            target[0] = 0;
            target[1] = 0;
            target[2] = 0;

            vector[0] = 0;
            vector[1] = 0;
            vector[2] = 1;


           
        }
    }
}
