using UnityEngine;

public class TestExcel : MonoBehaviour
{
    public string ExcelTableName;
    public string outputPath;

    void Start()
    {
        outputPath = Application.streamingAssetsPath + "/boat.xlsx";
        WriteXls(ExcelTableName, outputPath);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {

            LoadXls(outputPath);
        }
    }

    void LoadXls(string LoadPath)
    {
        Excel xls = ExcelHelper.LoadExcel(LoadPath);
        xls.ShowLog();
        xls.Tables[0].SetValue(1, 1, "asas");
        ExcelHelper.SaveExcel(xls, LoadPath);
    }


    void WriteXls(string excelTableName, string excelPath)
    {
        Excel xls = new Excel();
        ExcelTable table = new ExcelTable();
        table.TableName = excelTableName;
        xls.Tables.Add(table);
        xls.Tables[0].SetValue(1, 1, "1");
        xls.Tables[0].SetValue(1, 2, "2");
        xls.Tables[0].SetValue(1, 3, "3");
        xls.Tables[0].SetValue(2, 2, "2");
        xls.ShowLog();
        ExcelHelper.SaveExcel(xls, excelPath);
    }

}
