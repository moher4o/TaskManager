﻿using AutoMapper;
using TaskManager.Common.Mapping;
using System;
using System.Linq;

namespace TaskMenager.WebApi.Infrastructure.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            var allTypes = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(a => (a.GetName().Name.Contains("TaskMenager") || a.GetName().Name.Contains("TaskManager")))
                .SelectMany(a => a.GetTypes());

            allTypes
                .Where(t => t.IsClass && !t.IsAbstract 
                     && t.GetInterfaces()
                         .Where(i => t.IsGenericType)
                         .Select(i => i.GetGenericTypeDefinition()).Contains(typeof(IMapFrom<>)))
                .Select(t => new
                {
                    Source = t.GetInterfaces()
                              .Where(i => i.IsGenericType)
                              .Select(i => new
                              {
                                  Definition = i.GetGenericTypeDefinition(),
                                  Arguments = i.GetGenericArguments()
                              })
                              .Where(i => i.Definition == typeof(IMapFrom<>))
                              .SelectMany(i => i.Arguments)
                              .First(),
                    Destination = t
                })
                .ToList()
                .ForEach(mapping => this.CreateMap(mapping.Source, mapping.Destination));

            //custom mapping
            allTypes
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IHaveCustomMapping).IsAssignableFrom(t))
                .Select(Activator.CreateInstance)
                .Cast<IHaveCustomMapping>()
                .ToList()
                .ForEach(mapping => mapping.ConfigureMapping(this));

        }

    }
}
