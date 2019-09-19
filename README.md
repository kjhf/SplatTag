# SplatTag
Player and Team database for lookup of clan tags and teams.

SplatTag is built for .NET Core and so can be run on Windows, macOS, Linux, and from Console.

The solution is split into 3 parts:
- Console, which can drive the SplatTagController and its database.
- The Database, which can be substituted for an actual database in the future.
- The Core, which is the business logic for matching.

## TODOs
- Near-matching of names, e.g. ¡g should be equivelant to ig. Very useful for tags.
- Fetching of data from a local file.
- Fetching of data from a remote file.
- Command line arguments to pipe a matched result elsewhere and automated fetching of data.
