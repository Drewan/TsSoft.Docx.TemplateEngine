﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace TsSoft.Docx.TemplateEngine.Tags.Processors
{
    internal class RepeaterProcessor : AbstractProcessor
    {
        public RepeaterTag RepeaterTag { get; set; }

        public override void Process()
        {
            base.Process();
            var current = RepeaterTag.StartContent;
            var dataReaders = DataReader.GetReaders(RepeaterTag.Source).DefaultIfEmpty().ToList();
            var repeaterElements =
                TraverseUtils.ElementsBetween(RepeaterTag.StartContent, RepeaterTag.EndContent)
                             .Select(RepeaterTag.MakeElementCallback).ToList();
            for (var index = 0; index < dataReaders.Count; index++)
            {
                //dataReaders[index].MissingDataModeSettings = MissingDataMode.ThrowException;
                
                current = this.ProcessElements(
                    repeaterElements,
                    dataReaders[index], current, null, index + 1);
            }
            foreach (var repeaterElement in repeaterElements)
            {
                repeaterElement.XElement.Remove();
            }
            this.CleanUp(RepeaterTag.StartRepeater, RepeaterTag.StartContent);
            this.CleanUp(RepeaterTag.EndContent, RepeaterTag.EndRepeater);
        }

        private XElement ProcessElements(IEnumerable<RepeaterElement> elements, DataReader dataReader, XElement start, XElement parent, int index)
        {
            XElement result = null;
            XElement previous = start;
            foreach (var repeaterElement in elements)
            {
                if (repeaterElement.IsIndex)
                {
                    result = DocxHelper.CreateTextElement(repeaterElement.XElement, repeaterElement.XElement.Parent, index.ToString(CultureInfo.CurrentCulture));
                }
                else if (repeaterElement.IsItem && repeaterElement.HasExpression)
                {
                    result = DocxHelper.CreateTextElement(repeaterElement.XElement, repeaterElement.XElement.Parent, dataReader.ReadText(repeaterElement.Expression));
                }
                else
                {
                    var element = new XElement(repeaterElement.XElement);
                    element.RemoveNodes();
                    result = element;
                    if (repeaterElement.HasElements)
                    {
                        this.ProcessElements(repeaterElement.Elements, dataReader, null, result, index);
                    }
                    else
                    {
                        element.Value = repeaterElement.XElement.Value;
                    }
                }
                if (previous != null)
                {
                    previous.AddAfterSelf(result);
                    previous = result;
                }
                else
                {
                    parent.Add(result);
                }
            }
            return result;
        }
    }
}
