using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using Polymono.Graphics.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Graphics.Components
{
    class Textbox : Control, IClickable
    {
        public Dictionary<int, Control> Controls;

        public Textbox(ShaderProgram program,
            Vector3 position, Vector3 rotation, Vector3 scaling,
            Dictionary<int, Control> controls, Dictionary<int, AModel> models, Menu menu,
            int width, int height, int buffer, int windowWidth, int windowHeight,
            Matrix4 projection, Color4 colour, Color4 highlightColour,
            Color4 focusedColour, Color4 focusedHighlightColour, 
            string text = "", string fontLocation = "arial")
            : base(program, position, rotation, scaling, controls, models, menu, width, height, windowWidth, windowHeight, projection, text, fontLocation)
        {
            Controls = controls;
            CreateDefaultModel("Default", width, height, buffer, colour, highlightColour);
            CreateDefaultModel("Focused", width, height, buffer, focusedColour, focusedHighlightColour);
            State = ControlState.Unfocused;
        }

        public async void Click(Vector2 vector)
        {
            vector.Y = WindowHeight - vector.Y;
            Vector3 position = Models[Selector].Position;
            bool isHovering = PointInRectangle(
                new Vector2(position.X, position.Y),
                new Vector2(position.X + Width, position.Y),
                new Vector2(position.X + Width, position.Y - Height),
                new Vector2(position.X, position.Y - Height), vector);
            if (isHovering && !Models[Selector].IsHidden)
            {
                if (State == ControlState.Unfocused || State == ControlState.Normal)
                {
                    Polymono.Debug($"Textbox clicked: {Text}[{ID}]");
                    // Unfocus everything else.
                    foreach (var control in Controls.Values)
                    {
                        if (control is Textbox textbox)
                        {
                            if (textbox.State == ControlState.Focused)
                            {
                                textbox.State = ControlState.Unfocused;
                                textbox.Selector = "Default";
                            }
                        }
                    }
                    // Focus this object.
                    State = ControlState.Focused;
                    Selector = "Focused";
                } else
                {
                    State = ControlState.Unfocused;
                    Selector = "Default";
                }
            }
        }

        private bool PointInRectangle(Vector2 pos1, Vector2 pos2,
            Vector2 pos3, Vector2 pos4, Vector2 posMouse)
        {
            return IsRight(pos1, pos2, posMouse)
                && IsRight(pos2, pos3, posMouse)
                && IsRight(pos3, pos4, posMouse)
                && IsRight(pos4, pos1, posMouse);
        }

        private bool IsRight(Vector2 pos1, Vector2 pos2, Vector2 posMouse)
        {
            return ((pos2.X - pos1.X) * (posMouse.Y - pos1.Y)
                - (pos2.Y - pos1.Y) * (posMouse.X - pos1.X)) < 0;
        }

        public bool InputText(KeyboardState state, KeyboardState lastState)
        {
            if (State == ControlState.Focused)
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
                return true;
            }
            return false;
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

        private void CreateDefaultModel(string selector, int width, int height, int buffer,
            Color4 mainColour, Color4 highlightColour)
        {
            height = -height;
            Square mainSquare = new Square(
                buffer,                     // X position
                -buffer,                    // Y position
                width - (buffer * 2),       // Width
                height + (buffer * 2),      // Height
                mainColour);
            Square topSquare = new Square(
                0,                          // X position
                0,                          // Y position
                width - buffer,             // Width
                -buffer,                    // Height
                highlightColour, mainSquare.Append());
            Square rightSquare = new Square(
                width - buffer,             // X position
                0,                          // Y position
                buffer,                     // Width
                height + buffer,            // Height
                highlightColour, topSquare.Append());
            Square bottomSquare = new Square(
                buffer,                     // X position
                height + buffer,            // Y position
                width - buffer,             // Width
                -buffer,                    // Height
                highlightColour, rightSquare.Append());
            Square leftSquare = new Square(
                0,                          // X position
                -buffer,                    // Y position
                buffer,                     // Width
                height + buffer,            // Height
                highlightColour, bottomSquare.Append());
            // Create vertex list.
            List<Vertex> vertices = new List<Vertex>(
                mainSquare.Vertices.Count +
                topSquare.Vertices.Count +
                rightSquare.Vertices.Count +
                bottomSquare.Vertices.Count +
                leftSquare.Vertices.Count);
            // Adding each object to one list.
            vertices.AddRange(mainSquare.Vertices);
            vertices.AddRange(topSquare.Vertices);
            vertices.AddRange(rightSquare.Vertices);
            vertices.AddRange(bottomSquare.Vertices);
            vertices.AddRange(leftSquare.Vertices);
            // Create indices list.
            List<int> indices = new List<int>(
                mainSquare.Indices.Count +
                topSquare.Indices.Count +
                rightSquare.Indices.Count +
                bottomSquare.Indices.Count +
                leftSquare.Indices.Count);
            // Adding each object to one list.
            indices.AddRange(mainSquare.Indices);
            indices.AddRange(topSquare.Indices);
            indices.AddRange(rightSquare.Indices);
            indices.AddRange(bottomSquare.Indices);
            indices.AddRange(leftSquare.Indices);
            // Assign master list to vertex model.
            Models[selector].Vertices = vertices.ToArray();
            Models[selector].Indices = indices.ToArray();
            ControlBase(ref vertices, ref indices);
        }
    }
}
