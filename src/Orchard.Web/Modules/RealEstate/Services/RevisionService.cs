using System;
using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Users.Models;
using RealEstate.Helpers;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IRevisionService : IDependency
    {
        void CreateRevision(DateTime lastUpdatedDate, UserPartRecord lastUpdatedUser, PropertyPartRecord property,
            string fieldName, object valueBefore, object valueAfter);

        void CreateRevision(DateTime lastUpdatedDate, UserPartRecord lastUpdatedUser, CustomerPartRecord customer,
            string fieldName, object valueBefore, object valueAfter);

        IEnumerable<RevisionPart> GetPropertyRevisions(int id);
        IEnumerable<RevisionPart> GetCustomerRevisions(int id);

        // UserAction
        UserActionPartRecord GetUserAction(int id);
        UserActionPartRecord GetUserAction(string actionCssClass);

        // User Activities
        IEnumerable<UserActivityPartRecord> GetUserActivities();
        IEnumerable<UserActivityPartRecord> GetUserActivitiesCalledByUsers(CustomerPart customer);

        void SaveUserActivityAddNewProperty(DateTime createdDate, UserPart createdUser, PropertyPart property);
        void SaveUserActivityUpdateProperty(DateTime createdDate, UserPart createdUser, PropertyPart property);
        void SaveUserActivityUploadPropertyImages(DateTime createdDate, UserPart createdUser, PropertyPart property);
        void SaveUserActivityOwnPropertyDeleted(DateTime createdDate, UserPart createdUser, PropertyPart property);
        void SaveUserActivityServeCustomer(DateTime createdDate, UserPart createdUser, CustomerPart customer);
        void SaveUserActivityCallCustomer(DateTime createdDate, UserPart createdUser, CustomerPart customer);
    }

    public class RevisionService : IRevisionService
    {
        private readonly IContentManager _contentManager;
        private readonly IRepository<UserActivityPartRecord> _userActivityRepository;

        public RevisionService(
            IRepository<UserActivityPartRecord> userActivityRepository,
            IContentManager contentManager)
        {
            _userActivityRepository = userActivityRepository;
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void CreateRevision(DateTime lastUpdatedDate, UserPartRecord lastUpdatedUser, PropertyPartRecord property,
            string fieldName, object valueBefore, object valueAfter)
        {
            // Revision Record
            var rev = _contentManager.New<RevisionPart>("Revision");
            rev.CreatedDate = lastUpdatedDate;
            rev.CreatedUser = lastUpdatedUser;
            rev.FieldName = fieldName;
            rev.ValueBefore = valueBefore != null ? valueBefore.ToString() : "";
            rev.ValueAfter = valueAfter != null ? valueAfter.ToString() : "";
            rev.ContentType = "Property";
            rev.ContentTypeRecordId = property.Id;

            _contentManager.Create(rev);
        }

        public void CreateRevision(DateTime lastUpdatedDate, UserPartRecord lastUpdatedUser, CustomerPartRecord customer,
            string fieldName, object valueBefore, object valueAfter)
        {
            // Revision Record
            var rev = _contentManager.New<RevisionPart>("Revision");
            rev.CreatedDate = lastUpdatedDate;
            rev.CreatedUser = lastUpdatedUser;
            rev.FieldName = fieldName;
            rev.ValueBefore = valueBefore != null ? valueBefore.ToString() : "";
            rev.ValueAfter = valueAfter != null ? valueAfter.ToString() : "";
            rev.ContentType = "Customer";
            rev.ContentTypeRecordId = customer.Id;

            _contentManager.Create(rev);
        }

        public IEnumerable<RevisionPart> GetPropertyRevisions(int id)
        {
            return
                _contentManager.Query<RevisionPart, RevisionPartRecord>()
                    .Where(a => a.ContentType == "Property" && a.ContentTypeRecordId == id)
                    .OrderByDescending(a => a.CreatedDate)
                    .List();
        }

        public IEnumerable<RevisionPart> GetCustomerRevisions(int id)
        {
            return
                _contentManager.Query<RevisionPart, RevisionPartRecord>()
                    .Where(a => a.ContentType == "Customer" && a.ContentTypeRecordId == id)
                    .OrderByDescending(a => a.CreatedDate)
                    .List();
        }

        // UserAction

        public UserActionPartRecord GetUserAction(int id)
        {
            return _contentManager.Get(id).As<UserActionPart>().Record;
        }

        public UserActionPartRecord GetUserAction(string actionCssClass)
        {
            if (string.IsNullOrEmpty(actionCssClass)) return null;
            return
                _contentManager.Query<UserActionPart, UserActionPartRecord>()
                    .Where(a => a.CssClass == actionCssClass)
                    .List()
                    .First()
                    .Record;
        }

        // User Activities

        public IEnumerable<UserActivityPartRecord> GetUserActivities()
        {
            return _userActivityRepository.Table;
        }

        public IEnumerable<UserActivityPartRecord> GetUserActivitiesCalledByUsers(CustomerPart customer)
        {
            return
                _userActivityRepository.Fetch(
                    a =>
                        a.CustomerPartRecord == customer.Record &&
                        a.UserActionPartRecord.CssClass == "act-call-customer");
        }

        public void SaveUserActivityAddNewProperty(DateTime createdDate, UserPart createdUser, PropertyPart property)
        {
            var act = new UserActivityPartRecord
            {
                CreatedDate = createdDate,
                UserPartRecord = createdUser.Record,
                PropertyPartRecord = property.Record,
                UserActionPartRecord = GetUserAction("act-addnew")
            };

            _userActivityRepository.Create(act);
        }

        public void SaveUserActivityUpdateProperty(DateTime createdDate, UserPart createdUser, PropertyPart property)
        {
            DateTime startDate = DateExtension.GetStartOfDate(createdDate);
            DateTime endDate = DateExtension.GetEndOfDate(createdDate);
            if (
                !_userActivityRepository.Table.Any(
                    a =>
                        a.UserPartRecord.Id == createdUser.Id && a.PropertyPartRecord.Id == property.Id &&
                        a.CreatedDate >= startDate && a.CreatedDate <= endDate))
            {
                var act = new UserActivityPartRecord
                {
                    CreatedDate = createdDate,
                    UserPartRecord = createdUser.Record,
                    PropertyPartRecord = property.Record,
                    UserActionPartRecord = GetUserAction("act-update")
                };

                _userActivityRepository.Create(act);
            }
        }

        public void SaveUserActivityUploadPropertyImages(DateTime createdDate, UserPart createdUser,
            PropertyPart property)
        {
            UserActionPartRecord action = GetUserAction("act-upload-image");

            if (
                !_userActivityRepository.Table.Any(
                    a =>
                        a.UserPartRecord.Id == createdUser.Id && a.PropertyPartRecord.Id == property.Id &&
                        a.UserActionPartRecord == action))
            {
                var act = new UserActivityPartRecord
                {
                    CreatedDate = createdDate,
                    UserPartRecord = createdUser.Record,
                    PropertyPartRecord = property.Record,
                    UserActionPartRecord = action
                };

                _userActivityRepository.Create(act);
            }
        }

        public void SaveUserActivityOwnPropertyDeleted(DateTime createdDate, UserPart createdUser, PropertyPart property)
        {
            var act = new UserActivityPartRecord
            {
                CreatedDate = createdDate,
                UserPartRecord = createdUser.Record,
                PropertyPartRecord = property.Record,
                UserActionPartRecord = GetUserAction("act-deleted")
            };

            _userActivityRepository.Create(act);
        }

        public void SaveUserActivityServeCustomer(DateTime createdDate, UserPart createdUser, CustomerPart customer)
        {
            DateTime startDate = DateExtension.GetStartOfMonth(createdDate);
            DateTime endDate = DateExtension.GetEndOfMonth(createdDate);
            UserActionPartRecord action = GetUserAction("act-customer");

            if (
                !_userActivityRepository.Table.Any(
                    a =>
                        a.UserPartRecord.Id == createdUser.Id && a.CustomerPartRecord.Id == customer.Id &&
                        a.CreatedDate >= startDate && a.CreatedDate <= endDate &&
                        a.UserActionPartRecord == action))
            {
                var act = new UserActivityPartRecord
                {
                    CreatedDate = createdDate,
                    UserPartRecord = createdUser.Record,
                    CustomerPartRecord = customer.Record,
                    UserActionPartRecord = action
                };

                _userActivityRepository.Create(act);
            }
        }

        public void SaveUserActivityCallCustomer(DateTime createdDate, UserPart createdUser, CustomerPart customer)
        {
            DateTime startDate = DateExtension.GetStartOfDate(createdDate);
            DateTime endDate = DateExtension.GetEndOfDate(createdDate);
            UserActionPartRecord action = GetUserAction("act-call-customer");

            if (
                !_userActivityRepository.Table.Any(
                    a =>
                        a.UserPartRecord.Id == createdUser.Id && a.CustomerPartRecord.Id == customer.Id &&
                        a.CreatedDate >= startDate && a.CreatedDate <= endDate &&
                        a.UserActionPartRecord == action))
            {
                var act = new UserActivityPartRecord
                {
                    CreatedDate = createdDate,
                    UserPartRecord = createdUser.Record,
                    CustomerPartRecord = customer.Record,
                    UserActionPartRecord = action
                };

                _userActivityRepository.Create(act);
            }
        }
    }
}