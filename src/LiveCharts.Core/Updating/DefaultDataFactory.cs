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

using System.Collections.Generic;
using System.ComponentModel;
using LiveCharts.Charts;
using LiveCharts.Coordinates;
using LiveCharts.Drawing.Shapes;
using LiveCharts.Interaction.Points;
using LiveCharts.Interaction.Series;

#endregion

namespace LiveCharts.Updating
{
    /// <summary>
    /// Defines the default chart point factory.
    /// </summary>
    /// <seealso cref="IDataFactory" />
    public class DataFactory : IDataFactory
    {
        private static readonly object _nullKey = new object();

        /// <inheritdoc />
        public void Fetch<TModel, TCoordinate, TPointShape>(
            DataFactoryContext<TModel, TCoordinate> context,
            Dictionary<object, ChartPoint<TModel, TCoordinate, TPointShape>> tracker,
            out int count)
            where TCoordinate : ICoordinate
            where TPointShape : class, IShape
        {
            ModelToCoordinateMapper<TModel, TCoordinate> mapper = context.Mapper;
            bool notifiesChange = context.Series.Metadata.IsObservable;
            IList<TModel> collection = context.Collection;
            bool isValueType = context.Series.Metadata.IsValueType;

            for (int index = 0; index < collection.Count; index++)
            {
                var instance = collection[index];

                object key = isValueType ? index : instance ?? _nullKey;

                if (!tracker.TryGetValue(key, out ChartPoint<TModel, TCoordinate, TPointShape> chartPoint))
                {
                    chartPoint = new ChartPoint<TModel, TCoordinate, TPointShape>(context.Series, context.Chart.View);
                    tracker.Add(key, chartPoint);
                    if (notifiesChange)
                    {
                        var npc = (INotifyPropertyChanged)instance;

                        // ToDo: Evaluate memory consumption with and without observers...
                        void InvalidateOnPropertyChanged(object sender, PropertyChangedEventArgs e)
                        {
                            chartPoint.Chart.Model.Invalidate();
                        }

                        void DisposeByValPoint(IChartView view, object sender, bool force)
                        {
                            npc.PropertyChanged -= InvalidateOnPropertyChanged;
                            tracker.Remove(key);
                        }

                        npc.PropertyChanged += InvalidateOnPropertyChanged;
                        chartPoint.Disposed += DisposeByValPoint;
                    }
                }

                // feed our chart points ...
                chartPoint.Model = instance;
                chartPoint.Key = index;
                chartPoint.Series = context.Series;
                chartPoint.Chart = context.Chart.View;
                chartPoint.Coordinate = mapper.PointPredicate.Invoke(instance, index);

                // compare the dimensions to scale the chart.
                chartPoint.Coordinate.Compare(context);

                // register our chart point at the resource collector
                context.Chart.RegisterINotifyPropertyChanged(chartPoint);
            }

            count = collection.Count;
        }
    }
}
