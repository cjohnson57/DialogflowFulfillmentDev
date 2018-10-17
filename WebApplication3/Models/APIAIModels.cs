using System.Collections.Generic;

#pragma warning disable IDE1006 // Naming Styles

namespace WebApplication3.Models
{
    //These classes were generated using http://json2csharp.com/, based on the json files sent and received by Dialogflow.

    //This is the class for the json sent by Dialogflow to this application.
    public class ApiAiRequest
    {
        public string responseId { get; set; }
        public QueryResult queryResult { get; set; }
        public OriginalDetectIntentRequest originalDetectIntentRequest { get; set; }
        public string session { get; set; }
    }

    //This is the much simpler class for the json this application sends back to Dialogflow.
    public class ApiAiResponse
    {
        public string fulfillmentText { get; set; }
        public List<OutputContext> outputContexts { get; set; }
    }

    //These parameters are defined in the Dialogflow console
    public class Parameters
    {
        public string year { get; set; }
        public string year1 { get; set; }
        public string year2 { get; set; }
        public string code { get; set; }
        public string Topic { get; set; }
        public List<string> KeyWords { get; set; }
        public string Table { get; set; }
        public List<string> PersonConditions { get; set; }
        public List<PersonConditionIntVar> PersonConditionIntVar { get; set; }
        public List<string> CrashConditions { get; set; }
        public List<CrashConditionIntVar> CrashConditionIntVar { get; set; }
        public List<string> VehicleConditions { get; set; }
        public List<VehicleConditionIntVar> VehicleConditionIntVar { get; set; }
    }

    public class Text
    {
        public List<string> text { get; set; }
    }

    public class FulfillmentMessage
    {
        public Text text { get; set; }
    }

    public class PersonConditionIntVar
    {
        public double number { get; set; }
        public string PersonConditionInt { get; set; }
        public string Inequality { get; set; }
    }

    public class CrashConditionIntVar
    {
        public double number { get; set; }
        public string CrashConditionInt { get; set; }
        public string Inequality { get; set; }
    }

    public class VehicleConditionIntVar
    {
        public double number { get; set; }
        public string VehicleConditionInt { get; set; }
        public string Inequality { get; set; }
    }

    public class OutputContext
    {
        public string name { get; set; }
        public int lifespanCount { get; set; }
        public Parameters parameters { get; set; }
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
        public string type { get; set; }
        public string url { get; set; }
        public string name { get; set; }
    }

    public class OriginalDetectIntentRequest
    {
        public Payload payload { get; set; }
    }

    //This is for sending a response to Kommunicate that has links.
    public class KommunicateResponse
    {
        public string message { get; set; }
        public string platform { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Metadata
    {
        public string contentType { get; set; }
        public string templateId { get; set; }
        public List<Payload> payload { get; set; }
    }
}