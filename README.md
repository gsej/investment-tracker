
# Overview

This is a project to analyse information about my investment accounts.

## Quality

The account data is accurate.

It does not however include the values of each account at particular times - for this we need stock prices. Collecting this data is tricky, so there's some quality built in. If we calculate the value of an account at a particular time, we need the prices, and sometimes they are missing, so I include the age of prices or whether they are missing in tte output data.

## Inputs

### Account data

The inputs to the process are downloaded CSV files from ajbell. They represent stock transactions (buying and selling stocks) and cash transactions (deposits, withdrawals, interest, dividends, charges etc.)

The CSV files are transformed into JSON files as a first stage in processing. 

This data is private and so is in a separate private repository. 

There are sample files in the main repo. 

### Price data

Price information for securities is from a variety of sources. Some historical data is downloaded from various sites usually in CSV format, and then manually converted into JSON.

Other prices are pulled from various sources, ususally in JSON form. The code to do this sits in the other repository and the price data sits in a separate private repo. Some sample price data exists in this repo. 

The prices are collected as close of day prices, and as they are from different sources, there are often duplicates. Later when loading data, we made load multiple prices for the same security 
and date. This needs to be handled at the point of querying. Initially I filtered out duplicates on import, warning if there were large differences in price, but this made the import unacceptably slow.

## The code folder

This contains C# code arranged in various projecfts.

### AjBellParserConsole

This is a console app which reads CSV files downloaded from AjBell from the SampleData folder and 
converts the data to json files for use in later stages.

### LoaderConsole

This is a console app which deletes and recreates a SQL database, and then loads a variety of data (located by default in the SampleData folder, but which can be configured to look for the data elsewhere). 

Data loaded includes:
* a list of investment accounts.
* a list of the stocks we are interested in.
* statements relating to the investment accounts (in json form, prepared by the AjBellParserConsole).
* prices for the stocks we are interested in.
* exchange rates.

### Api

The Api project is a dotnet web api which allows requests to be run on the DB. 

### Web-angular

This is a front end to present the data. It's not great right now....

### Others

There are other library projects used by the Api and LoaderConsole