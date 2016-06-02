using Orchard.Users.Models;
namespace RealEstate.Models
{
    public class PropertyExchangePartRecord
    {
        public virtual int Id { get; set; }
        public virtual UserPartRecord User { get; set; }
        public virtual PropertyPartRecord Property { get; set; }
        public virtual CustomerPartRecord Customer { get; set; }
        public virtual ExchangeTypePartRecord ExchangeType { get; set; }
        public virtual double ExchangeValues { get; set; }
        public virtual PaymentMethodPartRecord PaymentMethod { get; set; }
    }
}
