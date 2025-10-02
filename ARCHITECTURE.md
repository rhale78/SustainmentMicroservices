# Architecture Documentation

## Table of Contents
1. [System Architecture](#system-architecture)
2. [Component Diagrams](#component-diagrams)
3. [Data Flow](#data-flow)
4. [Database Architecture](#database-architecture)
5. [Deployment Architecture](#deployment-architecture)
6. [Technology Stack](#technology-stack)

## System Architecture

### High-Level Architecture

The Sustainment Microservices framework follows a microservices architecture pattern with centralized registration and discovery services.

```
┌─────────────────────────────────────────────────────────────────┐
│                     Client Applications                         │
│              (Microservices using the framework)                │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     │ HTTP/REST
                     │
┌────────────────────▼────────────────────────────────────────────┐
│                  API Gateway Layer (Optional)                   │
└────────────────────┬────────────────────────────────────────────┘
                     │
         ┌───────────┴───────────┐
         │                       │
         ▼                       ▼
┌────────────────────┐  ┌────────────────────┐
│  Application       │  │  Queue             │
│  Registry API      │  │  Microservice      │
│  (Port 5000/5001)  │  │  (Port 6000/6001)  │
└─────────┬──────────┘  └─────────┬──────────┘
          │                       │
          │ Service Layer         │ Service Layer
          │                       │
          ▼                       ▼
┌────────────────────┐  ┌────────────────────┐
│  Registry          │  │  Queue             │
│  Services          │  │  Services          │
│  + Discovery       │  │                    │
└─────────┬──────────┘  └─────────┬──────────┘
          │                       │
          │ Repository Layer      │ Repository Layer
          │                       │
          ▼                       ▼
┌────────────────────┐  ┌────────────────────┐
│  Registry          │  │  Queue             │
│  Repository        │  │  Repository        │
└─────────┬──────────┘  └─────────┬──────────┘
          │                       │
          │ Data Access Layer     │ Data Access Layer
          │                       │
          ▼                       ▼
┌─────────────────────────────────────────────┐
│           SQL Server Database               │
│  ┌──────────────┐    ┌──────────────┐      │
│  │  Registry    │    │  Queue       │      │
│  │  Schema      │    │  Schema      │      │
│  └──────────────┘    └──────────────┘      │
└─────────────────────────────────────────────┘

          ┌─────────────────────┐
          │  Background         │
          │  Services           │
          │  - Health Checks    │
          │  - Monitoring       │
          └─────────────────────┘
```

### Layer Responsibilities

#### 1. API Layer (Controllers)
**Projects:** `ApplicationRegistry.Microservice`, `Queue.Microservice`

**Responsibilities:**
- HTTP endpoint exposure
- Request validation
- Response formatting
- Authentication/Authorization (when implemented)
- Swagger/OpenAPI documentation

**Pattern:** ASP.NET Core Web API with attribute routing

#### 2. Service Layer
**Projects:** `ApplicationRegistry.Services`, `Queue.Services`

**Responsibilities:**
- Business logic implementation
- Orchestration of multiple repository operations
- Transaction coordination
- Data transformation (Domain Models ↔ DTOs)
- Cross-cutting concerns (logging, validation)

**Pattern:** Service classes with interface-based dependency injection

#### 3. Repository Layer
**Projects:** `ApplicationRegistry.Repository`, `Queue.Repository`

**Responsibilities:**
- Data access abstraction
- Query composition
- Entity relationship management
- Database-specific operations
- Caching (when implemented)

**Pattern:** Repository pattern with specialized repository per entity

#### 4. Data Access Layer (DAL)
**Projects:** `Common.DAL`

**Responsibilities:**
- Generic CRUD operations
- SQL query execution
- Object-relational mapping
- Connection management
- Cycle detection for complex object graphs

**Pattern:** Generic base classes with template method pattern

#### 5. Domain Models
**Projects:** `ApplicationRegistry.DomainModels`, `Queue.DomainModels`

**Responsibilities:**
- Entity definitions
- Relationship definitions
- Navigation properties
- Business invariants (validation)

**Pattern:** Anemic domain model (primarily data holders)

#### 6. Data Transfer Objects (DTOs)
**Projects:** `ApplicationRegistry.Model`, `Queue.Model`

**Responsibilities:**
- API request/response shapes
- Data transfer between layers
- Serialization/deserialization
- Validation attributes

**Pattern:** Simple POCO classes with data annotations

#### 7. Interfaces
**Projects:** `ApplicationRegistry.Interfaces`, `Queue.Interfaces`

**Responsibilities:**
- Service contracts
- Repository contracts
- Dependency inversion
- Testability support

**Pattern:** Interface segregation principle

## Component Diagrams

### Application Registry Components

```
┌─────────────────────────────────────────────────────────────────┐
│                 ApplicationRegistry.Microservice                │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Controllers                                             │   │
│  │  - ApplicationRegistryController                         │   │
│  │  - ApplicationDiscoveryController                        │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Background Services                                     │   │
│  │  - HealthyURLBackgroundService                          │   │
│  │  - NotHealthyURLBackgroundService                       │   │
│  └──────────────────────────────────────────────────────────┘   │
└────────────────────────┬────────────────────────────────────────┘
                         │ depends on
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                  ApplicationRegistry.Services                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  ApplicationRegistryService                              │   │
│  │  - RegisterSelf()                                        │   │
│  │  - Register()                                            │   │
│  │  - VerifyApplicationModel()                             │   │
│  │  - SetApplicationActiveFlag()                           │   │
│  │  - IncrementRegistryInstanceHeartbeat()                 │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  ApplicationDiscoveryService                             │   │
│  │  - RegisterSelf()                                        │   │
│  │  - GetURLsForFriendlyName()                             │   │
│  │  - GetRoutes()                                           │   │
│  │  - GetAllApplicationHealthStatus()                      │   │
│  │  - UpdateURL()                                           │   │
│  └──────────────────────────────────────────────────────────┘   │
└────────────────────────┬────────────────────────────────────────┘
                         │ depends on
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                 ApplicationRegistry.Repository                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  ApplicationRegistryRepository (Root Repository)         │   │
│  │  - CreateOrFind()                                        │   │
│  │  - Save()                                                │   │
│  │  - UpgradeAndPurgeDatabase()                            │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Specialized Repositories                                │   │
│  │  - ApplicationRegistryItemRepository                     │   │
│  │  - ApplicationRegistryVersionRepository                  │   │
│  │  - ApplicationRegistryInstanceRepository                 │   │
│  │  - ApplicationDiscoveryRepository                        │   │
│  └──────────────────────────────────────────────────────────┘   │
└────────────────────────┬────────────────────────────────────────┘
                         │ depends on
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                         Common.DAL                              │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  DALObjectBase<T> (Generic Repository Base)              │   │
│  │  - ExecuteSQLQuery()                                     │   │
│  │  - SaveInternal()                                        │   │
│  │  - CreateTable()                                         │   │
│  │  - DropTable()                                           │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  CycleDetector                                           │   │
│  │  ObjectLoader                                            │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

### Supporting Components

```
┌─────────────────────────────────────────────────────────────────┐
│                    ApplicationRegistry.Common                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Helpers (Static Utility Class)                          │   │
│  │  - GetRegistryEntry()         - Assembly scanning        │   │
│  │  - GetApplicationHash()        - Hash computation        │   │
│  │  - GetDiscoveryEntries()       - Reflection discovery    │   │
│  │  - GetAssemblies()             - Assembly enumeration    │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                      Common.Extensions                          │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Configuration Extensions                                │   │
│  │  - GetBoolValueWithDefault()                            │   │
│  │  - GetIntValueWithDefault()                             │   │
│  │  - GetTimespanValue()                                   │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                         LogInstance                             │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Centralized Logging                                     │   │
│  │  - LogInformation()                                      │   │
│  │  - LogError()                                            │   │
│  │  - LogException()                                        │   │
│  │  - LogDebug()                                            │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

## Data Flow

### Registration Flow

```
┌──────────────┐
│ Microservice │
│   Startup    │
└──────┬───────┘
       │
       │ 1. Constructor injection creates
       │    ApplicationRegistryService
       │
       ▼
┌─────────────────────────────────────┐
│ ApplicationRegistryService.ctor     │
│ - Calls RegisterSelf()              │
└──────┬──────────────────────────────┘
       │
       │ 2. Initialize database
       │
       ▼
┌─────────────────────────────────────┐
│ UpgradeAndPurgeDatabase()           │
│ - Create tables if needed           │
│ - Purge if configured               │
└──────┬──────────────────────────────┘
       │
       │ 3. Build registry entry
       │
       ▼
┌─────────────────────────────────────┐
│ Helpers.GetRegistryEntry()          │
│ - Get assembly info                 │
│ - Compute hash                      │
│ - Get machine/path                  │
└──────┬──────────────────────────────┘
       │
       │ 4. Register application
       │
       ▼
┌─────────────────────────────────────┐
│ ApplicationRegistryService.Register()│
│ - CreateOrFind items                │
│ - Check permissions                 │
│ - Save to database                  │
└──────┬──────────────────────────────┘
       │
       │ 5. Register discovery
       │
       ▼
┌─────────────────────────────────────┐
│ ApplicationDiscoveryService.        │
│ RegisterSelf()                      │
│ - Scan controllers                  │
│ - Extract routes                    │
│ - Save URLs                         │
└──────┬──────────────────────────────┘
       │
       │ 6. Mark active
       │
       ▼
┌─────────────────────────────────────┐
│ SetApplicationActiveFlag(true)      │
│ - Update IsActive                   │
│ - Set LastStartDateTime             │
└─────────────────────────────────────┘
```

### Discovery Flow

```
┌──────────────┐
│ Client       │
│ Application  │
└──────┬───────┘
       │
       │ 1. Request service URL
       │    by friendly name
       │
       ▼
┌─────────────────────────────────────┐
│ GET /ApplicationDiscovery/          │
│ GetURLsForFriendlyName/{name}       │
└──────┬──────────────────────────────┘
       │
       │ 2. Query discovery item
       │
       ▼
┌─────────────────────────────────────┐
│ ApplicationDiscoveryRepository.     │
│ GetDiscoveryItemByFriendlyName()    │
│ - Query ApplicationDiscoveryItem    │
│ - Include related URLs              │
└──────┬──────────────────────────────┘
       │
       │ 3. Filter and validate URLs
       │
       ▼
┌─────────────────────────────────────┐
│ GetHealthyUrls()                    │
│ - Filter by HTTPS preference        │
│ - Check HealthStatus = "Healthy"    │
│ - Verify recent health check        │
│ - Format URLs with port/route       │
└──────┬──────────────────────────────┘
       │
       │ 4. Return URL list
       │
       ▼
┌─────────────────────────────────────┐
│ [                                   │
│   "https://server1:5001/api/queue", │
│   "https://server2:5001/api/queue"  │
│ ]                                   │
└──────┬──────────────────────────────┘
       │
       │ 5. Client makes HTTP call
       │    to discovered service
       │
       ▼
┌──────────────┐
│ Target       │
│ Microservice │
└──────────────┘
```

### Health Check Flow

```
┌────────────────────────────────────────┐
│ HealthyURLBackgroundService            │
│ (Hosted Service)                       │
└────────┬───────────────────────────────┘
         │
         │ Every N minutes
         │
         ▼
┌────────────────────────────────────────┐
│ GetMicroserviceURLs(healthyOnly=true)  │
│ - Query URLs with HealthStatus="Healthy"│
│ - Filter by last check time           │
└────────┬───────────────────────────────┘
         │
         │ For each URL
         │
         ▼
┌────────────────────────────────────────┐
│ CheckAndUpdateURLHealthStatus()        │
│                                        │
│ Try:                                   │
│   GET {url}:{port}/microserviceHealth  │
│   Parse health status from response    │
│ Catch HttpRequestException:            │
│   Status = "Down"                      │
└────────┬───────────────────────────────┘
         │
         │ If status changed
         │
         ▼
┌────────────────────────────────────────┐
│ ApplicationDiscoveryService.UpdateURL()│
│ - Update HealthStatus                  │
│ - Update LastHealthStatusCheckDateTime │
│ - Save to database                     │
└────────────────────────────────────────┘

     (Similar flow for NotHealthyURLBackgroundService
      but targets URLs that are not "Healthy")
```

## Database Architecture

### Entity-Relationship Diagram

```
┌──────────────────────────┐
│ ApplicationRegistryItem  │
│ ────────────────────────│
│ PK: ID (int)            │
│ ApplicationName (varchar)│
│ FirstInstallDateTime    │
└──────────┬───────────────┘
           │
           │ M:M via ApplicationRegistryItemApplicationRegistryVersion
           │
           ▼
┌──────────────────────────┐
│ ApplicationRegistryVersion│
│ ────────────────────────│
│ PK: ID (int)            │
│ ApplicationVersion      │
│ ApplicationHash         │
│ FK: PreviousVersionID   │◄──┐ Self-reference
│ BuildDateTime           │   │ for upgrade tracking
│ FirstInstallDateTime    │   │
└──────────┬───────────────┘   │
           │                   │
           │ M:M via ApplicationRegistryVersionApplicationRegistryInstance
           │
           ▼
┌──────────────────────────────┐
│ ApplicationRegistryInstance  │
│ ─────────────────────────────│
│ PK: ID (int)                │
│ MachineName                 │
│ ApplicationPath             │
│ NumberHeartbeats            │
│ LastHeartbeatDateTime       │
│ LastStartDateTime           │
│ InstallDateTime             │
│ IsActive                    │
└──────────┬───────────────────┘
           │
           │ M:M via ApplicationRegistryInstanceApplicationDiscoveryURL
           │
           ▼
┌──────────────────────────────┐
│ ApplicationDiscoveryURL      │
│ ─────────────────────────────│
│ PK: ID (int)                │
│ URL (varchar)               │
│ Port (int nullable)         │
│ HealthStatus (varchar)      │
│ LastHealthStatusCheckDateTime│
└──────────┬───────────────────┘
           │
           │ M:M via ApplicationDiscoveryURLApplicationDiscoveryItem
           │
           ▼
┌──────────────────────────────┐
│ ApplicationDiscoveryItem     │
│ ─────────────────────────────│
│ PK: ID (int)                │
│ FriendlyName                │
│ ControllerName              │
│ ControllerRoute             │
└──────────┬───────────────────┘
           │
           │ M:M via ApplicationDiscoveryMethodApplicationDiscoveryItem
           │
           ▼
┌──────────────────────────────┐
│ ApplicationDiscoveryMethod   │
│ ─────────────────────────────│
│ PK: ID (int)                │
│ HttpMethod                  │
│ Template                    │
│ MethodName                  │
└──────────────────────────────┘
```

### Table Relationships

**Many-to-Many Join Tables:**
1. `ApplicationRegistryItemApplicationRegistryVersion` - Links applications to versions
2. `ApplicationRegistryVersionApplicationRegistryInstance` - Links versions to instances
3. `ApplicationRegistryInstanceApplicationDiscoveryURL` - Links instances to URLs
4. `ApplicationDiscoveryURLApplicationDiscoveryItem` - Links URLs to discovery items
5. `ApplicationDiscoveryMethodApplicationDiscoveryItem` - Links methods to discovery items
6. `ApplicationRegistryVersionApplicationDiscoveryItem` - Links versions to discovery items

### Indexes

Automatically created indexes on:
- Primary keys (ID columns)
- All composite primary keys (join tables)

Recommended additional indexes:
- `ApplicationRegistryItem.ApplicationName`
- `ApplicationRegistryVersion.ApplicationHash`
- `ApplicationRegistryInstance.MachineName`
- `ApplicationRegistryInstance.IsActive`
- `ApplicationDiscoveryItem.FriendlyName`
- `ApplicationDiscoveryURL.HealthStatus`

## Deployment Architecture

### Single-Server Deployment

```
┌────────────────────────────────────────────────────────────┐
│                      Application Server                     │
│                                                             │
│  ┌─────────────────────┐       ┌─────────────────────┐    │
│  │ IIS / Kestrel       │       │ IIS / Kestrel       │    │
│  │                     │       │                     │    │
│  │ Application         │       │ Queue               │    │
│  │ Registry API        │       │ Microservice        │    │
│  │ Port 5000/5001      │       │ Port 6000/6001      │    │
│  └─────────────────────┘       └─────────────────────┘    │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │        Background Services (Same Process)           │   │
│  │        - HealthyURLBackgroundService                │   │
│  │        - NotHealthyURLBackgroundService             │   │
│  └─────────────────────────────────────────────────────┘   │
└────────────────────────┬───────────────────────────────────┘
                         │
                         │ SQL Connection
                         │
┌────────────────────────▼───────────────────────────────────┐
│                  SQL Server (Local or Remote)              │
│                  - ApplicationRegistry DB                  │
│                  - Queue DB                                │
└────────────────────────────────────────────────────────────┘
```

### Multi-Server Deployment

```
┌─────────────────────────────────────────────────────────────────┐
│                        Load Balancer                            │
│                     (HTTPS Termination)                         │
└───────┬────────────────────────────────────────────────┬────────┘
        │                                                │
        ▼                                                ▼
┌──────────────────────┐                    ┌──────────────────────┐
│   App Server 1       │                    │   App Server 2       │
│                      │                    │                      │
│  Registry API        │                    │  Registry API        │
│  + Background Svc    │                    │  + Background Svc    │
│                      │                    │                      │
│  Queue API           │                    │  Queue API           │
└──────────┬───────────┘                    └──────────┬───────────┘
           │                                           │
           │        SQL Connection (Shared)            │
           └───────────────────┬───────────────────────┘
                               │
┌──────────────────────────────▼──────────────────────────────────┐
│            SQL Server (High Availability)                       │
│            - AlwaysOn Availability Groups                       │
│            - Automatic Failover                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Container Deployment (Docker/Kubernetes)

```
┌─────────────────────────────────────────────────────────────────┐
│                    Kubernetes Cluster                           │
│                                                                 │
│  ┌────────────────────────────────────────────────────────┐    │
│  │  Namespace: sustainment-services                       │    │
│  │                                                         │    │
│  │  ┌──────────────────────────────────────────────┐     │    │
│  │  │  Deployment: application-registry            │     │    │
│  │  │  Replicas: 3                                 │     │    │
│  │  │  ┌─────────────────────────────────────┐    │     │    │
│  │  │  │  Pod 1                              │    │     │    │
│  │  │  │  - Container: registry-api          │    │     │    │
│  │  │  │  - Container: background-services   │    │     │    │
│  │  │  └─────────────────────────────────────┘    │     │    │
│  │  └──────────────────────────────────────────────┘     │    │
│  │                                                         │    │
│  │  ┌──────────────────────────────────────────────┐     │    │
│  │  │  Service: application-registry-service       │     │    │
│  │  │  Type: ClusterIP                             │     │    │
│  │  │  Port: 80 → 5000                            │     │    │
│  │  └──────────────────────────────────────────────┘     │    │
│  │                                                         │    │
│  │  ┌──────────────────────────────────────────────┐     │    │
│  │  │  Deployment: queue-microservice              │     │    │
│  │  │  Replicas: 2                                 │     │    │
│  │  └──────────────────────────────────────────────┘     │    │
│  │                                                         │    │
│  │  ┌──────────────────────────────────────────────┐     │    │
│  │  │  Service: queue-service                      │     │    │
│  │  │  Type: ClusterIP                             │     │    │
│  │  └──────────────────────────────────────────────┘     │    │
│  │                                                         │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  ConfigMap: registry-config                             │    │
│  │  - Connection strings                                   │    │
│  │  - Feature flags                                        │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  Secret: registry-secrets                               │    │
│  │  - Database passwords                                   │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────┬───────────────────────────────────────┘
                          │
                          │ External connection
                          │
┌─────────────────────────▼───────────────────────────────────────┐
│            Azure SQL Database / SQL Server                      │
│            - Managed service or external cluster                │
└─────────────────────────────────────────────────────────────────┘
```

### Docker Compose Example

```yaml
version: '3.8'

services:
  registry-api:
    image: sustainment/application-registry:latest
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ConnectionStrings__ApplicationRegistry=Server=sqlserver;Database=ApplicationRegistry;User=sa;Password=YourPassword
      - AllowNewApplicationRegistration=true
      - UpgradeDatabase=true
    depends_on:
      - sqlserver
    networks:
      - sustainment-network

  queue-api:
    image: sustainment/queue-microservice:latest
    ports:
      - "6000:80"
      - "6001:443"
    environment:
      - ConnectionStrings__Queue=Server=sqlserver;Database=Queue;User=sa;Password=YourPassword
      - RegistryUrl=http://registry-api
    depends_on:
      - sqlserver
      - registry-api
    networks:
      - sustainment-network

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - sustainment-network

networks:
  sustainment-network:
    driver: bridge

volumes:
  sqlserver-data:
```

## Technology Stack

### Framework & Language
- **.NET 6.0+** - Core framework
- **C# 10+** - Programming language
- **ASP.NET Core** - Web API framework

### Data Access
- **ADO.NET** - Direct SQL access via `SqlConnection`, `SqlCommand`
- **Custom ORM** - Generic `DALObjectBase<T>` with reflection
- **SQL Server** - Primary database (5.2.0 client)

### API & Communication
- **REST/HTTP** - API communication
- **JSON** - Serialization format
- **Swagger/OpenAPI** - API documentation

### Background Processing
- **IHostedService** - Background service hosting
- **BackgroundService** - Base class for long-running tasks

### Configuration
- **IConfiguration** - ASP.NET Core configuration system
- **appsettings.json** - Configuration files
- **Environment Variables** - Override mechanism

### Logging
- **Custom LogInstance** - Centralized logging
- **ILogger compatible** - Standard .NET logging interface

### Dependency Injection
- **Microsoft.Extensions.DependencyInjection** - Built-in DI container

### Reflection & Metadata
- **System.Reflection** - Assembly scanning
- **System.Reflection.Emit** - Dynamic code generation (CycleDetector)

### Security (Currently Limited)
- **MD5** - Hash computation (non-security)
- **HTTPS** - Transport security support

### Testing
- **MSTest** or **xUnit** - Unit testing framework (implied)

### Development Tools
- **Visual Studio 2022** - Primary IDE
- **Visual Studio Code** - Alternative IDE

## Scalability Considerations

### Horizontal Scaling
- **Stateless APIs** - All services are stateless, enabling load balancing
- **Database Bottleneck** - SQL Server is potential bottleneck
- **Connection Pooling** - Essential for multiple instances

### Vertical Scaling
- **Database** - Can scale SQL Server vertically
- **API Instances** - Can increase resources per instance

### Caching Strategies
- **Response Caching** - For frequently accessed data
- **Distributed Cache** - Redis for shared cache across instances
- **In-Memory Cache** - Per-instance caching

### Performance Optimizations
- **Async Operations** - All I/O operations are async
- **Query Optimization** - Indexes on frequently queried columns
- **Background Services** - Offload health checks from request path

## Security Architecture

### Current State
- No authentication implemented
- No authorization implemented
- Connection strings in configuration
- HTTPS support available but not enforced

### Recommended Security Layers

```
┌─────────────────────────────────────────────────────────────────┐
│                    API Gateway / Reverse Proxy                  │
│                    - SSL Termination                            │
│                    - Rate Limiting                              │
│                    - IP Filtering                               │
└────────────────────────┬────────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────────┐
│                    Authentication Layer                         │
│                    - JWT Bearer Tokens                          │
│                    - Azure AD / OAuth2                          │
└────────────────────────┬────────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────────┐
│                    Authorization Layer                          │
│                    - Role-Based Access Control                  │
│                    - Policy-Based Authorization                 │
└────────────────────────┬────────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────────┐
│                    Application Services                         │
└─────────────────────────────────────────────────────────────────┘
```

## Monitoring & Observability

### Recommended Additions

1. **Application Insights** - Azure monitoring
2. **Prometheus/Grafana** - Metrics and dashboards
3. **Seq/ELK Stack** - Structured logging
4. **Health Checks** - Already partially implemented
5. **Distributed Tracing** - OpenTelemetry

### Key Metrics to Track
- Request rate and latency
- Registration success/failure rate
- Health check status distribution
- Database connection pool metrics
- Background service execution time
- Discovery lookup performance

## Disaster Recovery

### Backup Strategy
- **Database Backups** - Regular SQL Server backups
- **Point-in-Time Recovery** - Transaction log backups
- **Configuration Backups** - Version control for settings

### High Availability
- **SQL Server Always On** - Database HA
- **Multiple API Instances** - Service redundancy
- **Load Balancer** - Automatic failover

### Recovery Procedures
1. Database restoration from backup
2. Redeploy API instances
3. Verify service discovery
4. Run health checks

## Future Architecture Considerations

### Potential Enhancements

1. **Service Mesh** - Istio/Linkerd for advanced networking
2. **Event-Driven Architecture** - Use queue for async registration
3. **CQRS** - Separate read/write models for better performance
4. **GraphQL Gateway** - Alternative query interface
5. **gRPC** - High-performance inter-service communication
6. **Distributed Transactions** - Saga pattern for complex operations
7. **Circuit Breaker** - Resilience for external calls
8. **API Versioning** - Support for multiple API versions
9. **Multi-Tenancy** - Isolated environments per tenant
10. **Audit Trail** - Full change history tracking
