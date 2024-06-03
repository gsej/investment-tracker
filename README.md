
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

Other prices are pulled from various sources, ususally in JSON form. The code to do this sits in the other repository and the price data sits in a separate private repo. 

The prices are collected as close of day prices, and as they are from different sources, there are often duplicates. Later when loading data, we take the first price encountered, and log warnings if we spot descrepancies that exceed 1%. This provides a certain quality control. 


## Code to clean the input data

To follow....

## The dotnet folder

### LoaderConsole

This solution contains a console application called LoaderConsole. It deletes and recreates a SQL datbase and loads all account and price data (and some others) 

### Api

The Api project is a dotnet web api which allows requests to be run on the DB. 

### Web-angular

This is a front end to present the data. It's not great right now....

### Others

There are other library projects used by the Api and LoaderConsole

Python programs to parse the cash statements and transaction history files downloaded from youinvest.



## Below here needs cleaning up...


## Instructions

### Parsing CSV

`parse_transaction_files.py` and `parse_cashstatement_files.py` take the CSV files from youinvest and emit JSON files (`transactions.json` and `cashstatement_items.json`) containing the relevant data in order.

### Create account state for a particular time

`process_transactions.py` reads the `transactions.json` file and outputs the securities held in an account at a particular time (currently it reads them all so produces the latest state).

`process_cashstatement_items.py` does the same, producing the cash balance.  
