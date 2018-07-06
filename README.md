# DialogflowFulfillmentDev
An application to develop a Dialogflow fulfillment webhook API for the HSRG's datareports site.

ValuesController.cs contains the API code that calls the fulfillment functions.
FulfillmentFunctions.cs contains the logic.
APIAllModels.cs contains the classes I'm using to represent DialogFlow's json data.

All other files are default asp.net new project files.

The purpose of this application is to develop the bot before in the future integrating it with datareports.lsu.edu.

Currently, all the bot can do is give the URL for a report when provided the report's year and code.
In the future it's planned to be able to assist the user in finding reports based on the report's name and topic,
and later to use prepared queries for the user to ask quick statistics without looking at a report.
