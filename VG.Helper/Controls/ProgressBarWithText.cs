using System;
using System.Drawing;
using System.Windows.Forms;


namespace VG.Helper.Controls
{
    public partial class ProgressBarWithText : ProgressBar
    {
        /// <summary>
        /// Optional custom text to display. If empty, shows percentage.
        /// </summary>
        public string CustomText { get; set; } = string.Empty;

        /// <summary>
        /// Color of the filled progress portion.
        /// </summary>
        public Color ProgressColor { get; set; } = Color.Green;

        /// <summary>
        /// Color of the text drawn on top.
        /// </summary>
        public Color TextColor { get; set; } = Color.Black;

        /// <summary>
        /// Font used for the text.
        /// </summary>
        public Font TextFont { get; set; } = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);

        public ProgressBarWithText()
        {
            // Enable custom painting
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = e.ClipRectangle;

            // Draw background
            using (SolidBrush backgroundBrush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillRectangle(backgroundBrush, rect);
            }

            // Draw filled portion
            rect.Width = (int)(rect.Width * ((double)this.Value / this.Maximum));
            if (rect.Width > 0)
            {
                using (SolidBrush progressBrush = new SolidBrush(this.ProgressColor))
                {
                    e.Graphics.FillRectangle(progressBrush, 0, 0, rect.Width, rect.Height);
                }
            }

            // Decide text to draw
            string text = string.IsNullOrEmpty(CustomText)
                ? $"{Value * 100 / Maximum}%"
                : CustomText;

            // Draw text centered
            SizeF textSize = e.Graphics.MeasureString(text, TextFont);
            PointF location = new PointF(
                (this.Width - textSize.Width) / 2,
                (this.Height - textSize.Height) / 2
            );

            using (SolidBrush textBrush = new SolidBrush(this.TextColor))
            {
                e.Graphics.DrawString(text, TextFont, textBrush, location);
            }
        }
    }


}
