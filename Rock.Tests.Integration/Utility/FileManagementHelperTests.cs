using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Update;

namespace Rock.Tests.Integration.Utility
{
    [TestClass]
    public class FileManagementHelperTests
    {
        private string _testDirectory;

        [TestInitialize]
        public void TestInitialize()
        {
            var testDirectory = Path.Combine( AppContext.BaseDirectory, $"..\\..\\{Guid.NewGuid()}" );
            _testDirectory = Path.GetFullPath( testDirectory );

            if ( !Directory.Exists( _testDirectory ) )
            {
                Directory.CreateDirectory( _testDirectory );
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if ( Directory.Exists( _testDirectory ) )
            {
                Directory.Delete( _testDirectory, true );
            }
        }

        [TestMethod]
        public void CleanUpDeletedFiles_ShouldRemoveRenamedFiles()
        {
            var fileName = "test.txt";
            var testFilename = Path.Combine( AppContext.BaseDirectory, $"{fileName}.1.rdelete" );
            File.WriteAllText( testFilename, "test" );

            FileManagementHelper.CleanUpDeletedFiles();

            Assert.That.IsFalse( File.Exists( testFilename ) );
        }

        [TestMethod]
        public void RenameFile_ShouldRenameFile()
        {
            var fileName = "test.txt";
            var testFilename = Path.Combine( _testDirectory, fileName );
            File.WriteAllText( testFilename, "test" );

            FileManagementHelper.RenameFile( testFilename );

            var expectedFilename = Path.Combine( _testDirectory, $"{fileName}.1.rdelete" );
            Assert.That.IsTrue( File.Exists( expectedFilename ) );
            Assert.That.IsFalse( File.Exists( testFilename ) );
        }

        [TestMethod]
        public void RenameFile_ShouldNotCrashIfFileDoesNotExists()
        {
            var fileName = "test.txt";
            var testFilename = Path.Combine( _testDirectory, fileName );

            FileManagementHelper.RenameFile( testFilename );

            var expectedFilename = Path.Combine( _testDirectory, $"{fileName}.1.rdelete" );
            Assert.That.IsFalse( File.Exists( expectedFilename ) );
            Assert.That.IsFalse( File.Exists( testFilename ) );
        }

        [TestMethod]
        public void RenameActiveFile_ShouldRenameContentDll()
        {
            var rootDirectory = Path.GetFullPath( Path.Combine( AppContext.BaseDirectory, $"..\\..\\" ) );

            var testFilename = $"..\\..\\{_testDirectory.Replace( rootDirectory, "" )}\\test.dll";
            File.WriteAllText( testFilename, "test" );

            FileManagementHelper.RenameActiveFile( testFilename );

            var expectedFilename = Path.Combine( _testDirectory, "test.dll.1.rdelete" );
            testFilename = Path.Combine( _testDirectory, "test.dll" );

            Assert.That.IsTrue( File.Exists( expectedFilename ) );
            Assert.That.IsFalse( File.Exists( testFilename ) );
        }

        [TestMethod]
        public void RenameActiveFile_ShouldRenameRoslynDll()
        {
            var testFilename = Path.Combine( _testDirectory, "roslyn\\test.dll" );
            Directory.CreateDirectory( Path.GetDirectoryName( testFilename ) );

            File.WriteAllText( testFilename, "test" );

            FileManagementHelper.RenameActiveFile( testFilename );

            var expectedFilename = Path.Combine( _testDirectory, "roslyn\\test.dll.1.rdelete" );
            Assert.That.IsTrue( File.Exists( expectedFilename ) );
            Assert.That.IsFalse( File.Exists( testFilename ) );
        }

        [TestMethod]
        public void RenameActiveFile_ShouldNotRenameBinDll()
        {
            var rootDirectory = Path.GetFullPath( Path.Combine( AppContext.BaseDirectory, $"..\\..\\" ) );

            var testFilename = $"..\\..\\{_testDirectory.Replace( rootDirectory, "" )}\\bin\\test.dll";

            Directory.CreateDirectory( Path.GetDirectoryName( testFilename ) );

            File.WriteAllText( testFilename, "test" );

            FileManagementHelper.RenameActiveFile( testFilename );

            var expectedFilename = Path.Combine( _testDirectory, "test.dll.1.rdelete" );
            testFilename = Path.Combine( _testDirectory, "bin\\test.dll" );

            Assert.That.IsFalse( File.Exists( expectedFilename ) );
            Assert.That.IsTrue( File.Exists( testFilename ) );
        }

        [TestMethod]
        public void TryDelete_ShouldNotThrowNotFoundException()
        {
            var fileName = "test.txt";
            var testFilename = Path.Combine( _testDirectory, fileName );
            var testFile = File.CreateText( testFilename );

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( "TRUNCATE TABLE ExceptionLog" );
                Assert.AreEqual( 0, new ExceptionLogService( rockContext ).Queryable().Count() );
            }

            FileManagementHelper.TryDelete( testFilename, false );
            Assert.That.IsTrue( File.Exists( testFilename ) );
            testFile.Close();

            using ( var rockContext = new RockContext() )
            {
                while ( new ExceptionLogService( rockContext ).Queryable().Count() == 0 )
                {
                    Thread.Sleep( 500 );
                }
                Assert.AreEqual( 1, new ExceptionLogService( rockContext ).Queryable().Count() );
            }
        }

        [TestMethod]
        public void TryDelete_ShouldThrowNotFoundException()
        {
            var fileName = "test.txt";
            var testFilename = Path.Combine( _testDirectory, fileName );
            var testFile = File.CreateText( testFilename );

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( "TRUNCATE TABLE ExceptionLog" );
                Assert.AreEqual( 0, new ExceptionLogService( rockContext ).Queryable().Count() );
            }

            Assert.That.ThrowsException<IOException>( () => FileManagementHelper.TryDelete( testFilename, true ) );
            testFile.Close();


            using ( var rockContext = new RockContext() )
            {
                while ( new ExceptionLogService( rockContext ).Queryable().Count() == 0 )
                {
                    Thread.Sleep( 500 );
                }
                Assert.AreEqual( 1, new ExceptionLogService( rockContext ).Queryable().Count() );
            }
        }
    }
}
