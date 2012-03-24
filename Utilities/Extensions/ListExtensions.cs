using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// Fast version of the RemoveAt function. Overwrites the element at the specified index
        /// with the last element in the list, then removes the last element, thus lowering the 
        /// inherent O(n) cost to O(1). Intended to be used on *unordered* lists only.
        /// </summary>
        /// <param name="_list">IList</param>
        /// <param name="_index">Index of the element to be removed.</param>
        public static void RemoveAtFast<T>(this IList<T> _list, int _index)
        {
            if (_index < 0) return;
            
            //get the amount of items in the list once
            int count = _list.Count - 1;

            if (_index > count) return;

            //copy the last item to the index being removed
            _list[_index] = _list[count];
            ///still calling remove at because the old item was copied to the removed index 
            ///and we need the list to reflect the remove operation            
            _list.RemoveAt(count);
            ///this will remove the last item which will not allow array.copy to be called resulting in a faster removal
            ///array.copy is not called  because the element being removed it at the end of the array.
        }

        /// <summary>
        /// Performs the specified action on each element in the list while providing the index to the action
        /// </summary>
        /// <typeparam name="T">The Type of Elements in the IList</typeparam>
        /// <param name="_list">The IList to perform the Action on</param>
        /// <param name="_action">The Action to perform on the element of the IList</param>
        public static void ForEach<T>(this IList<T> _list, Action<T> _action)
        {
            for (int index = 0, end = _list.Count; index < end; ++index)
            {
                _action(_list[index]);
            }
        }

        /// <summary>
        /// Performs the specified action on each element in the list while providing the index to the action
        /// </summary>
        /// <typeparam name="T">The Type of Elements in the IList</typeparam>
        /// <param name="_list">The IList to perform the Action on</param>
        /// <param name="_action">The Action to perform on the element of the IList</param>
        public static void ForEachWithIndex<T>(this IList<T> _list, Action<T, int> _action)
        {
            for (int index = 0, end = _list.Count; index < end; ++index)
            {
                _action(_list[index], index);
            }
        }

        /// <summary>
        /// Performs the specified action on each element in the list while providing the index and the origional list to the action
        /// </summary>
        /// <typeparam name="T">The Type of Elements in the IList</typeparam>
        /// <param name="_list">The IList to perform the Action on</param>
        /// <param name="_action">The Action to perform on the element of the IList</param>
        public static void ForEachWithIndex<T>(this IList<T> _list, Action<IList<T>, T, int> _action)
        {
            for (int index = 0, end = _list.Count; index < end; ++index)
            {
                _action(_list, _list[index], index);
            }
        }

        /// <summary>
        /// Includes (Adds) a item into the given if it is not already present
        /// </summary>
        /// <typeparam name="T">The type of the item being included into the list</typeparam>
        /// <param name="_list">The list to include the item</param>
        /// <param name="item">The item to be included</param>
        public static void Include<T>(this IList<T> _list, T item)
        {
            if (_list.Contains(item)) return;
            _list.Add(item);
        }
    }
}
