using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace RealEstate.MiniForum.FrontEnd
{
    public class Routes : IRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "danh-sach-moi-gioi",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "UserUpdateProfile"},
                            {"action", "ListUserAgencies"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },

                new RouteDescriptor {
                    Route = new Route(
                        "dien-dan/dang-bai-viet",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PostForumFrontEnd"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "dien-dan/cap-nhat-bai-viet/{title}-{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PostForumFrontEnd"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "dien-dan/dien-dan-bat-dong-san",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ThreadForumFrontEnd"},
                            {"action", "HomeForum"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "dien-dan/tin-tuc-thi-truong-bat-dong-san",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ThreadForumFrontEnd"},
                            {"action", "ListPostIsMarket"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                 new RouteDescriptor {
                    Route = new Route(
                        "dien-dan/tin-tuc-noi-bat",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ThreadForumFrontEnd"},
                            {"action", "ListPostIsHighlight"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                 new RouteDescriptor {
                    Route = new Route(
                        "news/tin-tuc-moi-nhat",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ThreadForumFrontEnd"},
                            {"action", "ListPostNews"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "dien-dan/du-an-bat-dong-san",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ThreadForumFrontEnd"},
                            {"action", "ListPostIsProject"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "dien-dan/ket-qua-tim-kiem-bai-viet",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PostForumFrontEnd"},
                            {"action", "ResultsFilter"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                 new RouteDescriptor {
                    Route = new Route(
                        "dien-dan/cap-nhat-thong-tin",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "UserUpdateProfile"},
                            {"action", "EditProfile"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                 new RouteDescriptor {
                    Route = new Route(
                        "dien-dan/xem-thong-tin-{username}-{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "UserUpdateProfile"},
                            {"action", "ViewProfile"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/upload/image-post",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "Upload"},
                            {"action", "UploadMedia"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/upload/file-attachment",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "Upload"},
                            {"action", "FileAttachments"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/forum/changestatus-post",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PostForumFrontEnd"},
                            {"action", "ChangeStatusPost"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/miniforum/AjaxSubmitCommentForum",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PostForumFrontEnd"},
                            {"action", "AjaxSubmitCommentForum"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/miniforum/AjaxDeleteComment",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PostForumFrontEnd"},
                            {"action", "AjaxDeleteComment"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/miniforum/AjaxPostToFacebook",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PostForumFrontEnd"},
                            {"action", "AjaxPostToFacebook"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/miniforum/AjaxLoadComment",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PostForumFrontEnd"},
                            {"action", "AjaxLoadComment"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/miniforum/AjaxLoadTopicFrontEnd",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PostForumFrontEnd"},
                            {"action", "AjaxLoadTopicFrontEnd"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/miniforum/AjaxPersonalPostPage",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PersonalPage"},
                            {"action", "AjaxPersonalPostPage"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/miniforum/AjaxPerDeletePost",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PersonalPage"},
                            {"action", "AjaxPerDeletePost"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/miniforum/AjaxPerDeleteComment",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PersonalPage"},
                            {"action", "AjaxPerDeleteComment"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/miniforum/AjaxGetPhotoUserSelect",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "UserUpdateProfile"},
                            {"action", "AjaxGetUserProfile"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/forum/AjaxLoadForumPostOfUser",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PostForumFrontEnd"},
                            {"action", "AjaxLoadForumPostOfUser"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/forum/AjaxLoadForumMenuOfUser",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PostForumFrontEnd"},
                            {"action", "AjaxLoadForumMenuOfUser"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/forum/AjaxDeletePost",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PostForumFrontEnd"},
                            {"action", "AjaxDeletePost"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/blog-ca-nhan/AjaxGetListFriend",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PersonalPage"},
                            {"action", "AjaxGetListFriend"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/blog-ca-nhan/AjaxGetListRequestFriend",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PersonalPage"},
                            {"action", "AjaxGetListRequestFriend"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/blog-ca-nhan/AjaxSearchFriend",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PersonalPage"},
                            {"action", "AjaxSearchFriend"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/blog-ca-nhan/AjaxCheckFriend",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PersonalPage"},
                            {"action", "AjaxCheckFriend"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },new RouteDescriptor {
                    Route = new Route(
                        "ajax/blog-ca-nhan/AjaxAddFriend",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PersonalPage"},
                            {"action", "AjaxAddFriend"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/blog-ca-nhan/AjaxAcceptFriend",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PersonalPage"},
                            {"action", "AjaxAcceptFriend"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "dien-dan/danh-sach-bai-viet-{ShortName}-{TopicId}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ThreadForumFrontEnd"},
                            {"action", "ViewTopic"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "dien-dan/{ShortName}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ThreadForumFrontEnd"},
                            {"action", "ListPostByThread"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 3,
                    Route = new Route(
                        "error-404",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ForumError"},
                            {"action", "ForumNotFound"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },                
                new RouteDescriptor {
                    Route = new Route(
                        "dien-dan/{ThreadShortName}/{TopicShortName}/{title}-{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PostForumFrontEnd"},
                            {"action", "PostDetail"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "dien-dan/tin-tuc-thi-truong",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ThreadForumFrontEnd"},
                            {"action", "ListPostIsMarket"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 3,
                    Route = new Route(
                        "blog-ca-nhan",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PersonalPage"},
                            {"action", "MyPage"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "blog-ca-nhan/{userName}-{userId}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PersonalPage"},
                            {"action", "FriendPage"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "get-tags/{tagname}-{tagId}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "OrchardTags"},
                            {"action", "Search"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },new RouteDescriptor {
                    Route = new Route(
                        "search-tag/{tagname}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "OrchardTags"},
                            {"action", "SearchByTagName"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                }
#region video
                ,new RouteDescriptor {
                    Route = new Route(
                        "video/danh-sach-video",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PostForumFrontEnd"},
                            {"action", "ListVideo"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                }
                ,
                new RouteDescriptor {
                    Route = new Route(
                        "video/{title}-{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PostForumFrontEnd"},
                            {"action", "VideoDetail"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                }
#endregion
            #region Redirect from RealEstate.ForumFrontEnd
                ,
                new RouteDescriptor {
                    Route = new Route(
                        "forum/tin-nhan",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ForumError"},
                            {"action", "RedirectMessage"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "forum/photo-album-cua-tui",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ForumError"},
                            {"action", "RedirectMyPhotoAlbum"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "forum/photo-album-cua-tui-{albumname}-{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ForumError"},
                            {"action", "RedirectViewMyPhotoAlbum"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "forum/photo-album-cua-ban-{username}-{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ForumError"},
                            {"action", "RedirectYourPhotoAlbum"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "forum/xem-photo-album-cua-ban-{photoname}-{userId}-{albumId}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ForumError"},
                            {"action", "RedirectViewYourPhotoAlbum"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "forum/danh-sach-ban-be",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ForumError"},
                            {"action", "RedirectMyListFriend"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "forum/danh-sach-ban-cua-{username}-{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ForumError"},
                            {"action", "RedirectYourListFriend"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "forum/danh-sach-yeu-cau-ket-ban",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ForumError"},
                            {"action", "RedirectRequestListFriend"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "forum/dien-dan-bat-dong-san",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ForumError"},
                            {"action", "RedirectForumHomePage"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 4,
                    Route = new Route(
                        "redirect",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "ForumError"},
                            {"action", "RedirectFromExternalLink"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },//// PeronalPage
                new RouteDescriptor {
                    Priority = 4,
                    Route = new Route(
                        "blog-ca-nhan/tin-nhan",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PersonalPage"},
                            {"action", "MyMessage"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 4,
                    Route = new Route(
                        "blog-ca-nhan/danh-sach-ban",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PersonalPage"},
                            {"action", "MyListFriend"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 4,
                    Route = new Route(
                        "blog-ca-nhan/danh-sach-ban/{UserName}-{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PersonalPage"},
                            {"action", "YourListFriend"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 4,
                    Route = new Route(
                        "blog-ca-nhan/danh-sach-yeu-cau-ket-ban",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"},
                            {"controller", "PersonalPage"},
                            {"action", "MyListRequestFriend"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 4,
                    Route = new Route(
                        "blog-ca-nhan/tin-nhan/{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxFrontEnd"},
                            {"action", "ViewMessageUser"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                }

            #endregion
            };
        }
    }
}