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
                    return "Alright, we will now find reports for year " + request.queryResult.parameters.year + ". What is the code for this report?";
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
                var url2 = "http://datareports.lsu.edu/Reports/TrafficReports/" + request.queryResult.parameters.year + "/Summary/Summary.asp";
                return "Here is your URL: " + url2;
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

        public string ListByTopic(ApiAiRequest request)
        {
            return "Hello World";
        }
    }
}