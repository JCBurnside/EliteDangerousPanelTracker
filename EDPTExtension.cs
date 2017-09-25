using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Web.Script.Serialization;
using PanelTrackerPlugin;

namespace PanelTrackerPlugin{
	public class PluginMain_PanelTracker{

        public static readonly string version = "0.0.1a.10"; 

		static bool _stopVariableToMonitor = false;

		public static String VA_DisplayName(){
			return "CMDR Digital_light's Elite Dangerous Panel Tracker";
		}
		
		public static String VA_DisplayInfo(){
			return "Panel Tracker\r\n\r\nJust a simple program to simplify tracking panel selections in VA.\r\n\r\n2017 James Burnside";
		}
		
		public static Guid VA_Id(){
			return new Guid("{0e736219-21ca-4fc1-87f6-0e28c10b50f1}");
		}
		
		public static void VA_StopCommand(){
			_stopVariableToMonitor = true;
		}

		public static void VA_Init1(dynamic vaProxy){
            vaProxy.WriteToLog("Running init", "green");
            setValues(ref vaProxy);
		} 

		public static void VA_Exit1(dynamic vaProxy){

		}
		public static void VA_Invoke1(dynamic vaProxy)
        {
            Controller c = new Controller(ref vaProxy);
            String context=vaProxy.Context;

            bool srv = false;
            int? nInt = vaProxy.GetInt("CurrentPanel");
            Panels currentPanel = nInt.HasValue ? (Panels)nInt : Panels.None;
            switch (context){
			case "Set Default":
                break;
            case "Open Tab":
                string tab = vaProxy.GetText("Tab");
                int? currentNavTab = vaProxy.GetInt("NavigationTab");
                currentNavTab = currentNavTab.HasValue ? currentNavTab : (int)NavigationTabs.Navigation;
                //vaProxy.WriteToLog(String.Format("target:{0} current:{1}"),tab,currentNavTab.Value.ToString());
                switch(tab.ToLower()){
                case "navigation":
                case "nav":
                    c.changeTabTo(NavigationTabs.Navigation, (NavigationTabs)currentNavTab, currentPanel, ref vaProxy);
                    vaProxy.SetInt("NavigationTab", (int)NavigationTabs.Navigation);
                    break;
                case "transaction":
                case "trans":
                    c.changeTabTo(NavigationTabs.Transaction, (NavigationTabs)currentNavTab, currentPanel, ref vaProxy);
                    vaProxy.SetInt("NavigationTab", (int)NavigationTabs.Transaction);
                    break;
                case "contacts":
                    c.changeTabTo(NavigationTabs.Contacts, (NavigationTabs)currentNavTab, currentPanel, ref vaProxy);
                    vaProxy.SetInt("NavigationTab", (int)NavigationTabs.Contacts);
                    break;
                }
                break;
            case "Close Panel":
                c.changePanel(Panels.None, ref vaProxy);
                break;
            case "Open Panel":
                string panel = vaProxy.GetText("Panel");
                switch (panel.ToLower())
                {
                case "comms":
                    c.changePanel(Panels.Comms,ref vaProxy);
                    break;
                case "navigation":
                case "nav":
                    c.changePanel(Panels.Navigation,ref vaProxy);
                    vaProxy.SessionState["CurrentPanel"] = Panels.Navigation;
                    break;
                case "roles":
                    c.changePanel(Panels.RolesShip,ref vaProxy);
                    break;
                case "status":
                    c.changePanel(Panels.Status, ref vaProxy);
                    break;
                }
                break;
            case "Reset Panel":
            case "":
            case null:
                break;
			}
		}

        private static void setValues(ref dynamic vaProxy)
        {
            vaProxy.SessionState["CurrentPanel"] = Panels.None;
            //vaProxy.SessionState.Add("CurrentPanel", Panels.None);
        }
	}
}