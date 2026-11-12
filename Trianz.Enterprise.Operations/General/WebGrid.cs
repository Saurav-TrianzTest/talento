namespace System.Web.Helpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Html;

    public enum WebGridPagerModes
    {
        None = 0,
        First = 1,
        PrevPage = 2,
        NextPage = 4,
        Last = 8,
        All = First | PrevPage | NextPage | Last,
        Numeric = 16,
        NextPrev = PrevPage | NextPage
    }

    public class WebGridColumn
    {
        public string ColumnName { get; set; }
        public string Header { get; set; }
        public string HeaderText { get; set; }
        public Func<dynamic, object> Format { get; set; }
        public bool CanSort { get; set; }
        public string Style { get; set; }

        public WebGridColumn(string columnName = null, string headerText = null, Func<dynamic, object> format = null, bool canSort = true, string style = null)
        {
            ColumnName = columnName;
            Header = headerText ?? columnName;
            HeaderText = headerText ?? columnName;
            Format = format;
            CanSort = canSort;
            Style = style;
        }
    }

    public class WebGrid
    {
        private readonly IEnumerable _source;
        private readonly bool _canPage;
        private readonly bool _canSort;
        private readonly int _rowsPerPage;
        private readonly string _selectionFieldName;
        private readonly string _ajaxUpdateContainerId;
        private readonly string _defaultSort;
        private int _pageIndex = 0;

        public WebGrid(
            IEnumerable source,
            bool canPage = true,
            bool canSort = true,
            int rowsPerPage = 10,
            string selectionFieldName = null,
            string ajaxUpdateContainerId = null,
            string defaultSort = null)
        {
            _source = source ?? new List<object>();
            _canPage = canPage;
            _canSort = canSort;
            _rowsPerPage = rowsPerPage;
            _selectionFieldName = selectionFieldName;
            _ajaxUpdateContainerId = ajaxUpdateContainerId;
            _defaultSort = defaultSort;
        }

        public int RowCount => GetRows().Count();

        public int TotalRowCount => GetRows().Count();

        public bool HasSelection => false;

        public bool CanPage => _canPage;

        public bool CanSort => _canSort;

        public WebGridColumn Column(
            string columnName = null,
            string headerText = null,
            string header = null,
            Func<dynamic, object> format = null,
            bool canSort = true,
            string style = null)
        {
            return new WebGridColumn(columnName, header ?? headerText ?? columnName, format, canSort, style);
        }

        public IEnumerable<WebGridColumn> Columns(params WebGridColumn[] columns)
        {
            return columns ?? new WebGridColumn[0];
        }

        public IEnumerable<WebGridColumn> Columns(IEnumerable<WebGridColumn> columns)
        {
            return columns ?? new List<WebGridColumn>();
        }

        public void Pager(WebGridPagerModes mode)
        {
            // No-op for compatibility
        }

        public HtmlString GetHtml(
            string tableStyle = null,
            string headerStyle = null,
            string footerStyle = null,
            string rowStyle = null,
            string alternatingRowStyle = null,
            string selectedRowStyle = null,
            bool fillEmptyRows = false,
            bool displayHeader = true,
            WebGridPagerModes mode = WebGridPagerModes.None,
            string firstText = null,
            string firstRouteName = null,
            string firstUrl = null,
            string previousText = null,
            string prevRouteName = null,
            string prevUrl = null,
            string nextText = null,
            string nextRouteName = null,
            string nextUrl = null,
            string lastText = null,
            string lastRouteName = null,
            string lastUrl = null,
            object htmlAttributes = null,
            IEnumerable<WebGridColumn> columns = null,
            string prevText = null)
        {
            var sb = new StringBuilder();
            var rows = GetPagedRows();

            sb.Append("<table");
            if (!string.IsNullOrEmpty(tableStyle))
                sb.Append(" class=\"").Append(EscapeHtml(tableStyle)).Append("\"");
            AppendHtmlAttributes(sb, htmlAttributes);
            sb.Append(">");

            // Header
            sb.Append("<thead><tr>");
            foreach (var column in columns ?? new List<WebGridColumn>())
            {
                sb.Append("<th");
                if (!string.IsNullOrEmpty(headerStyle))
                    sb.Append(" class=\"").Append(EscapeHtml(headerStyle)).Append("\"");
                sb.Append(">").Append(EscapeHtml(column.Header ?? column.ColumnName ?? string.Empty)).Append("</th>");
            }
            sb.Append("</tr></thead>");

            // Body
            sb.Append("<tbody>");
            int rowIndex = 0;
            foreach (var row in rows)
            {
                sb.Append("<tr");
                var rowStyleValue = rowIndex % 2 == 0 ? rowStyle : alternatingRowStyle;
                if (!string.IsNullOrEmpty(rowStyleValue))
                    sb.Append(" class=\"").Append(EscapeHtml(rowStyleValue)).Append("\"");
                sb.Append(">");

                foreach (var column in columns ?? new List<WebGridColumn>())
                {
                    sb.Append("<td");
                    if (!string.IsNullOrEmpty(column.Style))
                        sb.Append(" class=\"").Append(EscapeHtml(column.Style)).Append("\"");
                    sb.Append(">");

                    if (column.Format != null)
                    {
                        var formatted = column.Format(row);
                        if (formatted is HtmlString htmlString)
                        {
                            sb.Append(htmlString.Value);
                        }
                        else
                        {
                            sb.Append(EscapeHtml(formatted?.ToString() ?? string.Empty));
                        }
                    }
                    else
                    {
                        var val = GetPropertyValue(row, column.ColumnName);
                        sb.Append(EscapeHtml(val?.ToString() ?? string.Empty));
                    }

                    sb.Append("</td>");
                }

                sb.Append("</tr>");
                rowIndex++;
            }

            // Fill empty rows if needed
            if (fillEmptyRows && _canPage)
            {
                var emptyRows = _rowsPerPage - rows.Count;
                for (int i = 0; i < emptyRows; i++)
                {
                    sb.Append("<tr");
                    if (!string.IsNullOrEmpty(alternatingRowStyle))
                        sb.Append(" class=\"").Append(alternatingRowStyle).Append("\"");
                    sb.Append(">");
                    foreach (var column in columns ?? new List<WebGridColumn>())
                    {
                        sb.Append("<td>&nbsp;</td>");
                    }
                    sb.Append("</tr>");
                }
            }

            sb.Append("</tbody>");

            // Pager
            if (_canPage && mode != WebGridPagerModes.None)
            {
                sb.Append("<tfoot><tr><td colspan=\"").Append(columns?.Count() ?? 1).Append("\">");
                sb.Append(GetPagerHtml(mode, firstText, prevText, nextText, lastText));
                sb.Append("</td></tr></tfoot>");
            }

            sb.Append("</table>");

            return new HtmlString(sb.ToString());
        }

        private string GetPagerHtml(WebGridPagerModes mode, string firstText, string prevText, string nextText, string lastText)
        {
            var sb = new StringBuilder();
            var totalRows = GetRows().Count;
            var totalPages = _canPage ? (int)Math.Ceiling((double)totalRows / _rowsPerPage) : 1;
            var currentPageIndex = _pageIndex + 1;

            if ((mode & WebGridPagerModes.First) == WebGridPagerModes.First)
            {
                sb.Append("<a href=\"#\" class=\"first-page\">").Append(firstText ?? "<< First").Append("</a> ");
            }
            if ((mode & WebGridPagerModes.PrevPage) == WebGridPagerModes.PrevPage)
            {
                sb.Append("<a href=\"#\" class=\"prev-page\">").Append(prevText ?? "< Prev").Append("</a> ");
            }

            sb.Append("<span class=\"page-info\">Page " + currentPageIndex + " of " + totalPages + "</span> ");

            if ((mode & WebGridPagerModes.NextPage) == WebGridPagerModes.NextPage)
            {
                sb.Append("<a href=\"#\" class=\"next-page\">").Append(nextText ?? "Next >").Append("</a> ");
            }
            if ((mode & WebGridPagerModes.Last) == WebGridPagerModes.Last)
            {
                sb.Append("<a href=\"#\" class=\"last-page\">").Append(lastText ?? "Last >>").Append("</a>");
            }

            return sb.ToString();
        }

        private void AppendHtmlAttributes(StringBuilder sb, object htmlAttributes)
        {
            if (htmlAttributes == null) return;

            var props = htmlAttributes.GetType().GetProperties();
            foreach (var prop in props)
            {
                var name = prop.Name;
                if (name.StartsWith("_"))
                    name = name.Substring(1);
                name = name.Replace("_", "-");
                sb.Append(" ").Append(name).Append("=\"").Append(EscapeHtml(prop.GetValue(htmlAttributes)?.ToString() ?? "")).Append("\"");
            }
        }

        private object GetPropertyValue(dynamic row, string propertyName)
        {
            if (row is DynamicWrapper wrapper)
            {
                return wrapper.GetProperty(propertyName);
            }
            // If row is already dynamic (e.g., ExpandoObject or anonymous type cast to dynamic)
            try
            {
                var prop = row?.GetType().GetProperty(propertyName);
                if (prop != null)
                    return prop.GetValue(row);
            }
            catch
            {
            }
            return null;
        }

        private string EscapeHtml(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }

        private IList<dynamic> GetRows()
        {
            var rows = new List<dynamic>();
            foreach (var item in _source)
            {
                rows.Add(new DynamicWrapper(item));
            }
            return rows;
        }

        private IList<dynamic> GetPagedRows()
        {
            var allRows = GetRows();
            if (!_canPage)
            {
                return allRows;
            }

            var startIndex = _pageIndex * _rowsPerPage;
            return allRows.Skip(startIndex).Take(_rowsPerPage).ToList();
        }
    }

    public class DynamicWrapper : DynamicObject
    {
        private readonly object _wrapped;

        public DynamicWrapper(object wrapped)
        {
            _wrapped = wrapped;
        }

        public object GetProperty(string propertyName)
        {
            if (_wrapped == null) return null;

            var prop = _wrapped.GetType().GetProperty(propertyName);
            if (prop != null)
            {
                return prop.GetValue(_wrapped);
            }

            // Try fields
            var field = _wrapped.GetType().GetField(propertyName);
            if (field != null)
            {
                return field.GetValue(_wrapped);
            }

            return null;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = GetProperty(binder.Name);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_wrapped == null)
            {
                return false;
            }

            var prop = _wrapped.GetType().GetProperty(binder.Name);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(_wrapped, value);
                return true;
            }

            return false;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (_wrapped == null)
            {
                result = null;
                return false;
            }

            try
            {
                result = _wrapped.GetType().GetMethod(binder.Name)?.Invoke(_wrapped, args);
                return result != null;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public override string ToString()
        {
            return _wrapped?.ToString() ?? string.Empty;
        }
    }
}
