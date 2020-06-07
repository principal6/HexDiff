using System.Drawing;
using System.Windows.Forms;

namespace DynamicTabControl
{
    public class DynamicTabControl : TabControl
    {
        private TabPage pressedTabPage = null;

        public DynamicTabControl()
        {
            this.AllowDrop = true;
        }

        private TabPage getInteractingTab()
        {
            for (int i = 0; i < this.TabPages.Count; i++)
            {
                Rectangle tabRect = this.GetTabRect(i);
                if (tabRect.Contains(this.PointToClient(Cursor.Position)) == true)
                {
                    return this.TabPages[i];
                }
            }

            return null;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            pressedTabPage = getInteractingTab();

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && pressedTabPage != null)
            {
                this.DoDragDrop(pressedTabPage, DragDropEffects.Move);
            }
            
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            pressedTabPage = null;

            base.OnMouseUp(e);
        }

        private void swapTabPages(TabPage src, TabPage dst)
        {
            if (src == null || dst == null)
            {
                return;
            }

            int srci = this.TabPages.IndexOf(src);
            int dsti = this.TabPages.IndexOf(dst);
            this.TabPages[dsti] = src;
            this.TabPages[srci] = dst;

            if (this.SelectedIndex == srci)
            {
                this.SelectedIndex = dsti;
            }
            else if (this.SelectedIndex == dsti)
            {
                this.SelectedIndex = srci;
            }

            this.Refresh();
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            TabPage interactingTab = getInteractingTab();
            if (interactingTab == pressedTabPage)
            {
                drgevent.Effect = DragDropEffects.Move;
            }
            else
            {
                swapTabPages(interactingTab, pressedTabPage);
            }

            base.OnDragOver(drgevent);
        }
    }
}
