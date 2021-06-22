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

        public enum SortType { None, ByKey, ByKeyDescending, ByValue, ByValueDescending }

        private static List<string> _colorList = new List<string>() { "#FF0000", "#800000", "#FFFF00", "#808000", "#00FF00", "#008000", "#00FFFF", "#008080", "#0000FF", "#000080", "#FF00FF", "#800080" };

        #endregion

        /// <summary>
        /// Returns a Bar Chart configuration tailored for OEE Reporting
        /// </summary>
        /// <param name="machineNo"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static Chart GetBarChart(IDictionary<string, double?> data, string title, string scaleLabel, SortType sortType = SortType.None) // TODO: Add an enumeration to control sorting: None, by Key, by Value
        {
            // Format the chart itself
            var titleFontSize = 12;
            try { titleFontSize = int.Parse(SystemHelper.GetConfigValue("AppSettings:TitleFontSize")); } catch { }

            IDictionary<string, double?> sortedData;
            switch (sortType)
            {
                case SortType.None:
                    sortedData = data;
                    break;

                case SortType.ByKey:
                    sortedData = data.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);
                    break;

                case SortType.ByKeyDescending:
                    sortedData = data.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, y => y.Value);
                    break;

                case SortType.ByValue:
                    sortedData = data.OrderBy(x => x.Value).ToDictionary(x => x.Key, y => y.Value);
                    break;

                case SortType.ByValueDescending:
                    sortedData = data.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, y => y.Value);
                    break;

                default:
                    sortedData = data;
                    break;
            }

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
                Scales = new Scales()
                {
                    XAxes = new List<Scale>(){
                        new CartesianScale(){
                        ScaleLabel = new ScaleLabel(){
                            LabelString = "X-Axis Label",
                            Display = true } } },
                    YAxes = new List<Scale>() {
                        new CartesianScale() {
                            ScaleLabel = new ScaleLabel() {
                                LabelString = scaleLabel,
                                Display = !string.IsNullOrEmpty(scaleLabel)
                            } } }
                },
                Legend = new Legend() { Display = true }
            };

            // Specify the data to be used for the chart
            chart.Data = new ChartJSCore.Models.Data();
            chart.Data.Datasets = new List<Dataset>();
            //chart.Data.Labels = data.Select(x => x.Key).ToList(); // Change incoming data structure to List<My New Barchart Structure> where it has three properties: Series, Label, Value

            var labels = sortedData.Select(x => x.Key).Distinct().ToList();
            var i = 0;
            foreach (var label in labels)
            {
                var dataSet = new BarDataset()
                {
                    Label = label + "\n",
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
        public static Chart GetLineChart(IDictionary<string, List<double?>> data, string title, string scaleLabel)
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
                    Legend = new Legend() { Display = true }
                };

                ChartJSCore.Models.Data modelData = new ChartJSCore.Models.Data();
                var labels = data.Select(x => x.Key).Distinct().ToList();
                foreach (var label in labels)
                {
                    var series = data[label];
                    var linearDS = new LineDataset()
                    {
                        Label = label,
                        Data = series,
                        BackgroundColor = ChartColor.FromRgba(75, 192, 192, 0.4),
                        BorderColor = ChartColor.FromRgb(75, 192, 192),
                        BorderWidth = (series.Count() > 50) ? 2 : 3,
                        BorderDashOffset = 0.0,
                        BorderJoinStyle = "miter",
                        PointHitRadius = new List<int> { 10 },
                        Fill = "false",
                        ShowLine = true,
                        PointRadius = (series.Count() > 50) ? new List<int> { 0 } : new List<int> { 2 }
                    };
                    modelData.Datasets.Add(linearDS);
                }
            }

            return chart;
        }

        /// <summary>
        /// Returns a Bar Chart configuration tailored for OEE Reporting
        /// </summary>
        /// <param name="machineNo"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static Chart GetOEEPieChart(IDictionary<string, double?> data, string title, string scaleLabel)
        {
            var titleFontSize = 12;
            try { titleFontSize = int.Parse(SystemHelper.GetConfigValue("AppSettings:TitleFontSize")); } catch { }
            var legendFontSize = 12;
            try { legendFontSize = int.Parse(SystemHelper.GetConfigValue("AppSettings:LegendFontSize")); } catch { }

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
                Scales = new Scales()
                {
                    XAxes = new List<Scale>(),
                    YAxes = new List<Scale>() {
                        new CartesianScale() {
                            ScaleLabel = new ScaleLabel() {
                                Display = !string.IsNullOrEmpty(scaleLabel),
                                LabelString = scaleLabel,
                            } } }
                },
                Legend = new Legend() { Display = true }
            };

            var colorList = new List<ChartColor>();
            var labels = data.Keys;
            foreach (var name in labels)
            {
                chart.Data.Labels.Add(name);
                colorList.Add(ChartColor.CreateRandomChartColor(true));
            }

            ChartJSCore.Models.Data modelData = new ChartJSCore.Models.Data();
            var dataset = new PieDataset();
            dataset = new PieDataset()
            {
                Data = data.Values.ToList(),
                BackgroundColor = colorList,
                BorderColor = new List<ChartColor>() { ChartColor.FromHexString("#000000") },
                BorderWidth = 2
            };

            chart.Data.Datasets.Add(dataset);

            return chart;
        }
    }
}