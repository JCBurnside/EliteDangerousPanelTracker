using System;
using System.Diagnostics;

namespace PanelTrackerPlugin{
    public partial class PluginMain{
        public static void VA_Invoke1(dynamic vaProxy)
        {
            String context = vaProxy.Context;
            Process[] pname = Process.GetProcessesByName("EliteDangerous64");
            vaProxy.WriteToLog("In invoke with " + context ?? "", "orange");
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
            if (!vaProxy.SessionState["enabled"]&&context!="test process")
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
                case "Process":
                    controller.actionProcess(controller.preProcess(vaProxy.GetText("action")??""),vaProxy);
                    break;
                case "Launch":
                    if (!vaProxy.SessionState["Docked"])
                    {
                        break;
                    }
                    if (vaProxy.SessionState["inStarport"])
                        controller.actionProcess(vaProxy.SessionState["currentStation"].generateAction("Exit", vaProxy), vaProxy);
                    controller.changeTabTo(DockPanel.Disembark, vaProxy);
                    controller.actionProcess("1a", vaProxy);
                    break;
                case "Select Starport Service":
                    if (!vaProxy.SessionState["Docked"])
                    {
                        vaProxy.WriteToLog("Not Docked","red");
                        break;
                    }
                    try
                    {

                        if (!vaProxy.SessionState.ContainsKey("currentStation") || vaProxy.SessionState["currentStation"] == null)
                        {
                            vaProxy.WriteToLog("No Station", "red");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {

                        vaProxy.WriteToLog(ex.Message, "red");
                        break;
                    }
                    if (!vaProxy.SessionState.ContainsKey("inStarport") || !vaProxy.SessionState["inStarport"])
                    {
                        controller.changeTabTo(DockPanel.Starport, vaProxy);
                        controller.actionProcess("accept", vaProxy);
                        vaProxy.WriteToLog(vaProxy.GetText("target"), "orange");
                        vaProxy.SessionState["inStarport"] = true;
                    }
                    vaProxy.WriteToLog((!vaProxy.SessionState.ContainsKey("inStarport") || !vaProxy.SessionState["inStarport"]).ToString(),"orange");
                    decimal timeInSecs = vaProxy.GetDecimal("waitTime") ?? 3.5m;
                    vaProxy.WriteToLog("Created Station " + vaProxy.SessionState["currentStation"].ToString(), "green");
                    controller.actionProcess(vaProxy.SessionState["currentStation"].generateAction(vaProxy.GetText("target"), vaProxy), vaProxy);
                    break;
                case "Close Starport":
                case "Close Starport Services":
                    if (vaProxy.SessionState.ContainsKey("inStarport") && vaProxy.SessionState["inStarport"])
                    {
                        controller.actionProcess(vaProxy.SessionState["currentStation"].generateAction("Exit", vaProxy), vaProxy);
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
                            controller.changePanel(Panels.Targets, vaProxy);
                            break;
                        case "roles":
                            controller.changePanel(vaProxy.SessionState["inSrv"] ? Panels.RolesSRV : Panels.RolesShip, vaProxy);
                            break;
                        case "systems":
                            controller.changePanel(Panels.Systems, vaProxy);
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

    }
}