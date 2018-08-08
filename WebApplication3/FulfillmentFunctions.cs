using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ApiAiSDK;
using ApiAiSDK.Model;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Web.Script.Serialization;
using WebApplication3.Models;
using WebApplication3.Controllers;
using System.Data.SqlClient;
using System.Data;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace WebApplication3
{
    public class FulfillmentFunctions
    {
        //If a string is not null or empty, that means a user has provided it.

        //Handles when the user asks to find a report.
        public string FindReport(ApiAiRequest request)
        {

            if (!string.IsNullOrEmpty(request.queryResult.parameters.year) && !string.IsNullOrEmpty(request.queryResult.parameters.code))
            {
                return GiveURL(request);
            }
            else if (!string.IsNullOrEmpty(request.queryResult.parameters.year))
            {
                return CheckYear(request);
            }
            else
            {
                return "If you know the code for this report, you can tell me that. If not, we can search reports.";
            }
        }

        //Checks the year given when finding a report.
        public string CheckYear(ApiAiRequest request)
        {
            string year = Regex.Replace(request.queryResult.parameters.year, "[^0-9.]", "");
            if (int.Parse(year) < 2005 || int.Parse(year) > DateTime.Now.Year)
            {
                return "Sorry, we don't have reports for year " + year;
            }
            else
            {
                if (!string.IsNullOrEmpty(request.queryResult.parameters.code))
                {
                    return GiveURL(request);
                }
                else
                {
                    return "Alright, we will now find reports for year " + year + ". If you know the code for this report, you can tell me that and the year. If not, we can search reports.";
                }
            }
        }

        //Gives the report URL once a year and code have been given.
        public string GiveURL(ApiAiRequest request)
        {
            //If the user asked for a summary, that has a differnet URL so it has a different statement.
            if (!string.IsNullOrEmpty(request.queryResult.parameters.year) && !string.IsNullOrEmpty(request.queryResult.parameters.code) && request.queryResult.parameters.code.ToUpper() != "SUMMARY")
            {
                string year = Regex.Replace(request.queryResult.parameters.year, "[^0-9.]", "");
                string url = "http://datareports.lsu.edu/Reports.aspx?yr=" + year + "&rpt=" + request.queryResult.parameters.code + "&p=ci";
                return "Here is your URL: " + url;
            }
            else if (!string.IsNullOrEmpty(request.queryResult.parameters.year) && !string.IsNullOrEmpty(request.queryResult.parameters.code) && request.queryResult.parameters.code.ToUpper() == "SUMMARY")
            {
                string year = Regex.Replace(request.queryResult.parameters.year, "[^0-9.]", "");
                string url = "http://datareports.lsu.edu/Reports/TrafficReports/" + year + "/Summary/Summary.asp";
                return "Here is your URL: " + url;
            }
            else if (!string.IsNullOrEmpty(request.queryResult.parameters.year))
            {
                return "What is the code of this report?";
            }
            else if (!string.IsNullOrEmpty(request.queryResult.parameters.code))
            {
                return "What year would you like to find this report for?";
            }
            else
            {
                return "Please provide a year and report code.";
            }
        }

        //Gives the user a list of topics that reports may fall under.
        public string ListTopics(ApiAiRequest request)
        {
            using (SQLiteConnection cn = new SQLiteConnection("Data Source=|DataDirectory|\\CRASH_LINKS.sqlite3; Version=3"))
            {
                string query = "SELECT [REPORTHEADER], [REPORTHEADERLONG] From TRAFFIC_CMV_HEADERS";
                SQLiteCommand cmd = new SQLiteCommand(query, cn);

                string s = "Here are the topics:";

                cn.Open();
                SQLiteDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    s += Environment.NewLine + dr[0] + "- " + dr[1];
                }
                cn.Close();

                return s;
            }
        }

        //When the user gives a topic, lists all reports belonging to that topic.
        public string ListByTopic(ApiAiRequest request)
        {
            using (SQLiteConnection cn = new SQLiteConnection("Data Source=|DataDirectory|\\CRASH_LINKS.sqlite3; Version=3"))
            {
                string query = "SELECT [REPORTLETTER],[SUBHEADERNUM],[POSTNUMBER],[SUBHEADER] FROM TRAFFIC WHERE [REPORTLETTER] = '" + request.queryResult.parameters.Topic + "' AND [ACTIVE] = 1 ORDER BY [REPORTLETTER], [SUBHEADERNUM], [POSTNUMBER]";
                SQLiteCommand cmd = new SQLiteCommand(query, cn);

                string s = "Here are the reports in this topic:";

                cn.Open();
                SQLiteDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    s += Environment.NewLine + dr[0] + dr[1] + dr[2] + "- " + dr[3];
                }
                cn.Close();

                return s;
            }
        }

        //When the user gives keyword(s), lists reports containing those keywords.
        public string ListByKeyword(ApiAiRequest request, bool and)
        {
            using (SQLiteConnection cn = new SQLiteConnection("Data Source=|DataDirectory|\\CRASH_LINKS.sqlite3; Version=3"))
            {
                //The "and" bool decides whether the query should fetch reports containing all keywords (x AND y) or all reports containing any of the keywords (x OR y)
                string query = KeywordQueryBuilder(request, and);
                SQLiteCommand cmd = new SQLiteCommand(query, cn);

                string s = (and ? "Here are the reports that contain these keywords:" : "We could not find any results that contain all of these keywords, so here are results that contain one or more of these keywords:");

                cn.Open();
                SQLiteDataReader dr = cmd.ExecuteReader();
                bool foundatleastone = false;
                while(dr.Read())
                {
                    s += Environment.NewLine + dr[0] + dr[1] + dr[2] + "- " + dr[3];
                    foundatleastone = true;
                }
                cn.Close();
                //On the first run, "and" is true. x AND y is more likely to help the user find what they're looking for.
                //If no results are returned this way, the function runs again with "and" being false, so the user can still get some results.
                if (!foundatleastone)
                {
                    return (and ? ListByKeyword(request, false) : "Could not find results for these keywords.");
                }

                return s;
            }
        }

        //Builds the query for searching by keyword.
        private string KeywordQueryBuilder(ApiAiRequest request, bool and)
        {
            string query = "SELECT [REPORTLETTER],[SUBHEADERNUM],[POSTNUMBER],[SUBHEADER] FROM TRAFFIC WHERE (";
            //Keywords are resolved through Dialogflow. If the user says phrases such as "death", "killed", "fatality," 
            //they will all resolve to "fatal", which is a keyword many reports have.
            List<string> keywords = request.queryResult.parameters.KeyWords;
            for(int i = 0; i < keywords.Count(); i++)
            {
                //Makes it so the query checks if the keywords of a report contains each keyword given.
                if (i == 0)
                { 
                    query += "[KEYWORDS] LIKE '%" + keywords[i] + "%'";
                }
                else
                {
                    //If "and" is true, uses AND, otherwise uses OR
                    query += (and ? " AND [KEYWORDS] LIKE '%" + keywords[i] + "%'" : " OR [KEYWORDS] LIKE '%" + keywords[i] + "%'");
                }
            }
            query += ") AND [ACTIVE] = 1 ORDER BY [REPORTLETTER], [SUBHEADERNUM], [POSTNUMBER]";
            return query;
        }

        //This is the first function for the querying ability of the bot.
        public string Query(ApiAiRequest request)
        {
            using (SqlConnection cn = new SqlConnection("Data Source=dev-sqlsrv;Initial Catalog=CRASHDWHSRG;Integrated Security=true"))
            {
                TripleString ts = QueryQueryBuilder(request);
                string query = ts.string1;
                string conditionsforpeople = ts.string2;
                string query_total = ts.string3;
                SqlCommand cmd = new SqlCommand(query, cn);

                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                double result = 0;
                while (dr.Read())
                {
                    result = double.Parse(dr[0].ToString());
                }
                cn.Close();

                cmd = new SqlCommand(query_total, cn);

                cn.Open();
                dr = cmd.ExecuteReader();
                double total = 1;
                while(dr.Read())
                {
                    total = double.Parse(dr[0].ToString());
                }
                cn.Close();

                double percent = Math.Round((result / total) * 100, 3);

                string s = "Here are the conditions we considered:" + Environment.NewLine;
                s += conditionsforpeople;
                s += "Here is the result from those conditions:" + Environment.NewLine;
                s +=  result + " (" + percent + "%)";


                return s;
            }        
        }

        //This function builds the query for the query functionality.
        private TripleString QueryQueryBuilder(ApiAiRequest request)
        {
            string table = "";
            //If the base parameters don't contains the table (which they probably won't) searches the output contexts for the table's value until it finds it.
            if (!string.IsNullOrEmpty(request.queryResult.parameters.Table))
            {
                table = request.queryResult.parameters.Table;
            }
            else
            {
                for(int i = 0; i < request.queryResult.outputContexts.Count(); i++)
                {
                    if(!string.IsNullOrEmpty(request.queryResult.outputContexts[i].parameters.Table))
                    {
                        table = request.queryResult.outputContexts[i].parameters.Table;
                        break;
                    }
                }
            }
            string query = "SELECT COUNT(*) FROM " + table + " WHERE ";
            string query_total = query;
            string conditionsforpeople = "";
            List<string> conditions = new List<string>();
            //Gets the base conditions based on which table the query is using.
            //Conditions are resolved through Dialogflow, which takes something the user says such as "Moderate injury" and turns it into InjuryCode = 'C' for use in SQL
            switch (table)
            {
                case "FactPerson":
                    conditions = request.queryResult.parameters.PersonConditions;
                    conditionsforpeople += "People" + Environment.NewLine;
                    break;
                case "FactCrash":
                    conditions = request.queryResult.parameters.CrashConditions;
                    conditionsforpeople += "Crashes" + Environment.NewLine;
                    break;
                case "FactVehicle":
                    conditions = request.queryResult.parameters.VehicleConditions;
                    conditionsforpeople += "Vehicles" + Environment.NewLine;
                    break;
            }
            List<string> conditionsintvars = GetIntVars(request, table);
            conditions.AddRange(conditionsintvars);
            //Every (non-int) condition resolved through Dialogflow has a 2nd part. The full string is something like Injury='C';Injury: Moderate.
            //The purpose of the 2nd part of the string is to provide the user with a list of conditions that is readable to them.
            //We want the user to make sure the number is actually what they were looking for, and they obviously wouldn't know what InjuryCode = 'C' means.
            //So if a condition in this for loop has a semicolon, it splits the string into two parts, the first for the SQL query and the 2nd for the user.
            for (int i = 0; i < conditions.Count(); i++)
            {
                if(conditions[i].Contains(";"))
                {
                    string[] s = conditions[i].Split(';');
                    query += s[0] + " AND "; 
                    conditionsforpeople += s[1] + Environment.NewLine;
                }
                else
                {
                    query += conditions[i] + " AND ";
                    conditionsforpeople += conditions[i] + Environment.NewLine;
                }
            }
            //Queries can be either for a single year or a range of years. If year2 is empty it's for a single year.
            if (!string.IsNullOrEmpty(request.queryResult.parameters.year2))
            {
                string temp = "";
                string year1 = request.queryResult.parameters.year1;
                string year2 = request.queryResult.parameters.year2;
                //Puts whichever year is smaller first.
                if (int.Parse(year2) > int.Parse(year1))
                {
                    if(table == "FactCrash")
                    {
                        temp = "LEFT(DateSK, 4) >= '" + request.queryResult.parameters.year1 + "'";
                        temp += " AND LEFT(DateSK, 4) <= '" + request.queryResult.parameters.year2 + "'";
                    }
                    else
                    {
                        temp = "CrashSK in (select CrashPK from FactCrash where LEFT(DateSK, 4) >= '" + request.queryResult.parameters.year1 + "')";
                        temp += " AND CrashSK in (select CrashPK from FactCrash where LEFT(DateSK, 4) <= '" + request.queryResult.parameters.year2 + "')";
                    }
                    conditionsforpeople += "From " + year1 + "-" + year2 + Environment.NewLine;
                }
                else
                {
                    if (table == "FactCrash")
                    {
                        temp = "LEFT(DateSK, 4) <= '" + request.queryResult.parameters.year1 + "'";
                        temp += " AND LEFT(DateSK, 4) >= '" + request.queryResult.parameters.year2 + "'";
                    }
                    else
                    {
                        temp = "CrashSK in (select CrashPK from FactCrash where LEFT(DateSK, 4) <= '" + request.queryResult.parameters.year1 + "')";
                        temp += " AND CrashSK in (select CrashPK from FactCrash where LEFT(DateSK, 4) >= '" + request.queryResult.parameters.year2 + "')";
                    }
                    conditionsforpeople += "From " + year2 + "-" + year1 + Environment.NewLine;
                }
                query += temp;
                query_total += temp;
            }
            else
            {
                string temp = "";
                if (table == "FactCrash")
                {
                    temp = "LEFT(DateSK, 4) = '" + request.queryResult.parameters.year1 + "'";
                }
                else
                {
                    temp= "CrashSK in (select CrashPK from FactCrash where LEFT(DateSK, 4) = '" + request.queryResult.parameters.year1 + "')";
                }
                query += temp;
                query_total += temp;
                conditionsforpeople += "In " + request.queryResult.parameters.year1 + Environment.NewLine;
            }
            TripleString ts = new TripleString
            {
                string1 = query,
                string2 = conditionsforpeople,
                string3 = query_total
            };
            return ts;
        }

        struct TripleString
        {
            public string string1;
            public string string2;
            public string string3;
        }

        //IntVars are the name I use to refer to condition variables which involve a number. These are different from other conditions because they have a variable aspect
        //in them ("Age greater than x" -> Age > x) unlike the other conditions ("fatality" -> InjuryCode = 'A') Thus I have to retrieve them a different way.
        private List<string> GetIntVars(ApiAiRequest request, string table)
        {
            List<string> intconditions = new List<string>();
            switch (table)
            {
                case "FactPerson":
                    for (int i = 0; i < request.queryResult.parameters.PersonConditionIntVar.Count(); i++)
                    {
                        if(string.IsNullOrEmpty(request.queryResult.parameters.PersonConditionIntVar[i].Inequality))
                        {
                            intconditions.Add(request.queryResult.parameters.PersonConditionIntVar[i].PersonConditionInt + " = " + request.queryResult.parameters.PersonConditionIntVar[i].number.ToString());
                        }
                        else if(string.IsNullOrEmpty(request.queryResult.parameters.PersonConditionIntVar[i].PersonConditionInt))
                        {
                            intconditions.Add(request.queryResult.parameters.PersonConditionIntVar[i].Inequality + " " + request.queryResult.parameters.PersonConditionIntVar[i].number.ToString());
                        }
                        else
                        {
                            intconditions.Add(request.queryResult.parameters.PersonConditionIntVar[i].PersonConditionInt + " " + request.queryResult.parameters.PersonConditionIntVar[i].Inequality + " " + request.queryResult.parameters.PersonConditionIntVar[i].number.ToString());
                        }
                        intconditions[i] = CheckForAgeThing(intconditions[i]);
                    }
                    break;
                case "FactCrash":
                    for (int i = 0; i < request.queryResult.parameters.CrashConditionIntVar.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(request.queryResult.parameters.CrashConditionIntVar[i].Inequality))
                        {
                            intconditions.Add(request.queryResult.parameters.CrashConditionIntVar[i].CrashConditionInt + " = " + request.queryResult.parameters.CrashConditionIntVar[i].number.ToString());
                        }
                        else
                        {
                            intconditions.Add(request.queryResult.parameters.CrashConditionIntVar[i].CrashConditionInt + " " + request.queryResult.parameters.CrashConditionIntVar[i].Inequality + " " + request.queryResult.parameters.CrashConditionIntVar[i].number.ToString());
                        }
                    }
                    break;
                case "FactVehicle":
                    for (int i = 0; i < request.queryResult.parameters.VehicleConditionIntVar.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(request.queryResult.parameters.VehicleConditionIntVar[i].Inequality))
                        {
                            intconditions.Add(request.queryResult.parameters.VehicleConditionIntVar[i].VehicleConditionInt + " = " + request.queryResult.parameters.VehicleConditionIntVar[i].number.ToString());
                        }
                        else
                        {
                            intconditions.Add(request.queryResult.parameters.VehicleConditionIntVar[i].VehicleConditionInt + " " + request.queryResult.parameters.VehicleConditionIntVar[i].Inequality  + " " + request.queryResult.parameters.VehicleConditionIntVar[i].number.ToString());
                        }
                    }
                    break;
            }
            return intconditions;
        }

        private string CheckForAgeThing(string s)
        {
            if (s.IndexOf("Age") > -1)
            {
                if(s.IndexOf("Age", s.IndexOf("Age") + 1) > -1)
                {
                    s = s.Remove(s.IndexOf("Age"), 4);
                    return s;
                }
                else
                {
                    return s;
                }
            }
            else
            {
                return s;
            }
        }
    }
}