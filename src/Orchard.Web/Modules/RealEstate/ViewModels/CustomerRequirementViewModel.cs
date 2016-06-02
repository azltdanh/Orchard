//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using Orchard.ContentManagement;
//using RealEstate.Models;

//namespace RealEstate.ViewModels
//{
//    public class CustomerRequirementViewModel
//    {
//    }

//    public class CustomerRequirementCreateViewModel
//    {
//        public int? GroupId { get; set; }
//        public int CustomerId { get; set; }
//        public CustomerPartRecord Customer { get; set; }
//        public bool IsEnabled { get; set; }

//        public int? ProvinceId { get; set; }
//        public IEnumerable<LocationProvincePart> Provinces { get; set; }
//        public int? DistrictId { get; set; }
//        public int[] DistrictIds { get; set; }
//        public IEnumerable<LocationDistrictPartRecord> Districts { get; set; }
//        public int? WardId { get; set; }
//        public int[] WardIds { get; set; }
//        public IEnumerable<LocationWardPartRecord> Wards { get; set; }
//        public int? StreetId { get; set; }
//        public int[] StreetIds { get; set; }
//        public IEnumerable<LocationStreetPartRecord> Streets { get; set; }

//        public double? MinArea { get; set; }
//        public double? MaxArea { get; set; }
//        public double? MinWidth { get; set; }
//        public double? MaxWidth { get; set; }
//        public double? MinLength { get; set; }
//        public double? MaxLength { get; set; }

//        public int? DirectionId { get; set; }
//        public int[] DirectionIds { get; set; }
//        public IEnumerable<DirectionPartRecord> Directions { get; set; }

//        public int? LocationId { get; set; }
//        public IEnumerable<PropertyLocationPartRecord> Locations { get; set; }

//        public double? MinAlleyWidth { get; set; }
//        public double? MaxAlleyWidth { get; set; }
//        public int? MinAlleyTurns { get; set; }
//        public int? MaxAlleyTurns { get; set; }
//        public double? MinDistanceToStreet { get; set; }
//        public double? MaxDistanceToStreet { get; set; }
//        public double? MinFloors { get; set; }
//        public double? MaxFloors { get; set; }
//        public int? MinBedrooms { get; set; }
//        public int? MaxBedrooms { get; set; }
//        public int? MinBathrooms { get; set; }
//        public int? MaxBathrooms { get; set; }

//        public double? MinPrice { get; set; }
//        public double? MaxPrice { get; set; }
//        public int PaymentMethodId { get; set; }
//        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }

//        public IContent CustomerRequirement { get; set; }
//    }

//    public class CustomerRequirementEditViewModel
//    {
//        public int? GroupId
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().GroupId; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().GroupId = value; }
//        }
//        public int CustomerId { get; set; }
//        public CustomerPartRecord Customer
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().Customer; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().Customer = value; }
//        }
//        public bool IsEnabled
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().IsEnabled; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().IsEnabled = value; }
//        }

//        public int? ProvinceId { get; set; }
//        public IEnumerable<LocationProvincePart> Provinces { get; set; }
//        public int? DistrictId { get; set; }
//        public int[] DistrictIds { get; set; }
//        public IEnumerable<LocationDistrictPartRecord> Districts { get; set; }
//        public int? WardId { get; set; }
//        public int[] WardIds { get; set; }
//        public IEnumerable<LocationWardPartRecord> Wards { get; set; }
//        public int? StreetId { get; set; }
//        public int[] StreetIds { get; set; }
//        public IEnumerable<LocationStreetPartRecord> Streets { get; set; }

//        public double? MinArea
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MinArea; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MinArea = value; }
//        }
//        public double? MaxArea
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MaxArea; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MaxArea = value; }
//        }
//        public double? MinWidth
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MinWidth; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MinWidth = value; }
//        }
//        public double? MaxWidth
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MaxWidth; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MaxWidth = value; }
//        }
//        public double? MinLength
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MinLength; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MinLength = value; }
//        }
//        public double? MaxLength
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MaxLength; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MaxLength = value; }
//        }

//        public int? DirectionId { get; set; }
//        public int[] DirectionIds { get; set; }
//        public IEnumerable<DirectionPartRecord> Directions { get; set; }

//        public int? LocationId { get; set; }
//        public IEnumerable<PropertyLocationPartRecord> Locations { get; set; }

//        public double? MinAlleyWidth
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MinAlleyWidth; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MinAlleyWidth = value; }
//        }
//        public double? MaxAlleyWidth
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MaxAlleyWidth; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MaxAlleyWidth = value; }
//        }
//        public int? MinAlleyTurns
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MinAlleyTurns; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MinAlleyTurns = value; }
//        }
//        public int? MaxAlleyTurns
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MaxAlleyTurns; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MaxAlleyTurns = value; }
//        }
//        public double? MinDistanceToStreet
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MinDistanceToStreet; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MinDistanceToStreet = value; }
//        }
//        public double? MaxDistanceToStreet
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MaxDistanceToStreet; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MaxDistanceToStreet = value; }
//        }
//        public double? MinFloors
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MinFloors; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MinFloors = value; }
//        }
//        public double? MaxFloors
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MaxFloors; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MaxFloors = value; }
//        }
//        public int? MinBedrooms
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MinBedrooms; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MinBedrooms = value; }
//        }
//        public int? MaxBedrooms
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MaxBedrooms; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MaxBedrooms = value; }
//        }
//        public int? MinBathrooms
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MinBathrooms; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MinBathrooms = value; }
//        }
//        public int? MaxBathrooms
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MaxBathrooms; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MaxBathrooms = value; }
//        }

//        public double? MinPrice
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MinPrice; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MinPrice = value; }
//        }
//        public double? MaxPrice
//        {
//            get { return CustomerRequirement.As<CustomerRequirementPart>().MaxPrice; }
//            set { CustomerRequirement.As<CustomerRequirementPart>().MaxPrice = value; }
//        }
//        public int PaymentMethodId { get; set; }
//        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }

//        public IContent CustomerRequirement { get; set; }
//    }

//    public class CustomerRequirementIndexViewModel
//    {
//        public IList<CustomerRequirementEntry> CustomerRequirement { get; set; }
//        public CustomerRequirementIndexOptions Options { get; set; }
//        public dynamic Pager { get; set; }
//    }

//    public class CustomerRequirementEntry
//    {
//        public CustomerRequirementPartRecord CustomerRequirement { get; set; }
//        public bool IsChecked { get; set; }
//    }

//    public class CustomerRequirementIndexOptions
//    {
//        public string Search { get; set; }
//        public CustomerRequirementOrder Order { get; set; }
//        public CustomerRequirementFilter Filter { get; set; }
//        public CustomerRequirementBulkAction BulkAction { get; set; }
//    }

//    public enum CustomerRequirementOrder
//    {
//        SeqOrder,
//        Name
//    }

//    public enum CustomerRequirementFilter
//    {
//        All
//    }

//    public enum CustomerRequirementBulkAction
//    {
//        None,
//        Enable,
//        Disable,
//        Delete,
//    }
//}
