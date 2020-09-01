# SplatTag
Player and Team database for lookup of players, clan tags, and teams.

SplatTag is built for .NET Core and so can be run on Windows, macOS, Linux, and from Console.

The solution is split into 3 parts:
- Console/UI, which can drive the SplatTagController and its database. 
  - The Console is an optional basic interface and is built for .NET Core. 
  - The UI is a WPF app (.NET Framework), so your mileage on other OS's may vary.
  - SplatTagAndroid is an in-dev UI for Android phones.
- The Database, which at present is a json serializer. The database can be substituted for an actual database in the future.
- The Core, which is the business logic for matching.

There's also unit tests, because TDD is important.

## Currently supported importers
Importers can be found under SplatTagCore.Importers.
- LUTIJsonReader: Reads the LUTI [signups sheet](https://docs.google.com/spreadsheets/d/1C7-iJlJjN3cYWEQE5hq2Y_AG4Meg4DrA5Q_527wfl_o/edit#gid=0). Convert from Google Sheets to [.csv then to .json](https://www.csvjson.com/csv2json).
- BattlefyJsonReader: Reads Battlefy Data. Tested with Low Ink and InkTV, e.g. https://battlefy.com/low-ink//{id}/participants  https://battlefy.com/inktv//{id}/participants

## TODOs
### Features
- French League data
- Fetching of data from a remote site, e.g. SplatNet, sendou.ink
- Estimation of div based on tourney results
- Automated fetching of data.
  - Auto population of Twitter handles.
- Improve distribution process; a nice installer and updater
- Import from csv

### Bugs
- Fix bug where a player is no longer part of a team but shows as current if they have not joined a different team
- Improve sameness matching by checking FC and Discord tag.

## Licensing
This code is supplied with GNU General Public License v3.0.

## Contact
- https://twitter.com/kjhf1273
- Create an issue or PR on this repo (https://github.com/kjhf/SplatTag)
