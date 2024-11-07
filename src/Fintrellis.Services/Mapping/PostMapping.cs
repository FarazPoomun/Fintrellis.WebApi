using AutoMapper;
using Fintrellis.Services.Models;

namespace Fintrellis.Services.Mapping
{
    public class PostMapping : Profile
    {
        public PostMapping()
        {
            CreateMap<PostCreateRequest, Post>()
                .ForMember(z => z.PostId, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(z => z.CreatedDateTime, opt => opt.MapFrom(src => DateTime.UtcNow));
            
            CreateMap<PostUpdateRequest, Post>()
                .ForMember(z => z.UpdatedDateTime, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}
