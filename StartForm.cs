using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fractal
{
    public partial class StartForm : Form
    {
        private TextBox textBoxZoom;
        private List<Color> allColors = new List<Color>();
        private Color[] settedColors = new Color[MAX_COLORS];
        private Random random = new Random();
        private TableLayoutPanel colors;
        private Button saveButton;
        private static PictureBox pictureBox;
        private PointD centre;
        private double dx = 0, dy = 0, zoom = 1;
        private const int MAX_COLORS = 39;
        public StartForm()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;

            Type colorType = typeof(Color);
            PropertyInfo[] propInfos = colorType
                .GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public);
            foreach (PropertyInfo propInfo in propInfos)
            {
                Console.WriteLine(propInfo.Name);
                allColors.Add(Color.FromName(propInfo.Name));
            }

            allColors.Remove(Color.Black);
            var buildButton = new Button()
            {
                Text = "▶",
                Font = new Font(SystemFonts.DefaultFont.FontFamily, 14, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.Green,
                BackColor = Color.WhiteSmoke,
            };
            ToolTip ToolTip = new ToolTip();
            ToolTip.SetToolTip(buildButton, "Построить фрактал");
            textBoxZoom = new TextBox()
            {
                PlaceholderText = "Увеличение",
                MinimumSize = new Size(300, 30),
                Font = new Font(SystemFonts.DefaultFont.FontFamily, 14, FontStyle.Bold)
            };
            var layot = new FlowLayoutPanel()
            {
                Location = new Point(0, 0),
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
            };
            var selectColors = new Label()
            {
                Text = "Палитра",
                AutoSize = true,
                Font = new Font(SystemFonts.DefaultFont.FontFamily, 14, FontStyle.Bold),
                Location = new Point(0, layot.Height + 150)
            };
            var reset = new Button()
            {
                Location = new Point(selectColors.Location.X + 150, selectColors.Location.Y - 10),
                Size = new Size(50, 50),
                Font = new Font(SystemFonts.DefaultFont.FontFamily, 14, FontStyle.Bold),
                Text = "↺"
            };
            ToolTip.SetToolTip(reset, "Обновить палитру");
            reset.Click += SetColors;
            colors = new TableLayoutPanel()
            {
                Location = new Point(0, layot.Height + 200),
                AutoSize = true,
                RowCount = 3,
                ColumnCount = 4
            };
            for (int i = 0; i < MAX_COLORS; i++)
            {
                var colorButton = new Button() { Size = new Size(50, 50), Tag = i, Name = $"{i}" };
                colorButton.Click += ChangeColorClick;
                colors.Controls.Add(colorButton);
                ToolTip.SetToolTip(colorButton, $"{colorButton.BackColor.Name}");
            }
            SetColors();

            saveButton = new Button()
            {
                Location = new Point(buildButton.Location.X + 10, buildButton.Location.Y),
                AutoSize = true,
                Font = new Font(SystemFonts.DefaultFont.FontFamily, 14, FontStyle.Bold),
                Text = "\U0001f4be",
                ForeColor = Color.Black,
                BackColor = Color.WhiteSmoke,
                Visible = false
            };
            ToolTip.SetToolTip(saveButton, "Сохранить");
            saveButton.Click += SavePicture;

            pictureBox = new PictureBox()
            {
                Location = new Point(layot.Location.X + layot.Width + 150, 50),
                Size = new Size(Screen.PrimaryScreen.Bounds.Width - 400, Screen.PrimaryScreen.Bounds.Height - 200),
            };
            centre = new PointD(pictureBox.Width / 2, pictureBox.Height / 2);
            zoom = (double.TryParse(textBoxZoom.Text, out zoom)) ? double.Parse(textBoxZoom.Text) : 1;

            layot.Controls.Add(textBoxZoom);
            layot.Controls.Add(buildButton);
            layot.Controls.Add(saveButton);

            Controls.Add(layot);
            Controls.Add(colors);
            Controls.Add(selectColors);
            Controls.Add(reset);
            Controls.Add(pictureBox);
            pictureBox.Click += DrawFractal;
            buildButton.Click += DrawFractal;
        }

        public void SetColors(object _sender = null, EventArgs e = null)
        {
            for (int i = 0; i < MAX_COLORS; i++)
            {
                var color = allColors[random.Next(allColors.Count)];
                while (settedColors.Contains(color))
                {
                    color = allColors[random.Next(allColors.Count)];
                }
                settedColors[i] = color;
                var button = (Button)colors.Controls.Find($"{i}", true).First();
                button.BackColor = settedColors[i];
            }
        }

        public void ChangeColorClick(object _sender, EventArgs e)
        {
            var sender = _sender as Button;
            sender.BackColor = allColors[(allColors.IndexOf(sender.BackColor) + 1) % allColors.Count];
            settedColors[(int)sender.Tag] = sender.BackColor;
        }

        public Color GetColour(int iteration)
        {
            if (iteration < MAX_COLORS * 10)
                return settedColors[iteration / 10];
            return Color.Black;
        }

        public void DrawFractal(object sender, EventArgs e)
        {
            double newZoom = (double.TryParse(textBoxZoom.Text, out newZoom)) ? double.Parse(textBoxZoom.Text) : 1;
            var delta = newZoom / zoom;
            if (sender is PictureBox)
            {
                if (pictureBox.Image == null) return;
                var args = (MouseEventArgs)e;
                dx = (args.Location.X - centre.X) * delta;
                dy = (-args.Location.Y + centre.Y) * delta;
            }
            dx *= delta;
            dy *= delta;
            Bitmap bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            centre = new PointD(pictureBox.Width / 2 - dx, pictureBox.Height / 2 + dy);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var x = (i - centre.X) / (newZoom * pictureBox.Width / 4);
                    var y = (-j + centre.Y) / (newZoom * pictureBox.Width / 4);
                    var c = new Complex(x, y);
                    var z = new Complex(0, 0);
                    var iteration = 0;
                    while (iteration < MAX_COLORS * 10)
                    {
                        iteration++;
                        z = z * z + c;
                        if ((z.Real * z.Real + z.Imaginary * z.Imaginary) >= 4)
                            break;
                    }
                    bitmap.SetPixel(i, j, GetColour(iteration));
                }
            }
            pictureBox.Image = bitmap;
            saveButton.Visible = true;
            zoom = newZoom;
        }

        public void SavePicture(object _sender, EventArgs e)
        {
            Directory.CreateDirectory("./saved");
            if (pictureBox.Image != null)
                pictureBox.Image.Save($"./saved/{Guid.NewGuid()}.png");
        }
    }
}