﻿using System.Threading.Tasks;
using Data.Model;

namespace Tunder.API.Services
{
    public interface IThrottleService
    {
        Task<long> LogFailLoginAttempt(User user);
        Task<long> GetFailLoginAttempt(User user);
    }
}