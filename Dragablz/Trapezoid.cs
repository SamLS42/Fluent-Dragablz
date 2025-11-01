using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dragablz
{
    public class Trapezoid : ContentControl
    {
        private PathGeometry _pathGeometry;

        static Trapezoid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Trapezoid), new FrameworkPropertyMetadata(typeof(Trapezoid)));
            BackgroundProperty.OverrideMetadata(typeof(Trapezoid), new FrameworkPropertyMetadata(Panel.BackgroundProperty.DefaultMetadata.DefaultValue,
                        FrameworkPropertyMetadataOptions.AffectsRender));
        }

        public static readonly DependencyProperty PenBrushProperty = DependencyProperty.Register(
            "PenBrush", typeof(Brush), typeof(Trapezoid), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Transparent), FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Brush PenBrush
        {
            get => (Brush)GetValue(PenBrushProperty); set => SetValue(PenBrushProperty, value);
        }

        public static readonly DependencyProperty LongBasePenBrushProperty = DependencyProperty.Register(
            "LongBasePenBrush", typeof(Brush), typeof(Trapezoid), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Transparent), FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Brush LongBasePenBrush
        {
            get => (Brush)GetValue(LongBasePenBrushProperty); set => SetValue(LongBasePenBrushProperty, value);
        }

        public static readonly DependencyProperty PenThicknessProperty = DependencyProperty.Register(
            "PenThickness", typeof(double), typeof(Trapezoid), new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double PenThickness
        {
            get => (double)GetValue(PenThicknessProperty); set => SetValue(PenThicknessProperty, value);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size contentDesiredSize = base.MeasureOverride(constraint);

            if (contentDesiredSize.Width == 0 || double.IsInfinity(contentDesiredSize.Width)
                || contentDesiredSize.Height == 0 || double.IsInfinity(contentDesiredSize.Height))

                return contentDesiredSize;

            _pathGeometry = CreateGeometry(contentDesiredSize);
            Clip = _pathGeometry;

            return _pathGeometry.GetRenderBounds(new Pen(PenBrush, 1)
            {
                EndLineCap = PenLineCap.Flat,
                MiterLimit = 1
            }).Size;
        }

        private Pen CreatePen()
        {
            return new Pen(PenBrush, PenThickness)
            {
                EndLineCap = PenLineCap.Flat,
                MiterLimit = 1
            };
        }

        private static PathGeometry CreateGeometry(Size contentDesiredSize)
        {
            //TODO Make better :)  do some funky beziers or summit
            const double cheapRadiusBig = 6.0;
            const double cheapRadiusSmall = cheapRadiusBig / 2;

            const int angle = 0;
            const double radians = angle * (Math.PI / 180);

            Point startPoint = new(0, contentDesiredSize.Height + cheapRadiusSmall + cheapRadiusSmall);

            //clockwise starting at bottom left
            ArcSegment bottomLeftSegment = new(new Point(startPoint.X + cheapRadiusSmall, startPoint.Y - cheapRadiusSmall),
                new Size(cheapRadiusSmall, cheapRadiusSmall), 315, false, SweepDirection.Counterclockwise, true);
            double triangleX = Math.Tan(radians) * contentDesiredSize.Height;
            LineSegment leftSegment = new(new Point(bottomLeftSegment.Point.X + triangleX, bottomLeftSegment.Point.Y - contentDesiredSize.Height), true);
            ArcSegment topLeftSegment = new(new Point(leftSegment.Point.X + cheapRadiusBig, leftSegment.Point.Y - cheapRadiusSmall), new Size(cheapRadiusBig, cheapRadiusBig), 120, false, SweepDirection.Clockwise, true);
            LineSegment topSegment = new(new Point(contentDesiredSize.Width + cheapRadiusBig + cheapRadiusBig, 0), true);
            ArcSegment topRightSegment = new(new Point(contentDesiredSize.Width + cheapRadiusBig + cheapRadiusBig + cheapRadiusBig, cheapRadiusSmall), new Size(cheapRadiusBig, cheapRadiusBig), 40, false, SweepDirection.Clockwise, true);

            triangleX = Math.Tan(radians) * contentDesiredSize.Height;
            //triangleX = Math.Tan(radians)*(contentDesiredSize.Height - topRightSegment.Point.Y);
            LineSegment rightSegment =
                new(new Point(topRightSegment.Point.X + triangleX,
                    topRightSegment.Point.Y + contentDesiredSize.Height), true);

            Point bottomRightPoint = new(rightSegment.Point.X + cheapRadiusSmall,
                rightSegment.Point.Y + cheapRadiusSmall);
            ArcSegment bottomRightSegment = new(bottomRightPoint,
                new Size(cheapRadiusSmall, cheapRadiusSmall), 25, false, SweepDirection.Counterclockwise, true);
            Point bottomLeftPoint = new(0, bottomRightSegment.Point.Y);
            LineSegment bottomSegment = new(bottomLeftPoint, true);

            PathSegmentCollection pathSegmentCollection =
            [
                bottomLeftSegment, leftSegment, topLeftSegment, topSegment, topRightSegment, rightSegment, bottomRightSegment, bottomSegment
            ];
            PathFigure pathFigure = new(startPoint, pathSegmentCollection, true)
            {
                IsFilled = true
            };
            PathFigureCollection pathFigureCollection =
            [
                pathFigure
            ];
            PathGeometry geometryGroup = new(pathFigureCollection);
            geometryGroup.Freeze();

            return geometryGroup;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawGeometry(Background, CreatePen(), _pathGeometry);

            if (_pathGeometry == null) return;
            drawingContext.DrawGeometry(Background, new Pen(LongBasePenBrush, PenThickness)
            {
                EndLineCap = PenLineCap.Flat,
                MiterLimit = 1
            }, new LineGeometry(_pathGeometry.Bounds.BottomLeft + new Vector(3, 0), _pathGeometry.Bounds.BottomRight + new Vector(-3, 0)));
        }
    }
}
