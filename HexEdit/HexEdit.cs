using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace HexEdit
{
    public partial class HexEdit : UserControl
    {

        [DllImport("user32.dll")] static extern uint GetCaretBlinkTime();

        private static readonly int kHexIntervalX = 5;
        private static readonly int kHexIntervalY = 10;
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

                Width = _fontWidth * (_horzHexCount * 2) + kHexIntervalX * _horzHexCount + kCaretOffsetX * 2;
                Height = (_fontSize + kHexIntervalY) * 4;
            }
            get
            {
                return _horzHexCount;
            }
        }

        private Font _font;
        private string _text = "001234";

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
            SetStyle(ControlStyles.Selectable, true);

            AccessibleRole = AccessibleRole.Text;
            Enabled = true;
            Visible = true;
            TabStop = true;
            DoubleBuffered = true;
            CausesValidation = true;
            
            SelectionStart = 0;

            FontSize = 12;
            _font = new Font(new FontFamily("Consolas"), FontSize);
            _caretBlinkTime = (int)GetCaretBlinkTime();

            updateCaretSize();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            HorzHexCount = 16;
        }

        private void updateCaretSize()
        {
            _caretSize = _font.Height - kCaretSizeOffY;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            
            Focus();

            showCaret(true);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            
            showCaret(false);
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

            Invalidate();
        }

        private void drawCaret(PaintEventArgs e)
        {
            int finalX = kCaretOffsetX + _fontWidth * _selectionStartX;
            int finalY = kCaretOffsetY + (_fontSize + kHexIntervalY) * _selectionStartY;
            if (_selectionStartX > 0)
            {
                finalX += kHexIntervalX * (_selectionStartX / 2);
            }

            e.Graphics.DrawLine(Pens.Black, finalX, finalY, finalX, finalY + _caretSize);
        }

        private void drawText(PaintEventArgs e)
        {
            for (int i = 0; i < _text.Length; ++i)
            {
                int x = i % (HorzHexCount * 2);
                int y = i / (HorzHexCount * 2);

                int finalX = _fontWidth * x + kHexIntervalX * (x / 2);
                int finalY = (_fontSize + kHexIntervalY) * y;
                e.Graphics.DrawString(_text.Substring(i, 1), _font, Brushes.Black, finalX, finalY);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            drawText(e);

            long tickNow = (DateTime.Now.Ticks / 10000);
            if (tickNow > _caretPrevTick + _caretBlinkTime * 2)
            {
                _caretPrevTick = tickNow;
            }
            else if (tickNow > _caretPrevTick + _caretBlinkTime)
            {
                if (ContainsFocus == true)
                {
                    drawCaret(e);
                }
            }
            Invalidate();
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

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                showCaret(true);

                e.IsInputKey = true;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

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
                if (newSelectionStart > 0)
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

                ++SelectionStart;
            }

            Focus();
        }
    }
}
