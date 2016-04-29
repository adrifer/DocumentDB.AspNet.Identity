using System;
using System.Threading.Tasks;
using DocumentDB.AspNet.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Tests
{
    [TestClass]
    public class UserStoreTests : TestBase
    {
        private readonly UserStore<IdentityUser> _userStore;

        public UserStoreTests()
        {
            _userStore = new UserStore<IdentityUser>(new Uri(Endpoint), Key, Userdb, Usercoll);
        }

        private IdentityUser _testUser = new IdentityUser
        {
            UserName = "test@test.com",
            Email = "test@test.com"
        };

        [TestMethod]
        public async Task CanCreateUser()
        {
            await _userStore.CreateAsync(_testUser);

            var savedUser = await _userStore.FindByEmailAsync(_testUser.Email);

            Assert.IsNotNull(savedUser);
            Assert.AreEqual(_testUser.Email, savedUser.Email);
        }

        [TestMethod]
        public async Task UpdatesAreAppliedToUser()
        {
            await CanCreateUser();

            var savedUser = await _userStore.FindByEmailAsync(_testUser.Email);
            if (savedUser == null)
            {
                throw new NullReferenceException("savedUser");
            }

            savedUser.EmailConfirmed = true;

            await _userStore.UpdateAsync(savedUser);

            savedUser = await _userStore.FindByEmailAsync(_testUser.Email);

            Assert.IsNotNull(savedUser);
            Assert.IsTrue(savedUser.EmailConfirmed);
        }
    }
}