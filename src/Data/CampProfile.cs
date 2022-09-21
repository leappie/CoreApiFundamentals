using AutoMapper;
using CoreCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Data
{
    public class CampProfile : Profile
    {
        // Create a constructor and inside the constructor a map from the camp class to the campmodel class
        public CampProfile()
        {
            this.CreateMap<Camp, CampModel>();
                //.ForMember(c => c.Venue, o => o.MapFrom(m => m.Location.VenueName)); // Set mapping manually
        }


    }
}
