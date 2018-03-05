using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Polymono.Graphics
{
    public enum TextBoxState
    {
        Focused,
        Unfocused
    }

    class TextBox : Label
    {
        public TextBoxState State;

        public TextBox(ShaderProgram program, string text, Color4 colour,
            int x, int y, int width, int height, int windowWidth, int windowHeight,
            Matrix4 projection)
            : base(program, text, colour, x, y, width, height, windowWidth, windowHeight, projection)
        {
            State = TextBoxState.Unfocused;
        }

        public void Click(Vector2 mousePosition)
        {
            switch (LayoutAlign)
            {
                case LayoutAlign.TopRight:
                    mousePosition.Y = WindowHeight - mousePosition.Y;
                    break;
                case LayoutAlign.TopLeft:
                    mousePosition.X = WindowWidth - mousePosition.X;
                    mousePosition.Y = WindowHeight - mousePosition.Y;
                    break;
                case LayoutAlign.BottomRight:
                    break;
                case LayoutAlign.BottomLeft:
                    mousePosition.X = WindowWidth - mousePosition.X;
                    break;
                default:
                    break;
            }
            bool isHovering = PointInRectangle(Vertices[0].Position.Xy, Vertices[1].Position.Xy,
                Vertices[2].Position.Xy, Vertices[3].Position.Xy, mousePosition);
            if (isHovering && State == TextBoxState.Unfocused)
            {
                State = TextBoxState.Focused;
            }
            else
            {
                State = TextBoxState.Unfocused;
            }
        }

        public void InputText(KeyboardState state, KeyboardState lastState)
        {
            if (State == TextBoxState.Focused)
            {
                #region Toggle states
                bool isCapsing = (((ushort)GetKeyState(0x14)) & 0xffff) != 0;
                bool isShifting = state.IsKeyDown(Key.ShiftRight) || state.IsKeyDown(Key.ShiftLeft);
                #endregion
                #region Letter presses.
                for (Key key = Key.F1; key <= Key.NonUSBackSlash; key++)
                {
                    if (key == Key.Back)
                    {
                        continue;
                    }
                    if (IsUniquePress(state, lastState, key))
                    {
                        //if (isShifting)
                        //{
                        //    Polymono.Print(key.ToString().ToUpperInvariant());
                        //    Text += key.ToString().ToUpperInvariant();
                        //}
                        //else
                        //{
                        //    Polymono.Print(key.ToString().ToLowerInvariant());
                        //    Text += key.ToString().ToLowerInvariant();
                        //}
                        switch (key)
                        {
                            case Key.F1:
                                break;
                            case Key.F2:
                                break;
                            case Key.F3:
                                break;
                            case Key.F4:
                                break;
                            case Key.F5:
                                break;
                            case Key.F6:
                                break;
                            case Key.F7:
                                break;
                            case Key.F8:
                                break;
                            case Key.F9:
                                break;
                            case Key.F10:
                                break;
                            case Key.F11:
                                break;
                            case Key.F12:
                                break;
                            case Key.F13:
                                break;
                            case Key.F14:
                                break;
                            case Key.F15:
                                break;
                            case Key.F16:
                                break;
                            case Key.F17:
                                break;
                            case Key.F18:
                                break;
                            case Key.F19:
                                break;
                            case Key.F20:
                                break;
                            case Key.F21:
                                break;
                            case Key.F22:
                                break;
                            case Key.F23:
                                break;
                            case Key.F24:
                                break;
                            case Key.F25:
                                break;
                            case Key.F26:
                                break;
                            case Key.F27:
                                break;
                            case Key.F28:
                                break;
                            case Key.F29:
                                break;
                            case Key.F30:
                                break;
                            case Key.F31:
                                break;
                            case Key.F32:
                                break;
                            case Key.F33:
                                break;
                            case Key.F34:
                                break;
                            case Key.F35:
                                break;
                            case Key.Up:
                                break;
                            case Key.Down:
                                break;
                            case Key.Left:
                                break;
                            case Key.Right:
                                break;
                            case Key.Enter:
                                break;
                            case Key.Space:
                                Text += " ";
                                break;
                            case Key.Tab:
                                Text += "    ";
                                break;
                            case Key.BackSpace:
                                if (Text.Length > 0)
                                {
                                    Text = Text.Remove(Text.Length - 1);
                                }
                                break;
                            case Key.Delete:
                                Text = "";
                                break;
                            case Key.A:
                                Text += GetKeyText("a", "A", isCapsing ^ isShifting);
                                break;
                            case Key.B:
                                Text += GetKeyText("b", "B", isCapsing ^ isShifting);
                                break;
                            case Key.C:
                                Text += GetKeyText("c", "C", isCapsing ^ isShifting);
                                break;
                            case Key.D:
                                Text += GetKeyText("d", "D", isCapsing ^ isShifting);
                                break;
                            case Key.E:
                                Text += GetKeyText("e", "E", isCapsing ^ isShifting);
                                break;
                            case Key.F:
                                Text += GetKeyText("f", "F", isCapsing ^ isShifting);
                                break;
                            case Key.G:
                                Text += GetKeyText("g", "G", isCapsing ^ isShifting);
                                break;
                            case Key.H:
                                Text += GetKeyText("h", "H", isCapsing ^ isShifting);
                                break;
                            case Key.I:
                                Text += GetKeyText("i", "I", isCapsing ^ isShifting);
                                break;
                            case Key.J:
                                Text += GetKeyText("j", "J", isCapsing ^ isShifting);
                                break;
                            case Key.K:
                                Text += GetKeyText("k", "K", isCapsing ^ isShifting);
                                break;
                            case Key.L:
                                Text += GetKeyText("l", "L", isCapsing ^ isShifting);
                                break;
                            case Key.M:
                                Text += GetKeyText("m", "M", isCapsing ^ isShifting);
                                break;
                            case Key.N:
                                Text += GetKeyText("n", "N", isCapsing ^ isShifting);
                                break;
                            case Key.O:
                                Text += GetKeyText("o", "O", isCapsing ^ isShifting);
                                break;
                            case Key.P:
                                Text += GetKeyText("p", "P", isCapsing ^ isShifting);
                                break;
                            case Key.Q:
                                Text += GetKeyText("q", "Q", isCapsing ^ isShifting);
                                break;
                            case Key.R:
                                Text += GetKeyText("r", "R", isCapsing ^ isShifting);
                                break;
                            case Key.S:
                                Text += GetKeyText("s", "S", isCapsing ^ isShifting);
                                break;
                            case Key.T:
                                Text += GetKeyText("t", "T", isCapsing ^ isShifting);
                                break;
                            case Key.U:
                                Text += GetKeyText("u", "U", isCapsing ^ isShifting);
                                break;
                            case Key.V:
                                Text += GetKeyText("v", "V", isCapsing ^ isShifting);
                                break;
                            case Key.W:
                                Text += GetKeyText("w", "W", isCapsing ^ isShifting);
                                break;
                            case Key.X:
                                Text += GetKeyText("x", "X", isCapsing ^ isShifting);
                                break;
                            case Key.Y:
                                Text += GetKeyText("y", "Y", isCapsing ^ isShifting);
                                break;
                            case Key.Z:
                                Text += GetKeyText("z", "Z", isCapsing ^ isShifting);
                                break;
                            case Key.Number0:
                                Text += GetKeyText("0", ")", isShifting);
                                break;
                            case Key.Number1:
                                Text += GetKeyText("1", "!", isShifting);
                                break;
                            case Key.Number2:
                                Text += GetKeyText("2", "\"", isShifting);
                                break;
                            case Key.Number3:
                                Text += GetKeyText("3", "£", isShifting);
                                break;
                            case Key.Number4:
                                Text += GetKeyText("4", "$", isShifting);
                                break;
                            case Key.Number5:
                                Text += GetKeyText("5", "%", isShifting);
                                break;
                            case Key.Number6:
                                Text += GetKeyText("6", "^", isShifting);
                                break;
                            case Key.Number7:
                                Text += GetKeyText("7", "&", isShifting);
                                break;
                            case Key.Number8:
                                Text += GetKeyText("8", "*", isShifting);
                                break;
                            case Key.Number9:
                                Text += GetKeyText("9", "(", isShifting);
                                break;
                            case Key.Tilde:
                                Text += GetKeyText("`", "¬", isShifting);
                                break;
                            case Key.Minus:
                                Text += GetKeyText("-", "_", isShifting);
                                break;
                            case Key.Plus:
                                Text += GetKeyText("=", "+", isShifting);
                                break;
                            case Key.BracketLeft:
                                Text += GetKeyText("[", "}", isShifting);
                                break;
                            case Key.BracketRight:
                                Text += GetKeyText("]", "{", isShifting);
                                break;
                            case Key.Semicolon:
                                Text += GetKeyText(";", ":", isShifting);
                                break;
                            case Key.Quote:
                                Text += GetKeyText("'", "@", isShifting);
                                break;
                            case Key.Comma:
                                Text += GetKeyText(",", "<", isShifting);
                                break;
                            case Key.Period:
                                Text += GetKeyText(".", ">", isShifting);
                                break;
                            case Key.Slash:
                                Text += GetKeyText("/", "?", isShifting);
                                break;
                            case Key.BackSlash:
                                Text += GetKeyText(@"\", "|", isShifting);
                                break;
                            case Key.NonUSBackSlash:
                                Text += GetKeyText(@"\", "|", isShifting);
                                break;
                            default:
                                break;
                        }
                    }
                }
                #endregion
                #region Special functions
                if (IsUniquePress(state, lastState, Key.BackSpace) && Text.Length > 0)
                {
                    Text = Text.Remove(Text.Length - 1);
                }
                #endregion
            }
        }

        private string GetKeyText(string lowerChar, string upperChar, bool isCapital)
        {
            return (isCapital) ? upperChar : lowerChar;
        }

        private bool IsUniquePress(KeyboardState state, KeyboardState lastState, Key key)
        {
            return (state.IsKeyDown(key) && !lastState.IsKeyDown(key));
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        private bool PointInRectangle(Vector2 pos1, Vector2 pos2,
            Vector2 pos3, Vector2 pos4, Vector2 posMouse)
        {
            pos1.X += Position.X;
            pos1.Y += Position.Y;
            pos2.X += Position.X;
            pos2.Y += Position.Y;
            pos3.X += Position.X;
            pos3.Y += Position.Y;
            pos4.X += Position.X;
            pos4.Y += Position.Y;
            bool right12 = IsRight(pos1, pos2, posMouse);
            bool right23 = IsRight(pos2, pos3, posMouse);
            bool right34 = IsRight(pos3, pos4, posMouse);
            bool right41 = IsRight(pos4, pos1, posMouse);
            return right12 && right23 && right34 && right41;
        }

        private bool IsRight(Vector2 pos1, Vector2 pos2, Vector2 posMouse)
        {
            return ((pos2.X - pos1.X) * (posMouse.Y - pos1.Y)
                - (pos2.Y - pos1.Y) * (posMouse.X - pos1.X)) < 0;
        }
    }
}
