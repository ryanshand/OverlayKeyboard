using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OverlayKeyboard
{
    public partial class MainForm : Form
    {
        private KeyboardConfiguration config;
        private bool isShiftPressed = false;
        private bool isKeyboardVisible = true;
        private bool isDragging = false;
        private Point dragStartPoint;
        private Size originalSize;
        private Point originalLocation;
        private Size lastKeyboardSize;

        // For making the form always on top and click-through when hidden
        private const int WS_EX_TOPMOST = 0x00000008;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_NOACTIVATE = 0x08000000;



        public MainForm()
        {
            InitializeComponent();
            config = KeyboardConfiguration.Load();
            lastKeyboardSize = new Size(config.DefaultWidth, config.DefaultHeight);
            isShiftPressed = false; // Start in lowercase mode
            SetupForm();
            CreateKeyboard();
        }

        private void SetupForm()
        {
            this.Text = "Overlay Keyboard";
            this.Size = new Size(config.DefaultWidth, config.DefaultHeight);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(100, Screen.PrimaryScreen.WorkingArea.Height - config.DefaultHeight - 50);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(240, 240, 240); // Slightly transparent gray
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.Opacity = 0.9; // Make it slightly transparent
            
            // Prevent the form from stealing focus
            this.TopLevel = true;
            this.SetStyle(ControlStyles.Selectable, false);
        }

        private void CreateKeyboard()
        {
            this.Controls.Clear();


            // create the move/drag icon

            var dragButton = new Button
            {
                Text = "✥",
                Font = new Font("Segoe UI Symbol", 10, FontStyle.Regular),
                Size = new Size(30, 30),
                Location = new Point(10, 10),
                Cursor = Cursors.SizeAll // Optional
            };
            dragButton.MouseDown += MoveButton_MouseDown;
            dragButton.MouseMove += MoveButton_MouseMove;
            dragButton.MouseUp += MoveButton_MouseUp; 
            this.Controls.Add(dragButton);





            // Create show/hide button
            var showHideButton = new Button
            {
                Text = isKeyboardVisible ? "HIDE" : "SHOW",
                Size = new Size(80, 30),
                Location = new Point(50, 10),
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Symbol", 10, FontStyle.Bold)
            };
            showHideButton.Click += ShowHideButton_Click;
            this.Controls.Add(showHideButton);


            // Create f5 refresh button (only when keyboard is visible and ShowRefresh is true) 
            if (config.ShowRefresh)
            {
                var refreshButton = new Button
                {
                    Text = "REFRESH PAGE",
                    Size = new Size(90, 30),
                    Location = new Point(140, 10),
                    BackColor = Color.DarkBlue,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI Symbol", 10, FontStyle.Bold)
                };
                refreshButton.Click += SendF5KeyPress;
                this.Controls.Add(refreshButton);           
            }


            //set keyboard size when hidden
            if (!isKeyboardVisible)
            {
                this.Size = new Size(240, 50);
                return;
            }



            // Create RESET SIZE button (only when keyboard is visible)
            // Create X button (only when keyboard is visible and ShowExit is true)
            if (config.ShowReset)
            {
                var resetSizeButton = new Button
                {
                    Text = "RESET SIZE",
                    Size = new Size(90, 30),
                    Location = new Point(this.Width - 130, 10),
                    BackColor = Color.Orange,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI Symbol", 10, FontStyle.Bold)
                };
                resetSizeButton.Click += ResetSizeButton_Click;
                this.Controls.Add(resetSizeButton);
            }


            // Create X button (only when keyboard is visible and ShowExit is true)
            if (config.ShowExit)
            {
                var exitButton = new Button
                {
                    Text = "X",
                    Size = new Size(30, 30),
                    Location = new Point(this.Width - 40, 10),
                    BackColor = Color.Red,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI Symbol", 10, FontStyle.Bold)
                };
                exitButton.Click += ExitButton_Click;
                this.Controls.Add(exitButton);
            }


            // Create resize handle
            // Create resize handle (only when keyboard is visible and ShowResize is true)
            if (config.ShowResize)
            {
                  var resizeHandle = new Panel
                  {
                      Size = new Size(20, 20),
                      Location = new Point(this.Width - 25, this.Height - 25),
                      BackColor = Color.Gray,
                      Cursor = Cursors.SizeNWSE
                  };
                  resizeHandle.MouseDown += ResizeHandle_MouseDown;
                  resizeHandle.MouseMove += ResizeHandle_MouseMove;
                  resizeHandle.MouseUp += ResizeHandle_MouseUp;
                  this.Controls.Add(resizeHandle);
            }





                CreateNumberRow();
                CreateLetterRows();
                CreateSpacebarRow();
            }
       

        private void CreateNumberRow()
        {
            int startY = 50;
            int availableWidth = this.Width - 40; // Account for margins
            int keyWidth = availableWidth / 10;
            int keyHeight = Math.Min(config.NumberRowHeight, (this.Height - 100) / 5); // Ensure it fits

            string[] numbers = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };

            for (int i = 0; i < numbers.Length; i++)
            {
                var button = CreateKeyButton(numbers[i], new Point(20 + i * keyWidth, startY), new Size(keyWidth - config.KeySpacing, keyHeight));
                this.Controls.Add(button);
            }
        }

        private void CreateLetterRows()
        {
            int startY = 50 + Math.Min(config.NumberRowHeight, (this.Height - 100) / 5) + 5;
            int availableWidth = this.Width - 40; // Account for margins
            //int keyWidth = availableWidth / 10;
            int keyHeight = Math.Min(config.LetterRowHeight, (this.Height - 100) / 5); // Ensure it fits

            string[] row1 = { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p","!" };
            string[] row2 = { "a", "s", "d", "f", "g", "h", "j", "k", "l", ";", "#", "@" };
            string[] row3 = { "z", "x", "c", "v", "b", "n", "m", ",", ".", "/", "$", "%" };

            int keyWidthRow1 = availableWidth / row1.Length;
            int keyWidthRow2 = availableWidth / row2.Length;
            int keyWidthRow3 = availableWidth / row3.Length;

            // First row
            for (int i = 0; i < row1.Length; i++)
            {
                var button = CreateKeyButton(row1[i], new Point(20 + i * keyWidthRow1, startY), new Size(keyWidthRow1 - config.KeySpacing, keyHeight));
                this.Controls.Add(button);
            }

            // Second row
            startY += keyHeight + config.KeySpacing;
            for (int i = 0; i < row2.Length; i++)
            {
                var button = CreateKeyButton(row2[i], new Point(20 + i * keyWidthRow2, startY), new Size(keyWidthRow2 - config.KeySpacing, keyHeight));
                this.Controls.Add(button);
            }

            // Third row
            startY += keyHeight + config.KeySpacing;
            for (int i = 0; i < row3.Length; i++)
            {
                var button = CreateKeyButton(row3[i], new Point(20 + i * keyWidthRow3, startY), new Size(keyWidthRow3 - config.KeySpacing, keyHeight));
                this.Controls.Add(button);
            }

            // Special keys row
            startY += keyHeight + config.KeySpacing;
            CreateSpecialKeysRow(startY, keyWidthRow3, keyHeight);
        }

        private void CreateSpecialKeysRow(int startY, int keyWidth, int keyHeight)
        {
            // Shift key
            var shiftButton = CreateKeyButton("SHIFT", new Point(20, startY), new Size(keyWidth * 2 - config.KeySpacing, keyHeight));
            shiftButton.Click += ShiftButton_Click;
            shiftButton.BackColor = isShiftPressed ? Color.LightBlue : Color.White;
            this.Controls.Add(shiftButton);

            // Space bar
            var spaceButton = CreateKeyButton("SPACE", new Point(20 + keyWidth * 2, startY), new Size(keyWidth * 3 - config.KeySpacing, keyHeight));
            spaceButton.Click += (s, e) => SendKey(VirtualKeyCode.SPACE);
            this.Controls.Add(spaceButton);

            // Quote key
            var quoteButton = CreateKeyButton("\"", new Point(20 + keyWidth * 5, startY), new Size(keyWidth - config.KeySpacing, keyHeight));
            quoteButton.Click += (s, e) => SendChar('"');
            this.Controls.Add(quoteButton);

            // Enter key
            var enterButton = CreateKeyButton("ENTER", new Point(20 + keyWidth * 6, startY), new Size(keyWidth * 2 - config.KeySpacing, keyHeight));
            enterButton.Click += (s, e) => SendKey(VirtualKeyCode.RETURN);
            this.Controls.Add(enterButton);

            // Backspace key (to the right of ENTER)
            var backspaceButton = CreateKeyButton("←", new Point(20 + keyWidth * 8, startY), new Size(keyWidth - config.KeySpacing, keyHeight));
            backspaceButton.Click += (s, e) => SendKey(VirtualKeyCode.BACK);
            this.Controls.Add(backspaceButton);
        }

        private void CreateSpacebarRow()
        {
            // This is handled in CreateSpecialKeysRow for better layout
        }

        private Button CreateKeyButton(string text, Point location, Size size)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", config.KeyFontSize, FontStyle.Bold),
                UseVisualStyleBackColor = false,
                TabStop = false // Prevent tab focus
            };

            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.Gray;
            button.FlatAppearance.MouseDownBackColor = Color.LightGray;
            button.FlatAppearance.MouseOverBackColor = Color.LightYellow;

            // Handle letter keys
            if (text.Length == 1 && char.IsLetter(text[0]))
            {
                button.Click += (s, e) => SendLetter(text);
            }
            else if (char.IsDigit(text[0]))
            {
                button.Click += (s, e) => SendKey(GetVirtualKeyCode(text));
            }
            else if (text == "," || text == "." || text == ";" || text == "/" || text == "$" || text == "@" || text == "#" || text == "!" || text == "%")
            {
                button.Click += (s, e) => SendPunctuation(text);
            }

            return button;
        }


        private void SendLetter(string letter)
        {
            char charToSend = isShiftPressed ? letter.ToUpper()[0] : letter.ToLower()[0];
            SendChar(charToSend);
        }



        private void SendPunctuation(string punctuation)
        {
            char charToSend = punctuation[0];
            // Handle special shift behavior for semicolon
            if (punctuation == ";" && isShiftPressed)
            {
                charToSend = ':';
            }

            // Use Unicode input for symbols that don't have a VK code
            if (punctuation == "@" || punctuation == "$" || punctuation == "%" || punctuation == "!" || punctuation == "#")
            {
                SendUnicodeChar(charToSend);
            }
            else
            {
                SendChar(charToSend);
            }
        }


        private void SendKey(VirtualKeyCode keyCode)
        {
            // Use keybd_event for more reliable key sending
            keybd_event((byte)keyCode, 0, 0, 0);
            keybd_event((byte)keyCode, 0, KEYEVENTF_KEYUP, 0);
        }


        // Define necessary structures and enums for SendInput
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public int type;
            public INPUT_UNION U;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT_UNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        public const int INPUT_KEYBOARD = 1;
        public const uint KEYEVENTF_UNICODE = 0x0004;
        public const uint KEYEVENTF_KEYUPS = 0x0002;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);


        private void SendUnicodeChar(char ch)
        {
            INPUT[] inputs = new INPUT[2];

            // Key down
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].U.ki.wVk = 0; // <-- Must be 0 for Unicode input
            inputs[0].U.ki.wScan = ch;
            inputs[0].U.ki.dwFlags = KEYEVENTF_UNICODE;
            inputs[0].U.ki.time = 0;
            inputs[0].U.ki.dwExtraInfo = IntPtr.Zero;

            // Key up
            inputs[1].type = INPUT_KEYBOARD;
            inputs[1].U.ki.wVk = 0; // <-- Must be 0 for Unicode input
            inputs[1].U.ki.wScan = ch;
            inputs[1].U.ki.dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUPS;
            inputs[1].U.ki.time = 0;
            inputs[1].U.ki.dwExtraInfo = IntPtr.Zero;

            SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        }


        private void SendChar(char character)
        {
            // Use keybd_event for character sending
            VirtualKeyCode keyCode = GetCharVirtualKeyCode(character);
            if (keyCode != 0)
            {
                // Check if shift is needed for uppercase or special characters
                bool needsShift = char.IsUpper(character) || character == '"' || character == ':';
                
                if (needsShift)
                {
                    keybd_event((byte)VirtualKeyCode.VK_SHIFT, 0, 0, 0);
                }
                
                keybd_event((byte)keyCode, 0, 0, 0);
                keybd_event((byte)keyCode, 0, KEYEVENTF_KEYUP, 0);
                
                if (needsShift)
                {
                    keybd_event((byte)VirtualKeyCode.VK_SHIFT, 0, KEYEVENTF_KEYUP, 0);
                }
            }
        }

        private VirtualKeyCode GetCharVirtualKeyCode(char character)
        {
            return char.ToUpper(character) switch
            {
                'A' => VirtualKeyCode.VK_A,
                'B' => VirtualKeyCode.VK_B,
                'C' => VirtualKeyCode.VK_C,
                'D' => VirtualKeyCode.VK_D,
                'E' => VirtualKeyCode.VK_E,
                'F' => VirtualKeyCode.VK_F,
                'G' => VirtualKeyCode.VK_G,
                'H' => VirtualKeyCode.VK_H,
                'I' => VirtualKeyCode.VK_I,
                'J' => VirtualKeyCode.VK_J,
                'K' => VirtualKeyCode.VK_K,
                'L' => VirtualKeyCode.VK_L,
                'M' => VirtualKeyCode.VK_M,
                'N' => VirtualKeyCode.VK_N,
                'O' => VirtualKeyCode.VK_O,
                'P' => VirtualKeyCode.VK_P,
                'Q' => VirtualKeyCode.VK_Q,
                'R' => VirtualKeyCode.VK_R,
                'S' => VirtualKeyCode.VK_S,
                'T' => VirtualKeyCode.VK_T,
                'U' => VirtualKeyCode.VK_U,
                'V' => VirtualKeyCode.VK_V,
                'W' => VirtualKeyCode.VK_W,
                'X' => VirtualKeyCode.VK_X,
                'Y' => VirtualKeyCode.VK_Y,
                'Z' => VirtualKeyCode.VK_Z,
                '0' => VirtualKeyCode.VK_0,
                '1' => VirtualKeyCode.VK_1,
                '2' => VirtualKeyCode.VK_2,
                '3' => VirtualKeyCode.VK_3,
                '4' => VirtualKeyCode.VK_4,
                '5' => VirtualKeyCode.VK_5,
                '6' => VirtualKeyCode.VK_6,
                '7' => VirtualKeyCode.VK_7,
                '8' => VirtualKeyCode.VK_8,
                '9' => VirtualKeyCode.VK_9,               
                ' ' => VirtualKeyCode.SPACE,
                ',' => VirtualKeyCode.OEM_COMMA,
                '.' => VirtualKeyCode.OEM_PERIOD,
                ';' => VirtualKeyCode.OEM_1,
                ':' => VirtualKeyCode.OEM_1, // Same key as semicolon
                '/' => VirtualKeyCode.OEM_2,
                '"' => VirtualKeyCode.OEM_7,
                _ => 0
            };
        }

        private VirtualKeyCode GetVirtualKeyCode(string key)
        {
            return key.ToUpper() switch
            {
                "1" => VirtualKeyCode.VK_1,
                "2" => VirtualKeyCode.VK_2,
                "3" => VirtualKeyCode.VK_3,
                "4" => VirtualKeyCode.VK_4,
                "5" => VirtualKeyCode.VK_5,
                "6" => VirtualKeyCode.VK_6,
                "7" => VirtualKeyCode.VK_7,
                "8" => VirtualKeyCode.VK_8,
                "9" => VirtualKeyCode.VK_9,
                "0" => VirtualKeyCode.VK_0,
                "Q" => VirtualKeyCode.VK_Q,
                "W" => VirtualKeyCode.VK_W,
                "E" => VirtualKeyCode.VK_E,
                "R" => VirtualKeyCode.VK_R,
                "T" => VirtualKeyCode.VK_T,
                "Y" => VirtualKeyCode.VK_Y,
                "U" => VirtualKeyCode.VK_U,
                "I" => VirtualKeyCode.VK_I,
                "O" => VirtualKeyCode.VK_O,
                "P" => VirtualKeyCode.VK_P,
                "A" => VirtualKeyCode.VK_A,
                "S" => VirtualKeyCode.VK_S,
                "D" => VirtualKeyCode.VK_D,
                "F" => VirtualKeyCode.VK_F,
                "G" => VirtualKeyCode.VK_G,
                "H" => VirtualKeyCode.VK_H,
                "J" => VirtualKeyCode.VK_J,
                "K" => VirtualKeyCode.VK_K,
                "L" => VirtualKeyCode.VK_L,
                "Z" => VirtualKeyCode.VK_Z,
                "X" => VirtualKeyCode.VK_X,
                "C" => VirtualKeyCode.VK_C,
                "V" => VirtualKeyCode.VK_V,
                "B" => VirtualKeyCode.VK_B,
                "N" => VirtualKeyCode.VK_N,
                "M" => VirtualKeyCode.VK_M,
                ";" => VirtualKeyCode.OEM_1,
                "," => VirtualKeyCode.OEM_COMMA,
                "." => VirtualKeyCode.OEM_PERIOD,
                "/" => VirtualKeyCode.OEM_2,
                "\"" => VirtualKeyCode.OEM_7,
                _ => VirtualKeyCode.SPACE
            };
        }

        private void ShowHideButton_Click(object? sender, EventArgs e)
        {
            isKeyboardVisible = !isKeyboardVisible;
            
            if (isKeyboardVisible)
            {
                // Restore to last known keyboard size
                this.Size = lastKeyboardSize;
            }
            else
            {
                // Save current size before hiding
                lastKeyboardSize = this.Size;
            }
            
            CreateKeyboard();
        }

        private void MoveButton_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = e.Location;
                originalSize = this.Size;
                originalLocation = this.Location;
            }
        }

        private void MoveButton_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point newLocation = new Point(
                    this.Location.X + e.X - dragStartPoint.X,
                    this.Location.Y + e.Y - dragStartPoint.Y
                );
                this.Location = newLocation;
            }
        }

        private void MoveButton_MouseUp(object? sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void ShiftButton_Click(object? sender, EventArgs e)
        {
            isShiftPressed = !isShiftPressed;
            UpdateLetterButtons();
        }

        private void UpdateLetterButtons()
        {
            foreach (Control control in this.Controls)
            {
                if (control is Button button)
                {
                    if (button.Text.Length == 1 && char.IsLetter(button.Text[0]))
                    {
                        button.Text = isShiftPressed ? button.Text.ToUpper() : button.Text.ToLower();
                    }
                    else if (button.Text == "SHIFT")
                    {
                        button.BackColor = isShiftPressed ? Color.LightBlue : Color.White;
                    }
                    else if (button.Text == ";" || button.Text == ":")
                    {
                        button.Text = isShiftPressed ? ":" : ";";
                    }
                }
            }
        }

        private void ResizeHandle_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = e.Location;
                originalSize = this.Size;
            }
        }

        private void ResizeHandle_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // Calculate new size based on mouse movement from the initial drag point
                int deltaX = e.X - dragStartPoint.X;
                int newWidth = originalSize.Width + deltaX;
                
                // Calculate height based on width to maintain proper aspect ratio
                // Base height calculation: number row + 3 letter rows + special row + padding
                int calculatedHeight = config.NumberRowHeight + (3 * config.LetterRowHeight) + config.LetterRowHeight + 80; // 80 for padding and margins
                
                // Minimum size constraints
                newWidth = Math.Max(400, newWidth);
                calculatedHeight = Math.Max(200, calculatedHeight);

                this.Size = new Size(newWidth, calculatedHeight);
                CreateKeyboard(); // Recreate keyboard with new size
            }
        }

        private void ResizeHandle_MouseUp(object? sender, MouseEventArgs e)
        {
            isDragging = false;
            lastKeyboardSize = this.Size;
            config.DefaultWidth = this.Width;
            config.DefaultHeight = this.Height;
            config.Save();
        }


        private void SendF5KeyPress(object sender, EventArgs e)
        {
            const byte VK_F5 = 0x74;
            keybd_event(VK_F5, 0, 0, 0); // Key down
            keybd_event(VK_F5, 0, KEYEVENTF_KEYUP, 0); // Key up
        }

        private void ResetSizeButton_Click(object? sender, EventArgs e)
        {
            // Reset to the configured reset values
            System.Diagnostics.Debug.WriteLine($"Reset: ResetWidth={config.ResetWidth}, ResetHeight={config.ResetHeight}");
            config.DefaultWidth = config.ResetWidth;
            config.DefaultHeight = config.ResetHeight;
            lastKeyboardSize = new Size(config.DefaultWidth, config.DefaultHeight);
            this.Size = lastKeyboardSize;
            CreateKeyboard();
            config.Save();
            System.Diagnostics.Debug.WriteLine($"After Reset: Size={this.Size}");
        }

        private void ExitButton_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= (int)(WS_EX_TOPMOST | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
                return cp;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            config.Save();
            base.OnFormClosing(e);
        }

        // Windows API declarations
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_CHAR = 0x0102;

        // Virtual key codes
        private enum VirtualKeyCode : byte
        {
            VK_0 = 0x30, VK_1 = 0x31, VK_2 = 0x32, VK_3 = 0x33, VK_4 = 0x34,
            VK_5 = 0x35, VK_6 = 0x36, VK_7 = 0x37, VK_8 = 0x38, VK_9 = 0x39,
            VK_A = 0x41, VK_B = 0x42, VK_C = 0x43, VK_D = 0x44, VK_E = 0x45,
            VK_F = 0x46, VK_G = 0x47, VK_H = 0x48, VK_I = 0x49, VK_J = 0x4A,
            VK_K = 0x4B, VK_L = 0x4C, VK_M = 0x4D, VK_N = 0x4E, VK_O = 0x4F,
            VK_P = 0x50, VK_Q = 0x51, VK_R = 0x52, VK_S = 0x53, VK_T = 0x54,
            VK_U = 0x55, VK_V = 0x56, VK_W = 0x57, VK_X = 0x58, VK_Y = 0x59, VK_Z = 0x5A,
            SPACE = 0x20, RETURN = 0x0D, BACK = 0x08, VK_SHIFT = 0x10,
            OEM_1 = 0xBA, OEM_2 = 0xBF, OEM_7 = 0xDE, OEM_COMMA = 0xBC, OEM_PERIOD = 0xBE, OEM_4 = 0xDE,
            OEM_3 = 0xC0, OEM_5 = 0xBF, VK_F5 = 0x74
        }
    }
}
