namespace InterpolateShape
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.RightImageBox = new System.Windows.Forms.PictureBox();
            this.LeftImageBox = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.GLControl = new SharpGL.OpenGLControl();
            this.startButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.TopViewButton = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.FlontViewButton = new System.Windows.Forms.RadioButton();
            this.ParseViewButton = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.RightImageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LeftImageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GLControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // RightImageBox
            // 
            this.RightImageBox.Location = new System.Drawing.Point(13, 32);
            this.RightImageBox.Name = "RightImageBox";
            this.RightImageBox.Size = new System.Drawing.Size(320, 240);
            this.RightImageBox.TabIndex = 0;
            this.RightImageBox.TabStop = false;
            this.RightImageBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.imageClick);
            // 
            // LeftImageBox
            // 
            this.LeftImageBox.Location = new System.Drawing.Point(349, 32);
            this.LeftImageBox.Name = "LeftImageBox";
            this.LeftImageBox.Size = new System.Drawing.Size(320, 240);
            this.LeftImageBox.TabIndex = 1;
            this.LeftImageBox.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "RightImage";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(349, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "LeftImage";
            // 
            // GLControl
            // 
            this.GLControl.BackColor = System.Drawing.SystemColors.Control;
            this.GLControl.DrawFPS = false;
            this.GLControl.Location = new System.Drawing.Point(15, 287);
            this.GLControl.Name = "GLControl";
            this.GLControl.OpenGLVersion = SharpGL.Version.OpenGLVersion.OpenGL2_1;
            this.GLControl.RenderContextType = SharpGL.RenderContextType.DIBSection;
            this.GLControl.RenderTrigger = SharpGL.RenderTrigger.TimerBased;
            this.GLControl.Size = new System.Drawing.Size(320, 240);
            this.GLControl.TabIndex = 4;
            this.GLControl.OpenGLInitialized += new System.EventHandler(this.glInit);
            this.GLControl.OpenGLDraw += new SharpGL.RenderEventHandler(this.glDraw);
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(349, 501);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(93, 26);
            this.startButton.TabIndex = 5;
            this.startButton.Text = "REDraw";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(414, 287);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "CatchedPoint: 0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(416, 312);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "Area: 0";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(540, 479);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(129, 48);
            this.button1.TabIndex = 8;
            this.button1.Text = "All Points Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(349, 312);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(33, 153);
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(349, 472);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(33, 19);
            this.textBox1.TabIndex = 10;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(349, 287);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(33, 19);
            this.textBox2.TabIndex = 11;
            // 
            // TopViewButton
            // 
            this.TopViewButton.AutoSize = true;
            this.TopViewButton.Checked = true;
            this.TopViewButton.Location = new System.Drawing.Point(6, 29);
            this.TopViewButton.Name = "TopViewButton";
            this.TopViewButton.Size = new System.Drawing.Size(64, 16);
            this.TopViewButton.TabIndex = 12;
            this.TopViewButton.TabStop = true;
            this.TopViewButton.Text = "topView";
            this.TopViewButton.UseVisualStyleBackColor = true;
            this.TopViewButton.CheckedChanged += new System.EventHandler(this.TopViewButton_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.FlontViewButton);
            this.groupBox1.Controls.Add(this.ParseViewButton);
            this.groupBox1.Controls.Add(this.TopViewButton);
            this.groupBox1.Location = new System.Drawing.Point(416, 337);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 100);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ViewMode";
            // 
            // FlontViewButton
            // 
            this.FlontViewButton.AutoSize = true;
            this.FlontViewButton.Location = new System.Drawing.Point(106, 29);
            this.FlontViewButton.Name = "FlontViewButton";
            this.FlontViewButton.Size = new System.Drawing.Size(75, 16);
            this.FlontViewButton.TabIndex = 14;
            this.FlontViewButton.TabStop = true;
            this.FlontViewButton.Text = "FrontView";
            this.FlontViewButton.UseVisualStyleBackColor = true;
            this.FlontViewButton.CheckedChanged += new System.EventHandler(this.FlontViewButton_CheckedChanged);
            // 
            // ParseViewButton
            // 
            this.ParseViewButton.AutoSize = true;
            this.ParseViewButton.Location = new System.Drawing.Point(6, 66);
            this.ParseViewButton.Name = "ParseViewButton";
            this.ParseViewButton.Size = new System.Drawing.Size(77, 16);
            this.ParseViewButton.TabIndex = 13;
            this.ParseViewButton.Text = "ParseView";
            this.ParseViewButton.UseVisualStyleBackColor = true;
            this.ParseViewButton.CheckedChanged += new System.EventHandler(this.ParseViewButton_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 550);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.GLControl);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LeftImageBox);
            this.Controls.Add(this.RightImageBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.RightImageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LeftImageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GLControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox RightImageBox;
        private System.Windows.Forms.PictureBox LeftImageBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private SharpGL.OpenGLControl GLControl;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.RadioButton TopViewButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton ParseViewButton;
        private System.Windows.Forms.RadioButton FlontViewButton;
    }
}

