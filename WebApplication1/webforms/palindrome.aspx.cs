using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace WebApplication1
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        PalindromeLogic logic = new PalindromeLogic();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {   // Initialization
                ResultsDiv.Visible = false;
                PrevResultsDiv.Visible = false;
                UpdateDiv.Visible = false;

                Session["values"] = new ArrayList();
                ResultsRepeater.DataSource = Session["values"];

                Session["loaded"] = new ArrayList();
                PrevResultsRepeater.DataSource = Session["loaded"];
            }
            else if (Session["values"] != null)
            {
                ArrayList list = (ArrayList)Session["values"];

                foreach (String[] array in list)
                {
                    if (array[2] == "fadeIn") { array[2] = "default"; }
                }

                updateResults();
            }
        }

        // Updates the data source for the repeater which displays results. 
        // This in turn updates the displayed results.
        protected void updateResults()
        {
            ResultsRepeater.DataSource = Session["values"];
            ResultsRepeater.DataBind();
        }

        // Called when the 'Clear' button near the input field is pressed
        protected void ClearInput_Click(object sender, EventArgs e)
        {
            // Results that were deleted have to be cleared before any action that re-renders the page
            ArrayList list = (ArrayList)Session["values"];
            list.Remove(logic.findDeleted(list));
            updateResults();

            Input.Text = string.Empty;
            ErrorMessage.InnerText = string.Empty;
        }

        // Called when the 'Clear' button near the displayed results is pressed
        protected void ClearResults_Click(object sender, EventArgs e)
        {
            Session["values"] = new ArrayList();
            updateResults();

            ResultsDiv.Visible = false;
        }

        /*
         * When one of the [delete] buttons is pressed, Delete_Click is called.
         * 
         * Each result li has attr CommandArgument=<index in Session['values']>.  This index is used to locate the result
         * when it needs to be deleted.
         * 
         * In order for an element to be faded out, it must be displayed after a page PostBack.  This means deleted results
         * can't be deleted when the delete button is pressed.  Instead, the li's id is set to 'deleted'.  jQuery fades out elements 
         * with this id.
         * 
         * The 'deleted' id also signifies to the codebehind which results should be removed from Session['values']. When
         * Delete_Click is called, the last result which was marked as 'deleted' is found and stored so it can actually be removed
         * from the session. The result is not removed immediately, as that would change the size of the list, potentially causing the index
         * CommandArgument to be out of range.
         */
        protected void Delete_Click(object sender, EventArgs e)
        {
            // Variable initialization
            Button btn = (Button)sender;
            int index = Int32.Parse(btn.CommandArgument.ToString());
            ArrayList list = (ArrayList)Session["values"];

            // Find previously deleted result
            String[] deleted = logic.findDeleted(list);

            // Set result whose delete button was clicked as 'deleted'.
            ((String[])list[index])[2] = "deleted";

            // Remove previously deleted item after index value is no longer needed.
            list.Remove(deleted);

            // Hide results display if there are no results.
            if (list.Count <= 1) { ResultsDiv.Visible = false; }

            updateResults();
        }

        /* 
         * This method is called when the 'Run' button is clicked.
         * It performs input validation, cleans the input so it ignores special characters and puncutation,
         * checks to see if the string is a palindrome, and builds the result ands add it to the Session.
         */
        protected void Run_Click(object sender, EventArgs e)
        {
            // Attempt to peform palindrome check
            int diff = 0;
            try
            {
                diff = logic.isPalindrome(Request.Form["input"]);
            }
            // If input is invalid an exception is thrown
            catch (System.ArgumentException exception)
            {
                if (exception.Message.Equals("Empty string input"))
                {
                    ErrorMessage.InnerText = "Please enter a string";
                }
                else if (exception.Message.Equals("Input is too long"))
                {
                    ErrorMessage.InnerText = "String is too long.  Please enter a string no longer than 40 characters in length.";
                }
                return;
            }

            // If the check is successful, a result is build and added to Session
            string[] tuple = logic.buildResult(diff, Request.Form["input"]);
            ErrorMessage.InnerText = logic.createErrorMessage(diff, Request.Form["input"]);

            ArrayList list = (ArrayList)Session["values"];
            list.Insert(0, tuple);
            list.Remove(logic.findDeleted(list));
            updateResults();

            ResultsDiv.Visible = true;
        }

        // Called when the 'Save' button is pressed
        protected void Search_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                conn.Open();

                cmd = new SqlCommand("FindResultByString", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@string", Request.Form["Input"]);
                SqlDataReader reader = cmd.ExecuteReader();

                int palindromeID = 0;
                if (reader.Read()) { palindromeID = reader.GetInt32(0); }
                reader.Close();

                cmd = new SqlCommand("LoadResultById", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userid", (Int32)Session["userid"]);
                cmd.Parameters.AddWithValue("@pid", palindromeID);
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    ArrayList list = new ArrayList();
                    string[] result = new string[4] { reader.GetString(1), reader.GetString(2), "default", reader.GetString(0) };
                    list.Add(result);

                    Session["loaded"] = list;
                    PrevResultsRepeater.DataSource = Session["loaded"];
                    PrevResultsRepeater.DataBind();
                    PrevResultsDiv.Visible = true;
                }
                else
                {
                    ErrorMessage.InnerText = "No matching result found for your user.";
                }

                reader.Close();
                conn.Close();
            }
        }

        // Called when the 'Load Results' button is pressed
        protected void LoadResults_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                conn.Open();

                cmd = new SqlCommand("LoadUserResults", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userid", (Int32)Session["userid"]);
                SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows) { ErrorMessage.InnerText = "No results for this user"; }

                ArrayList list = new ArrayList();
                while (reader.Read())
                {
                    string[] result = new string[4] { reader.GetString(1), reader.GetString(2), "default", reader.GetString(0) };
                    list.Add(result);
                }

                conn.Close();

                Session["loaded"] = list;
                PrevResultsRepeater.DataSource = Session["loaded"];
                PrevResultsRepeater.DataBind();

                if (list.Count > 0) { PrevResultsDiv.Visible = true; }
            }
        }

        // Called when the 'Save Results' button is pressed
        protected void SaveResults_Click(object sender, EventArgs e)
        { 
            if (Session["userid"] == null)
            {
                ErrorMessage.InnerText = "You are not signed in as a user.  Please return to the homepage and sign in.";
                return;
            }

            ArrayList list = (ArrayList)Session["values"];
            list.Remove(logic.findDeleted(list));

            string connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                conn.Open();

                foreach (string[] array in list)
                {
                    // Insert palindrome result into results table
                    cmd = new SqlCommand("AddResult", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@string", array[3]);
                    cmd.Parameters.AddWithValue("@result", array[0]);
                    cmd.Parameters.AddWithValue("@palindrome", array[1]);
                    cmd.ExecuteNonQuery();

                    // Find ID of result

                    cmd = new SqlCommand("FindResultByString", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@string", array[3]);
                    SqlDataReader reader = cmd.ExecuteReader();

                    reader.Read();
                    int palindromeID = reader.GetInt32(0);
                    reader.Close();

                    // Create link between user and result
                    cmd = new SqlCommand("LinkUserResult", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@userid", (Int32)Session["userid"]);
                    cmd.Parameters.AddWithValue("@palindromeid", palindromeID);
                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        protected void PrevDelete_Click(object sender, EventArgs e)
        {
            // Variable initialization
            Button btn = (Button)sender;
            int index = Int32.Parse(btn.CommandArgument.ToString());
            ArrayList list = (ArrayList)Session["loaded"];

            string connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("FindResultByString", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@string", ((string[])list[index])[3]);
                SqlDataReader reader = cmd.ExecuteReader();

                reader.Read();
                int palindromeID = reader.GetInt32(0);
                reader.Close();

                cmd = new SqlCommand("DeleteResult", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@palindromeid", palindromeID);
                cmd.Parameters.AddWithValue("@userid", (Int32)Session["userid"]);
                cmd.ExecuteNonQuery();

                conn.Close();
            }

            list.RemoveAt(index);
            if (list.Count <= 0) { PrevResultsDiv.Visible = false; }

            PrevResultsRepeater.DataSource = Session["loaded"];
            PrevResultsRepeater.DataBind();
        }

        protected void PrevUpdate_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            Session["updateindex"] = Int32.Parse(btn.CommandArgument.ToString());

            SubmitUpdate.Enabled = true;
            UpdateDiv.Visible = true;
        }

        protected void SubmitUpdate_Click(object sender, EventArgs e)
        {
            // Variable initialization
            int index = (int)Session["updateindex"];
            string input = Request.Form["UpdateInput"];
            ArrayList list = (ArrayList)Session["loaded"];
            string origin = ((String[])list[index])[3];

            // Attempt to peform palindrome check
            int diff = 0;
            try
            {
                diff = logic.isPalindrome(input);
            }
            // If input is invalid an exception is thrown
            catch (System.ArgumentException exception)
            {
                if (exception.Message.Equals("Empty string input"))
                {
                    UpdateErrorMessage.InnerText = "Please enter a string";
                }
                else if (exception.Message.Equals("Input is too long"))
                {
                    UpdateErrorMessage.InnerText = "String is too long.  Please enter a string no longer than 40 characters in length.";
                }
                return;
            }

            // If the check is successful, a result is built
            string[] tuple = logic.buildResult(diff, input);
            tuple[2] = "default";
            UpdateErrorMessage.InnerText = logic.createErrorMessage(diff, input);

            string connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Get ID of old result
                SqlCommand cmd = new SqlCommand("FindResultByString", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@string", origin);
                SqlDataReader reader = cmd.ExecuteReader();

                reader.Read();
                int palindromeID = reader.GetInt32(0);
                reader.Close();

                // Figure out how many users point to that result
                cmd = new SqlCommand("CountInstancesOfPalindromeID", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@pid", palindromeID);
                reader = cmd.ExecuteReader();

                reader.Read();
                int count = reader.GetInt32(1);
                reader.Close();

                // If no users besides the current one reference the old result
                if (count <= 1)
                { 
                    cmd = new SqlCommand("FindResultByString", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@string", tuple[3]);
                    reader = cmd.ExecuteReader();

                    // If there is an existing result that matches the update
                    if (reader.Read())
                    {
                        int existingID = reader.GetInt32(0);
                        reader.Close();

                        // Delete old result since nothing points to it
                        cmd = new SqlCommand("DeleteResult", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@userid", (Int32)Session["userid"]);
                        cmd.Parameters.AddWithValue("@palindromeid", palindromeID);
                        cmd.ExecuteNonQuery();

                        // Create pointer to existing result that matches the update
                        cmd = new SqlCommand("LinkUserResult", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@userid", (Int32)Session["userid"]);
                        cmd.Parameters.AddWithValue("@palindromeid", existingID);
                        cmd.ExecuteNonQuery();
                    }
                    // If there is not an existing result that matches the update
                    else
                    {
                        reader.Close();

                        // Update the existing result
                        cmd = new SqlCommand("UpdateResult", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@string", tuple[3]);
                        cmd.Parameters.AddWithValue("@result", tuple[0]);
                        cmd.Parameters.AddWithValue("@palindrome", tuple[1]);
                        cmd.Parameters.AddWithValue("@palindromeid", palindromeID);
                        cmd.ExecuteNonQuery();
                    }
                }
                // If other users do reference the old result
                else
                {
                    // Add new result
                    cmd = new SqlCommand("AddResult", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@string", tuple[3]);
                    cmd.Parameters.AddWithValue("@result", tuple[0]);
                    cmd.Parameters.AddWithValue("@palindrome", tuple[1]);
                    cmd.ExecuteNonQuery();

                    // Find ID of new result
                    cmd = new SqlCommand("FindResultByString", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@string", tuple[3]);
                    reader = cmd.ExecuteReader();

                    reader.Read();
                    int new_palindromeID = reader.GetInt32(0);
                    reader.Close();

                    // Create link between user and new result
                    cmd = new SqlCommand("LinkUserResult", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@userid", (Int32)Session["userid"]);
                    cmd.Parameters.AddWithValue("@palindromeid", new_palindromeID);
                    cmd.ExecuteNonQuery();

                    // Remove old link
                    cmd = new SqlCommand("DeleteUserLink", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@userid", (Int32)Session["userid"]);
                    cmd.Parameters.AddWithValue("@palindromeid", palindromeID);
                    cmd.ExecuteNonQuery();
                }

                if (list.Count > 1) { LoadResults_Click(sender, e); }
                else
                {
                    cmd = new SqlCommand("FindResultByString", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@string", input);
                    reader = cmd.ExecuteReader();

                    int finalID = 0;
                    if (reader.Read()) { finalID = reader.GetInt32(0); }
                    reader.Close();

                    cmd = new SqlCommand("LoadResultById", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@userid", (Int32)Session["userid"]);
                    cmd.Parameters.AddWithValue("@pid", finalID);
                    reader = cmd.ExecuteReader();

                    ArrayList newlist = new ArrayList();
                    if (reader.Read())
                    {
                        newlist.Add(new string[4] { reader.GetString(1), reader.GetString(2), "default", reader.GetString(0) });
                    }
                    reader.Close();

                    Session["loaded"] = newlist;
                    PrevResultsRepeater.DataSource = Session["loaded"];
                    PrevResultsRepeater.DataBind();
                }

                conn.Close();
            }

            SubmitUpdate.Enabled = false;
        }

        protected void Hide_Click(object sender, EventArgs e)
        {
            PrevResultsDiv.Visible = false;
            UpdateDiv.Visible = false;
        }
    }
}