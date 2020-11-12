# Change Log

## In-Dev Releases

### v0.0.21
* Reworked internal id structure
* Reworked merging

### v0.0.20
* Reworked matching to show higher relevance results first.
* Added capability of filtering the results based on what you are looking for (FilterOptions)
* Improved FC parser

### v0.0.19
* Stat.ink compatibility

### v0.0.18
* Reworked Merger to use persistent information only.
* Sources.yaml now accepts relative path
* Split out BattlefySlugs, Usernames, and Twitch
* Improved context menus and now copies its info on click.

### v0.0.17
* Added Twitter buttons.
* Stability improvements.

### v0.0.16
* Added Tab Separated Values (TSV) Importer to allow importing of early LUTI data
* Final merge after database has been built to merge known entries. This should prevent cases where names that were previously not linked because of missing information become linked.
* Transformed input to also match special characters. This fixes a bug where typing accented characters causes no results.
* Minor text changes

### v0.0.15
* Added Sendou data and rejigged UI details to show this detail.
* Improved merging of known records.

### v0.0.14
* Now works off of JSON database snapshots to drastically improve startup and user experience
* Removed db classes in favour of purely JSON
* Division is now a single class for de/serialization purposes

### v0.0.13
* Better div support - makes French Leagues integration ready.
* Better sameness detection by matching names, Switch FCs, and Discord.

### v0.0.12
* Added command line arguments. 
* Added slider (not shown) for debug of smooth delay param

### v0.0.11
* Added Twitter link capability. "Twitter.json" can be loaded in dictionary form with team-name as key and twitter link as value.

### v0.0.10
* Supports InkTV. Corrected bugs around incomplete tournament data. Now loads all the known sources.
* Pulls friend codes from usernames. 
* Added a browse button to the sources loader.
* Added file browser to the sources

### v0.0.09
* Stability improvements and NRE fixes
* Now searches Discord tags and FCs
* Now able to add an entire folder to the database

### v0.0.08
* Reimplemented internal ids system.
* Improved sameness detection of Player and Team names.
* Support for Battlefy (LowInk), including additional Player information that it gives.
* Fixed a database editor crash.
* Added this change log.

### v0.0.07
* Added sources data on hover. 

### v0.0.06
* Smooth UI searching, reduced lag for small searches. 
* Minor framework changes for Android support.

### v0.0.05

#### v0.0.05.2
* Added icon.

#### v0.0.05.1
* Small improvement to handling of blank search. 

#### v0.0.05.0
* Added multiple database GUI and merging of players and teams. Lookup is now available from multiple sources.

### v0.0.04
* Added support for LUTI season X. 

### v0.0.03
* Added expander for teams to show player information on the team.

### v0.0.02
* Added empty search option
* Added versioning
* Added automatic scroll bars to outputs

### v0.0.01
* Initial release 

