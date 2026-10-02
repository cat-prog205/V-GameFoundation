namespace Gpm.Ui
{
    using System;
    using System.Collections.Generic;

    public class InfiniteScrollExtensions
    {
        public static Comparison<InfiniteScroll.DataContext> CreateSort<TData, TKey>(
            Func<TData, TKey> keySelector, 
            bool isDescending = false) 
            where TData : class 
        {
            return (a, b) =>
            {
                var itemA = a.data as TData;
                var itemB = b.data as TData;

                if (itemA == null && itemB == null) return 0;
                if (itemA == null) return 1;
                if (itemB == null) return -1;

                TKey valueA = keySelector(itemA);
                TKey valueB = keySelector(itemB);
                
                int result = Comparer<TKey>.Default.Compare(valueA, valueB);

                return isDescending ? -result : result;
            };
        }
    }

}

