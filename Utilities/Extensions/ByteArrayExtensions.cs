using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.Extensions
{
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// 	Find the first occurence of an byte[] in another byte[]
        /// </summary>
        /// <param name = "buf1">the byte[] to search in</param>
        /// <param name = "buf2">the byte[] to find</param>
        /// <returns>the first position of the found byte[] or -1 if not found</returns>
        /// <remarks>
        /// 	Contributed by blaumeister, http://www.codeplex.com/site/users/view/blaumeiser
        /// </remarks>
        public static int FindArrayInArray(this byte[] buf1, byte[] buf2)
        {
            int i, j, e = buf2.Length, je = buf1.Length - buf2.Length;
            for (j = 0 ; j < je; ++j)
            {
                for (i = 0; i < e; ++i)
                {
                    if (buf1[j + i] != buf2[i])
                        break;
                }
                if (i == e)
                    return j;
            }
            return -1;
        }
    }
}
