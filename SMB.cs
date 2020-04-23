﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Reecon
{
    class SMB
    {
        public static string TestAnonymousAccess(string IP, string username = "", string password = "")
        {
            string toReturn = "";
            try
            {
                using (SMB_NetworkShareAccesser.Access(IP, username, password))
                {
                    SMB_GetNetShares getNetShares = new SMB_GetNetShares();
                    List<SMB_GetNetShares.SHARE_INFO_1> shareInfoList = getNetShares.EnumNetShares(IP).ToList();
                    foreach (var shareInfo in shareInfoList)
                    {
                        toReturn += Environment.NewLine + " - Anonymous Share: " + shareInfo.shi1_netname + " - " + shareInfo.shi1_type + " - " + shareInfo.shi1_remark;
                    }
                }
            }
            catch (Exception ex)
            {
                toReturn += Environment.NewLine + " - Unable to Anonymous connect: " + ex.Message;
            }
            return toReturn;
        }

        public static void SMBBrute(string[] args)
        {
            // TODO: This still shows "Success" if:
            // - The username doesn't exist
            // - There is a space in the password
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("SMB Brute only currently works in Linux - Heh :p");
                return;
            }
            if (args.Length != 4)
            {
                Console.WriteLine("SMB Brute Usage: reecon -smb-brute IP Userfile Passfile");
                return;
            }
            string ip = args[1];
            string userFile = args[2];
            string passFile = args[3];
            if (!File.Exists(userFile))
            {
                Console.WriteLine("Unable to find UserFile: " + userFile);
                return;
            }
            if (!File.Exists(passFile))
            {
                Console.WriteLine("Unable to find Passfile: " + passFile);
                return;
            }
            List<string> userList = File.ReadAllLines(userFile).ToList();
            List<string> passList = File.ReadAllLines(passFile).ToList();
            foreach (string user in userList)
            {
                foreach (string pass in passList)
                {
                    List<string> outputResult = General.GetProcessOutput("smbclient", @"-L \\\\" + ip + " -U" + user + "%" + pass);
                    outputResult.RemoveAll(x => x.Equals("Unable to initialize messaging context"));
                    string resultItem = outputResult[0];
                    if (resultItem.Contains("NT_STATUS_HOST_UNREACHABLE"))
                    {
                        Console.WriteLine("Error - Unable to contact \\\\" + ip);
                        return;
                    }
                    else if (resultItem.Contains("NT_STATUS_LOGON_FAILURE"))
                    {
                        Console.WriteLine(user + ":" + pass + " - Failed");
                    }
                    else if (resultItem.Contains("NT_STATUS_UNSUCCESSFUL"))
                    {
                        Console.WriteLine("Fatal Error: " + resultItem);
                        return;
                    }
                    else
                    {
                        Console.WriteLine(user + ":" + pass + " - Success!");
                        return;
                    }
                }
            }
            // smbclient -L \\\\10.10.10.172 -USABatchJobs%SABatchJobs

        }
    }
}
