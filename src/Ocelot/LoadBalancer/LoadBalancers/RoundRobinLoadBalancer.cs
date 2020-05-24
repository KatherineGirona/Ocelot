﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocelot.Responses;
using Ocelot.Values;
using System;

namespace Ocelot.LoadBalancer.LoadBalancers
{
    public class RoundRobinLoadBalancer : ILoadBalancer
    {
        private readonly Func<Task<List<Service>>> _services;

        private int _last;

        public RoundRobinLoadBalancer(Func<Task<List<Service>>> services)
        {
            _services = services;
        }


        public async Task<Response<HostAndPort>> Lease()
        {
            var services = await _services.Invoke();
            if (_last >= services.Count)
            {
                _last = 0;
            }

            var next = await Task.FromResult(services[_last]);
            _last++;
            return new OkResponse<HostAndPort>(next.HostAndPort);
        }

        public void Release(HostAndPort hostAndPort)
        {
        }
    }
}
