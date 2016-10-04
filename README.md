[![discord](https://img.shields.io/badge/discord-OpenNos-blue.svg?style=flat)](https://discord.gg/N8eqPUh)

#Instructions to contribute#


##Disclaimer##
This project is a community project not for commercial use. The emulator itself is proof of concept of our idea to try out anything what's not possible on original servers. The result is to learn and program together for prove the study. 

##Legal##

This Website and Project is in no way affiliated with, authorized, maintained, sponsored or endorsed by Gameforge or any of its affiliates or subsidiaries. This is an independent and unofficial server for educational use ONLY. Using the Project might be against the TOS.

#Attention!#
This emulation software is not open source to host any private servers. It is open source to work together community-based.

We do not provide any modified client files. The alorithms are based on our logic.

##Before creating issue, you can contact us on Discord.##

##Special Information for Hamachii and VPN Users##
If you want to use the Servers you need to Modify the Program.cs of both OpenNos.Login and OpenNos.World and rebuild the code.
- Change "127.0.0.1" to "HamachiIp" (Eg. "12.34.567.89")
- Dont forget to Modify the app.config of the Login-Server to the correct redirection (<server Name="S1-OpenNos" WorldPort="1337" WorldIp="25.71.84.227" channelAmount="1" />)

###Contribution is only possible with Visual Studio 2015 (Community or other editions) and MySQL. and [StyleCop extension](https://stylecop.codeplex.com/)###
#NOTE BEFORE INSTALL#
- Error listen point : - This is WCF error install it or run opennos on visual studio
- What're the commands? : $Help
- Can we have your packet.txt : No! parse them yourself just sniff them!
- Can we have other files for parser? : Yes, simply by extracting them from your client : nslangdata.dat, nsgtddata.dat, nstcdata.dat
- On login nothing happen : verify you can connect with telnet on the correct port "telnet 127.0.0.1 80". if yes you're not on the correct port of your client. If not, you installed something wrong, check if you have disabled any programs working on port 80(eg.skype).
- If issue still hasnt been fixed look inside our troubleshooting file.
- Password is not recognized : verify that your password is hash in sha512 and that your launcher(made it yourself) is done with the most recent nostaleX.dat
- Monsters don't move : parse mv packets.
- Recipe don't work : parse each recipe by click on them for packets.
- The emulator closes after a few seconds : Please check if port 80 is not already in use (eg.skype...)

##1 Install SSDT For VS##
http://go.microsoft.com/fwlink/?LinkID=393520&clcid=0x409

##2 Install MySQL Installer (Just navigate thru Installer)##
http://dev.mysql.com/downloads/windows/installer/

Installer Packages:
- Custom Installation
  - MySQL Server x64 (Database Server)
  - MySQL Workbench x64 (Data Edtiting)
  - MySQL Notifier x86 (Taskbar Icon Status)
  - MySQL for Visual Studio x86
  - Connector/NET x86 (according to our test theres a issue with 6.9.9 version please use [6.9.8 instead](https://downloads.mysql.com/archives/get/file/mysql-connector-net-6.9.8.msi))
  
- Port: 3306
- User: test
- Password: test

##3 Install MYSQL for Visual Studio##

##4 Use the NuGet Package Manager to Update the Database##

- Go to Tools -> NuGet Package Manager -> Package Manager Console
- Choose Project OpenNos.DAL.EF.MySQL
- Type 'update-database' and update the Database
