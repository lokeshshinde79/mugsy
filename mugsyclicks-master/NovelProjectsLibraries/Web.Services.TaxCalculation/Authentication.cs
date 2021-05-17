#region

using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services.Protocols;

#endregion

namespace NovelProjects.Web.Services
{
  public class AuthHeader : SoapHeader
  {
    public string Username;
    public string PasswordHash;
  }

  public class Authentication
  {
    public static bool AuthenticateUser(string Username, string PasswordHash)
    {
      string _pwd = "";

      try
      {
        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Taxes"].ConnectionString))
        {
          conn.Open();

          SqlCommand sql = new SqlCommand("SELECT Password FROM WebserviceAccounts WHERE Username=@Username", conn);
          sql.Parameters.Add("@Username", SqlDbType.VarChar);
          sql.Parameters["@Username"].Value = Username;

          using (SqlDataReader reader = sql.ExecuteReader())
          {
            if (reader.HasRows)
            {
              reader.Read();
              _pwd = reader["Password"].ToString();
            }
          }
        }
      }
      catch
      {
      }

      return Encryption.Utilities.VerifyMd5Hash(_pwd, PasswordHash);
    }

  }
}