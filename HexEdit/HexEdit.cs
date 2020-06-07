using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace HexEdit
{
    public partial class HexEdit : UserControl
    {
        [DllImport("user32.dll")] static extern uint GetCaretBlinkTime();

        private static readonly int kIndentX = 40;
        private static readonly int kIndentY = 20;
        private static readonly int kInfoLineNumberOffsetX = 2;
        private static readonly int kInfoLineNumberOffsetY = 3;
        private static readonly int kHexIntervalX = 5;
        private static readonly int kHexIntervalY = 10;

        System.Windows.Forms.Timer _refreshTimer;

        private int _fontWidth;
        private int _fontSize;
        public int FontSize
        {
            set
            {
                _fontSize = value;
                _fontWidth = _fontSize - 3; 
            }
            get 
            {
                return _fontSize; 
            }
        }
        
        private int _horzHexCount;
        public int HorzHexCount
        {
            set
            {
                _horzHexCount = value;

                panel.Left = kIndentX;
                panel.Width = _fontWidth * (_horzHexCount * 2) + kHexIntervalX * _horzHexCount + kCaretOffsetX;
                Width = kIndentX + panel.Width + vScrollBar.Width;
                vScrollBar.Left = kIndentX + panel.Width;
            }
            get
            {
                return _horzHexCount;
            }
        }

        private int _vertHexCount;
        public int VertHexCount
        {
            set
            {
                _vertHexCount = value;

                panel.Top = kIndentY;
                vScrollBar.Top = kIndentY;
                vScrollBar.Height = panel.Height = (_fontSize + kHexIntervalY) * _vertHexCount;
                Height = panel.Height + _fontInfo.Height + kIndentY;
            }
            get
            {
                return _vertHexCount;
            }
        }

        private Font _font;

        private static readonly int kFontInfoSize = 10;
        private Font _fontInfo;

        private string _text = "001234";
        private int _lineCount = 1;
        private int _viewLineOffset = 0;

        private static readonly int kCaretOffsetX = 2;
        private static readonly int kCaretOffsetY = 3;
        private static readonly int kCaretSizeOffY = 4;
        private int _caretBlinkTime;
        private float _caretSize;
        private long _caretPrevTick;

        private int _selectionStart = 0;
        private int _selectionStartX = 0;
        private int _selectionStartY = 0;
        public int SelectionStart 
        {
            set
            {
                _selectionStart = value;
                if (HorzHexCount > 0)
                {
                    _selectionStartX = _selectionStart % (HorzHexCount * 2);
                    _selectionStartY = _selectionStart / (HorzHexCount * 2);
                }

                if (_selectionStartY < _viewLineOffset)
                {
                    vScrollBar.Value = _selectionStartY;
                }
                else if (_selectionStartY >= _viewLineOffset + VertHexCount)
                {
                    updateScrollbar();
                    vScrollBar.Value = _selectionStartY - VertHexCount + 1;
                }

                showCaret(true);
            }
            get
            {
                return _selectionStart;
            }
        }

        public HexEdit()
        {
            InitializeComponent();
            //SetStyle(ControlStyles.Selectable, true);
            
            //AccessibleRole = AccessibleRole.Text;
            Enabled = true;
            Visible = true;
            TabStop = true;
            DoubleBuffered = true;
            CausesValidation = true;
            
            SelectionStart = 0;

            FontSize = 12;
            _font = new Font(new FontFamily("Consolas"), FontSize);
            _fontInfo = new Font(new FontFamily("Consolas"), kFontInfoSize);
            _caretBlinkTime = (int)GetCaretBlinkTime();

            panel.Focus();

            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 10;
            _refreshTimer.Tick += new System.EventHandler(refreshTimer_Tick);
            _refreshTimer.Start();
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            Refresh();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            HorzHexCount = 16;
            VertHexCount = 4;

            updateCaretSize();
            updateLineCount();
            updateScrollbar();
        }

        private void updateCaretSize()
        {
            _caretSize = _font.Height - kCaretSizeOffY;
        }

        private void updateLineCount()
        {
            if (HorzHexCount == 0)
            {
                return;
            }

            _lineCount = _text.Length / (HorzHexCount * 2) + 1;
        }

        public void showCaret(bool value)
        {
            long tickNow = (DateTime.Now.Ticks / 10000);
            if (value == true)
            {
                _caretPrevTick = tickNow - _caretBlinkTime;
            }
            else
            {
                _caretPrevTick = tickNow;
            }

            panel.Invalidate();
        }

        private void drawCaret(PaintEventArgs e)
        {
            int finalX = kCaretOffsetX + _fontWidth * _selectionStartX + kHexIntervalX * (_selectionStartX / 2);
            int finalY = kCaretOffsetY + (_fontSize + kHexIntervalY) * (_selectionStartY - _viewLineOffset);
            e.Graphics.DrawLine(Pens.Black, finalX, finalY, finalX, finalY + _caretSize);
        }

        private void drawText(PaintEventArgs e)
        {
            for (int i = 0; i < _text.Length; ++i)
            {
                int x = i % (HorzHexCount * 2);
                int y = i / (HorzHexCount * 2);

                int finalX = _fontWidth * x + kHexIntervalX * (x / 2);
                int finalY = (_fontSize + kHexIntervalY) * (y - _viewLineOffset);
                e.Graphics.DrawString(_text.Substring(i, 1), _font, Brushes.Black, finalX, finalY);
            }
        }

        private void drawInfo(PaintEventArgs e)
        {
            for (int y = 0; y < VertHexCount; ++y)
            {
                int finalY = kIndentY + (_fontSize + kHexIntervalY) * y;
                e.Graphics.DrawString((_viewLineOffset + y).ToString("D4"), _fontInfo, Brushes.Black,
                    kInfoLineNumberOffsetX, kInfoLineNumberOffsetY + finalY);
            }

            e.Graphics.DrawString("Line: " + _selectionStartY.ToString(), _fontInfo, Brushes.Black, 
                kIndentX + 0, kIndentY + panel.Height);
            e.Graphics.DrawString("Line count: " + _lineCount.ToString(), _fontInfo, Brushes.Black, 
                kIndentX + 70, kIndentY + panel.Height);
            e.Graphics.DrawString("At: " + (_selectionStart / 2).ToString(), _fontInfo, Brushes.Black, 
                kIndentX + 190, kIndentY + panel.Height);
        }

        private void updateScrollbar()
        {
            updateLineCount();

            vScrollBar.SmallChange = 1;
            vScrollBar.LargeChange = 1;

            if (_lineCount > VertHexCount)
            {    
                vScrollBar.Maximum = _lineCount - VertHexCount;
            }
            else
            {
                vScrollBar.Maximum = 0;
            }
        }

        private bool isValidKeyInput(Keys key)
        {
            if (Keys.D0 <= key && key <= Keys.D9)
            {
                return true;
            }

            if (Keys.NumPad0 <= key && key <= Keys.NumPad9)
            {
                return true;
            }

            if (Keys.A <= key && key <= Keys.F)
            {
                return true;
            }

            return false;
        }

        private void panel_Paint(object sender, PaintEventArgs e)
        {
            drawText(e);

            long tickNow = (DateTime.Now.Ticks / 10000);
            if (tickNow > _caretPrevTick + _caretBlinkTime * 2)
            {
                _caretPrevTick = tickNow;
            }
            else if (tickNow > _caretPrevTick + _caretBlinkTime)
            {
                if (panel.Focused == true)
                {
                    drawCaret(e);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            drawInfo(e);
            
            base.OnPaint(e);
        }

        private void panel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                showCaret(true);

                e.IsInputKey = true;
            }

            if (e.KeyCode == Keys.Left)
            {
                SelectionStart = Math.Max(SelectionStart - 1, 0);
            }
            else if (e.KeyCode == Keys.Right)
            {
                SelectionStart = Math.Min(SelectionStart + 1, _text.Length);
            }
            else if (e.KeyCode == Keys.Up)
            {
                int newSelectionStart = _selectionStartX + (_selectionStartY - 1) * (HorzHexCount * 2);
                if (newSelectionStart >= 0)
                {
                    SelectionStart = newSelectionStart;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                int newSelectionStart = _selectionStartX + (_selectionStartY + 1) * (HorzHexCount * 2);
                if (newSelectionStart <= _text.Length)
                {
                    SelectionStart = newSelectionStart;
                }
            }
            else if (e.KeyCode == Keys.Home)
            {
                SelectionStart = 0;
            }
            else if (e.KeyCode == Keys.End)
            {
                SelectionStart = _text.Length;
            }

            // 수정
            if (e.KeyCode == Keys.Back)
            {
                if (SelectionStart % 2 != 0)
                {
                    ++SelectionStart;
                }
                string left = _text.Substring(0, Math.Max(SelectionStart - 2, 0));
                string right = _text.Substring(Math.Min(SelectionStart, _text.Length));
                _text = left + right;
                updateLineCount();

                if (SelectionStart > 0)
                {
                    SelectionStart -= 2;
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (SelectionStart % 2 != 0)
                {
                    --SelectionStart;
                }
                string left = _text.Substring(0, SelectionStart);
                string right = _text.Substring(Math.Min(SelectionStart + 2, _text.Length));
                _text = left + right;
                updateLineCount();
            }

            if (isValidKeyInput(e.KeyCode) == true)
            {
                int keyValue = e.KeyValue;
                if (96 <= keyValue && keyValue <= 96 + 9) // 넘패드 숫자
                {
                    keyValue -= 48;
                }

                string left = _text.Substring(0, SelectionStart);
                string right = _text.Substring(Math.Min(SelectionStart + 1, _text.Length));
                _text = left + (char)keyValue + right;
                updateLineCount();

                ++SelectionStart;
            }

            updateScrollbar();

            panel.Focus();
            panel.Invalidate();
        }

        private void panel_Enter(object sender, EventArgs e)
        {
            showCaret(true);
        }

        private void panel_Leave(object sender, EventArgs e)
        {
            showCaret(false);
        }

        private void vScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            _viewLineOffset = vScrollBar.Value;
        }

        private void vScrollBar_ValueChanged(object sender, EventArgs e)
        {
            _viewLineOffset = vScrollBar.Value;
        }
    }
}
