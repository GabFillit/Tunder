﻿using System.Threading.Tasks;
using Data.Model;
using Microsoft.Extensions.Configuration;

namespace Tunder.API.Services
{
    public class ThrottleService : IThrottleService
    {
        private readonly ICachingService _cachingService;
        private readonly IConfiguration _configs;

        public ThrottleService(ICachingService cachingService, IConfiguration configs)
        {
            _cachingService = cachingService;
            _configs = configs;
        }

        public async Task<long> LogFailLoginAttempt(User user)
        {
            return await _cachingService.IncrementValueFromKey(user.Email);
        }

        public async Task<long> GetFailLoginAttempt(User user)
        {
            return await _cachingService.GetValueFromKey(user.Email);
        }
    }
}