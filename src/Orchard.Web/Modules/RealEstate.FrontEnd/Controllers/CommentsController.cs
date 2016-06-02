using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using RealEstate.FrontEnd.ViewModels;
using RealEstate.Models;

namespace RealEstate.FrontEnd.Controllers
{
    [ValidateInput(false), Admin]
    public class CommentsController : Controller, IUpdateModel
    {
        private readonly ICommentService _commentService;
        private readonly ISiteService _siteService;

        public CommentsController(
            IOrchardServices services,
            ICommentService commentService,
            IShapeFactory shapeFactory,
            ISiteService siteService)
        {
            _commentService = commentService;
            Services = services;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties,
            string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public ActionResult Index(int id, CommentIndexOptions options, PagerParameters pagerParameters)
        {
            //#region OLD
            //var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            //// default options
            //if (options == null)
            //    options = new CommentIndexOptions();

            //var p = Services.ContentManager.Get<PropertyPart>(id);
            //var pComment = p.As<CommentsPart>();
            //switch (options.Filter)
            //{
            //    case CommentIndexFilter.All:
            //        //propertySettings = propertySettings.Where(u => u.RegistrationStatus == UserStatus.Approved);
            //        break;
            //}

            //var pagerShape = Shape.Pager(pager).TotalItemCount(pComment.Comments.Count());

            //var results = pComment.Comments
            //    .Skip(pager.GetStartIndex()).Take(pager.PageSize)
            //    .ToList();

            //if (!String.IsNullOrWhiteSpace(options.Search))
            //{
            //    results = results.Where(u => u.CommentText.Contains(options.Search)).ToList();
            //}

            //var model = new CommentsIndexViewModel
            //{
            //    Comments = results
            //        .Select(x => new CommentEntry { Comment = x.Record })
            //        .ToList(),
            //    Options = options,
            //    Pager = pagerShape
            //};

            //// maintain previous route data when generating page links
            //var routeData = new RouteData();
            //routeData.Values.Add("Options.Filter", options.Filter);

            //pagerShape.RouteData(routeData);

            //return View(model);
            //#endregion

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new CommentIndexOptions();

            var p = Services.ContentManager.Get<PropertyPart>(id);
            var pComment = p.As<CommentsPart>();
            switch (options.Filter)
            {
                case CommentIndexFilter.All:
                    //propertySettings = propertySettings.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(pComment.Comments.Count());

            List<CommentPart> results = pComment.Comments
                .OrderByDescending(c => c.CommentDateUtc)
                .Skip(pager.GetStartIndex()).Take(pager.PageSize)
                .ToList();

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                results = results.Where(u => u.CommentText.Contains(options.Search)).ToList();
            }

            var model = new CommentsIndexViewModel
            {
                Comments = results
                    .Select(x => new CommentEntry {Comment = x.Record})
                    .ToList(),
                Options = options,
                Pager = pagerShape,
                Property_Id = id
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);

            pagerShape.RouteData(routeData);

// ReSharper disable once Mvc.PartialViewNotResolved
            return PartialView("ManagerComments", model);
        }

        public ActionResult AjaxLoadListComments(int id, CommentIndexOptions options, PagerParameters pagerParameters)
        {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new CommentIndexOptions();

            var p = Services.ContentManager.Get<PropertyPart>(id);
            var pComment = p.As<CommentsPart>();
            switch (options.Filter)
            {
                case CommentIndexFilter.All:
                    //propertySettings = propertySettings.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(pComment.Comments.Count());

            List<CommentPart> results = pComment.Comments
                .OrderByDescending(c => c.CommentDateUtc)
                .Skip(pager.GetStartIndex()).Take(pager.PageSize)
                .ToList();

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                results = results.Where(u => u.CommentText.Contains(options.Search)).ToList();
            }

            var model = new CommentsIndexViewModel
            {
                Comments = results
                    .Select(x => new CommentEntry {Comment = x.Record})
                    .ToList(),
                Options = options,
                Pager = pagerShape,
                Property_Id = id
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);

            pagerShape.RouteData(routeData);

// ReSharper disable once Mvc.PartialViewNotResolved
            return PartialView("AdminListOfComments", model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            var viewModel = new CommentsIndexViewModel
            {
                Comments = new List<CommentEntry>(),
                Options = new CommentIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<CommentEntry> checkedEntries = viewModel.Comments.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case CommentIndexBulkAction.None:
                    break;
                case CommentIndexBulkAction.Delete:
                    if (
                        !Services.Authorizer.Authorize(Orchard.Comments.Permissions.ManageComments,
                            T("Couldn't delete comment")))
                        return new HttpUnauthorizedResult();

                    foreach (CommentEntry entry in checkedEntries)
                    {
                        _commentService.DeleteComment(entry.Comment.Id);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return RedirectToAction("Index");
        }

        [ValidateInput(false)]
        public ActionResult Create(int id, CommentIndexOptions options, PagerParameters pagerParameters)
        {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new CommentIndexOptions();

            var p = Services.ContentManager.Get<PropertyPart>(id);
            var pComment = p.As<CommentsPart>();
            switch (options.Filter)
            {
                case CommentIndexFilter.All:
                    //propertySettings = propertySettings.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(pComment.Comments.Count());

            List<CommentPart> results = pComment.Comments
                .OrderByDescending(c => c.CommentDateUtc)
                .Skip(pager.GetStartIndex()).Take(pager.PageSize)
                .ToList();

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                results = results.Where(u => u.CommentText.Contains(options.Search)).ToList();
            }

            var model = new CommentsIndexViewModel
            {
                Comments = results
                    .Select(x => new CommentEntry {Comment = x.Record})
                    .ToList(),
                Options = options,
                Pager = pagerShape,
                Property_Id = id
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Create(FormCollection input)
        {
            if (!Services.Authorizer.Authorize(Orchard.Comments.Permissions.AddComment, T("Couldn't add comment")))
                return this.RedirectLocal(input["Url"], "~/");

            var viewModel = new CommentsCreateViewModel();

            TryUpdateModel(viewModel);

            if (!ModelState.IsValidField("Name"))
            {
                Services.Notifier.Error(T("Name is mandatory and must have less than 255 chars"));
            }

            if (!ModelState.IsValidField("Email"))
            {
                Services.Notifier.Error(T("Email is invalid or is longer than 255 chars"));
            }

            if (!ModelState.IsValidField("Site"))
            {
                Services.Notifier.Error(T("Site url is invalid or is longer than 255 chars"));
            }

            if (!ModelState.IsValidField("CommentText"))
            {
                Services.Notifier.Error(T("Comment is mandatory"));
            }

            var context = new CreateCommentContext
            {
                Author = viewModel.Name,
                CommentText = viewModel.CommentText,
                Email = viewModel.Email,
                SiteName = viewModel.SiteName,
                CommentedOn = viewModel.CommentedOn,
            };

            if (ModelState.IsValid)
            {
                if (!String.IsNullOrEmpty(context.SiteName) && !context.SiteName.StartsWith("http://") &&
                    !context.SiteName.StartsWith("https://"))
                {
                    context.SiteName = "http://" + context.SiteName;
                }

                //CommentPart commentPart = _commentService.CreateComment(context, Services.WorkContext.CurrentSite.As<CommentSettingsPart>().ModerateComments);

                //if (commentPart.Status == CommentStatus.Pending)
                //{
                //    // if the user who submitted the comment has the right to moderate, don't make this comment moderated
                //    if (Services.Authorizer.Authorize(Orchard.Comments.Permissions.ManageComments))
                //    {
                //        commentPart.Status = CommentStatus.Approved;
                //    }
                //    else
                //    {
                //        Services.Notifier.Information(T("Your comment will appear after the site administrator approves it."));
                //    }
                //}
            }
            else
            {
                TempData["CreateCommentContext.Name"] = context.Author;
                TempData["CreateCommentContext.CommentText"] = context.CommentText;
                TempData["CreateCommentContext.Email"] = context.Email;
                TempData["CreateCommentContext.SiteName"] = context.SiteName;
            }

            return this.RedirectLocal(input["Url"], "~/");
        }

        public ActionResult Edit(int id)
        {
            CommentPart commentPart = _commentService.GetComment(id);
            if (commentPart == null)
                return new HttpNotFoundResult();

            var viewModel = new CommentsEditViewModel
            {
                CommentText = commentPart.CommentText,
                Email = commentPart.Email,
                Id = commentPart.Id,
                Name = commentPart.Author,
                SiteName = commentPart.SiteName,
                Status = commentPart.Status,
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Edit(FormCollection input, string returnUrl)
        {
            var viewModel = new CommentsEditViewModel();
            UpdateModel(viewModel);
            if (!Services.Authorizer.Authorize(Orchard.Comments.Permissions.ManageComments, T("Couldn't edit comment")))
                return new HttpUnauthorizedResult();

            //_commentService.UpdateComment(viewModel.Id, viewModel.Name, viewModel.Email, viewModel.SiteName, viewModel.CommentText, viewModel.Status);
            return this.RedirectLocal(returnUrl);
        }

        [HttpPost]
        public ActionResult Delete(int id, string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Orchard.Comments.Permissions.ManageComments, T("Couldn't delete comment")))
                return new HttpUnauthorizedResult();

            CommentPart commentPart = _commentService.GetComment(id);
            if (commentPart == null)
                return new HttpNotFoundResult();

            int commentedOn = commentPart.CommentedOn;
            _commentService.DeleteComment(id);

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Details", new {id = commentedOn}));
        }
    }
}