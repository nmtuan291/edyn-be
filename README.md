# Edyn Backend - Microservices Architecture

This project is for my study of microservice architecture and the course 'Distributed Applications'.

## Architecture Overview

This project implements a simple microservices architecture with the following components(at this moment):

- **API Gateway** - Ocelot-based gateway for routing requests
- **AuthService** - JWT-based authentication and authorization
- **UserService** - User management and profiles
- **ForumService** - Forum threads and discussions
- **ChatService** - Real-time messaging with SignalR
- **NotificationService** - Real-time notifications with SignalR

## Technologies Used

- **.NET 9** - ASP.NET Core framework
- **Entity Framework Core** - ORM with PostgreSQL
- **SignalR** - Real-time communication
- **RabbitMQ** - Message queuing for async communication
- **Redis** - Caching 
- **Ocelot** - API Gateway
- **JWT** - Authentication

- ## Getting Started

### 1. Setup Infrastructure(PostgreSQL, Redis, RabbitMQ)

### 2. Database Migration

Run migrations for each service:

```bash
# AuthService
cd Services/AuthService
dotnet ef database update

# UserService
cd Services/UserService
dotnet ef database update

# ForumService
cd Services/ForumService
dotnet ef database update

# ChatService
cd Services/ChatService
dotnet ef database update
```

### 3. Start Services

Each service can be started independently:

```bash
# API Gateway
cd ApiGateway
dotnet run

# AuthService
cd Services/AuthService
dotnet run

# UserService
cd Services/UserService
dotnet run

# ForumService
cd Services/ForumService
dotnet run

# ChatService
cd Services/ChatService
dotnet run

# NotificationService
cd Services/NotificationService
dotnet run
```

## Configuration

### Connection Strings

Update `appsettings.json` in each service with your database connection strings:

```json
{
  "ConnectionStrings": {
    "WebApiDatabase": "Host=localhost; Database=edyn_[service]db; Username=postgres; Password=your_password"
  }
}
```

### RabbitMQ Configuration

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  }
}
```

### Redis Configuration

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```
