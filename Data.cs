using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiBugDisapearingControls
{
    public class FacilityListModel
    {
        public List<FacilityModel> Facilities { get; set; }       
    }

    public class FacilityModel
    {
        public string FacilityId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string CityDistrict { get; set; }
        public string HouseNumber { get; set; }
        public string FlatNumber { get; set; }
        public string LogoPath { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Distance { get; set; }
        public List<ActivityGroupModel> ActivityGroups { get; set; }
        
        public List<CardModel> Cards { get; set; }
       
        public string DistanceInKm => $"{Math.Round(Distance)} km";
        public string FullAddress => string.IsNullOrEmpty(CityDistrict) ? $"{Address}, {City}" : $"{Address}, {CityDistrict}, {City}";
        public string DistanceInMetersRoundedToTens => $"{Math.Round((Distance % 1) * 100) * 10} m";
        public string DisplayableDistance => Distance > 1d
            ? DistanceInKm
            : DistanceInMetersRoundedToTens;
    }

    public class CardModel
    {
        public string CardId { get; set; }
        public string Name { get; set; }
        public string ThumbnailURL { get; set; }
        public bool AllowedForDiscounts { get; set; }
    }

    public class ActivityGroupModel
    {
        public string ActivityGroupId { get; set; }
        public string Name { get; set; }
        public bool IsPromoted { get; set; }
        public int ActivitiesCount { get; set; }
    }


}
