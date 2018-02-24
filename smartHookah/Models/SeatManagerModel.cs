// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SeatManagerModel.cs" company="">
//   
// </copyright>
// <summary>
//   The seat manager model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace smartHookah.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The seat manager model.
    /// </summary>
    public class SeatManagerModel
    {
        /// <summary>
        /// Gets or sets the seats.
        /// </summary>
        public List<Seat> Seats { get; set; }

        /// <summary>
        /// Gets or sets the place id.
        /// </summary>
        public int PlaceId { get; set; }

        /// <summary>
        /// Gets or sets the place.
        /// </summary>
        public Place place { get; set; }
    }
}