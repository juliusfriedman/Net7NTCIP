using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Threading;
using System.Security.Cryptography;
using System.IO;
using System.Xml;

namespace Airlink
{
    public class Modem : StringDictionary
    {
        // Fields
        private Mutex mutex;
        private static byte[] rijndaelIV = new byte[] { 0x59, 0x20, 0x40, 0x2b, 0x4e, 0xb1, 0xe0, 0x23, 0xc9, 0x2c, 170, 0x71, 0x5f, 0xce, 0xf1, 0xe2 };
        private static byte[] rijndaelKey = new byte[] { 
        0xe0, 0x98, 70, 0x7c, 0x3a, 0x95, 0x6c, 0xae, 30, 0xdf, 0xe7, 0x9b, 0x59, 0xd5, 0xcb, 0x80, 
        0x45, 0x5f, 0x20, 0xea, 0x7b, 80, 0xf2, 40, 0x4c, 0x25, 0xba, 0xee, 0x62, 80, 0xd9, 0xf7
     };

        // Methods
        public Modem(string method, string address)
        {
            this.mutex = new Mutex();
            this.Add("method", method);
            this.Add("addr", address);
        }

        public Modem(string method, string address, string name)
        {
            this.mutex = new Mutex();
            this.Add("method", method);
            this.Add("addr", address);
            this.Add("name", name);
        }

        private static string Decrypt(string cypherText)
        {
            ICryptoTransform transform = new RijndaelManaged().CreateDecryptor(rijndaelKey, rijndaelIV);
            byte[] buffer = Convert.FromBase64String(cypherText);
            MemoryStream stream = new MemoryStream(buffer);
            CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
            byte[] buffer2 = new byte[buffer.Length];
            int num = stream2.Read(buffer2, 0, buffer.Length);
            if (buffer2[0] > (num - 1))
            {
                return "";
            }
            return Encoding.UTF8.GetString(buffer2, num - buffer2[0], buffer2[0]);
        }

        private string EmptyIfNULL(string a)
        {
            if (a != null)
            {
                return a;
            }
            return string.Empty;
        }

        private static string Encrypt(string plainText)
        {
            ICryptoTransform transform = new RijndaelManaged().CreateEncryptor(rijndaelKey, rijndaelIV);
            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            if (bytes.Length > 0xff)
            {
                throw new Exception("Error: plaintext is too large.");
            }
            byte[] data = new byte[((((bytes.Length + 1) + 8) + transform.InputBlockSize) / transform.InputBlockSize) * transform.InputBlockSize];
            RandomNumberGenerator.Create().GetBytes(data);
            data[0] = (byte)bytes.Length;
            Array.Copy(bytes, 0, data, data.Length - bytes.Length, bytes.Length);
            MemoryStream stream = new MemoryStream();
            CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Write);
            stream2.Write(data, 0, data.Length);
            stream2.FlushFinalBlock();
            return Convert.ToBase64String(stream.ToArray());
        }

        public static Modem Load(XmlNode node)
        {
            XmlNode node2 = node.SelectSingleNode("method");
            XmlNode node3 = node.SelectSingleNode("addr");
            if ((node2 == null) || (node3 == null))
            {
                return null;
            }
            Modem modem = new Modem(node2.InnerText, node3.InnerText);
            foreach (XmlNode node4 in node.SelectNodes("*[name()!='method' and name()!='addr']"))
            {
                if (node4.InnerText.Length > 0)
                {
                    if (node4.Name != "password")
                    {
                        modem.Add(node4.Name, node4.InnerText);
                    }
                    else
                    {
                        XmlNode node5 = node4.SelectSingleNode("@enc");
                        if ((node5 != null) && (node5.InnerText == "0"))
                        {
                            modem.Add(node4.Name, Decrypt(node4.InnerText));
                            continue;
                        }
                        modem.Add(node4.Name, node4.InnerText);
                    }
                }
            }
            return modem;
        }

        public void Save(XmlTextWriter writer)
        {
            writer.WriteStartElement("modem");
            string[] array = new string[this.Keys.Count];
            this.Keys.CopyTo(array, 0);
            Array.Sort(array);
            foreach (string str in array)
            {
                if (this[str] != "")
                {
                    if (str != "password")
                    {
                        writer.WriteElementString(str, this[str]);
                    }
                    else
                    {
                        writer.WriteStartElement(str);
                        writer.WriteAttributeString("enc", "0");
                        writer.WriteString(Encrypt(this[str]));
                        writer.WriteEndElement();
                    }
                }
            }
            writer.WriteEndElement();
        }

        // Properties
        public string Address
        {
            get
            {
                return this.EmptyIfNULL(this["addr"]);
            }
        }

        public HashKey Key
        {
            get
            {
                return new HashKey(this.Method, this.Address);
            }
        }

        public string Method
        {
            get
            {
                return this.EmptyIfNULL(this["method"]);
            }
        }

        public Mutex Mutex
        {
            get
            {
                return this.mutex;
            }
        }

        public string Name
        {
            get
            {
                return this.EmptyIfNULL(this["name"]);
            }
            set
            {
                this["name"] = value;
            }
        }

        public string Password
        {
            get
            {
                return this.EmptyIfNULL(this["password"]);
            }
            set
            {
                this["password"] = value;
            }
        }

        public string Result
        {
            get
            {
                return this.EmptyIfNULL(this["result"]);
            }
            set
            {
                this["result"] = value;
                this["time"] = DateTime.Now.ToString("MM/dd HH:mm:ss");
            }
        }

        public DateTime Time
        {
            get
            {
                return DateTime.Parse(this["time"]);
            }
        }

        public string Username
        {
            get
            {
                return "user";
            }
        }

        // Nested Types
        public class HashKey
        {
            // Fields
            private string address;
            private string method;

            // Methods
            public HashKey(string method, string address)
            {
                this.method = method;
                this.address = address;
            }

            public override bool Equals(object o)
            {
                return ((o is Modem.HashKey) && ((this.method == ((Modem.HashKey)o).method) && (this.address == ((Modem.HashKey)o).address)));
            }

            public override int GetHashCode()
            {
                return (this.method + this.address).GetHashCode();
            }

            // Properties
            public string Address
            {
                get
                {
                    return this.address;
                }
            }

            public string Method
            {
                get
                {
                    return this.method;
                }
            }
        }
    }


}
