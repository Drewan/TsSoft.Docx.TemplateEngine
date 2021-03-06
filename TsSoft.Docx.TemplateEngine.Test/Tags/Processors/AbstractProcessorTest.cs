﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Xml.Linq;
using TsSoft.Commons.Utils;
using TsSoft.Docx.TemplateEngine.Tags;
using TsSoft.Docx.TemplateEngine.Tags.Processors;

namespace TsSoft.Docx.TemplateEngine.Test.Tags.Processors
{
    [TestClass]
    public class AbstractProcessorTest
    {
        private XElement documentRoot;

        [TestInitialize]
        public void Initialize()
        {
            using (var docStream = AssemblyResourceHelper.GetResourceStream(this, "TableProcessorTemplateTest.xml"))
            {
                var doc = XDocument.Load(docStream);
                this.documentRoot = doc.Root.Element(WordMl.BodyName);
            }
        }
        /*
        [TestMethod]
        public void CleanUpTest()
        {
            var tagTable = TraverseUtils.TagElement(this.documentRoot, "Table");
          //  var tagContent = TraverseUtils.TagElement(this.documentRoot, "Content");
           // var tagEndContent = TraverseUtils.TagElement(this.documentRoot, "EndContent");
            var tagEndTable = TraverseUtils.TagElement(this.documentRoot, "EndTable");
            var startMarkerTag = new XElement(tagTable);
            startMarkerTag.Elements().Remove();
            var endMarkerTag = new XElement(tagTable);
            endMarkerTag.Elements().Remove();

            tagTable.AddBeforeSelf(startMarkerTag);
            tagContent.AddAfterSelf(endMarkerTag);
            var processor = new TableProcessor();
            processor.CleanUp(tagTable, tagContent);
            var elementsBeforMarkers = TraverseUtils.ElementsBetween(startMarkerTag, endMarkerTag);
            Assert.IsFalse(elementsBeforMarkers.Any());

            startMarkerTag.Remove();
            endMarkerTag.Remove();
            tagEndContent.AddBeforeSelf(startMarkerTag);
            tagEndTable.AddAfterSelf(endMarkerTag);
            processor.CleanUp(tagEndContent, tagEndTable);
            elementsBeforMarkers = TraverseUtils.ElementsBetween(startMarkerTag, endMarkerTag);
            Assert.IsFalse(elementsBeforMarkers.Any());
        }*/
    }
}
