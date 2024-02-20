using System;
using System.Reflection;
using System.Collections.Generic;

namespace Easy
{



    public class OrderIndexAttribute : Attribute
    {
        public int orderIndex;

        public OrderIndexAttribute(int orderIndex)
        {
            this.orderIndex = orderIndex;
        }

        public static void Sort<T>(List<T> list)
        {
            bool isType = typeof(T) == typeof(Type);
            list.Sort((a, b) =>
            {
                Type typeA;
                Type typeB;
                if(isType)
                {
                    typeA = a as Type;
                    typeB = b as Type;
                }else{
                    typeA = a.GetType();
                    typeB = b.GetType();
                }

                if (typeA.IsDefined(typeof(OrderIndexAttribute)) && typeB.IsDefined(typeof(OrderIndexAttribute)))
                {
                    int indexA = typeA.GetCustomAttribute<OrderIndexAttribute>().orderIndex;
                    int indexB = typeB.GetCustomAttribute<OrderIndexAttribute>().orderIndex;

                    return indexA < indexB ? -1 : 1;
                }
                else if (typeA.IsDefined(typeof(OrderIndexAttribute)) && !typeB.IsDefined(typeof(OrderIndexAttribute)))
                {
                    return -1;
                }
                else if (!typeA.IsDefined(typeof(OrderIndexAttribute)) && typeB.IsDefined(typeof(OrderIndexAttribute)))
                {
                    return 1;
                }
                return 0;
            });
        }

        public static Dictionary<int,List<T>> GetBatchListByInterval<T>(List<T> list, int Interval = 100)
        {
            Dictionary<int, List<T>> result = new Dictionary<int, List<T>>();

            foreach (T item in list)
            {
                Type type = null;
                bool isType = typeof(T) == typeof(Type);
                if (isType)
                {
                    type = item as Type;
                }
                else
                {
                    type = item.GetType();
                }
                int index = int.MaxValue;
                if (type.IsDefined(typeof(OrderIndexAttribute)))
                {
                    index = type.GetCustomAttribute<OrderIndexAttribute>().orderIndex / Interval;
                }
                if(!result.ContainsKey(index))
                {
                    result[index] = new List<T>();
                }
                result[index].Add(item);
            }
            foreach (var item in result)
            {
                Sort<T>(item.Value);
            }
            return result;
        }

    }

}
