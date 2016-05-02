[![Build Status](http://134.255.238.79:8080/job/OpenNos/badge/icon)](http://134.255.238.79:8080/job/OpenNos/)
#Instructions to contribute

###Contribution is only possible with Visual Studio 2015 (Community or other editions) and MySQL.
#NOTE BEFORE INSTALL#
- Error listen point : - This is WCF error install it or run opennos on visual studio
- What's the command ? : $Help
- Can we have you're packet.txt : No!
- Can we have others files for parsers : Yes simply by exctract them from your client : nslang nsgtd nsetc 
- On login nothing happen : verify you can connect with telnet on the correct port "telnet 127.0.0.1 80" 
if yes you're not on the correct port of your client. If no you bad installed something.

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
