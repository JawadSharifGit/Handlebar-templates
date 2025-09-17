# Handlebars Email Helper (ASP.NET Core)

A lightweight ASP.NET Core Web API for managing and rendering Handlebars-based email templates with partials and culture-aware helpers. It uses EF Core with SQLite for persistence and exposes a clean REST API, plus Swagger UI for exploration.

## Features
- Manage email templates (CRUD)
- Manage reusable partials (CRUD)
- Render templates with dynamic data for previewing/testing
- Culture-aware helpers for dates, numbers, currency, and text
- Simulated email sending to console (easy to swap for a real provider)
- EF Core with SQLite storage
- Swagger/OpenAPI documentation in Development

## Project Structure
- `Program.cs` — App bootstrap, DI registration, Swagger setup, database seeding
- `Controllers/` — API endpoints
  - `TemplatesApiController.cs` — Email templates + rendering + helpers info
  - `PartialsApiController.cs` — Partials CRUD
- `Services/`
  - `TemplateService.cs` — Handlebars compile/render + helpers + partials integration
  - `TemplateRepository.cs` — Data access for templates/partials (via `AppDbContext`)
  - `TemplateApplicationService.cs` — Orchestration for template rendering workflows
  - `ConsoleEmailService.cs` — Simulated email sender, holds `EmailOptions`
- `Interfaces/` — DI interfaces (boundary of services)
- `Models/` — EF Core models and `AppDbContext`
- `Dto/` — Request/response DTOs for API
- `wwwroot/` — Static assets (if serving UI assets)
- `appsettings*.json` — Configuration (see `Email` section below)

## Prerequisites
- .NET SDK 9.0+

## Getting Started
1. Restore packages
   - `dotnet restore`
2. Build
   - `dotnet build`
3. Run (Development)
   - `dotnet run`
4. Open Swagger UI (Development only)
   - Navigate to `https://localhost:5001/swagger` or `http://localhost:5000/swagger`

The app seeds the database on startup (`SeedData.InitializeAsync` in `Models/SeedData.cs`).

## Configuration
Email settings are configured via `EmailOptions` (`Services/ConsoleEmailService.cs`) and bound from configuration section `Email`:

```json
{
  "Email": {
    "FromEmail": "no-reply@example.com",
    "FromName": "Example App"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=emails.db"
  }
}
```

- `FromEmail` — Sender email address used by the simulated email service
- `FromName` — Sender display name used by the simulated email service

Note: `appsettings.Development.json` and local SQLite `*.db` files are ignored by Git (see `.gitignore`). Provide production-appropriate configuration separately.

## Database
- Provider: SQLite via EF Core
- File: `emails.db` (local dev)
- Context: `Models/AppDbContext.cs`
- Entities: `EmailTemplate`, `Partial`

## API Overview
Base path: `/api`

### Templates
- GET `/api/templates` — List all templates
  - 200 OK: `EmailTemplateDto[]`
- GET `/api/templates/{id}` — Get template by ID
  - 200 OK: `EmailTemplate`, 404 if not found
- POST `/api/templates` — Create template
  - Body: `CreateTemplateRequest { name, subject, htmlBody }`
  - 201 Created: `EmailTemplate`
- PUT `/api/templates/{id}` — Update template
  - Body: `UpdateTemplateRequest { name, subject, htmlBody }`
  - 204 No Content, 404 if not found
- DELETE `/api/templates/{id}` — Delete template
  - 204 No Content, 404 if not found
- POST `/api/templates/{id}/render` — Render template with data
  - Body: `RenderTemplateRequest { data: object, culture?: string }`
  - 200 OK: `RenderedTemplateResponse { templateId, renderedSubject, renderedHtmlBody, culture, renderedAt }`
- POST `/api/templates/{id}/send-test` — Send a test email (simulated to console)
  - Body: `SendTestEmailRequest { testData: object, toEmail: string, toName?: string, culture?: string }`
  - 200 OK: `SendTestEmailResponse { success, templateId, toEmail, sentAt, message }`
- GET `/api/templates/helpers` — Get available helpers and usage examples
  - 200 OK: `HelpersResponse`

### Partials
- GET `/api/partials` — List all partials
  - 200 OK: `Partial[]`
- GET `/api/partials/{id}` — Get partial by ID
  - 200 OK: `Partial`, 404 if not found
- GET `/api/partials/by-name/{name}` — Get partial by name
  - 200 OK: `Partial`, 404 if not found
- POST `/api/partials` — Create partial
  - Body: `CreatePartialRequest { name, htmlContent }`
  - 201 Created: `Partial`
- PUT `/api/partials/{id}` — Update partial
  - Body: `UpdatePartialRequest { name, htmlContent }`
  - 204 No Content, 404 if not found
- DELETE `/api/partials/{id}` — Delete partial
  - 204 No Content, 404 if not found

## Handlebars Helpers (examples)
- Dates
  - `{{currentDate "yyyy-MM-dd" "en-US"}}`
  - `{{formatDate dateValue "MMM dd, yyyy" "en-US"}}`
  - `{{year "en-US"}}`
- Numbers
  - `{{currency amount "en-US"}}`
  - `{{formatNumber value "N2" "en-US"}}`
  - `{{percentage value "en-US"}}`
- Text
  - `{{uppercase text "en-US"}}`, `{{lowercase text "en-US"}}`, `{{titleCase text "en-US"}}`
  - `{{htmlEncode text}}`, `{{urlEncode text}}`
- Logic
  - `{{#eq a b}}...{{/eq}}`, `{{#neq a b}}...{{/neq}}`, `{{#gt a b}}...{{/gt}}`, `{{#lt a b}}...{{/lt}}`, `{{#ifNotEmpty value}}...{{/ifNotEmpty}}`
- Partials
  - `{{> partialName}}` (e.g., `{{> header}}`, `{{> footer}}`)

## Example: Create and Render a Template
1. Create a template
   - POST `/api/templates`
   - Body:
     ```json
     {
       "name": "WelcomeEmail",
       "subject": "Welcome, {{firstName}}!",
       "htmlBody": "<h1>Hello {{firstName}}!</h1><p>Thanks for joining.</p>"
     }
     ```
2. Render the template
   - POST `/api/templates/{id}/render`
   - Body:
     ```json
     {
       "data": { "firstName": "Taylor" },
       "culture": "en-US"
     }
     ```

## Notes
- The email sender is simulated (console output). To integrate a real provider (SMTP, SendGrid, etc.), implement `IEmailService` and swap the registration in `Program.cs`.
- Swagger is enabled in Development only.

## Development Tips
- If `emails.db` or other files were previously committed before the updated `.gitignore`, untrack them with:
  - `git rm --cached emails.db emails.db-shm emails.db-wal`
  - `git rm --cached appsettings.Development.json`
  - Then commit the changes.

## License
MIT (or your chosen license)
