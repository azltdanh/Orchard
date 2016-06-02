using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using RealEstate.NewLetter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstate.NewLetter.Drivers
{
    public class CustomerEmailExceptionPartDriver : ContentPartDriver<CustomerEmailExceptionPart>
    {
        protected override DriverResult Display(CustomerEmailExceptionPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_CustomerEmailExceptionPart",
                () => shapeHelper.Parts_CustomerEmailException(
                    Email: part.EmailException,
                    CodeRandom: part.CodeRandom,
                    StatusActive: part.StatusActive));
        }
        //GET
        protected override DriverResult Editor(CustomerEmailExceptionPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_CustomerEmailExceptionPart_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/CustomerEmailExceptionPart",
                    Model: part,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(CustomerEmailExceptionPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}