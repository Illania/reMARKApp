using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using NUnit.Framework;

namespace TestProject
{
    [TestFixture]
    public class ValidatorTest
    {
        const string test1 = "fp@nordic-it.com";
        const string test2 = "Ferdinando Papale <fp@nordic-it.com>";
        const string test3 = " Innovasjon AS - Support <support@innovasjon.as>";
        const string test4 = "SD Maersk Tankers <SD.MaerskTankers@fi.fujitsu.com>";
        const string test5 = "'Nordic IT - Service Team' <support@nordic-it.com>";
        const string test6 = "Philip Farnes (Braemar Group ICT) <philip.farnes@braemar.com>, Casper Thalund <ct@nordic-it.com>, " +
            "dh@nordic-it.com <dh@nordic-it.com>, support@nordic-it.com";
        const string test7 = "";
        const string test8 = "ab@nordic-it.com,,,";
        const string test9 = "fp, am@b.com";

        public IEnumerable<string> GetEmails(string text) => Validator.ExtractValidEmails(text).Cast<Match>().Select(m => m.Value).Distinct().ToArray();
        public IEnumerable<DocumentAddress> GetDocumentAddreses(string text) => Validator.ExtractValidDocumentAddresses(text);

        bool AreEmailsEqual(IEnumerable<string> emails, IEnumerable<DocumentAddress> documentAddresses)
        {
            return Enumerable.SequenceEqual(emails, documentAddresses.Select(da => da.Address));
        }

        bool AreNamesEqual(IEnumerable<string> names, IEnumerable<DocumentAddress> documentAddresses)
        {
            return Enumerable.SequenceEqual(names, documentAddresses.Select(da => da.Name));
        }

        [Test]
        public void EmailsExtractionTest()
        {
            var testCases = new[] { test1, test2, test3, test4, test5, test6, test7, test8, test9 };

            foreach (var testCase in testCases)
            {
                var emails = GetEmails(testCase);
                var das = GetDocumentAddreses(testCase);

                Assert.True(AreEmailsEqual(emails, das));
            }
        }

        [Test]
        public void NameExtractionTest1()
        {
            var names = new string[] { string.Empty };
            var das = GetDocumentAddreses(test1);

            Assert.True(AreNamesEqual(names, das));
        }

        [Test]
        public void NameExtractionTest2()
        {
            var names = new[] { "Ferdinando Papale" };
            var das = GetDocumentAddreses(test2);

            Assert.True(AreNamesEqual(names, das));
        }

        [Test]
        public void NameExtractionTest3()
        {
            var names = new string[] { "Innovasjon AS - Support" };
            var das = GetDocumentAddreses(test3);

            Assert.True(AreNamesEqual(names, das));
        }

        [Test]
        public void NameExtractionTest4()
        {
            var names = new[] { "SD Maersk Tankers" };
            var das = GetDocumentAddreses(test4);

            Assert.True(AreNamesEqual(names, das));
        }

        [Test]
        public void NameExtractionTest5()
        {
            var names = new[] { "'Nordic IT - Service Team'" };
            var das = GetDocumentAddreses(test5);

            Assert.True(AreNamesEqual(names, das));
        }

        [Test]
        public void NameExtractionTest6()
        {
            var names = new[] { "Philip Farnes (Braemar Group ICT)", "Casper Thalund", "dh@nordic-it.com", string.Empty };
            var das = GetDocumentAddreses(test6);

            Assert.True(AreNamesEqual(names, das));
        }

        [Test]
        public void NameExtractionTest7()
        {
            var das = GetDocumentAddreses(test7);

            Assert.AreEqual(das.Count(), 0);
        }

        [Test]
        public void NameExtractionTest8()
        {
            var names = new string[] { string.Empty };
            var das = GetDocumentAddreses(test8);

            Assert.True(AreNamesEqual(names, das));
        }

        [Test]
        public void NameExtractionTest9()
        {
            var names = new string[] { string.Empty };
            var das = GetDocumentAddreses(test9);

            Assert.True(AreNamesEqual(names, das));
        }
    }
}

