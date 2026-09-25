# Docker Contract

This document defines how the LicenseGuard services are built and run in containers. The current solution targets .NET 10 and uses MySQL/MariaDB through EF Core.

## Repository files

- Keep the service `Dockerfile` beside its application project, or at the repository root when the repository contains a single deployable API.
- Keep `compose.yaml` at the repository root for the local development stack.
- Set the Compose build context to the directory containing the solution and all referenced projects. Do not rely on files outside the build context.
- Use a multi-stage .NET build: the SDK image restores, builds, and publishes; the smaller ASP.NET runtime image runs the published output.
- Pin base images to an explicit supported .NET major version and update them deliberately.

## Image requirements

- Run the application as a non-root user in the runtime image.
- Use `.dockerignore` to exclude `.git`, IDE state, build output (`bin/`, `obj/`), local secrets, and unrelated files.
- Do not copy development credentials, user secrets, or environment-specific configuration into an image.
- Keep images reproducible: restore from the checked-in solution/project files and publish in Release configuration.
- Expose only the port the application listens on. Configure the URL and port through environment variables where practical.
- Provide a health endpoint and configure a container health check when the service exposes one.

## Local Compose requirements

- Compose should run the API and a MySQL-compatible database for local development.
- Persist database data in a named volume; do not store database state in the container writable layer.
- Supply database credentials through an untracked `.env` file or environment variables. Commit only a `.env.example` containing safe placeholders.
- Make the API wait for database readiness using a health check or equivalent readiness condition; container start order alone is insufficient.
- Configure the connection string using environment variables. Never commit production credentials or license secrets.
- Document database migration commands and service startup commands in the project README when those files are added.

## Runtime and operations

- The database is the source of truth for license state. Caches must not be required for correctness.
- Keep persistent data outside disposable containers and define backups separately from Compose volumes.
- Send logs to standard output/error and avoid logging license keys, passwords, connection strings, or other secrets.
- Keep development Compose overrides separate from production deployment configuration. Production secrets must come from the deployment platform's secret store.
- A container image must start with a clean environment and must not depend on files generated on a developer's machine.

## Verification before merge

For Docker-related changes, verify that the image builds from the documented context, the Compose configuration parses, and the API can reach a healthy database. Record the commands and results in the pull request.
