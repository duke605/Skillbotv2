using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fclp.Internals.Extensions;
using SkillBotv2.Extensions;

namespace SkillBotv2.Util
{
    class Table
    {
        public string Title { get; private set; }
        public string[] Headings { get; private set; }
        private List<Row> Rows { get; }
        private readonly Dictionary<int, int> _longest;

        public Table()
        {
            Rows = new List<Row>();
            _longest = new Dictionary<int, int>();
        }

        /// <summary>
        /// Sets the tables headings
        /// </summary>
        public Table SetHeadings(params string[] headings)
        {
            // Updating longest values
            UpdateLongest(headings);

            Headings = headings;
            return this;
        }

        /// <summary>
        /// Sets the title of the table
        /// </summary>
        public Table SetTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary>
        /// Adds a row to the table
        /// </summary>
        public Table AddRow(params object[] cols)
        {
            Row row = new Row();

            // Updating longest values
            UpdateLongest(cols);

            // Adding columns to row
            foreach (var col in cols)
            {

                // Creating column for row
                if (col is Column)
                    row.Columns.Add((Column) col);
                else
                    row.Columns.Add(new Column(col.ToString()));
            }
            
            Rows.Add(row);
            return this;
        }

        /// <summary>
        /// Updates the longest values
        /// </summary>
        private void UpdateLongest<T>(IEnumerable<T> @params)
        {
            @params.ForEachWithIndex((i, s) =>
            {
                int longest;

                if (_longest.TryGetValue(i, out longest))
                    _longest[i] = s.ToString().Length > longest ? s.ToString().Length : longest;
                else
                    _longest[i] = s.ToString().Length;
            });
        }

        public override string ToString()
        {
            int totalLength = _longest.Values.Sum() + _longest.Values.Count * 3 + 2;
            string ret = ".".PadRight(totalLength - 2, '-') + ".\n";
            
            // Printing title if there is one
            if (!Title.IsNullOrEmpty())
                ret += $"| {Title.Center(totalLength - 5)} |\n" +
                       "|".PadRight(totalLength - 2, '-') + "|\n";

            // Printing headers if has some
            Headings.ForEachWithIndex((i, h) =>
            {
                ret += $"| {h.Center(_longest[i])} ";

                if (Headings.Length - 1 == i)
                    ret += "|\n" +
                           "|".PadRight(totalLength - 2, '-') + "|\n";
            });

            // Printing rows
            Rows.ForEachWithIndex((i, r) =>
            {
                // Printing columns
                r.Columns.ForEachWithIndex((j, c) =>
                {
                    // Printing data
                    if (c.Align == Column.Alignment.Left)
                        ret += $"| {c.Text.PadRight(_longest[j])} ";
                    else
                        ret += $"| {c.Text.PadLeft(_longest[j])} ";
                });

                ret += "|\n";
            });

            // Closing table

            ret += "'".PadRight(totalLength - 2, '-') + "'";
            return ret;
        }

        public class Row
        {
            public List<Column> Columns { get; set; }

            public Row()
            {
                Columns = new List<Column>();
            }
        }

        public class Column
        {
            public string Text { get; set; }
            public Alignment Align { get; set; }

            public Column(object text, Alignment align = Alignment.Left)
            {
                Text = text.ToString();
                Align = align;
            }

            public override string ToString()
            {
                return Text;
            }

            public enum Alignment
            {
                Left,
                Right
            }
        }
    }
}
