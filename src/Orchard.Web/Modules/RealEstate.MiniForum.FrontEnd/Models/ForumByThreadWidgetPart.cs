using RealEstateForum.Service.Models;
using Orchard.ContentManagement;
using System.Collections.Generic;
using System.Web.Mvc;
namespace RealEstate.MiniForum.FrontEnd.Models
{
    public class ForumByThreadWidgetPart : ContentPart<ForumThreadPartRecord>
    {
        /// <summary>
        /// Forum Category name.
        /// </summary>
        public virtual string ThreadName
        {
            get { return Record.Name; }
            set { Record.Name = value; }
        }

        public string SelectedCategory { get; set; } // used on editor
        /// <summary>
        /// Categories available to be selected.
        /// </summary>
        public IEnumerable<SelectListItem> AvailableThreads { get; set; } // used on editor
    }
}