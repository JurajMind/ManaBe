﻿using smartHookah.Models.Db;
using smartHookah.Models.Db.Place;
using smartHookah.Models.Dto.Places;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace smartHookah.Services.Place
{
    using Place = Models.Db.Place.Place;

    public interface IPlaceService
    {
        Task<Place> GetPlace(int id);

        Task<Place> GetManagedPlace(int id);

        Task<bool> CanManagePlace(int id);

        Task<IEnumerable<PlaceReview>> GetPlaceReviews(int id, int pageSize = 10, int page = 0);

        IEnumerable<PipeAccesory> GetPlaceAccessories(Place place);

        Task<List<TobaccoMix>> GetPlaceTobaccoMixes(Place place);

        Task<Place> AddPlace(Place place, List<string> flags);

        Task<Address> GetLocation(Address address);
        Task<Place> AddFlags(int placeId, List<string> flags);

        Task<PlaceDashboardDto> PlaceDashboard(int placeId);
        bool? ValidateCoordinates(double? lng, double? lat);

        Task<List<Place>> GetWaitingPlaces();

        Task<Place> ChangePlaceState(int placeId, PlaceState newState);
    }
}