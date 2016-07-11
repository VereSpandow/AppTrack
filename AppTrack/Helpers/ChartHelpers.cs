using System;
using System.Collections.Generic;

using System.Web.UI.DataVisualization.Charting;
using System.IO;
using System.Drawing;

namespace AppTrack.Helpers
{

    public class ChartSeriesItem
    {
        public string Label { get; set; }
        public double Value { get; set; }
        public ChartSeriesItem(string _label, double _value)
        {
            Label = _label;
            Value = _value;
        }
    }

    public class ChartHelpers
    {
        public string getChartImage(Chart chart)
        {
            using (var stream = new MemoryStream())
            {
                string img = "<img src='data:image/png;base64,{0}' alt='' usemap='#ImageMap'>";
                chart.SaveImage(stream, ChartImageFormat.Png);
                string encoded = Convert.ToBase64String(stream.ToArray());
                return String.Format(img, encoded);
            }
        }

        public Title CreateTitle(string chartTitle)
        {
            Title title = new Title();
            title.Text = chartTitle;
            title.ShadowColor = Color.FromArgb(32, 0, 0, 0);
            title.Font = new Font("Trebuchet MS", 14F, FontStyle.Bold);
            title.ShadowOffset = 3;
            title.ForeColor = Color.FromArgb(26, 59, 105);
            return title;
        }


        public Legend CreateLegend()
        {
            Legend legend = new Legend();
            legend.Enabled = true;
            legend.ShadowColor = Color.FromArgb(32, 0, 0, 0);
            legend.Font = new Font("Trebuchet MS", 14F, FontStyle.Bold);
            legend.ShadowOffset = 3;
            legend.ForeColor = Color.FromArgb(26, 59, 105);
            legend.Title = "Legend";
            return legend;
        }


        public ChartArea CreateChartArea(string chartTitle, string AxisXTitle, string AxisYTitle)
        {
            ChartArea chartArea = new ChartArea();
            chartArea.Name = chartTitle;
            chartArea.BackColor = Color.Transparent;
            chartArea.AxisX.Title = AxisXTitle;
            chartArea.AxisY.Title = AxisYTitle;
            chartArea.AxisX.IsLabelAutoFit = false;
            chartArea.AxisY.IsLabelAutoFit = false;
            chartArea.AxisY.LabelStyle.Format = "$###,###,###.##";
            chartArea.AxisX.LabelStyle.Font = new Font("Verdana,Arial,Helvetica,sans-serif", 8F, FontStyle.Regular);
            chartArea.AxisY.LabelStyle.Font = new Font("Verdana,Arial,Helvetica,sans-serif", 8F, FontStyle.Regular);
            chartArea.AxisY.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisX.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisX.Interval = 1;
            return chartArea;
        }


        public Series CreateSeries(IList<ChartSeriesItem> results, SeriesChartType chartType, string chartTitle, string seriesName)
        {
            Series seriesDetail = new Series();
            seriesDetail.Name = seriesName;
            seriesDetail.IsValueShownAsLabel = false;
            seriesDetail.Color = Color.FromArgb(149, 200, 60);
            seriesDetail.ChartType = chartType;
            seriesDetail.BorderWidth = 2;

            DataPoint point;
            foreach (ChartSeriesItem result in results)
            {
                point = new DataPoint();
                point.AxisLabel = result.Label;
                point.YValues = new double[] { result.Value };
                seriesDetail.Points.Add(point);
            }

            /* 			for (int i = 1; i < 10; i++) 
                        { 
                            var p = seriesDetail.Points.Add(i); 
                            p.Label = String.Format("Point {0}", i); 
                            p.LabelMapAreaAttributes = String.Format("href=\"javascript:void(0)\" onclick=\"myfunction('{0}')\"", i); 
                            p["BarLabelStyle"] = "Center"; 
                        } 
 
             */
            seriesDetail.ChartArea = chartTitle;
            return seriesDetail;
        }

    }
}



