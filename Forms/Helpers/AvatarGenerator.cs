using System.Drawing;
using System.Drawing.Drawing2D;

namespace EmployeeManagementSystem.Helpers
{
    public static class AvatarGenerator
    {
        public static Image CreateDefaultAvatar(int size = 150)
        {
            var bitmap = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Background circle
                using (var brush = new SolidBrush(Color.FromArgb(74, 74, 74)))
                {
                    g.FillEllipse(brush, 0, 0, size, size);
                }

                // Head
                using (var brush = new SolidBrush(Color.FromArgb(102, 102, 102)))
                {
                    g.FillEllipse(brush, size * 0.33f, size * 0.2f, size * 0.33f, size * 0.33f);
                }

                // Body
                using (var brush = new SolidBrush(Color.FromArgb(102, 102, 102)))
                {
                    var bodyPath = new GraphicsPath();
                    var startY = size * 0.63f;
                    bodyPath.AddEllipse(size * 0.2f, startY, size * 0.6f, size * 0.5f);
                    g.FillPath(brush, bodyPath);
                }
            }
            return bitmap;
        }
    }
} 