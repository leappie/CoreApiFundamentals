using AutoMapper;
using CoreCodeCamp.Model;

namespace CoreCodeCamp.Data
{
    public class CampProfile : Profile
    {
        public CampProfile()
        {
            this.CreateMap<Camp, CampModel>()
                .ForMember(c => c.Venue, o => o.MapFrom(m => m.Location.VenueName)); // Specify where automapper should look for a certain value
        }
    }
}
