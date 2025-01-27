# LGDXRobot2-Cloud

XXX

### Links

*   [LGDXRobot2-MCU](https://gitlab.com/yukaitung/lgdxrobot2-mcu)
*   [LGDXRobot2-ChassisTuner](https://gitlab.com/yukaitung/lgdxrobot2-chassistuner)

# How it works

This software is developed with dotnet.

## LGDXRobot2Cloud.API

Dependency

* dotnet EF
  * Microsoft.EntityFrameworkCore
  * Microsoft.EntityFrameworkCore.Tools
  * Pomelo.EntityFrameworkCore.MySql
* AutoMapper

### Functionality

# Getting started

### Prerequisite

SQL: docker run --name postgres -e POSTGRES_PASSWORD=mysecretpassword -e POSTGRES_USER=admin -e POSTGRES_DB=LGDX -v postgres-data:/var/lib/postgresql/data -p 5432:5432 -d postgres

### Build & Run

XXX

```
 kiota generate -l CSharp -c LgdxApiClient -n LGDXRobot2Cloud.UI.Client -o ./Client -d https://localhost:5163/swagger/v1/swagger.json --clean-output
```