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

                if (_fontInfo != null)
                {
                    int newHeight = panel.Height + _fontInfo.Height + kIndentY;
                    if (0 < HorzHexCount && HorzHexCount < 16)
                    {
                        newHeight += kFontInfoSizePadded * 2;
                    }
                    Height = newHeight;
                }
            }
            get
            {
                return _vertHexCount;
            }
        }

        private static readonly int kDefaultFontSize = 12;
        private Font _font;

        private static readonly int kFontInfoSize = 10;
        private static readonly int kFontInfoSizePadded = kFontInfoSize + 2;
        private Font _fontInfo;

        private List<byte> _bytes = new List<byte>();
        private bool _isInserting = false;
        private int _lineCount = 1;
        private int _viewLineOffset = 0;

        private static readonly int kCaretOffsetX = 2;
        private static readonly int kCaretOffsetY = 3;
        private static readonly int kCaretSizeOffY = 4;
        private int _caretBlinkTime;
        private float _caretSize;
        private long _caretPrevTick;

        private int _caretAt = 0;
        private int _caretAtX = 0;
        private int _caretAtY = 0;
        public int CaretAt 
        {
            set
            {
                _caretAt = Math.Max(Math.Min(value, _bytes.Count), 0);

                if (HorzHexCount > 0)
                {
                    _caretAtX = _caretAt % HorzHexCount;
                    _caretAtY = _caretAt / HorzHexCount;
                }

                if (_caretAtY < _viewLineOffset)
                {
                    vScrollBar.Value = _caretAtY;
                }
                else if (_caretAtY >= _viewLineOffset + VertHexCount)
                {
                    updateScrollbar();
                    vScrollBar.Value = _caretAtY - VertHexCount + 1;
                }

                _isInserting = false;
                showCaret(true);
            }
            get
            {
                return _caretAt;
            }
        }

        public HexEdit()
        {
            InitializeComponent();
            //SetStyle(ControlStyles.Selectable, true);
            
            Enabled = true;
            Visible = true;
            TabStop = true;
            DoubleBuffered = true;
            CausesValidation = true;
            
            FontSize = kDefaultFontSize;
            _font = new Font(new FontFamily("Consolas"), FontSize);
            _fontInfo = new Font(new FontFamily("Consolas"), kFontInfoSize);
            _caretBlinkTime = (int)GetCaretBlinkTime();

            panel.Focus();
            panel.MouseWheel += Panel_MouseWheel;

            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 10;
            _refreshTimer.Tick += new System.EventHandler(refreshTimer_Tick);
            _refreshTimer.Start();

            CaretAt = 0;
            HorzHexCount = 16;
            VertHexCount = 8;
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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            HorzHexCount = HorzHexCount;
            VertHexCount = VertHexCount;

            updateCaretSize();
            updateLineCount();
            updateScrollbar();
        }

        private void updateCaretSize()
        {
            if (_font != null)
            {
                _caretSize = _font.Height - kCaretSizeOffY;
            }
        }

        private void updateLineCount()
        {
            if (HorzHexCount == 0)
            {
                return;
            }

            if (_bytes.Count == 0)
            {
                _lineCount = 0;
                return;
            }
            _lineCount = (_bytes.Count - 1) / HorzHexCount + 1;
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
            int finalX = kCaretOffsetX + _fontWidth * _caretAtX * 2 + kHexIntervalX * _caretAtX;
            int finalY = kCaretOffsetY + (_fontSize + kHexIntervalY) * (_caretAtY - _viewLineOffset);
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
                if (i == CaretAt && _isInserting == true)
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

            if (HorzHexCount >= 16)
            {
                e.Graphics.DrawString("Lines: " + _lineCount.ToString(), _fontInfo, Brushes.Black,
                kIndentX + 0, kIndentY + panel.Height);

                e.Graphics.DrawString("Line At: " + _caretAtY.ToString(), _fontInfo, Brushes.Black,
                    kIndentX + 100, kIndentY + panel.Height);

                e.Graphics.DrawString("Byte At: " + _caretAt.ToString() + " (0x" + _caretAt.ToString("X") + ")",
                    _fontInfo, Brushes.Black, kIndentX + 220, kIndentY + panel.Height);
            }
            else
            {
                e.Graphics.DrawString("Lines: " + _lineCount.ToString(), _fontInfo, Brushes.Black,
                    kIndentX, kIndentY + panel.Height);

                e.Graphics.DrawString("Line At: " + _caretAtY.ToString(), _fontInfo, Brushes.Black,
                    kIndentX, kIndentY + panel.Height + kFontInfoSizePadded);

                e.Graphics.DrawString("Byte At: " + _caretAt.ToString() + " (0x" + _caretAt.ToString("X") + ")",
                    _fontInfo, Brushes.Black, kIndentX, kIndentY + panel.Height + kFontInfoSizePadded * 2);
            }
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
            if (tickNow >= _caretPrevTick + _caretBlinkTime * 2)
            {
                _caretPrevTick = tickNow;
            }
            else if (tickNow >= _caretPrevTick + _caretBlinkTime)
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
            // Ctrl + X
            if (e.Control == true && e.KeyCode == Keys.X)
            {
                if (_bytes.Count > 0)
                {
                    int byteAt = Math.Min(CaretAt, _bytes.Count - 1);
                    byte caretByte = _bytes[byteAt];
                    string byteStr = caretByte.ToString("X");
                    Clipboard.SetText(byteStr);

                    _bytes.RemoveAt(byteAt);

                    if (CaretAt > _bytes.Count)
                    {
                        CaretAt = _bytes.Count;
                    }
                }
                return;
            }

            // Ctrl + C
            if (e.Control == true && e.KeyCode == Keys.C)
            {
                if (_bytes.Count > 0)
                {
                    int byteAt = Math.Min(CaretAt, _bytes.Count - 1);
                    byte caretByte = _bytes[byteAt];
                    string byteStr = caretByte.ToString("X");
                    Clipboard.SetText(byteStr);
                }
                return;
            }

            // Ctrl + V
            if (e.Control == true && e.KeyCode == Keys.V)
            {
                string clipboardStr = Clipboard.GetText();
                if (clipboardStr != null)
                {
                    int byteCountGuess = clipboardStr.Length / 2;
                    byte parsedByte;
                    for (int i = 0; i < byteCountGuess; ++i)
                    {
                        if (byte.TryParse(clipboardStr.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber, null, out parsedByte) == true)
                        {
                            _bytes.Insert(CaretAt, parsedByte);
                            ++CaretAt;
                        }
                    }
                }
                return;
            }

            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                showCaret(true);

                e.IsInputKey = true;
            }

            if (e.KeyCode == Keys.Left)
            {
                --CaretAt;
            }
            else if (e.KeyCode == Keys.Right)
            {
                ++CaretAt;
            }
            else if (e.KeyCode == Keys.Up)
            {
                int newSelectionStart = _caretAtX + (_caretAtY - 1) * HorzHexCount;
                if (newSelectionStart >= 0)
                {
                    CaretAt = newSelectionStart;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                int newSelectionStart = _caretAtX + (_caretAtY + 1) * HorzHexCount;
                if (_lineCount > _caretAtY + 1)
                {
                    CaretAt = newSelectionStart;
                }
            }
            else if (e.KeyCode == Keys.Home)
            {
                CaretAt = 0;
            }
            else if (e.KeyCode == Keys.End)
            {
                CaretAt = _bytes.Count;
            }

            // 수정
            if (e.KeyCode == Keys.Back)
            {
                if (_bytes.Count > 0 && CaretAt > 0)
                {
                    if (_isInserting == true)
                    {
                        _bytes.RemoveAt(CaretAt);
                        _isInserting = false;
                    }
                    else
                    {
                       _bytes.RemoveAt(CaretAt - 1);
                        --CaretAt;
                    }
                    
                    updateLineCount();
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (_bytes.Count > 0 && CaretAt < _bytes.Count)
                {
                    _bytes.RemoveAt(CaretAt);
                    updateLineCount();

                    _isInserting = false;
                }
            }

            if (isValidKeyInput(e.KeyCode) == true)
            {
                int keyValue = e.KeyValue;
                if (96 <= keyValue && keyValue <= 96 + 9) // 넘패드 숫자
                {
                    keyValue -= 48;
                }

                if (_isInserting == true)
                {
                    byte currByte = _bytes[CaretAt];
                    string hexStr = currByte.ToString("X");
                    hexStr = hexStr.Substring(0, 1) + (char)keyValue;
                    byte parsed = byte.Parse(hexStr, System.Globalization.NumberStyles.HexNumber);
                    _bytes[CaretAt] = parsed;

                    updateLineCount();
                    ++CaretAt;
                }
                else
                {
                    string hexStr = "";
                    hexStr += (char)keyValue;
                    hexStr += "0";
                    byte parsed = byte.Parse(hexStr, System.Globalization.NumberStyles.HexNumber);
                    _bytes.Insert(CaretAt, parsed);
                    updateLineCount();

                    _isInserting = true;
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

        private int calculateCaretAtByMousePosition(int mouseX, int mouseY)
        {
            int atX = Math.Min(Math.Max(mouseX / (_fontWidth * 2 + kHexIntervalX), 0), HorzHexCount - 1);
            int atY = Math.Min(Math.Max(_viewLineOffset + (mouseY / (_fontSize + kHexIntervalY)), 0), _lineCount - 1);
            return Math.Min(Math.Max(atY * HorzHexCount + atX, 0), _bytes.Count);
        }

        private void panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                CaretAt = calculateCaretAtByMousePosition(e.X, e.Y);
            }
        }

        private void panel_MouseDown(object sender, MouseEventArgs e)
        {
            CaretAt = calculateCaretAtByMousePosition(e.X, e.Y);
        }
    }
}
