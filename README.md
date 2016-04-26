[![Build Status](http://134.255.238.79:8080/job/OpenNos/badge/icon)](http://134.255.238.79:8080/job/OpenNos/)
#Instructions to contribute

###Contribution is only possible with Visual Studio 2015 (Community or other editions) and MySQL.

##1 Install SSDT For VS
http://go.microsoft.com/fwlink/?LinkID=393520&clcid=0x409

##2 Install MySQL Installer (Just navigate thru Installer)

http://dev.mysql.com/downloads/windows/installer/

Installer Packages:
- Custom Installation
  - MySQL Server x64 (Database Server)
  - MySQL Workbench x64 (Data Edtiting)
  - MySQL Notifier x86 (Taskbar Icon Status)
  - MySQL for Visual Studio x86
  - Connector/NET x86
  
- Port: 3306
- User: test
- Password. test

##3 Create new schema (opennos)

##4 Install MYSQL for Visual Studio

##5 Use the NuGet Package Manager to Update the Database

- Go to Tools -> NuGet Package Manager -> Package Manager Console
- Choose Project OpenNos.DAL.EF.MySQL
- Type 'update-database' and update the Database
