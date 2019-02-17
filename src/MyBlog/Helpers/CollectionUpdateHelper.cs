using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Helpers
{
    public static class CollectionUpdateHelper
    {
        public static CollectionUpdateChanges<T> UpdateCollection<T, TKey, TData>(
            ICollection<T> collectionToUpdate,
            Func<T, TKey> keySelector,
            IDictionary<TKey, TData> newData,
            Func<TData, T> newObjectFactory)
        {
            var changes = new CollectionUpdateChanges<T>();
            var addedData = new Dictionary<TKey, TData>(newData);
            foreach (var o in collectionToUpdate)
            {
                var key = keySelector(o);
                if (addedData.ContainsKey(key))
                {
                    addedData.Remove(key);
                }
                else
                {
                    changes.DeletedObjects.Add(o);
                }
            }

            foreach (var o in changes.DeletedObjects)
            {
                collectionToUpdate.Remove(o);
            }
            foreach (var data in addedData.Values)
            {
                var newObj = newObjectFactory(data);
                collectionToUpdate.Add(newObj);
                changes.AddedObjects.Add(newObj);
            }

            return changes;
        }
    }

    public class CollectionUpdateChanges<T>
    {
        public ICollection<T> DeletedObjects { get; set; } = new List<T>();
        public ICollection<T> AddedObjects { get; set; } = new List<T>();
    }
}
