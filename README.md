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

Here's how to build the image for the API (this needs to be ran from the sln dir because Docker
appears to not like parent directory traversals):

```sh
# sdocker build -t id-card-api:$(date +%s) -f ./src/Api/Dockerfile .
sdocker build -t id-card-api:latest -f ./src/Api/Dockerfile .
```

Here's an all-in-one command to create the image and run it (attached).

```sh
sdocker build -t id-card-api:latest -f ./src/Api/Dockerfile . \
  && sdocker run -p "[::1]:8080:8080" -e "ASPNETCORE_ENVIRONMENT=Development" id-card-api:latest
```

Here's a helpful lil command to clean up many images within a tag:

```sh
sdocker image rm $(sdocker image ls | grep "^id-card-api" | awk -F' ' '{print $1 ":" $2}')
```

## IdCardJob

This is a (stubbed) console application that uses `IdCardApi` to get ID cards for the relevant
families and write them to an output file.
