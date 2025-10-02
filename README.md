# Sustainment Microservices

A comprehensive microservices infrastructure solution providing application registration, discovery, and queue management capabilities for distributed systems.

## Overview

This repository contains a microservices framework designed to support enterprise application sustainment through automated registration, health monitoring, and service discovery. The system enables microservices to automatically register themselves, discover other services, and maintain health status with minimal configuration.

## Key Components

### 1. Application Registry System
The **Application Registry** is the core component that tracks all applications, their versions, instances, and deployment locations across the infrastructure.

**Key Features:**
- Automatic application registration with version tracking
- Instance-level tracking (machine name, path, installation date)
- Application hash-based version verification
- Active/inactive instance status management
- Heartbeat monitoring for instance health

**Projects:**
- `ApplicationRegistry.Microservice` - REST API microservice
- `ApplicationRegistry.Services` - Business logic layer
- `ApplicationRegistry.Repository` - Data access layer
- `ApplicationRegistry.DomainModels` - Entity models
- `ApplicationRegistry.Model` - Data transfer objects
- `ApplicationRegistry.Interfaces` - Service contracts
- `ApplicationRegistry.Common` - Shared utilities
- `ApplicationRegistry.BackgroundServices` - Health check services

### 2. Application Discovery System
Built on top of the registry, the **Application Discovery** system provides service discovery capabilities, allowing microservices to find and communicate with each other.

**Key Features:**
- Automatic endpoint discovery from controller attributes
- URL health status monitoring (Healthy/Unhealthy/Down)
- Friendly name-based service lookup
- HTTP/HTTPS endpoint management
- Route template discovery for API documentation

### 3. Queue Microservice
A message queue system for asynchronous communication between services.

**Projects:**
- `Queue.Microservice` - Queue API microservice
- `Queue.Services` - Queue business logic
- `Queue.Repository` - Queue data access
- `Queue.DomainModels` - Queue entities
- `Queue.Model` - Queue DTOs
- `Queue.Client` - Client library for queue access
- `Queue.Interfaces` - Queue contracts

### 4. Common Infrastructure
Shared components used across all microservices:

- `Common.DAL` - Generic data access layer with SQL Server support
- `Common.Repository` - Repository base classes
- `Common.Extensions` - Configuration and utility extensions
- `Common.Dependencies` - Dependency tracking
- `Common.NoAuthDiscoveryClient` - Discovery client without authentication
- `LogInstance` - Centralized logging infrastructure

## Architecture

The system follows a layered architecture pattern:

```
┌─────────────────────────────────────────────────────┐
│          Microservice API Controllers               │
│   (ApplicationRegistry.Controller, Queue.Microservice)│
└─────────────────────────────────────────────────────┘
                        │
┌─────────────────────────────────────────────────────┐
│              Service Layer                          │
│    (Business Logic, Registration, Discovery)        │
└─────────────────────────────────────────────────────┘
                        │
┌─────────────────────────────────────────────────────┐
│           Repository Layer                          │
│     (Data Access, SQL Operations)                   │
└─────────────────────────────────────────────────────┘
                        │
┌─────────────────────────────────────────────────────┐
│            SQL Server Database                      │
│   (Application Registry, Discovery, Queue)          │
└─────────────────────────────────────────────────────┘

     Background Services (Health Checks)
```

## Getting Started

### Prerequisites
- .NET 6.0 or later
- SQL Server (for database storage)
- Visual Studio 2022 or VS Code

### Configuration

Applications using this framework require the following configuration:

```json
{
  "ConnectionStrings": {
    "ApplicationRegistry": "Server=...;Database=ApplicationRegistry;..."
  },
  "urls": "http://localhost:5000;https://localhost:5001",
  "AllowNewApplicationRegistration": true,
  "AllowNewApplicationInstance": true,
  "AllowNewApplicationVersion": true,
  "UpgradeDatabase": false,
  "PurgeRegistry": false,
  "DiscoveryURLHealthStatusTimeSpan": "00:05:00"
}
```

### Registration Flow

When a microservice starts:
1. **Self-Registration**: Calls `ApplicationRegistryService.RegisterSelf()`
2. **Application Discovery**: Scans controllers and registers endpoints
3. **Health Monitoring**: Background services monitor URL health
4. **Active Flag**: Sets instance as active in registry

### Discovery Flow

To discover another service:
1. Call `ApplicationDiscoveryService.GetURLsForFriendlyName(friendlyName)`
2. Receive list of healthy URLs for the service
3. Use URL to communicate with the service

## Database Schema

The registry uses a normalized relational schema:

### Core Tables
- **ApplicationRegistryItem** - Application definition (Name)
- **ApplicationRegistryVersion** - Version information (Version, Hash, Build Date)
- **ApplicationRegistryInstance** - Deployment instance (Machine, Path, Status)

### Join Tables
- **ApplicationRegistryItemApplicationRegistryVersion** - Links applications to versions
- **ApplicationRegistryVersionApplicationRegistryInstance** - Links versions to instances

### Discovery Tables
- **ApplicationDiscoveryItem** - Service friendly name and route
- **ApplicationDiscoveryURL** - Endpoint URLs with health status
- **ApplicationDiscoveryMethod** - API method definitions

## API Endpoints

### Application Registry Controller

- `POST /ApplicationRegistry/Register` - Register an application
- `POST /ApplicationRegistry/Verify` - Verify application credentials
- `GET /ApplicationRegistry/{instanceId}` - Get instance hierarchy
- `POST /ApplicationRegistry/Active` - Check if instance is active
- `PUT /ApplicationRegistry/InstanceIsActiveFlag` - Update active status
- `GET /ApplicationRegistry/WhoIs/{instanceId}` - Get application name
- `GET /ApplicationRegistry/Location/{instanceId}` - Get instance location
- `PUT /ApplicationRegistry/RegistryInstanceHeartbeat` - Update heartbeat

### Application Discovery Controller

- `GET /ApplicationDiscovery/GetURLsForFriendlyName/{name}` - Get service URLs
- `GET /ApplicationDiscovery/GetRoutes/{name}` - Get API routes
- `GET /ApplicationDiscovery/HealthStatus` - Get all health statuses
- `GET /ApplicationDiscovery/HealthStatus/{friendlyName}` - Get specific health status

## Background Services

### HealthyURLBackgroundService
Periodically checks URLs that are currently marked as "Healthy" to ensure they remain responsive.

### NotHealthyURLBackgroundService
Periodically checks URLs that are marked as unhealthy to detect when they recover.

## Testing

The `RegistryTest` project provides integration testing capabilities for the registry system.

## For More Information

- [Detailed Architecture Documentation](ARCHITECTURE.md)
- [Application Registry Deep Dive](ApplicationRegistry-README.md)
- [Code Review and Recommendations](CODE_REVIEW.md)

## Contributing

This is a sustainment microservices framework. When adding new microservices:
1. Reference `ApplicationRegistry.Services` for automatic registration
2. Call `RegisterSelf()` during startup
3. Implement health checks at `/microserviceHealth`
4. Use standard controller attributes for discovery

## License

[Specify License]
