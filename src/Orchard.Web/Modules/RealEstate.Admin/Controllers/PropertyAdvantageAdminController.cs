using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.Caching;
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
    public class PropertyAdvantageAdminController : Controller, IUpdateModel
    {
        private readonly IPropertyAdvantageService _propertyAdvantageService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public PropertyAdvantageAdminController(
            IOrchardServices services,
            IPropertyAdvantageService propertyAdvantageService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _propertyAdvantageService = propertyAdvantageService;
            _signals = signals;
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

        public ActionResult Index(PropertyAdvantageIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyAdvantages,
                    T("Not authorized to list propertyAdvantages")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new PropertyAdvantageIndexOptions();

            IContentQuery<PropertyAdvantagePart, PropertyAdvantagePartRecord> propertyAdvantages =
                Services.ContentManager
                    .Query<PropertyAdvantagePart, PropertyAdvantagePartRecord>();

            switch (options.Filter)
            {
                case PropertyAdvantagesFilter.Advantages:
                    propertyAdvantages = propertyAdvantages.Where(a => a.ShortName == "adv");
                    break;
                case PropertyAdvantagesFilter.DisAdvantages:
                    propertyAdvantages = propertyAdvantages.Where(a => a.ShortName == "disadv");
                    break;
                case PropertyAdvantagesFilter.ApartmentAdvantages:
                    propertyAdvantages = propertyAdvantages.Where(a => a.ShortName == "apartment-adv");
                    break;
                case PropertyAdvantagesFilter.ApartmentInteriorAdvantages:
                    propertyAdvantages = propertyAdvantages.Where(a => a.ShortName == "apartment-interior-adv");
                    break;
                case PropertyAdvantagesFilter.ConstructionAdvantages:
                    propertyAdvantages = propertyAdvantages.Where(a => a.ShortName == "construction-adv");
                    break;
                case PropertyAdvantagesFilter.All:
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                propertyAdvantages =
                    propertyAdvantages.Where(
                        u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(propertyAdvantages.Count());

            switch (options.Order)
            {
                case PropertyAdvantagesOrder.SeqOrder:
                    propertyAdvantages = propertyAdvantages.OrderBy(a => a.ShortName).OrderBy(u => u.SeqOrder);
                    break;
                case PropertyAdvantagesOrder.Name:
                    propertyAdvantages = propertyAdvantages.OrderBy(a => a.ShortName).OrderBy(u => u.Name);
                    break;
            }

            List<PropertyAdvantagePart> results = propertyAdvantages
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new PropertyAdvantagesIndexViewModel
            {
                PropertyAdvantages = results
                    .Select(x => new PropertyAdvantageEntry {Advantage = x.Record})
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
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyAdvantages,
                    T("Not authorized to manage propertyAdvantages")))
                return new HttpUnauthorizedResult();

            var viewModel = new PropertyAdvantagesIndexViewModel
            {
                PropertyAdvantages = new List<PropertyAdvantageEntry>(),
                Options = new PropertyAdvantageIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<PropertyAdvantageEntry> checkedEntries = viewModel.PropertyAdvantages.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case PropertyAdvantagesBulkAction.None:
                    break;
                case PropertyAdvantagesBulkAction.Enable:
                    foreach (PropertyAdvantageEntry entry in checkedEntries)
                    {
                        Enable(entry.Advantage.Id);
                    }
                    break;
                case PropertyAdvantagesBulkAction.Disable:
                    foreach (PropertyAdvantageEntry entry in checkedEntries)
                    {
                        Disable(entry.Advantage.Id);
                    }
                    break;
                case PropertyAdvantagesBulkAction.Delete:
                    foreach (PropertyAdvantageEntry entry in checkedEntries)
                    {
                        Delete(entry.Advantage.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult DisAdvantages(PropertyAdvantageIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyAdvantages,
                    T("Not authorized to list propertyAdvantages")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new PropertyAdvantageIndexOptions();

            IContentQuery<PropertyAdvantagePart, PropertyAdvantagePartRecord> propertyAdvantages = Services
                .ContentManager
                .Query<PropertyAdvantagePart, PropertyAdvantagePartRecord>()
                .Where(a => a.ShortName == "disadv");

            switch (options.Filter)
            {
                case PropertyAdvantagesFilter.All:
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                propertyAdvantages =
                    propertyAdvantages.Where(
                        u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(propertyAdvantages.Count());

            switch (options.Order)
            {
                case PropertyAdvantagesOrder.SeqOrder:
                    propertyAdvantages = propertyAdvantages.OrderBy(a => a.ShortName).OrderBy(u => u.SeqOrder);
                    break;
                case PropertyAdvantagesOrder.Name:
                    propertyAdvantages = propertyAdvantages.OrderBy(a => a.ShortName).OrderBy(u => u.Name);
                    break;
            }

            List<PropertyAdvantagePart> results = propertyAdvantages
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new PropertyAdvantagesIndexViewModel
            {
                PropertyAdvantages = results
                    .Select(x => new PropertyAdvantageEntry {Advantage = x.Record})
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
        public ActionResult DisAdvantages(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyAdvantages,
                    T("Not authorized to manage propertyAdvantages")))
                return new HttpUnauthorizedResult();

            var viewModel = new PropertyAdvantagesIndexViewModel
            {
                PropertyAdvantages = new List<PropertyAdvantageEntry>(),
                Options = new PropertyAdvantageIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<PropertyAdvantageEntry> checkedEntries = viewModel.PropertyAdvantages.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case PropertyAdvantagesBulkAction.None:
                    break;
                case PropertyAdvantagesBulkAction.Enable:
                    foreach (PropertyAdvantageEntry entry in checkedEntries)
                    {
                        Enable(entry.Advantage.Id);
                    }
                    break;
                case PropertyAdvantagesBulkAction.Disable:
                    foreach (PropertyAdvantageEntry entry in checkedEntries)
                    {
                        Disable(entry.Advantage.Id);
                    }
                    break;
                case PropertyAdvantagesBulkAction.Delete:
                    foreach (PropertyAdvantageEntry entry in checkedEntries)
                    {
                        Delete(entry.Advantage.Id);
                    }
                    break;
            }

            return RedirectToAction("DisAdvantages", ControllerContext.RouteData.Values);
        }

        public ActionResult Create(PropertyAdvantagesFilter filter)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyAdvantages,
                    T("Not authorized to manage propertyAdvantages")))
                return new HttpUnauthorizedResult();

            string shortName = "";
            switch (filter)
            {
                case PropertyAdvantagesFilter.Advantages:
                    shortName = "adv";
                    break;
                case PropertyAdvantagesFilter.DisAdvantages:
                    shortName = "disadv";
                    break;
                case PropertyAdvantagesFilter.ApartmentAdvantages:
                    shortName = "apartment-adv";
                    break;
                case PropertyAdvantagesFilter.ApartmentInteriorAdvantages:
                    shortName = "apartment-interior-adv";
                    break;
                case PropertyAdvantagesFilter.ConstructionAdvantages:
                    shortName = "construction-adv";
                    break;
                case PropertyAdvantagesFilter.All:
                    return RedirectToAction("Index");
            }

            var propertyAdvantage = Services.ContentManager.New<PropertyAdvantagePart>("PropertyAdvantage");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyAdvantage.Create",
                Model:
                    new PropertyAdvantageCreateViewModel
                    {
                        ShortName = shortName,
                        CssClass = (shortName + "-"),
                        IsEnabled = true
                    }, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyAdvantage);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(PropertyAdvantageCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyAdvantages,
                    T("Not authorized to manage propertyAdvantages")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_propertyAdvantageService.VerifyPropertyAdvantageUnicity(createModel.Name))
                {
                    AddModelError("NotUniquePropertyAdvantageName", T("Advantage with that name already exists."));
                }
            }

            var propertyAdvantage = Services.ContentManager.New<PropertyAdvantagePart>("PropertyAdvantage");
            if (ModelState.IsValid)
            {
                propertyAdvantage.Name = createModel.Name;
                propertyAdvantage.ShortName = createModel.ShortName;
                propertyAdvantage.CssClass = createModel.CssClass;
                propertyAdvantage.SeqOrder = createModel.SeqOrder;
                propertyAdvantage.IsEnabled = createModel.IsEnabled;
                propertyAdvantage.AddedValue = createModel.AddedValue;

                Services.ContentManager.Create(propertyAdvantage);
            }

            dynamic model = Services.ContentManager.UpdateEditor(propertyAdvantage, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyAdvantage.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Advantages_Changed");

            Services.Notifier.Information(T("Advantage {0} created", propertyAdvantage.Name));

            var options = new PropertyAdvantageIndexOptions();

            switch (propertyAdvantage.ShortName)
            {
                case "adv":
                    options.Filter = PropertyAdvantagesFilter.Advantages;
                    break;
                case "disadv":
                    options.Filter = PropertyAdvantagesFilter.DisAdvantages;
                    break;
                case "apartment-adv":
                    options.Filter = PropertyAdvantagesFilter.ApartmentAdvantages;
                    break;
                case "apartment-interior-adv":
                    options.Filter = PropertyAdvantagesFilter.ApartmentInteriorAdvantages;
                    break;
                case "construction-adv":
                    options.Filter = PropertyAdvantagesFilter.ConstructionAdvantages;
                    break;
            }

            return RedirectToAction("Index", options);
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyAdvantages,
                    T("Not authorized to manage propertyAdvantages")))
                return new HttpUnauthorizedResult();

            var propertyAdvantage = Services.ContentManager.Get<PropertyAdvantagePart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyAdvantage.Edit",
                Model: new PropertyAdvantageEditViewModel {PropertyAdvantage = propertyAdvantage}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyAdvantage);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyAdvantages,
                    T("Not authorized to manage propertyAdvantages")))
                return new HttpUnauthorizedResult();

            var propertyAdvantage = Services.ContentManager.Get<PropertyAdvantagePart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(propertyAdvantage, this);

            var editModel = new PropertyAdvantageEditViewModel {PropertyAdvantage = propertyAdvantage};
            if (TryUpdateModel(editModel))
            {
                if (!_propertyAdvantageService.VerifyPropertyAdvantageUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniquePropertyAdvantageName",
                        T("PropertyAdvantage with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyAdvantage.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Advantages_Changed");

            Services.Notifier.Information(T("PropertyAdvantage {0} updated", propertyAdvantage.Name));

            var options = new PropertyAdvantageIndexOptions();

            switch (propertyAdvantage.ShortName)
            {
                case "adv":
                    options.Filter = PropertyAdvantagesFilter.Advantages;
                    break;
                case "disadv":
                    options.Filter = PropertyAdvantagesFilter.DisAdvantages;
                    break;
                case "apartment-adv":
                    options.Filter = PropertyAdvantagesFilter.ApartmentAdvantages;
                    break;
                case "apartment-interior-adv":
                    options.Filter = PropertyAdvantagesFilter.ApartmentInteriorAdvantages;
                    break;
                case "construction-adv":
                    options.Filter = PropertyAdvantagesFilter.ConstructionAdvantages;
                    break;
            }

            return RedirectToAction("Index", options);
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyAdvantages,
                    T("Not authorized to manage propertyAdvantages")))
                return new HttpUnauthorizedResult();

            var propertyAdvantage = Services.ContentManager.Get<PropertyAdvantagePart>(id);

            if (propertyAdvantage != null)
            {
                propertyAdvantage.IsEnabled = true;

                _signals.Trigger("Advantages_Changed");

                Services.Notifier.Information(T("PropertyAdvantage {0} updated", propertyAdvantage.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyAdvantages,
                    T("Not authorized to manage propertyAdvantages")))
                return new HttpUnauthorizedResult();

            var propertyAdvantage = Services.ContentManager.Get<PropertyAdvantagePart>(id);

            if (propertyAdvantage != null)
            {
                propertyAdvantage.IsEnabled = false;

                _signals.Trigger("Advantages_Changed");

                Services.Notifier.Information(T("PropertyAdvantage {0} updated", propertyAdvantage.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyAdvantages,
                    T("Not authorized to manage propertyAdvantages")))
                return new HttpUnauthorizedResult();

            var propertyAdvantage = Services.ContentManager.Get<PropertyAdvantagePart>(id);

            if (propertyAdvantage != null)
            {
                Services.ContentManager.Remove(propertyAdvantage.ContentItem);

                _signals.Trigger("Advantages_Changed");

                Services.Notifier.Information(T("Advantage {0} deleted", propertyAdvantage.Name));
            }

            return RedirectToAction("Index");
        }
    }
}