# Docker Funtimes

This repo is a playground for me to get more hands-on with Docker. More specifically:

- Have a .NET API that uses Docker compose to stand up dependencies (if I'm feeling spicy, then
  this'll include DB migrations).
- Have a .NET app that uses the first API as a dependency - the goal is to get familiar with
  writing a client that uses a homegrown API.

For all the commands in this file, the following alias exists (because Docker on Linux):

```shell
alias sdocker='sudo docker'
```

Recall that the following can be used to interactively play with an image.

```shell
sdocker run -it IMAGE
```

## IdCardApi

This is a (stubbed) API to build ID cards from the database, and cache the result.

```shell
sdocker compose down -v; sdocker compose up -d && sdocker compose ps -a && sdocker ps && sdocker volume ls && sdocker network ls
```

Here's how to build the image for the API (this needs to be ran from the sln dir because Docker
appears to not like parent directory traversals):

```shell
# sdocker build -t id-card-api:$(date +%s) -f ./src/Api/Dockerfile .
sdocker build -t id-card-api:latest -f ./src/Api/Dockerfile .
```

*NOTE*: A lot of these all-in-one comands can be made simpler with `docker compose SVC -q`

Here's an all-in-one command to create the image and run it (attached).

```shell
sdocker build -t id-card-api:latest -f ./src/Api/Dockerfile . \
  && sdocker run -p "[::1]:8080:8080" -e "ASPNETCORE_ENVIRONMENT=Development" id-card-api:latest
```

Here's a helpful lil command to clean up many images within a tag:

```shell
sdocker image rm $(sdocker image ls | grep "^id-card-api" | awk -F' ' '{print $1 ":" $2}')
```

## IdCardJob

This is a (stubbed) console application that uses `IdCardApi` to get ID cards for the relevant
families and write them to an output file.

TODO: It looks like the best course of action (unless I can find some networking magic) is to:

1. Don't expose ports for services w/in a compose, and have services talk through their shared DNS
1. Develop code w/in docker image (rip)
    - Have a local override to add all port exports and have overrides for just connex strs?
    - how best do? [docker compose watch](https://docs.docker.com/compose/how-tos/file-watch/)?
1. Use [include](https://docs.docker.com/compose/how-tos/multiple-compose-files/include) to make a
   god compose that can start anything (recall that `include` can target Git repo paths, and image
   repo paths!)
   - Does anything need to be done to allow the svcs to reach across the compose networks?
   - What if there's dup service names?
1. Use overrides to use a local compose instead of the Git URL when it is desired to debug a service
   locally
1. Use overrides to add in real DB images as needed
1. Use overrides to expose ports as desired, like for specific DBs and web APIs you wanna look at

TODO: if had a shared service, like event hub, how best do? just put in producer?

## Misc Links

Here're a buncha links to read through.

- [Best practices](https://docs.docker.com/build/building/best-practices/)
- [manuals](https://docs.docker.com/manuals/)
- [docker docs](https://docs.docker.com/)
- [dockerfile ref](https://docs.docker.com/reference/dockerfile/)

