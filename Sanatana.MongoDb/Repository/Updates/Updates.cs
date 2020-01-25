using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Sanatana.MongoDb.Repository
{
    public struct Updates<T>
        where T : class
    {
        public List<Update<T>> Sets { get; set; }
        public List<Update<T>> Increments { get; set; }
        public List<Update<T>> SetOnInserts { get; set; }
        public List<UpdateCollection<T>> Pushes { get; set; }
        public List<UpdateCollection<T>> Pulls { get; set; }
        public bool IsEmpty
        {
            get
            {
                return (Sets == null || Sets.Count == 0)
                    && (Increments == null || Increments.Count == 0)
                    && (SetOnInserts == null || SetOnInserts.Count == 0)
                    && (Pushes == null || Pushes.Count == 0)
                    && (Pulls == null || Pulls.Count == 0);
            }
        }


        //ctor
        public static Updates<T> Empty()
        {
            var updates = new Updates<T>();
            return updates;
        }


        //methods
        public Updates<T> Set(Expression<Func<T, object>> propertyExpression, object value)
        {
            var update = Update<T>.Property(propertyExpression, value);
            Sets = Sets ?? new List<Update<T>>();
            Sets.Add(update);
            return this;
        }

        public Updates<T> Increment(Expression<Func<T, object>> propertyExpression, object value)
        {
            var update = Update<T>.Property(propertyExpression, value);
            Increments = Increments ?? new List<Update<T>>();
            Increments.Add(update);
            return this;
        }

        public Updates<T> SetOnInsert(Expression<Func<T, object>> propertyExpression, object value)
        {
            var update = Update<T>.Property(propertyExpression, value);
            SetOnInserts = SetOnInserts ?? new List<Update<T>>();
            SetOnInserts.Add(update);
            return this;
        }

        public Updates<T> Push<TProperty>(Expression<Func<T, List<TProperty>>> propertyExpression, TProperty value)
        {
            var update = UpdateCollection<T>.Property(propertyExpression, value);
            Pushes = Pushes ?? new List<UpdateCollection<T>>();
            Pushes.Add(update);
            return this;
        }

        public Updates<T> Pull<TProperty>(Expression<Func<T, List<TProperty>>> propertyExpression, TProperty value)
        {
            var update = UpdateCollection<T>.Property(propertyExpression, value);
            Pulls = Pulls ?? new List<UpdateCollection<T>>();
            Pulls.Add(update);
            return this;
        }
    }

}
