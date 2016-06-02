using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace RealEstate.MiniForum
{
    public class Routes : IRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                #region Thread & Topic
                // Thread Index
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/Thread",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "ThreadAdminForum"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                //Thread Create
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/ThreadCreate",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "ThreadAdminForum"},
                            {"action", "ThreadCreate"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                //Thread Create
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/ThreadEdit/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "ThreadAdminForum"},
                            {"action", "ThreadEdit"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                //Thread Delete
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/Delete/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "ThreadAdminForum"},
                            {"action", "ThreadDelete"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                //Topic Index
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/TopicIndex/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "ThreadAdminForum"},
                            {"action", "TopicIndex"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                //Topic Create
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/TopicCreate/{Id}",// Id: ID Thread
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "ThreadAdminForum"},
                            {"action", "TopicCreate"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                //Topic Edit
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/TopicEdit/{Id}",//Id: Topic Id
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "ThreadAdminForum"},
                            {"action", "TopicEdit"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/Admin/RealEstate.MiniForum/LoadTopic",//Id: Topic Id
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "ThreadAdminForum"},
                            {"action", "AjaxLoadTopic"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                #endregion

                #region Post
                //Post Index
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/PostIndex",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "PostAdminForum"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                //Post Create
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/PostCreate",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "PostAdminForum"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                //HPost Create
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/HPostCreate",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "PostAdminForum"},
                            {"action", "HPostCreate"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                //Post edit
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/PostEdit/{Id}",//Id: Post Id
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "PostAdminForum"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                //HPost edit
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/HPostEdit/{Id}",//Id: Post Id
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "PostAdminForum"},
                            {"action", "HPostEdit"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                //Post Delete
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/PostDelete/{Id}",//Id: Post Id
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "PostAdminForum"},
                            {"action", "Delete"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                #region Import
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/ImportData",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "ImportData"},
                            {"action", "Import"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/ImportThreadCssImage",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "ImportData"},
                            {"action", "ImportCssImageThread"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                #endregion

                #endregion

                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/ReplaceUser",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "ReplaceUser"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstate.MiniForum/GetListOnlineUsers",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "User"},
                            {"action", "GetListOnlineUsers"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/SupportOnline",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "SupportOnlineConfig"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/SupportOnline/Add",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "SupportOnlineConfig"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/SupportOnline/Edit/{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "SupportOnlineConfig"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}