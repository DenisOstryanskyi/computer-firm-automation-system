using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CFAS
{
    public static class Icons
    {
        private static Image Draw(Color color, Action<Graphics, Pen, Brush> painter)
        {
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(color, 1.6f))
                using (var brush = new SolidBrush(color))
                {
                    painter(g, pen, brush);
                }
            }
            return bitmap;
        }

        private static void DrawPerson(Graphics g, Pen pen, Brush brush, int x, int y)
        {
            g.DrawEllipse(pen, x + 2, y, 5, 5);              
            g.DrawArc(pen, x - 1, y + 6, 9, 8, 180, 180);   
        }

        public static Image Client()
        {
            return Draw(Color.SteelBlue, (g, pen, brush) => DrawPerson(g, pen, brush, 4, 2));
        }

        public static Image Product()
        {
            return Draw(Color.SeaGreen, (g, pen, brush) =>
            {
                g.DrawRectangle(pen, 2, 4, 12, 10);
                g.DrawLine(pen, 2, 8, 14, 8);
            });
        }

        public static Image Order()
        {
            return Draw(Color.DarkOrange, (g, pen, brush) =>
            {
                g.DrawPolygon(pen, new Point[]
                {
                    new Point(2, 4), new Point(5, 4), new Point(6, 11),
                    new Point(12, 11), new Point(13, 5), new Point(14, 5)
                });
                g.FillEllipse(brush, 5, 12, 2, 2);
                g.FillEllipse(brush, 10, 12, 2, 2);
            });
        }

        public static Image Report()
        {
            return Draw(Color.MediumSlateBlue, (g, pen, brush) =>
            {
                g.FillRectangle(brush, 2, 9, 3, 5);
                g.FillRectangle(brush, 7, 5, 3, 9);
                g.FillRectangle(brush, 12, 2, 3, 12);
            });
        }

        public static Image Journal()
        {
            return Draw(Color.Sienna, (g, pen, brush) =>
            {
                g.DrawRectangle(pen, 2, 1, 12, 14);
                g.DrawLine(pen, 5, 5, 11, 5);
                g.DrawLine(pen, 5, 8, 11, 8);
                g.DrawLine(pen, 5, 11, 9, 11);
            });
        }

        public static Image Users()
        {
            return Draw(Color.Teal, (g, pen, brush) =>
            {
                DrawPerson(g, pen, brush, 1, 3);
                DrawPerson(g, pen, brush, 7, 2);
            });
        }

        public static Image Settings()
        {
            return Draw(Color.Gray, (g, pen, brush) =>
            {
                g.DrawEllipse(pen, 4, 4, 8, 8);
                g.DrawLine(pen, 8, 1, 8, 4);
                g.DrawLine(pen, 8, 12, 8, 15);
                g.DrawLine(pen, 1, 8, 4, 8);
                g.DrawLine(pen, 12, 8, 15, 8);
                g.FillEllipse(brush, 7, 7, 2, 2);
            });
        }

        public static Image Help()
        {
            return Draw(Color.RoyalBlue, (g, pen, brush) =>
            {
                using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
                {
                    g.DrawString("?", font, brush, 3, 0);
                }
            });
        }

        public static Image About()
        {
            return Draw(Color.RoyalBlue, (g, pen, brush) =>
            {
                using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
                {
                    g.DrawString("i", font, brush, 5, 1);
                }
            });
        }

        public static Image Logo()
        {
            var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.SteelBlue, 2f))
                using (var brush = new SolidBrush(Color.LightSteelBlue))
                {
                    g.DrawRectangle(pen, 4, 4, 24, 17);    
                    g.FillRectangle(brush, 7, 7, 18, 11); 
                    g.DrawLine(pen, 16, 21, 16, 26);       
                    g.DrawLine(pen, 10, 26, 22, 26);      
                }
            }
            return bitmap;
        }

        public static Icon WindowIcon()
        {
            return Icon.FromHandle(((Bitmap)Logo()).GetHicon());
        }
    }
}