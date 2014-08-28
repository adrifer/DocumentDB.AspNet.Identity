using Microsoft.AspNet.Identity;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;

namespace DocumentDB.AspNet.Identity
{
    public class UserStore<TUser> : 
        IUserLoginStore<TUser>, 
        IUserClaimStore<TUser>, 
        IUserRoleStore<TUser>,
        IUserPasswordStore<TUser>, 
        IUserSecurityStampStore<TUser>,
        IUserStore<TUser>, 
        IUserEmailStore<TUser>, 
        IUserLockoutStore<TUser, string>,
        IUserTwoFactorStore<TUser, string>,
        IUserPhoneNumberStore<TUser>
       where TUser : IdentityUser
    {
        private bool _disposed;

        private static DocumentClient client;

        private static string dataBaseName;

        private static bool initialized;

        private static Database database;

        private static Database Database
        {
            get
            {
                if (database == null)
                {
                    database = ReadOrCreateDatabase().Result;
                }

                return database;
            }
        }

        private static string usersLink;

        private static IQueryable<TUser> Users
        {
            get 
            {
                if (usersLink == null)
                {
                    var collection = InitializeConnection(Database.SelfLink, "Users").Result;
                    usersLink = collection.DocumentsLink;
                }
                return client.CreateDocumentQuery<TUser>(usersLink);
            }
        }

        public UserStore(Uri endPoint, string authKey, string dbName)
        {
            client = new DocumentClient(endPoint, authKey);
            dataBaseName = dbName;

            Initialize();
        }

        public Task AddLoginAsync(TUser user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            if (!user.Logins.Any(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey))
            {
                user.Logins.Add(login);
            }

            return Task.FromResult(0);
        }

        public async Task<TUser> FindAsync(UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (login == null)
                throw new ArgumentNullException("login");

            return await Task<TUser>.Run(() =>
                    Users.Where(u => u.Logins.Any(l => l.LoginProvider == login.LoginProvider && l.ProviderKey == login.ProviderKey))
                    .FirstOrDefault()
                );
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.Logins.ToIList());
        }

        public Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            if (login == null)
                throw new ArgumentNullException("login");

            user.Logins.Remove(u => u.LoginProvider == login.LoginProvider && u.ProviderKey == login.ProviderKey);

            return Task.FromResult(0);
        }

        public async Task CreateAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            var response = await client.CreateDocumentAsync(usersLink, user);
            user.Id = response.Resource.GetPropertyValue<string>("id");
        }

        public async Task DeleteAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            var doc = client.CreateDocumentQuery(usersLink)
                .Where(u => u.Id == user.Id).FirstOrDefault();

            await client.DeleteDocumentAsync(doc.SelfLink);
                
        }

        public async Task<TUser> FindByIdAsync(string userId)
        {
            ThrowIfDisposed();
            if (userId == null)
                throw new ArgumentNullException("userId");

            return await Task<TUser>.Run(() =>
                {
                    var user = Users.Where(u => u.Id == userId)
                    .AsEnumerable()
                    .FirstOrDefault();

                    return user;
                });
        }

        public async Task<TUser> FindByNameAsync(string userName)
        {
            ThrowIfDisposed();
            if (userName == null)
                throw new ArgumentNullException("userName");

            return await Task<TUser>.Run(() => {
                    var user = Users.Where(u => u.UserName == userName)
                    .AsEnumerable()
                    .FirstOrDefault();

                    return user;
                }
            );
        }

        public async Task UpdateAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            var doc = client.CreateDocumentQuery<Document>(usersLink)
                .Where(d => d.Id == user.Id)
                .AsEnumerable()
                .FirstOrDefault();

            await client.ReplaceDocumentAsync(doc.SelfLink, user);
        }

        public void Dispose()
        {
            _disposed = true;
        }

        public Task AddClaimAsync(TUser user, System.Security.Claims.Claim claim)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            if (!user.Claims.Any(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value))
            {
                user.Claims.Add(new IdentityUserClaim
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                });
            }

            return Task.FromResult(0);
        }

        public Task<IList<Claim>> GetClaimsAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            IList<Claim> result = user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
            return Task.FromResult(result);
        }

        public Task RemoveClaimAsync(TUser user, Claim claim)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.Claims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
            return Task.FromResult(0);
        }

        public Task AddToRoleAsync(TUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            if (roleName == null)
                throw new ArgumentNullException("roleName");

            if (!user.Roles.Any(x => x.Equals(roleName)))
            {
                user.Roles.Add(roleName);
            }

            return Task.FromResult(0);
        }

        public Task<IList<string>> GetRolesAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            IList<string> result = user.Roles.ToIList();
            return Task.FromResult(result);
        }

        public Task<bool> IsInRoleAsync(TUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            if (roleName == null)
                throw new ArgumentNullException("roleName");

            var isInRole = user.Roles.Any(x => x.Equals(roleName));

            return Task.FromResult(isInRole);
        }

        public Task RemoveFromRoleAsync(TUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            if (roleName == null)
                throw new ArgumentNullException("roleName");

            user.Roles.Remove(x => x.Equals(roleName));

            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.PasswordHash != null);
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetSecurityStampAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.SecurityStamp);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public async Task<TUser> FindByEmailAsync(string email)
        {
            ThrowIfDisposed();
            if (email == null)
                throw new ArgumentNullException("email");

            return await Task<TUser>.Run(() =>
                    Users.Where(u => u.Email == email)
                    .AsEnumerable()
                    .FirstOrDefault()
                );
        }

        public Task<string> GetEmailAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailAsync(TUser user, string email)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            if (email == null)
                throw new ArgumentNullException("email");

            user.Email = email;
            return Task.FromResult(0);
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task<int> GetAccessFailedCountAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.LockoutEnabled);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.LockoutEnd);
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.LockoutEnd = lockoutEnd;
            return Task.FromResult(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        public Task<string> GetPhoneNumberAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (phoneNumber == null)
                throw new ArgumentNullException("phoneNumber");

            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult(0);
        }

        private static async Task<Database> ReadOrCreateDatabase()
        {
            var databases = client.CreateDatabaseQuery()
                            .Where(db => db.Id == dataBaseName).ToArray();

            if (databases.Any())
            {
                return databases.First();
            }
            else
            {
                Database database = new Database { Id = dataBaseName };
                return await client.CreateDatabaseAsync(database);
            }
        }

        private static async Task<DocumentCollection> InitializeConnection(string databaseLink, string collectionId)
        {
            var collections = client.CreateDocumentCollectionQuery(databaseLink)
                            .Where(col => col.Id == collectionId).ToArray();

            if (collections.Any())
            {
                return collections.First();
            }
            else
            {
                return await client.CreateDocumentCollectionAsync(databaseLink,
                    new DocumentCollection { Id = collectionId });
            }
        }

        private static void Initialize()
        {
            if (!initialized)
            {
                var init = Users;
                initialized = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
