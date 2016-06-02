
namespace RealEstate.Users.ActiveDirectory.Models
{
	public class DomainRecord
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string UserName { get; set; }
		public virtual string Password { get; set; }
	}
}
