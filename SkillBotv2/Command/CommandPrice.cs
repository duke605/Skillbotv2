using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using Discord;
using Mono.Options;
using SkillBotv2.Util;
using Color = System.Drawing.Color;

namespace SkillBotv2.Command
{
    class CommandPrice : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            var a = new Arguments();
            var optSet = new OptionSet
            {
                { "c|chart=", (int? c) => a.Days = c }
            };

            a.Item = await RSUtil.GetItemForDynamic(string.Join(" ", optSet.Parse(args)));
            return a;
        }

        public async Task Execute(object a, Message message)
        {
            var args = (Arguments) a;
            string link = "";

            // Checking if user wants chart
            if (args.Days != null)
                link = await MakeChart(args.Item, args.Days.Value);

            // Outputing information
            if (args.Days != null)
                await message.Channel.SendMessage(
                    $"**{args.Item.Name}:** `{args.Item.Price.ToString("#,##0")}` GP\n" +
                    $"{link}");
            else
                await message.Channel.SendMessage(
                    $"**{args.Item.Name}:** `{args.Item.Price.ToString("#,##0")}` GP");

            using (var db = new Database())
            {
                // Adding item to db cause why not
                db.items.AddOrUpdate(args.Item);
            }
        }

        private async Task<string> MakeChart(item item, int days)
        {
            var history = (await RSUtil.GetPriceHistory(item.Name.Replace(@"\", "")))
                .Reverse()
                .Take(days)
                .Reverse()
                .Aggregate(new Dictionary<DateTime, int>(), (acc, h) =>
                {
                    acc[h.Key] = h.Value;
                    return acc;
                });
            var chart = new Chart();
            var chartArea = new ChartArea();
            var series = new Series();
            
            // Chart area setup
            double min = history.Aggregate(int.MaxValue, (a, h) => h.Value < a ? h.Value : a);
            double max = history.Aggregate(0, (a, h) => h.Value > a ? h.Value : a);
            min = Math.Floor(min*0.95);
            max = Math.Ceiling(Math.Min(double.MaxValue, max*1.05));
            
            // Chart setup
            chart.Size = new Size(539 + max.ToString("#,##0").Length * 8, 500);

            chartArea.AxisX.LabelStyle.Format = "MMM dd";
            chartArea.AxisY.LabelStyle.Format = "#,##0";
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(0x3e, 0x41, 0x46);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(0x3e, 0x41, 0x46);
            chartArea.AxisX.LabelStyle.ForeColor = Color.FromArgb(unchecked((int)4294967264));
            chartArea.AxisY.LabelStyle.ForeColor = Color.FromArgb(unchecked((int)4294967264));
            chartArea.BackColor = Color.FromArgb(0x2e, 0x31, 0x36);
            chart.BackColor = Color.FromArgb(0x2e, 0x31, 0x36);
            chartArea.AxisX.LabelStyle.Font = new Font("Consolas", 8);
            chartArea.AxisY.LabelStyle.Font = new Font("Consolas", 8);
            chartArea.AxisY.Minimum = min;
            chartArea.AxisY.Maximum = max;
            chart.ChartAreas.Add(chartArea);

            // Series setup
            series.Name = "Series1";
            series.ChartType = SeriesChartType.Line;
            series.XValueType = ChartValueType.DateTime;
            chart.Series.Add(series);

            // bind the datapoints
            chart.Series["Series1"].Points.DataBindXY(history.Keys, history.Values);
            
            // draw!
            chart.Invalidate();

            // write out a file
            using (var ms = new MemoryStream())
            {
                chart.SaveImage(ms, ChartImageFormat.Png);
                return await ImageUtil.PostToImgur(ms.ToArray());
            }
        }

        public struct Arguments
        {
            public item Item { get; set; }
            public int? Days { get; set; }
        }
    }
}
