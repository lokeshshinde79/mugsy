using System;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;

namespace NovelProjects.Web.Services
{
  #region Taxes Class
  public class Taxes
  {
    #region Get Tax info from DataBase
    public static TaxRate GetTaxes(TaxRate TaxRate)
    {
      try
      {
        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Taxes"].ConnectionString))
        {
          conn.Open();

          SqlCommand sql = new SqlCommand("SELECT SalesTax, TAX_SHIPPING_ALONE, TAX_SHIPPING_AND_HANDLING_TOGETHER FROM Taxes WHERE ZipCode=@ZipCode", conn);
          sql.Parameters.Add("@ZipCode", SqlDbType.VarChar);
          sql.Parameters["@ZipCode"].Value = TaxRate.ZipCode;

          using (SqlDataReader reader = sql.ExecuteReader())
          {
            // if we find the zipcode in the database
            if (reader != null && reader.HasRows)
            {
              reader.Read();
              TaxRate.Rate = reader.GetDecimal(0);
              TaxRate.TAX_SHIPPING_ALONE = reader.GetBoolean(1);
              TaxRate.TAX_SHIPPING_AND_HANDLING_TOGETHER = reader.GetBoolean(2);
            }
          }
        }
      }
      catch (Exception e)
      {
        // send error email
        SendErrorEmail.Send(e);
      }

      return TaxRate;
    }
    #endregion
  }
  #endregion

  #region TaxRate Class
  public class TaxRate
  {
    public string ZipCode;
    public decimal SubTotal;
    public decimal Shipping;
    public decimal Handling;

    public decimal Rate;
    public decimal Tax;
    public decimal SubTotalTax;
    public decimal ShippingTax;
    public decimal HandlingTax;

    public bool TAX_SHIPPING_ALONE;
    public bool TAX_SHIPPING_AND_HANDLING_TOGETHER;

    public decimal Total;

    public string ErrorMsg;

    public TaxRate()
    {
      Rate = 0;
      TAX_SHIPPING_ALONE = true;
      TAX_SHIPPING_AND_HANDLING_TOGETHER = true;
    }

    public TaxRate(string error)
    {
      ErrorMsg = error;
    }
  }
  #endregion
}
