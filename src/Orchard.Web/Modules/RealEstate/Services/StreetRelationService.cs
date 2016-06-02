using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IStreetRelationService : IDependency
    {
        bool VerifyStreetUnicity(int streetId, int wardId, int districtId);
        bool VerifyStreetUnicity(int id, int streetId, int wardId, int districtId);
        bool IsValidRelation(StreetRelationPartRecord item);

        bool IsValidRelation(int childStreetId, int childWardId, int childDistrictId, int parentStreetId,
            int parentWardId, int parentDistrictId);

        StreetRelationPart GetStreetRelation(int districtId, int wardId, int streetId);
    }

    public class StreetRelationService : IStreetRelationService
    {
        private readonly IContentManager _contentManager;

        public StreetRelationService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyStreetUnicity(int streetId, int wardId, int districtId)
        {
            return !_contentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
                .Where(r => r.Street.Id == streetId && r.Ward.Id == wardId && r.District.Id == districtId).List().Any();
        }

        public bool VerifyStreetUnicity(int id, int streetId, int wardId, int districtId)
        {
            return !_contentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
                .Where(r => r.Street.Id == streetId && r.Ward.Id == wardId && r.District.Id == districtId)
                .List()
                .Any(r => r.Id != id);
        }

        public bool IsValidRelation(StreetRelationPartRecord item)
        {
            LocationDistrictPartRecord childDistrict = item.District;
            LocationWardPartRecord childWard = item.Ward;
            LocationStreetPartRecord childStreet = item.Street;

            LocationDistrictPartRecord parentDistrict = item.RelatedDistrict;
            LocationWardPartRecord parentWard = item.RelatedWard;
            LocationStreetPartRecord parentStreet = item.RelatedStreet;

            if (parentDistrict == childDistrict && parentWard == childWard && parentStreet == childStreet)
                return false;

            while (HaveAncestors(parentDistrict, parentWard, parentStreet))
            {
                StreetRelationPartRecord p = GetParent(parentDistrict, parentWard, parentStreet);
                parentDistrict = p.RelatedDistrict;
                parentWard = p.RelatedWard;
                parentStreet = p.RelatedStreet;
                if (parentDistrict == childDistrict && parentWard == childWard && parentStreet == childStreet)
                    return false;
            }
            return true;
        }

        public bool IsValidRelation(int childStreetId, int childWardId, int childDistrictId, int parentStreetId,
            int parentWardId, int parentDistrictId)
        {
            if (parentDistrictId == childDistrictId && parentWardId == childWardId && parentStreetId == childStreetId)
                return false;

            while (HaveAncestors(parentDistrictId, parentWardId, parentStreetId))
            {
                StreetRelationPartRecord p = GetParent(parentDistrictId, parentWardId, parentStreetId);
                parentDistrictId = p.RelatedDistrict.Id;
                parentWardId = p.RelatedWard.Id;
                parentStreetId = p.RelatedStreet.Id;
                if (parentDistrictId == childDistrictId && parentWardId == childWardId &&
                    parentStreetId == childStreetId)
                    return false;
            }

            return true;
        }

        public StreetRelationPart GetStreetRelation(int districtId, int wardId, int streetId)
        {
            return _contentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
                .Where(r => r.Street.Id == streetId && r.Ward.Id == wardId && r.District.Id == districtId)
                .List()
                .FirstOrDefault();
        }

        private bool HaveAncestors(int parentDistrictId, int parentWardId, int parentStreetId)
        {
            return _contentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
                .Where(
                    a => a.District.Id == parentDistrictId && a.Ward.Id == parentWardId && a.Street.Id == parentStreetId)
                .List()
                .Any();
        }

        private bool HaveAncestors(LocationDistrictPartRecord pDistrict, LocationWardPartRecord pWard,
            LocationStreetPartRecord pStreet)
        {
            return _contentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
                .Where(a => a.District == pDistrict && a.Ward == pWard && a.Street == pStreet).List().Any();
        }

        private StreetRelationPartRecord GetParent(int parentDistrictId, int parentWardId, int parentStreetId)
        {
            return _contentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
                .Where(
                    a => a.District.Id == parentDistrictId && a.Ward.Id == parentWardId && a.Street.Id == parentStreetId)
                .List()
                .First()
                .Record;
        }

        private StreetRelationPartRecord GetParent(LocationDistrictPartRecord pDistrict, LocationWardPartRecord pWard,
            LocationStreetPartRecord pStreet)
        {
            return _contentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
                .Where(a => a.District == pDistrict && a.Ward == pWard && a.Street == pStreet)
                .List()
                .First()
                .Record;
        }

        //private bool HaveDescendants(StreetRelationPartRecord item)
        //{
        //    return _contentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
        //        .Where(
        //            a =>
        //                a.RelatedDistrict == item.District && a.RelatedWard == item.Ward &&
        //                a.RelatedStreet == item.Street).List().Any();
        //}

        //private IEnumerable<StreetRelationPartRecord> GetChilds(StreetRelationPartRecord item)
        //{
        //    return _contentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
        //        .Where(
        //            a =>
        //                a.RelatedDistrict == item.District && a.RelatedWard == item.Ward &&
        //                a.RelatedStreet == item.Street).List()
        //        .Select(a => a.Record);
        //}
    }
}