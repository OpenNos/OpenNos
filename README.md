[<img src="https://discordapp.com/api/guilds/210791003735457793/widget.png?style=shield">](https://discord.gg/qdPMDv5)
[<img src="https://img.shields.io/badge/Support-Us!-blue.svg">](https://www.paypal.me/OpenNosServer)

#Information!#
This repository is no longer our main development repository. Code is now hosted privately until we think it is somewhat stable. When it is, we push all changes back to GitHub. Before you ask: No, you won't get access to the Repository unless you are:
* a developer that is in our team
* a tester invited by us
Please do not ask us for invitation. You won't get one.

#Instructions to contribute#

##Disclaimer##
This project is a community project not for commercial use. The emulator itself is proof of concept of our idea to try out anything what's not possible on original servers. The result is to learn and program together for prove the study. 

##Legal##

This Website and Project is in no way affiliated with, authorized, maintained, sponsored or endorsed by Gameforge or any of its affiliates or subsidiaries. This is an independent and unofficial server for educational use ONLY. Using the Project might be against the TOS.

#Attention!#
This emulation software is not open source to host any private servers. It is open source to work together community-based.
We do not provide any modified client files. The alorithms are based on our logic.

###Before opening new issues troubleshooting can be found [here](TROUBLESHOOTING.md)###
###Read our [faq](FAQ.md) before asking questions###
###Contribution is only possible with Visual Studio 2015 (Community or other editions), Microsoft SQL Server 2016, [StyleCop extension](https://stylecop.codeplex.com/) and [ResX Resource Manager](https://resxresourcemanager.codeplex.com/)###

#Building the code#
##1. Install SSDT For Visual Studio##
http://go.microsoft.com/fwlink/?LinkID=393520&clcid=0x409

##2. Install or Configure Microsoft SQL Server 2016 (at least Developer Edition)
Detailed information will follow.
- https://www.microsoft.com/en-us/sql-server/sql-server-editions-developers
- SQL Server Management Studio (SSMS) Link : https://msdn.microsoft.com/en-us/library/mt238290.aspx
- Tutorial pastebin Link : http://pastebin.com/gRVENLFm

##3. Use the NuGet Package Manager to Update the Database##

- Go to Tools -> NuGet Package Manager -> Package Manager Console
- Choose Project OpenNos.DAL.EF
- Type 'update-database' and update the Database
