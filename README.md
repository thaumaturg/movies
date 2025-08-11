# Movies Application

A movie search application built with .NET 9 and Blazor WebAssembly that integrates with the OMDB API to provide movie search functionality and maintains search history.

## Setup

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (for Tailwind CSS)
- [PostgreSQL](https://www.postgresql.org/download/)
- [OMDB API Key](https://www.omdbapi.com/) (free registration required)

### Configuration

Update the configuration files in `Movies.API`:

**appsettings.Development.json** (for development):
```json
{
  "ConnectionStrings": {
    "MoviesDb": "Host=localhost; Port=5432; Database=movies_db; Username=your_username; Password=your_password; TimeZone=UTC"
  },
  "OMDbConfiguration": {
    "ApiKey": "your_omdb_api_key"
  }
}
```

**appsettings.json** (for production):
```json
{
  "ConnectionStrings": {
    "MoviesDb": "your_production_connection_string"
  },
  "OMDbConfiguration": {
    "ApiKey": "your_omdb_api_key"
  }
}
```

## Build Instructions

From the solution root directory:

```bash
# Restore .NET packages
dotnet restore

# Install Node.js dependencies for Blazor project
cd Movies.Blazor
npm install

# Build Solution
cd ..
dotnet build
```

## Run Instructions

```bash
# Run API
dotnet run --project Movies.API --launch-profile https
```
The API will be available at:
- HTTPS: `https://localhost:7067`
- HTTP: `http://localhost:5053`

```bash
# Run Blazor (in another terminal)
dotnet run --project Movies.Blazor --launch-profile https
```

The Blazor app will be available at:
- HTTPS: `https://localhost:7065`
- HTTP: `http://localhost:5078`

When both backend and frontend are running:
- Navigate to `https://localhost:7065` in your browser

## Development

### Database Migrations

If you add or change models, you may need to create and apply Entity Framework Core migrations:

```bash
# Install EF Core CLI tools if not already installed
dotnet tool install --global dotnet-ef
```

```bash
cd Movies.API
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

### Tailwind CSS

The Tailwind CSS is automatically built during compilation. To manually rebuild:

```bash
cd Movies.Blazor
npx @tailwindcss/cli -i ./Styles/input.css -o ./wwwroot/tailwind.css
```

No need to ensure the target database (`movies_db` by default) exists in your PostgreSQL instance - Entity Framework will handle creation.

## API Endpoints

### Movies Controller (`/api/movies`)

- **GET** `/api/movies/search?title={title}` - Search for movies by title
- **GET** `/api/movies/details/{imdbId}` - Get detailed movie information
- **GET** `/api/movies/search-history` - Get recent search history
