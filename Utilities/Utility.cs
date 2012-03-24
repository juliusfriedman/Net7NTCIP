using System;
using System.Net;
using iText = iTextSharp.text;
using mshtml;
using System.Windows;
using ASTITransportation.Extensions;

namespace ASTITransportation.Utilities
{
    public sealed class Utility
    {
        private Utility(){}

        /// <summary>
        /// Determine the computers first Ipv4 Address 
        /// </summary>
        /// <returns>The First IPV4 Address Found on the Machine</returns>
        public static IPAddress GetV4IPAddress()
        {
            return GetFirstIPAddress(System.Net.Sockets.AddressFamily.InterNetwork);
        }

        public static IPAddress GetFirstIPAddress(System.Net.Sockets.AddressFamily addressFamily)
        {
            foreach (System.Net.IPAddress ip in System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList) 
                if (ip.AddressFamily == addressFamily) return ip;
            return IPAddress.Loopback;
        }

        public static UInt32 HammingDistance(UInt32 x, UInt32 y)
        {
            UInt32 dist = 0, val = x ^ y;
            // Count the number of set bits (Knuth's algorithm)   
            while (val > 0) { ++dist; val &= val - 1; }
            return dist;
        }

        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = ++i);

            for (int j = 0; j <= m; d[0, j] = ++j);

            // Step 3
            for (int i = 1; i <= n; ++i)
            {
                //Step 4
                for (int j = 1; j <= m; ++j)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        public static void CreatePDF(System.IO.Stream stream, string htmlMarkup)
        {
            if(null == stream|| string.IsNullOrEmpty(htmlMarkup)) return;

            System.IO.MemoryStream m = new System.IO.MemoryStream();
            iText.Document document = new iText.Document();
            iText.pdf.PdfWriter writer = iText.pdf.PdfWriter.GetInstance(document, m);

            HTMLDocument pseudoDoc = new HTMLDocument();
            ((IHTMLDocument2)pseudoDoc).write(htmlMarkup);

            document.Open();

            IHTMLElementCollection children = (IHTMLElementCollection)pseudoDoc.getElementById("pdfContainer").children;

            foreach(IHTMLElement el in children)
            {
                IHTMLElement theEl = el;

                if (null == theEl.className) continue;

                string className = theEl.className.ToLowerInvariant();

                if (className == "ignore" || string.IsNullOrEmpty(className)) continue;

                string tagName = theEl.tagName.ToLowerInvariant();

                string innerText = theEl.innerText;

                if (string.IsNullOrEmpty(innerText))
                {
                    theEl = (IHTMLElement)((IHTMLElementCollection)el.children).item(null, 0);
                    tagName = theEl.tagName.ToLowerInvariant();
                    innerText = theEl.innerText;
                }

                if (tagName == "div" || tagName == "span")
                {                                        
                    document.Add(new iText.Paragraph(innerText));
                    continue;
                }

                if (tagName == "hr")
                {
                    if (className == "pagebreak")
                    {
                        document.NewPage();
                    }
                    else
                    {
                        iText.pdf.PdfPTable t = new iText.pdf.PdfPTable(1); 
                        t.HorizontalAlignment = iText.Element.ALIGN_CENTER; 
                        t.WidthPercentage = 100f; // this would be the 100 from setHorizontalLine 
                        t.SpacingAfter = 5f; 
                        t.SpacingBefore = 0f; 
                        t.DefaultCell.UseVariableBorders = true; 
                        t.DefaultCell.VerticalAlignment = iText.Element.ALIGN_MIDDLE; 
                        t.DefaultCell.HorizontalAlignment = iText.Element.ALIGN_CENTER; 
                        t.DefaultCell.Border = iText.Image.BOTTOM_BORDER; // This generates the line 
                        t.DefaultCell.BorderWidth = 1f; // this would be the 1 from setHorizontalLine 
                        t.DefaultCell.Padding = 0; 
                        t.AddCell("");
                        document.Add(t);                        
                    }
                }

                if (tagName == "img")
                {
                    iText.Image img = iText.Image.GetInstance(theEl.getAttribute("src", 0).ToString());                    
                    img.ScaleToFit(document.PageSize.Width - 100, document.PageSize.Height - 100);                    
                    document.Add(img);
                    continue;
                }
                
            }

            pseudoDoc.close();
            pseudoDoc = null;
            
            document.Close();
            
            writer.Flush();
            
            byte[] data = m.GetBuffer();

            try
            {
                stream.Write(data, 0, data.Length);
                stream.Flush();
                stream.Close();
            }
            catch
            {
                throw;
            }
            finally
            {                
                Array.Clear(data, 0, data.Length);
                data = null;
                document = null;
                writer = null;
                m.Dispose();
                m = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            
        }

        public static void CreatePDF(string filename, string htmlMarkup)
        {
            CreatePDF(new System.IO.FileStream(filename, System.IO.FileMode.Create), htmlMarkup);
        }

        /*public static void CreatePDFFromHTML(System.IO.Stream stream, string htmlMarkup)
        {
            System.Collections.Generic.List<iText.IElement> elements = iText.html.simpleparser.HTMLWorker.ParseToList(new System.IO.StringReader(htmlMarkup), null);
            iText.Document document = new iTextSharp.text.Document();
            System.IO.MemoryStream m = new System.IO.MemoryStream();
            iText.pdf.PdfWriter writer = iText.pdf.PdfWriter.GetInstance(document, m);
            elements.ForEach(el => document.Add(el));
            document.Close();
            writer.Flush();            
            byte[] data = m.GetBuffer();

            try
            {
                stream.Write(data, 0, data.Length);
                stream.Flush();
                stream.Close();
            }
            catch
            {
                throw;
            }
            finally
            {
                Array.Clear(data, 0, data.Length);
                data = null;
                document = null;
                writer = null;
                m.Dispose();
                m = null;
            }
        }*/

        public static void CreateRemappedRasterFromVector(string xamlFile, System.Windows.Media.Color newColor, System.IO.Stream stream, double height, double width)
        {
            CreateRemappedRasterFromVector(xamlFile, newColor, stream, height, width, null);
            return;
        }

        public static void CreateRemappedRasterFromVector(string xamlFile, System.Windows.Media.Color newColor, System.IO.Stream stream, double height, double width, string dynamicText)
        {
            string remapBase = "ReMapMe_";            
            int max = 3;
            int index = 0;
            int missed = 0;
            Object temp = null;
            string dynamText = "DynamicText";

            System.Windows.Controls.Canvas theVisual = System.Windows.Markup.XamlReader.Load(new System.IO.FileStream(xamlFile, System.IO.FileMode.Open)) as System.Windows.Controls.Canvas;

            if (null == theVisual) return;

            System.Windows.Size size = new System.Windows.Size(width, height);

            System.Windows.Media.DrawingVisual drawingVisual = new System.Windows.Media.DrawingVisual();

            System.Windows.Media.VisualBrush visualBrush = new System.Windows.Media.VisualBrush(theVisual);

            System.Windows.Rect rect = new System.Windows.Rect(new System.Windows.Point(), size);

            while(missed < max)
            {
                string elementid = remapBase + index;

                 temp = theVisual.FindName(elementid);

                if (null == temp)
                {
                    missed++;
                    index++;
                    continue;
                }

                System.Windows.Media.SolidColorBrush scb = ((temp as System.Windows.Shapes.Path).Fill as System.Windows.Media.SolidColorBrush);

                System.Windows.Media.Color c = scb.Color;
                c.B = newColor.B;
                c.R = newColor.R;
                c.G = newColor.G;

                scb.Color = c;
                index++;
            }
                        
            theVisual.Arrange(rect);
            theVisual.UpdateLayout();

            using (System.Windows.Media.DrawingContext dc = drawingVisual.RenderOpen())
            {
                dc.DrawRectangle(visualBrush, null, rect);
            }

            if (!dynamText.IsNullOrWhitespace())
            {
                temp = theVisual.FindName(dynamText);
                if (null != temp)
                {
                    //do dynamic text shit
                }
            }

            System.Windows.Media.Imaging.RenderTargetBitmap render = new System.Windows.Media.Imaging.RenderTargetBitmap((int)height, (int)width, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            render.Render(drawingVisual);            

            System.Windows.Media.Imaging.PngBitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();

            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(render));

            encoder.Save(stream);

            stream.Flush();
            stream.Close();
            
            visualBrush = null;
            drawingVisual = null;
            theVisual = null;
            render = null;            
            encoder = null;
        }

    }    
}
