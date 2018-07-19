﻿using System;
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
            if (int.Parse(request.queryResult.parameters.year) < 2005 || int.Parse(request.queryResult.parameters.year) > DateTime.Now.Year)
            {
                return "Sorry, we don't have reports for year " + request.queryResult.parameters.year;
            }
            else
            {
                if (!string.IsNullOrEmpty(request.queryResult.parameters.code))
                {
                    return GiveURL(request);
                }
                else
                {
                    return "Alright, we will now find reports for year " + request.queryResult.parameters.year + ". If you know the code for this report, you can tell me that and the year. If not, we can search reports.";
                }
            }
        }

        //Gives the report URL once a year and code have been given.
        public string GiveURL(ApiAiRequest request)
        {
            //If the user asked for a summary, that has a differnet URL so it has a different statement.
            if (!string.IsNullOrEmpty(request.queryResult.parameters.year) && !string.IsNullOrEmpty(request.queryResult.parameters.code) && request.queryResult.parameters.code.ToUpper() != "SUMMARY")
            {
                string url = "http://datareports.lsu.edu/Reports.aspx?yr=" + request.queryResult.parameters.year + "&rpt=" + request.queryResult.parameters.code + "&p=ci";
                return "Here is your URL: " + url;
            }
            else if (!string.IsNullOrEmpty(request.queryResult.parameters.year) && !string.IsNullOrEmpty(request.queryResult.parameters.code) && request.queryResult.parameters.code.ToUpper() == "SUMMARY")
            {
                string url = "http://datareports.lsu.edu/Reports/TrafficReports/" + request.queryResult.parameters.year + "/Summary/Summary.asp";
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
            SQLiteConnection cn = new SQLiteConnection("Data Source=|DataDirectory|\\CRASH_LINKS.sqlite3; Version=3");
            //Once we go back to MSSS: [CRASH_LINKS].[dbo].[TRAFFIC_CMV_HEADERS]
            string query = "SELECT [REPORTHEADER], [REPORTHEADERLONG] From TRAFFIC_CMV_HEADERS";
            SQLiteCommand cmd = new SQLiteCommand(query, cn);
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            DataTable tbl = new DataTable();

            cn.Open();
            da.Fill(tbl);
            cn.Close();

            string s = "Here are the topics:";
            for (int i = 0; i < tbl.Rows.Count; i++)
            {
                s += Environment.NewLine + tbl.Rows[i].ItemArray[0] + "- " + tbl.Rows[i].ItemArray[1];
            }

            return s;
        }

        //When the user gives a topic, lists all reports belonging to that topic.
        public string ListByTopic(ApiAiRequest request)
        {
            SQLiteConnection cn = new SQLiteConnection("Data Source=|DataDirectory|\\CRASH_LINKS.sqlite3; Version=3");
            string query = "SELECT [REPORTLETTER],[SUBHEADERNUM],[POSTNUMBER],[SUBHEADER] FROM TRAFFIC WHERE [REPORTLETTER] = '" + request.queryResult.parameters.Topic + "' AND [ACTIVE] = 1 ORDER BY [REPORTLETTER], [SUBHEADERNUM], [POSTNUMBER]";
            SQLiteCommand cmd = new SQLiteCommand(query, cn);
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            DataTable tbl = new DataTable();

            cn.Open();
            da.Fill(tbl);
            cn.Close();

            string s = "Here are the reports in this topic:";
            for (int i = 0; i < tbl.Rows.Count; i++)
            {
                s += Environment.NewLine + tbl.Rows[i].ItemArray[0] + tbl.Rows[i].ItemArray[1] + tbl.Rows[i].ItemArray[2] + "- " + tbl.Rows[i].ItemArray[3];
            }

            return s;
        }

        //When the user gives keyword(s), lists reports containing those keywords.
        public string ListByKeyword(ApiAiRequest request, bool and)
        {
            SQLiteConnection cn = new SQLiteConnection("Data Source=|DataDirectory|\\CRASH_LINKS.sqlite3; Version=3");
            //The "and" bool decides whether the query should fetch reports containing all keywords (x AND y) or all reports containing any of the keywords (x OR y)
            string query = KeywordQueryBuilder(request, and);
            SQLiteCommand cmd = new SQLiteCommand(query, cn);
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            DataTable tbl = new DataTable();

            cn.Open();
            da.Fill(tbl);
            cn.Close();

            //On the first run, "and" is true. x AND y is more likely to help the user find what they're looking for.
            //If no results are returned this way, the function runs again with "and" being false, so the user can still get some results.
            string s = (and ? "Here are the reports that contain these keywords:" : "We could not find any results that contain all of these keywords, so here are results that contain one or more of these keywords:");
            if (tbl.Rows.Count > 0)
            {
                for (int i = 0; i < tbl.Rows.Count; i++)
                {
                    s += Environment.NewLine + tbl.Rows[i].ItemArray[0] + tbl.Rows[i].ItemArray[1] + tbl.Rows[i].ItemArray[2] + "- " + tbl.Rows[i].ItemArray[3];
                }
            }
            else
            {
                return (and ? ListByKeyword(request, false) : "Could not find results for these keywords.");
            }

            return s;
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
            query += ") AND [ACTIVE] = 1 ORDER BY[REPORTLETTER], [SUBHEADERNUM], [POSTNUMBER]";
            return query;
        }

        //This is the first function for the querying ability of the bot.
        public string Query(ApiAiRequest request)
        {
            SqlConnection cn = new SqlConnection("Data Source=dev-sqlsrv;Initial Catalog=CRASHDWHSRG;Integrated Security=true");
            doublestring ds = QueryQueryBuilder(request);
            string query = ds.string1;
            string conditionsforpeople = ds.string2;
            SqlCommand cmd = new SqlCommand(query, cn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable tbl = new DataTable();

            cn.Open();
            da.Fill(tbl);
            cn.Close();

            string s = "Here are the conditions we considered:" + Environment.NewLine;
            s += conditionsforpeople;
            s += "Here is the result from those conditions:" + Environment.NewLine;
            s += tbl.Rows[0].ItemArray[0];
            return s;
        }

        //This function builds the query for the query functionality.
        private doublestring QueryQueryBuilder(ApiAiRequest request)
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
            string conditionsforpeople = "";
            List<string> conditions = new List<string>();
            //Gets the base conditions based on which table the query is using.
            //Conditions are resolved through Dialogflow, which takes something the user says such as "Moderate injury" and turns it into InjuryCode = 'C' for use in SQL
            switch (table)
            {
                case "FactPerson":
                    conditions = request.queryResult.parameters.PersonConditions;
                    break;
                case "FactCrash":
                    conditions = request.queryResult.parameters.CrashConditions;
                    break;
                case "FactVehicle":
                    conditions = request.queryResult.parameters.VehicleConditions;
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
                    query += conditions[i].Substring(0, conditions[i].IndexOf(";")) + " AND "; 
                    conditionsforpeople += conditions[i].Substring(conditions[i].IndexOf(";") + 1, conditions[i].Length - conditions[i].IndexOf(";") - 1) + Environment.NewLine;
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
                string year1 = request.queryResult.parameters.year1;
                string year2 = request.queryResult.parameters.year2;
                //Puts whichever year is smaller first.
                if (int.Parse(year2) > int.Parse(year1))
                {
                    query += table.Replace("Fact", "") + "Origin >= '" + year1 + "' AND ";
                    query += table.Replace("Fact", "") + "Origin <= '" + year2 + "'";
                    conditionsforpeople += "From " + year1 + "-" + year2 + Environment.NewLine;
                }
                else
                {
                    query += table.Replace("Fact", "") + "Origin >= '" + year2 + "' AND ";
                    query += table.Replace("Fact", "") + "Origin <= '" + year1 + "'";
                    conditionsforpeople += "From " + year2 + "-" + year1 + Environment.NewLine;
                }
            }
            else
            {
                //The name of the tables are FactX, and the column name in the tables containing the year the data came from is called XOrigin.
                //So this turns FactX into XOrigin, and checks if it's equal to the year specified by the user.
                query += table.Replace("Fact", "") + "Origin = '" + request.queryResult.parameters.year1 + "'";
                conditionsforpeople += "In " + request.queryResult.parameters.year1 + Environment.NewLine;
            }
            doublestring ds = new doublestring();
            ds.string1 = query;
            ds.string2 = conditionsforpeople;
            return ds;
        }

        struct doublestring
        {
            public string string1;
            public string string2;
        }

        //IntVars are the name I use to refer to condition variables which involve a number. These are different from other conditions because they have a variable aspect
        //in them ("Age greater than x" -> Age > x) unlike the other conditions ("fatality" -> InjuryCode = 'A') Thus I have to retrieve them a different way.
        private List<string> GetIntVars(ApiAiRequest request, string table)
        {
            List<string> intconditions = new List<string>();
            switch (table)
            {
                case "FactPerson":
                    for (int i = 0; i < request.queryResult.parameters.PersonConditionIntvar.Count(); i++)
                    {
                        intconditions.Add(request.queryResult.parameters.PersonConditionIntvar[i].PersonConditionInt + " " + request.queryResult.parameters.PersonConditionIntvar[i].number.ToString());
                    }
                    break;
                case "FactCrash":
                    for (int i = 0; i < request.queryResult.parameters.CrashConditionIntvar.Count(); i++)
                    {
                        intconditions.Add(request.queryResult.parameters.CrashConditionIntvar[i].CrashConditionInt + " " + request.queryResult.parameters.CrashConditionIntvar[i].number.ToString());
                    }
                    break;
                case "FactVehicle":
                    for (int i = 0; i < request.queryResult.parameters.VehicleConditionIntvar.Count(); i++)
                    {
                        intconditions.Add(request.queryResult.parameters.VehicleConditionIntvar[i].VehicleConditionInt + " " + request.queryResult.parameters.VehicleConditionIntvar[i].number.ToString());
                    }
                    break;
            }
            return intconditions;
        }
    }
}