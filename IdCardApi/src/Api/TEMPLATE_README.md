# Sawyer's WebApi Template

## TODO

Users of this template have some tasks to do to ensure the template is best
utilized.

### Health checks

- Replace `SampleHealthCheck` with a real health check.
- [Program.cs](./Program.cs)'s `MapHealthChecks` could use `RequireHost`
  in addition to `AllowAnonymous`.
- Ideally, have monitoring in place to periodically check that the API is still
  healthy.

### Middleware

Check the request pipeline in [Program.cs](./Program.cs) and check each of the
classes in [Middleware/](./Middleware) to ensure they are relevant for the
web API being built.

#### API Gateway Behaviors

Some middleware in this template would be best handled by an API Gateway, if it
exists. Here is the list of middleware to hopefully remove from this template:

- [ObfuscatePayloadOfServerErrors.cs](./Middleware/ObfuscatePayloadOfServerErrors.cs)
- [RateLimiting.cs](./Middleware/RateLimiting.cs)
- [RequestTimeouts.cs](./Middleware/RequestTimeouts.cs) (maybe, if you're
  feeling spicy and the environment makes sense)

#### Idempotent POSTs

If POSTs are in scope for the API, removing that middleware would be prudent.
Otherwise, the current implementation only uses an in memory cache to store its
idempotency tokens. Depending on your environment, it may be mildly to severely
critical to supplement that type with a persistance/distributed cache and/or
sticky sessions.

### Request size limiting

This template does *not* have request size limits in place. As such, it is
recommended to configure an upstream resource to enforce this limit. The
observed consencus for the rule of thumb is to not allow requests with a body
larger than 1 MB. (NGINX defaults to 1 MB and IIS defaults to 30 MB).

### Alerting

It is very likely that alerting around CPU and RAM usage is beneficial.
Additionally, when deploying on the cloud, alerting around Billing is prudent
as well.
