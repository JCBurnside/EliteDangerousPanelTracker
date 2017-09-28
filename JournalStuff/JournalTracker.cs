using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using PanelTrackerPlugin;

namespace PanelTrackerPlugin
{
    public delegate void JournalEvent(string line);
    public class JournalTracker
    {
        private dynamic vaProxy;
        private Controller controller;

        public static Regex json = new Regex(@"^{.*}$");

        public event JournalEvent trigger;
        public bool hasStarted = false;
        private Timer main;
        private string dir;
        private Regex regex;
        private long lastsize = 0;

        private string lastname = null;
        internal JournalTracker(string dir, Regex regex, dynamic vaProxy, Controller controller)
        {
            if (dir == null || dir.Trim() == "")
            {
                throw new Exception("No Directory");
            }
            else
            {
                this.dir = dir;
                this.regex = regex;
                main = new Timer(new TimerCallback(Loop), null, Timeout.Infinite, 100);
                this.vaProxy = vaProxy;
                this.controller = controller;
            }
        }

        internal void setController(Controller c)
        {
            this.controller = c;
        }

        internal void setVaProxy(dynamic vaProxy)
        {
            this.vaProxy = vaProxy;
        }

        private void Loop(object state)
        {
            FileInfo fileInfo = FindLatestFile(dir, regex);
            if (fileInfo == null || fileInfo.Name != lastname)
            {
                lastname = fileInfo == null ? null : fileInfo.Name;
                lastsize = 0;
            }
            else
            {
                long thisSize = fileInfo.Length;
                long seekpos = 0;
                int readLen = 0;
                if (lastsize != thisSize)
                {
                    switch (lastsize.CompareTo(thisSize))
                    {
                        case -1:
                            seekpos = lastsize;
                            readLen = (int)(thisSize - lastsize);
                            break;
                        case 1:
                            seekpos = 0L;
                            readLen = (int)thisSize;
                            break;
                    }
                    using (FileStream fs = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        fs.Seek(seekpos, SeekOrigin.Begin);
                        byte[] bytes = new byte[readLen];
                        int haveRead = 0;
                        while (haveRead < readLen)
                        {
                            haveRead += fs.Read(bytes, haveRead, readLen - haveRead);
                            fs.Seek(seekpos + haveRead, SeekOrigin.Begin);
                        }
                        string s = Encoding.UTF8.GetString(bytes);
                        string[] lines = Regex.Split(s, "\r?\n");
                        foreach (string line in lines)
                        {
                            trigger(line);
                        }
                    }
                }
                lastsize = thisSize;
            }
        }

        public JournalTracker addListener(JournalEvent listener)
        {
            trigger += listener;
            return this;
        }

        public FileInfo FindLatestFile(string dir, Regex filter)
        {
            if (dir != null)
            {
                DirectoryInfo directory = new DirectoryInfo(dir);
                if (directory != null)
                {
                    try
                    {
                        FileInfo info = directory.GetFiles().Where(f => filter == null || filter.IsMatch(f.Name)).OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                        if (info != null)
                        {
                            info.Refresh();
                        }
                        return info;
                    }
                    catch
                    { }
                }
            }
            return null;
        }

        public void start()
        {
            main.Change(0, 100);
            hasStarted = true;
        }

        public void stop()
        {
            main.Change(Timeout.Infinite, 0);
            main.Dispose();
            hasStarted = false;
        }
        public JournalEvent handle()
        {
            return (string line) =>
            {
                try
                {
                    Match match = json.Match(line);
                    if (match.Success)
                    {
                        Dictionary<string, object> data = recursiveDeserialize(line, vaProxy);
                        if (data.ContainsKey("timestamp"))
                        {
                            data["timestamp"] = ((DateTime?)data["timestamp"] ?? DateTime.Parse((string)data["timestamp"])).ToUniversalTime();
                        }
                        else
                        {
                            data["timestamp"] = DateTime.Now.ToUniversalTime();
                        }
                        if (!data.ContainsKey("event"))
                            return;
                        switch ((string)data["event"])
                        {
                            case "Docked":{
                                vaProxy.WriteToLog("Docked", "green");
                                vaProxy.SessionState["Docked"] = true;
                                vaProxy.SessionState["currentStation"] = new Station((string)data["StationName"], (string[])data["StationServices"]);
                                vaProxy.WriteToLog(vaProxy.SessionState["currentStation"].ToString(),"green");
                            //TODO:allow for use for automation purposes
                            }
                            break;
                            case "Undocked":
                                vaProxy.SessionState["Docked"] = false;
                                vaProxy.WriteToLog("UNDOCKED", "green");
                            // vaProxy.SessionState["station"] = new Station(null, new string[] { });
                            //TODO:reset variables changed by previous
                            break;
                            case "Touchdown":
                            //TODO:Allow certain automations
                            vaProxy.SessionState["Landed"] = true;
                                break;
                            case "Liftoff":
                            //TODO:Disallow certain automations
                            break;
                            case "FSDJump":
                                vaProxy.SessionState["jumping"] = false;
                            //TODO:reallow panel switching
                            break;
                            case "StartJump":
                                if ((string)data["JumpType"] == "Hyperspace")
                                {
                                    vaProxy.SessionState["jumping"] = true;
                                //TODO:disallow panel switching
                            }
                                break;
                            case "loadout":
                            // Might add this one day
                            break;
                            case "QuitACrew":
                            case "SelfDestruct":
                            case "ShipyardBuy":
                            case "ShipyardSwap":
                            case "Died":
                                PluginMain.setValues(vaProxy);
                                break;
                        // case "DockingCancelled": // Unsure of this one do the same thing.  Proceeding to test.
                        case "LaunchSRV":
                                vaProxy.SessionState["inSrv"] = true;
                                break;
                            case "DockSRV":
                                vaProxy.SessionState["inSrv"] = false;
                                break;
                            case "Interdiction":
                                if (vaProxy.GetBool("AutoClosePanels"))
                                {
                                    controller.changePanel(Panels.None, vaProxy);
                                }
                            //TODO: allow for auto closing of panels
                            break;
                            case "CrewHire":
                                break;
                            case "CrewFire":
                                break;
                            case "JoinACrew":
                                break;
                            case "ChangeCrewRole":
                                break;
                            case "LoadGame":
                                vaProxy.WriteToLog("Setting Enabled to " + (Process.GetProcessesByName("EliteDangerous64").Length >= 1), "orange");
                                vaProxy.SessionState["enabled"] = Process.GetProcessesByName("EliteDangerous64").Length >= 1;
                                break;
                            case "Fileheader":
                                vaProxy.SessionState["Loaded"] = false;
                                break;
                        }
                    }
                }
                catch { }
            };
        }
        private static Dictionary<string, object> recursiveDeserialize(string input, dynamic vaProxy)
        {
            Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(input);
            foreach (KeyValuePair<string, object> pair in data)
            {
                if (pair.Value.GetType() == typeof(string) && json.Match((string)pair.Value).Success)
                {
                    data[pair.Key] = recursiveDeserialize((string)pair.Value, vaProxy);
                }
                else if (pair.Value.GetType() == typeof(Array) && pair.Value.GetType().GetElementType() == typeof(string))
                {
                    string[] arr = (string[])pair.Value;
                    object[] output = new object[arr.Length];
                    for (int ctr = 0; ctr < arr.Length; ctr++)
                    {
                        if (json.Match(arr[ctr]).Success)
                        {
                            output[ctr] = recursiveDeserialize(arr[ctr], vaProxy);
                        }
                        else
                        {
                            output[ctr] = arr[ctr];
                        }
                    }
                    data[pair.Key] = output;
                }
            }
            return data;
        }
    }
}