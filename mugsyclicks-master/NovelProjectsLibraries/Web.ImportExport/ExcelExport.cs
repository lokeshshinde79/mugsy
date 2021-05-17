#region

using System;
using System.Data;
using System.Web;
using System.Web.UI;

#endregion

namespace NovelProjects.Web
{
  public enum ExportFormat
  {
    CSV,
    PCXLS
  }

  public class ExcelExport
  {
    #region Private variables 

    private Page page;

    #endregion

    public ExcelExport(Page TargetPage)
    {
      page = TargetPage;
    }

    public static void Export(Page TargetPage, DataSet Data, string FileName)
    {
      Export(TargetPage, Data, FileName, ExportFormat.PCXLS);
    }

    public static void Export(Page TargetPage, DataSet Data, string FileName, ExportFormat Format)
    {
      ExcelExport e = new ExcelExport(TargetPage);

      if (Format == ExportFormat.PCXLS)
      {
        e.ExportInPCXLSFormat(Data, FileName);
      }
      else
      {
        e.ExportInCSVFormat(Data, FileName);
      }
    }

    public void ExportInPCXLSFormat(DataSet Data, string FileName)
    {
      page.Response.Clear();
      page.Response.ContentType = "application/ms-excel";
      page.Response.AddHeader("Content-Disposition",
                              "attachment; Filename=\"" + HttpUtility.UrlDecode(FileName) + "\"");

      WriteWorkbookHeader();

      foreach (DataTable table in Data.Tables)
      {
        WriteTable(table);
      }

      WriteWorkbookFooter();
      page.Response.End();
    }

    public void ExportInCSVFormat(DataSet Data, string FileName)
    {
      page.Response.Clear();
      page.Response.ContentType = "application/ms-excel";
      page.Response.AddHeader("Content-Disposition",
                              "attachment; Filename=\"" + HttpUtility.UrlDecode(FileName) + "\"");

      foreach (DataTable table in Data.Tables)
      {
        WriteCSV(table);
      }

      page.Response.End();
    }


    private void WriteWorkbookHeader()
    {
      page.Response.Write("<?xml version=\"1.0\"?>\r\n");
      page.Response.Write("<?mso-application progid=\"Excel.Sheet\"?>\r\n");
      page.Response.Write("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"\r\n");
      page.Response.Write("xmlns:o=\"urn:schemas-microsoft-com:office:office\"\r\n");
      page.Response.Write("xmlns:x=\"urn:schemas-microsoft-com:office:excel\"\r\n");
      page.Response.Write("xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"\r\n");
      page.Response.Write("xmlns:html=\"http://www.w3.org/TR/REC-html40\">\r\n");
      page.Response.Write("<DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">\r\n");
      page.Response.Write("<LastAuthor>MSINC</LastAuthor>\r\n");
      page.Response.Write("  <Created>" + DateTime.Now.ToString() + "</Created>\r\n");
      page.Response.Write("  <Version>11.5703</Version>\r\n");
      page.Response.Write("</DocumentProperties>\r\n");
      page.Response.Write("<ExcelWorkbook xmlns=\"urn:schemas-microsoft-com:office:excel\">\r\n");
      page.Response.Write("  <ProtectStructure>False</ProtectStructure>\r\n");
      page.Response.Write("  <ProtectWindows>False</ProtectWindows>\r\n");
      page.Response.Write("</ExcelWorkbook>\r\n");
      page.Response.Write(" <Styles>\r\n");
      page.Response.Write("  <Style ss:ID=\"s1\">\r\n");
      page.Response.Write("   <Font ss:Bold=\"1\"/>\r\n");
      page.Response.Write("  </Style>\r\n");
      page.Response.Write(" </Styles>\r\n");
    }

    private void WriteWorkbookFooter()
    {
      page.Response.Write("</Workbook>\r\n");
    }

    private void WriteCSV(DataTable table)
    {
      WriteCSVHeader(table);
      WriteCSVRows(table);
    }

    private void WriteCSVHeader(DataTable table)
    {
      string collist = "";

      foreach (DataColumn column in table.Columns)
      {
        collist += "\"" + column.ColumnName + "\",";
      }

      if (collist.EndsWith(","))
      {
        collist = collist.Remove(collist.LastIndexOf(','));
      }

      page.Response.Write(collist + "\r\n");
    }

    private void WriteCSVRows(DataTable table)
    {
      foreach (DataRow Row in table.Rows)
      {

        string row = "";

        foreach (DataColumn column in table.Columns)
        {
          row += "\"" + Row[column] + "\",";
        }

        if (row.EndsWith(","))
        {
          row = row.Remove(row.LastIndexOf(','));
        }

        page.Response.Write(row + "\r\n");
      }
    }

    private void WriteTableHeader(DataTable table)
    {
      foreach (DataColumn column in table.Columns)
        page.Response.Write("<Column>" + column.ColumnName + "</Column>\r\n");

      page.Response.Write("<Row>\r\n");

      foreach (DataColumn column in table.Columns)
        page.Response.Write("<Cell ss:StyleID=\"s1\"><Data ss:Type=\"String\">" + column.ColumnName +
                            "</Data></Cell>\r\n");

      page.Response.Write("</Row>\r\n");
    }

    private void WriteTable(DataTable table)
    {
      page.Response.Write("<Worksheet ss:Name='" + table.TableName + "'>\r\n");
      page.Response.Write("<Table ss:ExpandedColumnCount=\"" + table.Columns.Count + "\" ss:ExpandedRowCount=\"" +
                          (table.Rows.Count + 1) + "\" x:FullColumns=\"1\" x:FullRows=\"1\">\r\n");
      WriteTableHeader(table);
      WriteTableRows(table);
      page.Response.Write("</Table>\r\n");
      page.Response.Write("</Worksheet>\r\n");
    }

    private void WriteTableRows(DataTable table)
    {
      foreach (DataRow Row in table.Rows)
        WriteTableRow(Row);
    }

    private bool IsNumber(string Value)
    {
      if (Value == "")
        return false;

      char[] chars = Value.ToCharArray();

      foreach (char ch in chars)
      {
        if (ch != '$' && ch != '.' && ch != ',' && !char.IsNumber(ch))
          return false;
      }

      return true;
    }

    private string GetExcelType(object Value)
    {
      if (Value == null || Value == DBNull.Value || Value is string)
        return "String";
        //			else if( Value is DateTime )
        //				return "Date";
      else if (IsNumber(Value.ToString()))
        return "Number";
      else
        return "String";
    }

    private void WriteTableRow(DataRow Row)
    {
      page.Response.Write("<Row>\r\n");

      foreach (object loop in Row.ItemArray)
      {
        page.Response.Write("<Cell><Data ss:Type=\"" + GetExcelType(loop) + "\">");

        if (loop != null && loop != DBNull.Value)
        {
          if (loop is byte[])
            page.Response.Write("(...)");
          else if (loop is decimal)
          {
            decimal decimalNumber = (decimal) loop;
            page.Response.Write(decimalNumber.ToString("N"));
          }
          else if (loop is DateTime)
          {
            page.Response.Write(((DateTime) loop).ToString("yyyy-MM-dd HH:mm:ss"));
          }
          else
          {
            page.Response.Write(HttpUtility.HtmlEncode(loop.ToString()));
          }
        }

        page.Response.Write("</Data></Cell>\r\n");
      }

      page.Response.Write("</Row>\r\n");
    }
  }
}