using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ARMManager.ViewModels
{
    public class DomainChoosingViewModel
    {
        [Required]
        [Display(Name = "Azure Domain")]
        public string Domain { get; set; }
    }
}
