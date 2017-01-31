using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace xmlLoader
{
    partial class FormMain
    {
        /// <summary>
        /// Строка состояния для главной формы приложения
        /// </summary>
        public class StatusStrip : System.Windows.Forms.StatusStrip
        {
            /// <summary>
            /// Интервал обновления (мсек) информации в строке состояния
            /// </summary>
            private static int MSEC_INTERVAL_TWINKLE = 800;
            /// <summary>
            /// Перечисление - известные типы сообщений
            /// </summary>
            public enum STATE { Unknown = -666
                , Exception = -3, Error, Warning, Action, Ready
            }
            /// <summary>
            /// Структура для хранения информации о типе сообщения
            /// </summary>
            struct EVENT
            {
                /// <summary>
                /// Наименование типа сообщения
                /// </summary>
                public string m_Name;
                /// <summary>
                /// Цветовая гамма наименования типа сообщения
                /// </summary>
                public Color m_clrBackground
                    , m_clrFore;
                /// <summary>
                /// Максимальное кол-во "миганий", после чего строка очищается
                ///  , при этом другие сообщения с меньшим приоритетом пропускаются
                /// </summary>
                public int m_MaxTwinkle;
            }
            /// <summary>
            /// Словарь с информацией о всех типах сообщений
            /// </summary>
            private Dictionary<STATE, EVENT> m_dictEvent;
            /// <summary>
            /// Реализация паттерна 'Singleton'
            /// </summary>
            private static StatusStrip _this;
            /// <summary>
            /// Элемент строки статуса - наименование сообщения
            /// </summary>
            private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEventName;
            /// <summary>
            /// Элемент строки статуса - метка даты/времени
            /// </summary>
            private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEventDateTime;
            /// <summary>
            /// Элемент строки статуса - описание сообщения
            /// </summary>
            private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEventDesc;
            /// <summary>
            /// Текущее состояние сообщения в строке статуса
            /// </summary>
            private STATE _state;
            /// <summary>
            /// Таймер для реализации эффекта мерцания + очистка крайнего сообщения
            /// </summary>
            private System.Threading.Timer m_timerTwinkle;
            /// <summary>
            /// Счетчик для эффекта мерцания
            /// </summary>
            private int iTwinkle;
            /// <summary>
            /// Конструктор - основной (без парметров)
            ///  для реализации паттерна 'Singleton'
            /// </summary>
            private StatusStrip() : base()
            {
                m_dictEvent = new Dictionary<STATE, EVENT>() {
                    { STATE.Exception, new EVENT () { m_Name = @"Исключение", m_MaxTwinkle = 30, m_clrFore = Color.Red } }
                    , { STATE.Error, new EVENT () { m_Name = @"Ошибка", m_MaxTwinkle = 30, m_clrFore = Color.OrangeRed } }
                    , { STATE.Warning, new EVENT () { m_Name = @"Предупреждение", m_MaxTwinkle = 15, m_clrFore = Color.Yellow } }
                    , { STATE.Action, new EVENT () { m_Name = @"Действие", m_MaxTwinkle = 0 } }
                };

                InitializeComponent();

                _state = STATE.Unknown;
                iTwinkle = (int)_state;
            }
            /// <summary>
            /// ДЛя доступа к единственному экземпляру объекта из-вне
            /// </summary>
            public static StatusStrip This
            {
                get {
                    if (_this == null)
                        _this = new StatusStrip();
                    else
                        ;

                    return _this;
                }
            }

            private void InitializeComponent()
            {
                this.toolStripStatusLabelEventName = new System.Windows.Forms.ToolStripStatusLabel();
                this.toolStripStatusLabelEventDateTime = new System.Windows.Forms.ToolStripStatusLabel();
                this.toolStripStatusLabelEventDesc = new System.Windows.Forms.ToolStripStatusLabel();

                SuspendLayout();
                // 
                // m_statusStripMain
                // 
                Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    this.toolStripStatusLabelEventName,
                    this.toolStripStatusLabelEventDateTime,
                    this.toolStripStatusLabelEventDesc
                });
                Location = new System.Drawing.Point(0, 694);
                Name = "m_statusStripMain";
                Size = new System.Drawing.Size(861, 22);
                TabIndex = 0;
                Text = "statusStripMain";
                // 
                // toolStripStatusLabelEventName
                // 
                this.toolStripStatusLabelEventName.AutoSize = false;
                this.toolStripStatusLabelEventName.Name = "toolStripStatusLabelEventName";
                this.toolStripStatusLabelEventName.Size = new System.Drawing.Size(120, 17);
                this.toolStripStatusLabelEventName.Text = "Событие";
                this.toolStripStatusLabelEventName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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

                ResumeLayout(false);
                PerformLayout();
            }
            /// <summary>
            /// Инициализация, при необходимости для сложных объектов
            /// </summary>
            public void Start()
            {
                reset();

                m_timerTwinkle = new System.Threading.Timer(fTimerTwinkle_callBack, null, MSEC_INTERVAL_TWINKLE, MSEC_INTERVAL_TWINKLE);
            }
            /// <summary>
            /// Освободить ресурсы
            /// </summary>
            public void Stop()
            {
                m_timerTwinkle.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                m_timerTwinkle.Dispose();
            }
            /// <summary>
            /// Обработчик события - таймер обновлн=ения информации в строке
            /// </summary>
            /// <param name="obj">Аргумент при вызове метода</param>
            private void fTimerTwinkle_callBack(object obj)
            {
                Action clear = delegate () {
                    toolStripStatusLabelEventName.Text = string.Empty;
                    toolStripStatusLabelEventName.ForeColor = Color.Black;

                    toolStripStatusLabelEventDateTime.Text =
                    toolStripStatusLabelEventDesc.Text =
                        string.Empty;

                    reset();
                };

                Action twinkle = delegate () {
                    toolStripStatusLabelEventName.Text = iTwinkle % 2 == 1 ? m_dictEvent[_state].m_Name : string.Empty;

                    iTwinkle++;

                    if (iTwinkle > m_dictEvent[_state].m_MaxTwinkle)
                        clear();
                    else
                        ;
                };

                if (!(iTwinkle < 0))
                // только для мерцающих сообщений
                    if (InvokeRequired == true)
                        BeginInvoke(twinkle);
                    else
                        twinkle();
                else
                // для немерцающих(Действие) сообщений
                    if (!(_state == STATE.Ready))
                        if (InvokeRequired == true)
                            BeginInvoke(clear);
                        else
                            clear();
                    else
                        ; // очищать строку не надо - она уже чистая
            }
            /// <summary>
            /// Сборсить тип сообщения, отменить эффект мерцания
            /// </summary>
            private void reset()
            {
                _state = STATE.Ready;
                iTwinkle = -1;
            }
            /// <summary>
            /// Отобразить сообщение в строке статуса
            /// </summary>
            /// <param name="state">Тип сообщения</param>
            /// <param name="msg">Описание сообщения</param>
            public void Message(STATE state, string msg)
            {
                if (Enum.IsDefined(typeof(STATE), state) == true) {
                    if (!(state > _state)) {
                        _state = state;
                        // кроме 'Action' остальные имеют эффект мерцания
                        iTwinkle = _state == STATE.Action ? -1 : 0;

                        //m_dictEvent[state].m_fMessage(msg);
                        BeginInvoke(new Action<string>(message), msg);
                    } else
                        ;
                } else
                    Logging.Logg().Error(string.Format(@"StatusStrip::Message (message={0}) - неизвестный тип сообщения...", msg), Logging.INDEX_MESSAGE.NOT_SET);
            }
            /// <summary>
            /// Отобразить сообщение
            /// </summary>
            /// <param name="msg">Описание сообщения</param>
            private void message(string msg)
            {
                toolStripStatusLabelEventName.Text = m_dictEvent[_state].m_Name;
                toolStripStatusLabelEventName.ForeColor = m_dictEvent[_state].m_clrFore;
                //toolStripStatusLabelEventName.BackColor = m_dictEvent[_state].m_clrBackground;

                toolStripStatusLabelEventDateTime.Text = DateTime.Now.ToString();
                toolStripStatusLabelEventDesc.Text = msg;
            }
        }
    }
}
