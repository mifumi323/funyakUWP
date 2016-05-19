using System;
using System.Threading;
using System.Threading.Tasks;
using MifuminSoft.funyak.View.Utility;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using FeatureLevel = SharpDX.Direct3D.FeatureLevel;
// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace UWPTests
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class SharpDXTest : Page
    {
        SharpDX.Direct3D11.Device d3dDevice;
        SharpDX.Direct2D1.Device d2dDevice;
        SharpDX.Direct2D1.DeviceContext d2dContext;
        SurfaceImageSource sis;

        private FpsTimer fpsTimer = new FpsTimer(60);
        private FpsCounter drawCounter = new FpsCounter();
        private FpsCounter drawTryCounter = new FpsCounter();
        private FpsCounter frameCounter = new FpsCounter();
        private bool isAlive = true;
        private AutoResetEvent drawNotifier = new AutoResetEvent(false);

        const int vw = 1920 / 32, vh = 1080 / 32;
        private int[,] colorMap = new int[vw, vh];
        double vx = 0, vy = 0, vr = 0, vs = 1;

        public SharpDXTest()
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            sis = new SurfaceImageSource(1920, 1080);
            CreateDeviceResources();
            image.Source = sis;
            var task = new Task(MainLoop);
            task.Start();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            StopMainLoop();
        }

        private void CreateDeviceResources()
        {
            var creationFlags = DeviceCreationFlags.BgraSupport;
#if DEBUG
            creationFlags |= DeviceCreationFlags.Debug;
#endif

            FeatureLevel[] featureLevels =
            {
                FeatureLevel.Level_11_1,
                FeatureLevel.Level_11_0,
                FeatureLevel.Level_10_1,
                FeatureLevel.Level_10_0,
                FeatureLevel.Level_9_3,
                FeatureLevel.Level_9_2,
                FeatureLevel.Level_9_1,
            };

            d3dDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, creationFlags, featureLevels);

            using (var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device>())
            {
                // Create the Direct2D device object and a corresponding context.
                d2dDevice = new SharpDX.Direct2D1.Device(dxgiDevice);

                d2dContext = new SharpDX.Direct2D1.DeviceContext(d2dDevice, DeviceContextOptions.None);

                // Query for ISurfaceImageSourceNative interface.
                using (var sisNative = ComObject.QueryInterface<ISurfaceImageSourceNative>(sis))
                    sisNative.Device = dxgiDevice;
            }
        }

        private void CompositionTarget_Rendering(object sender, object e)
        {
            drawTryCounter.Step();
            if (drawNotifier.WaitOne(100))
            {
                drawCounter.Step();
                BeginDraw(new Rect(0, 0, 1920, 1080));
                d2dContext.Clear(new RawColor4(0.5f, 0.6f, 0.7f, 1f));
                for (int x = 0; x < vw; x++)
                {
                    for (int y = 0; y < vh; y++)
                    {
                        using (var brush = new SharpDX.Direct2D1.SolidColorBrush(d2dContext, new RawColor4(0.25f * colorMap[x, y], 1.0f, 1.0f, 1.0f)))
                        {
                            d2dContext.FillRectangle(new RawRectangleF((float)(x * 32 * vs), (float)(y * 32 * vs), (float)((x + 1) * 32 * vs), (float)((y + 1) * 32 * vs)), brush);
                        }
                    }
                }
                vr += 0.01;
                using (var brush = new SharpDX.Direct2D1.SolidColorBrush(d2dContext, new RawColor4(0.0f, 0.0f, 0.0f, 1.0f)))
                {
                    d2dContext.FillRectangle(new RawRectangleF((float)((vw * 16 - 16 - (vw - 1) * 16 * Math.Cos(vr) - vx) * vs), (float)((vh * 16 - 16 - (vh - 1) * 16 * Math.Sin(vr) - vy) * vs), (float)((vw * 16 + 16 - (vw - 1) * 16 * Math.Cos(vr) - vx) * vs), (float)((vh * 16 + 16 - (vh - 1) * 16 * Math.Sin(vr) - vy) * vs)), brush);
                }
                textBlock.Text =
                        "Draw: " + drawCounter.Frame + " " + drawCounter.Fps + "\n" +
                        "Draw(Try): " + drawTryCounter.Frame + " " + drawTryCounter.Fps + "\n" +
                        "Frame:" + frameCounter.Frame + " " + frameCounter.Fps;
                EndDraw();
            }
            else
            {
                // フレームスキップ
            }
        }

        private void BeginDraw(Rect updateRect)
        {
            // Express target area as a native RECT type.
            var updateRectNative = new RawRectangle
            {
                Left = (int)updateRect.Left,
                Top = (int)updateRect.Top,
                Right = (int)updateRect.Right,
                Bottom = (int)updateRect.Bottom
            };

            // Query for ISurfaceImageSourceNative interface.
            using (var sisNative = ComObject.QueryInterface<ISurfaceImageSourceNative>(sis))
            {
                // Begin drawing - returns a target surface and an offset to use as the top left origin when drawing.
                try
                {
                    RawPoint offset;
                    using (var surface = sisNative.BeginDraw(updateRectNative, out offset))
                    {

                        // Create render target.
                        using (var bitmap = new Bitmap1(d2dContext, surface))
                        {
                            // Set context's render target.
                            d2dContext.Target = bitmap;
                        }

                        // Begin drawing using D2D context.
                        d2dContext.BeginDraw();

                        // Apply a clip and transform to constrain updates to the target update area.
                        // This is required to ensure coordinates within the target surface remain
                        // consistent by taking into account the offset returned by BeginDraw, and
                        // can also improve performance by optimizing the area that is drawn by D2D.
                        // Apps should always account for the offset output parameter returned by 
                        // BeginDraw, since it may not match the passed updateRect input parameter's location.
                        d2dContext.PushAxisAlignedClip(
                            new RawRectangleF(
                                (offset.X),
                                (offset.Y),
                                (offset.X + (float)updateRect.Width),
                                (offset.Y + (float)updateRect.Height)
                                ),
                            AntialiasMode.Aliased
                            );

                        d2dContext.Transform = new RawMatrix3x2(1, 0, 0, 1, offset.X, offset.Y);
                    }
                }
                catch (SharpDXException ex)
                {
                    if (ex.ResultCode == SharpDX.DXGI.ResultCode.DeviceRemoved ||
                        ex.ResultCode == SharpDX.DXGI.ResultCode.DeviceReset)
                    {
                        // If the device has been removed or reset, attempt to recreate it and continue drawing.
                        CreateDeviceResources();
                        BeginDraw(updateRect);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private void EndDraw()
        {
            // Remove the transform and clip applied in BeginDraw since
            // the target area can change on every update.
            d2dContext.Transform = new RawMatrix3x2(1, 0, 0, 1, 0, 0);
            d2dContext.PopAxisAlignedClip();

            // Remove the render target and end drawing.
            d2dContext.EndDraw();

            d2dContext.Target = null;

            // Query for ISurfaceImageSourceNative interface.
            using (var sisNative = ComObject.QueryInterface<ISurfaceImageSourceNative>(sis))
                sisNative.EndDraw();
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
