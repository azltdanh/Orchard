using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface ILocationApartmentRelationService : IDependency
    {
        bool VerifyLocationApartmentUnicity(int apartmentId, int districtId);
        bool VerifyLocationApartmentUnicity(int id, int apartmentId, int districtId);
        bool IsValidRelation(LocationApartmentRelationPartRecord item);
        bool IsValidRelation(int childApartmentId, int childDistrictId, int parentApartmentId, int parentDistrictId);

        LocationApartmentRelationPart GetLocationApartmentRelation(int districtId, int apartmentId);
    }

    public class LocationApartmentRelationService : ILocationApartmentRelationService
    {
        private readonly IContentManager _contentManager;

        public LocationApartmentRelationService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyLocationApartmentUnicity(int apartmentId, int districtId)
        {
            return !_contentManager.Query<LocationApartmentRelationPart, LocationApartmentRelationPartRecord>()
                .Where(r => r.LocationApartment.Id == apartmentId && r.District.Id == districtId).List().Any();
        }

        public bool VerifyLocationApartmentUnicity(int id, int apartmentId, int districtId)
        {
            return !_contentManager.Query<LocationApartmentRelationPart, LocationApartmentRelationPartRecord>()
                .Where(r => r.LocationApartment.Id == apartmentId && r.District.Id == districtId)
                .List()
                .Any(r => r.Id != id);
        }

        public bool IsValidRelation(LocationApartmentRelationPartRecord item)
        {
            LocationDistrictPartRecord childDistrict = item.District;
            LocationApartmentPartRecord childLocationApartment = item.LocationApartment;

            LocationDistrictPartRecord parentDistrict = item.RelatedDistrict;
            LocationApartmentPartRecord parentLocationApartment = item.RelatedLocationApartment;

            if (parentDistrict == childDistrict && parentLocationApartment == childLocationApartment)
                return false;

            while (HaveAncestors(parentDistrict, parentLocationApartment))
            {
                LocationApartmentRelationPartRecord p = GetParent(parentDistrict, parentLocationApartment);
                parentDistrict = p.RelatedDistrict;
                parentLocationApartment = p.RelatedLocationApartment;
                if (parentDistrict == childDistrict && parentLocationApartment == childLocationApartment)
                    return false;
            }
            return true;
        }

        public bool IsValidRelation(int childApartmentId, int childDistrictId, int parentApartmentId,
            int parentDistrictId)
        {
            if (parentDistrictId == childDistrictId && parentApartmentId == childApartmentId)
                return false;

            while (HaveAncestors(parentDistrictId, parentApartmentId))
            {
                LocationApartmentRelationPartRecord p = GetParent(parentDistrictId, parentApartmentId);
                parentDistrictId = p.RelatedDistrict.Id;
                parentApartmentId = p.RelatedLocationApartment.Id;
                if (parentDistrictId == childDistrictId && parentApartmentId == childApartmentId)
                    return false;
            }

            return true;
        }

        public LocationApartmentRelationPart GetLocationApartmentRelation(int districtId, int apartmentId)
        {
            return _contentManager.Query<LocationApartmentRelationPart, LocationApartmentRelationPartRecord>()
                .Where(r => r.LocationApartment.Id == apartmentId && r.District.Id == districtId)
                .List()
                .FirstOrDefault();
        }

        private bool HaveAncestors(int parentDistrictId, int parentApartmentId)
        {
            return _contentManager.Query<LocationApartmentRelationPart, LocationApartmentRelationPartRecord>()
                .Where(a => a.District.Id == parentDistrictId && a.LocationApartment.Id == parentApartmentId)
                .List()
                .Any();
        }

        private bool HaveAncestors(LocationDistrictPartRecord pDistrict, LocationApartmentPartRecord pLocationApartment)
        {
            return _contentManager.Query<LocationApartmentRelationPart, LocationApartmentRelationPartRecord>()
                .Where(a => a.District == pDistrict && a.LocationApartment == pLocationApartment).List().Any();
        }

        private LocationApartmentRelationPartRecord GetParent(int parentDistrictId, int parentApartmentId)
        {
            return _contentManager.Query<LocationApartmentRelationPart, LocationApartmentRelationPartRecord>()
                .Where(a => a.District.Id == parentDistrictId && a.LocationApartment.Id == parentApartmentId)
                .List()
                .First()
                .Record;
        }

        private LocationApartmentRelationPartRecord GetParent(LocationDistrictPartRecord pDistrict,
            LocationApartmentPartRecord pLocationApartment)
        {
            return _contentManager.Query<LocationApartmentRelationPart, LocationApartmentRelationPartRecord>()
                .Where(a => a.District == pDistrict && a.LocationApartment == pLocationApartment)
                .List()
                .First()
                .Record;
        }

        //private bool HaveDescendants(LocationApartmentRelationPartRecord item)
        //{
        //    return _contentManager.Query<LocationApartmentRelationPart, LocationApartmentRelationPartRecord>()
        //        .Where(a => a.RelatedDistrict == item.District && a.RelatedLocationApartment == item.LocationApartment)
        //        .List()
        //        .Any();
        //}

        //private IEnumerable<LocationApartmentRelationPartRecord> GetChilds(LocationApartmentRelationPartRecord item)
        //{
        //    return _contentManager.Query<LocationApartmentRelationPart, LocationApartmentRelationPartRecord>()
        //        .Where(a => a.RelatedDistrict == item.District && a.RelatedLocationApartment == item.LocationApartment)
        //        .List()
        //        .Select(a => a.Record);
        //}
    }
}