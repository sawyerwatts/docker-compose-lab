# Docker Compose Lab

This repo is a lab for me to get more hands-on with Docker with .NET, and especially Docker Compose. The
ultimate goal is to experience what it would take to write a microservices architecture that could be developed locally
in a disconnected environment, and that could be used to debug the microservice sea as a whole (assuming there are some
direct web API invocations and not purely messaging/events between services). More specifically:

- Add a number of common services to a Docker Compose to gain experience setting those up and with Docker Compose
  itself.
- Locally, be able to develop a .NET API that has all of its dependencies `docker compose`-ed, especially if one of
  those dependencies is another .NET API that I have built in another repo.
- Locally, be able to kick off an entrypoint microservice and easily run, and debug, any upstream microservice (direct
  or indirect).
- Understand how to easily swap out mocked database images with "real", deidentified database images to allow for more
  robust local debugging, regardless of the depth of that database within the microservice sea.

Starting docker:

```shell
# Normally:
sudo systemctl start docker

# If rootless:
systemctl --user start docker
```

## IdCardApi

[This repository](https://github.com/sawyerwatts/docker-compose-lab-idcardapi) contains a dependee .NET API that has a "
lot" of containerized dependencies, and is an example of an upstream service.

## IdCardJob

This is a (stubbed) console application that uses `IdCardApi` to get ID cards for the relevant
families and write them to an output file. This is an example of a downstream service.

## Dependencies between compose projects

To be able to work with multiple compose projects, `include` is a powerful instruction that recursively merges
many composes together, which imposes some constraints and conventions (some of which gets more awkward since compose
services cannot use variables in their names). As such, here are rules for create composes with the objective of letting
any dependency be run locally so that the current service can be debugged locally (and used as a dependency later):

- [ ] Have a `Dockerfile` to build and run the API, with the goal that .NET's `Development` environment will be used to
  run the API as a local compose service (as well as copying `appsettings.Docker.json` overtop
  `appsettings.Development.json`). See the settings strategy below.
- [ ] Create a `compose.yml` with the following constraints:
    - [ ] Environment variables should be globally unique unless explicitly desired otherwise.
        - This is because environment variables are shared between all composes, so they can override each other and
          cause weird behavior.
    - [ ] Make sure all compose resources' (services, volumes, networks, etc) names are globally unique - probably just
      prefix the repo onto the names.
        - Otherwise, the `Include` can encounter colliding resource names.
    - [ ] Don't expose any ports for services w/in any compose, and instead of using `localhost`, resolve domains
      through
      Docker's DNS. Export the API's dependencies' ports in `compose.override.yml`.
        - This is because if two composes export the same port, they'll collide.
    - [ ] Create a `*_start_depencenies` service, and have it `depend_on` any `Include`-ed services as needed.
        - When running or debugging this API on the host machine, this will let us easily start the API's dependencies
          via `docker compose up -d example_start_dependencies`
    - [ ] `Include` the necessary compose files for upstream services and have `*_start_dependencies` `depend_on` every
      direct dependency of this API
    - [ ] Create each of this API's upstream services, ideally each with a healthcheck, and add each to the `depend_on`
      within the `*_start_depencenies` service
        - [ ] If a shared service is needed, like an event hub, the upstream dependee (the producer) should create it,
          and then the downstream dependee (the consumer) should use what was already created.
    - [ ] Make a service for this API's `Dockerfile`, and make it `depend_on` the `*_start_depencenies` service.
      Ideally, this API service will also have a `healthcheck`.
        - This will let downstream composes `depend_on` this API while starting up all this API's dependencies.
- [ ] Settings strategy
    - [ ] `appsettings.Docker.json` should have the settings to run the API as a compose service, so it uses
      compose's DNS to reference its dependencies via their compose service name. There should be no need or reason to
      add non-compose secrets in this file since all dependencies should be mocked.
        - [ ] If not every service and secret can be mocked, put those into a `.env` or something, my condolences.
    - [ ] `appsettings.Development.json` should have the settings to run and debug the API on the host machine, like
      using `localhost` and the ports exported by `compose.override.yml`. This should not contain non-compose secrets.
    - [ ] .NET user secrets should have the confidential settings / secrets to run and debug on the host machine,
      presumably against an actual deployed DB, since these will override `appsettings.Development.json`
    - Depending on the deployed settings strategy, the duplication between `appsettings.Docker.json` and
      `appsettings.Development.json` can be removed.

To run or debug this API locally, it should be as simple as `docker compose up -d example_start_dependencies` and then
running or debugging the API using an IDE or `dotnet run`.

### `compose.override.yml` fun facts

Here are some misc background tidbits:

- When running `compose up`, a `compose.override.yml` can be used to add overrides to the compose. Critically, only the
  top-level compose's override file will be read, even if dependee composes also define override files.
- When using multiple composes, dry runs are very much your friend here:

    ```shell
    docker compose up --dry-run
    ```

With these constraints in mind, I see two broad conventions to implement a multi compose environment, and the deciding
distinction would be whether it is agreeable to do the .NET development within a docker container itself. Although if
there is no dependent app/compose (and there never will be a dependent app/compose), then it doesn't really matter.

This assumes a .NET app architecture with a solution, and a subdirectory for an API project.

There will be two sections per convention: one section detailing how to configure the repo for isolated development,
and one section detailing how to debug this repo when running the system as a whole.

### Checking Healthcheck Logs

`docker inspect --format "{{json .State.Health }}" <container name> | jq`

### Rebuilding a dockerfile

When making changes to a local Dockerfile that's being run through Docker Compose, you'll need to apply the `--build`
flag to have Docker Compose rebuild the image.

### TODO

- build `IdCardJob` and verify everything works as expected
- direct includes vs top-level god compose of custom services
- using the direct includes model, you could debug any API without dev containers if:
    1. all services resolved to unique, deterministic host ports regardless of starting on host vs via docker compose (
       good luck)
    2. when an API is `depend_on`-ed, there is no container status check (since starting a container for an already
       running port would fail)
        - TODO: would `depend_on` fail if one of the dependencies failed to come up? may need to set `required` to false
        - TODO: maybe also use an env var to opt-out of requiring a dependency to be required?
        - alt: only start the service if the port isn't used
    3. override the include paths to ensure drift is caught b/w local and remote
- explore/doc overrides
    - [ ] Recall that `include` supports a file path, a Git URL, or an OCI URL. As such, the default choice should be a
      Git URL or OCI URL so that dependencies can be managed by Docker and not by developers. However, if it is
      desirable to be able to debug a dependee, variable interpolation can be used in the `include` list elements to
      default to the Git/OCI URL while allowing for easy opting into running a local copy; here is an example, where if
      env var `IDCARDAPI_INCLUDE_OVERRIDE` is not set or is blank, then the URL will be used instead

    1. To run a local version of the dependee, use overrides to use a local compose path instead of the Git URL
    1. To use a real DB image for better debugging, use overrides as well
        - how seeding when using containerized DB: have a supplement `.env.db` to override all db image URLs and the
          bool controlling seeding
            - what if multiple svcs use the same source db?
    1. To expose service ports that are desired, like a swagger webpage or DB or pgadmin, use overrides as well

    - will prob need to have env vars to allow for overriding (use default if var is unset or blank)
      `${IDCARDAPI_COMPOSE_INCLUDE_PATH_OVERRIDE:-../IdCardApi/compose.yml}`

   ```yml
   include:
     - ${OVERRIDE_INCLUDE_IDCARDAPI:-https://www.fake.com/sawyerwatts/idcardapi.git/compose.yml}
   ```
- how could the docker settings n compose be checked? have the ci/cd run docker compose up the api?
- what if there's a circular dependency b/w dependencies? will compose be happy?
- Make a common services for myself (put in lab and extend in api lab?)
- Make an upstream SVC for idcardapi and verify the env var hopping works as hypothesized
- Once completed lab, install onto a podman and then rancher vms to see if it works there too
- When all done, write a wiki article on all this?
