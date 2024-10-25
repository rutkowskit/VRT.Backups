# VRT Backups App

The main purpose of this application, is creating backups according to the schedule.

## Installation on Windows machine

1. Open command prompt
1. Change directory to **VRT.Backups.Worker** 
1. Execute command ``` dotnet publish -c Release --self-contained ```
1. Copy content of the ``` .\bin\Release\net9.0\publish ``` folder to the destination folder (e.g. ``` d:\services\VRT.Backups ```)
1. Execute command ``` sc.exe create VRT.Backups start=delayed-auto binpath="d:\services\VRT.Backups\VRT.Backups.Worker.exe" displayname=VRT.Backups ```


## Current Features
1. Backups Mssql database
1. Supports advanced scheduling using Quartz library
1. Cleanup old backup files

## Roadmap
1. Add the function to backup files and directories
1. Add the function to encrypt backup files
1. ~~Add the function of deleting old backup files~~