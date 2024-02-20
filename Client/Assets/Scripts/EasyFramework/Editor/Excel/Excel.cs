using System.Collections.Generic;
using OfficeOpenXml;
using UnityEngine;

public class Excel
{
    public List<ExcelTable> Tables = new List<ExcelTable>();

    public Excel()
    {

    }

    public Excel(ExcelWorkbook wb)
    {
       // UnityEngine.Debuger.Log("wb.Worksheets.Count " + wb.Worksheets.Count);

        for (int i = 1; i <= wb.Worksheets.Count; i++)
        {
            ExcelWorksheet sheet = wb.Worksheets[i];
            ExcelTable table = new ExcelTable(sheet);
            Tables.Add(table);
        }
    }

    /// <summary>
    /// 显示整个Excel内容
    /// </summary>
    public void ShowLog()
    {
        for (int i = 0; i < Tables.Count; i++)
        {
            Tables[i].ShowLog();
        }
    }
    
    public object GetContent(int tableIndex,int row,int col)
    {
        return Tables[tableIndex].GetValue(row, col);
    }

    public List<List<string>> GetTableContent(int tableIndex)
    {
        return Tables[tableIndex].GetTableContent();
    }



    public void AddTable(string name)
    {
        ExcelTable table = new ExcelTable();
        table.TableName = name;
        Tables.Add(table);
    }

}
