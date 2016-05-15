using System;
using MifuminSoft.funyak.View.Utility;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace UWPTests
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class RectanglePage : Page
    {
        const int vw = 1920 / 32, vh = 1080 / 32;
        Rectangle[,] rectMap = new Rectangle[vw, vh];
        Rectangle rectMain = null;
        BitmapSource imageMap = null;
        FpsCounter counter = new FpsCounter();

        double vx = 0, vy = 0, vr = 0, vs =1;

        public RectanglePage()
        {
            InitializeComponent();
            imageMap = new BitmapImage(new Uri("ms-appx:///Assets/ice.bmp"));
            var random = new Random();
            for (int x = 0; x < vw; x++)
            {
                for (int y = 0; y < vh; y++)
                {
                    var brush = new ImageBrush();
                    brush.ImageSource = imageMap;
                    brush.Stretch = Stretch.None;
                    brush.AlignmentX = AlignmentX.Left;
                    brush.AlignmentY = AlignmentY.Top;
                    brush.Transform = new TransformGroup()
                    {
                        Children = new TransformCollection()
                        {
                            new TranslateTransform()
                            {
                                X = -32 * random.Next(5),
                                Y = 0,
                            },
                            new ScaleTransform()
                            {
                                CenterX = 0,
                                CenterY = 0,
                                ScaleX = vs,
                                ScaleY = vs,
                            },
                        }
                    };
                    rectMap[x, y] = new Rectangle()
                    {
                        Width = 32 * vs,
                        Height = 32 * vs,
                        Fill = brush,
                    };
                    canvas.Children.Add(rectMap[x, y]);
                }
            }
            rectMain = new Rectangle()
            {
                Width = 32 * vs,
                Height = 32 * vs,
                Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
            };
            canvas.Children.Add(rectMain);
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, object e)
        {
            for (int x = 0; x < vw; x++)
            {
                for (int y = 0; y < vh; y++)
                {
                    Canvas.SetLeft(rectMap[x, y], (x * 32 - vx) * vs);
                    Canvas.SetTop(rectMap[x, y], (y * 32 - vy) * vs);
                }
            }
            Canvas.SetLeft(rectMain, (vw * 16 - 16 - (vw - 1) * 16 * Math.Cos(vr) - vx) * vs);
            Canvas.SetTop(rectMain, (vh * 16 - 16 - (vh - 1) * 16 * Math.Sin(vr) - vy) * vs);
            vr += 0.01;
            counter.Step();
            textBlock.Text = "FPS：" + counter.Fps.ToString("0.00");
        }
    }
}
