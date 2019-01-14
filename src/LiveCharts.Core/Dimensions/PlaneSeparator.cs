using LiveCharts.Animations;
using LiveCharts.Charts;
using LiveCharts.Drawing;
using LiveCharts.Drawing.Brushes;
using LiveCharts.Drawing.Shapes;
using LiveCharts.Interaction.Events;
using System.Collections.Generic;
using System.Drawing;
#if NET45 || NET46
using Brush = LiveCharts.Drawing.Brushes.Brush;
#endif

namespace LiveCharts.Dimensions
{
    /// <summary>
    /// Represents a mark in the UI that hels a plane to make it more readable, for example in the case of a cartesian plane
    /// a separator normally is drawn every x units to make it more readable for the final user.
    /// </summary>
    internal class PlaneSeparator : IResource
    {
        /// <summary>
        /// Gets or sets the stroke.
        /// </summary>
        public Brush? Stroke { get; set; }

        /// <summary>
        /// Gets or sets the fill.
        /// </summary>
        public Brush? Fill { get; set; }

        /// <summary>
        /// Gets or sets the Stroke dash array.
        /// </summary>
        public IEnumerable<double>? StrokeDashArray { get; set; }

        /// <summary>
        /// Gets or sets the stroke thickness.
        /// </summary>
        public double StrokeThickness { get; set; }

        /// <summary>
        /// Gets the shape in the UI.
        /// </summary>
        public IRectangle? Shape { get; internal set; }

        /// <summary>
        /// Gets the label in the UI.
        /// </summary>
        public ILabel? Label { get; internal set; }

        internal void DrawLabel(IChartView chart, AnimatableArguments animationArgs, PointF pos)
        {
            if (Label == null)
            {
                Label = UIFactory.GetNewLabel(chart.Model);
                Label.FlushToCanvas(chart.Canvas, true);
                Label.Left = pos.X;
                Label.Top = pos.Y;
            }

            Label.Foreground = Stroke;

            Label.Animate(animationArgs)
                .Property(nameof(ILabel.Left), Label.Left, pos.X)
                .Property(nameof(ILabel.Top), Label.Top, pos.Y)
                .Begin();
        }

        internal void DrawShape(IChartView chart, AnimatableArguments animationArgs, RectangleViewModel vm)
        {
            if (Shape == null)
            {
                Shape = UIFactory.GetNewRectangle(chart.Model);
                Shape.FlushToCanvas(chart.Canvas, true);
                Shape.Left = vm.From.Left;
                Shape.Top = vm.From.Top;
                Shape.Width = vm.From.Width;
                Shape.Height = vm.From.Height;
                Shape.ZIndex = int.MinValue + 1;

                Shape.Animate(animationArgs)
                    .Property(nameof(IShape.Opacity), 0, 1)
                    .Begin();
            }

            Shape.StrokeDashArray = StrokeDashArray;
            Shape.StrokeThickness = StrokeThickness;
            Shape.Fill = Fill;
            Shape.Stroke = Stroke;

            Shape.Animate(animationArgs)
                .Property(nameof(IShape.Top), Shape.Top, vm.To.Top)
                .Property(nameof(IShape.Left), Shape.Left, vm.To.Left)
                .Property(nameof(IShape.Height),
                    Shape.Height,
                    vm.To.Height > StrokeThickness
                        ? vm.To.Height
                        : StrokeThickness)
                .Property(nameof(IShape.Width),
                    Shape.Width,
                    vm.To.Width > StrokeThickness
                        ? vm.To.Width
                        : StrokeThickness)
               .Begin();
        }

        #region IResource implementation

        public event DisposingResourceHandler Disposed;

        public object UpdateId { get; set; } = new object();

        void IResource.Dispose(IChartView view, bool force)
        {
            Disposed?.Invoke(view, this, force);
        }

        #endregion
    }
}