using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;
using Rock.Update;
using Rock.Update.Helpers;

namespace Rock.Tests.Integration.RockUpdate
{
    [TestClass]
    public class OfflinePageHelperTests
    {
        private string _offlineFilePath;
        private string _offlineTemplateFilePath;

        [TestInitialize]
        public void TestInitialize()
        {
            _offlineFilePath = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "app_offline.htm" );
            _offlineTemplateFilePath = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "app_offline-template.htm" );

            CleanupTestFiles();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CleanupTestFiles();
        }

        private void CleanupTestFiles()
        {
            if ( File.Exists( _offlineFilePath ) )
            {
                File.Delete( _offlineFilePath );
            }

            if ( File.Exists( _offlineTemplateFilePath ) )
            {
                File.Delete( _offlineTemplateFilePath );
            }
        }

        [TestMethod]
        public void RemoveOfflinePage_ShouldRemoveOfflinePage()
        {
            File.WriteAllText( _offlineFilePath, "test" );

            OfflinePageHelper.RemoveOfflinePage();

            Assert.That.IsFalse( File.Exists( _offlineFilePath ) );
        }

        [TestMethod]
        public void RemoveOfflinePage_ShouldNotCrashIfNoOfflinePageExists()
        {
            OfflinePageHelper.RemoveOfflinePage();

            Assert.That.IsFalse( File.Exists( _offlineFilePath ) );
        }

        [TestMethod]
        public void RemoveOfflinePage_CreateOfflinePageShouldCreateDefaultPage()
        {
            OfflinePageHelper.CreateOfflinePage();

            Assert.That.IsTrue( File.Exists( _offlineFilePath ) );

            var actualFile = File.ReadAllText( _offlineFilePath );
            var expectedFile = @"
                <html>
                    <head>
                    <title>Application Updating...</title>
                    </head>
                    <body>
                        <h1>One Moment Please</h1>
                        This application is undergoing an essential update and is temporarily offline.  Please give me a minute or two to wrap things up.
                    </body>
                </html>
                ";
            Assert.That.AreEqual( expectedFile, actualFile );
        }

        [TestMethod]
        public void RemoveOfflinePage_CreateOfflinePageShouldOverwriteExistingPage()
        {
            File.WriteAllText( _offlineFilePath, "test" );

            OfflinePageHelper.CreateOfflinePage();

            Assert.That.IsTrue( File.Exists( _offlineFilePath ) );

            var actualFile = File.ReadAllText( _offlineFilePath );
            var expectedFile = @"
                <html>
                    <head>
                    <title>Application Updating...</title>
                    </head>
                    <body>
                        <h1>One Moment Please</h1>
                        This application is undergoing an essential update and is temporarily offline.  Please give me a minute or two to wrap things up.
                    </body>
                </html>
                ";
            Assert.That.AreEqual( expectedFile, actualFile );
        }

        [TestMethod]
        public void RemoveOfflinePage_CreateOfflinePageShouldUseTemplatePage()
        {
            File.WriteAllText( _offlineTemplateFilePath, "test" );

            OfflinePageHelper.CreateOfflinePage();

            Assert.That.IsTrue( File.Exists( _offlineFilePath ) );

            var actualFile = File.ReadAllText( _offlineFilePath );
            var expectedFile = "test";
            Assert.That.AreEqual( expectedFile, actualFile );
        }

        [TestMethod]
        public void RemoveOfflinePage_CreateOfflinePageShouldOverwriteExistingPageWithTemplate()
        {
            File.WriteAllText( _offlineFilePath, "test" );
            File.WriteAllText( _offlineTemplateFilePath, "app_offline-template" );

            OfflinePageHelper.CreateOfflinePage();

            Assert.That.IsTrue( File.Exists( _offlineFilePath ) );

            var actualFile = File.ReadAllText( _offlineFilePath );
            var expectedFile = "app_offline-template";
            Assert.That.AreEqual( expectedFile, actualFile );
        }
    }
}
