using CRaft.Models;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace CRaft.Storage
{
    public class DiskStorage
    {
        private class StateMessage
        {
            public ulong? Term { get; }
            public string VotedFor { get; }

            public StateMessage(ulong? term, string votedFor)
            {
                Term = term;
                VotedFor = votedFor;
            }
        }

        private static ConcurrentQueue<StringEntry> entryQueue = new ConcurrentQueue<StringEntry>();
        private static ConcurrentQueue<StateMessage> stateQueue = new ConcurrentQueue<StateMessage>();
        private static readonly string storageFolderPath = Path.Join(
            "craft",
            "storage"
        );
        private static readonly string logStoragePath = Path.Join(
            storageFolderPath,
            "logs"
        );
        private static readonly string stateStoragePath = Path.Join(
            storageFolderPath,
            "state"
        );
        private static readonly string termStoragePath = Path.Join(
            stateStoragePath,
            "term"
        );
        private static readonly string votedForStoragePath = Path.Join(
            stateStoragePath,
            "votedFor"
        );
        private static Thread queueReaderThread = null;
        private static volatile bool isWriting = false;
        private static object threadObj = new object();

        public async Task<ulong> FetchCurrentTerm()
        {
            if (!File.Exists(termStoragePath)) return 0;
            return ulong.Parse(await File.ReadAllTextAsync(termStoragePath));
        }

        public async Task<string> FetchVotedFor()
        {
            if (!File.Exists(votedForStoragePath)) return string.Empty;
            return await File.ReadAllTextAsync(votedForStoragePath);
        }

        public async Task<StringEntry[]> FetchLogs()
        {
            if (!File.Exists(logStoragePath))
            {
                return new StringEntry[0];
            }

            
            string[] entryLogContents = await File.ReadAllLinesAsync(logStoragePath);
            return new List<string>(entryLogContents)
                .Select(s => JsonSerializer.Deserialize<StringEntry>(s))
                .ToArray();
        }

        public void UpdateCurrentTerm(ulong term)
        {
            stateQueue.Enqueue(new StateMessage(term, null));
        }

        public void UpdateVotedFor(string votedFor)
        {
            stateQueue.Enqueue(new StateMessage(null, votedFor));
        }

        public void AppendLogEntry(StringEntry entry)
        {
            lock (threadObj)
            {
                string entryJson = JsonSerializer.Serialize(entry);
                File.AppendAllText(logStoragePath, $"{entryJson}\n");
            }
        }

        private static void HandleApplicationException(object sender, UnhandledExceptionEventArgs e)
        {
            // TODO: something
        }

        public void Boot()
        {
            lock (threadObj)
            {
                if (queueReaderThread != null)
                {
                    return;
                }

                if (!Directory.Exists(storageFolderPath))
                {
                    Directory.CreateDirectory(storageFolderPath);
                }

                isWriting = true;
                queueReaderThread = new Thread(ReadPersistQueue);
                queueReaderThread.Start();
                AppDomain.CurrentDomain.UnhandledException -= HandleApplicationException;
                AppDomain.CurrentDomain.UnhandledException += HandleApplicationException;
            }
        }

        public void ShutDown()
        {
            lock (threadObj)
            {
                if (queueReaderThread == null)
                {
                    return;
                }

                isWriting = false;
                queueReaderThread.Join();
                queueReaderThread = null;
                AppDomain.CurrentDomain.UnhandledException -= HandleApplicationException;
            }
        }

        private void ReadPersistQueue()
        {
            while (isWriting)
            {
                if (entryQueue.TryDequeue(out StringEntry entry))
                {
                    AppendEntryInternal(entry);
                }
                if (stateQueue.TryDequeue(out StateMessage message))
                {
                    AppendStateInternal(message);
                }
            }
        }

        private void AppendEntryInternal(StringEntry entry)
        {
            string entryJson = JsonSerializer.Serialize(entry);
            File.AppendAllText(logStoragePath, $"{entryJson}\n");
        }

        private void AppendStateInternal(StateMessage message)
        {
            if (message.Term != null)
            {
                File.WriteAllText(termStoragePath, message.Term.ToString());
            }
            if (!string.IsNullOrWhiteSpace(message.VotedFor))
            {
                File.WriteAllText(votedForStoragePath, message.VotedFor);
            }
        }
    }
}
