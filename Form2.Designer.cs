namespace WinFormsApp1321
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            button1 = new Button();
            textBox1 = new TextBox();
            button2 = new Button();
            button3 = new Button();
            label3 = new Label();
            label4 = new Label();
            button4 = new Button();
            label5 = new Label();
            textBox2 = new TextBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft YaHei UI", 18F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(341, 9);
            label1.Name = "label1";
            label1.Size = new Size(110, 31);
            label1.TabIndex = 0;
            label1.Text = "检测模式";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft YaHei UI", 21.75F, FontStyle.Regular, GraphicsUnit.Point);
            label2.Location = new Point(153, 118);
            label2.Name = "label2";
            label2.Size = new Size(133, 38);
            label2.TabIndex = 1;
            label2.Text = "检测文件";
            label2.Click += label2_Click;
            // 
            // button1
            // 
            button1.Location = new Point(198, 319);
            button1.Name = "button1";
            button1.Size = new Size(88, 39);
            button1.TabIndex = 2;
            button1.Text = "开始";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(314, 132);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(137, 23);
            textBox1.TabIndex = 3;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // button2
            // 
            button2.Location = new Point(462, 319);
            button2.Name = "button2";
            button2.Size = new Size(79, 39);
            button2.TabIndex = 4;
            button2.Text = "取消";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(501, 118);
            button3.Name = "button3";
            button3.Size = new Size(106, 39);
            button3.TabIndex = 5;
            button3.Text = "选择";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(162, 168);
            label3.Name = "label3";
            label3.Size = new Size(0, 17);
            label3.TabIndex = 6;
            label3.Click += label3_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(162, 214);
            label4.Name = "label4";
            label4.Size = new Size(0, 17);
            label4.TabIndex = 7;
            label4.Click += label4_Click;
            // 
            // button4
            // 
            button4.Location = new Point(501, 199);
            button4.Name = "button4";
            button4.Size = new Size(106, 37);
            button4.TabIndex = 8;
            button4.Text = "选择";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Microsoft YaHei UI", 21.75F, FontStyle.Regular, GraphicsUnit.Point);
            label5.Location = new Point(153, 203);
            label5.Name = "label5";
            label5.Size = new Size(133, 38);
            label5.TabIndex = 9;
            label5.Text = "试件信息";
            label5.Click += label5_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(314, 211);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(137, 23);
            textBox2.TabIndex = 10;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(textBox2);
            Controls.Add(label5);
            Controls.Add(button4);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(textBox1);
            Controls.Add(button1);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "Form2";
            Text = "Form2";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Button button1;
        private TextBox textBox1;
        private Button button2;
        private Button button3;
        private Label label3;
        private Label label4;
        private Button button4;
        private Label label5;
        private TextBox textBox2;
    }
}