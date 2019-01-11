using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBlog.Helpers;
using Xunit;

namespace MyBlog.Tests
{
    public class CollectionUpdateHelperTests
    {
        [Fact]
        public void UpdateCollection_newDataShouldNotBeChanged()
        {
            var collection = new List<string> { "item1", "item2" };
            var newData = new Dictionary<char, string>
            {
                { '2',"item2 data" },
                { '3',"item3 data" },
            };
            var changes = CollectionUpdateHelper.updateCollection<string, char, string>(
                collection,
                s => s.Last(),
                newData,
                s => s.Substring(0, 5));

            Assert.Equal(2, newData.Count);
            Assert.Contains('2', newData.Keys);
            Assert.Contains('3', newData.Keys);

            Assert.Equal(2, collection.Count);
            Assert.Contains("item2", collection);
            Assert.Contains("item3", collection);

            Assert.Equal(1, changes.AddedObjects.Count);
            Assert.Contains("item3", changes.AddedObjects);

            Assert.Equal(1, changes.DeletedObjects.Count);
            Assert.Contains("item1", changes.DeletedObjects);
        }
    }
}
