﻿namespace TsSoft.Docx.TemplateEngine.Tags
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Xml.Linq;

    using TsSoft.Docx.TemplateEngine.Tags.Processors;

    internal class IfParser : GeneralParser
    {
        private const string StartTagName = "If";
        private const string EndTagName = "EndIf";

        public override XElement Parse(ITagProcessor parentProcessor, XElement startElement)
        {
            this.ValidateStartTag(startElement, StartTagName);
            var startTag = startElement;

            if (string.IsNullOrEmpty(startTag.Value))
            {
                throw new Exception(string.Format(MessageStrings.TagNotFoundOrEmpty, "If"));
            }

            var endTag = this.FindEndTag(startTag);

            var content = TraverseUtils.ElementsBetween(startTag, endTag);
            var expression = startTag.GetExpression();

            var ifTag = new IfTag
                            {
                                Conidition = expression,
                                EndIf = endTag,
                                IfContent = content,
                                StartIf = startTag,
                            };

            var ifProcessor = new IfProcessor { Tag = ifTag };
            if (content.FirstOrDefault() != null)
            {
                this.GoDeeper(ifProcessor, content.First(), endTag);
            }            
            parentProcessor.AddProcessor(ifProcessor);

            return endTag;
        }
        
        private XElement FindEndTag(XElement startTag)
        {
            var ifTagsOpened = 1;
            var current = startTag;
            while (ifTagsOpened > 0 && current != null)
            {
                var nextTagElements = TraverseUtils.NextTagElements(current, new Collection<string> { "if", "endif" }).ToList();
                foreach (var nextTagElement in nextTagElements)
                {
                    if (nextTagElement.IsTag("if"))
                    {
                        ifTagsOpened++;
                    }
                    else
                    {
                        if (--ifTagsOpened == 0)
                        {
                            return nextTagElement;
                        }
                    }
                }
            }
            if (current == null)
            {
                throw new Exception(string.Format(MessageStrings.TagNotFoundOrEmpty, "EndIf"));
            }
            return current;
        }
        /*
        private XElement FindEndTag(XElement startTag, string startTagName, string endTagName)
        {
            var startTagsOpened = 1;
            var current = startTag;
            while (startTagsOpened > 0 && current != null)
            {
                var nextTagElements = TraverseUtils.NextTagElements(current, new Collection<string> { startTagName, endTagName }).ToList();
                var index = -1;
                while ((index < nextTagElements.Count) && (startTagsOpened != 0))
                {
                    index++;
                    if (nextTagElements[index].IsTag(startTagName))
                    {
                        startTagsOpened++;
                    }
                    else
                    {
                        startTagsOpened--;
                    }
                }
                current = nextTagElements[index];
            }
            if (current == null)
            {
                throw new Exception(string.Format(MessageStrings.TagNotFoundOrEmpty, endTagName));
            }
            return current;
        }
        */
        private bool GoDeeper(ITagProcessor parentProcessor, XElement element, XElement endElement)
        {
            var endReached = false;
            do
            {
                if (element.IsSdt())
                {
                    if (element.Equals(endElement))
                    {
                        return true;
                    }
                    element = this.ParseSdt(parentProcessor, element);
                }
                else if (element.HasElements)
                {
                    endReached = this.GoDeeper(parentProcessor, element.Elements().First(), endElement);                    
                }
                element = element.NextElementWithUpTransition();
            }
            while (element != null && !endReached);
            return endReached;
        }
        /*
        private bool GoDeeper(ITagProcessor parentProcessor, XElement element)
        {
            var endReached = false;
            do
            {
                if (element.IsSdt())
                {
                    switch (this.GetTagName(element).ToLower())
                    {
                        case "endif":
                            return true;
                        
                        default:
                            element = this.ParseSdt(parentProcessor, element);
                            break;
                    }
                }
                else if (element.HasElements)
                {
                    endReached = this.GoDeeper(parentProcessor, element.Elements().First());
                }
                element = element.NextElement();
            }
            while (element != null && !endReached);
            return endReached;
        }
         */
    }
}
