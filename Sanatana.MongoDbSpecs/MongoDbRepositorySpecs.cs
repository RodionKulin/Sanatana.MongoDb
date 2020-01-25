using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using Sanatana.MongoDb;
using Sanatana.MongoDb.Repository;
using Sanatana.MongoDbSpecs.Samples;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using System.Linq;

namespace Sanatana.MongoDbSpecs
{
    [TestFixture]
    public class MongoDbRepositorySpecs
    {
        private IMongoCollection<Post> _collection;

        //shared
        [OneTimeSetUp]
        public void ClearCollection()
        {
            var connectionSettings = new MongoDbConnectionSettings
            {
                DatabaseName = "MongoDbSpecs",
                Host = "localhost",
                Port = 27017
            };
            var dbContext = new SamplesMongoDbContext(connectionSettings);
            _collection = dbContext.Posts;
            _collection.Database.DropCollection("Posts");
        }

        private IRepository<Post> GetRepositoryToTest(bool isMongoDbRepo)
        {
            return isMongoDbRepo
                ? (IRepository<Post>)new MongoDbRepository<Post>(_collection)
                : new MemoryRepository<Post>();
        }


        //specs
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task when_inserting_one_findone_returns_inserted_entity(bool isMongoDbRepo)
        {
            //prepare
            IRepository<Post> repository = GetRepositoryToTest(isMongoDbRepo);

            //invoke
            var newEntity = new Post
            {
                ID = ObjectId.GenerateNewId(),
                Text = "insert one",
                Counter = 1
            };
            await repository.InsertOne(newEntity);

            //assert
            var actualEntity = await repository.FindOne(x => x.ID == newEntity.ID);
            actualEntity.Should().BeEquivalentTo(newEntity);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task when_inserting_many_findmany_returns_inserted_entity(bool isMongoDbRepo)
        {
            //prepare
            IRepository<Post> repository = GetRepositoryToTest(isMongoDbRepo);

            //invoke
            Post[] newEntities = Enumerable.Range(1, 2)
                .Select(x => new Post
                {
                    ID = ObjectId.GenerateNewId(),
                    Text = "insert many",
                    Counter = x
                })
                .ToArray();
            await repository.InsertMany(newEntities);

            //assert
            ObjectId[] ids = newEntities.Select(x => x.ID).ToArray();
            var actualEntities = await repository.FindMany(x => ids.Contains(x.ID), 0, 10);
            actualEntities.Should().BeEquivalentTo(newEntities);
        }

        [Test]
        //[TestCase(true)]
        [TestCase(false)]
        public async Task when_deleting_one_findone_returns_nothing(bool isMongoDbRepo)
        {
            //prepare
            IRepository<Post> repository = GetRepositoryToTest(isMongoDbRepo);
            var newEntity = new Post
            {
                ID = ObjectId.GenerateNewId(),
                Text = "insert one",
                Counter = 1
            };
            await repository.InsertOne(newEntity);

            //invoke
            long deletedCount = await repository.DeleteOne(x => x.ID == newEntity.ID);

            //assert
            deletedCount.Should().Be(1);

            Post actualEntity = await repository.FindOne(x => x.ID == newEntity.ID);
            actualEntity.Should().BeNull();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task when_deleting_many_findmany_returns_nothing(bool isMongoDbRepo)
        {
            //prepare
            IRepository<Post> repository = GetRepositoryToTest(isMongoDbRepo);
            Post[] newEntities = Enumerable.Range(1, 2)
                .Select(x => new Post
                {
                    ID = ObjectId.GenerateNewId(),
                    Text = "insert many",
                    Counter = x
                })
                .ToArray();
            await repository.InsertMany(newEntities);

            //invoke
            ObjectId[] ids = newEntities.Select(x => x.ID).ToArray();
            long deletedCount = await repository.DeleteMany(x => ids.Contains(x.ID));

            //assert
            deletedCount.Should().Be(newEntities.Length);

            var actualEntities = await repository.FindMany(x => ids.Contains(x.ID), 0, 10);
            actualEntities.Should().BeEmpty();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task when_findoneanddeleting_then_findone_returns_nothing(bool isMongoDbRepo)
        {
            //prepare
            IRepository<Post> repository = GetRepositoryToTest(isMongoDbRepo);
            var newEntity = new Post
            {
                ID = ObjectId.GenerateNewId(),
                Text = "findoneanddelete",
                Counter = 1
            };
            await repository.InsertOne(newEntity);

            //invoke
            Post deletedEntity = await repository.FindOneAndDelete(x => x.ID == newEntity.ID);

            //assert
            deletedEntity.Should().BeEquivalentTo(newEntity);

            Post actualEntity = await repository.FindOne(x => x.ID == newEntity.ID);
            actualEntity.Should().BeNull();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task when_findoneandupdating_then_findone_returns_updated(bool isMongoDbRepo)
        {
            //prepare
            IRepository<Post> repository = GetRepositoryToTest(isMongoDbRepo);
            var newEntity = new Post
            {
                ID = ObjectId.GenerateNewId(),
                Text = "findoneandupdate",
                Counter = 1
            };
            await repository.InsertOne(newEntity);

            //invoke
            var updates = Updates<Post>.Empty().Set(x => x.Text, "updated");
            Post updatedEntity = await repository.FindOneAndUpdate(x => x.ID == newEntity.ID, updates);

            //assert
            updatedEntity.Should().BeEquivalentTo(newEntity);

            Post actualEntity = await repository.FindOne(x => x.ID == newEntity.ID);
            newEntity.Text = "updated";
            actualEntity.Should().BeEquivalentTo(newEntity);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task when_updating_many_with_push_then_findmany_returns_updated_results(bool isMongoDbRepo)
        {
            //prepare
            IRepository<Post> repository = GetRepositoryToTest(isMongoDbRepo);
            Post[] newEntities = Enumerable.Range(1, 2)
                .Select(x => new Post
                {
                    ID = ObjectId.GenerateNewId(),
                    Text = "insert many",
                    Counter = 1,
                    History = new List<int> { 1, 2 }
                })
                .ToArray();
            await repository.InsertMany(newEntities);

            //invoke
            ObjectId[] ids = newEntities.Select(x => x.ID).ToArray();
            var updates = Updates<Post>.Empty()
                .Set(x => x.Text, "updated")
                .Increment(x => x.Counter, 2)
                .Push(x => x.History, 3);
            long updatedCount = await repository.UpdateMany(x => ids.Contains(x.ID), updates);

            //assert
            updatedCount.Should().Be(newEntities.Length);

            List<Post> actualEntities = await repository.FindMany(x => ids.Contains(x.ID), 0, 10);
            actualEntities.Should().HaveCount(newEntities.Length);
            actualEntities.ForEach(actual =>
            {
                actual.Text.Should().Be("updated");
                actual.Counter.Should().Be(3);
                actual.History.Should().Contain(3); ;
            });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task when_updating_many_with_pull_then_findmany_returns_updated_results(bool isMongoDbRepo)
        {
            //prepare
            IRepository<Post> repository = GetRepositoryToTest(isMongoDbRepo);
            Post[] newEntities = Enumerable.Range(1, 2)
                .Select(x => new Post
                {
                    ID = ObjectId.GenerateNewId(),
                    Text = "insert many",
                    Counter = 1,
                    History = new List<int> { 1, 2 }
                })
                .ToArray();
            await repository.InsertMany(newEntities);

            //invoke
            ObjectId[] ids = newEntities.Select(x => x.ID).ToArray();
            var updates = Updates<Post>.Empty()
                .Set(x => x.Text, "updated")
                .Increment(x => x.Counter, 2)
                .Pull(x => x.History, 1);
            long updatedCount = await repository.UpdateMany(x => ids.Contains(x.ID), updates);

            //assert
            updatedCount.Should().Be(newEntities.Length);

            List<Post> actualEntities = await repository.FindMany(x => ids.Contains(x.ID), 0, 10);
            actualEntities.Should().HaveCount(newEntities.Length);
            actualEntities.ForEach(actual =>
            {
                actual.Text.Should().Be("updated");
                actual.Counter.Should().Be(3);
                actual.History.Should().NotContain(1);
            });
        }

        [Test]
        public async Task when_upserting_many_findmany_returns_upserted_results()
        {
            //prepare
            IRepository<Post> repository = new MongoDbRepository<Post>(_collection);
            List<Post> newEntities = Enumerable.Range(1, 3)
                .Select(x => new Post
                {
                    ID = ObjectId.GenerateNewId(),
                    Text = "original",
                    Counter = 1
                })
                .ToList();
            await repository.InsertOne(newEntities.First());
            newEntities.ForEach(x => x.Text = "upsert many");

            //invoke
            long upsertedCount = await repository.UpsertMany(newEntities);

            //assert
            upsertedCount.Should().Be(newEntities.Count);

            ObjectId[] ids = newEntities.Select(x => x.ID).ToArray();
            List<Post> actualEntities = await repository.FindMany(x => ids.Contains(x.ID), 0, 10);
            actualEntities.Should().HaveCount(newEntities.Count);
            actualEntities.ForEach(actual =>
            {
                actual.Text.Should().Be("upsert many");
                actual.Counter.Should().Be(1);
            });
        }

        [Test]
        public async Task when_updating_many_then_findmany_returns_updated_results()
        {
            //prepare
            IRepository<Post> repository = new MongoDbRepository<Post>(_collection);
            List<Post> newEntities = Enumerable.Range(1, 3)
                .Select(x => new Post
                {
                    ID = ObjectId.GenerateNewId(),
                    Text = "original",
                    Counter = 1
                })
                .ToList();
            await repository.InsertMany(newEntities);
            newEntities.ForEach(x => x.Text = "update many");

            //invoke
            long upsertedCount = await repository.UpdateMany(newEntities);

            //assert
            upsertedCount.Should().Be(newEntities.Count);

            ObjectId[] ids = newEntities.Select(x => x.ID).ToArray();
            List<Post> actualEntities = await repository.FindMany(x => ids.Contains(x.ID), 0, 10);
            actualEntities.Should().HaveCount(newEntities.Count);
            actualEntities.ForEach(actual =>
            {
                actual.Text.Should().Be("update many");
                actual.Counter.Should().Be(1);
            });
        }
    }
}
