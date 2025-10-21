namespace Mairie.API.Infrastructure.Security
{
    public static class DemandeRequirement
    {
        public static OwnsDemandeRequirement Create = new("Create");
        public static OwnsDemandeRequirement Read = new("Read");
        public static OwnsDemandeRequirement Update = new("Update");
        public static OwnsDemandeRequirement Delete = new("Delete");
        public static OwnsDemandeRequirement Approve = new("Approve");
    }
}
