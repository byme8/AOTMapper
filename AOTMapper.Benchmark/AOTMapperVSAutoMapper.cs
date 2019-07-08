using AOTMapper.Benchmark.Data;
using AutoMapper;

namespace AOTMapper.Benchmark
{
    public class AOTMapperVSAutoMapper
    {
        private IMapper mapper;

        public UserEntity UserEntity { get; }
        public User User { get; }

        public AOTMapperVSAutoMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserEntity>();
                cfg.CreateMap<UserEntity, User>()
                    .AfterMap((entity, user) => user.Name = $"{entity.FirstName} {entity.LastName}");
            });

            configuration.AssertConfigurationIsValid();
            this.mapper = configuration.CreateMapper();

            this.UserEntity = new UserEntity();
            this.User = this.mapper.Map<User>(this.UserEntity);
        }

        public void AutoMapperToUserEntity()
        {
            var userEntity = this.mapper.Map<UserEntity>(this.User);
        }

        public void AutoMapperToUser()
        {
            var userEntity = this.mapper.Map<User>(this.User);
        }
    }
}