using System;
using System.Collections.Generic;
using System.Web.Http;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{

    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value3" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        //POST is the only API function I use since it's the one Dialogflow uses.
        [Authorize]
        public IHttpActionResult Post([FromBody]ApiAiRequest request)
        {
          string intent = request.queryResult.intent.displayName;
            FulfillmentFunctions ff = new FulfillmentFunctions();
            //try
            //{
                switch (intent)
                {
                    case "FindReport":
                        return Respond(ff.FindReport(request), false);
                    case "FindReport.code":
                        return Respond(ff.GiveURL(request), false);
                    case "FindReport.year":
                        return Respond(ff.CheckYear(request), false);
                    case "SearchReport.topics":
                        return Respond(ff.ListTopics(request), false);
                    case "SearchReport.topics.search":
                        return Respond(ff.ListByTopic(request), true);
                    case "SearchReport.keywords.search":
                        return Respond(ff.ListByKeyword(request, true), true);
                    case "Query.People.conditions":
                        return Respond(ff.Query(request), false);
                    case "Query.Crashes.conditions":
                        return Respond(ff.Query(request), false);
                    case "Query.Vehicles.conditions":
                        return Respond(ff.Query(request), false);
                }
            //}
            //catch { }

            return Respond("Hello World", false);
        }

        //This function creates the response to send back to Dialogflow.
        public IHttpActionResult Respond(string responsetext, bool links)
        {
            if(links)
            {
                string[] lines = responsetext.Split(Environment.NewLine.ToCharArray()[0]);
                KommunicateResponse rs = new KommunicateResponse
                {
                    message = lines[0],
                    platform = "kommunicate",
                    metadata = new Metadata()
                    {
                        contentType = "300",
                        templateId = "3",
                        payload = new List<Payload>()
                    }
                };
                for (int i = 1; i < lines.Length; i++)
                {
                    string code = lines[i].Split('-')[0].Remove(0, 1);
                    Payload pl = new Payload
                    {
                        type = "link",
                        url = "http://datareports.lsu.edu/Reports.aspx?yr=" + DateTime.Now.Year + "&rpt=" + code + "&p=ci",
                        name = lines[i].Remove(0, 1)
                    };
                    rs.metadata.payload.Add(pl);
                }
                //return Json(rs);
                ApiAiResponse response = new ApiAiResponse
                {
                    fulfillmentText = responsetext,
                    outputContexts = new List<OutputContext>()
                };
                return Json(response);
            }
            else
            {
                ApiAiResponse response = new ApiAiResponse
                {
                    fulfillmentText = responsetext,
                    outputContexts = new List<OutputContext>()
                };
                return Json(response);
            }            
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
