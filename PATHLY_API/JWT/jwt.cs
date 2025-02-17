namespace PATHLY_API.JWT
{
	public class jwt
	{
		public string Key { get; set; }
		public string Issuer { get; set; }
		public string Audience { get; set; }
		public double ExpiresAt { get; set; }
	}
}
