using ImageResizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace RealEstate.Helpers
{
    public static class HtmlHelpers
    {
        //#region CHECK IP
        //// Check IPs
        //public static bool IsAllowedIP(this string userIpAddress)
        //{
        //    var IPAttribute = new FilterIPAttribute();
        //    IPAttribute.ConfigurationKeyAllowedSingleIPs = "AllowedAdminSingleIPs";
        //    IPAttribute.ConfigurationKeyAllowedMaskedIPs = "AllowedAdminMaskedIPs";
        //    IPAttribute.ConfigurationKeyDeniedSingleIPs = "DeniedAdminSingleIPs";
        //    IPAttribute.ConfigurationKeyDeniedMaskedIPs = "DeniedAdminMaskedIPs";

        //    // Check that the IP is allowed to access
        //    bool ipAllowed = IPAttribute.CheckAllowedIPs(userIpAddress);

        //    // Check that the IP is not denied to access
        //    bool ipDenied = IPAttribute.CheckDeniedIPs(userIpAddress);

        //    // Only allowed if allowed and not denied
        //    bool finallyAllowed = ipAllowed && !ipDenied;

        //    return finallyAllowed;
        //}
        //public static bool IsDeniedIP(this string userIpAddress)
        //{
        //    return !userIpAddress.IsAllowedIP();
        //}
        //#endregion
    }

    public static class FileExtension
    {
        const int maxImgSize = 500000; // 500KB
        const int maxImgWidth = 2048; // 2048px
        const int maxImgWidthAvatar = 250; // 2048px

        public static string SaveAsUserAvatar(this HttpPostedFileBase fileBase, int contentItemId)
        {
            string fileLocation = "/Media/Default/Avatars/" + contentItemId + "-" + DateTime.Now.Ticks + Path.GetExtension(fileBase.FileName ?? "").ToLower();

            var uploadFolder = Path.GetDirectoryName(fileLocation);
            var folder = new DirectoryInfo(HttpContext.Current.Server.MapPath(uploadFolder));
            if (!folder.Exists) folder.Create();

            // resize image if needed
            if (fileBase.ContentLength < maxImgSize) // file nhỏ hơn maxImgSize không cần resize
            {
                fileBase.SaveAs(HttpContext.Current.Server.MapPath(fileLocation));
            }
            else
            {
                var i = new ImageJob(fileBase, "~" + fileLocation, new Instructions("maxwidth=" + maxImgWidthAvatar + ";format=jpg;mode=max"));
                i.CreateParentDirectory = true; //Auto-create the uploads directory.
                i.Build();
            }
            return fileLocation;
        }

        public static string SaveAsUserFiles(this HttpPostedFileBase fileBase, int contentItemId)
        {
            // TODO: use folder name from settings
            //string uploadsFolder = HttpContext.Current.Server.MapPath("/UserFiles/" + contentItemId);

            //var folder = new DirectoryInfo(uploadsFolder);
            //if (!folder.Exists) folder.Create();

            string fileLocation = "/UserFiles/" + contentItemId + "/" + Guid.NewGuid() + Path.GetExtension(fileBase.FileName ?? "").ToLower();

            var uploadFolder = Path.GetDirectoryName(fileLocation);
            var folder = new DirectoryInfo(HttpContext.Current.Server.MapPath(uploadFolder));
            if (!folder.Exists) folder.Create();

            // resize image if needed
            if (fileBase.ContentLength < maxImgSize) // file nhỏ hơn maxImgSize không cần resize
            {
                fileBase.SaveAs(HttpContext.Current.Server.MapPath(fileLocation));
            }
            else
            {
                var i = new ImageJob(fileBase, "~" + fileLocation, new Instructions("maxwidth=" + maxImgWidth + ";;format=jpg;mode=max"));
                i.CreateParentDirectory = true; //Auto-create the uploads directory.
                i.Build();
            }
            return fileLocation;
        }

        public static string SaveAsFileLocation(this HttpPostedFileBase fileBase, string fileLocation)
        {
            var uploadFolder = Path.GetDirectoryName(fileLocation);
            var folder = new DirectoryInfo(HttpContext.Current.Server.MapPath(uploadFolder));
            if (!folder.Exists) folder.Create();

            // resize image if needed
            if (fileBase.ContentLength < maxImgSize) // file nhỏ hơn maxImgSize không cần resize
            {
                fileBase.SaveAs(HttpContext.Current.Server.MapPath(fileLocation));
            }
            else
            {
                var i = new ImageJob(fileBase, "~" + fileLocation, new Instructions("maxwidth=" + maxImgWidth + ";format=jpg;mode=max"));
                i.CreateParentDirectory = true; //Auto-create the uploads directory.
                i.Build();
            }
            return fileLocation;
        }
    }

    public static class DateExtension
    {
        public static string TimeAgo(this DateTime t)
        {
            TimeSpan timeSince = DateTime.Now.Subtract(t);

            if (timeSince.TotalMilliseconds < 1)
                return "chưa có";
            if (timeSince.TotalSeconds < 60)
                return string.Format("{0} giây trước", timeSince.Seconds);
            if (timeSince.TotalMinutes < 60)
                return string.Format("{0} phút trước", timeSince.Minutes);
            if (timeSince.TotalMinutes < 120)
                return "1 giờ trước";
            if (timeSince.TotalHours < 24)
                return string.Format("{0} giờ trước", timeSince.Hours);
            if (timeSince.TotalDays >= 1 && timeSince.TotalDays < 2)
                return "hôm qua";
            if (timeSince.TotalDays < 7)
                return string.Format("{0} ngày trước", timeSince.Days);
            if (timeSince.TotalDays < 14)
                return "1 tuần trước";
            if (timeSince.TotalDays < 21)
                return "2 tuần trước";
            if (timeSince.TotalDays < 28)
                return "3 tuần trước";
            if (timeSince.TotalDays < 60)
                return "1 tháng trước";
            if (timeSince.TotalDays < 365)
                return string.Format("{0} tháng trước", Math.Round(timeSince.TotalDays / 30));
            if (timeSince.TotalDays < 730)
                return "1 năm trước";
            //last but not least...
            return string.Format("{0} năm trước", Math.Round(timeSince.TotalDays / 365));
        }

        public static string DateTimeFormat(this HtmlHelper helper, DateTime date)
        {
            return
                string.Format(ConfigurationManager.AppSettings["DateTimeFormat"], date)
                    .Replace("SA", "sáng")
                    .Replace("CH", "chiều");
        }

        #region Today

        public static DateTime GetStartOfToday()
        {
            DateTime dt = DateTime.Today;
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0);
        }

        public static DateTime GetEndOfToday()
        {
            DateTime dt = DateTime.Today;
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999);
        }

        #endregion

        #region Date

        public static DateTime GetStartOfDate(DateTime d)
        {
            return new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, 0);
        }

        public static DateTime GetEndOfDate(DateTime d)
        {
            return new DateTime(d.Year, d.Month, d.Day, 23, 59, 59, 999);
        }

        #endregion

        #region Weeks

        public static DateTime GetStartOfLastWeek()
        {
            //int DaysToSubtract = (int)DateTime.Now.DayOfWeek + 7;
            //DateTime dt = DateTime.Now.Subtract(TimeSpan.FromDays(DaysToSubtract));
            DateTime dt = GetStartOfCurrentWeek().AddDays(-7);
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0);
        }

        public static DateTime GetEndOfLastWeek()
        {
            DateTime dt = GetStartOfCurrentWeek().AddDays(-1);
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999);
        }

        public static DateTime GetStartOfCurrentWeek()
        {
            var daysToSubtract = (int)DateTime.Now.DayOfWeek;
            DateTime dt = DateTime.Now.Subtract(TimeSpan.FromDays(daysToSubtract));
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0);
        }

        public static DateTime GetEndOfCurrentWeek()
        {
            DateTime dt = GetStartOfCurrentWeek().AddDays(6);
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999);
        }

        #endregion

        #region Months

        public static DateTime GetStartOfLastMonth()
        {
            DateTime dt = GetStartOfCurrentMonth().AddMonths(-1);
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0);
        }

        public static DateTime GetEndOfLastMonth()
        {
            DateTime dt = GetStartOfCurrentMonth().AddDays(-1);
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999);
        }

        public static DateTime GetStartOfCurrentMonth()
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0, 0);
        }

        public static DateTime GetEndOfCurrentMonth()
        {
            DateTime dt = GetStartOfCurrentMonth().AddMonths(1).AddDays(-1);
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999);
        }

        public static DateTime GetStartOfMonth(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, 0);
        }

        public static DateTime GetEndOfMonth(DateTime dt)
        {
            DateTime d = GetStartOfMonth(dt).AddMonths(1).AddDays(-1);
            return new DateTime(d.Year, d.Month, d.Day, 23, 59, 59, 999);
        }

        #endregion
    }

    public static class StringExtension
    {
        private const string HtmlTagPattern = "<.*?>";

        public static string StripHtml(this string source)
        {
            return Regex.Replace(source, HtmlTagPattern, string.Empty);
        }

        public static string Truncate(this string source, int length)
        {
            try
            {
                if (source.Length <= length)
                {
                    return source;
                }
                return source.Substring(0, length) + "...";
            }
            catch
            {
                return source;
            }
        }

        public static string ToSlug(this string source)
        {
            return ToSlug(source, int.MaxValue);
        }

        public static string ToSlug(this string source, int maxLength)
        {
            string str = RemoveSign4VietnameseString(source.ToLower());

            // invalid chars, make into spaces
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces/hyphens into one space       
            str = Regex.Replace(str, @"[\s-]+", " ").Trim();
            // cut and trim it
            str = str.Substring(0, str.Length <= maxLength ? str.Length : maxLength).Trim();
            // hyphens
            str = Regex.Replace(str, @"\s", "-");

            return str;
        }

        public static string RemoveSign4VietnameseString(this string source)
        {
            string[] vietnameseSigns =
            {
                "aAeEoOuUiIdDyY", "áàạảãâấầậẩẫăắằặẳẵ", "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ", "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ", "óòọỏõôốồộổỗơớờợởỡ", "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ", "úùụủũưứừựửữ", "ÚÙỤỦŨƯỨỪỰỬỮ", "íìịỉĩ", "ÍÌỊỈĨ",
                "đ", "Đ", "ýỳỵỷỹ", "ÝỲỴỶỸ"
            };

            //Tiến hành thay thế , lọc bỏ dấu cho chuỗi
            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                    source = source.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
            }

            return source;
        }

        public static int CountChar(this string source, char countChar)
        {
            int count = 0;
            try
            {
                count += source.Count(t => t == countChar);
            }
            catch (Exception)
            {
                count = 0;
            }
            return count;
        }

        public static int CountChar(this string source, char countChar, char stopChar)
        {
            int count = 0;
            try
            {
                count += source.TakeWhile(t => t != stopChar).Count(t => t == countChar);
            }
            catch (Exception)
            {
                count = 0;
            }
            return count;
        }

        public static string HtmlLinkAddRedirectAndNofollow(this string source)
        {
            return Regex.Replace(source, "<a[^>]+href=\"?'?(?!#[\\w-]+)([^'\">]+)\"?'?[^>]*>(.*?)</a>",
                "<a href=\"/redirect?url=$1\" rel=\"nofollow\">$2</a>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public static string GetImagesSrc(this string source)
        {
            return Regex.Match(source, "<img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase).Groups[1].Value;
        }
    }

    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>()
        {
            return f => true;
        }

        public static Expression<Func<T, bool>> False<T>()
        {
            return f => false;
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            InvocationExpression invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, bool>>
                (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            InvocationExpression invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, bool>>
                (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }

    public static class SitemapExtension
    {
        public static XDocument GenerateSitemap(List<SitemapUrl> items)
        {
            const string url = "http://dinhgianhadat.vn/{0}";
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var sitemap = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(ns + "urlset",
                    from i in items
                    select
                        //Add ns to every element.
                        new XElement(ns + "url",
                            new XElement(ns + "loc", string.Format(url, i.loc)),
                            new XElement(ns + "lastmod", i.lastmod),
                            new XElement(ns + "changefreq", i.changefreq),
                            new XElement(ns + "priority", i.priority)
                            )
                    )
                );
            return sitemap;
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupAdjacent<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            TKey last = default(TKey);
            bool haveLast = false;
            var list = new List<TSource>();

            foreach (TSource s in source)
            {
                TKey k = keySelector(s);
                if (haveLast)
                {
                    if (!k.Equals(last))
                    {
                        yield return new GroupOfAdjacent<TSource, TKey>(list, last);
                        list = new List<TSource> { s };
                        last = k;
                    }
                    else
                    {
                        list.Add(s);
                        last = k;
                    }
                }
                else
                {
                    list.Add(s);
                    last = k;
                    haveLast = true;
                }
            }
            if (haveLast)
                yield return new GroupOfAdjacent<TSource, TKey>(list, last);
        }

        public static void WriteStartElement(XmlWriter writer, XElement e)
        {
            XNamespace ns = e.Name.Namespace;
            writer.WriteStartElement(e.GetPrefixOfNamespace(ns),
                e.Name.LocalName, ns.NamespaceName);
            foreach (XAttribute a in e.Attributes())
            {
                ns = a.Name.Namespace;
                string localName = a.Name.LocalName;
                string namespaceName = ns.NamespaceName;
                writer.WriteAttributeString(
                    e.GetPrefixOfNamespace(ns),
                    localName,
                    namespaceName.Length == 0 && localName == "xmlns"
                        ? XNamespace.Xmlns.NamespaceName
                        : namespaceName,
                    a.Value);
            }
        }

        public static void WriteElement(XmlWriter writer, XElement e)
        {
            if (e.Name == "urlset")
            {
                WriteStartElement(writer, e);
                writer.WriteRaw(Environment.NewLine);

                // Create an XML writer that outputs no insignificant white space so that we can
                // write to it and explicitly control white space.
                var settings = new XmlWriterSettings
                {
                    Indent = false,
                    OmitXmlDeclaration = true,
                    ConformanceLevel = ConformanceLevel.Fragment
                };
                var sb = new StringBuilder();
                using (XmlWriter newXmlWriter = XmlWriter.Create(sb, settings))
                {
                    // Group adjacent runs so that they can be output with no whitespace between them
                    IEnumerable<IGrouping<bool, XNode>> groupedRuns = e.Nodes().GroupAdjacent(n =>
                    {
                        var element = n as XElement;
                        if (element != null && element.Name == "url")
                            return true;
                        return false;
                    });
                    foreach (var g in groupedRuns)
                    {
                        if (g.Key)
                        {
                            // Write white space so that the line of Run elements is properly indented.
                            //newXmlWriter.WriteRaw("".PadRight((e.Ancestors().Count() + 1) * 2));
                            foreach (XNode run in g)
                                run.WriteTo(newXmlWriter);
                            //newXmlWriter.WriteRaw(Environment.NewLine);
                        }
                        else
                        {
                            foreach (XNode g2 in g)
                            {
                                // Write some white space so that each child element is properly indented.
                                //newXmlWriter.WriteRaw("".PadRight((e.Ancestors().Count() + 1) * 2));
                                g2.WriteTo(newXmlWriter);
                                //newXmlWriter.WriteRaw(Environment.NewLine);
                            }
                        }
                    }
                }
                writer.WriteRaw(sb.ToString());
                writer.WriteRaw("".PadRight(e.Ancestors().Count() * 2));
                writer.WriteEndElement();
            }
            else
            {
                WriteStartElement(writer, e);
                foreach (XNode n in e.Nodes())
                {
                    var element = n as XElement;
                    if (element != null)
                    {
                        WriteElement(writer, element);
                        continue;
                    }
                    n.WriteTo(writer);
                }
                writer.WriteEndElement();
            }
        }

        public static string ToStringWithCustomWhiteSpace(XElement element)
        {
            // Create XmlWriter that indents.
            var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var sb = new StringBuilder();
            using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
                WriteElement(xmlWriter, element);
            return sb.ToString();
        }

        public class SitemapUrl
        {
            // ReSharper disable InconsistentNaming
            public string loc { get; set; }
            public string lastmod { get; set; }
            public string changefreq { get; set; }
            public string priority { get; set; }
            // ReSharper restore InconsistentNaming
        }
    }

    public class GroupOfAdjacent<TSource, TKey> : IGrouping<TKey, TSource>
    {
        public GroupOfAdjacent(List<TSource> source, TKey key)
        {
            GroupList = source;
            Key = key;
        }

        private List<TSource> GroupList { get; set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TSource>)this).GetEnumerator();
        }

        IEnumerator<TSource>
            IEnumerable<TSource>.GetEnumerator()
        {
            return ((IEnumerable<TSource>)GroupList).GetEnumerator();
        }

        public TKey Key { get; set; }
    }
}