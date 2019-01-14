﻿#region License
// The MIT License (MIT)
// 
// Copyright (c) 2016 Alberto Rodríguez Orozco & LiveCharts contributors
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using LiveCharts.Animations;
using LiveCharts.Charts;
using LiveCharts.Coordinates;
using LiveCharts.Drawing.Brushes;
using LiveCharts.Drawing.Shapes;
using LiveCharts.Drawing.Styles;
using LiveCharts.Interaction.Controls;
using LiveCharts.Interaction.Points;
using LiveCharts.Interaction.Series;
using LiveCharts.Updating;
#if NET45 || NET46
using Font = LiveCharts.Drawing.Styles.Font;
using Brush = LiveCharts.Drawing.Brushes.Brush;
#endif

#endregion

namespace LiveCharts.DataSeries
{
    /// <summary>
    /// A series with a defined coordinate.
    /// </summary>
    /// <typeparam name="TCoordinate">The type of the coordinate.</typeparam>
    /// <seealso cref="ISeries" />
    public interface ISeries<TCoordinate> : ISeries
        where TCoordinate : ICoordinate
    {
        /// <summary>
        /// Gets or sets the data label formatter, a delegate that will build every DataLabel.
        /// </summary>
        /// <value>
        /// The data label formatter.
        /// </value>
        Func<TCoordinate, string>? DataLabelFormatter { get; set; }

        /// <summary>
        /// Gets or sets the tooltip formatter, a delegate that will build the coordinate in the tooltip.
        /// </summary>
        /// <value>
        /// The tooltip formatter.
        /// </value>
        Func<TCoordinate, string>? TooltipFormatter { get; set; }
    }


    /// <summary>
    /// The series interface.
    /// </summary>
    public interface ISeries : IResource, INotifyPropertyChanged, ICoreChildAnimatable
    {
        /// <summary>
        /// Gets the resource key, the type used to style this element.
        /// </summary>
        /// <value>
        /// The resource key.
        /// </value>
        Type ThemeKey { get; }

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <value>
        /// The metadata.
        /// </value>
        SeriesMetadata Metadata { get; }

        /// <summary>
        /// Gets or sets the values to display in the plot.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        IEnumerable Values { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the series should display a label for every point in the series.
        /// </summary>
        /// <value>
        ///   <c>true</c> if display labels; otherwise, <c>false</c>.
        /// </value>
        bool DataLabels { get; set; }

        /// <summary>
        /// Gets or sets the data labels position.
        /// </summary>
        /// <value>
        /// The data labels position.
        /// </value>
        DataLabelsPosition DataLabelsPosition { get; set; }

        /// <summary>
        /// Gets or sets the data labels foreground.
        /// </summary>
        /// <value>
        /// The data labels foreground.
        /// </value>
        Brush DataLabelsForeground { get; set; }

        /// <summary>
        /// Gets or sets the default fill opacity, this property is used to determine the fill opacity of a point when 
        /// LiveCharts sets the fill automatically based on the theme.
        /// </summary>
        /// <value>
        /// The default fill opacity.
        /// </value>
        double DefaultFillOpacity { get; set; }

        /// <summary>
        /// Gets the default width of the point, this property is used internally by the library and should only be used
        ///  by you if you need to build a custom cartesian series.
        /// </summary>
        /// <value>
        /// The default width of the point.
        /// </value>
        float[] DefaultPointWidth { get; }

        /// <summary>
        /// Gets or sets the font, the font will be used as the <see cref="DataLabels"/> font of this series.
        /// </summary>
        /// <value>
        /// The font.
        /// </value>
        Font DataLabelsFont { get; set; }

        /// <summary>
        /// Gets or sets the geometry, the geometry property is used to represent the series in the legend and
        /// depending on the series type it could also set the base geometry to draw every point (Line, Scatter and Bubble series).
        /// </summary>
        /// <value>
        /// The geometry.
        /// </value>
        Geometry Geometry { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </value>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        string Title { get; set; }

        /// <summary>
        /// Gets the index of the group, -1 indicates that the series is not grouped.
        /// </summary>
        /// <value>
        /// The index of the group.
        /// </value>
        int GroupingIndex { get; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        Dictionary<ChartModel, Dictionary<string, object>> Content { get; }

        /// <summary>
        /// Gets the point margin, this property is used internally by the library and should only be used
        /// by you if you need to build a custom cartesian series.
        /// </summary>
        /// <value>
        /// The point margin.
        /// </value>
        float PointMargin { get; }

        /// <summary>
        /// Gets or sets the delay rule.
        /// </summary>
        /// <value>
        /// The delay rule.
        /// </value>
        DelayRules DelayRule { get; set; }

        /// <summary>
        /// Fetches the specified chart.
        /// </summary>
        /// <param name="chart">The chart.</param>
        /// <param name="context">The update context.</param>
        void Fetch(ChartModel chart, UpdateContext context);

        /// <summary>
        /// Stores the series as a chart resource.
        /// </summary>
        /// <param name="chart">The chart.</param>
        void UsedBy(ChartModel chart);

        /// <summary>
        /// Gets the interacted points according to a mouse position.
        /// </summary>
        /// <param name="pointerLocation">The pointer location.</param>
        /// <param name="selectionMode">The selection mode.</param>
        /// <param name="snapToClosest">Specifies if the result should only get the closest point.</param>
        /// <param name="chart">The chart view.</param>
        /// <returns></returns>
        IEnumerable<IChartPoint>? GetPointsAt(PointF pointerLocation, ToolTipSelectionMode selectionMode, bool snapToClosest, IChartView chart);

        /// <summary>
        /// Highlights a point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="chart">The chart view.</param>
        void OnPointHighlight(IChartPoint point, IChartView chart);

        /// <summary>
        /// Removes the highlight of a point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="chart">The chart view.</param>
        void RemovePointHighlight(IChartPoint point, IChartView chart);

        /// <summary>
        /// Gets the label for.
        /// </summary>
        /// <param name="coordinate">The coordinate.</param>
        string GetDataLabel(ICoordinate coordinate);

        /// <summary>
        /// Gets the tooltip label.
        /// </summary>
        /// <param name="coordinate">The coordinate.</param>
        /// <returns></returns>
        string GetTooltipLabel(ICoordinate coordinate);

        /// <summary>
        /// Updates the started.
        /// </summary>
        /// <param name="chart">The chart.</param>
        void UpdateStarted(IChartView chart);

        /// <summary>
        /// Updates the finished.
        /// </summary>
        /// <param name="chart">The chart.</param>
        void UpdateFinished(IChartView chart);

        /// <summary>
        /// Updates the view.
        /// </summary>
        /// <param name="chart">The chart.</param>
        /// <param name="context">The context.</param>
        void UpdateView(ChartModel chart, UpdateContext context);
    }
}
