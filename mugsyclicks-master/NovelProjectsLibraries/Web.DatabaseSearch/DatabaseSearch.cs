using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System;
using System.Collections;

namespace NovelProjects.Web
{
	/// <summary>
	/// Summary description
	/// </summary>

	public class DataBaseSearch
	{
		private bool exactPhraseSearch;

		public DataBaseSearch()
		{
			exactPhraseSearch = false;
		}


		public static DataTable Search_UseStorProc(String searchQuery, String searchField, IDbConnection conn, String SP_name, bool exactPhraseSearch)
		{
			DataBaseSearch d = new DataBaseSearch();
			d.exactPhraseSearch = exactPhraseSearch;
			DataSet ds = d.ExecuteStoredProcedure(conn, SP_name);

			return d.SearchHelper(searchQuery, searchField, ds);
		}


		public static DataTable Search(String searchQuery, String searchField, IDbConnection conn, String sql, bool exactPhraseSearch)
		{
			DataBaseSearch d = new DataBaseSearch();
			d.exactPhraseSearch = exactPhraseSearch;
			DataSet ds = d.ExecuteSqlStatement(conn, sql);

			return d.SearchHelper(searchQuery, searchField, ds);
		}


		public static DataTable Search(String searchQuery, String searchField, IDbConnection conn, String sql)
		{
			DataBaseSearch d = new DataBaseSearch();
			d.exactPhraseSearch = false;
			DataSet ds = d.ExecuteSqlStatement(conn, sql);

			return d.SearchHelper(searchQuery, searchField, ds);
		}


		public static DataTable Search(String searchQuery, String searchField, DataSet ds)
		{
			DataBaseSearch d = new DataBaseSearch();
			return d.SearchHelper(searchQuery, searchField, ds);
		}


		public static DataTable Search(String searchQuery, String searchField, DataTable dt)
		{
			DataBaseSearch d = new DataBaseSearch();
			DataSet ds = new DataSet();

			ds.Tables.Add(dt);

			return d.SearchHelper(searchQuery, searchField, ds);
		}


		private DataSet ExecuteStoredProcedure(IDbConnection conn, String SP_name)
		{
			DataSet ds = new DataSet();

			try
			{
				conn.Open();

				if ("System.Data.OleDb.OleDbConnection".Equals(conn.GetType().ToString()))
				{
					OleDbCommand cmd = new OleDbCommand(SP_name, (OleDbConnection)conn);
					cmd.CommandType = CommandType.StoredProcedure;

					OleDbDataAdapter adapter = new OleDbDataAdapter();
					adapter.SelectCommand = cmd;
					adapter.Fill(ds);
				}
				else if ("System.Data.SqlClient.SqlConnection".Equals(conn.GetType().ToString()))
				{
					SqlCommand cmd = new SqlCommand(SP_name, (SqlConnection)conn);
					cmd.CommandType = CommandType.StoredProcedure;

					SqlDataAdapter adapter = new SqlDataAdapter();
					adapter.SelectCommand = cmd;
					adapter.Fill(ds);
				}
			}
			finally { if (conn != null) conn.Close(); }

			return ds;
		}


		private DataSet ExecuteSqlStatement(IDbConnection conn, String sql)
		{
			DataSet ds = new DataSet();

			try
			{
				conn.Open();

				IDbDataAdapter adapter = null;
				if ("System.Data.OleDb.OleDbConnection".Equals(conn.GetType().ToString()))
				{
					adapter = new OleDbDataAdapter(sql, (OleDbConnection)conn);
				}
				else if ("System.Data.SqlClient.SqlConnection".Equals(conn.GetType().ToString()))
				{
					adapter = new SqlDataAdapter(sql, (SqlConnection)conn);
				}

				if (adapter != null) adapter.Fill(ds);
			}
			finally { if (conn != null) conn.Close(); }

			return ds;
		}


		private DataTable SearchHelper(String searchQuery, String searchField, DataSet ds)
		{
			// Get fields to search in
			ArrayList searchFieldArray = new ArrayList();
			if ("all".Equals(searchField.ToLower()))
			{
				foreach (DataColumn col in ds.Tables[0].Columns)
				{
					if ("System.String".Equals(col.DataType.ToString()))
					{
						searchFieldArray.Add(col.ColumnName);
					}
				}

				if (ds.Tables[0].Columns.Contains("ID"))
				{
					searchFieldArray.Add("ID");
				}
			}
			else if (searchField.Contains(","))
			{
				string[] ary = searchField.Split(',');

				foreach (string t in ary)
				{
					searchFieldArray.Add(t.Trim());
				}

				if (ds.Tables[0].Columns.Contains("ID"))
				{
					searchFieldArray.Add("ID");
				}
			}
			else
			{
				searchFieldArray.Add(searchField);
			}

			String[] tempArray;

			searchQuery = searchQuery.Trim();

			if (!exactPhraseSearch)
			{
				// Get query words
				searchQuery = searchQuery.Replace("\"", "");     // Remove double quotes
				searchQuery = searchQuery.Replace("(", "");      // Remove left parenthesis
				searchQuery = searchQuery.Replace(")", "");      // Remove right parenthesis
				searchQuery = searchQuery.Replace(";", "");      // Remove semi-colons
				searchQuery = searchQuery.Replace("*", "");      // Remove asterisks
				searchQuery = searchQuery.Replace("'", "");      // Remove asterisks
				searchQuery = searchQuery.ToLower();            // Change case to lower

				tempArray = searchQuery.Split(' ');
			}
			else
			{
				tempArray = new[] { searchQuery };
			}

			ArrayList searchQueryArray = new ArrayList();
			foreach (String queryWord in tempArray)
			{
				if (!"".Equals(queryWord) && !"and".Equals(queryWord))
				{
					searchQueryArray.Add(queryWord);
				}
			}

			// Search data table and remove any rows where query is false
			//DataTable Table = CalendarSearch.doRecurrence(ds.Tables[0]);
			DataTable Table = ds.Tables[0];

			//foreach (DataRow row in Table.Rows)
			for (int i = Table.Rows.Count - 1; i >= 0; i--)
			{
				DataRow row = Table.Rows[i];
				int foundCount = 0;
				foreach (String queryWord in searchQueryArray)
				{
					foreach (String field in searchFieldArray)
					{
						String tt = row[field].ToString();

						if (!exactPhraseSearch)
						{
							tt = tt.Replace("'", "");
							tt = tt.Replace("\"", "");
							tt = tt.Replace("(", "");
							tt = tt.Replace(")", "");
							tt = tt.Replace(";", "");
							tt = tt.Replace("*", "");
							tt = tt.ToLower();
						}

						if (tt.IndexOf(queryWord) >= 0)
						{
							foundCount++;
							break;
						}
					}
				}
				if (foundCount != searchQueryArray.Count)
				{
					row.Delete();
				}
			}

			Table.AcceptChanges();

			return Table;
		}
	}
}