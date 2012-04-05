﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using MARC.Everest.Xml;
using System.Xml;
using System.Reflection;
using MARC.Everest.DataTypes;
using MARC.Everest.Connectors;
using MARC.Everest.Formatters.XML.ITS1;
using MARC.Everest.Formatters.XML.Datatypes.R1;
using MARC.Everest.RMIM.UV.NE2008.Interactions;
using MARC.Everest.RMIM.UV.NE2008.Vocabulary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MARC.Everest.RMIM.UV.NE2008.RCMR_MT000001UV02;



namespace MARC.Everest.Test.Manual.Formatters
{
    /// <summary>
    /// Summary description for AddingAssemblies
    /// </summary>
    [TestClass]
    public class AddingAssembliesTest
    {
        public AddingAssembliesTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        
        /// <summary>
        /// Example 84
        /// Adding a pre-generated XML ITS 1.0 assembly.
        /// Cannot find assembly shown in Manual Example.
        /// </summary>
        [TestMethod]
        public void XMLITS1_SettingsTest01()
        {
            /*
            // Create formatter and setup graph aides
            var formatter = new XmlIts1Formatter();
            formatter.GraphAides.Add(new DatatypeFormatter()
            {
                CompatibilityMode = DatatypeFormatterCompatibilityMode.Universal
            });

            // Disable validation
            formatter.ValidateConformance = false;

            
            // Load the assembly and instruct the formatter to use the pre-genereated
            // code in the assembly
            formatter.AddFormatterAssembly(
                Assembly.LoadFile(@"C:\Users\pittersj\Documents\Everest\MARC.Everest.Formatters.XML.ITS1\obj\Debug\Refactor\MARC.Everest.Formatters.XML.ITS1.dll")
                ); 
             */
        }
    }
}
