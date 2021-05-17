using System;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web.UI;

namespace NovelProjects.Web
{
	[WebService(Namespace = "http://www.novelprojects.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.Web.Script.Services.ScriptService]
	public class SaveClicks : System.Web.Services.WebService
	{
		#region Save Click to database
		[WebMethod]
		public string SaveClick(int x, int y, int width, int height, string path)
		{
			HttpContext curr = HttpContext.Current;
			if (ConfigurationManager.AppSettings["HeatmapIPs"] != null && ConfigurationManager.AppSettings["HeatmapIPs"].ToString().Contains(curr.Request.UserHostAddress))
				return "Success";
			string conns = ConfigurationManager.AppSettings["HeatmapConn"] ?? ConfigurationManager.AppSettings["ControlsConn"];

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[conns].ConnectionString))
			{
				conn.Open();

				string Browser = curr.Request.Browser.Browser;
				if (Browser == "IE") Browser += " " + curr.Request.Browser.Version;

				SqlCommand sqlcmd = new SqlCommand("INSERT INTO ClickLog (X,Y,Browser,OS,IP,Width,Height,Path,ApplicationID) VALUES (@X,@Y,@Browser,@OS,@IP,@Width,@Height,@Path,@ApplicationID)", conn);
				sqlcmd.Parameters.AddWithValue("@X", x);
				sqlcmd.Parameters.AddWithValue("@Y", y);
				sqlcmd.Parameters.AddWithValue("@Browser", Browser);
				sqlcmd.Parameters.AddWithValue("@OS", curr.Request.Browser.Platform);
				sqlcmd.Parameters.AddWithValue("@IP", curr.Request.UserHostAddress);
				sqlcmd.Parameters.AddWithValue("@Width", width);
				sqlcmd.Parameters.AddWithValue("@Height", height);
				sqlcmd.Parameters.AddWithValue("@Path", (path != "") ? path : curr.Request.UrlReferrer.PathAndQuery);
				sqlcmd.Parameters.AddWithValue("@ApplicationID", ConfigurationManager.AppSettings["AppId"]);
				sqlcmd.ExecuteNonQuery();
			}

			return "Success";
		}
		#endregion

		#region Generate Heatmap from clicks
		[WebMethod]
		public string GenerateImage(string path, string browser, DateTime start, DateTime end, int width, int height, int sitewidth, bool center, bool query)
		{
			HttpContext curr = HttpContext.Current;
			DataTable Clicks = new DataTable();
			string unique = "";

			string conns = ConfigurationManager.AppSettings["HeatmapConn"] ?? ConfigurationManager.AppSettings["ControlsConn"];

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[conns].ConnectionString))
			{
				conn.Open();

				string IPs = "";
				string[] temp = (ConfigurationManager.AppSettings["HeatmapIPs"] ?? "").Split(new char[] { ',' });
				foreach (string s in temp) IPs += "'" + s + "',";
				if (IPs.IndexOf(",") >= 0)
					IPs = IPs.Remove(IPs.LastIndexOf(","));
				else IPs = "empty";

				string location = path != "" ? path : curr.Request.UrlReferrer.PathAndQuery;
				if (query && location.IndexOf("?") >= 0)
				{
					location = "%" + location.Remove(location.IndexOf("?")) + "%";
				}
				string querytext = " WHERE (Path=@Path OR Path=@Path2 OR Path=@Path3)";
				if (query) querytext = " WHERE (Path LIKE @Path OR Path LIKE @Path2 OR Path LIKE @Path3)";
				querytext += " AND (Browser LIKE @Browser OR OS LIKE @Browser) AND Date BETWEEN @Start AND @End AND ApplicationID=@ApplicationID AND IP NOT IN (" + IPs + ");";

				SqlCommand sqlcmd = new SqlCommand("SELECT * FROM ClickLog" + querytext, conn);
				sqlcmd.Parameters.AddWithValue("@Path", location);
				sqlcmd.Parameters.AddWithValue("@Path2", location + "index.aspx");
				sqlcmd.Parameters.AddWithValue("@Path3", location.Replace("index.aspx", ""));
				sqlcmd.Parameters.AddWithValue("@Browser", "%" + (browser=="Show All" ? "" : browser) + "%");
				sqlcmd.Parameters.AddWithValue("@Start", start);
				sqlcmd.Parameters.AddWithValue("@End", end.AddDays(1));
				sqlcmd.Parameters.AddWithValue("@ApplicationID", ConfigurationManager.AppSettings["AppId"]);
				SqlDataAdapter adapter = new SqlDataAdapter(sqlcmd);
				adapter.Fill(Clicks);

				sqlcmd.CommandText = "SELECT COUNT(DISTINCT IP) FROM ClickLog" + querytext;
				unique += sqlcmd.ExecuteScalar();
			}

			// Create canvas the size of the page
			using (Image canvas = new Bitmap(width > sitewidth ? width : sitewidth, height))
			{
				// Load the dot-Image
				Image pt = Image.FromFile(curr.Server.MapPath(curr.Application["ROOTPATH"] + "images/heatdot.png"));

				// Initialize Graphics object to work on the canvas
				Graphics g = Graphics.FromImage(canvas);
				g.Clear(Color.White);

				//Does math on total number of clicks to make heatmap relative
				ColorMatrix rcam = new ColorMatrix();
				//if (Clicks.Rows.Count > 255)
				//  rcam.Matrix33 = (255 / (float)Clicks.Rows.Count);
				//else rcam.Matrix33 = 1;
				rcam.Matrix33 = 1;
				ImageAttributes imgAttr = new ImageAttributes();
				imgAttr.SetColorMatrix(rcam, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

				foreach (DataRow dr in Clicks.Rows)
				{
					//If the site is centered need to make adjustments to align the points
					int pw = Convert.ToInt32(dr["Width"]);
					int px = Convert.ToInt32(dr["X"]);
					int py = Convert.ToInt32(dr["Y"]);

					int OriginalLeftAdjustment = ((pw > sitewidth ? pw : sitewidth) - sitewidth);
					if (center) OriginalLeftAdjustment /= 2;
					int CurrentLeftAdjustment = ((width > sitewidth ? width : sitewidth) - sitewidth);
					if (center) CurrentLeftAdjustment /= 2;
					px = px - OriginalLeftAdjustment + CurrentLeftAdjustment;

					g.DrawImage(pt, new Rectangle(px - pt.Width / 2, py - pt.Height / 2, pt.Width, pt.Height), 0, 0, pt.Width, pt.Height, GraphicsUnit.Pixel, imgAttr);
					//g.DrawImage(pt, px - pt.Width / 2, py - pt.Height / 2);
				}

				imgAttr = new ImageAttributes();
				ColorMap[] remapTable = new ColorMap[255];

				// Replace Color for all color-codes from 0,0,0 to 75, 75, 75 (RGB) (From black to dark-gray)
				for (int i = 0; i < 75; i++)
				{
					ColorMap c = new ColorMap();
					c.OldColor = Color.FromArgb(i, i, i);
					c.NewColor = Color.FromArgb(255 - i, 0, 0);
					remapTable[i] = c;
				}
				// Replace Color for all color-codes from 75, 75, 75 to 200, 200, 200 (RGB) (From dark-gray to gray)
				for (int i = 75; i < 200; i++)
				{
					ColorMap c = new ColorMap();
					c.OldColor = Color.FromArgb(i, i, i);
					c.NewColor = Color.FromArgb(0, 255 - i, 0);
					remapTable[i] = c;
				}
				// Replace Color for all color-codes from 200, 200, 200 to 255, 255, 255 (RGB) (From gray to light-gray - before it gets white!)
				for (int i = 200; i < 255; i++)
				{
					ColorMap c = new ColorMap();
					c.OldColor = Color.FromArgb(i, i, i);
					c.NewColor = Color.FromArgb(0, 0, i - 100);
					remapTable[i] = c;
				}
				// Set the RemapTable on the ImageAttributes object.
				// Draw Image with the new ImageAttributes (changes colors to heatmap)
				imgAttr.SetRemapTable(remapTable, ColorAdjustType.Bitmap);
				g.DrawImage(canvas, new Rectangle(0, 0, canvas.Width, canvas.Height), 0, 0, canvas.Width, canvas.Height, GraphicsUnit.Pixel, imgAttr);

				//Used to change background color for transparency
				imgAttr = new ImageAttributes();
				ColorMap[] cm = new ColorMap[1];
				ColorMap cw = new ColorMap();
				cw.OldColor = Color.White;
				cw.NewColor = Color.Black;
				cm[0] = cw;

				// Set the RemapTable on the new ImageAttributes object.
				imgAttr.SetRemapTable(cm, ColorAdjustType.Bitmap);
				g.DrawImage(canvas, new Rectangle(0, 0, canvas.Width, canvas.Height), 0, 0, canvas.Width, canvas.Height, GraphicsUnit.Pixel, imgAttr);

				// Setting transparency! Create a new color matrix and set the alpha value to 0.5
				ColorMatrix cam = new ColorMatrix();
				cam.Matrix33 = .7F;

				imgAttr = new ImageAttributes();
				imgAttr.SetColorMatrix(cam, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

				// Draw the final image with the image attributes specified
				using (Bitmap final = new Bitmap(canvas.Width, canvas.Height))
				{
					g = Graphics.FromImage(final);
					g.DrawImage(canvas, new Rectangle(0, 0, canvas.Width, canvas.Height), 0, 0, canvas.Width, canvas.Height, GraphicsUnit.Pixel, imgAttr);

					if (!Directory.Exists(curr.Server.MapPath("~/utils")))
						Directory.CreateDirectory(curr.Server.MapPath("~/utils"));
					final.Save(curr.Server.MapPath("~/") + "utils/heatmap.png", ImageFormat.Png);
				}

				g.Dispose();
			}

			return "{ \"Clicks\":\"" + Clicks.Rows.Count + "\", \"Unique\":\"" + unique + "\" }";
		}
		#endregion
	}
}