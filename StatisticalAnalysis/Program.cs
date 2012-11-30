using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftArtisans.OfficeWriter.ExcelWriter;

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

            using (var db = new DatabaseEntities())
            {
                var query = from b in db.Orders select b;
                var i = 0;
                sheet[i, 0].Value = "Order Date";
                sheet[i, 0].Value = "Customer";
                sheet[i, 0].Value = "Required Date";
                sheet[i, 0].Value = "Shipped Date";
                i++;
                
                foreach (var item in query)
                {
                    sheet[i, 0].Value = item.Order_Date;
                    sheet[i, 1].Value = item.Customer.Company_Name;
                    sheet[i, 2].Value = item.Required_Date;
                    sheet[i, 3].Value = item.Shipped_Date;
                    
                    i++;
                }

                sheet.CreateAreaOfColumns(0, 1).SetStyle(NumberFormat.DateFormat.DayOfWeekMonthDayYear);
            }

            app.Save(workbook, "C:\\Users\\sethm\\Desktop\\HelloWorld.xlsx");

        }
    }
}
