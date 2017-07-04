# IISplant
This is an IIS-based implant. It requires elevated permissions, but will create an Application in IIS that runs as System. The payload is a very simple aspx web shell. 

How it works:

It uses the Microsoft.Web.Administration.ServerManager to query IIS for currently installed sites, and prompts you for a choice. It will then inject an application with a random name into the site, which will use a System-level App Pool. The application is out of the temp folder, and it will create a junction to the original site's bin directory. After that, it will drop payload.dat (in this case, a aspx web shell) into the folder. Now the victim's web page will have a elevated web shell at /[random-string]/index.aspx. 

Why?

This was mainly created as an experiment to identify some persistence methods within IIS. The goal was to have a privelged web shell running inside of a site. It could serve as a foothold onto a server that is not as likely to be detected as standard perstistence techniques (but may be overwritten by web deployments or easily spotted by an admin checking app pools in IIS).
