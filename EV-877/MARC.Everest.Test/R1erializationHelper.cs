﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MARC.Everest.DataTypes.Interfaces;
using MARC.Everest.Interfaces;
using MARC.Everest.Formatters.XML.Datatypes.R2;
using MARC.Everest.Xml;
using System.IO;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MARC.Everest.Connectors;
using MARC.Everest.Formatters.XML.Datatypes.R1;

namespace MARC.Everest.Test
{
    /// <summary>
    /// Helper methods for serialization tests
    /// </summary>
    public static class R1SerializationHelper
    {

        /// <summary>
        /// Serialize as a string
        /// </summary>
        internal static String SerializeAsString(IGraphable graph)
        {
            DatatypeFormatter fmtr = new DatatypeFormatter();
            StringWriter sw = new StringWriter();
            XmlStateWriter xsw = new XmlStateWriter(XmlWriter.Create(sw, new XmlWriterSettings() { Indent = true }));
            xsw.WriteStartElement("test", "urn:hl7-org:v3");
            xsw.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            var result = fmtr.Graph(xsw, graph);
            xsw.WriteEndElement();
            xsw.Flush();
            sw.Flush();
            System.Diagnostics.Trace.WriteLine(sw.ToString());
            Assert.AreEqual(ResultCode.Accepted, result.Code);
            return sw.ToString();
        }


        /// <summary>
        /// Parse from string
        /// </summary>
        internal static T ParseString<T>(string xmlString)
        {
            return Util.Convert<T>(ParseString(xmlString, typeof(T)));
        }

        /// <summary>
        /// Parse a string
        /// </summary>
        /// <param name="actualXml"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static object ParseString(string xmlString, Type type)
        {
            StringReader sr = new StringReader(xmlString);
            DatatypeFormatter fmtr = new DatatypeFormatter();
            XmlStateReader rdr = new XmlStateReader(XmlReader.Create(sr));
            while (rdr.NodeType != XmlNodeType.Element)
                rdr.Read();
            var result = fmtr.Parse(rdr, type);
            Assert.AreEqual(ResultCode.Accepted, result.Code);
            return result.Structure;
        }
    }
}