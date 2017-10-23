namespace PanelTrackerPlugin
{
    public enum Panels
    {
        None,
        Targets,
        Comms,
        RolesShip,
        Systems,
        RolesSRV
    }
    public enum TargetsTabs
    {
        Navigation,
        Transaction,
        Contacts,
        SubTargets,
        Inventory
    }

    public enum CommTabs
    {
        Chat,
        Multicrew,
        Requests,
        Recent,
        Inbox,
        Settings
    }
    public enum ShipRoleTabs
    {
        All,
        Helm,
        Fighter,
        SRV,
        Crew
    }
    public enum SRVRoleTabs
    {
        Helm,
        SRV
    }
    public enum SystemsTabs
    {
        Stats,
        Modules,
        FireGroups,
        Inventory,
        Functions
    }

    public enum DockPanel
    {
        Starport,
        Enter,
        Disembark
    }

    public enum StarportPanels
    {
        Left,
        Middle,
        Right
    }

    public enum Action
    {
        up = 'u',
        down = 'd',
        left = 'l',
        right = 'r',
        accept = 'a',
        back = 'b',
        nextTab = 'n',
        prevTab = 'p'
    }
}