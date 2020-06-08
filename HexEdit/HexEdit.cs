using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace HexEdit
{
    public partial class HexEdit : UserControl
    {
        [DllImport("user32.dll")] static extern uint GetCaretBlinkTime();

        private static readonly int kIndentX = 40;
        private static readonly int kIndentY = 10;
        private static readonly int kInfoLineNumberOffsetX = 2;
        private static readonly int kInfoLineNumberOffsetY = 3;
        private static readonly int kHexIntervalX = 5;
        private static readonly int kHexIntervalY = 10;
        private static readonly int kMidLineHeight = (int)(kIndentY * 0.8);

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

        //private string _text = "001234";
        private bool _inserting = false;
        private List<byte> _bytes = new List<byte> { 0x00, 0x12, 0x34 };
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
                    _selectionStartX = _selectionStart % HorzHexCount;
                    _selectionStartY = _selectionStart / HorzHexCount;
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

                _inserting = false;
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
            panel.MouseWheel += Panel_MouseWheel;

            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 10;
            _refreshTimer.Tick += new System.EventHandler(refreshTimer_Tick);
            _refreshTimer.Start();
        }

        private void Panel_MouseWheel(object sender, MouseEventArgs e)
        {
            int delta = -e.Delta / SystemInformation.MouseWheelScrollDelta;
            int newValue = vScrollBar.Value + delta;
            newValue = Math.Max(newValue, 0);
            newValue = Math.Min(newValue, vScrollBar.Maximum);
            vScrollBar.Value = newValue;
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

            _lineCount = _bytes.Count / HorzHexCount + 1;
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
            int finalX = kCaretOffsetX + _fontWidth * _selectionStartX * 2 + kHexIntervalX * _selectionStartX;
            int finalY = kCaretOffsetY + (_fontSize + kHexIntervalY) * (_selectionStartY - _viewLineOffset);
            e.Graphics.DrawLine(Pens.Black, finalX, finalY, finalX, finalY + _caretSize);
        }

        private void drawHexString(PaintEventArgs e)
        {
            for (int i = 0; i < _bytes.Count; ++i)
            {
                int x = i % HorzHexCount;
                int y = i / HorzHexCount;

                int finalX = _fontWidth * (x * 2) + kHexIntervalX * x;
                int finalY = (_fontSize + kHexIntervalY) * (y - _viewLineOffset);

                string hexStr = _bytes[i].ToString("X");
                if (hexStr.Length == 1)
                {
                    hexStr = "0" + hexStr;
                }

                Brush brush = Brushes.Black;
                if (i == SelectionStart && _inserting == true)
                {
                    brush = Brushes.DarkRed;
                }
                e.Graphics.DrawString(hexStr.Substring(0, 1), _font, brush, finalX, finalY);
                
                finalX += kHexIntervalX * 2;
                e.Graphics.DrawString(hexStr.Substring(1, 1), _font, brush, finalX, finalY);
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

            int midLineX = kIndentX + panel.Width / 2;
            int midLineY = (kIndentY - kMidLineHeight) / 2;
            e.Graphics.DrawLine(Pens.Black, midLineX, midLineY, midLineX, midLineY + kMidLineHeight);

            e.Graphics.DrawString("Line: " + _selectionStartY.ToString(), _fontInfo, Brushes.Black, 
                kIndentX + 0, kIndentY + panel.Height);
            e.Graphics.DrawString("Line count: " + _lineCount.ToString(), _fontInfo, Brushes.Black, 
                kIndentX + 70, kIndentY + panel.Height);

            e.Graphics.DrawString("At: " + _selectionStart.ToString() + " (0x" + _selectionStart.ToString("X") + ")",
                _fontInfo, Brushes.Black, kIndentX + 190, kIndentY + panel.Height);
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
            drawHexString(e);

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
                SelectionStart = Math.Min(SelectionStart + 1, _bytes.Count);
            }
            else if (e.KeyCode == Keys.Up)
            {
                int newSelectionStart = _selectionStartX + (_selectionStartY - 1) * HorzHexCount;
                if (newSelectionStart >= 0)
                {
                    SelectionStart = newSelectionStart;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                int newSelectionStart = _selectionStartX + (_selectionStartY + 1) * HorzHexCount;
                if (_lineCount > _selectionStartY + 1)
                {
                    SelectionStart = Math.Min(newSelectionStart, _bytes.Count);
                }
            }
            else if (e.KeyCode == Keys.Home)
            {
                SelectionStart = 0;
            }
            else if (e.KeyCode == Keys.End)
            {
                SelectionStart = _bytes.Count;
            }

            // 수정
            if (e.KeyCode == Keys.Back)
            {
                if (_bytes.Count > 0)
                {
                    _bytes.RemoveAt(SelectionStart - 1);
                    updateLineCount();

                    if (SelectionStart > 0)
                    {
                        --SelectionStart;
                    }
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (_bytes.Count > 0)
                {
                    _bytes.RemoveAt(SelectionStart);
                    updateLineCount();
                }
            }

            if (isValidKeyInput(e.KeyCode) == true)
            {
                int keyValue = e.KeyValue;
                if (96 <= keyValue && keyValue <= 96 + 9) // 넘패드 숫자
                {
                    keyValue -= 48;
                }

                if (_inserting == true)
                {
                    byte currByte = _bytes[SelectionStart];
                    string hexStr = currByte.ToString("X");
                    hexStr = hexStr.Substring(0, 1) + (char)keyValue;
                    byte parsed = byte.Parse(hexStr, System.Globalization.NumberStyles.HexNumber);
                    _bytes[SelectionStart] = parsed;

                    updateLineCount();
                    ++SelectionStart;
                }
                else
                {
                    string hexStr = "";
                    hexStr += (char)keyValue;
                    hexStr += "0";
                    byte parsed = byte.Parse(hexStr, System.Globalization.NumberStyles.HexNumber);
                    _bytes.Insert(SelectionStart, parsed);
                    updateLineCount();

                    _inserting = true;
                }
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
