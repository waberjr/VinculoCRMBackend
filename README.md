# VinculoBackend

The project was generated using the [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture) version 10.8.0.

## Build

Run `dotnet build` to build the solution.

## Run

To run the application:

```bash
dotnet run --project .\src\AppHost
```

The Aspire dashboard will open automatically, showing the application URLs and logs.

## Entity Framework Core

Run EF commands from the `VinculoBackend` folder.

Install or update the EF CLI:

```bash
dotnet tool update --global dotnet-ef
```

Create a migration after changing the model:

```bash
dotnet ef migrations add NomeDaMigration --project .\src\Infrastructure --startup-project .\src\Web --output-dir Data\Migrations
```

Apply pending migrations to the configured local database:

```bash
dotnet ef database update --project .\src\Infrastructure --startup-project .\src\Web
```

List migrations:

```bash
dotnet ef migrations list --project .\src\Infrastructure --startup-project .\src\Web
```

Generate an idempotent SQL script:

```bash
dotnet ef migrations script --idempotent --project .\src\Infrastructure --startup-project .\src\Web --output .\artifacts\migrations.sql
```

Do not create migration files manually. Generate them with the EF CLI and review the generated code before applying it.

## Code Styles & Formatting

The template includes [EditorConfig](https://editorconfig.org/) support to help maintain consistent coding styles for multiple developers working on the same project across various editors and IDEs. The **.editorconfig** file defines the coding styles applicable to this solution.

## Code Scaffolding

The template includes support to scaffold new commands and queries.

Start in the `.\src\Application\` folder.

Create a new command:

```
dotnet new ca-usecase --name CreateTodoList --feature-name TodoLists --usecase-type command --return-type int
```

Create a new query:

```
dotnet new ca-usecase -n GetTodos -fn TodoLists -ut query -rt TodosVm
```

If you encounter the error *"No templates or subcommands found matching: 'ca-usecase'."*, install the template and try again:

```bash
dotnet new install Clean.Architecture.Solution.Template::10.8.0
```

## Test

The solution contains unit, integration, and functional tests.

To run the tests:
```bash
dotnet test
```

## Help
To learn more about the template go to the [project website](https://cleanarchitecture.jasontaylor.dev). Here you can find additional guidance, request new features, report a bug, and discuss the template with other users.
