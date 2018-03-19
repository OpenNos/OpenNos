# Errors #
1. Database issues
  * 'update-database' doesn't work i get "No migration ... information".
  * Table 'account' already exists.
  * Wrong password, but i set it properly in database.

2. Nu-Get issues
  * Project 'OpenNos.{name}' failed to build.
 
3. Connection issues
  * 'There was no endpoint listening at ... see more details'
  * Can login but cant join channel
  * On login nothing happens
  * Wrong password
  
4. Projects not loading
  * cannot load project
  
5. World Issues
  * Monsters don't move
  * Recipes donesn't work

# Fixes #
* Database issues
  1. Change Default project in Package Manager Console to OpenNos.EF.MySQL.
  2. Before using command update-database, make sure you dropped the opennos schema.
  3. Make sure you're using latest version of client with ip(127.0.0.1). (Look also on this cool [project](https://github.com/genyx/OpenNosClientLauncher))

* Nu-Get issues
  1. Make sure, that you used restore nu-get packages before trying to compile the project.

* Connection issues
  1. This issue is often caused by occupied ports 80 or 443, make sure all programs that use this ports are disabled (eg. Skype).
  2. Often caused by wrong client crypto make sure that you use latest version of client
  3. verify you can connect with telnet on the correct port "telnet 127.0.0.1 80". if yes you're not on the correct port of your client. If not, you installed something wrong, check if you have disabled any programs working on port 80(eg.skype).
  4. verify that your password is hash in sha512 and that your launcher(made it yourself) is done with the most recent nostaleX.dat

* Projects not loading
  1. Its caused because of missing stylecop please install it from [here](https://stylecop.codeplex.com/)
  
* World Issues
  1. parse mv packets.
  2. parse each recipe by clicking on them in game.

# Unexpected behavior #
- If project behaves unstable/improperly or something is off try to open a new issue explaining in details your problem with server
- You can also contact us on [our discord server](https://discordapp.com/invite/N8eqPUh).
