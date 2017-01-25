using System.Windows.Forms;

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
            System.Windows.Forms.GroupBox m_groupBoxListPackage;
            System.Windows.Forms.Label m_labelPackageDisplayCount;
            System.Windows.Forms.GroupBox m_groupBoxSession;
            System.Windows.Forms.Label m_labelUDPPort;
            System.Windows.Forms.GroupBox m_groupBoxDestSetting;
            System.Windows.Forms.TabControl m_tabControlDest;
            System.Windows.Forms.GroupBox m_groupBoxDatabase;
            System.Windows.Forms.Label m_labelDestDisplayCount;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.m_nudnPackageDisplayCount = new System.Windows.Forms.NumericUpDown();
            this.m_cbxPackageIndiciesIssue = new System.Windows.Forms.CheckBox();
            this.m_btnPackageReSend = new System.Windows.Forms.Button();
            this.m_dateTimePickerPackageFilterStart = new System.Windows.Forms.DateTimePicker();
            this.m_dateTimePickerPackageFilterEnd = new System.Windows.Forms.DateTimePicker();
            this.m_btnLoadPackageHistory = new System.Windows.Forms.Button();
            this.m_nudnSettingPackageHistoryDepth = new System.Windows.Forms.NumericUpDown();
            this.m_cbxSettingPackageHistoryIssue = new System.Windows.Forms.CheckBox();
            this.m_dgvPackageList = new DataGridView();
            this.ColumnPackageListItemCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnPackageListDatetimeInput = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnPackageListDatetimeOutput = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.m_cbxReadSessionStop = new System.Windows.Forms.CheckBox();
            this.m_nudnUDPPort = new System.Windows.Forms.NumericUpDown();
            this.m_cbxReadSessionStart = new System.Windows.Forms.CheckBox();
            this.m_dgvStatistic = new DataGridViewStatistic();
            this.ColumnSessionParameterValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.m_btnDestRemove = new System.Windows.Forms.Button();
            this.m_btnDestAdd = new System.Windows.Forms.Button();
            this.m_dgvDestList = new System.Windows.Forms.DataGridView();
            this.ColumnDestListName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnDestListState = new DataGridViewPressedButtonColumn();
            this.m_dgvDestSetting = new System.Windows.Forms.DataGridView();
            this.ColumnDestSettingValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.m_tpageDestValue = new System.Windows.Forms.TabPage();
            this.m_dgvDestValue = new System.Windows.Forms.DataGridView();
            this.m_tpageDestParameter = new System.Windows.Forms.TabPage();
            this.m_dgvDestParameter = new System.Windows.Forms.DataGridView();            
            this.m_nudnDestDisplayCount = new System.Windows.Forms.NumericUpDown();
            this.m_dgvDestDatabaseListAction = new System.Windows.Forms.DataGridView();
            this.ColumnDestDatabaseListActionItemCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnDestDatabaseListActionInput = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnDestDatabaseListActionCompleted = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.m_btnDestDatabaseDelete = new System.Windows.Forms.Button();
            this.m_dateTimePickerDestDataBaseFilterStart = new System.Windows.Forms.DateTimePicker();
            this.m_dateTimePickerDestDataBaseFilterStop = new System.Windows.Forms.DateTimePicker();
            this.m_btnDestDatabaseLoad = new System.Windows.Forms.Button();
            this.m_statusStripMain = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelEventName = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelEventDateTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelEventDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_menuStripMain = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.файлToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сервисToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.имитацияпакетодинToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.имитацияпакетциклToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.оПрограммеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.splitContainerMainRead = new System.Windows.Forms.SplitContainer();
            this.m_tabControlViewPackage = new System.Windows.Forms.TabControl();
            this.m_tpageViewPackageXml = new System.Windows.Forms.TabPage();
            this.m_tbxViewPackage = new System.Windows.Forms.TextBox();
            this.m_tpageViewPackageTree = new System.Windows.Forms.TabPage();
            this.m_treeViewPackage = new TreeViewPackage();
            this.m_tpageViewPackageTableValue = new TabPage();
            this.m_dgvViewPackageTableValue = new DataGridView();
            this.splitContainerMainWrite = new System.Windows.Forms.SplitContainer();
            m_groupBoxListPackage = new System.Windows.Forms.GroupBox();
            m_labelPackageDisplayCount = new System.Windows.Forms.Label();
            m_groupBoxSession = new System.Windows.Forms.GroupBox();
            m_labelUDPPort = new System.Windows.Forms.Label();
            m_groupBoxDestSetting = new System.Windows.Forms.GroupBox();
            m_tabControlDest = new System.Windows.Forms.TabControl();
            m_groupBoxDatabase = new System.Windows.Forms.GroupBox();
            m_labelDestDisplayCount = new System.Windows.Forms.Label();
            m_groupBoxListPackage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_nudnPackageDisplayCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_nudnSettingPackageHistoryDepth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvPackageList)).BeginInit();
            m_groupBoxSession.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_nudnUDPPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvStatistic)).BeginInit();
            m_groupBoxDestSetting.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvDestList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvDestSetting)).BeginInit();
            m_tabControlDest.SuspendLayout();
            this.m_tpageDestValue.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvDestValue)).BeginInit();
            this.m_tpageDestParameter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvDestParameter)).BeginInit();            
            m_groupBoxDatabase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_nudnDestDisplayCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvDestDatabaseListAction)).BeginInit();
            this.m_statusStripMain.SuspendLayout();
            this.m_menuStripMain.SuspendLayout();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.splitContainerMainRead.Panel1.SuspendLayout();
            this.splitContainerMainRead.Panel2.SuspendLayout();
            this.splitContainerMainRead.SuspendLayout();
            this.m_tabControlViewPackage.SuspendLayout();
            this.m_tpageViewPackageXml.SuspendLayout();
            this.m_tpageViewPackageTree.SuspendLayout();
            this.m_tpageViewPackageTableValue.SuspendLayout();
            this.splitContainerMainWrite.Panel1.SuspendLayout();
            this.splitContainerMainWrite.Panel2.SuspendLayout();
            this.splitContainerMainWrite.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_groupBoxListPackage
            // 
            m_groupBoxListPackage.Controls.Add(m_labelPackageDisplayCount);
            m_groupBoxListPackage.Controls.Add(this.m_nudnPackageDisplayCount);
            m_groupBoxListPackage.Controls.Add(this.m_cbxPackageIndiciesIssue);
            m_groupBoxListPackage.Controls.Add(this.m_btnPackageReSend);
            m_groupBoxListPackage.Controls.Add(this.m_dateTimePickerPackageFilterStart);
            m_groupBoxListPackage.Controls.Add(this.m_dateTimePickerPackageFilterEnd);
            m_groupBoxListPackage.Controls.Add(this.m_btnLoadPackageHistory);
            m_groupBoxListPackage.Controls.Add(this.m_nudnSettingPackageHistoryDepth);
            m_groupBoxListPackage.Controls.Add(this.m_cbxSettingPackageHistoryIssue);
            m_groupBoxListPackage.Controls.Add(this.m_dgvPackageList);
            m_groupBoxListPackage.Dock = System.Windows.Forms.DockStyle.Fill;
            m_groupBoxListPackage.Location = new System.Drawing.Point(0, 88);
            m_groupBoxListPackage.Name = "m_groupBoxListPackage";
            m_groupBoxListPackage.Size = new System.Drawing.Size(410, 227);
            m_groupBoxListPackage.TabIndex = 1;
            m_groupBoxListPackage.TabStop = false;
            m_groupBoxListPackage.Text = "Пакеты";
            // 
            // m_labelPackageDisplayCount
            // 
            m_labelPackageDisplayCount.AutoSize = true;
            m_labelPackageDisplayCount.Location = new System.Drawing.Point(6, 18);
            m_labelPackageDisplayCount.Name = "m_labelPackageDisplayCount";
            m_labelPackageDisplayCount.Size = new System.Drawing.Size(67, 13);
            m_labelPackageDisplayCount.TabIndex = 12;
            m_labelPackageDisplayCount.Text = "Отобразить";
            m_labelPackageDisplayCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // m_nudnPackageDisplayCount
            // 
            this.m_nudnPackageDisplayCount.Enabled = false;
            this.m_nudnPackageDisplayCount.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.m_nudnPackageDisplayCount.Location = new System.Drawing.Point(91, 16);
            this.m_nudnPackageDisplayCount.Minimum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.m_nudnPackageDisplayCount.Name = "m_nudnPackageDisplayCount";
            this.m_nudnPackageDisplayCount.Size = new System.Drawing.Size(56, 20);
            this.m_nudnPackageDisplayCount.TabIndex = 10;
            this.m_nudnPackageDisplayCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.m_nudnPackageDisplayCount.ThousandsSeparator = true;
            this.m_nudnPackageDisplayCount.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            // 
            // m_cbxPackageIndiciesIssue
            // 
            this.m_cbxPackageIndiciesIssue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_cbxPackageIndiciesIssue.Appearance = System.Windows.Forms.Appearance.Button;
            this.m_cbxPackageIndiciesIssue.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.m_cbxPackageIndiciesIssue.Enabled = false;
            this.m_cbxPackageIndiciesIssue.Location = new System.Drawing.Point(7, 127);
            this.m_cbxPackageIndiciesIssue.Name = "m_cbxPackageIndiciesIssue";
            this.m_cbxPackageIndiciesIssue.Size = new System.Drawing.Size(140, 23);
            this.m_cbxPackageIndiciesIssue.TabIndex = 9;
            this.m_cbxPackageIndiciesIssue.Text = "Выбрать все";
            this.m_cbxPackageIndiciesIssue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.m_cbxPackageIndiciesIssue.UseVisualStyleBackColor = true;
            // 
            // m_btnPackageReSend
            // 
            this.m_btnPackageReSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_btnPackageReSend.Enabled = false;
            this.m_btnPackageReSend.Location = new System.Drawing.Point(7, 156);
            this.m_btnPackageReSend.Name = "m_btnPackageReSend";
            this.m_btnPackageReSend.Size = new System.Drawing.Size(140, 23);
            this.m_btnPackageReSend.TabIndex = 8;
            this.m_btnPackageReSend.Text = "Повторить";
            this.m_btnPackageReSend.UseVisualStyleBackColor = true;
            // 
            // m_dateTimePickerPackageFilterStart
            // 
            this.m_dateTimePickerPackageFilterStart.Checked = false;
            this.m_dateTimePickerPackageFilterStart.CustomFormat = "dd.MM.yy hh:00";
            this.m_dateTimePickerPackageFilterStart.Enabled = false;
            this.m_dateTimePickerPackageFilterStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.m_dateTimePickerPackageFilterStart.Location = new System.Drawing.Point(7, 47);
            this.m_dateTimePickerPackageFilterStart.MinDate = new System.DateTime(2017, 1, 1, 0, 0, 0, 0);
            this.m_dateTimePickerPackageFilterStart.Name = "m_dateTimePickerPackageFilterStart";
            this.m_dateTimePickerPackageFilterStart.ShowCheckBox = true;
            this.m_dateTimePickerPackageFilterStart.ShowUpDown = true;
            this.m_dateTimePickerPackageFilterStart.Size = new System.Drawing.Size(140, 20);
            this.m_dateTimePickerPackageFilterStart.TabIndex = 6;
            this.m_dateTimePickerPackageFilterStart.Value = new System.DateTime(2017, 2, 1, 0, 0, 0, 0);
            // 
            // m_dateTimePickerPackageFilterEnd
            // 
            this.m_dateTimePickerPackageFilterEnd.Checked = false;
            this.m_dateTimePickerPackageFilterEnd.CustomFormat = "dd.MM.yy hh:00";
            this.m_dateTimePickerPackageFilterEnd.Enabled = false;
            this.m_dateTimePickerPackageFilterEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.m_dateTimePickerPackageFilterEnd.Location = new System.Drawing.Point(7, 73);
            this.m_dateTimePickerPackageFilterEnd.MinDate = new System.DateTime(2017, 1, 1, 0, 0, 0, 0);
            this.m_dateTimePickerPackageFilterEnd.Name = "m_dateTimePickerPackageFilterEnd";
            this.m_dateTimePickerPackageFilterEnd.ShowCheckBox = true;
            this.m_dateTimePickerPackageFilterEnd.ShowUpDown = true;
            this.m_dateTimePickerPackageFilterEnd.Size = new System.Drawing.Size(140, 20);
            this.m_dateTimePickerPackageFilterEnd.TabIndex = 5;
            this.m_dateTimePickerPackageFilterEnd.Value = new System.DateTime(2017, 1, 18, 23, 59, 0, 0);
            // 
            // m_btnLoadPackageHistory
            // 
            this.m_btnLoadPackageHistory.Enabled = false;
            this.m_btnLoadPackageHistory.Location = new System.Drawing.Point(7, 99);
            this.m_btnLoadPackageHistory.Name = "m_btnLoadPackageHistory";
            this.m_btnLoadPackageHistory.Size = new System.Drawing.Size(140, 23);
            this.m_btnLoadPackageHistory.TabIndex = 4;
            this.m_btnLoadPackageHistory.Text = "Загрузить";
            this.m_btnLoadPackageHistory.UseVisualStyleBackColor = true;
            // 
            // m_nudnSettingPackageHistoryDepth
            // 
            this.m_nudnSettingPackageHistoryDepth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_nudnSettingPackageHistoryDepth.Enabled = false;
            this.m_nudnSettingPackageHistoryDepth.Location = new System.Drawing.Point(91, 200);
            this.m_nudnSettingPackageHistoryDepth.Name = "m_nudnSettingPackageHistoryDepth";
            this.m_nudnSettingPackageHistoryDepth.Size = new System.Drawing.Size(56, 20);
            this.m_nudnSettingPackageHistoryDepth.TabIndex = 2;
            this.m_nudnSettingPackageHistoryDepth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.m_nudnSettingPackageHistoryDepth.ThousandsSeparator = true;
            // 
            // m_cbxSettingPackageHistoryIssue
            // 
            this.m_cbxSettingPackageHistoryIssue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_cbxSettingPackageHistoryIssue.AutoSize = true;
            this.m_cbxSettingPackageHistoryIssue.Enabled = false;
            this.m_cbxSettingPackageHistoryIssue.Location = new System.Drawing.Point(6, 203);
            this.m_cbxSettingPackageHistoryIssue.Name = "m_cbxSettingPackageHistoryIssue";
            this.m_cbxSettingPackageHistoryIssue.Size = new System.Drawing.Size(69, 17);
            this.m_cbxSettingPackageHistoryIssue.TabIndex = 1;
            this.m_cbxSettingPackageHistoryIssue.Text = "История";
            this.m_cbxSettingPackageHistoryIssue.UseVisualStyleBackColor = true;
            // 
            // m_dgvPackageList
            // 
            this.m_dgvPackageList.AllowUserToAddRows = false;
            this.m_dgvPackageList.AllowUserToDeleteRows = false;
            this.m_dgvPackageList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_dgvPackageList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_dgvPackageList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnPackageListItemCount,
            this.ColumnPackageListDatetimeInput,
            this.ColumnPackageListDatetimeOutput});
            this.m_dgvPackageList.Location = new System.Drawing.Point(150, 16);
            this.m_dgvPackageList.Name = "m_dgvPackageList";
            this.m_dgvPackageList.ReadOnly = true;
            this.m_dgvPackageList.RowHeadersVisible = false;
            this.m_dgvPackageList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            m_dgvPackageList.MultiSelect = false;
            this.m_dgvPackageList.Size = new System.Drawing.Size(257, 204);
            this.m_dgvPackageList.TabIndex = 0;
            // 
            // ColumnPackageListItemCount
            // 
            this.ColumnPackageListItemCount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.ColumnPackageListItemCount.Frozen = true;
            this.ColumnPackageListItemCount.HeaderText = "Элем.";
            this.ColumnPackageListItemCount.Name = "ColumnPackageListItemCount";
            this.ColumnPackageListItemCount.ReadOnly = true;
            this.ColumnPackageListItemCount.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnPackageListItemCount.Width = 62;
            // 
            // ColumnPackageListDatetimeInput
            // 
            //this.ColumnPackageListDatetimeInput.Frozen = true;
            this.ColumnPackageListDatetimeInput.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnPackageListDatetimeInput.HeaderText = "Получен";
            this.ColumnPackageListDatetimeInput.Name = "ColumnPackageListDatetimeInput";
            this.ColumnPackageListDatetimeInput.ReadOnly = true;
            this.ColumnPackageListDatetimeInput.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // ColumnPackageListDatetimeOutput
            // 
            this.ColumnPackageListDatetimeOutput.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnPackageListDatetimeOutput.HeaderText = "Отправлен";
            this.ColumnPackageListDatetimeOutput.Name = "ColumnPackageListDatetimeOutput";
            this.ColumnPackageListDatetimeOutput.ReadOnly = true;
            this.ColumnPackageListDatetimeOutput.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // m_groupBoxSession
            // 
            m_groupBoxSession.Controls.Add(this.m_cbxReadSessionStop);
            m_groupBoxSession.Controls.Add(this.m_nudnUDPPort);
            m_groupBoxSession.Controls.Add(m_labelUDPPort);
            m_groupBoxSession.Controls.Add(this.m_cbxReadSessionStart);
            m_groupBoxSession.Controls.Add(this.m_dgvStatistic);
            m_groupBoxSession.Dock = System.Windows.Forms.DockStyle.Top;
            m_groupBoxSession.Location = new System.Drawing.Point(0, 0);
            m_groupBoxSession.Name = "m_groupBoxSession";
            m_groupBoxSession.Size = new System.Drawing.Size(410, 88);
            m_groupBoxSession.TabIndex = 0;
            m_groupBoxSession.TabStop = false;
            m_groupBoxSession.Text = "Сессия";
            // 
            // m_cbxReadSessionStop
            // 
            this.m_cbxReadSessionStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_cbxReadSessionStop.Appearance = System.Windows.Forms.Appearance.Button;
            this.m_cbxReadSessionStop.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.m_cbxReadSessionStop.Checked = true;
            this.m_cbxReadSessionStop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.m_cbxReadSessionStop.Location = new System.Drawing.Point(7, 61);
            this.m_cbxReadSessionStop.Name = "m_cbxReadSessionStop";
            this.m_cbxReadSessionStop.Size = new System.Drawing.Size(140, 21);
            this.m_cbxReadSessionStop.TabIndex = 12;
            this.m_cbxReadSessionStop.Text = "Стоп";
            this.m_cbxReadSessionStop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.m_cbxReadSessionStop.UseVisualStyleBackColor = true;
            this.m_cbxReadSessionStop.AutoCheck = false;
            this.m_cbxReadSessionStop.Click += new System.EventHandler(this.cbxSession_Click);
            // 
            // m_nudnUDPPort
            // 
            this.m_nudnUDPPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_nudnUDPPort.Location = new System.Drawing.Point(91, 15);
            this.m_nudnUDPPort.Maximum = new decimal(new int[] {
            1100,
            0,
            0,
            0});
            this.m_nudnUDPPort.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.m_nudnUDPPort.Name = "m_nudnUDPPort";
            this.m_nudnUDPPort.Size = new System.Drawing.Size(56, 20);
            this.m_nudnUDPPort.TabIndex = 10;
            this.m_nudnUDPPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.m_nudnUDPPort.Value = new decimal(new int[] {
            1052,
            0,
            0,
            0});
            this.m_nudnUDPPort.Enabled = false;
            // 
            // m_labelUDPPort
            // 
            m_labelUDPPort.AutoSize = true;
            m_labelUDPPort.Location = new System.Drawing.Point(3, 17);
            m_labelUDPPort.Name = "m_labelUDPPort";
            m_labelUDPPort.Size = new System.Drawing.Size(35, 13);
            m_labelUDPPort.TabIndex = 11;
            m_labelUDPPort.Text = "Порт:";
            m_labelUDPPort.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;            
            // 
            // m_cbxReadSessionStart
            // 
            this.m_cbxReadSessionStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_cbxReadSessionStart.Appearance = System.Windows.Forms.Appearance.Button;
            this.m_cbxReadSessionStart.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.m_cbxReadSessionStart.Location = new System.Drawing.Point(7, 40);
            this.m_cbxReadSessionStart.Name = "m_cbxReadSessionStart";
            this.m_cbxReadSessionStart.Size = new System.Drawing.Size(140, 21);
            this.m_cbxReadSessionStart.TabIndex = 10;
            this.m_cbxReadSessionStart.Tag = "INDEX_CONTROL.CBX_READ_SESSION_START";
            this.m_cbxReadSessionStart.Text = "Старт";
            this.m_cbxReadSessionStart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.m_cbxReadSessionStart.UseVisualStyleBackColor = true;
            this.m_cbxReadSessionStart.AutoCheck = false;
            this.m_cbxReadSessionStart.Click += new System.EventHandler(this.cbxSession_Click);
            // 
            // m_dgvSession
            // 
            this.m_dgvStatistic.AllowUserToAddRows = false;
            this.m_dgvStatistic.AllowUserToDeleteRows = false;
            this.m_dgvStatistic.AllowUserToResizeRows = false;
            this.m_dgvStatistic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_dgvStatistic.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_dgvStatistic.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnSessionParameterValue});
            this.m_dgvStatistic.Location = new System.Drawing.Point(150, 13);
            this.m_dgvStatistic.MultiSelect = false;
            this.m_dgvStatistic.Name = "m_dgvStatistic";
            this.m_dgvStatistic.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.m_dgvStatistic.Size = new System.Drawing.Size(257, 69);
            this.m_dgvStatistic.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.RowHeaderSelect;
            this.m_dgvStatistic.TabIndex = 0;
            // 
            // ColumnSessionParameterValue
            // 
            this.ColumnSessionParameterValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnSessionParameterValue.HeaderText = "Значение";
            this.ColumnSessionParameterValue.Name = "ColumnSessionParameterValue";
            this.ColumnSessionParameterValue.ReadOnly = true;
            // 
            // m_groupBoxDestSetting
            // 
            m_groupBoxDestSetting.Controls.Add(this.m_btnDestRemove);
            m_groupBoxDestSetting.Controls.Add(this.m_btnDestAdd);
            m_groupBoxDestSetting.Controls.Add(this.m_dgvDestList);
            m_groupBoxDestSetting.Controls.Add(this.m_dgvDestSetting);
            m_groupBoxDestSetting.Dock = System.Windows.Forms.DockStyle.Top;
            m_groupBoxDestSetting.Location = new System.Drawing.Point(0, 0);
            m_groupBoxDestSetting.Name = "m_groupBoxDestSetting";
            m_groupBoxDestSetting.Size = new System.Drawing.Size(410, 126);
            m_groupBoxDestSetting.TabIndex = 0;
            m_groupBoxDestSetting.TabStop = false;
            m_groupBoxDestSetting.Text = "Источники данных";
            // 
            // m_btnDestRemove
            // 
            this.m_btnDestRemove.Enabled = false;
            this.m_btnDestRemove.Location = new System.Drawing.Point(78, 96);
            this.m_btnDestRemove.Name = "m_btnDestRemove";
            this.m_btnDestRemove.Size = new System.Drawing.Size(69, 23);
            this.m_btnDestRemove.TabIndex = 10;
            this.m_btnDestRemove.Text = "Удалить";
            this.m_btnDestRemove.UseVisualStyleBackColor = true;
            // 
            // m_btnDestAdd
            // 
            this.m_btnDestAdd.Enabled = false;
            this.m_btnDestAdd.Location = new System.Drawing.Point(7, 96);
            this.m_btnDestAdd.Name = "m_btnDestAdd";
            this.m_btnDestAdd.Size = new System.Drawing.Size(69, 23);
            this.m_btnDestAdd.TabIndex = 9;
            this.m_btnDestAdd.Text = "Добавить";
            this.m_btnDestAdd.UseVisualStyleBackColor = true;
            // 
            // m_dgvDestList
            // 
            this.m_dgvDestList.AllowUserToAddRows = false;
            this.m_dgvDestList.AllowUserToDeleteRows = false;
            this.m_dgvDestList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_dgvDestList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_dgvDestList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnDestListName,
            this.ColumnDestListState});
            this.m_dgvDestList.Location = new System.Drawing.Point(7, 19);
            this.m_dgvDestList.MultiSelect = false;
            this.m_dgvDestList.Name = "m_dgvDestList";
            this.m_dgvDestList.RowHeadersVisible = false;
            this.m_dgvDestList.Size = new System.Drawing.Size(140, 71);
            this.m_dgvDestList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.RowHeaderSelect;
            this.m_dgvDestList.TabIndex = 1;
            // 
            // ColumnDestListName
            // 
            this.ColumnDestListName.Frozen = true;
            this.ColumnDestListName.HeaderText = "Наим.";
            this.ColumnDestListName.Name = "ColumnDestListName";
            // 
            // ColumnDestListState
            // 
            this.ColumnDestListState.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnDestListState.HeaderText = "Стат.";
            this.ColumnDestListState.Name = "ColumnDestListState";
            this.ColumnDestListState.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnDestListState.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // m_dgvDestSetting
            // 
            this.m_dgvDestSetting.AllowUserToAddRows = false;
            this.m_dgvDestSetting.AllowUserToDeleteRows = false;
            this.m_dgvDestSetting.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_dgvDestSetting.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_dgvDestSetting.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnDestSettingValue});
            this.m_dgvDestSetting.Location = new System.Drawing.Point(150, 19);
            this.m_dgvDestSetting.MultiSelect = false;
            this.m_dgvDestSetting.Name = "m_dgvDestSetting";
            this.m_dgvDestSetting.ReadOnly = true;
            this.m_dgvDestSetting.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.m_dgvDestSetting.AllowUserToResizeRows = false;
            this.m_dgvDestSetting.Size = new System.Drawing.Size(257, 101);
            this.m_dgvDestSetting.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.RowHeaderSelect;
            this.m_dgvDestSetting.TabIndex = 0;
            // 
            // ColumnDestSettingValue
            // 
            this.ColumnDestSettingValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnDestSettingValue.HeaderText = "Значение";
            this.ColumnDestSettingValue.Name = "ColumnDestSettingValue";
            this.ColumnDestSettingValue.ReadOnly = true;
            // 
            // m_tabControlDest
            // 
            m_tabControlDest.Controls.Add(this.m_tpageDestValue);
            m_tabControlDest.Controls.Add(this.m_tpageDestParameter);            
            m_tabControlDest.Dock = System.Windows.Forms.DockStyle.Fill;
            m_tabControlDest.Location = new System.Drawing.Point(0, 0);
            m_tabControlDest.Name = "m_tabControlDest";
            m_tabControlDest.SelectedIndex = 0;
            m_tabControlDest.Size = new System.Drawing.Size(447, 351);
            m_tabControlDest.TabIndex = 0;
            m_tabControlDest.Selecting += tabControl_Selecting;
            // 
            // m_tpageDestValue
            // 
            this.m_tpageDestValue.Controls.Add(this.m_dgvDestValue);
            this.m_tpageDestValue.Location = new System.Drawing.Point(4, 22);
            this.m_tpageDestValue.Name = "m_tpageDestValue";
            this.m_tpageDestValue.Padding = new System.Windows.Forms.Padding(3);
            this.m_tpageDestValue.Size = new System.Drawing.Size(439, 325);
            this.m_tpageDestValue.TabIndex = 1;
            this.m_tpageDestValue.Text = "Значения";
            this.m_tpageDestValue.UseVisualStyleBackColor = true;
            // 
            // m_dgvDestValue
            // 
            this.m_dgvDestValue.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_dgvDestValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_dgvDestValue.Location = new System.Drawing.Point(3, 3);
            this.m_dgvDestValue.Name = "m_dgvDestValue";
            this.m_dgvDestValue.Size = new System.Drawing.Size(433, 319);
            this.m_dgvDestValue.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.RowHeaderSelect;
            this.m_dgvDestValue.TabIndex = 0;
            // 
            // m_tpageDestParameter
            // 
            this.m_tpageDestParameter.Controls.Add(this.m_dgvDestParameter);
            this.m_tpageDestParameter.Location = new System.Drawing.Point(4, 22);
            this.m_tpageDestParameter.Name = "m_tpageDestParameter";
            this.m_tpageDestParameter.Padding = new System.Windows.Forms.Padding(3);
            this.m_tpageDestParameter.Size = new System.Drawing.Size(439, 325);
            this.m_tpageDestParameter.TabIndex = 0;
            this.m_tpageDestParameter.Text = "Параметры";
            this.m_tpageDestParameter.UseVisualStyleBackColor = true;
            ((Control)this.m_tpageDestParameter).Enabled = false;
            // 
            // m_dgvDestParameter
            // 
            this.m_dgvDestParameter.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_dgvDestParameter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_dgvDestParameter.Location = new System.Drawing.Point(3, 3);
            this.m_dgvDestParameter.Name = "m_dgvDestParameter";
            this.m_dgvDestParameter.Size = new System.Drawing.Size(433, 319);
            this.m_dgvDestParameter.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.RowHeaderSelect;
            this.m_dgvDestParameter.TabIndex = 0;            
            // 
            // m_groupBoxDatabase
            // 
            m_groupBoxDatabase.Controls.Add(m_labelDestDisplayCount);
            m_groupBoxDatabase.Controls.Add(this.m_nudnDestDisplayCount);
            m_groupBoxDatabase.Controls.Add(this.m_dgvDestDatabaseListAction);
            m_groupBoxDatabase.Controls.Add(this.m_btnDestDatabaseDelete);
            m_groupBoxDatabase.Controls.Add(this.m_dateTimePickerDestDataBaseFilterStart);
            m_groupBoxDatabase.Controls.Add(this.m_dateTimePickerDestDataBaseFilterStop);
            m_groupBoxDatabase.Controls.Add(this.m_btnDestDatabaseLoad);
            m_groupBoxDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            m_groupBoxDatabase.Location = new System.Drawing.Point(0, 126);
            m_groupBoxDatabase.Name = "m_groupBoxDatabase";
            m_groupBoxDatabase.Size = new System.Drawing.Size(410, 225);
            m_groupBoxDatabase.TabIndex = 1;
            m_groupBoxDatabase.TabStop = false;
            m_groupBoxDatabase.Text = "База данных";
            // 
            // m_labelDestDisplayCount
            // 
            m_labelDestDisplayCount.AutoSize = true;
            m_labelDestDisplayCount.Location = new System.Drawing.Point(8, 20);
            m_labelDestDisplayCount.Name = "m_labelDestDisplayCount";
            m_labelDestDisplayCount.Size = new System.Drawing.Size(67, 13);
            m_labelDestDisplayCount.TabIndex = 14;
            m_labelDestDisplayCount.Text = "Отобразить";
            m_labelDestDisplayCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // m_nudnDestDisplayCount
            // 
            this.m_nudnDestDisplayCount.Enabled = false;
            this.m_nudnDestDisplayCount.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.m_nudnDestDisplayCount.Location = new System.Drawing.Point(93, 18);
            this.m_nudnDestDisplayCount.Minimum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.m_nudnDestDisplayCount.Name = "m_nudnDestDisplayCount";
            this.m_nudnDestDisplayCount.Size = new System.Drawing.Size(56, 20);
            this.m_nudnDestDisplayCount.TabIndex = 13;
            this.m_nudnDestDisplayCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.m_nudnDestDisplayCount.ThousandsSeparator = true;
            this.m_nudnDestDisplayCount.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            // 
            // m_dgvDestDatabaseListAction
            // 
            this.m_dgvDestDatabaseListAction.AllowUserToAddRows = false;
            this.m_dgvDestDatabaseListAction.AllowUserToDeleteRows = false;
            this.m_dgvDestDatabaseListAction.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_dgvDestDatabaseListAction.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_dgvDestDatabaseListAction.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnDestDatabaseListActionItemCount,
            this.ColumnDestDatabaseListActionInput,
            this.ColumnDestDatabaseListActionCompleted});
            this.m_dgvDestDatabaseListAction.Location = new System.Drawing.Point(153, 18);
            this.m_dgvDestDatabaseListAction.Name = "m_dgvDestDatabaseListAction";
            this.m_dgvDestDatabaseListAction.ReadOnly = true;
            this.m_dgvDestDatabaseListAction.RowHeadersVisible = false;
            this.m_dgvDestDatabaseListAction.Size = new System.Drawing.Size(257, 204);
            this.m_dgvDestDatabaseListAction.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.m_dgvDestDatabaseListAction.TabIndex = 11;
            // 
            // ColumnDestDatabaseListActionItemCount
            // 
            this.ColumnDestDatabaseListActionItemCount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.ColumnDestDatabaseListActionItemCount.Frozen = true;
            this.ColumnDestDatabaseListActionItemCount.HeaderText = "Элем.";
            this.ColumnDestDatabaseListActionItemCount.Name = "ColumnDestDatabaseListActionItemCount";
            this.ColumnDestDatabaseListActionItemCount.ReadOnly = true;
            this.ColumnDestDatabaseListActionItemCount.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnDestDatabaseListActionItemCount.Width = 62;
            // 
            // ColumnDestDatabaseListActionInput
            // 
            //this.ColumnDestDatabaseListActionInput.Frozen = true;
            this.ColumnDestDatabaseListActionInput.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnDestDatabaseListActionInput.HeaderText = "Получен";
            this.ColumnDestDatabaseListActionInput.Name = "ColumnDestDatabaseListActionInput";
            this.ColumnDestDatabaseListActionInput.ReadOnly = true;
            this.ColumnDestDatabaseListActionInput.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // ColumnDestDatabaseListActionCompleted
            // 
            this.ColumnDestDatabaseListActionCompleted.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnDestDatabaseListActionCompleted.HeaderText = "Обработан";
            this.ColumnDestDatabaseListActionCompleted.Name = "ColumnDestDatabaseListActionCompleted";
            this.ColumnDestDatabaseListActionCompleted.ReadOnly = true;
            this.ColumnDestDatabaseListActionCompleted.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // m_btnDestDatabaseDelete
            // 
            this.m_btnDestDatabaseDelete.Enabled = false;
            this.m_btnDestDatabaseDelete.Location = new System.Drawing.Point(6, 129);
            this.m_btnDestDatabaseDelete.Name = "m_btnDestDatabaseDelete";
            this.m_btnDestDatabaseDelete.Size = new System.Drawing.Size(140, 23);
            this.m_btnDestDatabaseDelete.TabIndex = 10;
            this.m_btnDestDatabaseDelete.Text = "Удалить";
            this.m_btnDestDatabaseDelete.UseVisualStyleBackColor = true;
            // 
            // m_dateTimePickerDestDataBaseFilterStart
            // 
            this.m_dateTimePickerDestDataBaseFilterStart.Checked = false;
            this.m_dateTimePickerDestDataBaseFilterStart.CustomFormat = "dd.MM.yy hh:00";
            this.m_dateTimePickerDestDataBaseFilterStart.Enabled = false;
            this.m_dateTimePickerDestDataBaseFilterStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.m_dateTimePickerDestDataBaseFilterStart.Location = new System.Drawing.Point(7, 49);
            this.m_dateTimePickerDestDataBaseFilterStart.MinDate = new System.DateTime(2017, 1, 1, 0, 0, 0, 0);
            this.m_dateTimePickerDestDataBaseFilterStart.Name = "m_dateTimePickerDestDataBaseFilterStart";
            this.m_dateTimePickerDestDataBaseFilterStart.ShowCheckBox = true;
            this.m_dateTimePickerDestDataBaseFilterStart.ShowUpDown = true;
            this.m_dateTimePickerDestDataBaseFilterStart.Size = new System.Drawing.Size(140, 20);
            this.m_dateTimePickerDestDataBaseFilterStart.TabIndex = 9;
            this.m_dateTimePickerDestDataBaseFilterStart.Value = new System.DateTime(2017, 2, 1, 0, 0, 0, 0);
            // 
            // m_dateTimePickerDestDataBaseFilterStop
            // 
            this.m_dateTimePickerDestDataBaseFilterStop.Checked = false;
            this.m_dateTimePickerDestDataBaseFilterStop.CustomFormat = "dd.MM.yy hh:00";
            this.m_dateTimePickerDestDataBaseFilterStop.Enabled = false;
            this.m_dateTimePickerDestDataBaseFilterStop.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.m_dateTimePickerDestDataBaseFilterStop.Location = new System.Drawing.Point(7, 75);
            this.m_dateTimePickerDestDataBaseFilterStop.MinDate = new System.DateTime(2017, 1, 1, 0, 0, 0, 0);
            this.m_dateTimePickerDestDataBaseFilterStop.Name = "m_dateTimePickerDestDataBaseFilterStop";
            this.m_dateTimePickerDestDataBaseFilterStop.ShowCheckBox = true;
            this.m_dateTimePickerDestDataBaseFilterStop.ShowUpDown = true;
            this.m_dateTimePickerDestDataBaseFilterStop.Size = new System.Drawing.Size(140, 20);
            this.m_dateTimePickerDestDataBaseFilterStop.TabIndex = 8;
            this.m_dateTimePickerDestDataBaseFilterStop.Value = new System.DateTime(2017, 1, 18, 23, 59, 0, 0);
            // 
            // m_btnDestDatabaseLoad
            // 
            this.m_btnDestDatabaseLoad.Enabled = false;
            this.m_btnDestDatabaseLoad.Location = new System.Drawing.Point(7, 101);
            this.m_btnDestDatabaseLoad.Name = "m_btnDestDatabaseLoad";
            this.m_btnDestDatabaseLoad.Size = new System.Drawing.Size(140, 23);
            this.m_btnDestDatabaseLoad.TabIndex = 7;
            this.m_btnDestDatabaseLoad.Text = "Загрузить";
            this.m_btnDestDatabaseLoad.UseVisualStyleBackColor = true;
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
            this.сервисToolStripMenuItem,
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
            this.файлToolStripSeparator1,
            this.выходToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // файлToolStripSeparator1
            // 
            this.файлToolStripSeparator1.Name = "файлToolStripSeparator1";
            this.файлToolStripSeparator1.Size = new System.Drawing.Size(104, 6);
            // 
            // выходToolStripMenuItem
            // 
            this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.выходToolStripMenuItem.Text = "Выход";
            this.выходToolStripMenuItem.Click += new System.EventHandler(this.выходToolStripMenuItem_Click);
            // 
            // сервисToolStripMenuItem
            // 
            this.сервисToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.имитацияпакетодинToolStripMenuItem,
            this.имитацияпакетциклToolStripMenuItem});
            this.сервисToolStripMenuItem.Name = "сервисToolStripMenuItem";
            this.сервисToolStripMenuItem.Size = new System.Drawing.Size(64, 20);
            this.сервисToolStripMenuItem.Text = "Отладка";
            // 
            // имитацияпакетодинToolStripMenuItem
            // 
            this.имитацияпакетодинToolStripMenuItem.Enabled = false;
            this.имитацияпакетодинToolStripMenuItem.Name = "имитацияпакетодинToolStripMenuItem";
            this.имитацияпакетодинToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.имитацияпакетодинToolStripMenuItem.Text = "Имитация-пакет-один";
            this.имитацияпакетодинToolStripMenuItem.Click += new System.EventHandler(this.имитацияпакетодинToolStripMenuItem_Click);
            // 
            // имитацияпакетциклToolStripMenuItem
            // 
            this.имитацияпакетциклToolStripMenuItem.CheckOnClick = true;
            this.имитацияпакетциклToolStripMenuItem.Enabled = false;
            this.имитацияпакетциклToolStripMenuItem.Name = "имитацияпакетциклToolStripMenuItem";
            this.имитацияпакетциклToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.имитацияпакетциклToolStripMenuItem.Text = "Имитация-пакет-цикл";
            this.имитацияпакетциклToolStripMenuItem.CheckedChanged += new System.EventHandler(this.имитацияпакетциклToolStripMenuItem_CheckedChanged);
            // 
            // оПрограммеToolStripMenuItem
            // 
            this.оПрограммеToolStripMenuItem.Enabled = false;
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
            this.splitContainerMain.Panel1MinSize = 310;
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.splitContainerMainWrite);
            this.splitContainerMain.Panel2MinSize = 150;
            this.splitContainerMain.Size = new System.Drawing.Size(861, 670);
            this.splitContainerMain.SplitterDistance = 315;
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
            this.splitContainerMainRead.Panel1.Controls.Add(m_groupBoxListPackage);
            this.splitContainerMainRead.Panel1.Controls.Add(m_groupBoxSession);
            // 
            // splitContainerMainRead.Panel2
            // 
            this.splitContainerMainRead.Panel2.Controls.Add(this.m_tabControlViewPackage);
            this.splitContainerMainRead.Size = new System.Drawing.Size(861, 315);
            this.splitContainerMainRead.SplitterDistance = 410;
            this.splitContainerMainRead.TabIndex = 0;
            // 
            // m_tabControlViewPackage
            // 
            this.m_tabControlViewPackage.Controls.Add(this.m_tpageViewPackageXml);
            this.m_tabControlViewPackage.Controls.Add(this.m_tpageViewPackageTree);
            this.m_tabControlViewPackage.Controls.Add(this.m_tpageViewPackageTableValue);
            this.m_tabControlViewPackage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_tabControlViewPackage.Location = new System.Drawing.Point(0, 0);
            this.m_tabControlViewPackage.Name = "m_tabControlViewPackage";
            this.m_tabControlViewPackage.SelectedIndex = 0;
            this.m_tabControlViewPackage.Size = new System.Drawing.Size(447, 315);
            this.m_tabControlViewPackage.TabIndex = 0;
            // 
            // m_tpageViewPackageXml
            // 
            this.m_tpageViewPackageXml.Controls.Add(this.m_tbxViewPackage);
            this.m_tpageViewPackageXml.Location = new System.Drawing.Point(4, 22);
            this.m_tpageViewPackageXml.Name = "m_tpageViewPackageXml";
            this.m_tpageViewPackageXml.Padding = new System.Windows.Forms.Padding(3);
            this.m_tpageViewPackageXml.Size = new System.Drawing.Size(439, 289);
            this.m_tpageViewPackageXml.TabIndex = 0;
            this.m_tpageViewPackageXml.Text = "XML";
            this.m_tpageViewPackageXml.UseVisualStyleBackColor = true;
            // 
            // m_labelViewPackage
            // 
            this.m_tbxViewPackage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_tbxViewPackage.Location = new System.Drawing.Point(3, 3);
            this.m_tbxViewPackage.Name = "m_tbxViewPackage";
            this.m_tbxViewPackage.Size = new System.Drawing.Size(433, 283);
            this.m_tbxViewPackage.TabIndex = 0;
            this.m_tbxViewPackage.ReadOnly = true;
            this.m_tbxViewPackage.Multiline = true;
            this.m_tbxViewPackage.WordWrap = true;
            this.m_tbxViewPackage.ScrollBars = ScrollBars.Both;
            this.m_tbxViewPackage.Text = string.Empty;
            // 
            // m_tpageViewPackageTree
            // 
            this.m_tpageViewPackageTree.Controls.Add(this.m_treeViewPackage);
            this.m_tpageViewPackageTree.Location = new System.Drawing.Point(4, 22);
            this.m_tpageViewPackageTree.Name = "m_tpageViewPackageTree";
            this.m_tpageViewPackageTree.Padding = new System.Windows.Forms.Padding(3);
            this.m_tpageViewPackageTree.Size = new System.Drawing.Size(439, 289);
            this.m_tpageViewPackageTree.TabIndex = 1;
            this.m_tpageViewPackageTree.Text = "Структура";
            this.m_tpageViewPackageTree.UseVisualStyleBackColor = true;
            ((Control)this.m_tpageViewPackageTree).Enabled = true;
            // 
            // m_treeViewPackage
            // 
            this.m_treeViewPackage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_treeViewPackage.Location = new System.Drawing.Point(3, 3);
            this.m_treeViewPackage.Name = "m_treeViewPackage";
            this.m_treeViewPackage.Size = new System.Drawing.Size(433, 283);
            this.m_treeViewPackage.TabIndex = 0;
            // 
            // m_tpageViewPackageTableValue
            // 
            this.m_tpageViewPackageTableValue.Controls.Add(this.m_dgvViewPackageTableValue);
            this.m_tpageViewPackageTableValue.Location = new System.Drawing.Point(4, 22);
            this.m_tpageViewPackageTableValue.Name = "m_tpageViewPackageTableValue";
            this.m_tpageViewPackageTableValue.Padding = new System.Windows.Forms.Padding(3);
            this.m_tpageViewPackageTableValue.Size = new System.Drawing.Size(439, 289);
            this.m_tpageViewPackageTableValue.TabIndex = 2;
            this.m_tpageViewPackageTableValue.Text = "Табл.-значения";
            this.m_tpageViewPackageTableValue.UseVisualStyleBackColor = true;
            ((Control)this.m_tpageViewPackageTableValue).Enabled = false;
            // 
            // m_dgvViewPackageTableValue
            // 
            this.m_dgvViewPackageTableValue.AllowUserToAddRows = false;
            this.m_dgvViewPackageTableValue.AllowUserToDeleteRows = false;
            this.m_dgvViewPackageTableValue.Dock = DockStyle.Fill;
            this.m_dgvViewPackageTableValue.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_dgvViewPackageTableValue.Location = new System.Drawing.Point(153, 18);
            this.m_dgvViewPackageTableValue.Name = "m_dgvViewPackageTableValue";
            this.m_dgvViewPackageTableValue.ReadOnly = true;
            this.m_dgvViewPackageTableValue.RowHeadersVisible = false;
            this.m_dgvViewPackageTableValue.Size = new System.Drawing.Size(257, 204);
            this.m_dgvViewPackageTableValue.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.m_dgvViewPackageTableValue.TabIndex = 11;
            // 
            // splitContainerMainWrite
            // 
            this.splitContainerMainWrite.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMainWrite.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMainWrite.Name = "splitContainerMainWrite";
            // 
            // splitContainerMainWrite.Panel1
            // 
            this.splitContainerMainWrite.Panel1.Controls.Add(m_groupBoxDatabase);
            this.splitContainerMainWrite.Panel1.Controls.Add(m_groupBoxDestSetting);
            // 
            // splitContainerMainWrite.Panel2
            // 
            this.splitContainerMainWrite.Panel2.Controls.Add(m_tabControlDest);
            this.splitContainerMainWrite.Size = new System.Drawing.Size(861, 351);
            this.splitContainerMainWrite.SplitterDistance = 410;
            this.splitContainerMainWrite.TabIndex = 0;
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
            this.MinimumSize = new System.Drawing.Size(8, 640);
            this.Name = "FormMain";
            this.Text = @"";
            //this.Load += new System.EventHandler(this.FormMain_Load); // подписка в базовом классе
            m_groupBoxListPackage.ResumeLayout(false);
            m_groupBoxListPackage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_nudnPackageDisplayCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_nudnSettingPackageHistoryDepth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvPackageList)).EndInit();
            m_groupBoxSession.ResumeLayout(false);
            m_groupBoxSession.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_nudnUDPPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvStatistic)).EndInit();
            m_groupBoxDestSetting.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvDestList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvDestSetting)).EndInit();
            m_tabControlDest.ResumeLayout(false);
            this.m_tpageDestValue.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvDestValue)).EndInit();
            this.m_tpageDestParameter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvDestParameter)).EndInit();            
            m_groupBoxDatabase.ResumeLayout(false);
            m_groupBoxDatabase.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_nudnDestDisplayCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvDestDatabaseListAction)).EndInit();
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
            this.m_tabControlViewPackage.ResumeLayout(false);
            this.m_tpageViewPackageXml.ResumeLayout(false);
            this.m_tpageViewPackageTree.ResumeLayout(false);
            this.m_tpageViewPackageTableValue.ResumeLayout(false);
            this.splitContainerMainWrite.Panel1.ResumeLayout(false);
            this.splitContainerMainWrite.Panel2.ResumeLayout(false);
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
        private System.Windows.Forms.TabControl m_tabControlViewPackage;
        private System.Windows.Forms.TabPage m_tpageViewPackageXml;
        private System.Windows.Forms.TextBox m_tbxViewPackage;
        private System.Windows.Forms.TabPage m_tpageViewPackageTree;
        private TreeViewPackage m_treeViewPackage;
        private System.Windows.Forms.TabPage m_tpageViewPackageTableValue;
        private System.Windows.Forms.DataGridView m_dgvViewPackageTableValue;
        private System.Windows.Forms.Button m_btnLoadPackageHistory;
        private System.Windows.Forms.NumericUpDown m_nudnSettingPackageHistoryDepth;
        private System.Windows.Forms.CheckBox m_cbxSettingPackageHistoryIssue;
        private DataGridViewStatistic m_dgvStatistic;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEventName;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEventDateTime;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEventDesc;
        private System.Windows.Forms.SplitContainer splitContainerMainWrite;
        private System.Windows.Forms.DateTimePicker m_dateTimePickerPackageFilterStart;
        private System.Windows.Forms.DateTimePicker m_dateTimePickerPackageFilterEnd;
        private System.Windows.Forms.CheckBox m_cbxPackageIndiciesIssue;
        private System.Windows.Forms.Button m_btnPackageReSend;
        private System.Windows.Forms.CheckBox m_cbxReadSessionStart;
        private System.Windows.Forms.NumericUpDown m_nudnUDPPort;
        private System.Windows.Forms.ToolStripMenuItem сервисToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem имитацияпакетодинToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem имитацияпакетциклToolStripMenuItem;
        private System.Windows.Forms.CheckBox m_cbxReadSessionStop;
        private System.Windows.Forms.TabPage m_tpageDestParameter;
        private System.Windows.Forms.DataGridView m_dgvDestParameter;
        private System.Windows.Forms.TabPage m_tpageDestValue;
        private System.Windows.Forms.DataGridView m_dgvDestValue;
        private DataGridView m_dgvPackageList;
        private System.Windows.Forms.DataGridView m_dgvDestSetting;
        private System.Windows.Forms.Button m_btnDestRemove;
        private System.Windows.Forms.Button m_btnDestAdd;
        private System.Windows.Forms.DataGridView m_dgvDestList;
        private System.Windows.Forms.Button m_btnDestDatabaseDelete;
        private System.Windows.Forms.DateTimePicker m_dateTimePickerDestDataBaseFilterStart;
        private System.Windows.Forms.DateTimePicker m_dateTimePickerDestDataBaseFilterStop;
        private System.Windows.Forms.Button m_btnDestDatabaseLoad;
        private System.Windows.Forms.NumericUpDown m_nudnPackageDisplayCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSessionParameterValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnDestSettingValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnDestListName;
        private DataGridViewPressedButtonColumn ColumnDestListState;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPackageListItemCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPackageListDatetimeInput;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPackageListDatetimeOutput;
        private System.Windows.Forms.NumericUpDown m_nudnDestDisplayCount;
        private System.Windows.Forms.DataGridView m_dgvDestDatabaseListAction;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnDestDatabaseListActionItemCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnDestDatabaseListActionInput;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnDestDatabaseListActionCompleted;
    }
}

