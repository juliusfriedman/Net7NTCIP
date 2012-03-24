using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.Classes
{
    //Class which represnts bytes as chars
    public unsafe sealed class BString
    {
        #region Fields
        string _string = System.String.Empty;
        #endregion

        #region Constructors

        public BString(string s)
        {
            _string = s;
        }

        public BString(char[] chars, int start) : this(chars, start, chars.Length - start) { }

        public BString(char[] chars, int start, int length)
        {
            _string = new String(chars, start, length);
        }

        public unsafe BString(byte[] chars, int start) : this(chars, 0, chars.Length - start, null) { }

        public unsafe BString(byte[] chars, int start, int length, System.Text.Encoding encoding = null)
        {
            if (encoding == null) encoding = System.Text.Encoding.Default;
            fixed (byte* bytep = chars)
            {
                sbyte* charp = (sbyte*)bytep;
                _string = new string(charp, start, length, encoding);
            }
        }

        #endregion

        #region Indexers

        public char this[int index]
        {
            get
            {
                return _string[index];
            }
        }

        public byte this[short index]
        {
            get
            {
                return (byte)_string[index];
            }
        }

        #endregion

        #region Methods

        public char GetChar(int i)
        {
            return this[i];
        }

        public byte GetByte(int i)
        {
            return (Byte)this[i];
        }

        /// <summary>
        /// Get the bytes of the BString
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            char[] charz = _string.ToCharArray();
            byte[] bytes = new byte[charz.Length];
            for (int i = 0, e = charz.Length; i < e; ++i) bytes[i] = (byte)charz[i];
            return bytes;
        }

        /// <summary>
        /// Get the chars of the BString
        /// </summary>
        /// <returns></returns>
        public char[] ToCharArray()
        {
            return _string.ToCharArray();
        }

        /// <summary>
        /// Determines equality
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is BString) return (obj as BString) == this;
            else if (obj is char[]) return (obj as char[]) == this;
            else if (obj is byte[]) return (obj as byte[]) == this;
            else if (obj is String) return (obj as String) == this;
            else return _string.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _string.GetHashCode();
        }

        public override string ToString()
        {
            return _string;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Compare a BString to a String 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(BString a, String b)
        {
            return a._string == b;
        }

        /// <summary>
        /// Compare a BString to a BString 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(BString a, BString b)
        {
            return a._string == b._string;
        }

        /// <summary>
        /// Compare a BString to a byte 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(BString a, byte[] b)
        {
            return Object.Equals(a.ToByteArray(), b);
        }

        /// <summary>
        /// Compare a BString to a char[] 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(BString a, char[] b)
        {
            return Object.Equals(a.ToCharArray(), b);
        }

        /// <summary>
        /// Compare a BString to a String
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(BString a, String b)
        {
            return !(a == b);
        }

        public static bool operator !=(BString a, BString b)
        {
            return !(a == b);
        }

        public static bool operator !=(BString a, byte[] b)
        {
            return !(a == b);
        }

        public static bool operator !=(BString a, char[] b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Byte[] to BString implicit
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static implicit operator BString(byte[] s)
        {
            return new BString(s, 0);
        }

        /// <summary>
        /// Char[] to BString implicit
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static implicit operator BString(char[] s)
        {
            return new BString(s, 0);
        }

        /// <summary>
        /// BString to Byte[] implicit
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static implicit operator byte[](BString b)
        {
            return b.ToByteArray();
        }

        /// <summary>
        /// BString to Char[] implicit
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static implicit operator char[](BString b)
        {
            return b.ToCharArray();
        }

        /// <summary>
        /// BString to String implcit
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static implicit operator System.String(BString s)
        {
            return s._string;
        }

        /// <summary>
        /// String to BString implicit
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static implicit operator BString(System.String s)
        {
            return new BString(s);
        }


        #endregion
    }
}
