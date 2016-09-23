[![Build Status](http://134.255.238.79:8080/job/OpenNos/badge/icon)](http://134.255.238.79:8080/job/OpenNos/)
[![Build Status](http://91.134.173.36:8080/job/OpenNos/badge/icon)](http://91.134.173.36:8080/job/OpenNos/)

#Errors#
1. Database issues
  * 'update-database' doesn't work i get "No migration ... information".
  * Table 'account' already exists.
  * Wrong password, but i set it properly in database.

2. Nu-Get issues
  * Project 'OpenNos.{name}' failed to build.
 
3. Connection issues
  * 'There was no endpoint listening at ... see more details'
  * Can login but cant join channel
 
#Fixes#
1. Database issues
  1. Change Default project in Package Manager Console to OpenNos.EF.MySQL.
  2. Before using command update-database, make sure you dropped the opennos schema.
  3. Make sure you're using latest version of client with ip(127.0.0.1). (Look also on this cool [project](https://github.com/genyx/OpenNosClientLauncher))

2. Nu-Get issues
  1. Make sure, that you used restore nu-get packages before trying to compile the project.

3. Connection issues
  1. This issue is often caused by occupied ports 80 or 443, make sure all programs that use this ports are disabled (eg. Skype).
  2. Often caused by wrong client crypto make sure that you use latest version of client

#Unexpected behavior#
- If project behaves unstable/improperly or something is off try to open a new issue explaining in details your problem with server
- You can also contact us on [our discord server](https://discordapp.com/invite/N8eqPUh).
