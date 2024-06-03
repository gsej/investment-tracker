# SampleData folder

This folder contains sample input files. It is used if the `dataFolder` and `priceFolder` values in the
config files are left blank.

## Top level files

### AccountsStatements folder

This folder contains one folder per investment account, the name of the folder should be the AccountCode of the account. 
Each of these folders should contain CSV files. At the moment
the only files supported are the transaction statement downloads, and cash statement downloads from AjBell.

The `AjBellParserConsole` application will process these files and create a `transactions.json` file and `cashstatement_items.json` file
in each folder. 

### The prices folder

This folder contains json files which describe prices of stocks. The internal structure of the folder
doesn't matter - all json files will be processed recursively. 

### Top level files.

The two top level files are `accounts.json` which describes the different accounts (i.e. investment accounts)
which are in use. This file is used by the `LoaderConsole` application to populate the `Accounts` table in the database.

The `stocks.json` file contains a list of stocks. Again, this is used by the `LoaderConsole` to populate the `Stocks` table (and a couple of associated tables).

Once these files have been consumed, the `LoaderConsole` will import the `transactions.json` and `cashstatement_items.json` files
from those folders in `AccountsStatemnts` which correspond to accounts in `accounts.json`.

Likewise, it will import prices from the `Prices` folder which correspond to stocks in the `stocks.json` file.

## Specifying a different folder

If you want to use a different folder, you can specify a `dataFolder` in the appsettings.json file
in the `AjBellParserConsole` and `LoaderConsole` projects. This will be used as an alternative root folder instead of `SampleData`.

If you want to use a different folder for prices, you can also specify a `priceFolder` in the appsettings.json file for the `LoaderConsole` project. If not, it will assume 
the prices are in a `Prices` folder under the `dataFolder`.

This mechanism is indended to allow you to keep your private data outside of the repository.

