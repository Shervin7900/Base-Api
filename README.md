# Discount Manager Microservices

The Discount Manager is a microservices-based application designed for high scalability and modularity. This repository contains the core infrastructure and base microservice components used across the ecosystem.

## 🚀 Project Overview

The project follows a modern microservices architecture where shared concerns (authentication, observation, caching) are centralized in a `BaseApi` project. This base infrastructure is packaged and distributed to other microservices to ensure consistency and speed up development.

## 🛠 Technology Stack

### Core Framework
- **Runtime**: [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- **Architecture**: Microservices, Repository Pattern, Dependency Injection

### Databases & Caching
- **Primary DB**: Microsoft SQL Server 2022
- **NoSQL DB**: MongoDB
- **Caching**: Redis (using Microsoft.Extensions.Caching.Hybrid)

### Infrastructure & Tools
- **Identity**: [Duende IdentityServer](https://duendesoftware.com/products/identityserver) with ASP.NET Identity
- **Observability**: [OpenTelemetry](https://opentelemetry.io/) (Metrics, Traces, Prometheus)
- **Health Checks**: ASP.NET Core Health Checks with UI Dashboard
- **Documentation**: Swagger/OpenAPI (Swashbuckle)
- **Containerization**: Docker & Docker Compose

## ✨ Key Features

- **Centralized Identity**: Unified OAuth2/OpenID Connect provider for all microservices.
- **Deep Observability**: Out-of-the-box telemetry for HTTP requests, database calls, and runtime metrics.
- **Resilient Infrastructure**: Integrated health monitoring for SQL Server, MongoDB, and Redis.
- **Hybrid Caching**: Optimized multi-level caching strategy using Redis and local memory.
- **Modular Design**: Infrastructure components are exposed as easy-to-use extension methods.

## 🏁 Getting Started

### Prerequisites
- Docker Desktop
- .NET 10 SDK

### Running with Docker Compose
To spin up the entire infrastructure (SQL Server, MongoDB, Redis, and the BaseApi):

```powershell
docker-compose up --build
```

The API will be accessible at:
- **HTTP**: [http://localhost:7000/swagger](http://localhost:7000/swagger)
- **HTTPS**: [https://localhost:7001/swagger](https://localhost:7001/swagger)

### Local Scripts
- `build.bat` / `builder.sh`: Quick build scripts for the solution.
- `pack.ps1**: Packages the `BaseApi` into a NuGet package located in `/nupkgs`.

## 🧪 Testing

The project maintains high standards of quality through multiple testing layers:

- **Unit Tests**: Logic verification for individual components.
- **Integration Tests**: Testing interactions with real (or in-memory) databases and dependencies.
- **E2E Tests**: Full-flow validation of API endpoints.

To run all tests:
```bash
dotnet test
```

## 📦 Packaging

The `BaseApi` project is designed to be packable. When built, it generates a NuGet package (`DiscountManager.BaseApi`) that other services in the Discount Manager ecosystem can consume to quickly inherit all base infrastructure capabilities.
