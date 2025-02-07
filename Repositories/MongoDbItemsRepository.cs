using Catalog.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Catalog.Repositories
{
    public class MongoDbItemsRepository : IItemsRepository
    {
        private const string databaseName = "catalog";
        private const string collectionName = "items";
        private readonly IMongoCollection<Item> itemsCollection;
       
       public MongoDbItemsRepository(IMongoClient mongoClient)
       {
         IMongoDatabase database = mongoClient.GetDatabase(databaseName);
         itemsCollection= database.GetCollection<Item> (collectionName);
       }

       public async Task CreateItemAsync(Item item)
       {
          await itemsCollection.InsertOneAsync(item);
       }

        public async Task DeleteItemAsync(Item item)
        {
            await itemsCollection.DeleteOneAsync(x=>x.Id==item.Id);
        }

        public async Task<Item> GetItemAsync(Guid id)
        {
            return await itemsCollection.Find(x=>x.Id==id).SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<Item>> GetItemsAsync()
        {
            return await itemsCollection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task UpdateItemAsync(Item item)
        {
            await itemsCollection.ReplaceOneAsync(x=>x.Id==item.Id,item);
        }
    }
}
       