// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PriceGroupDto.cs" company="">
//   
// </copyright>
// <summary>
//   The price group dto.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using smartHookah.Models.Db;

namespace smartHookah.Controllers
{
    /// <summary>
    /// The price group dto.
    /// </summary>
    public class PriceGroupDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceGroupDto"/> class.
        /// </summary>
        /// <param name="priceGroup">
        /// The price group.
        /// </param>
        public PriceGroupDto(PriceGroup priceGroup)
        {
            this.Name = priceGroup.Name;
            this.Id = priceGroup.Id;
            this.Price = priceGroup.Price;
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        public decimal Price { get; set; }
    }
}