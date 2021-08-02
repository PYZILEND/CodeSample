using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.IO;

namespace VatsCallsMonitoring.Models
{
    class ExcelReader
    {
        public static void ReadCallsByUsersData()
        {            
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            DirectoryInfo dir = new DirectoryInfo("UserCalls");
            foreach (FileInfo file in dir.GetFiles("*.xlsx"))
            {
                using (ExcelPackage excel = new ExcelPackage(file))
                {
                    ExcelWorksheet worksheet = excel.Workbook.Worksheets.FirstOrDefault();
                    int rowCount = worksheet.Dimension.Rows;

                    for(int i = 2; i < rowCount; i++)
                    {

                    }
                }
            }
        }
    }
}