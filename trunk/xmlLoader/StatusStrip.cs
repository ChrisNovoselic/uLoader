using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xmlLoader
{
    partial class FormMain
    {
        public class StatusStrip : System.Windows.Forms.StatusStrip
        {
            private static int MSEC_INTERVAL_TWINKLE = 800;

            public enum STATE { Exception = -2, Error = -1, Ready, Action, Warning }

            struct EVENT
            {
                public string m_Name;

                public Action<string> m_fMessage;
            }

            private Dictionary<STATE, EVENT> m_dictEvent;

            private static StatusStrip _this;

            private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEventName;
            private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEventDateTime;
            private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEventDesc;

            private STATE _state;

            private System.Threading.Timer m_timerTwinkle;
            private int iTwinkle;

            private StatusStrip() : base()
            {
                m_dictEvent = new Dictionary<STATE, EVENT>() {
                    { STATE.Exception, new EVENT () { m_Name = @"Исключение", m_fMessage = exception } }
                    , { STATE.Error, new EVENT () { m_Name = @"Ошибка", m_fMessage = error } }
                    , { STATE.Action, new EVENT () { m_Name = @"Действие", m_fMessage = action } }
                    , { STATE.Warning, new EVENT () { m_Name = @"Предупреждение", m_fMessage = warning } }
                };

                InitializeComponent();

                reset();

                m_timerTwinkle = new System.Threading.Timer(fTimerTwinkle_callBack, null, MSEC_INTERVAL_TWINKLE, MSEC_INTERVAL_TWINKLE);
            }

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

                ResumeLayout(false);
                PerformLayout();
            }

            private void fTimerTwinkle_callBack(object obj)
            {
                Action clear = delegate () {
                    toolStripStatusLabelEventName.Text = string.Empty;

                    toolStripStatusLabelEventDateTime.Text =
                    toolStripStatusLabelEventDesc.Text =
                        string.Empty;

                    reset();
                };

                if (!(iTwinkle < 0))
                {
                    toolStripStatusLabelEventName.Text = iTwinkle % 2 == 1 ? m_dictEvent[_state].m_Name : string.Empty;

                    iTwinkle++;
                }
                else
                    if (InvokeRequired == true)
                        BeginInvoke(clear);
                    else
                        clear();
            }

            private void reset()
            {
                _state = STATE.Ready;
                iTwinkle = -1;
            }

            public void Message(STATE state, string msg)
            {
                if (Enum.IsDefined(typeof(STATE), state) == true) {
                    _state = state;

                    m_dictEvent[state].m_fMessage(msg);
                } else
                    Logging.Logg().Error(string.Format(@"StatusStrip::Message (message={0}) - неизвестный тип сообщения...", msg), Logging.INDEX_MESSAGE.NOT_SET);
            }

            private void message(string msg)
            {
                toolStripStatusLabelEventDateTime.Text = DateTime.Now.ToString();
                toolStripStatusLabelEventDesc.Text = msg;
            }

            private void exception(string msg)
            {
                _state = STATE.Exception;
                iTwinkle = 0;

                message(msg);
            }

            private void error(string msg)
            {
                _state = STATE.Error;
                iTwinkle = 0;

                message(msg);
            }

            private void action(string msg)
            {
                _state = STATE.Action;
                iTwinkle = -1;

                message(msg);
            }

            private void warning(string msg)
            {
                _state = STATE.Warning;
                iTwinkle = 0;

                message(msg);
            }
        }
    }
}
