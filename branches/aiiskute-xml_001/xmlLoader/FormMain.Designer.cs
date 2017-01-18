namespace xmlLoader
{
    partial class FormMain
    {
        /// <summary>
        /// Обязательная переменная конструктора.
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
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.m_statusStripMain = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelEventName = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelEventDateTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelEventDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_menuStripMain = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сединениепортToolStripMenuItem = new System.Windows.Forms.ToolStripTextBox();
            this.файлToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.оПрограммеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.splitContainerMainRead = new System.Windows.Forms.SplitContainer();
            this.m_groupBoxListPackage = new System.Windows.Forms.GroupBox();
            this.m_btnLoadHistory = new System.Windows.Forms.Button();
            this.m_dateTimePickerHistory = new System.Windows.Forms.DateTimePicker();
            this.m_nudnSettingHistoryDepth = new System.Windows.Forms.NumericUpDown();
            this.m_cbxSettingHistoryIssue = new System.Windows.Forms.CheckBox();
            this.m_dgvListPackage = new System.Windows.Forms.DataGridView();
            this.m_groupBoxStatistic = new System.Windows.Forms.GroupBox();
            this.m_dgvStatistic = new System.Windows.Forms.DataGridView();
            this.ColumnParameter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.m_tabControlViewPackage = new System.Windows.Forms.TabControl();
            this.m_tpageViewPackageXml = new System.Windows.Forms.TabPage();
            this.m_labelViewPackage = new System.Windows.Forms.Label();
            this.m_tpageViewPackageTree = new System.Windows.Forms.TabPage();
            this.m_treeViewPackage = new System.Windows.Forms.TreeView();
            this.splitContainerMainWrite = new System.Windows.Forms.SplitContainer();
            this.m_dateTimePickerFilterEnd = new System.Windows.Forms.DateTimePicker();
            this.m_dateTimePickerFilterStart = new System.Windows.Forms.DateTimePicker();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.m_statusStripMain.SuspendLayout();
            this.m_menuStripMain.SuspendLayout();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.splitContainerMainRead.Panel1.SuspendLayout();
            this.splitContainerMainRead.Panel2.SuspendLayout();
            this.splitContainerMainRead.SuspendLayout();
            this.m_groupBoxListPackage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_nudnSettingHistoryDepth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvListPackage)).BeginInit();
            this.m_groupBoxStatistic.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvStatistic)).BeginInit();
            this.m_tabControlViewPackage.SuspendLayout();
            this.m_tpageViewPackageXml.SuspendLayout();
            this.m_tpageViewPackageTree.SuspendLayout();
            this.splitContainerMainWrite.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_statusStripMain
            // 
            this.m_statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelEventName,
            this.toolStripStatusLabelEventDateTime,
            this.toolStripStatusLabelEventDesc});
            this.m_statusStripMain.Location = new System.Drawing.Point(0, 694);
            this.m_statusStripMain.Name = "m_statusStripMain";
            this.m_statusStripMain.Size = new System.Drawing.Size(861, 22);
            this.m_statusStripMain.TabIndex = 0;
            this.m_statusStripMain.Text = "statusStrip1";
            // 
            // toolStripStatusLabelEventName
            // 
            this.toolStripStatusLabelEventName.AutoSize = false;
            this.toolStripStatusLabelEventName.Name = "toolStripStatusLabelEventName";
            this.toolStripStatusLabelEventName.Size = new System.Drawing.Size(120, 17);
            this.toolStripStatusLabelEventName.Text = "Событие";
            this.toolStripStatusLabelEventName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabelEventDateTime
            // 
            this.toolStripStatusLabelEventDateTime.AutoSize = false;
            this.toolStripStatusLabelEventDateTime.Name = "toolStripStatusLabelEventDateTime";
            this.toolStripStatusLabelEventDateTime.Size = new System.Drawing.Size(120, 17);
            this.toolStripStatusLabelEventDateTime.Text = "Дата/время";
            this.toolStripStatusLabelEventDateTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabelEventDesc
            // 
            this.toolStripStatusLabelEventDesc.Name = "toolStripStatusLabelEventDesc";
            this.toolStripStatusLabelEventDesc.Size = new System.Drawing.Size(606, 17);
            this.toolStripStatusLabelEventDesc.Spring = true;
            this.toolStripStatusLabelEventDesc.Text = "Описание события";
            this.toolStripStatusLabelEventDesc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // m_menuStripMain
            // 
            this.m_menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem,
            this.оПрограммеToolStripMenuItem});
            this.m_menuStripMain.Location = new System.Drawing.Point(0, 0);
            this.m_menuStripMain.Name = "m_menuStripMain";
            this.m_menuStripMain.Size = new System.Drawing.Size(861, 24);
            this.m_menuStripMain.TabIndex = 1;
            this.m_menuStripMain.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.сединениепортToolStripMenuItem,
            this.файлToolStripSeparator1,
            this.выходToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // сединениепортToolStripMenuItem
            // 
            this.сединениепортToolStripMenuItem.AcceptsReturn = true;
            this.сединениепортToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.сединениепортToolStripMenuItem.Name = "сединениепортToolStripMenuItem";
            this.сединениепортToolStripMenuItem.Size = new System.Drawing.Size(162, 21);
            this.сединениепортToolStripMenuItem.Text = "Сединение(порт)";
            // 
            // файлToolStripSeparator1
            // 
            this.файлToolStripSeparator1.Name = "файлToolStripSeparator1";
            this.файлToolStripSeparator1.Size = new System.Drawing.Size(219, 6);
            // 
            // выходToolStripMenuItem
            // 
            this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.выходToolStripMenuItem.Text = "Выход";
            this.выходToolStripMenuItem.Click += new System.EventHandler(this.выходToolStripMenuItem_Click);
            // 
            // оПрограммеToolStripMenuItem
            // 
            this.оПрограммеToolStripMenuItem.Name = "оПрограммеToolStripMenuItem";
            this.оПрограммеToolStripMenuItem.Size = new System.Drawing.Size(83, 20);
            this.оПрограммеToolStripMenuItem.Text = "О программе";
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 24);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.splitContainerMainRead);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.splitContainerMainWrite);
            this.splitContainerMain.Size = new System.Drawing.Size(861, 670);
            this.splitContainerMain.SplitterDistance = 345;
            this.splitContainerMain.TabIndex = 2;
            // 
            // splitContainerMainRead
            // 
            this.splitContainerMainRead.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMainRead.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMainRead.Name = "splitContainerMainRead";
            // 
            // splitContainerMainRead.Panel1
            // 
            this.splitContainerMainRead.Panel1.Controls.Add(this.m_groupBoxListPackage);
            this.splitContainerMainRead.Panel1.Controls.Add(this.m_groupBoxStatistic);
            // 
            // splitContainerMainRead.Panel2
            // 
            this.splitContainerMainRead.Panel2.Controls.Add(this.m_tabControlViewPackage);
            this.splitContainerMainRead.Size = new System.Drawing.Size(861, 345);
            this.splitContainerMainRead.SplitterDistance = 287;
            this.splitContainerMainRead.TabIndex = 0;
            // 
            // m_groupBoxListPackage
            // 
            this.m_groupBoxListPackage.Controls.Add(this.checkBox1);
            this.m_groupBoxListPackage.Controls.Add(this.button1);
            this.m_groupBoxListPackage.Controls.Add(this.m_dateTimePickerFilterStart);
            this.m_groupBoxListPackage.Controls.Add(this.m_dateTimePickerFilterEnd);
            this.m_groupBoxListPackage.Controls.Add(this.m_btnLoadHistory);
            this.m_groupBoxListPackage.Controls.Add(this.m_dateTimePickerHistory);
            this.m_groupBoxListPackage.Controls.Add(this.m_nudnSettingHistoryDepth);
            this.m_groupBoxListPackage.Controls.Add(this.m_cbxSettingHistoryIssue);
            this.m_groupBoxListPackage.Controls.Add(this.m_dgvListPackage);
            this.m_groupBoxListPackage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_groupBoxListPackage.Location = new System.Drawing.Point(0, 100);
            this.m_groupBoxListPackage.Name = "m_groupBoxListPackage";
            this.m_groupBoxListPackage.Size = new System.Drawing.Size(287, 245);
            this.m_groupBoxListPackage.TabIndex = 1;
            this.m_groupBoxListPackage.TabStop = false;
            this.m_groupBoxListPackage.Text = "Пакеты";
            // 
            // m_btnLoadHistory
            // 
            this.m_btnLoadHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_btnLoadHistory.Location = new System.Drawing.Point(151, 215);
            this.m_btnLoadHistory.Name = "m_btnLoadHistory";
            this.m_btnLoadHistory.Size = new System.Drawing.Size(130, 23);
            this.m_btnLoadHistory.TabIndex = 4;
            this.m_btnLoadHistory.Text = "Загрузить";
            this.m_btnLoadHistory.UseVisualStyleBackColor = true;
            // 
            // m_dateTimePickerHistory
            // 
            this.m_dateTimePickerHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_dateTimePickerHistory.Checked = false;
            this.m_dateTimePickerHistory.Location = new System.Drawing.Point(151, 185);
            this.m_dateTimePickerHistory.Name = "m_dateTimePickerHistory";
            this.m_dateTimePickerHistory.ShowCheckBox = true;
            this.m_dateTimePickerHistory.ShowUpDown = true;
            this.m_dateTimePickerHistory.Size = new System.Drawing.Size(130, 20);
            this.m_dateTimePickerHistory.TabIndex = 3;
            // 
            // m_nudnSettingHistoryDepth
            // 
            this.m_nudnSettingHistoryDepth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_nudnSettingHistoryDepth.Location = new System.Drawing.Point(6, 218);
            this.m_nudnSettingHistoryDepth.Name = "m_nudnSettingHistoryDepth";
            this.m_nudnSettingHistoryDepth.Size = new System.Drawing.Size(130, 20);
            this.m_nudnSettingHistoryDepth.TabIndex = 2;
            this.m_nudnSettingHistoryDepth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // m_cbxSettingHistoryIssue
            // 
            this.m_cbxSettingHistoryIssue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_cbxSettingHistoryIssue.AutoSize = true;
            this.m_cbxSettingHistoryIssue.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.m_cbxSettingHistoryIssue.Location = new System.Drawing.Point(6, 187);
            this.m_cbxSettingHistoryIssue.Name = "m_cbxSettingHistoryIssue";
            this.m_cbxSettingHistoryIssue.Size = new System.Drawing.Size(125, 17);
            this.m_cbxSettingHistoryIssue.TabIndex = 1;
            this.m_cbxSettingHistoryIssue.Text = "Сохранять историю";
            this.m_cbxSettingHistoryIssue.UseVisualStyleBackColor = true;
            // 
            // m_dgvListPackage
            // 
            this.m_dgvListPackage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_dgvListPackage.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_dgvListPackage.Location = new System.Drawing.Point(3, 47);
            this.m_dgvListPackage.Name = "m_dgvListPackage";
            this.m_dgvListPackage.Size = new System.Drawing.Size(281, 97);
            this.m_dgvListPackage.TabIndex = 0;
            // 
            // m_groupBoxStatistic
            // 
            this.m_groupBoxStatistic.Controls.Add(this.m_dgvStatistic);
            this.m_groupBoxStatistic.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_groupBoxStatistic.Location = new System.Drawing.Point(0, 0);
            this.m_groupBoxStatistic.Name = "m_groupBoxStatistic";
            this.m_groupBoxStatistic.Size = new System.Drawing.Size(287, 100);
            this.m_groupBoxStatistic.TabIndex = 0;
            this.m_groupBoxStatistic.TabStop = false;
            this.m_groupBoxStatistic.Text = "Статистика";
            // 
            // m_dgvStatistic
            // 
            this.m_dgvStatistic.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_dgvStatistic.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnParameter,
            this.ColumnValue});
            this.m_dgvStatistic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_dgvStatistic.Location = new System.Drawing.Point(3, 16);
            this.m_dgvStatistic.Name = "m_dgvStatistic";
            this.m_dgvStatistic.Size = new System.Drawing.Size(281, 81);
            this.m_dgvStatistic.TabIndex = 0;
            // 
            // ColumnParameter
            // 
            this.ColumnParameter.Frozen = true;
            this.ColumnParameter.HeaderText = "Параметр";
            this.ColumnParameter.Name = "ColumnParameter";
            this.ColumnParameter.ReadOnly = true;
            // 
            // ColumnValue
            // 
            this.ColumnValue.HeaderText = "Значение";
            this.ColumnValue.Name = "ColumnValue";
            this.ColumnValue.ReadOnly = true;
            // 
            // m_tabControlViewPackage
            // 
            this.m_tabControlViewPackage.Controls.Add(this.m_tpageViewPackageXml);
            this.m_tabControlViewPackage.Controls.Add(this.m_tpageViewPackageTree);
            this.m_tabControlViewPackage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_tabControlViewPackage.Location = new System.Drawing.Point(0, 0);
            this.m_tabControlViewPackage.Name = "m_tabControlViewPackage";
            this.m_tabControlViewPackage.SelectedIndex = 0;
            this.m_tabControlViewPackage.Size = new System.Drawing.Size(570, 345);
            this.m_tabControlViewPackage.TabIndex = 0;
            // 
            // m_tpageViewPackageXml
            // 
            this.m_tpageViewPackageXml.Controls.Add(this.m_labelViewPackage);
            this.m_tpageViewPackageXml.Location = new System.Drawing.Point(4, 22);
            this.m_tpageViewPackageXml.Name = "m_tpageViewPackageXml";
            this.m_tpageViewPackageXml.Padding = new System.Windows.Forms.Padding(3);
            this.m_tpageViewPackageXml.Size = new System.Drawing.Size(562, 319);
            this.m_tpageViewPackageXml.TabIndex = 0;
            this.m_tpageViewPackageXml.Text = "XML";
            this.m_tpageViewPackageXml.UseVisualStyleBackColor = true;
            // 
            // m_labelViewPackage
            // 
            this.m_labelViewPackage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_labelViewPackage.Location = new System.Drawing.Point(3, 3);
            this.m_labelViewPackage.Name = "m_labelViewPackage";
            this.m_labelViewPackage.Size = new System.Drawing.Size(556, 313);
            this.m_labelViewPackage.TabIndex = 0;
            this.m_labelViewPackage.Text = resources.GetString("m_labelViewPackage.Text");
            // 
            // m_tpageViewPackageTree
            // 
            this.m_tpageViewPackageTree.Controls.Add(this.m_treeViewPackage);
            this.m_tpageViewPackageTree.Location = new System.Drawing.Point(4, 22);
            this.m_tpageViewPackageTree.Name = "m_tpageViewPackageTree";
            this.m_tpageViewPackageTree.Padding = new System.Windows.Forms.Padding(3);
            this.m_tpageViewPackageTree.Size = new System.Drawing.Size(562, 319);
            this.m_tpageViewPackageTree.TabIndex = 1;
            this.m_tpageViewPackageTree.Text = "Структура";
            this.m_tpageViewPackageTree.UseVisualStyleBackColor = true;
            // 
            // m_treeViewPackage
            // 
            this.m_treeViewPackage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_treeViewPackage.Location = new System.Drawing.Point(3, 3);
            this.m_treeViewPackage.Name = "m_treeViewPackage";
            this.m_treeViewPackage.Size = new System.Drawing.Size(556, 313);
            this.m_treeViewPackage.TabIndex = 0;
            // 
            // splitContainerMainWrite
            // 
            this.splitContainerMainWrite.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMainWrite.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMainWrite.Name = "splitContainerMainWrite";
            this.splitContainerMainWrite.Size = new System.Drawing.Size(861, 321);
            this.splitContainerMainWrite.SplitterDistance = 292;
            this.splitContainerMainWrite.TabIndex = 0;
            // 
            // m_dateTimePickerFilterEnd
            // 
            this.m_dateTimePickerFilterEnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_dateTimePickerFilterEnd.Checked = false;
            this.m_dateTimePickerFilterEnd.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.m_dateTimePickerFilterEnd.Location = new System.Drawing.Point(151, 19);
            this.m_dateTimePickerFilterEnd.Name = "m_dateTimePickerFilterEnd";
            this.m_dateTimePickerFilterEnd.ShowCheckBox = true;
            this.m_dateTimePickerFilterEnd.ShowUpDown = true;
            this.m_dateTimePickerFilterEnd.Size = new System.Drawing.Size(130, 20);
            this.m_dateTimePickerFilterEnd.TabIndex = 5;
            this.m_dateTimePickerFilterEnd.Value = new System.DateTime(2017, 1, 18, 23, 59, 0, 0);
            // 
            // m_dateTimePickerFilterStart
            // 
            this.m_dateTimePickerFilterStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_dateTimePickerFilterStart.Checked = false;
            this.m_dateTimePickerFilterStart.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.m_dateTimePickerFilterStart.Location = new System.Drawing.Point(6, 19);
            this.m_dateTimePickerFilterStart.Name = "m_dateTimePickerFilterStart";
            this.m_dateTimePickerFilterStart.ShowCheckBox = true;
            this.m_dateTimePickerFilterStart.ShowUpDown = true;
            this.m_dateTimePickerFilterStart.Size = new System.Drawing.Size(130, 20);
            this.m_dateTimePickerFilterStart.TabIndex = 6;
            this.m_dateTimePickerFilterStart.Value = new System.DateTime(2017, 1, 18, 0, 0, 0, 0);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(151, 152);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(130, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Повторить";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox1.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBox1.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox1.Location = new System.Drawing.Point(6, 152);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(130, 23);
            this.checkBox1.TabIndex = 9;
            this.checkBox1.Text = "Сохранять историю";
            this.checkBox1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(861, 716);
            this.Controls.Add(this.splitContainerMain);
            this.Controls.Add(this.m_statusStripMain);
            this.Controls.Add(this.m_menuStripMain);
            this.MainMenuStrip = this.m_menuStripMain;
            this.Name = "FormMain";
            this.Text = "XML-Загрузчик АИИС КУТЭ";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.m_statusStripMain.ResumeLayout(false);
            this.m_statusStripMain.PerformLayout();
            this.m_menuStripMain.ResumeLayout(false);
            this.m_menuStripMain.PerformLayout();
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            this.splitContainerMain.ResumeLayout(false);
            this.splitContainerMainRead.Panel1.ResumeLayout(false);
            this.splitContainerMainRead.Panel2.ResumeLayout(false);
            this.splitContainerMainRead.ResumeLayout(false);
            this.m_groupBoxListPackage.ResumeLayout(false);
            this.m_groupBoxListPackage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_nudnSettingHistoryDepth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvListPackage)).EndInit();
            this.m_groupBoxStatistic.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvStatistic)).EndInit();
            this.m_tabControlViewPackage.ResumeLayout(false);
            this.m_tpageViewPackageXml.ResumeLayout(false);
            this.m_tpageViewPackageTree.ResumeLayout(false);
            this.splitContainerMainWrite.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip m_statusStripMain;
        private System.Windows.Forms.MenuStrip m_menuStripMain;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator файлToolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem оПрограммеToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.SplitContainer splitContainerMainRead;
        private System.Windows.Forms.GroupBox m_groupBoxListPackage;
        private System.Windows.Forms.GroupBox m_groupBoxStatistic;
        private System.Windows.Forms.TabControl m_tabControlViewPackage;
        private System.Windows.Forms.TabPage m_tpageViewPackageXml;
        private System.Windows.Forms.Label m_labelViewPackage;
        private System.Windows.Forms.TabPage m_tpageViewPackageTree;
        private System.Windows.Forms.TreeView m_treeViewPackage;
        private System.Windows.Forms.Button m_btnLoadHistory;
        private System.Windows.Forms.DateTimePicker m_dateTimePickerHistory;
        private System.Windows.Forms.NumericUpDown m_nudnSettingHistoryDepth;
        private System.Windows.Forms.CheckBox m_cbxSettingHistoryIssue;
        private System.Windows.Forms.DataGridView m_dgvListPackage;
        private System.Windows.Forms.DataGridView m_dgvStatistic;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnParameter;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnValue;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEventName;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEventDateTime;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEventDesc;
        private System.Windows.Forms.ToolStripTextBox сединениепортToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainerMainWrite;
        private System.Windows.Forms.DateTimePicker m_dateTimePickerFilterStart;
        private System.Windows.Forms.DateTimePicker m_dateTimePickerFilterEnd;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button1;
    }
}

