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

#region Import
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate/Import",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Estimation"},
                            {"controller", "Import"},
                            {"action", "Import"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Estimation"}
                        },
                        new MvcRouteHandler())
                },
#endregion

#region Estimation

                // EstimateAllProperties
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "RealEstate/EstimateAllProperties",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Estimation"},
                            {"controller", "Estimation"},
                            {"action", "EstimateAllProperties"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Estimation"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "RealEstate/EstimateAll",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Estimation"},
                            {"controller", "Estimation"},
                            {"action", "EstimateAll"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Estimation"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "RealEstate/EstimateAllStart",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Estimation"},
                            {"controller", "Estimation"},
                            {"action", "EstimateAllStart"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Estimation"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "RealEstate/EstimateAllEnd",
                        new RouteValueDictionary {
                            {"area", "RealEstate.Estimation"},
                            {"controller", "Estimation"},
                            {"action", "EstimateAllEnd"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "RealEstate.Estimation"}
                        },
                        new MvcRouteHandler())
                },

#endregion

            };
        }
    }
}