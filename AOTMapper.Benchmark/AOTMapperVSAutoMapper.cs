﻿using System;
using AOTMapper.Benchmark.Data;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Toolchains.InProcess;

namespace AOTMapper.Benchmark
{
    [MemoryDiagnoser]
    public class AOTMapperVSAutoMapper
    {
        private IMapper mapper;

        public UserEntity UserEntity { get; }
        public User User { get; }

        public AOTMapperVSAutoMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserEntity>()
                    .ForMember(o => o.Id, o => o.MapFrom((entity, user) => Guid.Empty));
                cfg.CreateMap<UserEntity, User>()
                    .ForMember(o => o.Name, o => o.MapFrom((entity, user) => user.Name = $"{entity.FirstName} {entity.LastName}"));
            });

            configuration.AssertConfigurationIsValid();
            this.mapper = configuration.CreateMapper();

            this.UserEntity = new UserEntity(Guid.Empty, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            this.User = this.mapper.Map<User>(this.UserEntity);
        }

        [Benchmark]
        public void AutoMapperToUserEntity()
        {
            var userEntity = this.mapper.Map<UserEntity>(this.User);
        }

        [Benchmark]
        public void AOTMapperToUserEntity()
        {
            var user = this.User.MapToUserEntity();
        }

        [Benchmark]
        public void AutoMapperToUser()
        {
            var user = this.mapper.Map<User>(this.UserEntity);
        }

        [Benchmark]
        public void AOTMapperToUser()
        {
            var user = this.UserEntity.MapToUser();
        }
    }
}