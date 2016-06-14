using System;
using System.Threading.Tasks;
using DocumentDB.AspNet.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Tests
{
    [TestClass]
    public class UserStoreTests : TestBase
    {
        private readonly UserStore<IdentityUser> _userStore;

        public UserStoreTests()
        {
            Init();
            _userStore = new UserStore<IdentityUser>(Client, Database, Collection, true);
        }

        [TestMethod]
        public async Task CanCreateUser()
        {
            var testUser = new IdentityUser
            {
                UserName = "testUser01",
                Email = "testUser01@test.com",
            };

            await _userStore.CreateAsync(testUser);

            var savedUser = await _userStore.FindByEmailAsync(testUser.Email);

            Assert.IsNotNull(savedUser);
            Assert.AreEqual(testUser.Email, savedUser.Email);
        }

        [TestMethod]
        public async Task UpdatesAreAppliedToUser()
        {
            var testUser = new IdentityUser
            {
                UserName = "testUser01",
                Email = "testUser01@test.com",
            };

            await _userStore.CreateAsync(testUser);

            var savedUser = await _userStore.FindByEmailAsync(testUser.Email);
            if (savedUser == null)
            {
                throw new NullReferenceException("savedUser");
            }

            savedUser.EmailConfirmed = true;

            await _userStore.UpdateAsync(savedUser);

            savedUser = await _userStore.FindByEmailAsync(testUser.Email);

            Assert.IsNotNull(savedUser);
            Assert.IsTrue(savedUser.EmailConfirmed);
        }

        [TestMethod]
        public async Task UsersWithCustomIdsPersistThroughStorage()
        {
            var testUser = new IdentityUser
            {
                UserName = "testUser03",
                Email = "testUser03@test.com",
                Id = "testUser03@test.com",
            };

            await _userStore.CreateAsync(testUser);

            var savedUser = await _userStore.FindByEmailAsync(testUser.Email);

            Assert.IsNotNull(savedUser);
            Assert.AreEqual(testUser.Id, savedUser.Id);
        }

        [TestMethod]
        public async Task UsersWithNoSetIdDefaultToNewGuid()
        {
            var testUser = new IdentityUser
            {
                UserName = "testUser04",
                Email = "testUser04@test.com"
            };

            await _userStore.CreateAsync(testUser);

            var savedUser = await _userStore.FindByEmailAsync(testUser.Email);

            Guid guidId;
            Assert.IsTrue(!string.IsNullOrEmpty(savedUser.Id));
            Assert.IsTrue(Guid.TryParse(savedUser.Id, out guidId));
        }

        [TestMethod]
        public async Task CanFindUserByLoginInfo()
        {
            var testUser = new IdentityUser
            {
                UserName = "testUser05",
                Email = "testUser05@test.com"
            };

            await _userStore.CreateAsync(testUser);

            var user = await _userStore.FindByEmailAsync(testUser.Email);
            Assert.IsNotNull(user);      

            var loginInfo = new UserLoginInfo("ATestLoginProvider", "ATestKey292929");
            await _userStore.AddLoginAsync(user, loginInfo);

            var userByLoginInfo = await _userStore.FindAsync(loginInfo);

            Assert.IsNotNull(userByLoginInfo);
        }
    }
}