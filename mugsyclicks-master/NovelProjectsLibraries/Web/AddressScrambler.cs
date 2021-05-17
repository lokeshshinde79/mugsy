using System;

namespace NovelProjects.Web
{
  #region AddressScrambler Class
  public class AddressScrambler
  {
    public static String Encode(String address)
    {
      String encoded = "";
      int offset = 7;
      for (int i = 0; i < address.Length; i++)
      {
        encoded += Convert.ToInt32(address[i] + offset);
        if (i < address.Length - 1) encoded += ",";
      }
      return encoded;
    }
  }
  #endregion
}
