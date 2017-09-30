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
    public partial class PluginMain{
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
    }
}