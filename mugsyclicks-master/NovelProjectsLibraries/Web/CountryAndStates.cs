using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace NovelProjects.Web
{
  #region Country And States
  public class CountryAndStates
  {
    #region Private Variables
    private static string DB
    {
      get
      {
        return string.IsNullOrEmpty(ConfigurationManager.AppSettings["CountryAndStatesConnectionString"])
                 ? @"Server=unreal.novelprojects.com\sql2005; Initial Catalog=np_global; User ID=np_global;Password=08novel08;"
                 : ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["CountryAndStatesConnectionString"]].ConnectionString;
      }
    }

    //private static string DB = @"Server=unreal.novelprojects.com\sql2005; Initial Catalog=np_global; User ID=np_global;Password=08novel08;";
    #endregion

    #region Country Lookup
    public static string CountryLookup(string country_name)
    {
      SqlConnection conn = new SqlConnection(DB);
      string country = null;

      try
      {
        conn.Open();

        SqlCommand sql = new SqlCommand("SELECT Country FROM Countries WHERE Name=@Name", conn);
        sql.Parameters.Add("@Name", SqlDbType.VarChar);
        sql.Parameters["@Name"].Value = country_name;

        country = sql.ExecuteScalar().ToString();
      }
      finally { if (conn != null) conn.Close(); }

      return country;
		}

		public static string CountryNameLookup(string abbr)
		{
			SqlConnection conn = new SqlConnection(DB);
			string country = null;

			try
			{
				conn.Open();

				SqlCommand sql = new SqlCommand("SELECT Name FROM Countries WHERE Country=@Country", conn);
				sql.Parameters.Add("@Country", SqlDbType.VarChar);
				sql.Parameters["@Country"].Value = abbr;

				country = sql.ExecuteScalar().ToString();
			}
			finally { if (conn != null) conn.Close(); }

			return country;
		}
		#endregion

    #region Get Countries

		[Obsolete("This method has been deprecated. Use GetCountryList().")]
    public static DataTable GetCountries()
    {
      SqlConnection conn = new SqlConnection(DB);
      DataTable dt = new DataTable();

      try
      {
        conn.Open();

        SqlCommand sql = new SqlCommand("SELECT Country, Name FROM Countries ORDER BY OrderNum", conn);
        SqlDataAdapter adapter = new SqlDataAdapter(sql);
        adapter.Fill(dt);
      }
      finally { conn.Close(); }

      return dt;
    }

		public static List<Country> GetCountryList()
		{
			List<Country> countryList = new List<Country>();

			try
			{
				using (SqlConnection conn = new SqlConnection(DB))
				{
					conn.Open();

					SqlCommand sqlcmd = new SqlCommand("SELECT ID, Country, Name, OrderNum FROM Countries ORDER BY OrderNum", conn);

					using (SqlDataReader reader = sqlcmd.ExecuteReader())
					{
						if (reader == null || !reader.HasRows)
						{
							return countryList;
						}

						while (reader.Read())
						{
							Country country = new Country
																	{
																		Id = Convert.ToInt32(reader["ID"]),
																		CountryCode = reader["Country"].ToString(),
																		Name = reader["Name"].ToString(),
																		OrderNum = Convert.ToInt32(reader["OrderNum"])
																	};

							countryList.Add(country);
						}
					}
				}
			}
			catch (Exception)
			{
				
				throw;
			}

			return countryList;
		}

    #endregion

    #region Get States

		[Obsolete("This method has been deprecated. Use GetStateList(String countryCode).")]
    public static DataTable GetStates(string sel_country)
    {
      SqlConnection conn = new SqlConnection(DB);
      DataTable dt = new DataTable();

      try
      {
        conn.Open();

        SqlCommand sql = new SqlCommand("SELECT Countries.Country, States.State, States.Name FROM States JOIN Countries ON States.Country=Countries.ID WHERE Countries.Country=@Country ORDER BY Name", conn);
        sql.Parameters.Add("@Country", SqlDbType.VarChar);
        sql.Parameters["@Country"].Value = sel_country;
        SqlDataAdapter adapter = new SqlDataAdapter(sql);
        adapter.Fill(dt);
      }
      finally { conn.Close(); }

      return dt;
		}

		public static List<State> GetStateList(String countryCode)
		{
			List<State> stateList = new List<State>();

			try
			{
				using (SqlConnection conn = new SqlConnection(DB))
				{
					conn.Open();

					SqlCommand sqlcmd = new SqlCommand("SELECT States.ID, States.Country, States.State, States.Name FROM States JOIN Countries ON States.Country=Countries.ID WHERE Countries.Country=@Country ORDER BY Name", conn);

					sqlcmd.Parameters.Add("@Country", SqlDbType.VarChar);
					sqlcmd.Parameters["@Country"].Value = countryCode;

					using (SqlDataReader reader = sqlcmd.ExecuteReader())
					{
						if (reader == null || !reader.HasRows)
						{
							return stateList;
						}

						while (reader.Read())
						{
							State state = new State
							{
								Id = Convert.ToInt32(reader["ID"]),
								Country = Convert.ToInt32(reader["Country"]),
								StateCode = reader["State"].ToString(),
								Name = reader["Name"].ToString()
							};

							stateList.Add(state);
						}
					}
				}
			}
			catch (Exception)
			{

				throw;
			}

			return stateList;
		}

    #endregion

    #region Code to set/update country order number
    private static void SetCountryOrderNum()
    {
      return;

      SqlConnection conn = new SqlConnection(DB);
      DataTable dt = new DataTable();

      try
      {
        conn.Open();

        SqlCommand sql;

        sql = new SqlCommand("" +
          "UPDATE Countries SET OrderNum=1 WHERE Country='US'; " +
          "UPDATE Countries SET OrderNum=2 WHERE Country='CA';" +
          "UPDATE Countries SET OrderNum=3 WHERE Country='AA';", conn);

        sql.ExecuteNonQuery();

        sql = new SqlCommand("SELECT * FROM Countries WHERE OrderNum IS NULL ORDER BY Name;", conn);

        SqlDataAdapter adapter = new SqlDataAdapter(sql);
        adapter.Fill(dt);


        int ordernum = 4;

        foreach (DataRow dr in dt.Rows)
        {
          sql = new SqlCommand("UPDATE Countries SET OrderNum=@OrderNum WHERE Country=@Country;", conn);
          sql.Parameters.AddWithValue("@OrderNum", ordernum);
          sql.Parameters.AddWithValue("@Country", dr["Country"].ToString());
          sql.ExecuteNonQuery();

          ordernum++;
        }
      }
      finally { if (conn != null) conn.Close(); }
    }
    #endregion
  }
  #endregion

	public class Country
	{
		public Int32 Id { get; set; }
		public String CountryCode { get; set; }
		public String Name { get; set; }
		public Int32 OrderNum { get; set; }
	}

	public class State
	{
		public Int32 Id { get; set; }
		public Int32 Country { get; set; }
		public String StateCode { get; set; }
		public String Name { get; set; }
	}
}
