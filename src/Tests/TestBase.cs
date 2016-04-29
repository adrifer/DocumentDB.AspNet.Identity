using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Utilities;

namespace Tests
{
    public abstract class TestBase
    {
        protected DocumentClient Client;
        protected Database Database;
        protected DocumentCollection Collection;

        protected readonly string Endpoint = "";
        protected readonly string Key = "";
        protected readonly string Userdb = "test";
        protected readonly string Usercoll = "test";

        [TestInitialize]
        public void Init()
        {
            Client = new DocumentClient(new Uri(Endpoint), Key);
            Database = DatabaseFactory.ReadOrCreateDatabase(Client, Userdb);
            Collection = DatabaseFactory.ReadOrCreateCollection(Client, Database.SelfLink, Usercoll);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Client.DeleteDatabaseAsync(Database.SelfLink).Wait();
            Client.Dispose();
        }
    }
}