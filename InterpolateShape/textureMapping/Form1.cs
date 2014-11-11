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
using SharpGL.SceneGraph.Assets;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using LabImg;

namespace textureMapping
{
    public partial class Form1 : Form
    {
       
        const int NumberOfTexture = 1;
        uint[] texID = new uint[NumberOfTexture];
        float rotation = 0.0F;
        Texture texture = new Texture();

        public Form1()
        {
            InitializeComponent();
            OpenGL gl = openGLControl1.OpenGL;

            //テクスチャを有効化する
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            //テクスチャを作成
            Bitmap bmp = new Bitmap(Environment.CurrentDirectory + @"\RectifyRight.bmp");
            bmp.RotateFlip(RotateFlipType.Rotate180FlipX);

            texture.Create(gl, bmp);
            
        }

        private void glDraw(object sender, SharpGL.RenderEventArgs args)
        {

            OpenGL gl = openGLControl1.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();
            

            //カメラ設定
            gl.MatrixMode(SharpGL.Enumerations.MatrixMode.Projection);
            gl.LoadIdentity();
            //gl.Ortho(-openGLControl1.Width * 2, openGLControl1.Width * 2, -openGLControl1.Height * 2, openGLControl1.Height * 2, 0.01, 10000);
            gl.Perspective(60.0F, openGLControl1.Width / openGLControl1.Height, 0.01, 1000);
            gl.LookAt(-500.0, 0.0, 10.0, 0, 0, 0, 0, 0, 1);
            gl.Viewport(0, 0, openGLControl1.Width, openGLControl1.Height);
            gl.MatrixMode(SharpGL.Enumerations.MatrixMode.Modelview);


            gl.LoadIdentity();
            gl.Rotate(rotation, 0, 0, 1);

            //テクスチャをバインドする
            texture.Bind(gl);

            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(0.0, 1.0); 

            gl.Vertex(0, 100, 100);
            gl.TexCoord(0.0, 0.0); 

            gl.Vertex(0, 100, -100);
            gl.TexCoord(1.0, 0.0);

            gl.Vertex(0, -100, -100);
            gl.TexCoord(1.0, 1.0);

            gl.Vertex(0, -100, 100);
            gl.End();

            rotation+=10;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void glInit(object sender, EventArgs e)
        {

        }
    }
}
