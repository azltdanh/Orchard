using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RealEstate.MiniForum.ViewModels
{
    public class ImportDataViewModel
    {
        [Required(ErrorMessage="Vui lòng nhập ID  cũ.")]
        public int OldTopicId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ID  mới.")]
        public int NewTopicId { get; set; }
    }
}