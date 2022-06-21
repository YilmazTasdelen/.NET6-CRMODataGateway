namespace CRMODataGateway.Models
{
    public class User
    {
        public string userName { get; set; }
        public string DomainName { get; set; }
		public Guid SystemUserId { get; set; }
		public Guid Dealer { get; set; }
		public string TerritoryId { get; set; }
		public string TerritoryIdName { get; set; }
		public Guid OrganizationId { get; set; }
		public string OrganizationIdName { get; set; }
		public Guid BusinessUnitId { get; set; }
		public string BusinessUnitIdName { get; set; }
		public string Title { get; set; }
		public int? IsDisabled { get; set; }
		public int? AccessMode { get; set; }
		public string AccessCode { get; set; }

	}
}
