using System;
using System.Data;
using System.Text;
using System.Web.Services;
using System.Data.SqlClient;
using System.Configuration;

namespace NovelProjects.Web
{
	[WebService(Namespace = "http://www.novelprojects.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.Web.Script.Services.ScriptService]
	public class AutoCompleteService : WebService
	{
		#region Search
		[WebMethod]
    public string Search(string Query, string Table, string Display, string Search, string ConnString, string ApplicationId, string NoResultsMessage, string Filters)
		{
			DataTable dt = new DataTable();
			StringBuilder sb = new StringBuilder();

      if (string.IsNullOrEmpty(ConnString))
      {
        ConnString = ConfigurationManager.AppSettings["AutoCompleteConnectionString"];
      }

		  using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnString].ConnectionString))
			{
				SqlCommand sqlcmd = conn.CreateCommand();

				string select = "SELECT DISTINCT " + Search + " FROM " + Table + " WHERE";
				if (!string.IsNullOrEmpty(ApplicationId) && ApplicationId != Guid.Empty.ToString())
					select += " ApplicationID=@ApplicationId AND";

				// State=NC|City=Raleigh
				foreach (string Filter in Filters.Split('|'))
				{
					if (string.IsNullOrEmpty(Filter)) continue;

					string param = Filter.Substring(0, Filter.IndexOf('='));
					select += " " + param + "=@" + param + " AND";
					sqlcmd.Parameters.AddWithValue("@" + param, Filter.Substring(param.Length + 1));
				}

				if (select.EndsWith("AND"))
					select = select.Substring(0, select.Length - 3);
				if (select.EndsWith("WHERE"))
					select = select.Substring(0, select.Length - 5);

				sqlcmd.CommandText = select;
				sqlcmd.Parameters.AddWithValue("@ApplicationId", ApplicationId);


				SqlDataAdapter adapter = new SqlDataAdapter(sqlcmd);
				adapter.Fill(dt);

				dt = DataBaseSearch.Search(Query, "all", dt);

				foreach (DataRow dr in dt.Rows)
				{
					sb.Append(dr[Display] + "\n");
				}
				if (dt.Rows.Count == 0)
				{
          if (!string.IsNullOrEmpty(NoResultsMessage))
          {
            sb.Append(NoResultsMessage);
          }
          else
          {
            sb.Append("No match found.");
          }
				}
			}

			return sb.ToString();
		}
		#endregion
	}
}