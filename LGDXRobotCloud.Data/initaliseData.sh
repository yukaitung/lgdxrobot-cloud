#!/bin/bash

sleep 1s
dotnet LGDXRobotCloud.Data.dll --initialiseData "true" --email "email@example.com" --fullName "Full Name" --userName "admin" --password "123456" --seedData "true"