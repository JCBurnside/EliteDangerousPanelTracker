namespace PanelTrackerPlugin
{
    internal enum Panels
    {
        None,
        Navigation,
        Comms,
        RolesShip,
        Status,
        RolesSRV
    }
    internal enum NavigationTabs
    {
        Navigation,
        Transaction,
        Contacts,
        SubTargets,
        Inventory
    }

    internal enum CommTabs
    {
        Chat,
        Multicrew,
        Messages,
        Recent,
        Notifactions,
        Settings
    }
    internal enum ShipRoleTabs
    {
        All,
        Helm,
        Fighter,
        SRV,
        Crew
    }
    internal enum SRVRoleTabs
    {
        Helm,
        SRV
    }
    internal enum StatusTabs
    {
        Stats,
        Modules,
        FireGroups,
        Inventory,
        Functions
    }
    internal enum ShipSRVOptions
    {
        Back,
        Details,
        Deploy,
        Picture// IDK why that is a selectable option but it is so I have to account for it.
    }
}