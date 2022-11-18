# Hi Weclcom to use this tool to generate your db script

## Command Notes

Open `Powershell` or `CMD`, enter into this project folder, then use the tow commands below to execute database migration:

+ dotnet ef migrations add <current_change_title>
+ dotnet ef migrations script -o MigrationSqls\<sql_file_name>.sql

### Create a database

Open `Powershell` or `CMD`, enter into this project folder, then use the this command to create a test database.

+ dotnet ef database update