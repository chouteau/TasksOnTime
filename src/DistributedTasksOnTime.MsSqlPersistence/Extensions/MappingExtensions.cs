using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;

namespace DistributedTasksOnTime.MsSqlPersistence.Extensions
{
    internal static class MappingExtensions
    {
        public static void ResolveUsing<TSource, TDestination, TMember, TResult>(this IMemberConfigurationExpression<TSource, TDestination, TMember> member,
                          Func<TSource, TResult> resolver)
      => member.MapFrom((Func<TSource, TDestination, TResult>)((src, dest) => resolver(src)));

    }
}
