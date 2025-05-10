# Docker Funtimes

This repo is a playground for me to get more hands-on with Docker. More specifically:

- Have a .NET API that uses Docker compose to stand up dependencies (if I'm feeling spicy, then
  this'll include DB migrations).
- Have a .NET app that uses the first API as a dependency - the goal is to get familiar with
  writing a client that uses a homegrown API.

## IdCardApi

This is a (stubbed) API to build ID cards from the database, and cache the result.

```sh
sdocker compose down -v; sdocker compose up -d && sdocker compose ps && sdocker ps && sdocker volume ls && sdocker network ls
```

## IdCardJob

This is a (stubbed) console application that uses `IdCardApi` to get ID cards for the relevant
families and write them to an output file.
