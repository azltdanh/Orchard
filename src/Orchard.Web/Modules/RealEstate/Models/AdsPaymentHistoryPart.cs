using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Users.Models;

namespace RealEstate.Models
{
    public class AdsPaymentHistoryPartRecord : ContentPartRecord
    {
        public virtual UserPartRecord User { get; set; }
        public virtual UserPartRecord UserPerform { get; set; }
        public virtual long EndBalance { get; set; }
        public virtual long StartBalance { get; set; }
        //xem giá trị trước giao dịch trên cùng hàng hiện tại thay vì phải xem ở hàng trước đó.
        public virtual PropertyPartRecord Property { get; set; }
        public virtual AdsPaymentConfigPartRecord PaymentConfig { get; set; }
        public virtual DateTime? DateTrading { get; set; }
        public virtual string Note { get; set; }
        public virtual bool PayStatus { get; set; }
        public virtual int PostingDates { get; set; }
        public virtual bool IsInternal { get; set; }
        public virtual long TransactionValue { get; set; } //Giá trị giao dịch
    }

    public class AdsPaymentHistoryPart : ContentPart<AdsPaymentHistoryPartRecord>
    {
        public UserPartRecord User
        {
            get { return Record.User; }
            set { Record.User = value; }
        }

        public UserPartRecord UserPerform
        {
            get { return Record.UserPerform; }
            set { Record.UserPerform = value; }
        }

        public long EndBalance
        {
            get { return Retrieve(r => r.EndBalance); }
            set { Store(r => r.EndBalance, value); }
        }

        public long StartBalance
        {
            get { return Retrieve(r => r.StartBalance); }
            set { Store(r => r.StartBalance, value); }
        }

        public PropertyPartRecord Property
        {
            get { return Record.Property; }
            set { Record.Property = value; }
        }

        public AdsPaymentConfigPartRecord PaymentConfig
        {
            get { return Record.PaymentConfig; }
            set { Record.PaymentConfig = value; }
        }

        public DateTime? DateTrading
        {
            get { return Retrieve(r => r.DateTrading); }
            set { Store(r => r.DateTrading, value); }
        }

        public string Note
        {
            get { return Retrieve(r => r.Note); }
            set { Store(r => r.Note, value); }
        }

        public bool PayStatus
        {
            get { return Retrieve(r => r.PayStatus); }
            set { Store(r => r.PayStatus, value); }
        }

        public int PostingDates
        {
            get { return Retrieve(r => r.PostingDates); }
            set { Store(r => r.PostingDates, value); }
        }

        public bool IsInternal
        {
            get { return Retrieve(r => r.IsInternal); }
            set { Store(r => r.IsInternal, value); }
        }

        public long TransactionValue
        {
            get { return Retrieve(r => r.TransactionValue); }
            set { Store(r => r.TransactionValue, value); }
        }
    }
}