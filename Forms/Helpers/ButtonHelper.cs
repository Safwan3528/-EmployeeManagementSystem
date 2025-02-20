using System.Drawing;
using System.Windows.Forms;

namespace EmployeeManagementSystem.Helpers
{
    public static class ButtonHelper
    {
        public static Button CreateStyledButton(string text, Color backColor, DockStyle dock = DockStyle.Top, int tabIndex = 0)
        {
            return new Button
            {
                Text = text,
                Dock = dock,
                Height = 35,
                Margin = new Padding(0, 5, 0, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatAppearance = { BorderSize = 0 },
                TabIndex = tabIndex
            };
        }
    }
} 