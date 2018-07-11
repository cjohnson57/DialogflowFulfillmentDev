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
                return "What year would you like to find this report from?";
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
                    return "Alright, we will now find reports for year " + request.queryResult.parameters.year + ". If you know the code for this report, you can tell me that. If not, we can search reports.";
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
            SqlConnection cn = new SqlConnection("Data Source=dev-sqlsrv;Initial Catalog=CRASH_LINKS;Integrated Security=True");
            string query = "SELECT [REPORTHEADER], [REPORTHEADERLONG] From [CRASH_LINKS].[dbo].[TRAFFIC_CMV_HEADERS]";
            SqlCommand cmd = new SqlCommand(query, cn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable tbl = new DataTable();

            try
            {
                cn.Open();
                da.Fill(tbl);
                cn.Close();
            }
            catch(Exception ex)
            {
                return ex.InnerException.Message;
            }
            string s = "Here are the topics:";
            for(int i = 0; i < tbl.Rows.Count; i++)
            {
                s += Environment.NewLine + tbl.Rows[i].ItemArray[0] + "- " + tbl.Rows[i].ItemArray[1];
            }

            return s;
        }

        public string ListByTopic(ApiAiRequest request)
        {
            SqlConnection cn = new SqlConnection("Data Source=dev-sqlsrv;Initial Catalog=CRASH_LINKS;Integrated Security=True");
            string query = "SELECT [REPORTLETTER],[SUBHEADERNUM],[POSTNUMBER],[SUBHEADER] FROM [CRASH_LINKS].[dbo].[TRAFFIC] WHERE [REPORTLETTER] = '" + request.queryResult.parameters.Topic + "' AND [ACTIVE] = 1 ORDER BY [REPORTLETTER], [SUBHEADERNUM], [POSTNUMBER]";
            SqlCommand cmd = new SqlCommand(query, cn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
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

        public string ListByKeyword(ApiAiRequest request)
        {
            SqlConnection cn = new SqlConnection("Data Source=HSRG-100N5\\TESTSERVER;Initial Catalog=CRASH_LINKS;Integrated Security=True");
            string query = KeywordQueryBuilder(request);

            SqlCommand cmd = new SqlCommand(query, cn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable tbl = new DataTable();

            cn.Open();
            da.Fill(tbl);
            cn.Close();

            string s = "Here are the reports that contain this phrase:";
            for (int i = 0; i < tbl.Rows.Count; i++)
            {
                s += Environment.NewLine + tbl.Rows[i].ItemArray[0] + tbl.Rows[i].ItemArray[1] + tbl.Rows[i].ItemArray[2] + "- " + tbl.Rows[i].ItemArray[3];
            }

            return s;
        }

        private string KeywordQueryBuilder(ApiAiRequest request)
        {
            string query = "SELECT [REPORTLETTER],[SUBHEADERNUM],[POSTNUMBER],[SUBHEADER] FROM[CRASH_LINKS].[dbo].[TRAFFIC] WHERE (";
            List<string> keywords = dumbfunction(request);
            for(int i = 0; i < keywords.Count(); i++)
            {
                if (i == 0)
                {
                    query += "[KEYWORDS] LIKE '%" + keywords[i] + "%'";
                }
                else
                {
                    query += " OR [KEYWORDS] LIKE '%" + keywords[i] + "%'";
                }
            }
            query += ") AND [ACTIVE] = 1 ORDER BY[REPORTLETTER], [SUBHEADERNUM], [POSTNUMBER]";
            return query;
        }

        private List<string> dumbfunction(ApiAiRequest request)
        {
            List<string> keywords = new List<string>();
            if (request.queryResult.parameters.KeyWord != "")
            {
                keywords.Add(request.queryResult.parameters.KeyWord);
            }
            if (request.queryResult.parameters.KeyWord1 != "")
            {
                keywords.Add(request.queryResult.parameters.KeyWord1);
            }
            if (request.queryResult.parameters.KeyWord2 != "")
            {
                keywords.Add(request.queryResult.parameters.KeyWord2);
            }
            if (request.queryResult.parameters.KeyWord3 != "")
            {
                keywords.Add(request.queryResult.parameters.KeyWord3);
            }
            if (request.queryResult.parameters.KeyWord4 != "")
            {
                keywords.Add(request.queryResult.parameters.KeyWord4);
            }
            if (request.queryResult.parameters.KeyWord5 != "")
            {
                keywords.Add(request.queryResult.parameters.KeyWord5);
            }
            if (request.queryResult.parameters.KeyWord6 != "")
            {
                keywords.Add(request.queryResult.parameters.KeyWord6);
            }
            if (request.queryResult.parameters.KeyWord7 != "")
            {
                keywords.Add(request.queryResult.parameters.KeyWord7);
            }
            if (request.queryResult.parameters.KeyWord8 != "")
            {
                keywords.Add(request.queryResult.parameters.KeyWord8);
            }
            if (request.queryResult.parameters.KeyWord9 != "")
            {
                keywords.Add(request.queryResult.parameters.KeyWord9);
            }
            return keywords;
        }
    }
}