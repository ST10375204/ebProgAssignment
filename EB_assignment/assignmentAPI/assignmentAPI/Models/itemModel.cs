using System.ComponentModel.DataAnnotations;

namespace assignmentAPI.Models
{
    public class itemModel
    {
    
        public int? itemId { get; set; }
        [Required]
        public string itemName { get; set; }

        [Required]
        public string itemCategory { get; set; }

        public string itemDesc { get; set; }


        public double? itemPrice { get; set; } 

        public string? productionDate { get; set; } 

        public string? imgUrl { get; set; }

        [Required]
        public string userID { get; set; } 
    }
}
