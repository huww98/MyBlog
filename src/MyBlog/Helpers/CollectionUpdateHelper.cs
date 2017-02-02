using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Helpers
{
    static class CollectionUpdateHelper
    {
        public static CollectionUpdateChanges<T> updateCollection<T, TKey, TData>(
            ICollection<T> collectionToUpdate,
            Func<T, TKey> keySelector,
            IDictionary<TKey, TData> newData,
            Func<TData, T> newObjectFactory)
        {
            CollectionUpdateChanges<T> changes = new CollectionUpdateChanges<T>();
            foreach (var o in collectionToUpdate)
            {
                var key = keySelector(o);
                if (newData.ContainsKey(key))
                {
                    newData.Remove(key);
                }
                else
                {
                    changes.deletedObjects.Add(o);
                }
            }

            foreach (var o in changes.deletedObjects)
            {
                collectionToUpdate.Remove(o);
            }
            foreach (var data in newData.Values)
            {
                var newObj = newObjectFactory(data);
                collectionToUpdate.Add(newObj);
                changes.addedObjects.Add(newObj);
            }

            return changes;
        }
    }

    class CollectionUpdateChanges<T>
    {
        public ICollection<T> deletedObjects { get; set; } = new List<T>();
        public ICollection<T> addedObjects { get; set; } = new List<T>();

    }
}
