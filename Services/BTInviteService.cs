using SlickTicket.Models;
using SlickTicket.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlickTicket.Services
{
    public class BTInviteService : IBTInviteService
    {
        public Task<bool> AcceptInviteAsync(Guid? token, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnyInviteAsync(Guid token, string email)
        {
            throw new NotImplementedException();
        }

        public Task<Invite> GetInviteAsync(Guid token, string email)
        {
            throw new NotImplementedException();
        }

        public Task<Invite> GetInviteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            throw new NotImplementedException();
        }
    }
}
