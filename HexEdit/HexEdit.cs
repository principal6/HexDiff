using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;

namespace HexEditProject
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

        // Timer
        System.Windows.Forms.Timer _refreshTimer;

        private static readonly Color kEditBackColor = Color.FromArgb(240, 240, 255);
        private Color _editBackColor;
        public Color EditBackColor
        {
            set 
            {
                _editBackColor = value;
                if (panel != null)
                {
                    panel.BackColor = _editBackColor;
                }
            }
            get
            {
                return _editBackColor;
            }
        }

        // === Font
        private static readonly FontFamily kDefaultFontFamily = new FontFamily("Consolas");
        private static readonly int kDefaultFontSize = 12;
        private int _fontWidth = kDefaultFontSize - 3;

        private static readonly int kFontInfoSize = 10;
        private static readonly int kFontInfoSizePadded = kFontInfoSize + 2;

        private static readonly Color kInfoTextColorDisabled = Color.FromArgb(127, 127, 127);
        private Color _infoTextColorEnabled = Color.Black;
        private Color _infoTextColor;
        [Description("줄 번호, 줄 개수 등 정보 텍스트의 색상입니다.")]
        public Color InfoTextColor
        {
            set
            {
                _infoTextColorEnabled = value;
                updateFontInfoColor();
            }
            get
            {
                return _infoTextColorEnabled;
            }
        }

        private Font _fontInfo;

        private Color _insertingForeColor = Color.DarkRed;
        [Description("입력 중인 Hex Digit 이 가질 색상입니다.")]
        public Color InsertingForeColor
        {
            set
            {
                _insertingForeColor = value;
            }
            get
            {
                return _insertingForeColor;
            }
        }
        // ===

        private Color _selectionColor = Color.FromArgb(80, 160, 255);
        public Color SelectionColor
        {
            set
            {
                _selectionColor = value;
            }
            get
            {
                return _selectionColor;
            }
        }

        // HorzHexCount
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

        // VertHexCount
        private int _vertHexCount;
        public int VertHexCount
        {
            set
            {
                _vertHexCount = value;

                panel.Top = kIndentY;
                vScrollBar.Top = kIndentY;
                vScrollBar.Height = panel.Height = ((int)Font.Size + kHexIntervalY) * _vertHexCount;

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

        private List<byte> _bytes = new List<byte>();
        private bool _isInputDone = true;
        private int _lineCount = 1;
        private int _viewLineOffset = 0;

        // === Caret
        private static readonly int kCaretOffsetX = 2;
        private static readonly int kCaretOffsetY = 3;
        private static readonly int kCaretSizeOffY = 4;
        private int _caretBlinkTime;
        private float _caretSize;
        private long _caretPrevTick;
        private int _caretAt = 0;
        private int _caretAtX = 0;
        private int _caretAtY = 0;
        private int CaretAt 
        {
            set
            {
                _caretAt = Math.Max(Math.Min(value, _bytes.Count), 0);

                Point xy = getSelXyFromIndex(_caretAt);
                _caretAtX = xy.X;
                _caretAtY = xy.Y;

                updateLineCount();
                updateScrollbar();

                if (_caretAtY < _viewLineOffset)
                {
                    vScrollBar.Value = _caretAtY;
                }
                else if (_caretAtY >= _viewLineOffset + VertHexCount)
                {
                    vScrollBar.Value = _caretAtY - VertHexCount + 1;
                }

                _isInputDone = true;
                
                showCaret(true);
            }
            get
            {
                return _caretAt;
            }
        }
        // ===

        // === Selection
        private int _selectionStart = 0;
        public int SelectionStart
        {
            set
            {
                _selectionStart = Math.Max(Math.Min(value, _bytes.Count), 0);
            }
            get
            {
                return _selectionStart;
            }
        }

        private int _selectionLength = 0;
        public int SelectionLength
        {
            set
            {
                _selectionLength = Math.Max(Math.Min(value, _bytes.Count - SelectionStart), 0);
            }
            get
            {
                return _selectionLength;
            }
        }
        // ===

        public bool HasSelection()
        {
            return SelectionLength > 0;
        }

        public HexEdit()
        {
            InitializeComponent();

            Visible = true;
            TabStop = true;
            DoubleBuffered = true;
            CausesValidation = true;

            EditBackColor = kEditBackColor;
            Font = new Font(kDefaultFontFamily, kDefaultFontSize);
            _fontInfo = new Font(kDefaultFontFamily, kFontInfoSize);
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

            updateFontInfoColor();
        }

        private Point getSelXyFromIndex(int index)
        {
            Point result = new Point(0, 0);
            if (HorzHexCount > 0)
            {
                result.X = index % HorzHexCount;
                result.Y = index / HorzHexCount;
            }
            return result;
        }

        private int getSelIndexFromXy(int x, int y)
        {
            return (x + y * HorzHexCount);
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

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            _fontWidth = (int)Font.Size - 3;
        }

        private void updateFontInfoColor()
        {
            if (Enabled == true)
            {
                _infoTextColor = _infoTextColorEnabled;
            }
            else
            {
                _infoTextColor = kInfoTextColorDisabled;
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            updateFontInfoColor();

            Refresh();
        }

        private void updateCaretSize()
        {
            _caretSize = Font.Height - kCaretSizeOffY;
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
            int finalX = kCaretOffsetX + _fontWidth * _caretAtX * 2 + kHexIntervalX * _caretAtX;
            int finalY = kCaretOffsetY + ((int)Font.Size + kHexIntervalY) * (_caretAtY - _viewLineOffset);
            e.Graphics.DrawLine(Pens.Black, finalX, finalY, finalX, finalY + _caretSize);
        }

        private void drawHexString(PaintEventArgs e)
        {
            for (int i = 0; i < _bytes.Count; ++i)
            {
                int x = i % HorzHexCount;
                int y = i / HorzHexCount;

                int finalX = _fontWidth * (x * 2) + kHexIntervalX * x;
                int finalY = ((int)Font.Size + kHexIntervalY) * (y - _viewLineOffset);

                string hexStr = _bytes[i].ToString("X");
                if (hexStr.Length == 1)
                {
                    hexStr = "0" + hexStr;
                }

                Brush brush = new SolidBrush(ForeColor);
                if (i == CaretAt && _isInputDone == false)
                {
                    brush = new SolidBrush(InsertingForeColor);
                }
                e.Graphics.DrawString(hexStr.Substring(0, 1), Font, brush, finalX, finalY);
                
                finalX += kHexIntervalX * 2;
                e.Graphics.DrawString(hexStr.Substring(1, 1), Font, brush, finalX, finalY);
            }
        }

        private void drawSelection(PaintEventArgs e)
        {
            if (SelectionLength > 0)
            {
                Brush brush = new SolidBrush(Color.FromArgb(100, SelectionColor.R, SelectionColor.G, SelectionColor.B));

                int selectionEnd = SelectionStart + SelectionLength;
                Point selectionStartXy = getSelXyFromIndex(SelectionStart);
                Point selectionEndXy = getSelXyFromIndex(selectionEnd);

                int selectionLineCount = selectionEndXy.Y - selectionStartXy.Y + 1;
                if (selectionLineCount == 1)
                {
                    int pixelX = kCaretOffsetX + _fontWidth * selectionStartXy.X * 2 + kHexIntervalX * selectionStartXy.X;
                    int pixelY = kCaretOffsetY + ((int)Font.Size + kHexIntervalY) * (selectionStartXy.Y - _viewLineOffset);

                    int pixelWidth = _fontWidth * SelectionLength * 2 + kHexIntervalX * SelectionLength;
                    e.Graphics.FillRectangle(brush, pixelX, pixelY, pixelWidth, Font.Height);
                }
                else
                {
                    for (int i = 0; i < selectionLineCount; ++i)
                    {
                        int pixelX = kCaretOffsetX;
                        int currentLineSelectionLength;
                        if (i == 0)
                        {
                            // 첫번째 줄
                            pixelX = kCaretOffsetX + _fontWidth * selectionStartXy.X * 2 + kHexIntervalX * selectionStartXy.X;
                            currentLineSelectionLength = HorzHexCount - selectionStartXy.X;
                        }
                        else if (i == selectionLineCount - 1)
                        {
                            // 마지막 줄
                            currentLineSelectionLength = selectionEndXy.X;
                        }
                        else
                        {
                            // 중간 줄
                            currentLineSelectionLength = HorzHexCount;
                        }

                        int lineAt = selectionStartXy.Y + i;
                        int pixelY = kCaretOffsetY + ((int)Font.Size + kHexIntervalY) * (lineAt - _viewLineOffset);
                        int pixelWidth = (_fontWidth * 2 + kHexIntervalX) * currentLineSelectionLength;
                        e.Graphics.FillRectangle(brush, pixelX, pixelY, pixelWidth, Font.Height);
                    }
                }
            }
        }

        private void drawInfo(PaintEventArgs e)
        {
            Brush brush = new SolidBrush(_infoTextColor);

            for (int y = 0; y < VertHexCount; ++y)
            {
                int finalY = kIndentY + ((int)Font.Size + kHexIntervalY) * y;
                e.Graphics.DrawString((_viewLineOffset + y).ToString("D4"), _fontInfo, brush,
                    kInfoLineNumberOffsetX, kInfoLineNumberOffsetY + finalY);
            }

            int midLineX = kIndentX + panel.Width / 2;
            int midLineY = (kIndentY - kMidLineHeight) / 2;
            e.Graphics.DrawLine(Pens.Black, midLineX, midLineY, midLineX, midLineY + kMidLineHeight);

            if (HorzHexCount >= 16)
            {
                e.Graphics.DrawString("Lines: " + _lineCount.ToString(), _fontInfo, brush,
                kIndentX + 0, kIndentY + panel.Height);

                e.Graphics.DrawString("Line At: " + _caretAtY.ToString(), _fontInfo, brush,
                    kIndentX + 100, kIndentY + panel.Height);

                e.Graphics.DrawString("Byte At: " + _caretAt.ToString() + " (0x" + _caretAt.ToString("X") + ")",
                    _fontInfo, brush, kIndentX + 220, kIndentY + panel.Height);
            }
            else
            {
                e.Graphics.DrawString("Lines: " + _lineCount.ToString(), _fontInfo, brush,
                    kIndentX, kIndentY + panel.Height);

                e.Graphics.DrawString("Line At: " + _caretAtY.ToString(), _fontInfo, brush,
                    kIndentX, kIndentY + panel.Height + kFontInfoSizePadded);

                e.Graphics.DrawString("Byte At: " + _caretAt.ToString() + " (0x" + _caretAt.ToString("X") + ")",
                    _fontInfo, brush, kIndentX, kIndentY + panel.Height + kFontInfoSizePadded * 2);
            }
        }

        private void updateScrollbar()
        {
            updateLineCount();

            vScrollBar.SmallChange = 1;
            vScrollBar.LargeChange = 1;

            if (_lineCount >= VertHexCount)
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

            drawSelection(e);

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
            int oldCaretAt = CaretAt;

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
                    SelectionLength = 0;

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
                e.IsInputKey = true;
            }

            int newCaretAt;
            switch (e.KeyCode)
            {
                case Keys.Left:
                    --CaretAt;
                    break;
                case Keys.Right:
                    ++CaretAt;
                    break;
                case Keys.Up:
                    newCaretAt = getSelIndexFromXy(_caretAtX, _caretAtY - 1);
                    if (newCaretAt >= 0)
                    {
                        CaretAt = newCaretAt;
                    }
                    break;
                case Keys.Down:
                    newCaretAt = getSelIndexFromXy(_caretAtX, _caretAtY + 1);
                    if (_lineCount > _caretAtY + 1)
                    {
                        CaretAt = newCaretAt;
                    }
                    break;
                case Keys.Home:
                    CaretAt = 0;
                    break;
                case Keys.End:
                    CaretAt = _bytes.Count;
                    break;
            }

            // 수정
            if (e.KeyCode == Keys.Back)
            {
                deleteBefore();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                deleteAfter();
            }

            if (isValidKeyInput(e.KeyCode) == true)
            {
                int keyValue = e.KeyValue;
                if (96 <= keyValue && keyValue <= 96 + 9) // 넘패드 숫자
                {
                    keyValue -= 48;
                }

                if (_isInputDone == true)
                {
                    deleteSelection();

                    string hexStr = "";
                    hexStr += (char)keyValue;
                    hexStr += "0";
                    byte parsed = byte.Parse(hexStr, System.Globalization.NumberStyles.HexNumber);
                    _bytes.Insert(CaretAt, parsed);
                    updateLineCount();

                    _isInputDone = false;
                }
                else
                {
                    byte currByte = _bytes[CaretAt];
                    string hexStr = currByte.ToString("X");
                    hexStr = hexStr.Substring(0, 1) + (char)keyValue;
                    byte parsed = byte.Parse(hexStr, System.Globalization.NumberStyles.HexNumber);
                    _bytes[CaretAt] = parsed;

                    updateLineCount();
                    ++CaretAt;
                }
            }

            updateSelection(e.Shift, oldCaretAt);

            updateScrollbar();

            panel.Focus();
            panel.Invalidate();
        }

        private void deleteBefore()
        {
            if (deleteSelection() == true)
            {
                return;
            }

            if (_bytes.Count > 0 && CaretAt > 0)
            {
                if (_isInputDone == true)
                {
                    _bytes.RemoveAt(CaretAt - 1);
                    --CaretAt;
                }
                else
                {
                    _bytes.RemoveAt(CaretAt);
                    _isInputDone = true;
                }

                updateLineCount();
            }
        }
        
        private void deleteAfter()
        {
            if (deleteSelection() == true)
            {
                return;
            }

            if (_bytes.Count > 0 && CaretAt < _bytes.Count)
            {
                _bytes.RemoveAt(CaretAt);
                updateLineCount();

                _isInputDone = true;
            }
        }

        private bool deleteSelection()
        {
            if (HasSelection() == true)
            {
                CaretAt = SelectionStart;
                _bytes.RemoveRange(SelectionStart, SelectionLength);
                SelectionLength = 0;
                return true;
            }
            return false;
        }

        private void updateSelection(bool maintainSelection, int oldCaretAt)
        {
            // Caret의 위치가 변경되었을 때
            if (maintainSelection == true)
            {
                if (oldCaretAt != CaretAt)
                {
                    // Caret의 깜빡임을 초기화한다.
                    showCaret(true);

                    if (HasSelection() == false)
                    {
                        if (CaretAt < oldCaretAt)
                        {
                            SelectionStart = CaretAt;
                            SelectionLength = oldCaretAt - SelectionStart;
                        }
                        else
                        {
                            SelectionStart = oldCaretAt;
                            SelectionLength = CaretAt - SelectionStart;
                        }
                    }
                    else
                    {
                        int selectionEnd = SelectionStart + SelectionLength;
                        if (oldCaretAt == SelectionStart)
                        {
                            SelectionStart = CaretAt;
                            SelectionLength = selectionEnd - SelectionStart;
                        }
                        else
                        {
                            SelectionLength = CaretAt - SelectionStart;
                        }
                    }
                }
            }
            else
            {
                SelectionLength = 0;
            }
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
            int atY = Math.Min(Math.Max(_viewLineOffset + (mouseY / ((int)Font.Size + kHexIntervalY)), 0), _lineCount - 1);
            return Math.Min(Math.Max(atY * HorzHexCount + atX, 0), _bytes.Count);
        }

        private void panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int oldCaretAt = CaretAt;
                CaretAt = calculateCaretAtByMousePosition(e.X, e.Y);

                if (oldCaretAt != CaretAt)
                {
                    updateSelection(true, oldCaretAt);
                }
            }
        }

        private void panel_MouseDown(object sender, MouseEventArgs e)
        {
            int oldCaretAt = CaretAt;
            CaretAt = calculateCaretAtByMousePosition(e.X, e.Y);

            if (oldCaretAt == CaretAt)
            {
                updateSelection(false, oldCaretAt);
            }
            else
            {
                bool isShiftPressed = false;
                if ((ModifierKeys & Keys.Shift) != 0)
                {
                    isShiftPressed = true;
                }
                updateSelection(isShiftPressed, oldCaretAt);
            }
        }
    }
}
