using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace CRaft.State
{
    public enum NodeStatus
    {
        Follower,
        Candidate,
        Leader
    }

    public static class NodeState
    {
        private static readonly object threadObj = new object();

        private static string id = Guid.NewGuid().ToString();

        private static NodeStatus status = NodeStatus.Follower;
        private static int electionTimeoutMs = new Random().Next(150, 300);

        private static string leaderId = null;
        private static ulong currentTerm = 0;
        private static string votedFor = null;
        private static IList<Entry> logs = new List<Entry>();
        private static ulong commitIndex = 0;
        private static ulong lastApplied = 0;
        private static ConcurrentDictionary<string, ulong> nextIndexes = new ConcurrentDictionary<string, ulong>();
        private static ConcurrentDictionary<string, ulong> matchIndexes = new ConcurrentDictionary<string, ulong>();
        private static IDictionary<string, Peer> peers = new Dictionary<string, Peer>();

        public static string NodeId => id;

        public static ulong CurrentTerm
        {
            get
            {
                lock (threadObj)
                {
                    return currentTerm;
                }
            }
            set
            {
                lock (threadObj)
                {
                    currentTerm = value;
                }
            }
        }

        public static ulong CommitIndex
        {
            get
            {
                lock (threadObj)
                {
                    return commitIndex;
                }
            }
            set
            {
                lock (threadObj)
                {
                    commitIndex = value;
                }
            }
        }

        public static ulong LastApplied
        {
            get
            {
                lock (threadObj)
                {
                    return lastApplied;
                }
            }
            set
            {
                lock (threadObj)
                {
                    lastApplied = value;
                }
            }
        }

        public static string VotedFor
        {
            get
            {
                lock (threadObj)
                {
                    return votedFor;
                }
            }
            set
            {
                lock (threadObj)
                {
                    votedFor = value;
                }
            }
        }

        public static string LeaderId
        {
            get
            {
                lock (threadObj)
                {
                    return leaderId;
                }
            }
            set
            {
                lock (threadObj)
                {
                    leaderId = value;
                }
            }
        }

        public static IList<Entry> Logs
        {
            get
            {
                lock (threadObj)
                {
                    return logs;
                }
            }
        }

        public static void AddEntry(Entry entry)
        {
            lock (threadObj)
            {
                logs.Add(entry);
            }
        }

        public static ulong GetNextIndex(string serverId) => nextIndexes.GetValueOrDefault(serverId);

        public static void UpdateNextIndex(string serverId)
        {
            nextIndexes.AddOrUpdate(serverId, (ulong)Logs.Count + 1, (serverId, curVal) =>
            {
                if (curVal > (ulong)Logs.Count + 1) return curVal;
                return (ulong)Logs.Count + 1;
            }); 
        }

        public static ulong GetMatchIndex(string serverId) => matchIndexes.GetValueOrDefault(serverId);

        public static void UpdateMatchIndex(string serverId, ulong lastSent)
        {
            matchIndexes.AddOrUpdate(serverId, lastSent, (serverId, curVal) =>
            {
                if (curVal > lastSent) return curVal;
                return lastSent;
            });
        }

        public static bool AddPeer(Peer peer)
        {
            lock (threadObj)
            {
                return peers.TryAdd(peer.PeerId, peer);
            }
        }

        public static bool RemovePeer(Peer peer)
        {
            lock (threadObj)
            {
                return peers.Remove(peer.PeerId);
            }
        }

        public class Entry
        {
            public ulong Term { get; private set; }
            public ulong Index { get; private set; }
            public string Data { get; private set; }

            public Entry(ulong term, ulong index, string data)
            {
                Term = term;
                Index = index;
                Data = data;
            }
        }

        public class Peer
        {
            public string PeerId { get; private set; }
            public string PeerUri { get; private set; }

            public Peer(string peerId, string peerUri)
            {
                PeerId = peerId;
                PeerUri = peerUri;
            }
        }
    }
}
