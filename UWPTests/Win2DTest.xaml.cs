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
        private FpsTimer fpsTimer = new FpsTimer(60);
        private FpsCounter drawCounter = new FpsCounter();
        private FpsCounter drawTryCounter = new FpsCounter();
        private FpsCounter frameCounter = new FpsCounter();
        private CanvasTextFormat canvasTextFormat = null;
        private bool isAlive = true;
        private AutoResetEvent drawNotifier = new AutoResetEvent(false);
        private CanvasBitmap source = null;

        public Win2DTest()
        {
            InitializeComponent();
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
                    int i = 1;
                    for (long ticks = DateTime.Now.Ticks; ticks > DateTime.Now.AddMilliseconds(-10).Ticks;)
                    {
                        //ds.DrawEllipse(155 + i, 115, 80, 30, Colors.Black, 3);
                        ds.DrawImage(source, new Rect(i, i, 500, 500), new Rect(0, 0, 300, 300));
                        i++;
                    }
                    ds.DrawText(
                        "Draw: " + drawCounter.Frame + " " + drawCounter.Fps + "\n" +
                        "Draw(Try): " + drawTryCounter.Frame + " " + drawTryCounter.Fps + "\n" +
                        "Frame:" + frameCounter.Frame + " " + frameCounter.Fps + "\n" +
                        "RenderSize:" + canvas.RenderSize + "\n" +
                        "i:" + i,
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
            source = await CanvasBitmap.LoadAsync(CanvasDevice.GetSharedDevice(), "Assets/Square150x150Logo.scale-200.png");
            var swapChain = new CanvasSwapChain(CanvasDevice.GetSharedDevice(), 800, 600, 96);
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
