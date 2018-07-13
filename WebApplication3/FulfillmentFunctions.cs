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

        public string FindReport(ApiAiRequest request)
        {

            if (request.queryResult.parameters.year != "" && request.queryResult.parameters.year != "")
            {
                return GiveURL(request);
            }
            else if (request.queryResult.parameters.year != "")
            {
                return CheckYear(request);
            }
            else
            {
                return "If you know the code for this report, you can tell me that. If not, we can search reports.";
            }
        }

        public string CheckYear(ApiAiRequest request)
        {
            if (int.Parse(request.queryResult.parameters.year) < 2005 || int.Parse(request.queryResult.parameters.year) > DateTime.Now.Year)
            {
                return "Sorry, we don't have reports for year " + request.queryResult.parameters.year;
            }
            else
            {
                if (request.queryResult.parameters.code != "")
                {
                    return GiveURL(request);
                }
                else
                {
                    return "Alright, we will now find reports for year " + request.queryResult.parameters.year + ". If you know the code for this report, you can tell me that and the year. If not, we can search reports.";
                }
            }
        }

        public string GiveURL(ApiAiRequest request)
        {
            if (request.queryResult.parameters.year != "" && request.queryResult.parameters.code != "" && request.queryResult.parameters.code.ToUpper() != "SUMMARY")
            {
                string url = "http://datareports.lsu.edu/Reports.aspx?yr=" + request.queryResult.parameters.year + "&rpt=" + request.queryResult.parameters.code + "&p=ci";
                return "Here is your URL: " + url;
            }
            else if (request.queryResult.parameters.year != "" && request.queryResult.parameters.code != "" && request.queryResult.parameters.code.ToUpper() == "SUMMARY")
            {
                string url = "http://datareports.lsu.edu/Reports/TrafficReports/" + request.queryResult.parameters.year + "/Summary/Summary.asp";
                return "Here is your URL: " + url;
            }
            else if (request.queryResult.parameters.year != "")
            {
                return "What is the code of this report?";
            }
            else if (request.queryResult.parameters.code != "")
            {
                return "What year would you like to find this report for?";
            }
            else
            {
                return "Please provide a year and report code.";
            }
        }

        public string ListTopics(ApiAiRequest request)
        {
            SQLiteConnection cn = new SQLiteConnection("Data Source=|DataDirectory|\\CRASH_LINKS.sqlite3; Version=3");
            //Once we go back to MSSS: [CRASH_LINKS].[dbo].[TRAFFIC_CMV_HEADERS]
            string query = "SELECT [REPORTHEADER], [REPORTHEADERLONG] From TRAFFIC_CMV_HEADERS";
            SQLiteCommand cmd = new SQLiteCommand(query, cn);
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            DataTable tbl = new DataTable();

            try
            {
                cn.Open();
                da.Fill(tbl);
                cn.Close();
            }
            catch(Exception ex)
            {
                return "Error connecting to database.";
            }
            string s = "Here are the topics:";
            for (int i = 0; i < tbl.Rows.Count; i++)
            {
                s += Environment.NewLine + tbl.Rows[i].ItemArray[0] + "- " + tbl.Rows[i].ItemArray[1];
            }

            return s;
        }

        public string ListByTopic(ApiAiRequest request)
        {
            SQLiteConnection cn = new SQLiteConnection("Data Source=|DataDirectory|\\CRASH_LINKS.sqlite3; Version=3");
            string query = "SELECT [REPORTLETTER],[SUBHEADERNUM],[POSTNUMBER],[SUBHEADER] FROM TRAFFIC WHERE [REPORTLETTER] = '" + request.queryResult.parameters.Topic + "' AND [ACTIVE] = 1 ORDER BY [REPORTLETTER], [SUBHEADERNUM], [POSTNUMBER]";
            SQLiteCommand cmd = new SQLiteCommand(query, cn);
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            DataTable tbl = new DataTable();

            try
            {
                cn.Open();
                da.Fill(tbl);
                cn.Close();
            }
            catch (Exception ex)
            {
                return ex.InnerException.Message;
            }
            string s = "Here are the reports in this topic:";
            for (int i = 0; i < tbl.Rows.Count; i++)
            {
                s += Environment.NewLine + tbl.Rows[i].ItemArray[0] + tbl.Rows[i].ItemArray[1] + tbl.Rows[i].ItemArray[2] + "- " + tbl.Rows[i].ItemArray[3];
            }

            return s;
        }

        public string ListByKeyword(ApiAiRequest request, bool and)
        {
            SQLiteConnection cn = new SQLiteConnection("Data Source=|DataDirectory|\\CRASH_LINKS.sqlite3; Version=3");
            string query = KeywordQueryBuilder(request, and);
            SQLiteCommand cmd = new SQLiteCommand(query, cn);
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            DataTable tbl = new DataTable();

            cn.Open();
            da.Fill(tbl);
            cn.Close();

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

        private string KeywordQueryBuilder(ApiAiRequest request, bool and)
        {
            string query = "SELECT [REPORTLETTER],[SUBHEADERNUM],[POSTNUMBER],[SUBHEADER] FROM TRAFFIC WHERE (";
            List<string> keywords = request.queryResult.parameters.KeyWords;
            for(int i = 0; i < keywords.Count(); i++)
            {
                if (i == 0)
                {
                    query += "[KEYWORDS] LIKE '%" + keywords[i] + "%'";
                }
                else
                {
                    query += (and ? " AND [KEYWORDS] LIKE '%" + keywords[i] + "%'" : " OR [KEYWORDS] LIKE '%" + keywords[i] + "%'");
                }
            }
            query += ") AND [ACTIVE] = 1 ORDER BY[REPORTLETTER], [SUBHEADERNUM], [POSTNUMBER]";
            return query;
        }

        public string Query(ApiAiRequest request)
        {
            SqlConnection cn = new SqlConnection("Data Source=dev-sqlsrv;Initial Catalog=CRASHDWHSRG;Integrated Security=true");
            string query = QueryQueryBuilder(request);
            SqlCommand cmd = new SqlCommand(query, cn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable tbl = new DataTable();

            cn.Open();
            da.Fill(tbl);
            cn.Close();

            string s = "The result is ";
            s += tbl.Rows[0].ItemArray[0];
            return s;
        }

        private string QueryQueryBuilder(ApiAiRequest request)
        {
            string query = "SELECT COUNT(*) FROM " + request.queryResult.parameters.Table + " WHERE ";
            List<string> conditions = new List<string>();
            switch(request.queryResult.parameters.Table)
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
            for (int i = 0; i < conditions.Count(); i++)
            {
                query += conditions[i] + " AND ";
            }
            query += request.queryResult.parameters.Table.Replace("Fact", "") + "Origin = '" + request.queryResult.parameters.year + "'" ;
            return query;
        }
    }
}