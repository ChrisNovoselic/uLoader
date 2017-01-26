using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

//!!! Для переноса в HClassLibrary
namespace xmlLoader
{
    /// <summary>
    /// Ячейка представления "кнопка" с возможностью фиксации состояния "нажата"
    /// </summary>
    public class DataGridViewPressedButtonCell : DataGridViewButtonCell
    {
        /// <summary>
        /// Признак состояния нажата/не_нажата
        /// </summary>
        private bool pressedValue;
        /// <summary>
        /// Признак состояния нажата/не_нажата
        /// </summary>
        public bool Pressed
        {
            get { return pressedValue; }

            set { pressedValue = value; Value = value == true ? @"<-" : @"->"; }
        }
        /// <summary>
        /// Override the Clone method so that the Enabled property is copied.
        /// </summary>
        /// <returns>Копия ячейки</returns>
        public override object Clone()
        {
            DataGridViewPressedButtonCell cell = (DataGridViewPressedButtonCell)base.Clone();

            cell.Pressed = this.Pressed;

            return cell;
        }
        /// <summary>
        /// By default, not pressed the button cell
        /// </summary>
        public DataGridViewPressedButtonCell()
        {
            this.pressedValue = false;
        }

        protected override void Paint(Graphics graphics
            , Rectangle clipBounds
            , Rectangle cellBounds
            , int rowIndex
            , DataGridViewElementStates elementState
            , object value
            , object formattedValue
            , string errorText
            , DataGridViewCellStyle cellStyle
            , DataGridViewAdvancedBorderStyle advancedBorderStyle
            , DataGridViewPaintParts paintParts)
        {
            // The button cell is disabled, so paint the border,  
            // background, and disabled button for the cell.
            if (this.pressedValue) {
                // Draw the cell background, if specified.
                if ((paintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background) {
                    SolidBrush cellBackground = new SolidBrush(cellStyle.BackColor);

                    graphics.FillRectangle(cellBackground, cellBounds);

                    cellBackground.Dispose();
                } else
                    ;

                // Draw the cell borders, if specified.
                if ((paintParts & DataGridViewPaintParts.Border) == DataGridViewPaintParts.Border) {
                    PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
                } else
                    ;

                // Calculate the area in which to draw the button.
                Rectangle buttonArea = cellBounds;
                Rectangle buttonAdjustment = this.BorderWidths(advancedBorderStyle);

                buttonArea.X += buttonAdjustment.X;
                buttonArea.Y += buttonAdjustment.Y;

                buttonArea.Height -= buttonAdjustment.Height;
                buttonArea.Width -= buttonAdjustment.Width;

                // Draw the disabled button.                
                ButtonRenderer.DrawButton(graphics, buttonArea, PushButtonState.Pressed);

                // Draw the disabled button text. 
                if (this.FormattedValue is String) {
                    TextRenderer.DrawText(graphics, (string)this.FormattedValue, this.DataGridView.Font, buttonArea, SystemColors.ControlText);
                } else
                    ;
            } else {
            // The button cell is enabled, so let the base class 
            // handle the painting.
                base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
            }
        }

        //protected override void OnClick(DataGridViewCellEventArgs e)
        //{
        //    if (Enabled == true)
        //        base.OnClick(e);
        //    else
        //        ;
        //}
    }
    /// <summary>
    /// Столбец для представления с типом ячеек "кнопка", с возможностью фиксации состояния "нажата"
    /// </summary>
    public class DataGridViewPressedButtonColumn : DataGridViewButtonColumn
    {
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public DataGridViewPressedButtonColumn()
        {
            this.CellTemplate = new DataGridViewPressedButtonCell();
        }
    }

}
