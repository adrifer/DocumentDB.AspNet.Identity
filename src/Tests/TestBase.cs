using Microsoft.Azure.Documents.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests
{
	public abstract class TestBase
	{
		protected DocumentClient Client;

		protected readonly string Endpoint = "https://localhost:8081/";
		protected readonly string Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
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