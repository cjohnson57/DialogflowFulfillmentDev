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
        public IHttpActionResult Post([FromBody]ApiAiRequest request)
        {
          string intent = request.queryResult.intent.displayName;
            FulfillmentFunctions ff = new FulfillmentFunctions();
            //try
            //{
                switch (intent)
                {
                    case "FindReport":
                        return respond(ff.FindReport(request));
                    case "FindReport.code":
                        return respond(ff.GiveURL(request));
                    case "FindReport.year":
                        return respond(ff.CheckYear(request));
                    case "FindReport.topics":
                        return respond(ff.ListTopics(request));
                    case "FindReport.listbytopic":
                        return respond(ff.ListByTopic(request));
                    case "FindReport.listbykeyword":
                        return respond(ff.ListByKeyword(request, true));
                    case "Query.People.conditions":
                        return respond(ff.Query(request));
                    case "Query.Crashes.conditions":
                        return respond(ff.Query(request));
                    case "Query.Vehicles.conditions":
                        return respond(ff.Query(request));
                }
            //}
            //catch { }

            return respond("Hello World");
        }

        //This function creates the response to send back to Dialogflow.
        public IHttpActionResult respond(string responsetext)
        {
            ApiAiResponse response = new ApiAiResponse();
            response.fulfillmentText = responsetext;
            return Json(response);
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
