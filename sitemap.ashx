<%@ WebHandler Language="C#" Class="HerbertMilhomme.sitemap" %>
//<script runat="server">

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Xml;

namespace HerbertMilhomme
{
    
    public class sitemap : IHttpHandler
    {//using System.Data.SqlServerCe;

        public void ProcessRequest(HttpContext context) {
            context.Response.ContentType = "text/xml";
            using (XmlTextWriter writer = new XmlTextWriter(context.Response.OutputStream, Encoding.UTF8)) {
                writer.WriteStartDocument();
                writer.WriteStartElement("urlset");
                writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
                //writer.WriteStartElement("url");

				//command.Parameters.Add(new System.Data.SqlServerCe.SqlCeParameter("SessionID", Session["SESSION_KEY"]));
				//command.ExecuteNonQuery();}}}
				string connect = "Data Source=mssql.webmatrix-appliedi.net;Initial Catalog=HerbServer1;User ID=HMServer;Password=HMadmin1"; //ConfigurationManager.ConnectionStrings["ColonielHeights"].ConnectionString;
				//string connectionString = @"Data Source=|DataDirectory|\ColonielHeights.sdf";
                string url = "herbertmilhomme.com";
                using (SqlConnection conn = new SqlConnection(connect)) {
				//using (System.Data.SqlServerCe.SqlCeConnection connection = new  System.Data.SqlServerCe.SqlCeConnection(connectionString)){
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM website_pages LEFT JOIN website_sitemap-freq ON website_pages.frequency=website_sitemap-freq.freqid WHERE filename IS NOT NULL", conn)) {
					//using (System.Data.SqlServerCe.SqlCeCommand command = new System.Data.SqlServerCe.SqlCeCommand("SELECT * FROM", connection)){
						//SELECT TOP 1 DateCreated FROM Articles ORDER BY ArticleID DESC 
						//SELECT ArticleID, DateAmended FROM Articles ORDER BY ArticleID DESC 
						//SELECT ArticleTypeID FROM ArticleTypes 
						//SELECT CategoryID FROM Categories
                        cmd.CommandType = CommandType.Text; //StoredProcedure;
						//command.CommandType = CommandType.StoredProcedure;
                        conn.Open();
						//connection.Open();
                        using (SqlDataReader rdr = cmd.ExecuteReader()) {
                        //using (SqlCeDataReader rdr = command.ExecuteReader()) {
							// Make a reference to a directory.
							//System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(@"./");
							// Get a reference to each directory in that directory.
							//di.EnumerateFiles("*", System.IO.SearchOption.AllDirectories);
                            // Get the date of the most recent article
                            while (rdr.Read()) {
							//System.IO.FileInfo file = new System.IO.FileInfo((string)rdr["urlpath"]+(string)rdr["filename"]+(string)rdr["fileformat"]);
							//System.IO.DirectoryInfo file = new System.IO.DirectoryInfo((string)rdr["urlpath"]);
							//file.Refresh();
							writer.WriteStartElement("url");
                            writer.WriteElementString("loc", string.Format("{0}", "http://"+rdr["hostname"]+url+rdr["urlpath"]+rdr["filename"]));
                            writer.WriteElementString("lastmod", string.Format("{0:yyyy-MM-dd'T'HH:mm:ff}", rdr["lastmodified"]));
                            writer.WriteElementString("changefreq", string.Format("{0}", rdr["frequency"]));
                            writer.WriteElementString("priority", string.Format("{0}", rdr["priority"]));
							//writer.WriteStartElement("xhtml:link");
							//writer.WriteAttributeString("rel", "alternate");
							//writer.WriteAttributeString("hreflang", "en");
							//<xhtml:link rel="alternate" media="only screen and (max-width: 640px)" href="http://www.autoquoter.com/aq/en/Mobile" />
							//<xhtml:link rel="alternate" hreflang="es" href="http://www.autoquoter.com/aq/es/Home" />
                            writer.WriteEndElement();}
							/*
                            // Move to the Article IDs
                            rdr.NextResult();
                            while (rdr.Read()) {
                                writer.WriteStartElement("url");
                                writer.WriteElementString("loc", string.Format("{0}Article.aspx?ArticleID={1}", url,  rdr[0]));

                                //if (rdr[1] != DBNull.Value)
                                //    writer.WriteElementString("lastmod", string.Format("{0:yyyy-MM-dd'T'HH:mm:ff}", rdr[1]));
                                writer.WriteElementString("changefreq", "monthly");
                                writer.WriteElementString("priority", "0.5");
                                writer.WriteEndElement();
                            }
                            // Move to the Article Type IDs
                            rdr.NextResult();
                            while (rdr.Read()) {
                                writer.WriteStartElement("url");
                                writer.WriteElementString("loc", string.Format("{0}ArticleTypes.aspx?Type={1}", url, rdr[0]));
                                writer.WriteElementString("priority", "0.5");
                                writer.WriteEndElement();
                            }
                            // Finally move to the Category IDs
                            rdr.NextResult();
                            while (rdr.Read()) {
                                writer.WriteStartElement("url");
                                writer.WriteElementString("loc", string.Format("{0}Category.aspx?Category={1}", url, rdr[0]));
                                writer.WriteElementString("priority", "0.5");
                                writer.WriteEndElement();
                            }*/
                            writer.WriteEndElement();
                            writer.WriteEndDocument();
                            writer.Flush();
                        }
                        context.Response.End();
                    }
                }
            }
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}       
//</script>