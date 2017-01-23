using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace uLoaderCommon
{
    public abstract class FormMainBase : Form
    {
        private System.Windows.Forms.NotifyIcon m_notifyIcon;
        /// <summary>
        /// Объект обработки очереди событий
        /// </summary>
        protected HHandlerQueue m_handler;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public FormMainBase()
        {
            new HCmd_Arg();

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            //this.m_notifyIcon.Icon = this.Icon; // пиктограмма еще не определена
            this.m_notifyIcon.Click += new System.EventHandler(NotifyIcon_Click);

            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_Closing);
            this.Load += new System.EventHandler(this.FormMain_Load);
        }
        /// <summary>
        /// Создать объект - обработчик очереди событий
        /// </summary>
        /// <param name="typeHandler"></param>
        protected virtual void createHandlerQueue(Type typeHandler)
        {
            m_handler = Activator.CreateInstance(typeHandler, HCmd_Arg.NameFileINI) as HHandlerQueue;
        }
        /// <summary>
        /// Инициализация размеров/размещения главной формы приложения
        /// </summary>
        protected void initFormMainSizing()
        {
            this.m_notifyIcon.Icon = this.Icon;

            if (HCmd_Arg.IsNormalized == true) {
            // нормальный размер окна
                this.OnMaximumSizeChanged(null);
            } else {
            // минимизация
                Message msg = new Message();
                msg.Msg = 0x112;
                msg.WParam = (IntPtr)(0xF020);
                WndProc(ref msg);
            }
        }
        /// <summary>
        /// Класс обработки "своих" команд
        /// </summary>
        public class HCmd_Arg : HClassLibrary.HCmd_Arg
        {
            public static bool IsNormalized = true;

            public static string NameFileINI = string.Empty;

            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="args">Массив аргументов командной строки</param>
            public HCmd_Arg()
                : base(Environment.GetCommandLineArgs())
            {
                foreach (KeyValuePair<string, string> pair in m_dictCmdArgs)
                    switch (pair.Key) {
                        case "conf_ini":
                            NameFileINI = pair.Value;
                            break;
                        case "minimize":
                            IsNormalized = false;
                            break;
                        default:
                            //strNameFileINI = string.Empty;
                            break;
                    }
            }
        }
        /// <summary>
        /// Переопределение ф-и обработки оконных событий
        ///  переопрделение нажатия на кнопку свернуть
        /// </summary>
        /// <param name="m">Объект сообщения</param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x112)
            {
                if (m.WParam.ToInt32() == 0xF020)
                {
                    this.WindowState = FormWindowState.Minimized;
                    this.ShowInTaskbar = false;
                    m_notifyIcon.Visible = true;

                    return;
                }
            }
            else
                ;

            base.WndProc(ref m);
        }
        /// <summary>
        /// Обработчик события - нажатие на пиктограмму в области системных оповещений ОС
        /// </summary>
        /// <param name="sender">Объект - инициатор события - ???</param>
        /// <param name="e">Аргумент события</param>
        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            m_notifyIcon.Visible = false;
        }
        /// <summary>
        /// Обработчик события - закрытие формы
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие (форма)</param>
        /// <param name="e">Аргумент события</param>
        protected virtual void FormMain_Closing(object sender, FormClosingEventArgs e)
        {
            m_notifyIcon.Visible = false;

            m_handler.Activate(false); m_handler.Stop();
        }

        protected virtual void FormMain_Load(object sender, EventArgs e)
        {
        }
    }
}
