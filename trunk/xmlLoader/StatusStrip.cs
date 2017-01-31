using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xmlLoader
{
    partial class FormMain
    {
        private class StatusStrip : System.Windows.Forms.StatusStrip
        {
            private enum STATE { Exception = -2, Error = -1, Ready, Warning }

            private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEventName;
            private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEventDateTime;
            private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEventDesc;

            private STATE _state;

            private System.Threading.Timer m_timerTwinkle;

            public StatusStrip() : base()
            {
                InitializeComponent();

                m_timerTwinkle = new System.Threading.Timer(fTimerTwinkle_callBack, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
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
            }

            public void Exception(string message)
            {
            }

            public void Error(string message)
            {
            }

            public void Action(string message)
            {
                toolStripStatusLabelEventDateTime.Text = DateTime.Now.ToString();
                toolStripStatusLabelEventDesc.Text = message;
            }

            public void Warning(string message)
            {
            }
        }
    }
}
