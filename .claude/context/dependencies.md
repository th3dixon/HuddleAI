# Project Dependencies

## Core Framework
- **Target Framework**: .NET 9.0
- **Language Version**: C# 12.0
- **Nullable Reference Types**: Disabled

## NuGet Packages
### Primary Sources
- **NuGet.org**: Standard package repository
- **DevExpress Private Feed**: https://nuget.devexpress.com/3y9KIRRtr3OFOrqZMfgTJsh9l11xVgamEZxlND3doTwNvkvCBG/api/v3/index.json

### Entity Framework Core
- Microsoft.EntityFrameworkCore.SqlServer (9.0.6)
- Microsoft.EntityFrameworkCore.Design (9.0.6)
- Microsoft.EntityFrameworkCore.Tools (9.0.6)

### ASP.NET Core
- Microsoft.AspNetCore.Authentication.JwtBearer (9.0.6)
- Microsoft.AspNetCore.Identity.EntityFrameworkCore (9.0.6)
- Microsoft.AspNetCore.Mvc.NewtonsoftJson (9.0.6)

### Validation & Mapping
- FluentValidation (12.0.0)
- Riok.Mapperly (4.2.1)

### Logging
- Serilog.AspNetCore (9.0.0)
- Serilog.Sinks.File (7.0.0)
- Serilog.Sinks.Console (6.0.0)
- Serilog.Sinks.MSSqlServer (8.2.0)

### API Documentation
- Microsoft.AspNetCore.OpenApi (9.0.6)
- NSwag.AspNetCore (14.4.0)
- Swashbuckle.AspNetCore (8.1.4)
- Swashbuckle.AspNetCore.Annotations (8.1.4)

### Testing
- xunit (2.9.3)
- xunit.runner.visualstudio (3.1.1)
- Moq (4.20.72)
- Microsoft.EntityFrameworkCore.InMemory (9.0.6)

### Utilities
- Newtonsoft.Json (13.0.3)
- Polly (8.6.0)
- MediatR (12.5.0)

## DevExpress Components
- DevExpress.AspNetCore (24.2.7)
- License: ewogICJmb3JtYXQiOiAxLAogICJjdXN0b21lcklkIjogIjc4NDM2ODFmLWI3NzEtNDI4Ny05NjE1LWQwOTRkYmJiMDYyOSIsCiAgIm1heFZlcnNpb25BbGxvd2VkIjogMjQyCn0=.CCH+G4eIkM/TRvDurM01cW6CcQqvVqo55BDLKy9dFDBeCtKp48qjZusJLR+iXqIEaDOXZq6Y2OfpAqWodF3Hp0ZfbKClbb1HEN3+TMuFjxFx2lSNi+vp8JZDKvcMd2vYAplXHw==

## External Services
- **Database**: SQL Server 2022+
- **Cache**: Redis (optional)
- **Email**: SendGrid/SMTP
- **Storage**: Azure Blob Storage

## Development Tools
- Visual Studio 2022 (17.8+)
- Visual Studio Code
- SQL Server Management Studio
- Git

## Deployment
- Docker support
- Azure App Service ready
- IIS compatible
