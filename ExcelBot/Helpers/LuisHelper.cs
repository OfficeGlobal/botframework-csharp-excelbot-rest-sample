﻿/* 
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using Microsoft.Bot.Builder.Luis.Models;

namespace ExcelBot.Helpers
{
    public static class LuisHelper
    {
        public static string GetCellEntity(IList<EntityRecommendation> entities)
        {
            var entity = entities.FirstOrDefault<EntityRecommendation>((e) => e.Type == "Cell");
            return (entity != null) ? entity.Entity.ToUpper() : null;
        }

        public static string GetNameEntity(IList<EntityRecommendation> entities)
        {
            var index = entities.IndexOf<EntityRecommendation>((e) => e.Type == "Name");
            if (index >= 0)
            {
                var name = new StringBuilder();
                var separator = "";
                while ((index < entities.Count) && (entities[index].Type == "Name"))
                {
                    name.Append($"{separator}{entities[index].Entity}");
                    separator = " ";
                    ++index;
                }
                return name.ToString().Replace(" _ ","_").Replace(" - ", "-");
            }
            else
            {
                return null;
            }
        }

        public static string GetChartEntity(IList<EntityRecommendation> entities)
        {
            var names = entities.Where<EntityRecommendation>((e) => (e.Type == "Name"));
            if (names != null)
            {
                var name = new StringBuilder();
                var separator = "";
                foreach (var entitiy in names)
                {
                    name.Append($"{separator}{entitiy.Entity}");
                    separator = " ";
                }
                return name.ToString();
            }
            else
            {
                return null;
            }
        }

        public static object GetValue(LuisResult result)
        {
            if (result.Entities.Count == 0)
            {
                // There is no entities in the query
                return null;
            }

            // Check for a string value
            var first = result.Entities.FirstOrDefault(er => ((er.Type == "builtin.number") || (er.Type == "Text") || (er.Type == "Workbook")));
            if (first != null)
            {
                var startIndex = (int)(result.Entities.Where(er => ((er.Type == "builtin.number") || (er.Type == "Text") || (er.Type == "Workbook"))).Min(er => er.StartIndex));
                var endIndex = (int)(result.Entities.Where(er => ((er.Type == "builtin.number") || (er.Type == "Text") || (er.Type == "Workbook"))).Max(er => er.EndIndex));
                return result.Query.Substring(startIndex, endIndex - startIndex + 1);
            }

            // Check for a number value
            var numberEntity = result.Entities.FirstOrDefault(er => er.Type == "builtin.number");
            if (numberEntity != null)
            {
                // There is a number entity in the query
                return Double.Parse(numberEntity.Entity.Replace(" ", ""));
            }

            // No value was found
            return null;
        }

        public static string GetFilenameEntity(IList<EntityRecommendation> entities)
        {
            var sb = new StringBuilder();
            var separator = "";
            foreach (var entity in entities)
            {
                if (entity.Entity != "xlsx")
                {
                    sb.Append(separator);
                    sb.Append(entity.Entity);
                    separator = " ";
                }
            }
            var filename = sb.ToString().Replace(" _ ", "_").Replace(" - ", "-");
            return filename;
        }

        public static object[] GetTableRow(IList<EntityRecommendation> entities, string query)
        {
            var items = new List<object>();
            var sb = new StringBuilder();
            var separator = "";
            foreach (var entity in entities.OrderBy<EntityRecommendation, int?>(e => e.StartIndex))
            {
                if (entity.Type.ToLower() == "text")
                {
                    if ((entity.Entity == ",") || (entity.Entity == ";"))
                    {
                        if (sb.Length > 0)
                        {
                            items.Add(sb.ToString());
                            sb.Clear();
                            separator = "";
                        }
                    }
                    else
                    {
                        sb.Append($"{separator}{query.Substring(entity.StartIndex ?? 0, entity.EndIndex - entity.StartIndex + 1 ?? 0)}");
                        separator = " ";
                    }
                } 
                else if (entity.Type.ToLower() == "builtin.number")
                {
                    if (sb.Length > 0)
                    {
                        items.Add(sb.ToString());
                        sb.Clear();
                        separator = "";
                    }
                    items.Add(Double.Parse(entity.Entity));
                }
            }
            if (sb.Length > 0)
            {
                items.Add(sb.ToString());
            }
            return items.ToArray();
        }
    }
}