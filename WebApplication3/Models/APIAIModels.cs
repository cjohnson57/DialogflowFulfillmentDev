using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Parameters
    {
        public string year { get; set; }
        public string code { get; set; }
    }

    public class Text
    {
        public List<string> text { get; set; }
    }

    public class FulfillmentMessage
    {
        public Text text { get; set; }
    }

    public class Parameters2
    {
        public string codeoriginal { get; set; }
        public string code { get; set; }
        public string year { get; set; }
        public string yearoriginal { get; set; }
    }

    public class OutputContext
    {
        public string name { get; set; }
        public int lifespanCount { get; set; }
        public Parameters2 parameters { get; set; }
    }

    public class Intent
    {
        public string name { get; set; }
        public string displayName { get; set; }
    }

    public class QueryResult
    {
        public string queryText { get; set; }
        public Parameters parameters { get; set; }
        public bool allRequiredParamsPresent { get; set; }
        public List<FulfillmentMessage> fulfillmentMessages { get; set; }
        public List<OutputContext> outputContexts { get; set; }
        public Intent intent { get; set; }
        public int intentDetectionConfidence { get; set; }
        public string languageCode { get; set; }
    }

    public class Payload
    {
    }

    public class OriginalDetectIntentRequest
    {
        public Payload payload { get; set; }
    }

    public class ApiAiRequest
    {
        public string responseId { get; set; }
        public QueryResult queryResult { get; set; }
        public OriginalDetectIntentRequest originalDetectIntentRequest { get; set; }
        public string session { get; set; }
    }

    public class ApiAiResponse
    {
        public string fulfillmentText { get; set; }
        public List<object> outputContexts { get; set; }
    }
}