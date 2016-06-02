using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace RealEstate {
    public class Routes : IRouteProvider {
        
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {

                // Home
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "RealEstate",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "Home"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                
                // AutoLogin
                new RouteDescriptor {
                    Route = new Route(
                        "AutoLogin",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "AutoLogin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },

#region Real Estate

                // Property
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Details/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyAdmin"},
                            {"action", "Details"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/View/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyAdmin"},
                            {"action", "View"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/AjaxUploadYoutube",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "Home"},
                            {"action", "UploadYoutube"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
#endregion

#region Customer

                // Customer
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/Customer",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CustomerAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/Customer/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CustomerAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Customer/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CustomerAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Customer/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CustomerAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },

#endregion

#region Address & Location

                // Direction
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/Direction",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "DirectionAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/Direction/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "DirectionAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Direction/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "DirectionAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Direction/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "DirectionAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },

                // LocationProvince
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/LocationProvince",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationProvinceAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/LocationProvince/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationProvinceAdmin"},
                            {"action", "Item"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/LocationProvince/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationProvinceAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/LocationProvince/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationProvinceAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/LocationProvince/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationProvinceAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },

                // LocationDistrict
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/LocationDistrict",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationDistrictAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/LocationDistrict/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationDistrictAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/LocationDistrict/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationDistrictAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/LocationDistrict/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationDistrictAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },

                
                // LocationWard
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/LocationWard",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationWardAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/LocationWard/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationWardAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/LocationWard/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationWardAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/LocationWard/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationWardAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },

                
                // LocationStreet
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/LocationStreet",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationStreetAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/LocationStreet/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationStreetAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/LocationStreet/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationStreetAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/LocationStreet/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationStreetAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                
                // StreetRelation
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/StreetRelation",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "StreetRelationAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/StreetRelation/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "StreetRelationAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/StreetRelation/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "StreetRelationAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/StreetRelation/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "StreetRelationAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/LocationApartmentAdmin/ApartmentCartIndex/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationApartmentAdmin"},
                            {"action", "ApartmentCartIndex"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/LocationApartmentAdmin/ApartmentCartCreate",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationApartmentAdmin"},
                            {"action", "ApartmentCartCreate"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
#endregion

#region Property Attributes

                // PropertyStatus
                #region PropertyStatus
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/PropertyStatus",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyStatusAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/PropertyStatus/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyStatusAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PropertyStatus/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyStatusAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PropertyStatus/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyStatusAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                #endregion
                
                // PropertyFlag
                #region PropertyFlag
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/PropertyFlag",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyFlagAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/PropertyFlag/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyFlagAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PropertyFlag/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyFlagAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PropertyFlag/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyFlagAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                #endregion

                // PropertyLocation
                #region PropertyLocation
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/PropertyLocation",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyLocationAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/PropertyLocation/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyLocationAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PropertyLocation/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyLocationAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PropertyLocation/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyLocationAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                #endregion

                // PropertyAdvantage
                #region PropertyAdvantage
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Property/Advantages",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyAdvantageAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Property/DisAdvantages",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyAdvantageAdmin"},
                            {"action", "DisAdvantages"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Property/Advantages/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyAdvantageAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Property/Advantages/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyAdvantageAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Property/Advantages/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyAdvantageAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                #endregion
                
                // PropertyInterior
                #region PropertyInterior
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/PropertyInterior",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyInteriorAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/PropertyInterior/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyInteriorAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PropertyInterior/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyInteriorAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PropertyInterior/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyInteriorAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                #endregion
                
                // PropertyLegalStatus
                #region PropertyLegalStatus
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/PropertyLegalStatus",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyLegalStatusAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/PropertyLegalStatus/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyLegalStatusAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PropertyLegalStatus/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyLegalStatusAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PropertyLegalStatus/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyLegalStatusAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                #endregion
                
                // PropertyTypeGroup
                #region PropertyTypeGroup
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/PropertyTypeGroup",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyTypeGroupAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/PropertyTypeGroup/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyTypeGroupAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PropertyTypeGroup/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyTypeGroupAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PropertyTypeGroup/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyTypeGroupAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                #endregion

                // PropertyType
                #region PropertyType
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/PropertyType",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyTypeAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/PropertyType/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyTypeAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PropertyType/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyTypeAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PropertyType/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyTypeAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                #endregion
                
                // PropertyTypeConstruction
                #region PropertyTypeConstruction
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/PropertyTypeConstruction",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyTypeConstructionAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/PropertyTypeConstruction/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyTypeConstructionAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PropertyTypeConstruction/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyTypeConstructionAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PropertyTypeConstruction/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PropertyTypeConstructionAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                #endregion
                
                // AdsType
                #region AdsType
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/AdsType",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "AdsTypeAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/AdsType/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "AdsTypeAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/AdsType/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "AdsTypeAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/AdsType/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "AdsTypeAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                #endregion

#endregion
                
#region Payment Configs

                // PaymentMethod
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/PaymentMethod",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PaymentMethodAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/PaymentMethod/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PaymentMethodAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PaymentMethod/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PaymentMethodAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PaymentMethod/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PaymentMethodAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                
                // PaymentUnit
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/PaymentUnit",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PaymentUnitAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/PaymentUnit/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PaymentUnitAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PaymentUnit/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PaymentUnitAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PaymentUnit/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PaymentUnitAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                
                // PaymentExchange
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/PaymentExchange",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PaymentExchangeAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/PaymentExchange/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PaymentExchangeAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PaymentExchange/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PaymentExchangeAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/PaymentExchange/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "PaymentExchangeAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },

#endregion

#region Coefficients Configs
                
                // CoefficientWidth
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/Coefficient/Width",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CoefficientWidthAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/Coefficient/Width/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CoefficientWidthAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Coefficient/Width/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CoefficientWidthAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Coefficient/Width/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CoefficientWidthAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                
                // CoefficientLength
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/Coefficient/Length",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CoefficientLengthAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/Coefficient/Length/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CoefficientLengthAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Coefficient/Length/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CoefficientLengthAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Coefficient/Length/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CoefficientLengthAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                
                // CoefficientAlley
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/Coefficient/Alley",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CoefficientAlleyAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/Coefficient/Alley/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CoefficientAlleyAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Coefficient/Alley/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CoefficientAlleyAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Coefficient/Alley/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CoefficientAlleyAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                
                // CoefficientAlleyDistance
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "Admin/RealEstate/Coefficient/AlleyDistance",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CoefficientAlleyDistanceAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 2,
                    Route = new Route(
                        "Admin/RealEstate/Coefficient/AlleyDistance/Create",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CoefficientAlleyDistanceAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Coefficient/AlleyDistance/Edit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CoefficientAlleyDistanceAdmin"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Coefficient/AlleyDistance/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "CoefficientAlleyDistanceAdmin"},
                            {"action", "Remove"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                
#endregion

#region Users
                
                new RouteDescriptor {
                    Route = new Route(
                        "thanh-vien/tu-choi-truy-cap",
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"},
                            {"controller", "Account"},
                            {"action", "AccessDenied"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"}
                        },
                        new MvcRouteHandler())
                },
                
                new RouteDescriptor {
                    Route = new Route(
                        "thanh-vien/dang-nhap",
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"},
                            {"controller", "Account"},
                            {"action", "LogOn"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"}
                        },
                        new MvcRouteHandler())
                },
                
                new RouteDescriptor {
                    Route = new Route(
                        "thanh-vien/thoat",
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"},
                            {"controller", "Account"},
                            {"action", "LogOff"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"}
                        },
                        new MvcRouteHandler())
                },
                
                new RouteDescriptor {
                    Route = new Route(
                        "thanh-vien/dang-ky",
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"},//Orchard.Users
                            {"controller", "Account"},
                            {"action", "Register"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"}//old: RealEstateForum.FrontEnd
                        },
                        new MvcRouteHandler())
                },
                
                new RouteDescriptor {
                    Route = new Route(
                        "thanh-vien/quen-mat-khau",
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"},
                            {"controller", "Account"},
                            {"action", "RequestLostPassword"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"}
                        },
                        new MvcRouteHandler())
                },
                
                new RouteDescriptor {
                    Route = new Route(
                        "thanh-vien/tao-mat-khau-moi",
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"},
                            {"controller", "Account"},
                            {"action", "LostPassword"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"}
                        },
                        new MvcRouteHandler())
                },
                
                new RouteDescriptor {
                    Route = new Route(
                        "thanh-vien/doi-mat-khau-thanh-cong",
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"},
                            {"controller", "Account"},
                            {"action", "ChangePasswordSuccess"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"}
                        },
                        new MvcRouteHandler())
                },
                
                new RouteDescriptor {
                    Route = new Route(
                        "thanh-vien/dang-cho-duyet",
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"},
                            {"controller", "Account"},
                            {"action", "RegistrationPending"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"}
                        },
                        new MvcRouteHandler())
                },
                
                new RouteDescriptor {
                    Route = new Route(
                        "thanh-vien/xac-thuc-email",
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"},
                            {"controller", "Account"},
                            {"action", "ChallengeEmailSent"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"}
                        },
                        new MvcRouteHandler())
                },
                
                new RouteDescriptor {
                    Route = new Route(
                        "thanh-vien/xac-thuc-email-thanh-cong",
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"},
                            {"controller", "Account"},
                            {"action", "ChallengeEmailSuccess"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"}
                        },
                        new MvcRouteHandler())
                },
                
                new RouteDescriptor {
                    Route = new Route(
                        "thanh-vien/chua-xac-thuc-email",
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"},
                            {"controller", "Account"},
                            {"action", "ChallengeEmailFail"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"}
                        },
                        new MvcRouteHandler())
                },
                
#endregion

#region PaymentHistory & Config
                // AdsPaymentHistory
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstateAdmin/AdsPaymentHistory",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "AdsPayment"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                // AdsPaymentConfig
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstateAdmin/AdsPaymentConfig",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "AdsPayment"},
                            {"action", "PaymentConfigIndex"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                // AddPayment
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstateAdmin/AddPayment",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "AdsPayment"},
                            {"action", "AddPayment"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                // AddPaymentConfig Create
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstateAdmin/PaymentCreate/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "AdsPayment"},
                            {"action", "PaymentConfigCreate"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                // AddPaymentConfig Edit
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstateAdmin/PaymentEdit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "AdsPayment"},
                            {"action", "PaymentConfigEdit"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/StreetImport",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "LocationImport"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/Management/VideoYoutube",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"},
                            {"controller", "VideoManage"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Admin"}
                        },
                        new MvcRouteHandler())
                }
        #endregion
            };
        }
    }
}