using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MahnitolaList.Helpers
{
    public class InsertionAdorner : Adorner
    {
        private readonly bool _isAbove;
        private static readonly Pen _pen = new Pen(Brushes.DodgerBlue, 2);

        public InsertionAdorner(UIElement adornedElement, bool isAbove) : base(adornedElement)
        {
            IsHitTestVisible = false;
            _isAbove = isAbove;
        }

        protected override void OnRender(DrawingContext dc)
        {
            var r = new Rect(AdornedElement.RenderSize);
            double y = _isAbove ? 0 : r.Height;
            dc.DrawLine(_pen, new Point(0, y), new Point(r.Width, y));
        }
    }
}
