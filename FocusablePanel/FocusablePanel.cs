using System;
using System.Windows.Forms;

namespace FocusablePanel
{
    public class FocusablePanel : Panel
    {
        public FocusablePanel()
        {
            SetStyle(ControlStyles.Selectable, true);
            DoubleBuffered = true;
            TabStop = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();
            base.OnMouseDown(e);
        }

        protected override void OnEnter(EventArgs e)
        {
            Invalidate();
            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            Invalidate();
        }
    }
}
