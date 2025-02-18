# LGDXRobot-Cloud

A web application for managing and morning LGDXRobot operations, built with .NET and Blazor.

## How it works

This application consists of following projects:

* LGDXRobotCloud.API - The core API for the application
* LGDXRobotCloud.Data - Defines the database schema and data access structures
* LGDXRobotCloud.UI - The UI for the application
* LGDXRobotCloud.Utilities - The shared utilities for the application
* LGDXRobotCloud.Worker - Performs the time-consuming tasks for the application

## Getting started

### Prerequisite

This application requires PostgreSQL, RabbitMQ and a SMTP server, you can install them using Docker. 

For the PostgreSQL, you can create it with a table called `LGDXRobotCloud` using the following command:

```bash
docker run --name postgres -e POSTGRES_PASSWORD=mysecretpassword -e POSTGRES_USER=admin -e POSTGRES_DB=LGDXRobotCloud -v postgres-data:/var/lib/postgresql/data -p 5432:5432 -d postgres
```

For RabbitMQ, you can use the following command:

```bash
docker run --name rabbitmq -p 5672:5672 -p 15672:15672 -d rabbitmq:4.0-management
```

You will need a SMTP server to sending emails. You can either use a free email service that have SMTP support, or you can build a SMTP server.

### Build & Run

There are few pre-configurations that you need to do before you can run the application.

#### 1. Issue Certificates

Certificates are used to secure the communication between the Cloud and the Robots. Therefore, you need the ability of issuing certificates. You can read [this](https://kb.teramind.co/en/articles/8791235-how-to-generate-your-own-self-signed-ssl-certificates-for-use-with-an-on-premise-deployments) on how to generate your own self-signed certificates. Once you are familiar with the commands, you can issue the following certificates:

1. A root certificate
2. Using the root certificate to issue a certificate for gRPC communication, then export it as a PFX file

#### 2. Configuration

Below are the configurations for the API, the UI, and the Worker. Most of the configuations are setting the connection to other services, so you need the creditials of the services below.

* PostgreSQL
* RabbitMQ
* SFTP

For the screts, refer to the [Official Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) on how to create the secrets.

##### 2.1 LGDXRobotCloud.API

`appsettings.json`

```json
"Kestrel": {
  "Endpoints": {
    "gRPC": {
      "Url": "https://*:5162", // The port number must be lower than the port number of the HTTPS endpoints
      "Protocols": "Http2",
      "Certificate": {
        "Path": "grpc.pfx" // The path to the certificate file for the gRPC endpoint
      }
    },
    "Https": {
      "Url": "https://*:5163", // The port number must be higher than the port number of the gRPC endpoints
      "Protocols": "Http1AndHttp2"
      // You could use another certificate for the HTTPS endpoint
    }
  }
},
"LGDXRobot2": {
  "RootCertificateSN": "", // The serial number of the root certificate
  "RobotCertificateValidDay": 365, // The number of days that a robot certificate is valid
  "ApiMaxPageSize": 100 // The maximum number of items that can be returned in a page
}
```

`secrets`

```json
{
  "PGSQLConnectionString": "Host=localhost;Username=admin;Password=secret;Database=LGDX", // The connection string to the PostgreSQL database
  "RabbitMq": {
    "Host": "localhost", // The host name or IP address of the RabbitMQ server
    "VirtualHost": "/", // The virtual host of the RabbitMQ server
    "Username": "username", // The username for the RabbitMQ server
    "Password": "secret" // The password for the RabbitMQ server
  },
  "LGDXRobot2Secret": {
    "LgdxUserJwtIssuer": "LGDXRobot2Users", // The issuer of the JWT token for the LGDXRobot2 users
    "LgdxUserJwtSecret": "secret with at least 32 characters", // The secret of the JWT token for the LGDXRobot2 users
    "RobotClientsJwtIssuer": "LGDXRobot2RobotClients", // The issuer of the JWT token for the LGDXRobot2 robot clients
    "RobotClientsJwtSecret": "secret with at least 32 characters" // The secret of the JWT token for the LGDXRobot2 robot clients
  }
}
```

##### 2.2 LGDXRobotCloud.Data

`secrets` 

```json
{
  "PGSQLConnectionString": "Host=localhost;Username=admin;Password=secret;Database=LGDX" // The connection string to the PostgreSQL database
}
```

##### 2.3 LGDXRobotCloud.UI

`appsettings.json`

```json
{
  "LGDXRobotCloudApiUrl": "https://localhost:5163", // The URL of the LGDXRobotCloud API
}
```

`secrets`

```json
{
  "RabbitMq": {
    "Host": "localhost", // The host name or IP address of the RabbitMQ server
    "VirtualHost": "/", // The virtual host of the RabbitMQ server
    "Username": "username", // The username for the RabbitMQ server
    "Password": "secret" // The password for the RabbitMQ server
  }
}
```

##### 2.4 LGDXRobotCloud.Worker

`appsettings.json`

```json
"EmailLinks": { // The values belows are used to generate the links in the emails
  "AccessUrl": "https://localhost:5103/", // The URL of the LGDXRobotCloud UI
  "PasswordResetPath": "ResetPassword" // The path to the password reset page in the LGDXRobotCloud UI
}
```

`secrets`

```json
{
  "PGSQLConnectionString": "Host=localhost;Username=admin;Password=secret;Database=LGDX", // The connection string to the PostgreSQL database
  "RabbitMq": {
    "Host": "localhost", // The host name or IP address of the RabbitMQ server
    "VirtualHost": "/", // The virtual host of the RabbitMQ server
    "Username": "username", // The username for the RabbitMQ server
    "Password": "secret" // The password for the RabbitMQ server
  },
  "Email": {
    "FromName": "From name", // The name of the sender
    "FromAddress": "email address", // The email address of the sender
    "Host": "SFTP server", // The host name or IP address of the SFTP server
    "Port": 587, // The port number of the SFTP server
    "Username": "SFTP username", // The username for the SFTP server
    "Password": "SFTP password", // The password for the SFTP server
  }
}
```

#### 3. Create Database

You need to create the database and a first user.

```bash
cd LGDXRobotCloud.Data
dotnet ef database update && dotnet run --initialiseData "true"  --email "email@example.com" --fullName "Full Name" --userName "admin" --password "password"
```

#### 4. Run

You can run the application using `dotnet run` in below projects:

* LGDXRobotCloud.API
* LGDXRobotCloud.UI
* LGDXRobotCloud.Worker

#### 5. Extra Configuration

A map is required for the application to work, you should generate a map in ROS2 Nav2 stack.

1. Go to Navigation -> Realms and select "View" for "First Realm"
2. Convert the map from PGM to PNG, then upload the image
3. Update Resolution, Origin X, Origin Y, Origin Rotation
4. Restart LGDXRobotCloud.UI

## Development Notes

Use the following command to generate the client in LGDXRobotCloud.UI:

```bash
kiota generate -l CSharp -c LgdxApiClient -n LGDXRobotCloud.UI.Client -o ./Client -d https://localhost:5163/swagger/v1/swagger.json --clean-output
```
