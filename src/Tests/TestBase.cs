using System;
using Microsoft.Azure.Documents.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    public abstract class TestBase
    {
        protected DocumentClient Client;

        protected readonly string Endpoint = "";
        protected readonly string Key = "";
        protected readonly string Database = "test";
        protected readonly string Collection = "test";

        [TestInitialize]
        public void Init()
        {
            Client = new DocumentClient(new Uri(Endpoint), Key);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(Database)).Wait();
            Client.Dispose();
        }
    }
}