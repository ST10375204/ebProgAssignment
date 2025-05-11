using System.ComponentModel.DataAnnotations;

namespace assignment.Models
{
    public class itemModel
    {


        [Required(ErrorMessage = "Item name is required.")]
        public string itemName { get; set; }

        [Required(ErrorMessage = "Item categroy is required.")]
        public string itemCategory { get; set; }
        
        [Required(ErrorMessage = "Item  description is required.")]
        public string itemDesc { get; set; }


        public double? itemPrice { get; set; }

        public string? productionDate { get; set; }

        public string? imgUrl { get; set; }

        [Required]
        public string userID { get; set; }
    }
}
