﻿using AutoMapper;
using PlantifyApp.Apis.Dtos;
using PlantifyApp.Core.Models;
using System.Net;

namespace PlantifyApp.Apis.Helpers
{
    public class MappingProfiles:Profile
    {
        public MappingProfiles()
        {

            CreateMap<Posts, PostDto>().ReverseMap();
            CreateMap<Comments, CommentDto>().ReverseMap();
            CreateMap<Likes, LikeDto>().ReverseMap();

        }
    }
}
