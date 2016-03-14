namespace uLoader
{
    partial class FormAbout
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAbout));
            this.m_btnOk = new System.Windows.Forms.Button();
            this.m_pictureBoxIconMain = new System.Windows.Forms.PictureBox();
            this.m_labelProductVersion = new System.Windows.Forms.Label();
            this.m_labelZ = new System.Windows.Forms.Label();
            this.labelR = new System.Windows.Forms.Label();
            this.linkLabelR = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.m_pictureBoxIconMain)).BeginInit();
            this.SuspendLayout();
            // 
            // m_btnOk
            // 
            this.m_btnOk.Location = new System.Drawing.Point(125, 75);
            this.m_btnOk.Name = "m_btnOk";
            this.m_btnOk.Size = new System.Drawing.Size(75, 23);
            this.m_btnOk.TabIndex = 0;
            this.m_btnOk.Text = "Ок";
            this.m_btnOk.UseVisualStyleBackColor = true;
            this.m_btnOk.Click += new System.EventHandler(this.m_btnOk_Click);
            // 
            // m_pictureBoxIconMain
            // 
            this.m_pictureBoxIconMain.Location = new System.Drawing.Point(6, 6);
            this.m_pictureBoxIconMain.Name = "m_pictureBoxIconMain";
            this.m_pictureBoxIconMain.Size = new System.Drawing.Size(64, 64);
            this.m_pictureBoxIconMain.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.m_pictureBoxIconMain.TabIndex = 1;
            this.m_pictureBoxIconMain.TabStop = false;
            // 
            // m_labelProductVersion
            // 
            this.m_labelProductVersion.AutoSize = true;
            this.m_labelProductVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_labelProductVersion.Location = new System.Drawing.Point(79, 9);
            this.m_labelProductVersion.Name = "m_labelProductVersion";
            this.m_labelProductVersion.Size = new System.Drawing.Size(105, 13);
            this.m_labelProductVersion.TabIndex = 2;
            this.m_labelProductVersion.Text = "ProductVersion...";
            // 
            // m_labelZ
            // 
            this.m_labelZ.AutoSize = true;
            this.m_labelZ.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_labelZ.Location = new System.Drawing.Point(78, 35);
            this.m_labelZ.Name = "m_labelZ";
            this.m_labelZ.Size = new System.Drawing.Size(167, 13);
            this.m_labelZ.TabIndex = 3;
            this.m_labelZ.Text = "Заказчик: \"Генрация\" (НТЭЦ-5)";
            // 
            // labelR
            // 
            this.labelR.AutoSize = true;
            this.labelR.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelR.Location = new System.Drawing.Point(81, 54);
            this.labelR.Name = "labelR";
            this.labelR.Size = new System.Drawing.Size(139, 13);
            this.labelR.TabIndex = 4;
            this.labelR.Text = "Разработчик: Хряпин А.Н.";
            // 
            // linkLabelR
            // 
            this.linkLabelR.AutoSize = true;
            this.linkLabelR.Location = new System.Drawing.Point(218, 52);
            this.linkLabelR.Name = "linkLabelR";
            this.linkLabelR.Size = new System.Drawing.Size(98, 13);
            this.linkLabelR.TabIndex = 5;
            this.linkLabelR.TabStop = true;
            this.linkLabelR.Text = "ChrjapinAN@itss.ru";
            // 
            // FormAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 105);
            this.Controls.Add(this.linkLabelR);
            this.Controls.Add(this.labelR);
            this.Controls.Add(this.m_labelZ);
            this.Controls.Add(this.m_labelProductVersion);
            this.Controls.Add(this.m_pictureBoxIconMain);
            this.Controls.Add(this.m_btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAbout";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "О программе...";
            this.Load += new System.EventHandler(this.FormAbout_Load);
            ((System.ComponentModel.ISupportInitialize)(this.m_pictureBoxIconMain)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button m_btnOk;
        private System.Windows.Forms.PictureBox m_pictureBoxIconMain;
        private System.Windows.Forms.Label m_labelProductVersion;
        private System.Windows.Forms.Label m_labelZ;
        private System.Windows.Forms.Label labelR;
        private System.Windows.Forms.LinkLabel linkLabelR;
    }
}