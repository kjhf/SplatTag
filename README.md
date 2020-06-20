# SplatTag
Player and Team database for lookup of clan tags and teams.

SplatTag is built for .NET Core and so can be run on Windows, macOS, Linux, and from Console.

The solution is split into 3 parts:
- Console/UI, which can drive the SplatTagController and its database. 
  - The Console is an optional basic interface and is built for .NET Core. 
  - The UI is a WPF app (.NET Framework), so your mileage on other OS's may vary.
- The Database, which at present is a json serializer. The database can be substituted for an actual database in the future.
- The Core, which is the business logic for matching.

There's also unit tests, because TDD is important.

## Currently supported importers
Importers can be found under SplatTagCore.Importers.
- LUTIJsonReader: Reads the LUTI [signups sheet](https://docs.google.com/spreadsheets/d/1C7-iJlJjN3cYWEQE5hq2Y_AG4Meg4DrA5Q_527wfl_o/edit#gid=0). Convert from Google Sheets to [.csv then to .json](https://www.csvjson.com/csv2json).

## TODOs
- UI for mobile devices
- Import from csv
- Fetching of data from a remote site, e.g. SplatNet, sendou.ink
- Command line arguments to pipe a matched result elsewhere and automated fetching of data.
- Parsing of NNIDs if available to link player names
- Twitter and Discord handles
- A nice installer

## Licensing
This code is supplied with GNU General Public License v3.0.

## Contact
- https://twitter.com/kjhf1273
- Create an issue or PR on this repo (https://github.com/kjhf/SplatTag)
