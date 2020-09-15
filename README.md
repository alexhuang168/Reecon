# Reecon

Reelix's Recon - A small program for network recon. This program is still in early stages of development and should probably not be used by anyone.
* Version: 0.23
* Build Status: <img src = "https://travis-ci.com/Reelix/Reecon.svg?branch=master" valign="middle" /> <--- This will probably say it's failing until .NET 5 is fully released.
* Requirements: [NMap 7.80+](https://nmap.org/download.html), [Mono 6.10+](https://www.mono-project.com/download/stable/) (Linux) / .NET 4.8 (Windows)
* Recommended:
  * HTTP/S Enumeration: [Gobuster](https://github.com/OJ/gobuster)
  * SMB Enumeration: [smbclient](https://github.com/SecureAuthCorp/impacket/blob/master/examples/smbclient.py)
  * Kerberos Enumeration: [Kerbrute](https://github.com/ropnop/kerbrute), [GetNPUsers](https://github.com/SecureAuthCorp/impacket/blob/master/examples/GetNPUsers.py), [secretsdump](https://github.com/SecureAuthCorp/impacket/blob/master/examples/secretsdump.py)
  * Printer Enumeration: [PRET](https://github.com/RUB-NDS/PRET)	