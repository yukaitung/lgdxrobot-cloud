#!/bin/bash
export PATH="$PATH:/root/.dotnet/tools"
dotnet-ef database update --context LgdxContext
dotnet-ef database update --context LgdxLogsContext
dotnet run --initialiseData "true" --email "email@example.com" --fullName "Full Name" --userName "admin" --password "123456"