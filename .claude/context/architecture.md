# Architecture Document

## Overview
This document describes the architecture patterns and principles used in this project

## Core Architecture
- **Pattern**: Service-based architecture with direct DbContext access
- **Framework**: .NET 9.0 with Entity Framework Core
- **Database**: SQL Server
- **API**: RESTful API with ASP.NET Core

## Key Principles
1. **No Unit of Work Pattern**: Services interact directly with DbContext
2. **Service Layer**: All business logic encapsulated in service classes
3. **Dependency Injection**: Constructor injection for all dependencies
4. **Async/Await**: All data operations are asynchronous
5. **Single Responsibility**: Only one class declaration must exist per file.

## Layer Responsibilities

### Controllers
- HTTP request/response handling
- Model validation
- Authorization
- Delegate business logic to services

### Services
- Business logic implementation
- Direct DbContext interaction
- Transaction management
- Business rule validation

### Data Layer
- Entity Framework Core DbContext
- Entity configurations
- Database migrations
- Query optimizations

### Models
- Entity models (database representation)
- DTOs (Data Transfer Objects)
- Request/Response models


## Error Handling
- Global exception middleware
- Structured error responses
- Detailed logging with Serilog

## Security
- JWT Bearer authentication
- Role-based authorization
- Policy-based authorization for complex scenarios
