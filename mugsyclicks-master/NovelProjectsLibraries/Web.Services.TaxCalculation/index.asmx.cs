#region

using System;
using System.ComponentModel;
using System.Web.Services;
using System.Web.Services.Protocols;

#endregion

namespace NovelProjects.Web.Services
{
  /// <summary>
  /// Summary description for NovelProjects.Web.Services
  /// </summary>
  [WebService(Namespace = "http://www.novelprojects.com/",
    Description = "This is a webservice to calculate sales tax by zip code.")]
  [WebServiceBinding(ConformsTo = WsiProfiles.None)]
  [ToolboxItem(false)]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
  public class TaxCalculation : WebService
  {
    public AuthHeader AuthCred;
    private TaxRate rate;

    public TaxCalculation()
    {
      rate = new TaxRate();
    }

    #region Tax Code Logic

    /// 
    /// Sales tax is calculated at the time of checkout based upon the shipping destination of your order.
	/// (see http://www.dornc.com/faq/use.html, http://www.amazon.com/gp/help/customer/display.html?nodeId=468512, http://support.quickbooks.intuit.com/support/pages/knowledgebasearticle/1009634)
    /// No sales tax is charged on the purchase of Gift Certificates or Gift Cards.
    /// 
    /// if the item purchased has the shipping cost builtin to the price and the item is taxable
    ///  then the whole amount (of the item) is taxable
    /// 
    /// 
    /// if TAX_SHIPPING_AND_HANDLING_TOGETHER is false and TAX_SHIPPING_ALONE is false (ie. CO - 23 states/regions in all as of 9/13/09)
    ///   don't charge taxes on shipping or handling
    /// 
    /// if TAX_SHIPPING_AND_HANDLING_TOGETHER is false and TAX_SHIPPING_ALONE is true
    ///   don't think this case will ever happen (atleast that's the case with the current data from all states)
    /// 
    /// if TAX_SHIPPING_AND_HANDLING_TOGETHER is true and TAX_SHIPPING_ALONE is false (ie. CA - 12 states/regions in all as of 9/13/09)
    ///  if the items purchased are taxable
    ///    handling charges are taxable (if they are seperate from the shipping)
    ///    shipping charges are EXEMPT if
    ///     - The shipping charge does not exceed the actual cost of shipping.
    ///     - The US Postal Service, a common carrier such as UPS or an independent contractor is used to deliver the item directly to you as the customer.
    ///     - The invoice clearly uses terms such as "shipping," "delivery," or "postage" as a charge separate from the items sold.
    /// 
    /// if TAX_SHIPPING_AND_HANDLING_TOGETHER is true and TAX_SHIPPING_ALONE is true (ie. NC - 27 states/regions in all as of 9/13/09)
    ///  shipping is always taxable
    ///  if the items purchased are taxable
    ///    handling charges are taxable (if they are seperate from the shipping)
    /// 

    #endregion

    #region Test Method (Requires no Authentication Header)

    [WebMethod(
      Description =
        "Calculates sales tax using the shipping zip code, taxable total, and shipping and handling.  Use this method if shipping and handling are a combined charge."
      )]
    public TaxRate GetTaxesTest(string ZipCode, double TaxableTotal, double ShippingAndHandling)
    {
      rate.ZipCode = ZipCode;
      rate.SubTotal = Convert.ToDecimal(TaxableTotal);
      rate.Shipping = Convert.ToDecimal(ShippingAndHandling);


      rate = Taxes.GetTaxes(rate);



      /// if TAX_SHIPPING_AND_HANDLING_TOGETHER is false and TAX_SHIPPING_ALONE is false
      if (!rate.TAX_SHIPPING_AND_HANDLING_TOGETHER && !rate.TAX_SHIPPING_ALONE)
      {
        rate.ShippingTax = 0;
      }
      /// if TAX_SHIPPING_AND_HANDLING_TOGETHER is true and TAX_SHIPPING_ALONE is false
      else if (rate.TAX_SHIPPING_AND_HANDLING_TOGETHER && !rate.TAX_SHIPPING_ALONE)
      {
        // don't charge handling b/c it's built in to the shipping cost
        // shipping will almost always be exempt under this condition
        rate.ShippingTax = 0;
      }
      /// if TAX_SHIPPING_AND_HANDLING_TOGETHER is true and TAX_SHIPPING_ALONE is true
      else if (rate.TAX_SHIPPING_AND_HANDLING_TOGETHER && rate.TAX_SHIPPING_ALONE)
      {
        // shipping will always be taxable (handling is taxable b/c it's built into the shipping)
        rate.ShippingTax = Math.Round(rate.Rate * rate.Shipping, 2);
      }


      rate.SubTotalTax = Math.Round(rate.Rate * rate.SubTotal, 2);
      rate.Tax = rate.SubTotalTax + rate.ShippingTax;

      rate.Total = rate.SubTotal + rate.Shipping + rate.Tax;

      return rate;
    }

    #endregion

    #region Get Taxes

    [WebMethod(MessageName = "GetTaxes", Description = "Calculates sales tax using the shipping zip code and taxable total.")]
    [SoapHeader("AuthCred")]
    public TaxRate GetTaxes(string ZipCode, double TaxableTotal)
    {
      if (AuthCred == null)
      {
        return new TaxRate("ERROR: Please supply credentials.");
      }
      if (!Authentication.AuthenticateUser(AuthCred.Username, AuthCred.PasswordHash))
      {
        return new TaxRate("ERROR: Invalid login.");
      }

      rate.ZipCode = ZipCode;
      rate.SubTotal = Convert.ToDecimal(TaxableTotal);

      rate = Taxes.GetTaxes(rate);

      rate.SubTotalTax = Math.Round(rate.Rate*rate.SubTotal, 2);
      rate.Tax = rate.SubTotalTax;
      rate.Total = rate.SubTotal + rate.Tax;

      return rate;
    }

    #endregion

    #region Get Taxes With Combined Shipping And Handling

    /*
     * ShippingAndHandling should be treated as if were just Shipping
     */

    [WebMethod(
      Description =
        "Calculates sales tax using the shipping zip code, taxable total, and shipping and handling.  Use this method if shipping and handling are a combined charge."
      )]
    [SoapHeader("AuthCred")]
    public TaxRate GetTaxesWithCombinedShippingAndHandling(string ZipCode, double TaxableTotal, double ShippingAndHandling)
    {
      if (AuthCred == null)
      {
        return new TaxRate("ERROR: Please supply credentials.");
      }
      if (!Authentication.AuthenticateUser(AuthCred.Username, AuthCred.PasswordHash))
      {
        return new TaxRate("ERROR: Invalid login.");
      }

      rate.ZipCode = ZipCode;
      rate.SubTotal = Convert.ToDecimal(TaxableTotal);
      rate.Shipping = Convert.ToDecimal(ShippingAndHandling);


      rate = Taxes.GetTaxes(rate);



      /// if TAX_SHIPPING_AND_HANDLING_TOGETHER is false and TAX_SHIPPING_ALONE is false
      if (!rate.TAX_SHIPPING_AND_HANDLING_TOGETHER && !rate.TAX_SHIPPING_ALONE)
      {
        rate.ShippingTax = 0;
      }
      /// if TAX_SHIPPING_AND_HANDLING_TOGETHER is true and TAX_SHIPPING_ALONE is false
      else if (rate.TAX_SHIPPING_AND_HANDLING_TOGETHER && !rate.TAX_SHIPPING_ALONE)
      {
        // don't charge handling b/c it's built in to the shipping cost
        // shipping will almost always be exempt under this condition
        rate.ShippingTax = 0;
      }
      /// if TAX_SHIPPING_AND_HANDLING_TOGETHER is true and TAX_SHIPPING_ALONE is true
      else if (rate.TAX_SHIPPING_AND_HANDLING_TOGETHER && rate.TAX_SHIPPING_ALONE)
      {
        // shipping will always be taxable (handling is taxable b/c it's built into the shipping)
        rate.ShippingTax = Math.Round(rate.Rate * rate.Shipping, 2);
      }
      

      rate.SubTotalTax = Math.Round(rate.Rate*rate.SubTotal, 2);
      rate.Tax = rate.SubTotalTax + rate.ShippingTax;

      rate.Total = rate.SubTotal + rate.Shipping + rate.Tax;

      return rate;
    }

    #endregion

    #region Get Taxes With Shipping And Handling

    [WebMethod(
      Description =
        "Calculates sales tax using the shipping zip code, taxable total, shipping, and handling. Use this method if shipping and handling are seperate charges.  Each State/Zipcode determines how shipping and handling as seperate charges will be taxed."
      )]
    [SoapHeader("AuthCred")]
    public TaxRate GetTaxesWithShippingAndHandling(string ZipCode, double TaxableTotal, double Shipping, double Handling)
    {
      if (AuthCred == null)
      {
        return new TaxRate("ERROR: Please supply credentials.");
      }
      if (!Authentication.AuthenticateUser(AuthCred.Username, AuthCred.PasswordHash))
      {
        return new TaxRate("ERROR: Invalid login.");
      }

      rate.ZipCode = ZipCode;
      rate.SubTotal = Convert.ToDecimal(TaxableTotal);
      rate.Shipping = Convert.ToDecimal(Shipping);
      rate.Handling = Convert.ToDecimal(Handling);


      rate = Taxes.GetTaxes(rate);



      /// if TAX_SHIPPING_AND_HANDLING_TOGETHER is false and TAX_SHIPPING_ALONE is false
      if (!rate.TAX_SHIPPING_AND_HANDLING_TOGETHER && !rate.TAX_SHIPPING_ALONE)
      {
        rate.ShippingTax = 0;
      }
      /// if TAX_SHIPPING_AND_HANDLING_TOGETHER is true and TAX_SHIPPING_ALONE is false
      else if (rate.TAX_SHIPPING_AND_HANDLING_TOGETHER && !rate.TAX_SHIPPING_ALONE)
      {
        //  if the items purchased are taxable, handling charges are taxable
        if (TaxableTotal > 0)
        {
          rate.HandlingTax = Math.Round(rate.Rate * rate.Handling, 2);
        }
        
        // shipping will almost always be exempt under this condition
        rate.ShippingTax = 0;
      }
      /// if TAX_SHIPPING_AND_HANDLING_TOGETHER is true and TAX_SHIPPING_ALONE is true
      else if (rate.TAX_SHIPPING_AND_HANDLING_TOGETHER && rate.TAX_SHIPPING_ALONE)
      {
        //  if the items purchased are taxable, handling charges are taxable
        if (TaxableTotal > 0)
        {
          rate.HandlingTax = Math.Round(rate.Rate * rate.Handling, 2);
        }
        
        // shipping will always be taxable
        rate.ShippingTax = Math.Round(rate.Rate * rate.Shipping, 2);
      }
      
      #region old
      //// if TaxShippingAlone is false and TaxShippingAndHandlingTogether is false
      ////   don't charge taxes on shipping or handling


      //// if TaxShippingAndHandlingTogether is true and TaxShippingAlone is false
      ////    tax should only be applied to the handling fees
      //// if TaxShippingAndHandlingTogether is false and TaxShippingAlone is true
      ////    then don't charge tax on the Handling
      
      
      //// else if TaxShippingAlone is true and TaxShippingAndHandlingTogether is false
      ////   then don't charge tax on the Handling
      //// else 
      ////   charge taxes on shipping and handling
      //if (!rate.TAX_SHIPPING_ALONE && !rate.TAX_SHIPPING_AND_HANDLING_TOGETHER)
      //{
      //  // don't charge taxes on shipping or handling
      //}
      //else if (rate.TAX_SHIPPING_ALONE && !rate.TAX_SHIPPING_AND_HANDLING_TOGETHER)
      //{
      //  if (rate.Shipping != 0)
      //  {
      //    rate.ShippingTax = Math.Round(rate.Rate*rate.Shipping, 2);
      //  }
      //}
      //else
      //{
      //  if (rate.Handling != 0)
      //  {
      //    rate.HandlingTax = Math.Round(rate.Rate*rate.Handling, 2);
      //  }
      //  if (rate.Shipping != 0)
      //  {
      //    rate.ShippingTax = Math.Round(rate.Rate*rate.Shipping, 2);
      //  }
      //}


      ////if (rate.Handling != 0)
      ////{
      ////  // if TaxShippingAlone is true and TaxShippingAndHandlingTogether is false

      ////  if ((!rate.TAX_SHIPPING_ALONE && rate.TAX_SHIPPING_AND_HANDLING_TOGETHER) || (rate.TAX_SHIPPING_ALONE && rate.TAX_SHIPPING_AND_HANDLING_TOGETHER))
      ////  {
      ////    rate.HandlingTax = Math.Round(rate.Rate * rate.Handling, 2);
      ////  }
      ////}

      ////if (rate.Shipping != 0)
      ////{
      ////  if ((rate.TAX_SHIPPING_ALONE && !rate.TAX_SHIPPING_AND_HANDLING_TOGETHER) || (rate.TAX_SHIPPING_ALONE && rate.TAX_SHIPPING_AND_HANDLING_TOGETHER))
      ////  {
      ////    rate.ShippingTax = Math.Round(rate.Rate * rate.Shipping, 2);
      ////  }
      ////}
      #endregion

      rate.SubTotalTax = Math.Round(rate.Rate*rate.SubTotal, 2);
      rate.Tax = rate.SubTotalTax + rate.ShippingTax + rate.HandlingTax;

      rate.Total = rate.SubTotal + rate.Shipping + rate.Handling + rate.Tax;

      return rate;
    }

    #endregion
  }
}