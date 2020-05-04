using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace HypothesisHelper
{
    public partial class Datagraph : Form
    {
        public double[] x { get; set; }
        public double[] y { get; set; }
        public string[] xlabels { get; set; }
        public int count { get; set; }
        private int t { get; set; }

        MathFunctions mf = new MathFunctions();

        Series series;

        public Datagraph(int type, string title)
        {
            InitializeComponent();

            Globals.datagraph = this;
            t = type;

            this.Text = title;

            switch (type)
            {
                case 0: // Line Chart
                    XYchart.ChartAreas["ChartArea1"].AxisX.Title = "Sample Number";
                    XYchart.ChartAreas["ChartArea1"].AxisY.Title = "Value";
                    XYchart.ChartAreas["ChartArea1"].AxisX.TextOrientation = TextOrientation.Horizontal;
                    XYchart.ChartAreas["ChartArea1"].AxisX.TitleAlignment = StringAlignment.Center;
                    XYchart.ChartAreas["ChartArea1"].AxisY.TitleAlignment = StringAlignment.Center;
                    series = XYchart.Series.Add("X-Y Plot");
                    series.ChartType = SeriesChartType.Line;
                    break;

                case 1: // Scatter Chart
                    XYchart.ChartAreas["ChartArea1"].AxisX.Title = "A";
                    XYchart.ChartAreas["ChartArea1"].AxisY.Title = "B";
                    XYchart.ChartAreas["ChartArea1"].AxisX.TextOrientation = TextOrientation.Horizontal;
                    XYchart.ChartAreas["ChartArea1"].AxisX.TitleAlignment = StringAlignment.Center;
                    XYchart.ChartAreas["ChartArea1"].AxisY.TextOrientation = TextOrientation.Horizontal;
                    XYchart.ChartAreas["ChartArea1"].AxisY.TitleAlignment = StringAlignment.Center;
                    series = XYchart.Series.Add("X-Y Plot");
                    series.ChartType = SeriesChartType.Point;
                    break;

                case 2: // Normalized Line Chart
                    XYchart.ChartAreas["ChartArea1"].AxisX.Title = "Sample Number";
                    XYchart.ChartAreas["ChartArea1"].AxisY.Title = "Normalized Value";
                    XYchart.ChartAreas["ChartArea1"].AxisX.TextOrientation = TextOrientation.Horizontal;
                    XYchart.ChartAreas["ChartArea1"].AxisX.TitleAlignment = StringAlignment.Center;
                    XYchart.ChartAreas["ChartArea1"].AxisY.TitleAlignment = StringAlignment.Center;
                    XYchart.ChartAreas["ChartArea1"].AxisY.Interval = 0.2;
                    XYchart.ChartAreas["ChartArea1"].AxisY.Maximum = 1.0;
                    XYchart.ChartAreas["ChartArea1"].AxisY.Minimum = 0.0;
                    series = XYchart.Series.Add("X-Y Plot");
                    series.ChartType = SeriesChartType.Line;
                    break;

                case 3: // Normalized Scatter
                    XYchart.ChartAreas["ChartArea1"].AxisX.Title = "A";
                    XYchart.ChartAreas["ChartArea1"].AxisY.Title = "B";
                    XYchart.ChartAreas["ChartArea1"].AxisX.TextOrientation = TextOrientation.Horizontal;
                    XYchart.ChartAreas["ChartArea1"].AxisX.TitleAlignment = StringAlignment.Center;
                    XYchart.ChartAreas["ChartArea1"].AxisY.TextOrientation = TextOrientation.Horizontal;
                    XYchart.ChartAreas["ChartArea1"].AxisY.TitleAlignment = StringAlignment.Center;
                    XYchart.ChartAreas["ChartArea1"].AxisX.Interval = 0.2;
                    XYchart.ChartAreas["ChartArea1"].AxisY.Interval = 0.2;
                    XYchart.ChartAreas["ChartArea1"].AxisX.Maximum = 1.0;
                    XYchart.ChartAreas["ChartArea1"].AxisX.Minimum = 0.0;
                    XYchart.ChartAreas["ChartArea1"].AxisY.Maximum = 1.0;
                    XYchart.ChartAreas["ChartArea1"].AxisY.Minimum = 0.0;
                    series = XYchart.Series.Add("X-Y Plot");
                    series.ChartType = SeriesChartType.Point;
                    break;

                case 4: // Bar Chart
                    XYchart.ChartAreas["ChartArea1"].AxisX.Title = "Range Average";
                    XYchart.ChartAreas["ChartArea1"].AxisY.Title = "Count";

                    series = XYchart.Series.Add("X-Y Plot");
                    series.ChartType = SeriesChartType.Column;
                    break;
            }
        }

        public void SetPoints()
        {
            int i;
            double[] xn,yn;

            switch (t)
            {
                case 0:
                case 1:
                    try
                    {
                        XYchart.Series[0].Points.Clear();
                        for (i = 0; i < count; i++)
                        {
                            series.Points.AddXY(x[i], y[i]);
                        }
                    }
                    catch
                    {
                        DialogResult dialogResult = MessageBox.Show("Error creating Plot", "Chart Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    break;

                case 2:
                    yn = mf.Normalize(y, y.Length);

                    try
                    {
                        XYchart.Series[0].Points.Clear();
                        for (i = 0; i < count; i++)
                        {
                            series.Points.AddXY(x[i], yn[i]);
                        }
                    }
                    catch
                    {
                        DialogResult dialogResult = MessageBox.Show("Error creating Plot", "Chart Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    break;

                case 3:  
                    xn = mf.Normalize(x, x.Length);
                    yn = mf.Normalize(y, y.Length);

                    try
                    {
                        XYchart.Series[0].Points.Clear();
                        for (i = 0; i < count; i++)
                        {
                            series.Points.AddXY(xn[i], yn[i]);
                        }
                    }
                    catch
                    {
                        DialogResult dialogResult = MessageBox.Show("Error creating Plot", "Chart Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    break;

                case 4:
                    try
                    {
                        XYchart.Series[0].Points.Clear();
                        for (i = 0; i < count; i++)
                        {
                            series.Points.AddXY(xlabels[i], y[i]);
                        }
                    }
                    catch
                    {
                        DialogResult dialogResult = MessageBox.Show("Error creating Plot", "Chart Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    break;
            }
        }
    }
}
