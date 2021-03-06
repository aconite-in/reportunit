﻿namespace ReportUnit.Parser
{
    using System;
    using System.IO;
    using System.Xml;

    using Logging;

    internal static class ParserFactory
    {
        private static Logger logger = Logger.GetLogger();

        /// <summary>
        /// Find the appropriate Parser for the test file
        /// </summary>
        /// <param name="resultsFile"></param>
        /// <returns></returns>
        public static IParser LoadParser(string resultsFile)
        {
            if (!File.Exists(resultsFile))
            {
                logger.Error("Input file does not exist " + resultsFile);
                return null;
            }
            
            string fileExtension = Path.GetExtension(resultsFile);
            
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                logger.Error("Input file does not have a file extension: " + resultsFile);
                return null;
            }

            IParser fileParser = null;

            switch (TestRunnerType(resultsFile))
            {
                case TestRunner.NUnit:
                    logger.Info("The file " + resultsFile + " contains NUnit test results");
                    fileParser = new NUnit().LoadFile(resultsFile);
                    break;
                case TestRunner.Gallio:
                    logger.Info("The file " + resultsFile + " contains Gallio test results");
                    fileParser = new Gallio().LoadFile(resultsFile);
                    break;
                case TestRunner.MSTest2010:
                    logger.Info("The file " + resultsFile + " contains MSTest 2010 test results");
                    fileParser = new MsTest2010().LoadFile(resultsFile);
					break;
				case TestRunner.XUnitV1:
                    logger.Info("The file " + resultsFile + " contains xUnit v1 test results");
					fileParser = new XUnitV1().LoadFile(resultsFile);
					break;
				case TestRunner.XUnitV2:
					logger.Info("The file " + resultsFile + " contains xUnit v2 test results");
					fileParser = new XUnitV2().LoadFile(resultsFile);
					break;
                case TestRunner.TestNG:
                    logger.Info("The file " + resultsFile + " contains TestNG test results");
                    fileParser = new TestNG().LoadFile(resultsFile);
                    break;
                default:
                    logger.Info("Skipping " + resultsFile + ". It is not of a known test runner type.");
                    break;
            }

            return fileParser;
        }

        private static TestRunner TestRunnerType(string filePath)
        {
            XmlDocument doc = new XmlDocument();

            XmlNamespaceManager nsmgr;

            try
            {
                doc.Load(filePath);

                if (doc.DocumentElement == null)
                    return TestRunner.Unknown;

                string fileExtension = Path.GetExtension(filePath).ToLower();

                if (fileExtension.EndsWith("trx"))
                {
                    // MSTest2010
                    nsmgr = new XmlNamespaceManager(doc.NameTable);
                    nsmgr.AddNamespace("ns", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");

                    // check if its a mstest 2010 xml file 
                    // will need to check the "//TestRun/@xmlns" attribute - value = http://microsoft.com/schemas/VisualStudio/TeamTest/2010
                    XmlNode testRunNode = doc.SelectSingleNode("ns:TestRun", nsmgr);
                    if (testRunNode != null && testRunNode.Attributes != null && testRunNode.Attributes["xmlns"] != null && testRunNode.Attributes["xmlns"].InnerText.Contains("2010"))
                    {
                        return TestRunner.MSTest2010;
                    }
                }

                if (fileExtension.EndsWith("xml"))
                {
                    // Gallio
                    nsmgr = new XmlNamespaceManager(doc.NameTable);
                    nsmgr.AddNamespace("ns", "http://www.gallio.org/");

                    XmlNode model = doc.SelectSingleNode("//ns:testModel", nsmgr);
                    if (model != null) return TestRunner.Gallio;


					// xUnit - will have <assembly ... test-framework="xUnit.net 2....."/>
					XmlNode assemblyNode = doc.SelectSingleNode("//assembly");
	                if (assemblyNode != null && assemblyNode.Attributes != null &&
	                    assemblyNode.Attributes["test-framework"] != null)
	                {
		                string testFramework = assemblyNode.Attributes["test-framework"].InnerText.ToLower();

						if (testFramework.Contains("xunit"))
		                {
			                if (testFramework.Contains(" 2."))
							{
								return TestRunner.XUnitV2;   
			                }
							else if (testFramework.Contains(" 1."))
							{
								return TestRunner.XUnitV1;
							}
		                }
	                }


                    // NUnit
                    // NOTE: not all nunit test files (ie when have nunit output format from other test runners) will contain the environment node
                    //            but if it does exist - then it should have the nunit-version attribute
                    XmlNode envNode = doc.SelectSingleNode("//environment");
                    if (envNode != null && envNode.Attributes != null && envNode.Attributes["nunit-version"] != null) return TestRunner.NUnit;

                    // check for test-suite nodes - if it has those - its probably nunit tests
                    var testSuiteNodes = doc.SelectNodes("//test-suite");
                    if (testSuiteNodes != null && testSuiteNodes.Count > 0) return TestRunner.NUnit;


                    // TestNG
                    if (doc.DocumentElement.Name == "testng-results")
                        return TestRunner.TestNG;
                }
            }
            catch { }

            return TestRunner.Unknown;
        }
    }
}
