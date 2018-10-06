using System;
using DirectoryHelperLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DirectoryHelperLibraryTest
{
    [TestClass]
    public class FileProcessorTest
    {
        [TestMethod]
        public void SimpleFileProcessor_FromPath_ShouldReturnPath()
        {
            IFileProcessor processor = new SimpleFileProcessor();
            Assert.AreEqual(processor.Process(@"fd\sfd\fi"), @"fd\sfd\fi", true, "results should be equal");
        }

        [TestMethod]
        public void CsFileProcessor_FromCsPath_ShouldReturnCsPath()
        {
            IFileProcessor processor = new CsFileProcessor();
            Assert.AreEqual(processor.Process(@"fd\sfd\fi.cs"), @"fd\sfd\fi.cs/", true, "results should be equal");
        }

        [TestMethod]
        public void CsFileProcessor_FromNonCsPath_ShouldReturnNull()
        {
            IFileProcessor processor = new CsFileProcessor();
            Assert.AreEqual(processor.Process(@"fd\sfd\fi"), null, true, "results should be null");
        }

        [TestMethod]
        public void ReverseFileProcessor1_FromPath_ShouldReturnReversedPath()
        {
            IFileProcessor processor = new ReverseFileProcessor1();
            Assert.AreEqual(processor.Process(@"f\bla\ra\t.dat"), @"t.dat\ra\bla\f", true, "results should be equal");
        }

        [TestMethod]
        public void ReverseFileProcessor2_FromPath_ShouldReturnReversedPath()
        {
            IFileProcessor processor = new ReverseFileProcessor2();
            Assert.AreEqual(processor.Process(@"f\bla\ra\t.dat"), @"tad.t\ar\alb\f", true, "results should be equal");
        }
    }
}
