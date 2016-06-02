using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;

namespace RealEstate.API
{
    public class DeploymentApiRoutes : IRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
            {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {

            new RouteDescriptor {
                Route = new Route(
                    "json/getjson",
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"},
                        {"controller", "Json"},
                        {"action", "Getjson"}
                    },
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"}
                    },
                    new MvcRouteHandler())
            },
            new RouteDescriptor {
                Route = new Route(
                    "json/GetApartmentInfoForJson",
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"},
                        {"controller", "Json"},
                        {"action", "GetApartmentInfoForJson"}
                    },
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"}
                    },
                    new MvcRouteHandler())
            },

            #region User
            new RouteDescriptor {
                Route = new Route(
                    "jsapi/login",
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"},
                        {"controller", "APIAccount"},
                        {"action", "Login"}
                    },
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"}
                    },
                    new MvcRouteHandler())
            },
            new RouteDescriptor {
                Route = new Route(
                    "jsapi/register",
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"},
                        {"controller", "APIAccount"},
                        {"action", "Register"}
                    },
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"}
                    },
                    new MvcRouteHandler())
            },
            new RouteDescriptor {
                Route = new Route(
                    "jsapi/registerRest",
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"},
                        {"controller", "APIAccount"},
                        {"action", "RegisterRest"}
                    },
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"}
                    },
                    new MvcRouteHandler())
            },
            new RouteDescriptor {
                Route = new Route(
                    "jsapi/requestlostpassword",
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"},
                        {"controller", "APIAccount"},
                        {"action", "RequestLostPassword"}
                    },
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"}
                    },
                    new MvcRouteHandler())
            },
            new RouteDescriptor {
                Route = new Route(
                    "jsapi/updatelostpassword",
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"},
                        {"controller", "APIAccount"},
                        {"action", "UpdateLostPassword"}
                    },
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"}
                    },
                    new MvcRouteHandler())
            },
            new RouteDescriptor {
                Route = new Route(
                    "jsapi/changepassword",
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"},
                        {"controller", "APIAccount"},
                        {"action", "ChangePassword"}
                    },
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"}
                    },
                    new MvcRouteHandler())
            },
            new RouteDescriptor {
                Route = new Route(
                    "jsapi/UpdateProfile",
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"},
                        {"controller", "APIAccount"},
                        {"action", "UpdateProfile"}
                    },
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"}
                    },
                    new MvcRouteHandler())
            },
            new RouteDescriptor {
                Route = new Route(
                    "jsapi/ValidateLoginWithFacebook",
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"},
                        {"controller", "APIAccount"},
                        {"action", "ValidateLoginWithFacebook"}
                    },
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"}
                    },
                    new MvcRouteHandler())
            },
            new RouteDescriptor {
                Route = new Route(
                    "jsapi/ValidateLoginWithGoogle",
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"},
                        {"controller", "APIAccount"},
                        {"action", "ValidateLoginWithGoogle"}
                    },
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                        {"area", "RealEstate.API"}
                    },
                    new MvcRouteHandler())
            },

        #endregion

             #region Properties

                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/propertyCreate",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "PropertyAPI"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/PropertyAPI/CreateRest",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "PropertyAPI"},
                            {"action", "CreateRest"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },

                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/PropertyAPI/CreateRestFull",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "PropertyAPI"},
                            {"action", "CreateRestFull"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },

                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/propertyEdit",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "PropertyAPI"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/propertyDelete",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "PropertyAPI"},
                            {"action", "AjaxDelete"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/PropertyAPI/AjaxTrashed",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "PropertyAPI"},
                            {"action", "AjaxTrashed"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },

                #endregion

            #region Estimation
                
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/PropertyAPI/EstimatePrice",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "PropertyAPI"},
                            {"action", "EstimatePrice"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },

                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/PropertyAPI/IsEstimateable",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "PropertyAPI"},
                            {"action", "IsEstimateable"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },

                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/PropertyAPI/IsValidUserAddress",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "PropertyAPI"},
                            {"action", "IsValidUserAddress"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },

                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/PropertyAPI/EstimationCreatePost",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "PropertyAPI"},
                            {"action", "EstimationCreatePost"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },

                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/PropertyAPI/EstimationEditPost",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "PropertyAPI"},
                            {"action", "EstimationEditPost"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },

	        #endregion
                
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/PropertyAPI/AjaxDeleteRequirementByGroup",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "PropertyAPI"},
                            {"action", "AjaxDeleteRequirementByGroup"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },

                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/UpdatePaymentHistoryV2",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "PropertyAPI"},
                            {"action", "UpdatePaymentHistoryV2"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/UploadImage",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "Upload"},
                            {"action", "UploadImage"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/UploadUserAvatar",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "Upload"},
                            {"action", "UploadUserAvatar"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/UploadImageForumPost",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "Upload"},
                            {"action", "UploadImageForumPost"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
                //new RouteDescriptor {
                //    Route = new Route(
                //        "jsapi/UploadAttachImagePost",
                //        new RouteValueDictionary {
                //            {"area", "RealEstate.API"},
                //            {"controller", "Upload"},
                //            {"action", "UploadAttachImagePost"}
                //        },
                //        new RouteValueDictionary(),
                //        new RouteValueDictionary {
                //            {"area", "RealEstate.API"}
                //        },
                //        new MvcRouteHandler())
                //},
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/AjaxPermanentlyDeletePropertyImage",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "Upload"},
                            {"action", "AjaxPermanentlyDeletePropertyImage"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/AjaxPermanentlyDeletePropertyImageREST",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "Upload"},
                            {"action", "AjaxPermanentlyDeletePropertyImageREST"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/AjaxPublishPropertyImage",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "Upload"},
                            {"action", "AjaxPublishPropertyImage"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/AjaxPublishPropertyImageREST",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "Upload"},
                            {"action", "AjaxPublishPropertyImageREST"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/AjaxSetAvatarPropertyImage",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "Upload"},
                            {"action", "AjaxSetAvatarPropertyImage"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/AjaxSetAvatarPropertyImageREST",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "Upload"},
                            {"action", "AjaxSetAvatarPropertyImageREST"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
            #region Forum
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/forumpostCreate",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "ForumPost"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/forumpostCreateREST",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "ForumPost"},
                            {"action", "CreateREST"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/forumpostUpdate",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "ForumPost"},
                            {"action", "Update"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/forumpostUpdateREST",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "ForumPost"},
                            {"action", "UpdateREST"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/forumpostDeleteREST",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "ForumPost"},
                            {"action", "DeleteREST"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                },
	    #endregion
                
                new RouteDescriptor {
                    Route = new Route(
                        "jsapi/DGNDPayment",
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"},
                            {"controller", "PropertyAPI"},
                            {"action", "DGNDPayment"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.API"}
                        },
                        new MvcRouteHandler())
                }
        };
        }
    }
}