# CredibilityUi

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 21.1.2.

## Folder Structure
```
ui/
└── credibility-ui/                 # Main Angular Application
    ├── src/
    │   ├── app/
    │   │   ├── core/               # Singleton services: auth, interceptors, guards, layout
    │   │   ├── shared/             # Reusable components, pipes, and directives
    │   │   ├── features/           # Feature-based modules
    │   │   │   ├── auth/           # Login and Registration flows
    │   │   │   ├── websites/       # Website management and monitoring
    │   │   │   ├── ratings/        # Credibility rating logic and displays
    │   │   │   └── admin/          # Administrative dashboard and controls
    │   │   ├── app.routes.ts       # Main application routing definitions
    │   │   ├── app.component.* # Root component files
    │   │   └── app.config.ts       # Global application configuration
    │   ├── assets/                 # Static assets (images, icons, fonts)
    │   ├── environments/           # Environment-specific configurations (Dev/Prod)
    │   └── main.ts                 # Main entry point for the application
    ├── angular.json                # Angular CLI configuration
    ├── package.json                # Project dependencies and scripts
    ├── tsconfig.json               # TypeScript compiler configuration
    └── README.md                   # UI-specific documentation           # Main project 
```
# Credibility Index - Frontend UI

The user interface for the Credibility Index, built with Angular.

### 🚀 Local Setup
1. **Prerequisites**: 
   - [Node.js](https://nodejs.org/) (LTS)
   - [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli`)


## Development server

To start a local development server, run:

```bash
make dev-ui
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
make build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
