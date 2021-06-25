using ChartJSCore.Helpers;
using ChartJSCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.SystemHelpers;

namespace ITHelper.Helpers
{
    public static class ChartFactory
    {
        #region Enums &  Instance Objects

        public enum SortType { None, ByLabel, ByLabelDescending, ByValue, ByValueDescending }

        private static List<string> _colorList = new List<string>() { "#FF0000", "#800000", "#FFFF00", "#808000", "#00FF00", "#008000", "#00FFFF", "#008080", "#0000FF", "#000080", "#FF00FF", "#800080" };

        #endregion

        /// <summary>
        /// Returns a Bar Chart configuration tailored for OEE Reporting
        /// </summary>
        /// <param name="machineNo"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static Chart GetBarChart(IEnumerable<BarChartValue> data, string title, string scaleLabel, SortType sortType = SortType.None, bool displayLegend = true)
        {
            // Format the chart itself
            var titleFontSize = 12;
            try { titleFontSize = int.Parse(SystemHelper.GetConfigValue("AppSettings:TitleFontSize")); } catch { }

            var chart = new Chart();
            chart.Type = Enums.ChartType.Bar;
            chart.Options = new Options()
            {
                Responsive = true,
                Title = new Title()
                {
                    Display = !string.IsNullOrEmpty(title),
                    Text = title,
                    FontSize = titleFontSize
                },                
                Legend = new Legend() { Display = displayLegend }
            };

            // Specify the Chart Object to use
            chart.Data = new ChartJSCore.Models.Data();
            chart.Data.Datasets = new List<Dataset>();
            var allSeries = data.Select(x => x.Group).Distinct().ToList();
            chart.Data.Labels = allSeries;

            // Sort the data based on user request
            IDictionary<string, double?> sortedData;
            switch (sortType)
            {
                case SortType.None:
                    sortedData = data.ToDictionary(x => x.Label, y => y.Value);
                    break;

                case SortType.ByLabel:
                    sortedData = data.OrderBy(a => a.Label).ToDictionary(x => x.Label, y => y.Value);
                    break;

                case SortType.ByLabelDescending:
                    sortedData = data.OrderByDescending(a => a.Label).ToDictionary(x => x.Label, y => y.Value);
                    break;

                case SortType.ByValue:
                    sortedData = data.OrderBy(a => a.Value).ToDictionary(x => x.Label, y => y.Value);
                    break;

                case SortType.ByValueDescending:
                    sortedData = data.OrderByDescending(a => a.Value).ToDictionary(x => x.Label, y => y.Value);
                    break;

                default:
                    sortedData = data.ToDictionary(x => x.Label, y => y.Value);
                    break;
            }

            var labels = sortedData.Select(x => x.Key).ToList();
            var i = 0;
            foreach (var label in labels)
            {
                var dataSet = new BarDataset()
                {
                    Label = label,
                    Data = new List<double?> { sortedData[label] },
                    BackgroundColor = new List<ChartColor>() { ChartColor.FromHexString(_colorList[i % _colorList.Count]) },
                    BorderColor = new List<ChartColor>() { ChartColor.FromHexString("#000000") }
                };
                chart.Data.Datasets.Add(dataSet);
                i++;
            }

            // Return the chart to the caller
            return chart;
        }

        /// <summary>
        /// Returns a Line Chart configuration tailored for OEE Reporting
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static Chart GetLineChart(List<LineChartValue> data, string title, string scaleLabel, bool displayLegend = true)
        {
            // TODO: How do I put a time series to this?

            var titleFontSize = 12;
            try { titleFontSize = int.Parse(SystemHelper.GetConfigValue("AppSettings:TitleFontSize")); } catch { }
            var legendFontSize = 12;
            try { legendFontSize = int.Parse(SystemHelper.GetConfigValue("AppSettings:LegendFontSize")); } catch { }

            var chart = new Chart();
            chart.Type = Enums.ChartType.Line;
            if (data?.Count() > 0)
            {
                chart.Options = new Options()
                {
                    Responsive = true,
                    Title = new Title()
                    {
                        Display = !string.IsNullOrEmpty(title),
                        Text = title,
                        FontSize = titleFontSize
                    },
                    Scales = new Scales()
                    {
                        XAxes = new List<Scale>(),
                        YAxes = new List<Scale>() {
                        new CartesianScale() {
                            ScaleLabel = new ScaleLabel() {
                                Display = !string.IsNullOrEmpty(scaleLabel),
                                LabelString = scaleLabel
                            } } }
                    },
                    Legend = new Legend() { Display = displayLegend }
                };

                chart.Data = new ChartJSCore.Models.Data();
                chart.Data.Datasets = new List<Dataset>();

                var allSeries = data.Select(x => x.Label).Distinct().ToList();
                chart.Data.Labels = data.OrderBy(a => a.XValue).Select(x => x.XValue).Distinct().ToList();
                var i = 0;
                foreach (var series in allSeries)
                {
                    var samples = new List<double?>();
                    foreach (var label in chart.Data.Labels)
                    { samples.Add(data.Where(x => x.XValue.Equals(label) && x.Label.Equals(series)).Select(y => y.YValue).FirstOrDefault()); }

                    var linearDS = new LineDataset()
                    {
                        Label = series,
                        Data = samples,
                        SpanGaps = true,
                        BackgroundColor = ChartColor.FromHexString(_colorList[i % _colorList.Count]),
                        BorderColor = ChartColor.FromHexString(_colorList[i % _colorList.Count]),
                        BorderWidth = (samples.Count() > 50) ? 2 : 3,
                        BorderDashOffset = 0.0,
                        BorderJoinStyle = "miter",
                        PointHitRadius = new List<int> { 10 },
                        Fill = "false",
                        ShowLine = true,
                        PointRadius = (samples.Count() > 50) ? new List<int> { 0 } : new List<int> { 2 }
                    };
                    chart.Data.Datasets.Add(linearDS);
                    i++;
                }
            }

            return chart;
        }

        /// <summary>
        /// Returns a Pie Chart configuration displaying the data provided
        /// </summary>
        /// <param name="data">The data to be displayed</param>
        /// <param name="title">The overall chart title</param>
        /// <param name="scaleLabel"></param>
        /// <returns></returns>
        public static Chart GetPieChart(IDictionary<string, double?> data, string title, string scaleLabel, SortType sortType = SortType.None, bool displayLegend = true)
        {
            var titleFontSize = 12;
            try { titleFontSize = int.Parse(SystemHelper.GetConfigValue("AppSettings:TitleFontSize")); } catch { }
            var legendFontSize = 12;
            try { legendFontSize = int.Parse(SystemHelper.GetConfigValue("AppSettings:LegendFontSize")); } catch { }

            // Sort the data based on user request
            IDictionary<string, double?> sortedData;
            switch (sortType)
            {
                case SortType.None:
                    sortedData = data;
                    break;

                case SortType.ByLabel:
                    sortedData = data.OrderBy(a => a.Key).ToDictionary(x => x.Key, y => y.Value);
                    break;

                case SortType.ByLabelDescending:
                    sortedData = data.OrderByDescending(a => a.Key).ToDictionary(x => x.Key, y => y.Value);
                    break;

                case SortType.ByValue:
                    sortedData = data.OrderBy(a => a.Value).ToDictionary(x => x.Key, y => y.Value);
                    break;

                case SortType.ByValueDescending:
                    sortedData = data.OrderByDescending(a => a.Value).ToDictionary(x => x.Key, y => y.Value);
                    break;

                default:
                    sortedData = data;
                    break;
            }

            var chart = new Chart();
            chart.Type = Enums.ChartType.Pie;
            chart.Options = new Options()
            {
                Responsive = true,
                Title = new Title()
                {
                    Display = !string.IsNullOrEmpty(title),
                    Text = title,
                    FontSize = titleFontSize
                },
                Legend = new Legend() { Display = displayLegend }
            };

            chart.Data = new ChartJSCore.Models.Data();
            chart.Data.Datasets = new List<Dataset>();
            chart.Data.Labels = new List<string>();

            var colorList = new List<ChartColor>();
            var borderColors = new List<ChartColor>();
            var labels = sortedData.Keys;
            var i = 0;
            foreach (var name in labels)
            {
                chart.Data.Labels.Add(name);
                colorList.Add(ChartColor.FromHexString(_colorList[i++ % _colorList.Count]));
                borderColors.Add(ChartColor.FromHexString("#000000"));
            }

            //ChartJSCore.Models.Data modelData = new ChartJSCore.Models.Data();
            var dataset = new PieDataset()
            {
                Data = sortedData.Values.ToList(),
                BackgroundColor = colorList,
                BorderColor = borderColors,
                BorderWidth = 2
            };

            chart.Data.Datasets.Add(dataset);

            return chart;
        }
    }

    public class BarChartValue
    {
        public string Group { get; set; }

        public string Label { get; set; }

        public double? Value { get; set; }
    }

    public class LineChartValue
    {
        public string Label { get; set; }

        public string XValue { get; set; }

        public double? YValue { get; set; }
    }
}