using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Collections;
using System.Threading;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace WebApplication1
{
    public partial class WebForm2 : System.Web.UI.Page
    {
        Logic logic;

        protected void Page_Load(object sender, EventArgs e)
        {
            /*** Used to set up empty ArrayList of values to be used for displaying anagram results
                 Data is bound to the Repeater that is used to dynamically create the results ***/

            if (!IsPostBack)
            {
                ArrayList values = new ArrayList();

                Session["values"] = values;
                Session["flag"] = "";
                setHiddenFlag();

                Repeater1.DataSource = Session["values"];
                Repeater1.DataBind();

            }

            logic = new Logic();
        }

        /*** Fires when the "Run" button is hit on the webpage
         *   Updates the session arraylist with new results returned from "isAnagram" helper
         *   function. Data is then rebound and the results div is displayed with results 
         *   inside ***/

        protected void RunButton_Click(object sender, EventArgs e)
        {
            String str1 = String1.Text;
            String str2 = String2.Text;

            ArrayList list = (ArrayList)Session["values"];
            ResponseHolder rh = logic.isAnagram(str1, str2);
            list.Add(rh);

            Debug.WriteLine(rh.CountMissing);

            if (rh.Flag.Equals("notAnagram"))
            {
                CharactersMissingError.Text = "*** Change " + rh.CountMissing + " letter(s) from " + str2 + " to make an anagram ***";
                CharactersMissingError.Style.Add("display", "inline-block");
            }
            else
            {
                CharactersMissingError.Text = "";
                CharactersMissingError.Style.Add("display", "none");
            }

            Session["flag"] = "add";
            setHiddenFlag();
            Session["values"] = list;

            Repeater1.DataSource = Session["values"];
            Repeater1.DataBind();

            DeletionDiv.Visible = false;
            SaveSuccessDiv.Visible = false;
            NoResultsDiv.Visible = false;
            resultsDiv.Visible = true;
            NoDeleteResultsDiv.Visible = false;

        }

        /*** Fires when the "Clear Results" button is clicked. 
         *   Updates the session ArrayList to empty, binds the data and hides
         *   the results Div. 
         ***/

        protected void ClearResults_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Hellooooooo");

            Session["values"] = new ArrayList();

            Repeater1.DataSource = Session["values"];
            Repeater1.DataBind();

            resultsDiv.Visible = false;
            SaveSuccessDiv.Visible = false;
            NoResultsDiv.Visible = false;
            NoDeleteResultsDiv.Visible = false;
        }

        /*** Fires when a single result is deleted.
         *   Receives the button that sent the delete request and the index of the item in the ArrayList.
         *   Uses the item index to delete the correct Response from the ArrayList.
         *   Updates the Session ArrayList and rebinds the data.
         *   Sets a delete flag so that we know we don't have to create a fadein animation for the 
         *   last response in the list.
         *   Sleeps for half a second to allow the fadeout animation to occur before page refresh.
         ***/

        protected void DeleteResult_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            ArrayList values = (ArrayList)Session["values"];
            values.RemoveAt(int.Parse(btn.CommandArgument.ToString()));
            Session["flag"] = "delete";
            setHiddenFlag();
            Thread.Sleep(400);

            Repeater1.DataSource = Session["values"];
            Repeater1.DataBind();

            NoResultsDiv.Visible = false;
            NoDeleteResultsDiv.Visible = false;

        }

        protected void SaveResults_Click(object sender, EventArgs e)
        {
            string connStr = "";
            connStr = ConfigurationManager.ConnectionStrings["Conn"].ToString();
            ArrayList results = (ArrayList)Session["values"];

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                int i = 0;
                int response = 0;
                ResponseHolder rh;
                while (i < results.Count)
                {
                    rh = (ResponseHolder)results[i];
                    string String1 = rh.String1;
                    string String2 = rh.String2;
                    string result = rh.Response;
                    int userID = (Int32)Session["userid"];
                    string anagram = rh.Flag;

                    string query = "IF NOT EXISTS (SELECT * " +
                                                 "FROM AnagramResults ar " +
                                                 "WHERE ar.String1 = '" + String1 + "' AND ar.String2 = '" + String2 + "') " +
                                   "INSERT INTO AnagramResults VALUES('" +
                                    String1 + "','" + String2 + "','" +
                                    result + "','" + anagram + "');";

                    string anagramLinkQuery = "IF NOT EXISTS (SELECT * FROM AnagramUserLink WHERE UserId=" + userID + " AND AnagramId=(SELECT Id FROM AnagramResults WHERE String1='" + String1 + "' AND String2='" + String2 + "')) " +
                                             "INSERT INTO AnagramUserLink VALUES(" + userID + ", (SELECT Id FROM dbo.AnagramResults WHERE String1 = '" + String1 + "' AND String2 = '" + String2 + "')); ";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();

                    SqlCommand cmd2 = new SqlCommand(anagramLinkQuery, conn);
                    cmd2.Connection.Open();
                    response += cmd2.ExecuteNonQuery();
                    cmd.Connection.Close();
                    i++;
                }

                SaveSuccessLabel.Text = "All results successfully saved!";
                SaveSuccessLabel.Style.Add("color", "green");
                SaveSuccessDiv.Visible = true;

            }

            Session["flag"] = "clear";
            FlagHiddenInput.Value = (string)Session["flag"];
            NoResultsDiv.Visible = false;
            NoDeleteResultsDiv.Visible = false;
            DeletionDiv.Visible = false;
        }

        protected void LoadResults_Click(object sender, EventArgs e)
        {

            string connStr = "";
            connStr = ConfigurationManager.ConnectionStrings["Conn"].ToString();

            int userID = (Int32)Session["userid"];

            ArrayList list = new ArrayList();

            string str1 = String1.Text;
            string str2 = String2.Text;

            using (SqlConnection conn = new SqlConnection(connStr))
            {

                //SqlCommand cmd = new SqlCommand("SELECT Result, Anagram, String1, String2 FROM dbo.AnagramResults WHERE String1='" + str1 + "' AND String2='" + str2 + "';", conn);
                /*SqlCommand cmd2 = new SqlCommand("SELECT ar.Result, ar.Anagram  " +
                                                 "FROM AnagramResults ar, Users usr, AnagramUserLink aul " +
                                                 "WHERE aul.AnagramId = " +
                                                 "(SELECT Id FROM AnagramResults WHERE String1 = '" + str1 + "' AND String2 = '" + str2 + "') and aul.UserId = " + userID + ";", conn);*/
                /*SqlCommand cmd2 = new SqlCommand("SELECT Result, Anagram " +
                                                 "FROM AnagramResults ar " +
                                                 "WHERE ar.Id IN(SELECT AnagramId FROM AnagramUserLink WHERE UserId=" + userID + ") AND ar.String1 = '" + str1 + "' AND ar.String2 = '" + str2 + "';", conn);*/

                SqlCommand cmd2 = new SqlCommand("SELECT Result, Anagram, String1, String2 " +
                                                 "FROM AnagramResults ar " +
                                                 "WHERE ar.Id IN(SELECT AnagramId FROM AnagramUserLink WHERE UserId=" + userID + ") AND String1 LIKE '%" + str1 + "%' AND String2 LIKE '%" + str2 + "%';", conn);

                cmd2.Connection.Open();
                SqlDataReader rdr = cmd2.ExecuteReader();
                ResponseHolder rh;
                int i = 0;
                while (rdr.Read())
                {
                    Debug.WriteLine("Str1: " + rdr.GetValue(2));
                    Debug.WriteLine("Str2: " + rdr.GetValue(3));
                    rh = new ResponseHolder((string)rdr.GetValue(0), (string)rdr.GetValue(1), 0, (string)rdr.GetValue(2), (string)rdr.GetValue(3));
                    list.Add(rh);
                    i++;
                }
                cmd2.Connection.Close();
                Session["values"] = list;

                Repeater1.DataSource = Session["values"];
                Repeater1.DataBind();

                Debug.WriteLine("i: " + i);

                if (i == 0)
                {
                    NoResultsLabel.Text = "No Results Found...";
                    NoResultsDiv.Visible = true;
                }
                else
                {
                    NoResultsDiv.Visible = false;
                }
                resultsDiv.Visible = true;
                NoDeleteResultsDiv.Visible = false;
                DeletionDiv.Visible = false;
                SaveSuccessDiv.Visible = false;
            }
        }

        protected void DeleteResults_Click(object sender, EventArgs e)
        {

            string connStr = "";
            connStr = ConfigurationManager.ConnectionStrings["Conn"].ToString();

            String str1 = String1.Text;
            String str2 = String2.Text;

            int response = -1;
            int anagramID;
            object potentialid;
            int userID = (Int32)Session["userid"];

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string anagramIdQuery = "SELECT ar.ID " +
                                        "FROM AnagramResults ar " +
                                        "WHERE ar.Id IN(SELECT AnagramId FROM AnagramUserLink WHERE UserId =" + userID + ") AND ar.String1 = '" + str1 + "' AND ar.String2 = '" + str2 + "';";

                SqlCommand cmd = new SqlCommand(anagramIdQuery, conn);
                cmd.Connection.Open();
                potentialid = cmd.ExecuteScalar();
                if (potentialid != null)
                {
                    anagramID = (int)potentialid;
                    cmd.Connection.Close();

                    SqlCommand cmd3 = new SqlCommand("DELETE From dbo.AnagramUserLink WHERE UserId=" + userID + " AND AnagramId=" + anagramID + ";", conn);
                    cmd3.Connection.Open();
                    response = cmd3.ExecuteNonQuery();
                    cmd3.Connection.Close();

                }

            }

            Session["values"] = new ArrayList();
            Repeater1.DataSource = Session["values"];
            Repeater1.DataBind();

            if (potentialid != null)
            {
                resultsDiv.Visible = false;
                if (response > 0)
                {
                    deleteResult.Text = "Anagram results for " + str1 + " and " + str2 + " were successfully deleted!";
                }
                else
                {
                    deleteResult.Text = "No results for " + str1 + " and " + str2 + "...";
                }
                DeletionDiv.Visible = true;
                NoResultsDiv.Visible = false;
            }
            else
            {
                NoDeleteResultsLabel.Text = "No results to delete...";
                NoDeleteResultsDiv.Visible = true;
                NoResultsDiv.Visible = false;
            }
        }

        protected void UpdateResult_Click(object sender, EventArgs e)
        {
            int index = Int32.Parse(hiddenUpdateOk.Value);
            ArrayList values = (ArrayList)Session["values"];
            Debug.WriteLine("Count: " + values.Count);
            ResponseHolder response = (ResponseHolder)values[index];
            string first = response.String1;
            string second = response.String2;

            Debug.WriteLine("First: " + first);
            Debug.WriteLine("Second: " + second);

            int userID = (Int32)Session["userid"];

            string connStr = "";
            connStr = ConfigurationManager.ConnectionStrings["Conn"].ToString();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string CheckForResultsQuery = "SELECT count(*) " +
                                     "FROM AnagramResults ar " +
                                     "WHERE ar.Id IN(SELECT AnagramId FROM AnagramUserLink WHERE UserId=" + userID + ") AND ar.String1 = '" + first + "' AND ar.String2 = '" + second + "'; ";

                SqlCommand cmd = new SqlCommand(CheckForResultsQuery, conn);
                cmd.Connection.Open();
                int numResults = (int)cmd.ExecuteScalar();
                cmd.Connection.Close();

                Debug.WriteLine("NumResults: " + numResults);

                if (numResults > 0)
                {
                    ResponseHolder rhUpdated = logic.isAnagram(String1.Text, String2.Text);

                    string updateQuery = "IF NOT EXISTS (SELECT * " +
                                                        "FROM AnagramResults ar " +
                                                        "WHERE ar.String1 = '" + rhUpdated.String1 + "' AND ar.String2 = '" + rhUpdated.String2 + "') " +
                                         "INSERT INTO AnagramResults VALUES('" + rhUpdated.String1 + "', '" + rhUpdated.String2 + "', '" + rhUpdated.Response + "', '" + rhUpdated.Flag + "');";
                    SqlCommand cmd2 = new SqlCommand(updateQuery, conn);
                    cmd2.Connection.Open();
                    int ok = (int)cmd2.ExecuteNonQuery();
                    cmd2.Connection.Close();

                    string updateUserLinkQuery = "IF NOT EXISTS (SELECT * FROM AnagramUserLink WHERE UserId = " + userID + " AND AnagramId = (SELECT Id FROM AnagramResults WHERE String1 = '" + rhUpdated.String1 + "' AND String2 = '" + rhUpdated.String2 + "')) " + 
                                                 "UPDATE dbo.AnagramUserLink SET AnagramId = (SELECT Id FROM AnagramResults WHERE String1 = '" + rhUpdated.String1 + "' AND String2 = '" + rhUpdated.String2 + "') " + 
                                                    "WHERE UserId = " + userID + " AND AnagramId = (SELECT Id FROM AnagramResults WHERE String1 = '" + first + "' AND String2 = '" + second + "') " +
                                                    "ELSE DELETE FROM AnagramUserLink WHERE UserId = " + userID + " AND AnagramId = (SELECT Id FROM AnagramResults WHERE String1 = '" + first + "' AND String2 = '" + second + "'); ";
                    SqlCommand cmd3 = new SqlCommand(updateUserLinkQuery, conn);
                    cmd3.Connection.Open();
                    int ok2 = (int)cmd3.ExecuteNonQuery();
                    cmd3.Connection.Close();

                    Debug.WriteLine("Ok: " + ok2);

                    if (ok2 > 0)
                    {
                        Debug.WriteLine(values.ToString());

                        UpdateResponseField.Value = "Update";
                        Debug.WriteLine("OK!");
                        values[index] = rhUpdated;

                        int i = 0;
                        while (i < values.Count)
                        {
                            ResponseHolder rhtemp = (ResponseHolder)values[i];
                            Debug.WriteLine("Str1: " + rhtemp.String1);
                            Debug.WriteLine("Str2: " + rhtemp.String2);
                            if (rhtemp.String1.Equals(first) && rhtemp.String2.Equals(second))
                            {
                                values[i] = rhUpdated;
                                Debug.WriteLine("In");
                            }
                            i++;
                        }

                        Session["values"] = values;

                        Repeater1.DataSource = Session["values"];
                        Repeater1.DataBind();

                        CharactersMissingError.Visible = false;
                        CharactersMissingError.Text = "";
                        SaveSuccessDiv.Visible = false;
                    }
                }
                else
                {
                    UpdateResponseField.Value = "NoUpdate";
                    Debug.WriteLine("NOT OK!");
                }
                NoResultsDiv.Visible = false;
                NoDeleteResultsDiv.Visible = false;
                DeletionDiv.Visible = false;
            }
        }

        /*** Used on the front end to check if the item being added to the Results is the
         *   last item being added. If it is, and the request is an add request (checked by the flag 
         *   that is set on the Session) the last item is given the class hidden, which sets it as
         *   "display: none" and allows it to be faded in on pageload.
         *   If the request is to delete, no class will be added.
         ***/

        public String checkLast(int index)
        {
            ArrayList values = (ArrayList)Session["values"];
            String flag = (String)Session["flag"];

            return logic.checkLast(values, flag, index);
        }

        public void setHiddenFlag()
        {
            FlagHiddenInput.Value = (string)Session["flag"];
        }

        public void ClearTop_Click(object sender, EventArgs e)
        {
            CharactersMissingError.Text = "";
            DeletionDiv.Visible = false;
            Session["flag"] = "clear";
            FlagHiddenInput.Value = (string)Session["flag"];
        }

    }
}