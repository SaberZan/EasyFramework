using OfficeOpenXml;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;


public class ExcelHelper
{
    public static Excel LoadExcel(string path)
    {
       // UnityEngine.Debuger.Log("path "+ path);


        FileInfo file = new FileInfo(path);

        ///UnityEngine.Debuger.Log("file.Length "+file.Length);

        ExcelPackage ep = new ExcelPackage(file);
        Excel xls = new Excel(ep.Workbook);
        return xls;
    }

    public static Excel LoadExcelFromStream(string path)
    {
        UnityWebRequest network = new UnityWebRequest(path);
        //  Debuger.LogError("network.error "+ network.error);
        while (!network.isDone) {}
        //Debuger.Log("network.bytes length " + network.bytes.Length);
        MemoryStream ms = new MemoryStream(network.downloadHandler.data);
        ExcelPackage ep = new ExcelPackage(ms);
        Excel xls = new Excel(ep.Workbook);
        return xls;
    }

    public static Excel CreateExcel(string path)
    {
        ExcelPackage ep = new ExcelPackage();
        ep.Workbook.Worksheets.Add("sheet");
        Excel xls = new Excel(ep.Workbook);
        SaveExcel(xls, path);
        return xls;
    }

    public static void SaveExcel(Excel xls, string path)
    {
        FileInfo output = new FileInfo(path);
        ExcelPackage ep = new ExcelPackage();
        for (int i = 0; i < xls.Tables.Count; i++)
        {
            ExcelTable table = xls.Tables[i];
            ExcelWorksheet sheet = ep.Workbook.Worksheets.Add(table.TableName);
            for (int row = 1; row <= table.NumberOfRows; row++)
            {
                for (int column = 1; column <= table.NumberOfColumns; column++)
                {
                    sheet.Cells[row, column].Value = table.GetValue(row, column);
                }
            }
        }
        ep.SaveAs(output);
    }
}
