using System.ComponentModel.DataAnnotations;

namespace Catalog.Dtos
{
    //will need these fields to create an item
    public record CreateItemDto
    {
        [Required]
        public required string Name { get; set; }

        [Required]
        [Range(1, 1000)]
        public decimal Price { get; set; }
    }
}