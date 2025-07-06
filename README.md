# Docker Compose Lab

This repo is a lab for me to get more hands-on with Docker with .NET, and especially Docker Compose. The
ultimate goal is to experience what it would take to write a microservices architecture that could be developed locally
in a disconnected environment, and that could be used to debug the system as a whole. More specifically:

- Add a number of common services to a Docker Compose to gain experience setting those up and with Docker Compose
  itself.
- Locally, be able to develop a .NET API that has all of its dependencies `docker compose`-ed, especially if one of
  those dependencies is another .NET API that I have built in another repo.
- Locally, be able to kick off the entrypoint microservice and easily run, and debug, any part of the microservice sea
  as needed.
- Understand how to easily swap out mocked database images with "real", deidentified database images to allow for more
  robust local debugging, regardless of the depth of that database within the microservice sea.

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

## Dependencies between compose projects

To best manage dependencies between compose projects, `include` is a powerful instruction that recursively merges
many composes together, which imposes some constraints and conventions (some of which gets more awkward since compose
services cannot use variables in their names):

1. Don't expose ports for services w/in any compose, and have services talk through their shared DNS
1. Make sure all compose resources (services, volumes, networks, etc) names are globally unique. This is a lil
  awkward, but by far the least awkward choice that exists. Prob want to just prefix all resources with the
  repo/solution name
   - Similarly, make all environment variable names unique since they are all loaded into a shared shell; otherwise,
     here be dragons.
1. When making the compose service for the app itself (more on that below), be sure to add dependencies on all of the
   relevant services, and if the app has a healthcheck API, check that as well.
1. Recall that `include` supports a file path, a Git URL, or an OCI URL. As such, the default choice should be a Git URL
   or OCI URL so that dependencies can be managed by Docker and not by developers. However, if it is desirable to be
   able to debug a dependee, variable interpolation can be used in the `include` list elements to default to the Git/OCI
   URL while allowing for easy opting into running a local copy; here is an example, where if env var
   `IDCARDAPI_INCLUDE_OVERRIDE` is not set or is blank, then the URL will be used instead

   ```yml
   include:
     - ${IDCARDAPI_INCLUDE_OVERRIDE:-https://www.fake.com/sawyerwatts/idcardapi.git/compose.yml}
   ```

Here are some misc background tidbits:

- When running `compose up`, a `compose.override.yml` can be used to add overrides to the compose. Critically, only the
  top-level compose's override file will be read, even if dependee composes also define override files.
- When using multiple composes, dry runs are very much your friend here:

    ```shell
    sdocker compose up --dry-run
    ```

With these constraints in mind, I see two broad conventions to implement a multi compose environment, and the deciding
distinction would be whether it is agreeable to do the .NET development within a docker container itself. Although if
there is no dependent app/compose (and there never will be a dependent app/compose), then it doesn't really matter.

This assumes a .NET app architecture with a solution, and a subdirectory for an API project.

There will be two sections per convention: one section detailing how to configure the repo for isolated development,
and one section detailing how to debug this repo when running the system as a whole.

### Developing inside a container

#### Development Setup

1. In the root of the repo, create a `compose.yml` with the necessary dependencies (including `include`s to Git URLs).
1. In the root of the repo, create a dockerfile for the API, and add that service to `compose.yml`.
   - Add all the dependencies to this API service.
   - If the API has a healthcheck, add that to the compose service as well.
   - TODO: whatever needs to be done for dev w/in a container
1. Configure the API's `appsettings.Development.json` to use connection strings, host names, and ports to go through
   `compose.yml`'s DNS (AKA the hostname is the compose service name).
1. In the root of the repo, create a `compose.override.yml` which exposes the ports on whatever you would like to
   access from the host machine, like a PGAdmin webpage, swagger webpage, or a DB to query.

#### System Debugging

1. When this API is a dependee, to debug this API, set the `*_INCLUDE_OVERRIDE` environment variable to the file path of
   the cloned `compose.yml`.
   - TODO: is it really this easy? prob need to start it first tho or something

### Developing outside a container

#### Development Setup

1. In the root of the repo, create a `compose.yml` with the necessary dependencies (including `include`s).
1. In `compose.yml`, create a compose service named `dependencies` that has dependencies on all the dependencies.
1. In the root of the repo, create a `compose.override.yml` that exposes the ports of the dependencies to the host
   machine.
1. Configure the API's `appsettings.Development.json` to use connection strings, host names, and ports that access the
   host machine's ports that were just exposed.
1. In the root of the repo, create a dockerfile for the API, and add that service to `compose.yml`.
   - The compose service should have dependencies on all the dependencies.
   - Within the dockerfile, have a flag to set environment variables for the final image, and these variables should be
     the connection strings, host names, and ports to have the containerized app.

TODO: finish this and test it

#### System Debugging

TODO: this

### Unorganized

1. Make the dependee compose
1. Make the dependent compose, and use [include](https://docs.docker.com/compose/how-tos/multiple-compose-files/include)
   to bring in the dependee compose's Git URL (or the relative disk path when then composes are in the same repo).
   Recall that OCIs can be used as well, if the dependee compose has been published there.
1. Overrides can be used for a variety of reasons (ideally via a global override from the dependent project that starts
   the compose):
   1. To run a local version of the dependee, use overrides to use a local compose path instead of the Git URL 
   1. To use a real DB image for better debugging, use overrides as well
   1. To expose service ports that are desired, like a swagger webpage or DB or pgadmin, use overrides as well

### TODO

- Actually impl both styles, and make actual repos so can check that each style works in isolation, cumulatively, and
  when debugging dependees.
    - will prob need to have env vars to allow for overriding (use default if var is unset or blank)
      `${IDCARDAPI_COMPOSE_INCLUDE_PATH_OVERRIDE:-../IdCardApi/compose.yml}`
- how seeding when using containerized DB: have a supplement `.env.db` to override all db image URLs and the bool
  controlling seeding
- what if a compose makes an local network? are its services still in the default network too?
- Does merge use local or a global compose proj name(s)?
- if had a shared service, like event hub, how best do locally and globally? just put in producer?
- Have a local override to add all port exports and have overrides for just connex strs?
- how best code/debug w/in container? [docker compose watch](https://docs.docker.com/compose/how-tos/file-watch/)?
- When all done, write a wiki article on all this?
- Compose Extend a common SVC from git URL?

## Misc Links

Here're a buncha links to read through.

- [Best practices](https://docs.docker.com/build/building/best-practices/)
- [docker compose](https://docs.docker.com/compose/)
- [manuals](https://docs.docker.com/manuals/)
- [docker docs](https://docs.docker.com/)
- [dockerfile ref](https://docs.docker.com/reference/dockerfile/)

