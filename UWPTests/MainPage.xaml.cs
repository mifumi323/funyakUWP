using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace UWPTests
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MenuList.ItemsSource = new object[]
            {
                new {
                    Name = "SharpDX.Direct2Dのテスト",
                    Type = typeof(SharpDXTest),
                },
                new {
                    Name = "ドラッグアンドドロップのテスト",
                    Type = typeof(DragAndDropTest),
                },
                new {
                    Name = "Rectangleで画像表示するテスト",
                    Type = typeof(RectanglePage),
                },
                new {
                    Name = "Win2Dを動かすテスト",
                    Type = typeof(Win2DTest),
                },
            };
        }

        private void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dynamic selectedItem = MenuList.SelectedItem;
            if (selectedItem == null) return;
            var type = (Type)selectedItem.Type;
            MainFrame.Navigate(type);
        }
    }
}
