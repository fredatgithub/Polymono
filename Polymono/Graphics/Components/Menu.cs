using Polymono.Graphics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Graphics.Components
{
    /// <summary>
    /// Menu class which affects the bahaviour of all elements stored within. Acts as a basic list.
    /// </summary>
    class Menu
    {
        public static List<Menu> Menus = new List<Menu>();
        public List<Control> Controls;

        public Menu()
        {
            Controls = new List<Control>();
            Menus.Add(this);
        }

        public static void ShowAll()
        {
            foreach (var menu in Menus)
            {
                menu.Show();
            }
        }

        public static void HideAll()
        {
            foreach (var menu in Menus)
            {
                menu.Hide();
            }
        }

        public void Add(Control control)
        {
            Controls.Add(control);
        }

        public void Remove(Control control)
        {
            Controls.Remove(control);
        }

        public void Show()
        {
            foreach (var control in Controls)
            {
                control.Show();
            }
        }

        public void Hide()
        {
            foreach (var control in Controls)
            {
                control.Hide();
            }
        }

        public void CreateBuffers()
        {
            foreach (var control in Controls)
            {
                control.CreateBuffer();
            }
        }

        public void Update()
        {
            foreach (var control in Controls)
            {
                control.Update();
            }
        }

        public void UpdateModelMatrix()
        {
            foreach (var control in Controls)
            {
                control.UpdateModelMatrix();
            }
        }

        public void Render()
        {
            foreach (var control in Controls)
            {
                control.Render();
            }
        }
    }
}
