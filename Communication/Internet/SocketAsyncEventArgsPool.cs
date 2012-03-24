using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Communication.Internet
{    
    internal static class SocketAsyncEventArgsPool
    {
        /// <summary>
        /// Pool of SocketAsyncEventArgs
        /// </summary>
        static Stack<SocketAsyncEventArgs> m_Pool;

        /// <summary>
        /// Initializes the object pool to the specified size.
        /// </summary>
        /// <param name="capacity">Maximum number of SocketAsyncEventArgs objects the pool can hold.</param>
        static SocketAsyncEventArgsPool()
        {
            m_Pool = new Stack<SocketAsyncEventArgs>(1024);
        }

        /// <summary>
        /// Removes a SocketAsyncEventArgs instance from the pool.
        /// </summary>
        /// <returns>SocketAsyncEventArgs removed from the pool.</returns>
        static internal SocketAsyncEventArgs Pop()
        {
            lock (m_Pool)
            {
                if (m_Pool.Count > 0) return m_Pool.Pop();
                return null;
            }
        }

        /// <summary>
        /// Add a SocketAsyncEventArg instance to the pool. 
        /// </summary>
        /// <param name="item">SocketAsyncEventArgs instance to add to the pool.</param>
        static internal void Push(SocketAsyncEventArgs item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");
            }
            lock (m_Pool)
            {
                m_Pool.Push(item);
            }
        }

    }
}
