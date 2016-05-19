using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using MifuminSoft.funyak.View.Utility;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace UWPTests
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class Win2DTest : Page
    {
        const int vw = 1920 / 32, vh = 1080 / 32;
        private FpsTimer fpsTimer = new FpsTimer(60);
        private FpsCounter drawCounter = new FpsCounter();
        private FpsCounter drawTryCounter = new FpsCounter();
        private FpsCounter frameCounter = new FpsCounter();
        private CanvasTextFormat canvasTextFormat = null;
        private bool isAlive = true;
        private AutoResetEvent drawNotifier = new AutoResetEvent(false);
        private CanvasBitmap source = null;
        private int[,] colorMap = new int[vw, vh];
        double vx = 0, vy = 0, vr = 0, vs = 1;

        public Win2DTest()
        {
            InitializeComponent();
            var random = new Random();
            for (int x = 0; x < vw; x++)
            {
                for (int y = 0; y < vh; y++)
                {
                    colorMap[x, y] = random.Next(5);
                }
            }
        }

        private void CompositionTarget_Rendering(object sender, object e)
        {
            drawTryCounter.Step();
            if (drawNotifier.WaitOne(100))
            {
                // 描画を行う
                drawCounter.Step();

                var swapChain = canvas.SwapChain;

                using (var ds = swapChain.CreateDrawingSession(Colors.AliceBlue))
                {
                    ds.Antialiasing = CanvasAntialiasing.Antialiased;
                    for (int x = 0; x < vw; x++)
                    {
                        for (int y = 0; y < vh; y++)
                        {
                            ds.DrawImage(source, new Rect(x * 32 * vs, y * 32 * vs, 32 * vs, 32 * vs), new Rect(colorMap[x, y] * 32, 0, 32, 32));
                        }
                    }
                    vr += 0.01;
                    ds.FillRectangle((float)((vw * 16 - 16 - (vw - 1) * 16 * Math.Cos(vr) - vx) * vs), (float)((vh * 16 - 16 - (vh - 1) * 16 * Math.Sin(vr) - vy) * vs), (float)(32 * vs), (float)(32 * vs), Colors.White);
                    ds.DrawText(
                        "Draw: " + drawCounter.Frame + " " + drawCounter.Fps + "\n" +
                        "Draw(Try): " + drawTryCounter.Frame + " " + drawTryCounter.Fps + "\n" +
                        "Frame:" + frameCounter.Frame + " " + frameCounter.Fps + "\n" +
                        "RenderSize:" + canvas.RenderSize,
                        100, 100, Colors.Red, canvasTextFormat);
                }
                swapChain.Present();
            }
            else
            {
                // フレームスキップ
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            source = await CanvasBitmap.LoadAsync(CanvasDevice.GetSharedDevice(), "Assets/ice.bmp");
            var swapChain = new CanvasSwapChain(CanvasDevice.GetSharedDevice(), 1920, 1080, 96);
            canvas.SwapChain = swapChain;
            canvasTextFormat = new CanvasTextFormat()
            {
                FontFamily = "Poor Richard",
                FontSize = 32,
            };
            var task = new Task(MainLoop);
            task.Start();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            StopMainLoop();
            canvas.SwapChain.Dispose();
            canvas.SwapChain = null;
        }

        private async void MainLoop()
        {
            while (isAlive)
            {
                OnFrame();
                frameCounter.Step();
                drawNotifier.Set();
                await fpsTimer.Wait();
            }
        }

        private void OnFrame()
        {
            // まだ何も作っていない
        }

        private void StopMainLoop()
        {
            isAlive = false;
        }
    }
}
