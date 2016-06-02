using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Mello.ImageGallery.Models.Plugins.SliderCycle
{
    public class SliderCycleSetting
    {
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("{");
            stringBuilder.AppendLine(string.Concat("fx : 'fadeZoom'"));
            stringBuilder.AppendLine("}");


            return stringBuilder.ToString();
        }
    }
}