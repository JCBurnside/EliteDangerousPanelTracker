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
    public partial class PluginMain
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
            setValues(vaProxy);

            vaProxy.SessionState["journal"] = new JournalTracker(GetSaves(), new Regex(@"^Journal\.[0-9\.]+\.log$"), vaProxy, new Controller(vaProxy));
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

}