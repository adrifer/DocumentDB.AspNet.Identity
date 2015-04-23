using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DocumentDB.AspNet.Identity;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;

namespace Tests
{
    [TestClass]
    public class UserStoreTests
    {
        DocumentClient client = null;
        Database database = null;
        DocumentCollection collection = null;

        readonly string endpoint = "";
        readonly string key = "";
        readonly string userdb = "test";
        readonly string usercoll = "test";
        
        [TestInitialize]
        public void Init()
        {
            client = new DocumentClient(new Uri(endpoint), key);
            database = Utils.ReadOrCreateDatabase(client, userdb);
            collection = Utils.ReadOrCreateCollection(client, database.SelfLink, usercoll);
        }

        [TestMethod]
        public async Task CanCreateUser()
        {
            string username = "test_user";

            var userstore = new UserStore<IdentityUser>(new Uri(endpoint), key, userdb, usercoll);
            await userstore.CreateAsync(new IdentityUser("test_user"));
            IdentityUser user = await userstore.FindByNameAsync(username);
            Assert.IsNotNull(user);
            Assert.AreEqual(username, user.UserName);
        }

        [TestCleanup]
        public void Cleanup()
        {
            client.DeleteDatabaseAsync(database.SelfLink).Wait();
            client.Dispose();
        }
    }
}
