using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication1
{
    public partial class WebForm3 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Links.Visible = false;
        }

        protected void UserSubmit_Click(object sender, EventArgs e)
        { 
            if(Request.Form["UserText"].Length <= 0)
            {
                ErrorMessage.InnerText = "Please enter a username";
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = " IF NOT EXISTS (SELECT id FROM dbo.Users WHERE username = @username) "+
                                    " INSERT INTO dbo.Users (username) VALUES (@username)";

                conn.Open();

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", Request.Form["UserText"]);
                
                cmd.ExecuteNonQuery();

                
                query = "SELECT id FROM dbo.Users WHERE username = '"+Request.Form["UserText"]+"'";
                cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                reader.Read();
                Session["userid"] = reader.GetInt32(0);
                
                conn.Close();
            }

            Links.Visible = true;
        }
    }
}