using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftArtisans.OfficeWriter.ExcelWriter;
using MathNet.Numerics.Statistics;

namespace StatisticalAnalysis
{
    class Program
    {
        
        static void Main(string[] args)
        {

            ExcelApplication app = new ExcelApplication();
            Workbook workbook = app.Create(ExcelApplication.FileFormat.Xlsx);
            for (int i = 0; i < workbook.Worksheets.Count; i++)
                workbook.Worksheets.Delete(0);
            Worksheet sheet = workbook.Worksheets.CreateWorksheet("Data");

            var shipTimes = new List<double>();
            var orderPrices = new List<double>();
            var orderCount = new List<double>();

            using (var db = new DatabaseEntities())
            {
                var query = from b in db.Orders select b;
                var i = 0;
                sheet[i, 0].Value = "Order Date";
                sheet[i, 1].Value = "Customer";
                sheet[i, 2].Value = "Required Date";
                sheet[i, 3].Value = "Shipped Date";
                i++;

                // Write the order data out to Excel
                foreach (var item in query)
                {
                    sheet[i, 0].Value = item.Order_Date;
                    sheet[i, 1].Value = item.Customer.Company_Name;
                    sheet[i, 2].Value = item.Required_Date;
                    sheet[i, 3].Value = item.Shipped_Date;

                    sheet[i, 0].Style.NumberFormat = NumberFormat.DateFormat.MonthDayYear;
                    sheet[i, 2].Style.NumberFormat = NumberFormat.DateFormat.MonthDayYear;
                    sheet[i, 3].Style.NumberFormat = NumberFormat.DateFormat.MonthDayYear;

                    // Calculate the time from order to delivery
                    if ((item.Shipped_Date != null) && (item.Order_Date != null))
                        shipTimes.Add((double)(item.Shipped_Date.Value.Subtract(item.Order_Date.Value)).TotalDays);

                    // Calculate the price of each order
                    decimal totalPrice = 0;
                    foreach (Order_Detail detail in item.Order_Details)
                        totalPrice += detail.Unit_Price;
                    
                    orderPrices.Add((double)totalPrice);

                    orderCount.Add(item.Order_Details.Count);

                    i++;
                }

            }

            sheet = workbook.Worksheets.CreateWorksheet("DeliveryTime");
            WriteHistogramToSheet(sheet, shipTimes);

            sheet = workbook.Worksheets.CreateWorksheet("OrderValue");
            WriteHistogramToSheet(sheet, orderPrices);

            sheet = workbook.Worksheets.CreateWorksheet("OrderCount");
            WriteHistogramToSheet(sheet, orderCount);

            app.Save(workbook, "C:\\Users\\sethm\\Desktop\\HelloWorld.xlsx");

        }

        private static void WriteHistogramToSheet(Worksheet sheet, IEnumerable<double> data)
        {
            // Throw delivery time into the histogram
            Histogram hist = new Histogram(data, 60);

            sheet[0, 0].Value = "Count";
            sheet[0, 1].Value = "Width";
            sheet[0, 2].Value = "LowerBound";
            sheet[0, 3].Value = "UpperBound";

            // Write the histogram data out to Excel
            for (int i = 1; i < hist.BucketCount; i++)
            {
                sheet[i, 0].Value = hist[i].Count;
                sheet[i, 1].Value = hist[i].Width;
                sheet[i, 2].Value = hist[i].LowerBound;
                sheet[i, 3].Value = hist[i].UpperBound;
            }

            // Plot the histogram data
            Charts charts = sheet.Charts;
            Anchor anchor = sheet.CreateAnchor(4, 6, 0, 0);
            Chart chart = charts.CreateChart(ChartType.Column.Clustered, anchor);
            SeriesCollection collection = chart.SeriesCollection;
            collection.CategoryData = String.Format("{0}!{1}:{2}", sheet.Name, sheet.Cells[1, 2].Name, sheet.Cells[50, 2].Name);
            Series series = collection.CreateSeries(String.Format("{0}!{1}:{2}", sheet.Name, sheet.Cells[1, 0].Name, sheet.Cells[50, 0].Name));

        }
    }
}
