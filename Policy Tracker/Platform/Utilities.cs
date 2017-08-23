﻿using PolicyTracker.DomainModel.Framework;
using System;
using System.Linq;
using System.Reflection;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System.IO;

namespace PolicyTracker.Platform.Utilities
{
    public static class ObjectUtils
    {
        public static object GetNestedPropertyValue(object obj, string property)
        {
            var propertyNames = property.Split('.');

            foreach (var p in propertyNames)
            {
                Type type = obj.GetType();
                PropertyInfo info = type.GetProperty(p);

                if (info != null) obj = info.GetValue(obj, null);
            }

            return obj;
        }

        /// <summary>
        /// If the nested Property of the object is null this will fail. Not sure how to best handle
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static PropertyInfo GetNestedProperty(object obj, string property)
        {
            var propertyNames = property.Split('.');
            PropertyInfo info = null;

            foreach (var p in propertyNames)
            {
                Type type = obj.GetType();
                info = type.GetProperty(p);

                if (info != null) obj = info.GetValue(obj, null);
            }

            return info;
        }

        public static void SetProperty(string compoundProperty, object target, object value)
        {
            string[] bits = compoundProperty.Split('.');
            for (int i = 0; i < bits.Length - 1; i++)
            {
                PropertyInfo propertyToGet = target.GetType().GetProperty(bits[i]);
                target = propertyToGet.GetValue(target, null);
            }
            PropertyInfo propertyToSet = target.GetType().GetProperty(bits.Last());
            propertyToSet.SetValue(target, value, null);
        }

        public static PropertyFilter GetPropertyFilter(string propName, Type dataType, string operand, object value, object value2 = null)
        {
            PropertyFilter.Comparator op = (PropertyFilter.Comparator)Enum.Parse(typeof(PropertyFilter.Comparator), operand);
            PropertyFilter filter = null;

            if (dataType == typeof(Int32))
            {
                filter = new PropertyFilter(propName, op, Convert.ToInt32(value));
            }
            else if (dataType == typeof(Int64))
            {
                filter = new PropertyFilter(propName, op, Convert.ToInt64(value));
            }
            else if (dataType == typeof(DateTime))
            {
                if (value2 != null)
                    filter = new PropertyFilter(propName, op, Convert.ToDateTime(value), Convert.ToDateTime(value2));
                else
                    filter = new PropertyFilter(propName, op, Convert.ToDateTime(value));
            }
            else if (dataType == typeof(Boolean))
            {
                filter = new PropertyFilter(propName, Convert.ToBoolean(value));
            }
            else
            {
                filter = new PropertyFilter(propName, op, value);
            }
            return filter;
        }
    }

    public static class PDFRenderer
    {
        private const int HorizontalMargin = 40;
        private const int VerticalMargin = 40;

        public static byte[] Render(string htmlText, string pageTitle, bool isLanscape)
        {
            byte[] renderedBuffer;
            var pageSize = PageSize.A4;
            if (isLanscape)
                pageSize = PageSize.A4.Rotate();

            using (var outputMemoryStream = new MemoryStream())
            {
                using (var pdfDocument = new Document(pageSize, HorizontalMargin, HorizontalMargin, VerticalMargin, VerticalMargin))
                {
                    PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDocument, outputMemoryStream);
                    pdfWriter.CloseStream = false;
                    pdfWriter.PageEvent = new PrintHeaderFooter { Title = pageTitle };
                    pdfDocument.Open();
                    using (var htmlViewReader = new StringReader(htmlText))
                    {
                        using (var htmlWorker = new HTMLWorker(pdfDocument))
                        {
                            htmlWorker.Parse(htmlViewReader);
                        }
                    }
                }

                renderedBuffer = new byte[outputMemoryStream.Position];
                outputMemoryStream.Position = 0;
                outputMemoryStream.Read(renderedBuffer, 0, renderedBuffer.Length);
            }

            return renderedBuffer;
        }
    }

    public class PrintHeaderFooter : PdfPageEventHelper
    {
        private PdfContentByte pdfContent;
        private PdfTemplate pageNumberTemplate;
        private BaseFont baseFont;
        private DateTime printTime;

        public string Title { get; set; }

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            printTime = DateTime.Now;
            baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            pdfContent = writer.DirectContent;
            pageNumberTemplate = pdfContent.CreateTemplate(50, 50);
        }

        public override void OnStartPage(PdfWriter writer, Document document)
        {
            base.OnStartPage(writer, document);

            Rectangle pageSize = document.PageSize;

            if (Title != string.Empty)
            {
                pdfContent.BeginText();
                pdfContent.SetFontAndSize(baseFont, 11);
                pdfContent.SetRGBColorFill(0, 0, 0);
                pdfContent.SetTextMatrix(pageSize.GetLeft(40), pageSize.GetTop(40));
                pdfContent.ShowText(Title);
                pdfContent.EndText();
            }
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);

            int pageN = writer.PageNumber;
            string text = pageN + " - ";
            float len = baseFont.GetWidthPoint(text, 8);

            Rectangle pageSize = document.PageSize;
            pdfContent = writer.DirectContent;
            pdfContent.SetRGBColorFill(100, 100, 100);

            pdfContent.BeginText();
            pdfContent.SetFontAndSize(baseFont, 8);
            pdfContent.SetTextMatrix(pageSize.Width / 2, pageSize.GetBottom(30));
            pdfContent.ShowText(text);
            pdfContent.EndText();

            pdfContent.AddTemplate(pageNumberTemplate, (pageSize.Width / 2) + len, pageSize.GetBottom(30));

            pdfContent.BeginText();
            pdfContent.SetFontAndSize(baseFont, 8);
            pdfContent.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, printTime.ToString(), pageSize.GetRight(40), pageSize.GetBottom(30), 0);
            pdfContent.EndText();
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            pageNumberTemplate.BeginText();
            pageNumberTemplate.SetFontAndSize(baseFont, 8);
            pageNumberTemplate.SetTextMatrix(0, 0);
            pageNumberTemplate.ShowText(string.Empty + (writer.PageNumber - 1));
            pageNumberTemplate.EndText();
        }
    }
}
