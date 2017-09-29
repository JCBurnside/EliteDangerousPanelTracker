using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using PanelTrackerPlugin;

namespace PanelTrackerPlugin
{
    public class PluginMain
    {
        public static readonly string version = "0.0.1a.10";


        private static Regex json = new Regex(@"^{.*}$");

        static bool _stopVariableToMonitor = false;

        public static String VA_DisplayName()
        {
            return "CMDR Digital_light's Elite Dangerous Panel Tracker";
        }

        public static String VA_DisplayInfo()
        {
            return "Panel Tracker\r\n\r\nJust a simple program to simplify tracking panel selections in VA.\r\n\r\n2017 James Burnside";
        }

        public static Guid VA_Id()
        {
            return new Guid("{0e736219-21ca-4fc1-87f6-0e28c10b50f1}");
        }

        public static void VA_StopCommand()
        {
            _stopVariableToMonitor = true;

        }

        public static void VA_Init1(dynamic vaProxy)
        {
            vaProxy.SessionState.Add("enabled", false);
            vaProxy.WriteToLog("Running init", "green");
            setValues(vaProxy);

            vaProxy.SessionState["journal"] = new JournalTracker(GetSaves(), new Regex(@"^Journal\.[0-9\.]+\.log$"),vaProxy,new Controller(vaProxy));
            vaProxy.SessionState["journal"].start();
        }

        private static string GetSaves()
        {
            IntPtr path;
            int result = SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"), 0, new IntPtr(0), out path);
            if (result >= 0)
            {
                return Marshal.PtrToStringUni(path) + @"\Frontier Developments\Elite Dangerous";
            }
            else
            {
                throw new ExternalException("Failed to find the saved games directory.", result);
            }
        }

        [DllImport("Shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)]Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
        public static void VA_Exit1(dynamic vaProxy)
        {
            if (vaProxy.SessionState["journal"] == null)
            {
                if (vaProxy.SessionState["journal"].hasStarted)
                {
                    vaProxy.SessionState["journal"].stop();
                }
                vaProxy.SessionState["journal"] = null;
            }
        }
        public static void VA_Invoke1(dynamic vaProxy)
        {
            String context = vaProxy.Context;
            Process[] pname = Process.GetProcessesByName("EliteDangerous64");
            vaProxy.WriteToLog("In invoke with "+context??"","orange");
            if (vaProxy.SessionState["journal"] != null && !vaProxy.SessionState["journal"].hasStarted)
            {
                vaProxy.SessionState["journal"].start();
            }
            if (pname.Length < 1)
            {
                vaProxy.SessionState["enabled"] = false;
                vaProxy.WriteToLog("Unable to execute command.  Elite(Client) is not running", "red");
                return;
            }
            if (!vaProxy.SessionState["enabled"])
            {
                vaProxy.WriteToLog("Ship is not ready.", "red");
                return;
            }
            bool srv = vaProxy.SessionState["inSrv"];
            string tab = vaProxy.GetText("Tab");
            Controller controller = new Controller(vaProxy);
            vaProxy.SessionState["journal"].setVaProxy(vaProxy);
            vaProxy.SessionState["journal"].setController(controller);
            switch (context)
            {
                case "Set Default":
                    break;
                case "Launch":
                    if(!vaProxy.SessionState["Docked"]){
                        break;   
                    }
                    controller.changeTabTo(DockPanel.Disembark,vaProxy); 
                    break;
                case "Select Starport Service":
                    if (!vaProxy.SessionState["Docked"])
                    {
                        break;
                    }
                    try{
                        
                    if (!vaProxy.SessionState.ContainsKey("currentStation")||vaProxy.SessionState["currentStation"] == null)
                    {
                        vaProxy.WriteToLog("No Station", "red");
                        break;
                    }
                    } catch(Exception ex) {
                        
                        vaProxy.WriteToLog(ex.Message, "red");
                        break;
                    }
                    if (!vaProxy.SessionState.ContainsKey("inStarport")||!vaProxy.SessionState["inStarport"])
                    {
                        controller.changeTabTo(DockPanel.Starport, vaProxy);
                        controller.actionProcess("1" + (char)Action.accept, vaProxy);
                        vaProxy.WriteToLog(vaProxy.GetText("target"), "orange");
                        vaProxy.SessionState["inStarport"] = true;
                    }
                    decimal timeInSecs = vaProxy.GetDecimal("waitTime") ?? 3.0m;
                    vaProxy.WriteToLog("Created Station " + vaProxy.SessionState["currentStation"].ToString(), "green");
                    controller.actionProcess(vaProxy.SessionState["currentStation"].generateAction(vaProxy.GetText("target"),vaProxy),vaProxy);
                    break;
                case "Close Starport":
                case "Close Starport Services":
                    if (vaProxy.SessionState.ContainsKey("inStarport")&&vaProxy.SessionState["inStarport"])
                    {
                        controller.actionProcess("" + (char)Action.back, vaProxy);
                    }
                    vaProxy.SessionState["inStarport"] = false;
                    break;
                case "Open Navigation Tab":
                    navTab(tab, controller, ref vaProxy);
                    break;
                case "Open Comms Tab":
                    commsTab(tab, controller, ref vaProxy);
                    break;
                case "Open Roles Tab":
                    if (!srv)
                    {
                        rolesShipTab(tab, controller, ref vaProxy);
                    }
                    break;
                case "Open Status Tab":
                    statusTabs(tab, controller, ref vaProxy);
                    break;
                case "Close Panel":
                    controller.changePanel(Panels.None, vaProxy);
                    break;
                case "Open Panel":
                    string panel = vaProxy.GetText("Panel");
                    switch (panel.ToLower())
                    {
                        case "comms":
                            controller.changePanel(Panels.Comms, vaProxy);
                            break;
                        case "navigation":
                        case "nav":
                            controller.changePanel(Panels.Navigation, vaProxy);
                            break;
                        case "roles":
                            controller.changePanel(vaProxy.SessionState["inSrv"] ? Panels.RolesSRV : Panels.RolesShip, vaProxy);
                            break;
                        case "status":
                            controller.changePanel(Panels.Status, vaProxy);
                            break;
                    }

                    break;
                case "Print Current Panel":
                    string s = vaProxy.SessionState["currentPanel"].ToString();
                    s = s != null ? s : Panels.None.ToString();
                    vaProxy.WriteToLog(s, "red");
                    break;
                case "Reset Panel":
                case "":
                case null:
                    break;
            }
            vaProxy.WriteToLog(vaProxy.SessionState["currentPanel"].ToString(), "orange");
        }

        private static void statusTabs(string tab, Controller controller, ref dynamic vaProxy)
        {
            switch (tab)
            {
                case "stats":
                    controller.changeTabTo(StatusTabs.Stats, vaProxy);
                    break;
                case "firegroups":
                case "groups":
                    controller.changeTabTo(StatusTabs.FireGroups, vaProxy);
                    break;
                case "inventory":
                case "inv":
                    controller.changeTabTo(StatusTabs.Inventory, vaProxy);
                    break;
                case "modules":
                case "mod":
                case "mods":
                    controller.changeTabTo(StatusTabs.Modules, vaProxy);
                    break;
                case "functions":
                case "func":
                    controller.changeTabTo(StatusTabs.Functions, vaProxy);
                    break;
            }
        }

        private static void rolesShipTab(string tab, Controller controller, ref dynamic vaProxy)
        {
            switch (tab.ToLower())
            {
                case "all":
                    controller.changeTabTo(ShipRoleTabs.All, vaProxy);
                    break;
                case "helm":
                    controller.changeTabTo(ShipRoleTabs.Helm, vaProxy);
                    break;
                case "crew":
                    controller.changeTabTo(ShipRoleTabs.Crew, vaProxy);
                    break;
                case "srv":
                    controller.changeTabTo(ShipRoleTabs.SRV, vaProxy);
                    break;
                case "fighter":
                    controller.changeTabTo(ShipRoleTabs.Fighter, vaProxy);
                    break;
            }
        }

        private static void commsTab(string tab, Controller controller, ref dynamic vaProxy)
        {
            switch (tab.ToLower())
            {
                case "chat":
                    controller.changeTabTo(CommTabs.Chat, vaProxy);
                    break;
                case "multicrew":
                case "crew":
                case "friends":
                    controller.changeTabTo(CommTabs.Multicrew, vaProxy);
                    break;
                case "messages":
                    controller.changeTabTo(CommTabs.Messages, vaProxy);
                    break;
                case "recent":
                case "histroy":
                    controller.changeTabTo(CommTabs.Recent, vaProxy);
                    break;
                case "notifications":
                    controller.changeTabTo(CommTabs.Inbox, vaProxy);
                    break;
                case "settings":
                    controller.changeTabTo(CommTabs.Settings, vaProxy);
                    break;
            }
        }

        private static void navTab(string tab, Controller controller, ref dynamic vaProxy)
        {
            switch (tab.ToLower())
            {
                case "navigation":
                case "nav":
                    controller.changeTabTo(NavigationTabs.Navigation, vaProxy);
                    break;
                case "transaction":
                case "trans":
                    controller.changeTabTo(NavigationTabs.Transaction, vaProxy);
                    break;
                case "contacts":
                    controller.changeTabTo(NavigationTabs.Contacts, vaProxy);
                    break;
                case "sub targets":
                case "subtargets":
                case "sub":
                    controller.changeTabTo(NavigationTabs.SubTargets, vaProxy);
                    break;
                case "inventroy":
                case "inv":
                    controller.changeTabTo(NavigationTabs.Inventory, vaProxy);
                    break;
            }
        }
        
        public static void setValues(dynamic vaProxy)
        {
            vaProxy.SessionState["currentPanel"] = Panels.None;
            vaProxy.SessionState["currentNavigationTab"] = NavigationTabs.Navigation;
            vaProxy.SessionState["currentCommsTab"] = CommTabs.Chat;
            vaProxy.SessionState["currentShipRolesTab"] = ShipRoleTabs.All;
            vaProxy.SessionState["currentSRVRolesTab"] = SRVRoleTabs.SRV;
            vaProxy.SessionState["currentStatusTab"] = StatusTabs.Stats;
            vaProxy.SessionState["currentDockedTab"] = DockPanel.Starport;
            vaProxy.SessionState["inStarport"] = false;
            vaProxy.SessionState["inSrv"] = false;
            vaProxy.SessionState["Loaded"] = false;
            vaProxy.SessionState["Docked"] = false;
            vaProxy.SessionState["Landed"] = false;
            vaProxy.SessionState["jumping"] = false;
            vaProxy.SessionState["modules"] = null;
            //vaProxy.SessionState.Add("CurrentPanel", Panels.None);
        }

        public static void setValues(ref dynamic vaProxy, Dictionary<string, object> data)
        {
        }
    }
    static class Util
    {
        public static T[] Join<T>(this T[] start, params T[][] list)
        {
            var result = new T[list.Length + 1][];
            result[0] = start;
            int pos = 1;
            foreach (T[] t in list) result[pos++] = t;
            return ConcatArrays(result);
        }
        public static T[] ConcatArrays<T>(params T[][] list)
        {
            int count = 0;
            foreach (T[] t in list) count += t.Length;
            var result = new T[count];
            int offset = 0;
            for (int x = 0; x < list.Length; x++)
            {
                list[x].CopyTo(result, offset);
                offset += list[x].Length;
            }
            return result;
        }
    }

}