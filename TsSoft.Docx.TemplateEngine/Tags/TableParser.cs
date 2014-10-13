﻿using System;
using System.Linq;
using System.Xml.Linq;
using TsSoft.Docx.TemplateEngine.Tags.Processors;

namespace TsSoft.Docx.TemplateEngine.Tags
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Parse table tag
    /// </summary>
    internal class TableParser : GeneralParser
    {
        private static Func<XElement, IEnumerable<TableElement>> MakeTableElementCallback = parentElement =>
            {
                ICollection<TableElement> tableElements = new Collection<TableElement>();
                var currentTagElement = TraverseUtils.NextTagElements(parentElement).FirstOrDefault();
                while (currentTagElement != null && currentTagElement.Ancestors().Any(element => element.Equals(parentElement)))
                {
                    var tableElement = ToTableElement(currentTagElement);

                    if (tableElement.IsItem || tableElement.IsIndex || tableElement.IsItemIf)
                    {
                        tableElements.Add(tableElement);
                    }
                    if (tableElement.IsItemIf)
                    {
                        currentTagElement = tableElement.EndTag;
                    }
                    currentTagElement = TraverseUtils.NextTagElements(currentTagElement).FirstOrDefault();
                }

                return tableElements;
            };

        /// <summary>
        /// Do parsing
        /// </summary>
        public override XElement Parse(ITagProcessor parentProcessor, XElement startElement)
        {
            this.ValidateStartTag(startElement, "Table");

            if (parentProcessor == null)
            {
                throw new ArgumentNullException();
            }

            var endTableTag = this.TryGetRequiredTags(startElement, "EndTable").First();
            var itemsElements = this.TryGetRequiredTags(startElement, endTableTag, "Items");

            if (itemsElements.All(e => string.IsNullOrEmpty(e.Value)))
            {
                throw new Exception(string.Format(MessageStrings.TagNotFoundOrEmpty, "Items"));
            }

            var contentTag = this.TryGetRequiredTags(startElement, endTableTag, "Content").First();
            var endContentTag = this.TryGetRequiredTags(contentTag, endTableTag, "EndContent").First();

            var tag = new TableTag
                          {
                              TagTable = startElement,
                              TagEndTable = endTableTag,
                              ItemsSource = itemsElements.First().Value,
                              TagContent = contentTag,
                              TagEndContent = endContentTag
                          };

            var dynamicRowElement = TraverseUtils.TagElementsBetween(startElement, endTableTag, "DynamicRow").FirstOrDefault();
            if (dynamicRowElement != null)
            {
                int dynamicRowValue;
                tag.DynamicRow = int.TryParse(dynamicRowElement.Value, out dynamicRowValue)
                                     ? dynamicRowValue
                                     : (int?)null;
            }

            var tableElement = contentTag.ElementsAfterSelf(WordMl.TableName).FirstOrDefault(element => element.IsBefore(endContentTag));
            if (tableElement != null)
            {
                tag.Table = tableElement;
            }

            tag.MakeTableElementCallback = MakeTableElementCallback;

            var processor = new TableProcessor { TableTag = tag };

            var contentElements = TraverseUtils.ElementsBetween(contentTag, endContentTag);
            if (contentElements.Any())
            {
                this.GoDeeper(processor, contentElements.First());
            }
            parentProcessor.AddProcessor(processor);

            return endTableTag;
        }

        private static IEnumerable<TableElement> MakeTableElement(XElement startElement, XElement endElement)
        {
            ICollection<TableElement> tableElements = new Collection<TableElement>();
            var currentTagElement = TraverseUtils.NextTagElements(startElement).FirstOrDefault();
            while (currentTagElement != null && currentTagElement.IsBefore(endElement))
            {
                var tableElement = ToTableElement(currentTagElement);

                if (tableElement.IsItem || tableElement.IsIndex || tableElement.IsItemIf)
                {
                    tableElements.Add(tableElement);
                }
                if (tableElement.IsItemIf)
                {
                    currentTagElement = tableElement.EndTag;
                }
                currentTagElement = TraverseUtils.NextTagElements(currentTagElement).FirstOrDefault();
            }

            return tableElements;
        }

        private static TableElement ToTableElement(XElement tagElement)
        {
            var tableElement = new TableElement
                {
                    IsItem = tagElement.IsTag("item"),
                    IsIndex = tagElement.IsTag("itemindex"),
                    IsItemIf = tagElement.IsTag("itemif"),
                    StartTag = tagElement,
                };
            if (tableElement.IsItem)
            {
                tableElement.Expression = tagElement.GetExpression();
            }
            else if (tableElement.IsItemIf)
            {
                tableElement.EndTag = FindEndTag(tableElement.StartTag);
                tableElement.Expression = tagElement.GetExpression();
                tableElement.TagElements = MakeTableElement(tableElement.StartTag, tableElement.EndTag);
            }

            return tableElement;
        }

        private static XElement FindEndTag(XElement startTag)
        {
            var ifTagsOpened = 1;
            var current = startTag;
            while (ifTagsOpened > 0 && current != null)
            {
                var nextTagElements = TraverseUtils.NextTagElements(current, new Collection<string> { "itemif", "enditemif" }).ToList();
                int index = -1;
                while ((index < nextTagElements.Count) && (ifTagsOpened != 0))
                {
                    index++;
                    if (nextTagElements[index].IsTag("itemif"))
                    {
                        ifTagsOpened++;
                    }
                    else
                    {
                        ifTagsOpened--;
                    }
                }
                current = nextTagElements[index];
            }
            if (current == null)
            {
                throw new Exception(string.Format(MessageStrings.TagNotFoundOrEmpty, "enditemif"));
            }
            return current;
        }

        private void GoDeeper(ITagProcessor parentProcessor, XElement element)
        {
            do
            {
                if (element.IsSdt())
                {
                    switch (this.GetTagName(element).ToLower())
                    {
                        case "item":
                        case "itemindex":
                        case "itemif":
                        case "enditemif":
                            break;
                        default:
                            element = this.ParseSdt(parentProcessor, element);
                            break;
                    }
                }
                else if (element.HasElements)
                {
                    this.GoDeeper(parentProcessor, element.Elements().First());
                }
                element = element.NextElement();
            }
            while (element != null && (!element.IsSdt() || GetTagName(element).ToLower() != "endcontent"));
        }
    }
}
