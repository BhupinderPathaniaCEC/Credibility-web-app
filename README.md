# Credibility-web-app
A modern, full-stack web application that allows authenticated users to discover, rate, and review websites. This project demonstrates a secure implementation of ASP.NET Core Web API and Angular, focusing on robust authentication and reactive UI patterns

## 📖 Documentation
Welcome to the project! Please review the following guides before starting:
* [🚀 Developer Onboarding Checklist](./docs/onboarding.md) - Setup your local environment.
* [🔄 Working Agreements & Workflow](./docs/workflow.md) - Branching, PRs, and Definition of Done.


## High-level Architecture Diagram
Detailed architecture documentation can be found here: [docs/architecture.md](./docs/decision-records/ADR-001-architecture.md).

## Main Features
* **User Registration & Auth**: Secure onboarding via OpenIddict.
* **Website Credibility**: Real-time scoring based on accuracy, transparency, and trust.
* **Category Management**: Organized browsing of domains.

## Tech Stack
* **Backend**: .NET 10.0 Web API
* **Frontend**: Angular (located in `ui/credibility-ui`)
* **Auth**: OpenIddict (OAuth2/OIDC)
* **Database**: SQL Server / EF Core

## Getting Started

### Prerequisites
* .NET 10 SDK
* Node.js & npm
* SQL Server
* WSL2
* Ubuntu Distribution

### Setup and Run
Use the included `Makefile` to automate the setup:

1.  **Install Dependencies**:
    ```bash
    make setup-dependencies
    ```
2.  **Build the Project**:
    ```bash
    make build
    ```
3.  **Run Locally**:
    **Backend:** [Technical Setup Guide](src/README.md)
    * Frontend Setup: See ui/README.md for Angular UI instructions.

## Folder Structure
Refer to the [Project Structure Map](./docs/workflow.md) for a full tree of the `src/` and `ui/` directories.

## Documentation Links
* [API Specification (OpenAPI)](./docs/team/openapi.yml)
* [Registration API Design](./docs/qms/registration-design.md)
* [Database Schema](./docs/decision-records/ADR-002-database.md)

## License
MIT License

[def]: ./src/README.md