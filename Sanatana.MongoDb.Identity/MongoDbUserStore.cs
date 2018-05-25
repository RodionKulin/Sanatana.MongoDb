using Sanatana.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Identity;

namespace Sanatana.MongoDb.Identity
{
    public class MongoDbUserStore<TUser>
        : IUserStore<TUser>
        , IUserPasswordStore<TUser>
        , IUserSecurityStampStore<TUser>
        , IUserLockoutStore<TUser>
        , IUserEmailStore<TUser>  
        , IUserClaimStore<TUser>  
        , IUserTwoFactorStore<TUser>
        , IUserPhoneNumberStore<TUser>
        , IUserLoginStore<TUser>
        , IDisposable
        where TUser : MongoDbIdentityUser
    {
        //fields
        protected IdentityMongoDbContext<TUser> _context;
        

        //init
        public MongoDbUserStore(MongoDbConnectionSettings settings)
        {
            _context = new IdentityMongoDbContext<TUser>(settings);
        }



        //IUserStore
        public virtual Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public virtual Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public virtual Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }

        public virtual Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public virtual Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.FromResult(0);
        }

        public virtual async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            string error = null;

            try
            {
                var options = new InsertOneOptions()
                {
                };

                await _context.Users.InsertOneAsync(user, options, cancellationToken);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return error == null
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError()
                {
                    Description = error
                });
        }

        public virtual async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            string error = null;

            try
            {
                FilterDefinition<TUser> filter
                    = Builders<TUser>.Filter.Where(p => p.Id == user.Id);

                UpdateDefinition<TUser> update = Builders<TUser>.Update
                    .Combine()
                    .SetAllMappedMembers(user);

                UpdateResult result = await _context.Users.UpdateOneAsync(filter, update, null, cancellationToken);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return error == null
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError()
                {
                    Description = error
                });
        }

        public virtual async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            string error = null;

            try
            {
                FilterDefinition<TUser> filter
                    = Builders<TUser>.Filter.Where(p => p.Id == user.Id);

                DeleteResult deleteResult = await _context.Users.DeleteOneAsync(filter, cancellationToken);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return error == null
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError()
                {
                    Description = error
                });
        }

        public virtual async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            ObjectId id = new ObjectId(userId);
            FilterDefinition<TUser> filter
                = Builders<TUser>.Filter.Where(p => p.Id == id);

            IAsyncCursor<TUser> cursor = await _context.Users.FindAsync(filter, null, cancellationToken);
            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<TUser> FindByNameAsync(string normalizedName, CancellationToken cancellationToken)
        {
            FilterDefinition<TUser> filter
                = Builders<TUser>.Filter.Where(p => p.NormalizedUserName == normalizedName);

            IAsyncCursor<TUser> cursor = await _context.Users.FindAsync(filter, null, cancellationToken);
            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }




        //IUserPasswordStore
        public virtual Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public virtual Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public virtual Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
        {
            bool hasPassword = !string.IsNullOrEmpty(user.PasswordHash);
            return Task.FromResult(hasPassword);
        }


        //IUserSecurityStampStore

        public virtual Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public virtual Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }



        //IUserLockoutStore
        public virtual Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
        {
            DateTimeOffset? endDate = user.LockoutEndDateUtc.HasValue
                ? new DateTimeOffset?(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc))
                : null;

            return Task.FromResult(endDate);
        }

        public virtual Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            user.LockoutEndDateUtc = lockoutEnd == null
                ? null
                : new DateTime?(lockoutEnd.Value.UtcDateTime);

            return Task.FromResult(0);
        }

        public virtual Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        public virtual Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        public virtual Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public virtual Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public virtual Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }


        

        //IUserEmailStore
        public virtual Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public virtual Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public virtual Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public virtual Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public virtual async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            FilterDefinition<TUser> filter
              = Builders<TUser>.Filter.Where(p => p.NormalizedEmail == normalizedEmail);

            IAsyncCursor<TUser> cursor = await _context.Users.FindAsync(filter, null, cancellationToken);
            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }

        public virtual Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }

        public virtual Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.FromResult(0);
        }



        //IUserClaimStore
        public virtual async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
        {
            FilterDefinition<MongoDbUserClaim> filter
              = Builders<MongoDbUserClaim>.Filter.Where(p => p.UserId == user.Id);

            IAsyncCursor<MongoDbUserClaim> cursor = await _context.Claims.FindAsync(filter, null, cancellationToken);
            List<MongoDbUserClaim> userClaims = await cursor.ToListAsync(cancellationToken);

            userClaims = userClaims ?? new List<MongoDbUserClaim>();

            List<Claim> claims = userClaims.Select(c =>
                new Claim(c.Type, c.Value, null, c.Issuer, c.OriginalIssuer)).ToList();

            return claims;
        }

        public virtual Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            List<MongoDbUserClaim> items = claims.Select(p => new MongoDbUserClaim()
            {
                UserId = user.Id,
                Type = p.Type,
                Value = p.Value,
                Issuer = p.Issuer,
                OriginalIssuer = p.OriginalIssuer
            }).ToList();

            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            return _context.Claims.InsertManyAsync(items, options, cancellationToken);
        }

        public virtual Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            FilterDefinition<MongoDbUserClaim> filter = Builders<MongoDbUserClaim>.Filter.Where(
                p => p.UserId == user.Id
                && p.Type == claim.Type);

            UpdateDefinition<MongoDbUserClaim> update = Builders<MongoDbUserClaim>.Update
                .Set(p => p.Value, newClaim.Value)
                .Set(p => p.Issuer, newClaim.Issuer)
                .Set(p => p.OriginalIssuer, newClaim.OriginalIssuer);

            UpdateOptions options = new UpdateOptions()
            {
                IsUpsert = false
            };

            return _context.Claims.UpdateOneAsync(filter, update, options, cancellationToken);
        }

        public virtual Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            List<string> claimTypes = claims.Select(p => p.Type)
                .Distinct().ToList();

            FilterDefinition<MongoDbUserClaim> filter = Builders<MongoDbUserClaim>.Filter.Where(
                p => p.UserId == user.Id
                && claimTypes.Contains(p.Type));

            return _context.Claims.DeleteManyAsync(filter, cancellationToken);
        }

        public virtual async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            FilterDefinition<MongoDbUserClaim> claimsFilter = Builders<MongoDbUserClaim>.Filter.Where(
                p => p.Type == claim.Type
                && p.Value == p.Value);

            IAsyncCursor<MongoDbUserClaim> claimsCursor = await _context.Claims.FindAsync(claimsFilter, null, cancellationToken);
            List<MongoDbUserClaim> userClaims = await claimsCursor.ToListAsync(cancellationToken);

            userClaims = userClaims ?? new List<MongoDbUserClaim>();
            List<ObjectId> userIDs = userClaims.Select(c => c.UserId).Distinct().ToList();

            List<TUser> users = null;
            if (userIDs.Count > 0)
            {
                FilterDefinition<TUser> usersFilter
                    = Builders<TUser>.Filter.Where(p => userIDs.Contains(p.Id));

                IAsyncCursor<TUser> cursor = await _context.Users.FindAsync(usersFilter, null, cancellationToken);
                users = await cursor.ToListAsync(cancellationToken);
            }

            return users ?? new List<TUser>();
        }

        

        //IUserTwoFactorStore
        public virtual Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        public virtual Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }



        //IUserPhoneNumberStore
        public virtual Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }

        public virtual Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public virtual Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public virtual Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult(0);
        }



        //IUserLoginStore
        public virtual Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            MongoDbUserLogin item = new MongoDbUserLogin();
            item.UserId = user.Id;
            item.LoginProvider = login.LoginProvider;
            item.ProviderKey = login.ProviderKey;
            item.ProviderDisplayName = login.ProviderDisplayName;

            return _context.UserLogins.InsertOneAsync(item, null, cancellationToken);
        }

        public virtual Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            FilterDefinition<MongoDbUserLogin> filter = Builders<MongoDbUserLogin>.Filter.Where(
                p => p.UserId == user.Id
                && p.LoginProvider == loginProvider
                && p.ProviderKey == providerKey);

            return _context.UserLogins.DeleteOneAsync(filter, cancellationToken);
        }

        public virtual async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
        {
            FilterDefinition<MongoDbUserLogin> filter = Builders<MongoDbUserLogin>.Filter.Where(
                p => p.UserId == user.Id);

            IAsyncCursor<MongoDbUserLogin> cursor = await _context.UserLogins.FindAsync(filter, null, cancellationToken);
            List<MongoDbUserLogin> userLogins = await cursor.ToListAsync(cancellationToken);

            userLogins = userLogins ?? new List<MongoDbUserLogin>();
            return userLogins
                .Select(p => new UserLoginInfo(p.LoginProvider, p.ProviderKey, p.ProviderDisplayName))
                .ToList();
        }

        public virtual async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            FilterDefinition<MongoDbUserLogin> filter = Builders<MongoDbUserLogin>.Filter.Where(
                p => p.LoginProvider == loginProvider
                && p.ProviderKey == providerKey);

            IAsyncCursor<MongoDbUserLogin> cursor = await _context.UserLogins.FindAsync(filter, null, cancellationToken);
            MongoDbUserLogin userLogin = await cursor.FirstOrDefaultAsync(cancellationToken);

            if (userLogin == null)
                return null;

            return await FindByIdAsync(userLogin.UserId.ToString(), cancellationToken);
        }


     
        //IDisposable
        public virtual void Dispose()
        {
        }

    }
}
