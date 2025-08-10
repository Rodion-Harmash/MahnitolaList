using MahnitolaList.Helpers;
using MahnitolaList.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MahnitolaList.Views
{
    public partial class PlaylistWindow : Window
    {
        private string selectedDirectory = string.Empty;
        public ObservableCollection<TrackItem> Tracks { get; } = new();

        private InsertionAdorner? _lineAdorner;
        private AdornerLayer? _lineLayer;

        private Window? _ghostWindow;
        private Size _ghostSize;
        private FrameworkElement? _pressedItemContainer;
        private bool _dragStarted;

        public PlaylistWindow()
        {
            InitializeComponent();
            DataContext = this;

            GiveFeedback += Window_GiveFeedback;
            QueryContinueDrag += Window_QueryContinueDrag;
        }

        private void ChkIncludeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtRadioName.Text))
                TxtRadioName.Text = "Patron Radio - 148.8FM";
        }

        private void ChkIncludeRadio_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                selectedDirectory = dialog.SelectedPath;
                LoadTracks();
            }
        }

        private void LoadTracks()
        {
            try
            {
                Tracks.Clear();
                var items = Utils.GetWavFiles(selectedDirectory)
                                 .Where(t => !t.FileName.Equals("stop_stop.wav", StringComparison.OrdinalIgnoreCase));
                foreach (var t in items) Tracks.Add(t);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка завантаження треків: " + ex.Message, "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnShuffle_Click(object sender, RoutedEventArgs e)
        {
            var list = Tracks.ToList();
            Utils.Shuffle(list);
            Tracks.Clear();
            foreach (var t in list) Tracks.Add(t);
            RefreshView();
        }

        private void BtnGenerateOsc_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedDirectory) || !Directory.Exists(selectedDirectory))
            {
                MessageBox.Show("Шлях не вибрано.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Tracks.Count == 0)
            {
                MessageBox.Show("Список треків порожній.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool includeRadio = ChkIncludeRadio.IsChecked == true;
                string radioName = (TxtRadioName.Text ?? "").Trim();
                if (includeRadio && radioName.Length == 0)
                    radioName = "Patron Radio - 148.8FM";

                var content = Utils.GenerateOscContent(Tracks.ToList(), includeRadio, radioName);
                var outPath = Path.Combine(selectedDirectory, "playlist.osc");
                Utils.SaveOscFile(outPath, content);
                MessageBox.Show("Файл списку створено успішно.", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка створення файлу: " + ex.Message, "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }

        private void TracksList_PreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            _pressedItemContainer = ItemsControl.ContainerFromElement(TracksList, e.OriginalSource as DependencyObject) as FrameworkElement;
            _dragStarted = false;
        }

        private void TracksList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_pressedItemContainer == null) return;
            if (e.LeftButton != MouseButtonState.Pressed) { _pressedItemContainer = null; return; }

            var moveDelta = (e.GetPosition(this) - e.GetPosition(_pressedItemContainer));
            if (!_dragStarted &&
                (Math.Abs(moveDelta.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(moveDelta.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                StartGhostWindow(_pressedItemContainer);
                _dragStarted = true;

                var data = (_pressedItemContainer as ListBoxItem)!.DataContext;
                DragDrop.DoDragDrop(_pressedItemContainer, data, DragDropEffects.Move);

                EndGhostWindow();
                _pressedItemContainer = null;
                _dragStarted = false;
            }
        }

        private void StartGhostWindow(FrameworkElement element)
        {
            var bmp = CreateSnapshot(element, out _ghostSize);

            _ghostWindow = new Window
            {
                Width = TracksList.ActualWidth,
                Height = _ghostSize.Height,
                AllowsTransparency = true,
                Background = null,
                IsHitTestVisible = false,
                WindowStyle = WindowStyle.None,
                Topmost = true,
                ShowInTaskbar = false,
                Opacity = 0.75,
                Content = new Image
                {
                    Source = bmp,
                    Margin = new Thickness(0, 4, 0, 0)
                }
            };

            UpdateGhostPosition();
            _ghostWindow.Show();
        }

        private void EndGhostWindow()
        {
            if (_ghostWindow != null)
            {
                _ghostWindow.Close();
                _ghostWindow = null;
            }
        }

        private void Window_GiveFeedback(object? sender, GiveFeedbackEventArgs e)
        {
            if (_ghostWindow != null)
            {
                e.UseDefaultCursors = false;
                UpdateGhostPosition();
                e.Handled = true;
            }
        }

        private void Window_QueryContinueDrag(object? sender, QueryContinueDragEventArgs e)
        {
            if (e.Action is DragAction.Cancel or DragAction.Drop)
            {
                EndGhostWindow();
                HideLine();
            }
        }

        private void UpdateGhostPosition()
        {
            if (_ghostWindow == null) return;

            var mousePos = System.Windows.Forms.Control.MousePosition;
            var listBoxTopLeft = TracksList.PointToScreen(new Point(0, 0));
            double listBoxWidth = TracksList.ActualWidth;
            double listBoxHeight = TracksList.ActualHeight;

            const double H_OFFSET = 1.0;
            _ghostWindow.Left = listBoxTopLeft.X + H_OFFSET;
            _ghostWindow.Width = Math.Max(40, listBoxWidth - H_OFFSET);

            double desiredY = mousePos.Y;
            double minY = listBoxTopLeft.Y;
            double maxY = listBoxTopLeft.Y + listBoxHeight - _ghostSize.Height - 15;
            double newY = Math.Max(minY, Math.Min(desiredY, maxY));

            _ghostWindow.Top = newY;
        }

        private static ImageSource CreateSnapshot(FrameworkElement element, out Size size)
        {
            element.UpdateLayout();
            double w = Math.Max(1.0, element.ActualWidth);
            double h = Math.Max(1.0, element.ActualHeight);
            size = new Size(w, h);

            var rtb = new RenderTargetBitmap((int)Math.Ceiling(w), (int)Math.Ceiling(h), 96, 96, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                var vb = new VisualBrush(element);
                dc.DrawRectangle(vb, null, new Rect(new Size(w, h)));
            }
            rtb.Render(dv);
            return rtb;
        }

        private void TracksList_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(TrackItem))) return;

            var posInList = e.GetPosition(TracksList);
            var sv = GetScrollViewer(TracksList);
            if (sv != null)
            {
                const double zone = 30;
                if (posInList.Y < zone) sv.LineUp();
                else if (posInList.Y > TracksList.ActualHeight - zone) sv.LineDown();
            }

            var target = GetItemContainerAt(TracksList, posInList);
            if (target != null)
            {
                bool above = e.GetPosition(target).Y < target.ActualHeight / 2;
                ShowLine(target, above);
            }
            else
            {
                var last = TracksList.ItemContainerGenerator.ContainerFromIndex(TracksList.Items.Count - 1) as ListBoxItem;
                if (last != null) ShowLine(last, false);
            }
        }

        private void TracksList_Drop(object sender, DragEventArgs e)
        {
            EndGhostWindow();
            HideLine();
            if (!e.Data.GetDataPresent(typeof(TrackItem))) return;

            var dragged = (TrackItem)e.Data.GetData(typeof(TrackItem))!;
            int oldIndex = Tracks.IndexOf(dragged);

            var pos = e.GetPosition(TracksList);
            var targetContainer = GetItemContainerAt(TracksList, pos);

            int newIndex;
            if (targetContainer == null)
            {
                newIndex = Tracks.Count;
            }
            else
            {
                var targetItem = (TrackItem)targetContainer.DataContext;
                bool above = e.GetPosition(targetContainer).Y < targetContainer.ActualHeight / 2;
                newIndex = Tracks.IndexOf(targetItem);
                if (!above) newIndex++;
            }

            if (newIndex > oldIndex) newIndex--;
            newIndex = Math.Max(0, Math.Min(newIndex, Tracks.Count));

            if (newIndex != oldIndex)
            {
                Tracks.RemoveAt(oldIndex);
                Tracks.Insert(newIndex, dragged);
                RefreshView();
            }
        }

        private static ListBoxItem? GetItemContainerAt(ListBox listBox, Point pos)
        {
            var el = listBox.InputHitTest(pos) as DependencyObject;
            while (el != null && el is not ListBoxItem)
                el = VisualTreeHelper.GetParent(el);
            return el as ListBoxItem;
        }

        private static ScrollViewer? GetScrollViewer(DependencyObject d)
        {
            if (d is ScrollViewer sv) return sv;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var res = GetScrollViewer(VisualTreeHelper.GetChild(d, i));
                if (res != null) return res;
            }
            return null;
        }

        private void ShowLine(ListBoxItem item, bool above)
        {
            HideLine();
            _lineLayer = AdornerLayer.GetAdornerLayer(item);
            if (_lineLayer == null) return;
            _lineAdorner = new InsertionAdorner(item, above);
            _lineLayer.Add(_lineAdorner);
        }

        private void HideLine()
        {
            if (_lineAdorner != null && _lineLayer != null)
            {
                _lineLayer.Remove(_lineAdorner);
                _lineAdorner = null;
                _lineLayer = null;
            }
        }

        private void RefreshView()
        {
            CollectionViewSource.GetDefaultView(Tracks)?.Refresh();
        }
    }
}
