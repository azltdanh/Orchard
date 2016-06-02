using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;

namespace RealEstate.Admin.Controllers
{
    [ValidateInput(false), Admin]
    public class UserActionAdminController : Controller, IUpdateModel
    {
        private readonly IUserActionService _propertyInteriorService;
        private readonly ISiteService _siteService;

        public UserActionAdminController(
            IOrchardServices services,
            IUserActionService propertyInteriorService,
            IShapeFactory shapeFactory,
            ISiteService siteService)
        {
            Services = services;
            _propertyInteriorService = propertyInteriorService;
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

        public ActionResult Index(UserActionIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to list propertyInteriors")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new UserActionIndexOptions();

            IContentQuery<UserActionPart, UserActionPartRecord> propertyInteriors = Services.ContentManager
                .Query<UserActionPart, UserActionPartRecord>();

            switch (options.Filter)
            {
                case UserActionFilter.All:
                    //propertyInteriors = propertyInteriors.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                propertyInteriors =
                    propertyInteriors.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(propertyInteriors.Count());

            switch (options.Order)
            {
                case UserActionOrder.SeqOrder:
                    propertyInteriors = propertyInteriors.OrderBy(u => u.SeqOrder);
                    break;
                case UserActionOrder.Name:
                    propertyInteriors = propertyInteriors.OrderBy(u => u.Name);
                    break;
            }

            List<UserActionPart> results = propertyInteriors
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new UserActionIndexViewModel
            {
                UserActions = results
                    .Select(x => new UserActionEntry {UserAction = x.Record})
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage propertyInteriors")))
                return new HttpUnauthorizedResult();

            var viewModel = new UserActionIndexViewModel
            {
                UserActions = new List<UserActionEntry>(),
                Options = new UserActionIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<UserActionEntry> checkedEntries = viewModel.UserActions.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case UserActionBulkAction.None:
                    break;
                case UserActionBulkAction.Enable:
                    foreach (UserActionEntry entry in checkedEntries)
                    {
                        Enable(entry.UserAction.Id);
                    }
                    break;
                case UserActionBulkAction.Disable:
                    foreach (UserActionEntry entry in checkedEntries)
                    {
                        Disable(entry.UserAction.Id);
                    }
                    break;
                case UserActionBulkAction.Delete:
                    foreach (UserActionEntry entry in checkedEntries)
                    {
                        Delete(entry.UserAction.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage propertyInteriors")))
                return new HttpUnauthorizedResult();

            var propertyInterior = Services.ContentManager.New<UserActionPart>("UserAction");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UserAction.Create",
                Model: new UserActionCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyInterior);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(UserActionCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage propertyInteriors")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_propertyInteriorService.VerifyUserActionUnicity(createModel.Name))
                {
                    AddModelError("NotUniqueUserActionName", T("UserAction with that name already exists."));
                }
            }

            var propertyInterior = Services.ContentManager.New<UserActionPart>("UserAction");
            if (ModelState.IsValid)
            {
                propertyInterior.Name = createModel.Name;
                propertyInterior.ShortName = createModel.ShortName;
                propertyInterior.CssClass = createModel.CssClass;
                propertyInterior.SeqOrder = createModel.SeqOrder;
                propertyInterior.IsEnabled = createModel.IsEnabled;
                propertyInterior.Point = createModel.Point;

                Services.ContentManager.Create(propertyInterior);
            }

            dynamic model = Services.ContentManager.UpdateEditor(propertyInterior, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UserAction.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("UserAction {0} created", propertyInterior.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage propertyInteriors")))
                return new HttpUnauthorizedResult();

            var propertyInterior = Services.ContentManager.Get<UserActionPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UserAction.Edit",
                Model: new UserActionEditViewModel {UserAction = propertyInterior}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyInterior);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage propertyInteriors")))
                return new HttpUnauthorizedResult();

            var propertyInterior = Services.ContentManager.Get<UserActionPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(propertyInterior, this);

            var editModel = new UserActionEditViewModel {UserAction = propertyInterior};
            if (TryUpdateModel(editModel))
            {
                if (!_propertyInteriorService.VerifyUserActionUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniqueUserActionName", T("UserAction with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UserAction.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("UserAction {0} updated", propertyInterior.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage propertyInteriors")))
                return new HttpUnauthorizedResult();

            var propertyInterior = Services.ContentManager.Get<UserActionPart>(id);

            if (propertyInterior != null)
            {
                propertyInterior.IsEnabled = true;
                Services.Notifier.Information(T("UserAction {0} updated", propertyInterior.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage propertyInteriors")))
                return new HttpUnauthorizedResult();

            var propertyInterior = Services.ContentManager.Get<UserActionPart>(id);

            if (propertyInterior != null)
            {
                propertyInterior.IsEnabled = false;
                Services.Notifier.Information(T("UserAction {0} updated", propertyInterior.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage propertyInteriors")))
                return new HttpUnauthorizedResult();

            var propertyInterior = Services.ContentManager.Get<UserActionPart>(id);

            if (propertyInterior != null)
            {
                Services.ContentManager.Remove(propertyInterior.ContentItem);
                Services.Notifier.Information(T("UserAction {0} deleted", propertyInterior.Name));
            }

            return RedirectToAction("Index");
        }
    }
}