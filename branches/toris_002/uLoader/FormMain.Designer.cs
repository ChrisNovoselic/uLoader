using HClassLibrary;

namespace uLoader
{
    partial class FormMain
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.MainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.файлКонфигурацияToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.файлКонфигурацияЗагрузитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.файлКонфигурацияСохранитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.видToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.работаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.конфигурацияToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.помощьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.оПрограммеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.m_notifyIcon = new System.Windows.Forms.NotifyIcon ();
            this.m_notifyIcon.Click += new System.EventHandler(NotifyIcon_Click);

            this.MainMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_menuStripMain
            // 
            this.MainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.файлToolStripMenuItem,
                this.видToolStripMenuItem,
                this.помощьToolStripMenuItem
            });
            this.MainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MainMenuStrip.Name = "m_menuStripMain";
            this.MainMenuStrip.Size = new System.Drawing.Size(792, 24);
            this.MainMenuStrip.TabIndex = 0;
            this.MainMenuStrip.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.файлКонфигурацияToolStripMenuItem
                , new System.Windows.Forms.ToolStripSeparator()
                , this.выходToolStripMenuItem
            });
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // файлфайлКонфигурацияToolStripMenuItem
            // 
            this.файлКонфигурацияToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.файлКонфигурацияЗагрузитьToolStripMenuItem
                , this.файлКонфигурацияСохранитьToolStripMenuItem
            });
            this.файлКонфигурацияToolStripMenuItem.Name = "файлКонфигурацияToolStripMenuItem";
            this.файлКонфигурацияToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.файлКонфигурацияToolStripMenuItem.Text = "Конфигурация";
            this.файлКонфигурацияToolStripMenuItem.Enabled = false;
            // 
            // файлКонфигурацияЗагрузитьToolStripMenuItem
            // 
            this.файлКонфигурацияЗагрузитьToolStripMenuItem.Name = "файлКонфигурацияЗагрузитьToolStripMenuItem";
            this.файлКонфигурацияЗагрузитьToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.файлКонфигурацияЗагрузитьToolStripMenuItem.Text = "Загрузить";
            this.файлКонфигурацияЗагрузитьToolStripMenuItem.Click += new System.EventHandler(this.файлКонфигурацияЗагрузитьToolStripMenuItem_Click);
            // 
            // файлКонфигурацияСохранитьToolStripMenuItem
            // 
            this.файлКонфигурацияСохранитьToolStripMenuItem.Name = "файлКонфигурацияСохранитьToolStripMenuItem";
            this.файлКонфигурацияСохранитьToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.файлКонфигурацияСохранитьToolStripMenuItem.Text = "Сохранить";
            this.файлКонфигурацияСохранитьToolStripMenuItem.Click += new System.EventHandler(this.файлКонфигурацияСохранитьToolStripMenuItem_Click);
            // 
            // выходToolStripMenuItem
            // 
            this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.выходToolStripMenuItem.Text = "Выход";
            this.выходToolStripMenuItem.Click += new System.EventHandler(this.выходToolStripMenuItem_Click);
            // 
            // видToolStripMenuItem
            // 
            this.видToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.работаToolStripMenuItem,
            this.конфигурацияToolStripMenuItem});
            this.видToolStripMenuItem.Name = "видToolStripMenuItem";
            this.видToolStripMenuItem.Size = new System.Drawing.Size(38, 20);
            this.видToolStripMenuItem.Text = "Вид";
            // 
            // работаToolStripMenuItem
            // 
            this.работаToolStripMenuItem.Checked = true;
            this.работаToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.работаToolStripMenuItem.Enabled = false;
            this.работаToolStripMenuItem.Name = "работаToolStripMenuItem";
            this.работаToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.работаToolStripMenuItem.Text = "Работа";
            // 
            // конфигурацияToolStripMenuItem
            // 
            this.конфигурацияToolStripMenuItem.Name = "конфигурацияToolStripMenuItem";
            this.конфигурацияToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.конфигурацияToolStripMenuItem.Text = "Конфигурация";
            // 
            // помощьToolStripMenuItem
            // 
            this.помощьToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.оПрограммеToolStripMenuItem});
            this.помощьToolStripMenuItem.Name = "помощьToolStripMenuItem";
            this.помощьToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.помощьToolStripMenuItem.Text = "Помощь";
            // 
            // оПрограммеToolStripMenuItem
            // 
            this.оПрограммеToolStripMenuItem.Name = "оПрограммеToolStripMenuItem";
            this.оПрограммеToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.оПрограммеToolStripMenuItem.Text = "О программе";
            this.оПрограммеToolStripMenuItem.Click += new System.EventHandler(this.оПрограммеToolStripMenuItem_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 640);
            this.Controls.Add(this.MainMenuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Загрузчик данных (универсальный)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.MainMenuStrip.ResumeLayout(false);
            this.MainMenuStrip.PerformLayout();

            //
            // m_TabCtrl
            //
            m_TabCtrl = new HClassLibrary.HTabCtrlEx();
            this.m_TabCtrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right))
            );
            this.m_TabCtrl.Location = new System.Drawing.Point(0, MainMenuStrip.ClientSize.Height);
            this.m_TabCtrl.Name = "tabCtrl";
            this.m_TabCtrl.SelectedIndex = 0;
            this.m_TabCtrl.Size = new System.Drawing.Size(this.ClientSize.Width, this.ClientSize.Height - MainMenuStrip.ClientSize.Height - 0/*m_statusStripMain.ClientSize.Height*/);
            this.m_TabCtrl.TabIndex = 3;
            this.m_TabCtrl.SelectedIndexChanged += new System.EventHandler(this.TabCtrl_OnSelectedIndexChanged);
            this.m_TabCtrl.EventPrevSelectedIndexChanged += new DelegateIntFunc(this.TabCtrl_OnPrevSelectedIndexChanged);
            this.Controls.Add(this.m_TabCtrl);

            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private HTabCtrlEx m_TabCtrl;

        private System.Windows.Forms.NotifyIcon m_notifyIcon;

        //private System.Windows.Forms.MenuStrip m_menuStripMain;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem файлКонфигурацияToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem файлКонфигурацияЗагрузитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem файлКонфигурацияСохранитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem видToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem работаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem конфигурацияToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem помощьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem оПрограммеToolStripMenuItem;
    }
}

