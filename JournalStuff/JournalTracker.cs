using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        private bool firstRan = false,firstRunning=false;

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
                main=new Timer(Loop,null,0,100);
                this.vaProxy = vaProxy;
                this.controller = controller;
                trigger += handle();
            }
        }
        private void first(string[] lines)
        {
            if (firstRan || firstRunning) return;
            firstRunning=true;
            vaProxy.WriteToLog("Running First","blue");
            foreach (string line in lines)
            {
                trigger(line);
            }
            vaProxy.WriteToLog("First finished","blue");
            firstRan = true;
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
                        if(firstRan){
                            foreach (string line in lines)
                            {
                                trigger(line);
                            }
                        }else
                            first(lines);
                    }
                }
                lastsize = thisSize;
            }
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
                        FileInfo info = (from f in directory.GetFiles() where filter == null || filter.IsMatch(f.Name) select f).OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                        if (info != null)
                        {
                            info.Refresh();
                        }
                        return info;
                    }
                    catch (Exception ex)
                    { vaProxy.WriteToLog(ex.Message, "orange"); }
                }
            }
            return null;
        }

        public void start()
        {
            if (hasStarted) return;
            main.Change(0,Timeout.Infinite);
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
                        Dictionary<string, object> data = recursiveDeserialize(line);
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
                        vaProxy.WriteToLog((string)data["event"], "orange");
                        switch ((string)data["event"])
                        {
                            case "Docked":
                                {
                                    try
                                    {
                                        vaProxy.WriteToLog("Docked", "green");
                                        vaProxy.SessionState["Docked"] = true;
                                        vaProxy.SessionState["currentStation"] = new Station((string)data["StationName"], ref vaProxy, ((List<object>)data["StationServices"]).ToArray());
                                        vaProxy.SessionState["inStarport"]=false;
                                        //TODO:allow for use for automation purposes
                                    }
                                    catch (Exception ex)
                                    {
                                        vaProxy.WriteToLog(ex.Message, "red");
                                    }
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
                catch (Exception ex) { vaProxy.WriteToLog(ex.Message, "red"); }
            };
        }
        private Dictionary<string, object> recursiveDeserialize(string input)
        {
            Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(input);
            foreach (var key in data.Keys.ToArray())
            {
                var Value = data[key];
                if (Value is JObject)
                {
                    data[key] = recursiveDeserialize((Value as JObject).ToObject<Dictionary<string, object>>());
                }
                if (Value is JArray)
                {
                    data[key] = recursiveDeserialize(Value as JArray);
                }
            }
            return data;
        }
        private IDictionary<string, object> recursiveDeserialize(Dictionary<string, object> data)
        {
            foreach (var key in data.Keys.ToArray())
            {
                var value = data[key];
                if (value is JObject)
                    if ((value as JObject).ToObject<Dictionary<string, object>>() != null)
                        data[key] = recursiveDeserialize((value as JObject).ToObject<Dictionary<string, object>>());
                    else
                        data[key] = null;

                if (value is JArray)
                    data[key] = recursiveDeserialize(value as JArray);
            }

            return data;
        }

        private IList<object> recursiveDeserialize(JArray data)
        {
            var list = data.ToObject<List<object>>();

            for (int i = 0; i < list.Count; i++)
            {
                var value = list[i];
                if (value is JObject)
                    if ((value as JObject).ToObject<Dictionary<string, object>>() != null)
                        list[i] = recursiveDeserialize((value as JObject).ToObject<Dictionary<string, object>>());

                if (value is JArray)
                    list[i] = recursiveDeserialize(value as JArray);
            }
            return list;
        }
    }
}