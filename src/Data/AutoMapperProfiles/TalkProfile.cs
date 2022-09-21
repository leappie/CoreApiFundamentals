﻿using AutoMapper;
using CoreCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Data.AutoMapperProfiles
{
    public class TalkProfile : Profile
    {
        public TalkProfile()
        {
            CreateMap<Talk, TalkModel>()
                .ReverseMap();
                //.ForMember(t => t.Camp, opt => opt.Ignore()).ForMember(t => t.Speaker, opt => opt.Ignore());
        }
    }
}
