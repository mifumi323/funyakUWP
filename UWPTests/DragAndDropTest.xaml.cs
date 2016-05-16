using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace UWPTests
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class DragAndDropTest : Page
    {
        int dragOverCount = 0;
        int dragDropCount = 0;
        string operation = "";
        string data = "";

        public DragAndDropTest()
        {
            InitializeComponent();
            UpdateText();
        }

        private void UpdateText()
        {
            textBlock.Text = "ここにドロップ\nOperation: " + operation + "\nDragOver: " + dragOverCount + "\nDragDrop: " + dragDropCount + "\nData: " + data;
        }

        private void Rectangle_DragOver(object sender, DragEventArgs e)
        {
            dragOverCount++;
            operation = "DragOver";

            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.DragUIOverride.Caption = "File";
            }
            else if (e.DataView.Contains(StandardDataFormats.Text))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.DragUIOverride.Caption = "Text";
            }

            UpdateText();
            e.Handled = true;
        }

        private async void Rectangle_Drop(object sender, DragEventArgs e)
        {
            dragDropCount++;
            operation = "DragDrop";

            var d = e.GetDeferral();

            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var files = await e.DataView.GetStorageItemsAsync();
                var file = files.First();
                data = file.Name;
            }
            else if (e.DataView.Contains(StandardDataFormats.Text))
            {
                data = await e.DataView.GetTextAsync();
            }

            UpdateText();

            d.Complete();
        }
    }
}
