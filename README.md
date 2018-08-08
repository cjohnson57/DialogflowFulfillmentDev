# DialogflowWebhookAPI

The API for our CARTS dialogflow bot to make calls to.

Important files:

Controllers/ValuesController.cs; contains POST function which the API calls with

FulfillmentFunctions.cs; contains the many functions used by ValuesControllers to get data

Models/ApiAIModels.cs; contains the models which come from the Dialogflow JSONs sent and received

BasicAuthHttpModule.cs; contains authorization code 