using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;

using HClassLibrary;

namespace uLoader
{
    public partial class PanelWork
    {
        private partial class PanelLoader : PanelCommonDataHost
        {
            //Ключи элементов управления
            public enum KEY_CONTROLS { DGV_GROUP_SOURCES, LABEL_DLLNAME_GROUPSOURCES, BUTTON_DLLNAME_GROUPSOURCES, CBX_SOURCE_OF_GROUP
                                        , DGV_GROUP_SIGNALS
                                        , RBUTTON_CUR_DATETIME, NUMUD_CUR_DATETIME
                                        , RBUTTON_COSTUMIZE, CALENDAR_COSTUMIZE, TBX_BEGIN_TIME, TBX_END_TIME, NUMUD_COSTUMIZE_STEP
                                        , DGV_SIGNALS_OF_GROUP
            };
        
            public PanelLoader()
                : base (4, 1)
            {
                InitializeComponent();
            }

            public PanelLoader(IContainer container) : base (4, 1)
            {
                container.Add(this);

                InitializeComponent();
            }
        }

        partial class PanelLoader
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

            #region Код, автоматически созданный конструктором компонентов

            /// <summary>
            /// Обязательный метод для поддержки конструктора - не изменяйте
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            private void InitializeComponent()
            {
                components = new System.ComponentModel.Container();

                //this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

                Control ctrl = null;
                HPanelCommon panelColumns;

                this.SuspendLayout ();

                //Панель источников данных
                panelColumns = new PanelCommonULoader(5, 8);
                this.Controls.Add(panelColumns, 0, 0);
                this.SetColumnSpan(panelColumns, 1); this.SetRowSpan(panelColumns, 1);
                //Группы источников
                ctrl = new DataGridView ();
                ctrl.Name = KEY_CONTROLS.DGV_GROUP_SOURCES.ToString ();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as DataGridView).Columns.AddRange (
                    new DataGridViewColumn [] {
                        new DataGridViewTextBoxColumn ()
                        , new DataGridViewButtonColumn ()
                    }
                );                
                (ctrl as DataGridView).AllowUserToResizeColumns = false;
                (ctrl as DataGridView).AllowUserToResizeRows = false;
                (ctrl as DataGridView).AllowUserToAddRows = false;
                (ctrl as DataGridView).AllowUserToDeleteRows = false;
                (ctrl as DataGridView).MultiSelect = false;
                (ctrl as DataGridView).SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                (ctrl as DataGridView).RowHeadersVisible = false;
                (ctrl as DataGridView).Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                (ctrl as DataGridView).Columns[1].Width = 37;
                (ctrl as DataGridView).RowsAdded += new DataGridViewRowsAddedEventHandler(panelLoader_RowsAdded);
                panelColumns.Controls.Add(ctrl, 0, 0);
                panelColumns.SetColumnSpan(ctrl, 5); panelColumns.SetRowSpan(ctrl, 6);
                //Библиотека для загрузки
                ctrl = new Label();
                ctrl.Name = KEY_CONTROLS.LABEL_DLLNAME_GROUPSOURCES.ToString();
                (ctrl as Label).BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                (ctrl as Label).FlatStyle = FlatStyle.Standard;
                ctrl.Dock = DockStyle.Fill;
                panelColumns.Controls.Add(ctrl, 0, 6);
                panelColumns.SetColumnSpan(ctrl, 4); panelColumns.SetRowSpan(ctrl, 1);
                //Кнопка для выгрузки/загрузки библиотеки
                ctrl = new Button();
                ctrl.Name = KEY_CONTROLS.BUTTON_DLLNAME_GROUPSOURCES.ToString();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as Button).Text = @"<->";
                panelColumns.Controls.Add(ctrl, 4, 6);
                panelColumns.SetColumnSpan(ctrl, 1); panelColumns.SetRowSpan(ctrl, 1);
                //Выбор текущего источника
                ctrl = new ComboBox();
                ctrl.Name = KEY_CONTROLS.CBX_SOURCE_OF_GROUP.ToString();
                ctrl.Dock = DockStyle.Bottom;
                panelColumns.Controls.Add(ctrl, 0, 7);
                panelColumns.SetColumnSpan(ctrl, 5); panelColumns.SetRowSpan(ctrl, 1);

                //Панель опроса
                panelColumns = new PanelCommonULoader(5, 8);
                this.Controls.Add(panelColumns, 1, 0);
                this.SetColumnSpan(panelColumns, 1); this.SetRowSpan(panelColumns, 1);
                //Группы сигналов
                ctrl = new DataGridView();
                ctrl.Name = KEY_CONTROLS.DGV_GROUP_SIGNALS.ToString ();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as DataGridView).Columns.AddRange(
                    new DataGridViewColumn[] {
                        new DataGridViewTextBoxColumn ()
                        , new DataGridViewButtonColumn ()
                    }
                );                
                (ctrl as DataGridView).AllowUserToResizeColumns = false;
                (ctrl as DataGridView).AllowUserToResizeRows = false;
                (ctrl as DataGridView).AllowUserToAddRows = false;
                (ctrl as DataGridView).AllowUserToDeleteRows = false;
                (ctrl as DataGridView).MultiSelect = false;
                (ctrl as DataGridView).SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                (ctrl as DataGridView).RowHeadersVisible = false;
                (ctrl as DataGridView).Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                (ctrl as DataGridView).Columns[1].Width = 37;
                (ctrl as DataGridView).RowsAdded += new DataGridViewRowsAddedEventHandler(panelLoader_RowsAdded);
                panelColumns.Controls.Add(ctrl, 0, 0);
                panelColumns.SetColumnSpan(ctrl, 5); panelColumns.SetRowSpan(ctrl, 3);
                //ГроупБокс режима опроса
                ctrl = new GroupBox();
                (ctrl as GroupBox).Text = @"Режим опроса";
                ctrl.Dock = DockStyle.Fill;
                panelColumns.Controls.Add(ctrl, 0, 3);
                panelColumns.SetColumnSpan(ctrl, 5); panelColumns.SetRowSpan(ctrl, 5);
                //Панель для ГроупБокса
                HPanelCommon panelGroupBox = new PanelCommonULoader(8, 7);
                panelGroupBox.Dock = DockStyle.Fill;
                ctrl.Controls.Add(panelGroupBox);
                //Текущая дата/время
                //РадиоБуттон (тек. дата/время)
                ctrl = new RadioButton ();
                ctrl.Name = KEY_CONTROLS.RBUTTON_CUR_DATETIME.ToString ();
                (ctrl as RadioButton).Text = @"Текущие дата/время";
                ctrl.Dock = DockStyle.Fill;
                (ctrl as RadioButton).CheckedChanged += new EventHandler(panelLoader_CheckedChanged);
                panelGroupBox.Controls.Add(ctrl, 0, 0);
                panelGroupBox.SetColumnSpan(ctrl, 8); panelGroupBox.SetRowSpan(ctrl, 1);
                //Описание для интервала
                ctrl = new Label();
                (ctrl as Label).Text = @"Интервал(сек)";
                //ctrl.Dock = DockStyle.Bottom;
                ctrl.Anchor = ((AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left));
                panelGroupBox.Controls.Add(ctrl, 0, 1);
                panelGroupBox.SetColumnSpan(ctrl, 5); panelGroupBox.SetRowSpan(ctrl, 1);
                //NumericUpDown изменения интервала
                ctrl = new NumericUpDown();
                ctrl.Name = KEY_CONTROLS.NUMUD_CUR_DATETIME.ToString();
                (ctrl as NumericUpDown).Minimum = 6; (ctrl as NumericUpDown).Maximum = 600;
                (ctrl as NumericUpDown).Increment = 10;
                ctrl.Dock = DockStyle.Bottom;
                panelGroupBox.Controls.Add(ctrl, 5, 1);
                panelGroupBox.SetColumnSpan(ctrl, 3); panelGroupBox.SetRowSpan(ctrl, 1);
                //Выборочно
                //РадиоБуттон (выборочно)
                ctrl = new RadioButton();
                ctrl.Name = KEY_CONTROLS.RBUTTON_COSTUMIZE.ToString();
                (ctrl as RadioButton).Text = @"Выборочно";
                ctrl.Dock = DockStyle.Fill;
                (ctrl as RadioButton).CheckedChanged += new EventHandler(panelLoader_CheckedChanged);
                panelGroupBox.Controls.Add(ctrl, 0, 3);
                panelGroupBox.SetColumnSpan(ctrl, 8); panelGroupBox.SetRowSpan(ctrl, 1);
                //Календарь
                ctrl = new DateTimePicker ();
                ctrl.Name = KEY_CONTROLS.CALENDAR_COSTUMIZE.ToString();
                ctrl.Dock = DockStyle.Fill;
                panelGroupBox.Controls.Add(ctrl, 0, 4);
                panelGroupBox.SetColumnSpan(ctrl, 8); panelGroupBox.SetRowSpan(ctrl, 1);
                ////Начало периода
                //ctrl = new Label();
                //(ctrl as Label).Text = @"Начало";
                ////ctrl.Dock = DockStyle.Bottom;
                //ctrl.Anchor = AnchorStyles.Bottom;
                //panelGroupBox.Controls.Add(ctrl, 0, 5);
                //panelGroupBox.SetColumnSpan(ctrl, 2); panelGroupBox.SetRowSpan(ctrl, 1);
                ////Тире
                ////Окончание периода
                //ctrl = new Label();
                //(ctrl as Label).Text = @"Оконч.";
                //ctrl.Dock = DockStyle.Bottom;
                //panelGroupBox.Controls.Add(ctrl, 3, 5);
                //panelGroupBox.SetColumnSpan(ctrl, 2); panelGroupBox.SetRowSpan(ctrl, 1);
                ////Тире
                ////Шаг(мин)
                ////NumericUpDown изменения шага
                //ctrl = new Label();
                //(ctrl as Label).Text = @"Шаг(мин)";
                //ctrl.Dock = DockStyle.Bottom;
                //panelGroupBox.Controls.Add(ctrl, 6, 5);
                //panelGroupBox.SetColumnSpan(ctrl, 2); panelGroupBox.SetRowSpan(ctrl, 1);
                //Начало периода
                ctrl = new TextBox();
                ctrl.Name = KEY_CONTROLS.TBX_BEGIN_TIME.ToString();
                ctrl.Dock = DockStyle.Fill;
                panelGroupBox.Controls.Add(ctrl, 0, 6);
                panelGroupBox.SetColumnSpan(ctrl, 2); panelGroupBox.SetRowSpan(ctrl, 1);
                //Тире
                //Окончание периода
                ctrl = new TextBox();
                ctrl.Name = KEY_CONTROLS.TBX_END_TIME.ToString();
                ctrl.Dock = DockStyle.Fill;
                panelGroupBox.Controls.Add(ctrl, 3, 6);
                panelGroupBox.SetColumnSpan(ctrl, 2); panelGroupBox.SetRowSpan(ctrl, 1);
                //Тире
                //Шаг(мин)
                //NumericUpDown изменения шага
                ctrl = new NumericUpDown();
                ctrl.Name = KEY_CONTROLS.NUMUD_COSTUMIZE_STEP.ToString();
                (ctrl as NumericUpDown).Minimum = 1; (ctrl as NumericUpDown).Maximum = 60;
                ctrl.Dock = DockStyle.Fill;
                panelGroupBox.Controls.Add(ctrl, 6, 6);
                panelGroupBox.SetColumnSpan(ctrl, 2); panelGroupBox.SetRowSpan(ctrl, 1);

                //Панель рез-ов опроса
                panelColumns = new PanelCommonULoader(10, 8);
                this.Controls.Add(panelColumns, 2, 0);
                this.SetColumnSpan(panelColumns, 2); this.SetRowSpan(panelColumns, 1);

                this.ResumeLayout (false);
                this.PerformLayout ();
            }

            #endregion

            private void panelLoader_RowsAdded (object obj, EventArgs ev)
            {
                int cnt = (obj as DataGridView).Rows.Count;
                (obj as DataGridView).Rows[cnt - 1].Cells [1].Value = @"->";
            }

            private void panelLoader_CheckedChanged(object obj, EventArgs ev)
            {
                RadioButton rBtn = obj as RadioButton;
                KEY_CONTROLS key;

                if (rBtn.Checked == true)
                {
                    bool bCostumizeEnabled =
                        rBtn.Name.Equals(KEY_CONTROLS.RBUTTON_COSTUMIZE.ToString())
                        //false
                        ;
                    
                    //if (rBtn.Name.Equals (KEY_CONTROLS.RBUTTON_CUR_DATETIME.ToString ()) == true)
                    //{
                    //}
                    //else
                    //    if (rBtn.Name.Equals (KEY_CONTROLS.RBUTTON_COSTUMIZE.ToString ()) == true)
                    //    {
                    //         bCostumizeEnabled = true;
                    //    }
                    //    else
                    //        ;

                    key = KEY_CONTROLS.NUMUD_CUR_DATETIME;
                    Controls.Find(key.ToString(), true)[0].Enabled = ! bCostumizeEnabled;

                    key = KEY_CONTROLS.CALENDAR_COSTUMIZE;
                    Controls.Find(key.ToString(), true)[0].Enabled = bCostumizeEnabled;
                    key = KEY_CONTROLS.TBX_BEGIN_TIME;
                    Controls.Find(key.ToString(), true)[0].Enabled = bCostumizeEnabled;
                    key = KEY_CONTROLS.TBX_END_TIME;
                    Controls.Find(key.ToString(), true)[0].Enabled = bCostumizeEnabled;
                    key = KEY_CONTROLS.NUMUD_COSTUMIZE_STEP;
                    Controls.Find(key.ToString(), true)[0].Enabled = bCostumizeEnabled;
                }
                else
                    ; //Не "отмеченные" - игнорировать
            }
        }

        private class PanelLoaderSource : PanelLoader
        {
            public PanelLoaderSource ()
            {
                InitializeComponent ();
            }

            private void InitializeComponent ()
            {
                HPanelCommon panelColumns;

                this.SuspendLayout();

                //Панель сигналов группы
                //panelColumns = new PanelCommonULoader(1, 8);
                //this.Controls.Add(panelColumns, 2, 0);
                //this.SetColumnSpan(panelColumns, 2); this.SetRowSpan(panelColumns, 1);
                panelColumns = this.Controls[2] as HPanelCommon;                

                DataGridView ctrl = new DataGridView ();
                ctrl.Name = KEY_CONTROLS.DGV_SIGNALS_OF_GROUP.ToString ();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as DataGridView).Columns.AddRange(
                    new DataGridViewColumn[] {
                        new DataGridViewTextBoxColumn ()
                        , new DataGridViewTextBoxColumn ()
                        , new DataGridViewTextBoxColumn ()
                    }
                );                 
                (ctrl as DataGridView).AllowUserToResizeColumns = false;
                (ctrl as DataGridView).AllowUserToResizeRows = false;
                (ctrl as DataGridView).AllowUserToAddRows = false;
                (ctrl as DataGridView).AllowUserToDeleteRows = false;
                (ctrl as DataGridView).MultiSelect = false;
                (ctrl as DataGridView).SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                (ctrl as DataGridView).RowHeadersVisible = false;
                foreach (DataGridViewColumn col in (ctrl as DataGridView).Columns)
                    if ((ctrl as DataGridView).Columns.IndexOf(col) < (ctrl as DataGridView).Columns.Count - 1)
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    else
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                panelColumns.Controls.Add(ctrl, 0, 0);
                panelColumns.SetColumnSpan(ctrl, 10); panelColumns.SetRowSpan(ctrl, 5);

                this.ResumeLayout (false);
                this.PerformLayout ();
            }
        }

        private class PanelLoaderDest : PanelLoader
        {
            public PanelLoaderDest()
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
            }
        }
    }    
}
