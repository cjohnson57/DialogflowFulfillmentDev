# DialogflowFulfillmentDev
An application to develop a Dialogflow fulfillment webhook API for the HSRG's datareports site.

ValuesController.cs contains the API code that calls the fulfillment functions.
FulfillmentFunctions.cs contains the logic.
APIAllModels.cs contains the classes I'm using to represent DialogFlow's json data.

All other files are default asp.net new project files.

The purpose of this application is to develop the bot before in the future integrating it with datareports.lsu.edu.

Currently the bot can give a URL for a report when provided the code and year, search for reports based on topics and keywords, and build some simple queries based on conditions given by the user.

For the query functionality, here is an example phrase given by the user:

    driver males older than 25 in 2012

Which will then build this SQL query:

    SELECT COUNT(*) FROM FactPerson WHERE PersonType = 'A' AND Sex = 'M' AND Age > 25 AND PersonOrigin = '2012'
    
And return this as the result:

    Here are the conditions we considered:
    Person: Driver
    Sex: Male
    Age > 25
    In 2012
    Here is the result from those conditions:
    107514
