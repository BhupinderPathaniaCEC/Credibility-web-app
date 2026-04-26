# Deployment helpers

This directory contains deploy helpers for GitHub Actions and Docker Compose.

## GitHub Actions

- `.github/workflows/deploy.yml` runs after the `.NET Core CI` workflow completes successfully on `main`.
- It builds the Docker image and then deploys to a remote host if the required secrets are set.

## Required secrets for remote deployment

- `DEPLOY_HOST`
- `DEPLOY_USER`
- `DEPLOY_SSH_KEY`
- `DEPLOY_PATH`

## Environment variables used by docker-compose

- `DEFAULT_CONNECTION` (example: `Server=db;Database=CredibilityDb;User=sa;Password=YourStrongPassword`)
- `SA_PASSWORD`
- `JWT_KEY`
- `JWT_ISSUER`
- `JWT_AUDIENCE`
- `AUTH_DB_CONNECTION`
- `JWT_SECRET`
- `CORS_ORIGIN`

## Local commands

Use the Makefile targets added for local Docker testing:

```bash
make docker-build
make docker-up
make docker-down
```
