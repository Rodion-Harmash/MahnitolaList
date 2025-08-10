using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MahnitolaList.Helpers
{
    public sealed class DragGhostAdorner : Adorner
    {
        private readonly ImageSource _image;
        private Point _position;
        private readonly Size _size;

        public double OpacityValue { get; set; } = 0.7;

        public DragGhostAdorner(UIElement adornerScope, ImageSource image, Size size)
            : base(adornerScope)
        {
            IsHitTestVisible = false;
            _image = image;
            _size = size;
        }

        public void UpdatePosition(Point pos)
        {
            _position = pos;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            dc.PushOpacity(OpacityValue);
            dc.DrawImage(_image, new Rect(_position, _size));
            dc.Pop();
        }
    }
}
