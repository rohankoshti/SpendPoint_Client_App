using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SpendPoint
{
    static class Helper
    {
        private static List<string> clientNameWordsToRemove =
        "RE: Count:,RE: Count -,RE: Count-,RE: Count,Count -,Count-,Count :,Count:,Count,RE: Order:,RE: Order -,RE: Order-,RE: Order,Order :,Order -,Order-,Order:,Order,RE :,RE:,RE -,RE-".Split(',').ToList();

        public static IEnumerable<string> GetLines(this string str, bool removeEmptyLines = true)
        {
            return str.Split(new[] { "\r\n", "\r", "\n" },
                removeEmptyLines ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
        }

        public static string RemoveSpecialChars(this string input, string newString = "")
        {
            return Regex.Replace(input, @"[^0-9a-zA-Z ,]+", newString);
        }

        public static string RemoveCommas(this string input)
        {
            return input.Replace(",", "");
        }

        public static string GetOutputFileName(OutputFileType outputFileType, string clientName, string jobNumber)
        {
            string response = string.Empty;
            string extension = ".xlsx";
            switch (outputFileType)
            {
                case OutputFileType.Order:
                    response = "Order - " + clientName + " - Job " + jobNumber + extension;
                    break;
                case OutputFileType.Quote:
                    response = "Household Count - " + clientName + extension;
                    break;
                default:
                    break;
            }
            return response;
        }

        public static string GetEmailSubject(OutputFileType outputFileType, string clientName, string jobNumber, GeneratingEmailType generatingEmailType)
        {
            string response = string.Empty;
            if (generatingEmailType == GeneratingEmailType.ReplyToExistingEmail)
            {
                switch (outputFileType)
                {
                    case OutputFileType.Order:
                        response = "RE: Order - " + clientName + " - Job " + jobNumber;
                        break;
                    case OutputFileType.Quote:
                        response = "RE: Count – " + clientName;
                        break;
                    default:
                        break;
                }
            }
            else if (generatingEmailType == GeneratingEmailType.NewEmail)
            {
                switch (outputFileType)
                {
                    case OutputFileType.Order:
                        response = "Order: " + clientName + " - Job No. " + jobNumber;
                        break;
                    case OutputFileType.Quote:
                        response = "Count: " + clientName;
                        break;
                    default:
                        break;
                }
            }
            return response;
        }

        public static string AddDoubleQuotes(this string value)
        {
            return "\"" + value + "\"";
        }

        public static string AddDoubleQuotesIfCommaPresentInside(this string value)
        {
            if (value.Contains(","))
                return "\"" + value + "\"";
            return value;
        }

        public static Tuple<int, int> GetColumnSize(string path, char delimiter)
        {
            var lines = File.ReadAllLines(path);
            var csvDataCount = Tuple.Create(lines.Length, lines[0].Split(delimiter).Length);
            return csvDataCount;
        }

        public static Func<T, T> CreateNewStatement<T>(string fields)
        {
            // input parameter "o"
            var xParameter = Expression.Parameter(typeof(T), "o");

            // new statement "new Data()"
            var xNew = Expression.New(typeof(T));

            // create initializers
            var bindings = fields.Split(',').Select(o => o.Trim())
                .Select(o =>
                {
                    // property "Field1"
                    var mi = typeof(T).GetProperty(o);

                    // original value "o.Field1"
                    var xOriginal = Expression.Property(xParameter, mi);

                    // set value "Field1 = o.Field1"
                    return Expression.Bind(mi, xOriginal);
                }
            );

            // initialization "new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            var xInit = Expression.MemberInit(xNew, bindings);

            // expression "o => new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            var lambda = Expression.Lambda<Func<T, T>>(xInit, xParameter);

            // compile to Func<Data, Data>
            return lambda.Compile();
        }

        public static string StringWordsRemove(this string stringToClean)
        {
            string str = stringToClean;
            foreach (var item in clientNameWordsToRemove)
            {
                if (str.StartsWith(item))
                {
                    int index = str.IndexOf(item);
                    str = (index < 0)
                        ? str
                        : str.Remove(index, item.Length);
                }
            }
            return str.Replace("RE:", "").Replace("RE", "").Replace("Count:", "").Replace("Count", "").Replace("Order:", "").Replace("Order", "").Trim();
        }

        public static string TrimStart(this string target, string trimChars)
        {
            return target.TrimStart(trimChars.ToCharArray());
        }

        public static string GetEnumDisplayName(this Enum enumType)
        {
            return enumType.GetType().GetMember(enumType.ToString())
                           .First()
                           .GetCustomAttribute<DisplayAttribute>()
                           .Name;
        }

        public static List<List<T>> Split<T>(this List<T> items, int sliceSize = 30)
        {
            List<List<T>> list = new List<List<T>>();
            for (int i = 0; i < items.Count; i += sliceSize)
                list.Add(items.GetRange(i, Math.Min(sliceSize, items.Count - i)));
            return list;
        }

        public static string RemoveEmptyLines(this string lines)
        {
            return Regex.Replace(lines, @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline).TrimEnd();
        }
    }

    #region ZipHouseHold Classes
    public class ZipHouseHoldStatistics
    {
        public string Header { get; set; }
        public List<ZipHouseHoldData> Records { get; set; } = new List<ZipHouseHoldData>();
    }

    public class ZipHouseHoldData
    {
        public string ZipCodes { get; set; }
        public long CntHouseHolds { get; set; }
        public string StrCntHouseHolds { get; set; }
    }
    #endregion
}
