using CRaft.Models;
using CRaft.Storage;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CRaft.Tests.Storage
{
    [TestFixture]
    public class DiskStorageTests
    {
        private DiskStorage diskStorage = new DiskStorage();

        [SetUp]
        public void SetUp()
        {
            diskStorage.Boot();
        }

        [TearDown]
        public void TearDown()
        {
            diskStorage.ShutDown();
            // Delete everything so far stored
            Directory.Delete(Path.Join(Directory.GetCurrentDirectory(), "craft"), true);
        }

        [Test]
        public async Task StoreEntry()
        {
            StringEntry entry = new StringEntry(1, 1, "hello");
            diskStorage.AppendLogEntry(entry);

            StringEntry[] storedEntries = await diskStorage.FetchLogs();
            Assert.AreEqual(1, storedEntries.Length);
            Assert.AreEqual(entry.Term, storedEntries[0].Term);
            Assert.AreEqual(entry.Index, storedEntries[0].Index);
            Assert.AreEqual(entry.Data, storedEntries[0].Data);
        }

        [Test]
        public async Task StoreManyEntries()
        {
            for (int i = 0; i < 1000; i++)
            {
                var entry = new StringEntry((ulong)i + 1, (ulong)i + 1, (i + 1).ToString());
                diskStorage.AppendLogEntry(entry);
            }

            // We need to make sure the async file writes complete.
            System.Threading.Thread.Sleep(50);

            StringEntry[] storedEntries = await diskStorage.FetchLogs();
            Assert.AreEqual(1000, storedEntries.Length);
        }

        [Test]
        public async Task StoreManyEntriesFromDifferentThreads()
        {
            void CreateEntriesLocal(ulong startingIndex, ulong endingIndex)
            {
                for (ulong i = startingIndex; i < endingIndex; i++)
                {
                    var entry = new StringEntry(i + 1, i + 1, (i + 1).ToString());
                    diskStorage.AppendLogEntry(entry);
                }
            };

            var thread1 = new Thread(() => CreateEntriesLocal(0, 1000));
            var thread2 = new Thread(() => CreateEntriesLocal(1000, 2000));
            var thread3 = new Thread(() => CreateEntriesLocal(2000, 3000));
            
            thread1.Start();
            thread2.Start();
            thread3.Start();

            thread1.Join();
            thread2.Join();
            thread3.Join();

            StringEntry[] storedEntries = await diskStorage.FetchLogs();
            Assert.AreEqual(3000, storedEntries.Length);
        }
    }
}
