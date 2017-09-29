using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PanelTrackerPlugin;

namespace PanelTrackerPlugin
{
    internal class Controller
    {
        private const string format = "((EDPT:{0}))";
        private Dictionary<String, bool> commands = new Dictionary<string, bool>();
        public Controller(dynamic vaProxy)
        {
            commands.Add("up", vaProxy.CommandExists(String.Format(format, "up")));
            commands.Add("right", vaProxy.CommandExists(String.Format(format, "right")));
            commands.Add("down", vaProxy.CommandExists(String.Format(format, "down")));
            commands.Add("left", vaProxy.CommandExists(String.Format(format, "left")));
            commands.Add("accept", vaProxy.CommandExists(String.Format(format, "accept")));
            commands.Add("back", vaProxy.CommandExists(String.Format(format, "back")));
            commands.Add("nextTab", vaProxy.CommandExists(String.Format(format, "nextTab")));
            commands.Add("prevTab", vaProxy.CommandExists(String.Format(format, "prevTab")));
            commands.Add("navigation", vaProxy.CommandExists(String.Format(format, "navigation")));
            commands.Add("comms", vaProxy.CommandExists(String.Format(format, "comms")));
            commands.Add("roles", vaProxy.CommandExists(String.Format(format, "roles")));
            commands.Add("status", vaProxy.CommandExists(String.Format(format, "status")));
            foreach (KeyValuePair<string, bool> pair in commands)
            {
                if (!pair.Value)
                {
                    vaProxy.WriteToLog(String.Format("Nessary command {0} not found. This may cause issues.", String.Format(format, pair.Key)), "red");
                }
            }
        }

        public void changePanel(Panels panel, dynamic vaProxy)
        {
            // Panels? currentPanel = (Panels?)vaProxy.SessionState["CurrentPanel"];
            Panels currentPanel = vaProxy.SessionState["currentPanel"];
            vaProxy.WriteToLog("Trying to switch to " + panel + " from " + currentPanel, "green");
            vaProxy.WriteToLog(String.Format("CP:{0}", currentPanel), "red");
            bool exists;
            if (currentPanel != panel)
            {
                switch (panel)
                {
                    case Panels.Comms:
                        if (commands["comms"])
                        {
                            vaProxy.ExecuteCommand(String.Format(format, "comms"), true);
                        }
                        break;
                    case Panels.Navigation:
                        commands.TryGetValue("navigation", out exists);
                        if (exists)
                        {
                            vaProxy.ExecuteCommand(String.Format(format, "navigation"), true);
                        }
                        break;
                    case Panels.RolesShip:
                    case Panels.RolesSRV:
                        commands.TryGetValue("roles", out exists);
                        if (exists)
                        {
                            vaProxy.ExecuteCommand(String.Format(format, "roles"), true);
                        }
                        break;
                    case Panels.Status:
                        commands.TryGetValue("status", out exists);
                        if (exists)
                        {
                            vaProxy.ExecuteCommand(String.Format(format, "status"), true);
                        }
                        break;
                    case Panels.None:
                        switch (currentPanel)
                        {
                            case Panels.Comms:
                                commands.TryGetValue("comms", out exists);
                                if (exists)
                                {
                                    vaProxy.ExecuteCommand(String.Format(format, "comms"), true);
                                }
                                break;
                            case Panels.Navigation:
                                commands.TryGetValue("navigation", out exists);
                                if (exists)
                                {
                                    vaProxy.ExecuteCommand(String.Format(format, "navigation"), true);
                                }
                                break;
                            case Panels.RolesShip:
                            case Panels.RolesSRV:
                                commands.TryGetValue("roles", out exists);
                                if (exists)
                                {
                                    vaProxy.ExecuteCommand(String.Format(format, "roles"), true);
                                }
                                break;
                            case Panels.Status:
                                commands.TryGetValue("status", out exists);
                                if (exists)
                                {
                                    vaProxy.ExecuteCommand(String.Format(format, "status"), true);
                                }
                                break;
                        }
                        break;
                }
                vaProxy.SessionState["currentPanel"] = panel;
            }
        }
        public void changeTabTo(NavigationTabs tab, dynamic vaProxy)
        {
            Panels currentPanel = vaProxy.SessionState["currentPanel"];
            NavigationTabs currentTab = vaProxy.SessionState["currentNavigationTab"];
            if (currentPanel != Panels.Navigation)
                changePanel(Panels.Navigation, vaProxy);
            if (currentTab != tab)
            {
                int ctI = (int)currentTab,
                    ntI = (int)tab;
                if (ctI > ntI)
                {
                    actionProcess((ctI - ntI).ToString() + (char)Action.prevTab, vaProxy);
                }
                else
                {
                    actionProcess((ntI - ctI).ToString() + (char)Action.nextTab, vaProxy);
                }
            }
            vaProxy.SessionState["currentNavigationTab"] = tab;
        }
        public void changeTabTo(CommTabs tab, dynamic vaProxy)
        {
            Panels currentPanel = vaProxy.SessionState["currentPanel"];
            CommTabs currentTab = vaProxy.SessionState["currentCommTab"];
            if (currentPanel != Panels.Comms)
                changePanel(Panels.Comms, vaProxy);
            if (currentTab != tab)
            {
                int ctI = (int)currentTab,
                    ntI = (int)tab;
                if (ctI > ntI)
                {
                    actionProcess((ctI - ntI).ToString() + (char)Action.prevTab, vaProxy);
                }
                else
                {
                    actionProcess((ntI - ctI).ToString() + (char)Action.nextTab, vaProxy);
                }
            }
            vaProxy.SessionState["currentCommTab"] = tab;
        }
        public void changeTabTo(ShipRoleTabs tab, dynamic vaProxy)
        {
            Panels currentPanel = vaProxy.SessionState["currentPanel"];
            ShipRoleTabs currentTab = vaProxy.SessionState["currentShipRolesTab"];
            if (currentPanel != Panels.RolesShip)
                changePanel(Panels.RolesShip, vaProxy);
            if (currentTab != tab)
            {
                int ctI = (int)currentTab,
                    ntI = (int)tab;
                if (ctI > ntI)
                {
                    actionProcess((ctI - ntI).ToString() + (char)Action.prevTab, vaProxy);
                }
                else
                {
                    actionProcess((ntI - ctI).ToString() + (char)Action.nextTab, vaProxy);
                }
            }
            vaProxy.SessionState["currentShipRolesTab"] = tab;
        }
        public void changeTabTo(SRVRoleTabs tab, dynamic vaProxy)
        {
            Panels currentPanel = vaProxy.SessionState["currentPanel"];
            SRVRoleTabs currentTab = vaProxy.SessionState["currentSRVRolesTab"];
            if (currentPanel != Panels.RolesSRV)
                changePanel(Panels.RolesSRV, vaProxy);
            if (currentTab != tab)
            {
                int ctI = (int)currentTab,
                    ntI = (int)tab;
                if (ctI > ntI)
                {
                    actionProcess((ctI - ntI).ToString() + (char)Action.prevTab, vaProxy);
                }
                else
                {
                    actionProcess((ntI - ctI).ToString() + (char)Action.nextTab, vaProxy);
                }
            }
            vaProxy.SessionState["currentSRVRole"] = tab;
        }
        public void changeTabTo(StatusTabs tab, dynamic vaProxy)
        {
            Panels currentPanel = vaProxy.SessionState["currentPanel"];
            StatusTabs currentTab = vaProxy.SessionState["currentStatusTab"];
            if (currentPanel != Panels.Status)
                changePanel(Panels.Status, vaProxy);
            if (currentTab != tab)
            {
                int ctI = (int)currentTab,
                    ntI = (int)tab;
                if (ctI > ntI)
                {
                    actionProcess((ctI - ntI).ToString() + (char)Action.prevTab, vaProxy);
                }
                else
                {
                    actionProcess((ntI - ctI).ToString() + (char)Action.nextTab, vaProxy);
                }
            }
            vaProxy.SessionState["currentStatusTab"] = tab;
        }

        public void changeTabTo(DockPanel target, dynamic vaProxy)
        {
            changePanel(Panels.None, vaProxy);
            DockPanel currentTab = vaProxy.SessionState["currentDockedTab"];
            if (target != currentTab)
            {
                int cpos = (int)currentTab,
                    tpos = (int)target;
                if (cpos > tpos)
                {
                    actionProcess((cpos - tpos).ToString() + (char)Action.up, vaProxy);
                }
                else
                {
                    actionProcess((tpos - cpos).ToString() + (char)Action.down, vaProxy);
                }
            }
            vaProxy.SessionState["currentDockedTab"] = target;
        }

        public void actionProcess(string input, dynamic vaProxy)
        {
            try
            {
                string[] actions = input.Contains(';') ? input.TrimEnd().Split(';') : new string[] { input };
                foreach (string action in actions)
                {
                    vaProxy.WriteToLog(action,"green");

                    bool exists;
                    Action task = (Action)action.ToLower()[action.Length - 1];
                    int count = 1;
                    string possiblities = "";
                    foreach (Action a in Enum.GetValues(typeof(Action)))
                    {
                        possiblities += (char)a;
                    }
                    Regex formated = new Regex(@"^[0-9]+[" + possiblities + "]$");
                    if (formated.Match(action.ToLower()).Success) count = int.Parse(action.Substring(0, action.Length - 1));
                    if (count <= 0) count = 1;
                    switch (task)
                    {
                        case Action.up:
                            commands.TryGetValue("up", out exists);
                            if (exists)
                                for (; count > 0; count--)
                                    vaProxy.ExecuteCommand(String.Format(format, "up"), true);
                            else
                                vaProxy.WriteToLog("Cannot Execute Because up Command Is Missing");
                            break;
                        case Action.right:
                            commands.TryGetValue("right", out exists);
                            if (exists)
                                for (; count > 0; count--)
                                    vaProxy.ExecuteCommand(String.Format(format, "right"), true);
                            else
                                vaProxy.WriteToLog("Cannot Execute Because right Command Is Missing");
                            break;
                        case Action.down:
                            commands.TryGetValue("down", out exists);
                            if (exists)
                                for (; count > 0; count--)
                                    vaProxy.ExecuteCommand(String.Format(format, "down"), true);
                            else
                                vaProxy.WriteToLog("Cannot Execute Because down Command Is Missing");
                            break;
                        case Action.left:
                            commands.TryGetValue("left", out exists);
                            if (exists)
                                for (; count > 0; count--)
                                    vaProxy.ExecuteCommand(String.Format(format, "left"), true);
                            else
                                vaProxy.WriteToLog("Cannot Execute Because left Command Is Missing");
                            break;
                        case Action.nextTab:
                            commands.TryGetValue("nextTab", out exists);
                            if (exists)
                                for (; count > 0; count--)
                                    vaProxy.ExecuteCommand(String.Format(format, "nextTab"), true);
                            else
                                vaProxy.WriteToLog("Cannot Execute Because nextTab Command Is Missing");
                            break;
                        case Action.prevTab:
                            commands.TryGetValue("prevTab", out exists);
                            if (exists)
                                for (; count > 0; count--)
                                    vaProxy.ExecuteCommand(String.Format(format, "prevTab"), true);
                            else
                                vaProxy.WriteToLog("Cannot Execute Because prevTab Command Is Missing");
                            break;
                        case Action.accept:
                            commands.TryGetValue("accept", out exists);
                            if (exists)
                                for (; count > 0; count--)
                                    vaProxy.ExecuteCommand(String.Format(format, "accept"), true);
                            else
                                vaProxy.WriteToLog("Cannot Execute Because accept Command Is Missing");
                            break;
                        case Action.back:
                            commands.TryGetValue("back", out exists);
                            if (exists)
                                for (; count > 0; count--)
                                    vaProxy.ExecuteCommand(String.Format(format, "back"), true);
                            else
                                vaProxy.WriteToLog("Cannot Execute Because back Command Is Missing");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                vaProxy.WriteToLog(ex.Message,"red");
            }
        }
    }
}
