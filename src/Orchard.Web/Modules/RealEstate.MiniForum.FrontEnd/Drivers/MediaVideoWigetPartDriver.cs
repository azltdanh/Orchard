using Orchard.ContentManagement.Drivers;
using RealEstate.MiniForum.FrontEnd.Models;

namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class MediaVideoWigetPartDriver : ContentPartDriver<MediaVideoWidgetPart>
    {
        protected override DriverResult Display(MediaVideoWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_MediaVideoWidget", () =>
            {
                var shape = shapeHelper.Parts_MediaVideoWidget();
                shape.ContentPart = part;
                return shape;
            });
        }
    }
}
