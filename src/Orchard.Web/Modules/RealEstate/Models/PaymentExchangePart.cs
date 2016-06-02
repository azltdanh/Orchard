using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    // ReSharper disable InconsistentNaming
    public class PaymentExchangePartRecord : ContentPartRecord
    {
        public virtual float USD { get; set; }
        public virtual float SJC { get; set; }
        public virtual DateTime Date { get; set; }
    }

    public class PaymentExchangePart : ContentPart<PaymentExchangePartRecord>
    {
        public float USD
        {
            get { return Retrieve(r => r.USD); }
            set { Store(r => r.USD, value); }
        }

        public float SJC
        {
            get { return Retrieve(r => r.SJC); }
            set { Store(r => r.SJC, value); }
        }

        public DateTime Date
        {
            get { return Retrieve(r => r.Date); }
            set { Store(r => r.Date, value); }
        }
    }
    // ReSharper restore InconsistentNaming
}