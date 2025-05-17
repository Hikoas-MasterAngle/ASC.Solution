using ASC.Model.BaseTypes;
using ASC.Model.Models;
using ASC.WEB.Areas.Configuration.Models;
using ASC.WED.Areas.Configuration.Models;
using AutoMapper;

namespace ASC.WEB.Areas.Configuration.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<MasterDataKey, MasterDataKeyViewModel>();
            CreateMap<MasterDataKeyViewModel, MasterDataKey>();
            CreateMap<MasterDataValue, MasterDataValueViewModel>();
            CreateMap<MasterDataValueViewModel, MasterDataValue>();
        }
    }

}
    
