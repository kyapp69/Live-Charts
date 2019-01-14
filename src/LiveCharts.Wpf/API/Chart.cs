#region License
// The MIT License (MIT)
// 
// Copyright (c) 2016 Alberto Rodr�guez Orozco & LiveCharts contributors
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights to 
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
// of the Software, and to permit persons to whom the Software is furnished to 
// do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR 
// OTHER DEALINGS IN THE SOFTWARE.
#endregion
#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using LiveCharts.Charts;
using LiveCharts.DataSeries;
using LiveCharts.Dimensions;
using LiveCharts.Drawing;
using LiveCharts.Interaction.Controls;
using LiveCharts.Interaction.Events;

#endregion

namespace LiveCharts.Wpf
{
    /// <summary>
    /// Defines a chart class.
    /// </summary>
    /// <seealso cref="Canvas" />
    /// <seealso cref="IChartView" />
    public abstract class Chart : Canvas, IChartView
    {
        /// <summary>
        /// Initializes the <see cref="Chart"/> class.
        /// </summary>
        static Chart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Chart),
                new FrameworkPropertyMetadata(typeof(Chart)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Chart"/> class.
        /// </summary>
        protected Chart()
        {
            VisualDrawMargin = new ContentPresenter();
            DrawMargin = Padding.Empty;
            SetValue(LegendProperty, new ChartLegend());
            SetValue(DataTooltipProperty, new ChartToolTip());
            Unloaded += OnUnloaded;
            SizeChanged += OnSizeChanged;
            TooltipPopup = new Popup
            {
                AllowsTransparency = true,
                Placement = PlacementMode.RelativePoint
            };
            Children.Add(TooltipPopup);
            Children.Add(VisualDrawMargin);
        }

        /// <summary>
        /// Gets the tooltip popup.
        /// </summary>
        /// <value>
        /// The tooltip popup.
        /// </value>
        public Popup TooltipPopup { get; protected set; }

        public ContentPresenter VisualDrawMargin { get; set; }

        #region Dependency properties

        /// <summary>
        /// The draw margin property, default is LiveCharts.Drawing.Padding.Empty which means that the library will calculate it.
        /// </summary>
        public static readonly DependencyProperty DrawMarginProperty = DependencyProperty.Register(
            nameof(DrawMargin), typeof(Padding), typeof(Chart),
            new PropertyMetadata(Padding.Empty, RaiseOnPropertyChanged(nameof(DrawMargin))));

        /// <summary>
        /// The series property.
        /// </summary>
        public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register(
            nameof(Series), typeof(IEnumerable<ISeries>), typeof(Chart),
            new PropertyMetadata(null, RaiseOnPropertyChanged(nameof(Series))));

        /// <summary>
        /// The animations speed property, default value is 550 milliseconds.
        /// </summary>
        public static readonly DependencyProperty AnimationsSpeedProperty = DependencyProperty.Register(
            nameof(AnimationsSpeed), typeof(TimeSpan), typeof(Chart),
            new PropertyMetadata(TimeSpan.FromMilliseconds(1000), RaiseOnPropertyChanged(nameof(AnimationsSpeed))));

        /// <summary>
        /// The tooltip timeout property, default is 150 ms.
        /// </summary>
        public static readonly DependencyProperty TooltipTimeoutProperty = DependencyProperty.Register(
            nameof(TooltipTimeOut), typeof(TimeSpan), typeof(Chart),
            new PropertyMetadata(TimeSpan.FromMilliseconds(150)));

        /// <summary>
        /// The legend property, default is DefaultLegend class.
        /// </summary>
        public static readonly DependencyProperty LegendProperty = DependencyProperty.Register(
            nameof(Legend), typeof(ILegend), typeof(Chart),
            new PropertyMetadata(null, RaiseOnPropertyChanged(nameof(Legend))));

        /// <summary>
        /// The legend position property
        /// </summary>
        public static readonly DependencyProperty LegendPositionProperty = DependencyProperty.Register(
            nameof(LegendPosition), typeof(LegendPosition), typeof(Chart),
            new PropertyMetadata(LegendPosition.None, RaiseOnPropertyChanged(nameof(LegendPosition))));

        /// <summary>
        /// The updater state property
        /// </summary>
        public static readonly DependencyProperty UpdaterStateProperty = DependencyProperty.Register(
            nameof(UpdaterState), typeof(UpdaterStates), typeof(Chart),
            new PropertyMetadata(UpdaterStates.Running, RaiseOnPropertyChanged(nameof(UpdaterState))));

        /// <summary>
        /// The data tooltip property.
        /// </summary>
        public static readonly DependencyProperty DataTooltipProperty = DependencyProperty.Register(
            nameof(DataToolTip), typeof(IDataToolTip), typeof(Chart),
            new PropertyMetadata(null));

        /// <summary>
        /// The animation line property, default is bounce medium.
        /// </summary>
        public static readonly DependencyProperty AnimationLineProperty = DependencyProperty.Register(
            nameof(EasingFunction), typeof(Animations.Ease.IEasingFunction), typeof(Chart),
            new PropertyMetadata(Animations.EasingFunctions.Ease));

        /// <summary>
        /// The chart update preview command property
        /// </summary>
        public static readonly DependencyProperty ChartUpdatePreviewCommandProperty = DependencyProperty.Register(
            nameof(ChartUpdatePreviewCommand), typeof(ICommand), typeof(Chart), 
            new PropertyMetadata(default(ICommand), (o, args) =>
            {
                var chart = (Chart) o;
                chart.Model.UpdatePreviewCommand = chart.ChartUpdatePreviewCommand;
            }));

        /// <summary>
        /// The chart updated command property
        /// </summary>
        public static readonly DependencyProperty ChartUpdatedCommandProperty = DependencyProperty.Register(
            nameof(ChartUpdatedCommand), typeof(ICommand), typeof(Chart),
            new PropertyMetadata(default(ICommand), (o, args) =>
            {
                var chart = (Chart) o;
                chart.Model.UpdatedCommand = chart.ChartUpdatedCommand;
            }));

        /// <summary>
        /// The data pointer entered command property
        /// </summary>
        public static readonly DependencyProperty DataPointerEnteredCommandProperty = DependencyProperty.Register(
            nameof(DataPointerEnteredCommand), typeof(ICommand), typeof(Chart),
            new PropertyMetadata(default(ICommand), (o, args) =>
            {
                var chart = (Chart) o;
                chart.Model.DataPointerEnteredCommand = chart.DataPointerEnteredCommand;
            }));

        /// <summary>
        /// The data pointer left command property
        /// </summary>
        public static readonly DependencyProperty DataPointerLeftCommandProperty = DependencyProperty.Register(
            nameof(DataPointerLeftCommand), typeof(ICommand), typeof(Chart),
            new PropertyMetadata(default(ICommand), (o, args) =>
            {
                var chart = (Chart) o;
                chart.Model.DataPointerLeftCommand = chart.DataPointerLeftCommand;
            }));

        /// <summary>
        /// The data pointer down command property
        /// </summary>
        public static readonly DependencyProperty DataPointerDownCommandProperty = DependencyProperty.Register(
            nameof(DataPointerDownCommand), typeof(ICommand), typeof(Chart),
            new PropertyMetadata(default(ICommand), (o, args) =>
            {
                var chart = (Chart) o;
                chart.Model.DataPointerDownCommand = chart.DataPointerDownCommand;
            }));

        private ChartModel _model;

        #endregion

        #region private and protected methods

        /// <summary>
        /// Gets the planes in the current chart.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected abstract IList<IList<Plane>> GetOrderedDimensions();

        /// <summary>
        /// Notifies that the specified property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        protected static PropertyChangedCallback RaiseOnPropertyChanged(string propertyName)
        {
            return (sender, eventArgs) =>
            {
                var chart = (Chart) sender;
                chart.OnPropertyChanged(propertyName);
            };
        }

        /// <summary>
        /// Called when [unloaded].
        /// </summary>
        /// <param name="sender1">The sender1.</param>
        /// <param name="routedEventArgs">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnUnloaded(object sender1, RoutedEventArgs routedEventArgs)
        {
            Dispose();
        }

        /// <summary>
        /// Called when [model set].
        /// </summary>
        protected virtual void OnModelSet()
        {
            Model.DataPointerDown += (chart, points, args) =>
            {
                DataPointerDown?.Invoke(chart, points, args);
            };
            Model.DataPointerEntered += (chart, points, args) =>
            {
                DataPointerEntered?.Invoke(chart, points, args);
            };
            Model.DataPointerLeft += (chart, points, args) =>
            {
                DataPointerLeft?.Invoke(chart, points, args);
            };
        }

        private void OnSizeChanged(object sender1, SizeChangedEventArgs sizeChangedEventArgs)
        {
            ViewResized?.Invoke(this);
        }

        #endregion

        #region IChartView implementation

        public event ChartEventHandler ViewResized;

        /// <inheritdoc />
        public event ChartEventHandler ChartUpdatePreview
        {
            add => Model.UpdatePreview += value;
            remove => Model.UpdatePreview -= value;
        }

        /// <inheritdoc cref="IChartView.ChartUpdatePreview" />
        public ICommand ChartUpdatePreviewCommand
        {
            get => (ICommand) GetValue(ChartUpdatePreviewCommandProperty);
            set => SetValue(ChartUpdatePreviewCommandProperty, value);
        }

        /// <inheritdoc />
        public event ChartEventHandler ChartUpdated
        {
            add => Model.Updated += value;
            remove => Model.Updated -= value;
        }

        /// <inheritdoc cref="IChartView.ChartUpdated" />
        public ICommand ChartUpdatedCommand
        {
            get => (ICommand) GetValue(ChartUpdatedCommandProperty);
            set => SetValue(ChartUpdatedCommandProperty, value);
        }

        /// <inheritdoc />
        public event DataInteractionHandler DataPointerEntered;

        /// <inheritdoc />
        public ICommand DataPointerEnteredCommand
        {
            get => (ICommand)GetValue(DataPointerEnteredCommandProperty);
            set => SetValue(DataPointerEnteredCommandProperty, value);
        }

        /// <inheritdoc />
        public event DataInteractionHandler DataPointerLeft;

        /// <inheritdoc />
        public ICommand DataPointerLeftCommand
        {
            get => (ICommand)GetValue(DataPointerLeftCommandProperty);
            set => SetValue(DataPointerLeftCommandProperty, value);
        }

        /// <inheritdoc />
        public event DataInteractionHandler DataPointerDown;

        /// <inheritdoc />
        public ICommand DataPointerDownCommand
        {
            get => (ICommand)GetValue(DataPointerDownCommandProperty);
            set => SetValue(DataPointerDownCommandProperty, value);
        }

        /// <inheritdoc cref="IChartView.Model"/>
        public ChartModel Model
        {
            get => _model;
            protected set
            {
                _model = value;
                OnModelSet();
            }
        }

        public ICanvas Canvas
        {
            get => (ICanvas) VisualDrawMargin.Content;
            set => VisualDrawMargin.Content = value;
        }

        /// <inheritdoc cref="IChartView.ControlSize"/>
        float[] IChartView.ControlSize => new[] {(float) ActualWidth, (float) ActualHeight};

        /// <inheritdoc cref="IChartView.DrawMargin"/>
        public Padding DrawMargin
        {
            get => (Padding) GetValue(DrawMarginProperty);
            set => SetValue(DrawMarginProperty, value);
        }

        /// <inheritdoc />
        IList<IList<Plane>> IChartView.Dimensions => GetOrderedDimensions();

        /// <inheritdoc cref="IChartView.Series"/>
        public IEnumerable<ISeries> Series
        {
            get => (IEnumerable<ISeries>) GetValue(SeriesProperty);
            set => SetValue(SeriesProperty, value);
        }

        /// <inheritdoc />
        public UpdaterStates UpdaterState
        {
            get => (UpdaterStates) GetValue(UpdaterStateProperty);
            set => SetValue(UpdaterStateProperty, value);
        }

        /// <inheritdoc cref="IChartView.AnimationsSpeed"/>
        public TimeSpan AnimationsSpeed
        {
            get => (TimeSpan) GetValue(AnimationsSpeedProperty);
            set => SetValue(AnimationsSpeedProperty, value);
        }

        /// <inheritdoc />
        public Animations.Ease.IEasingFunction EasingFunction
        {
            get => (Animations.Ease.IEasingFunction) GetValue(AnimationLineProperty);
            set => SetValue(AnimationLineProperty, value);
        }

        /// <inheritdoc />
        public TimeSpan TooltipTimeOut
        {
            get => (TimeSpan) GetValue(TooltipTimeoutProperty);
            set => SetValue(TooltipTimeoutProperty, value);
        }

        /// <inheritdoc cref="IChartView.Legend"/>
        public ILegend Legend
        {
            get => (ILegend) GetValue(LegendProperty);
            set => SetValue(LegendProperty, value);
        }

        /// <inheritdoc cref="IChartView.LegendPosition"/>
        public LegendPosition LegendPosition
        {
            get => (LegendPosition) GetValue(LegendPositionProperty);
            set => SetValue(LegendPositionProperty, value);
        }

        /// <inheritdoc cref="IChartView.DataToolTip"/>
        public IDataToolTip DataToolTip
        {
            get => (IDataToolTip) GetValue(DataTooltipProperty);
            set => SetValue(DataTooltipProperty, value);
        }

        void IChartView.SetContentArea(RectangleF rectangle)
        {
            SetLeft(VisualDrawMargin, rectangle.Left);
            SetTop(VisualDrawMargin, rectangle.Top);
            VisualDrawMargin.Width = rectangle.Width;
            VisualDrawMargin.Height = rectangle.Height;
        }

        /// <inheritdoc />
        public void ForceUpdate(bool restartAnimations = false)
        {
            Model.Invalidate(restartAnimations, true);
        }

        void IChartView.InvokeOnUiThread(Action action)
        {
            Dispatcher.Invoke(action);
        }

        #endregion

        #region INPC implementation

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public virtual void Dispose()
        {
            Model.Dispose();
            Series = null;
            Children.Remove(TooltipPopup);
            Children.Remove(VisualDrawMargin);
            SetValue(LegendProperty, null);
            SetValue(DataTooltipProperty, null);
            TooltipPopup = null;
            VisualDrawMargin = null;
            Unloaded -= OnUnloaded;
            SizeChanged -= OnSizeChanged;
            GC.Collect();
        }

        void IChartView.CapturePointer()
        {
            CaptureMouse();
        }

        void IChartView.ReleasePointerCapture()
        {
            ReleaseMouseCapture();
        }
    }
}
