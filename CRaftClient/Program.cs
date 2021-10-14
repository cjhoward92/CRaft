using CRaft;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;

namespace CRaftClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // Command to run server
            // dotnet run -p CRaft -- --port=500X
            Console.WriteLine("We are starting the CRaft Clients");

            var ports = new int[] { 5001, 5002, 5003 };
            var portClients = new Dictionary<string, (int, Craft.CraftClient)>();

            foreach (var port in ports)
            {
                var channel = GrpcChannel.ForAddress($"https://localhost:{port}");
                var client = new Craft.CraftClient(channel);
                var reply = client.GetNodeId(new GetNodeIdRequest());
                portClients.Add(reply.NodeId, (port, client));
            }

            foreach (var portClientPair in portClients)
            {
                var mainId = portClientPair.Key;
                var mainPort = portClientPair.Value.Item1;
                var mainClient = portClientPair.Value.Item2;
                foreach (var subPair in portClients)
                {
                    var subId = subPair.Key;
                    var subPort = subPair.Value.Item1;

                    if (mainId == subPair.Key)
                    {
                        continue;
                    }

                    var reply = mainClient.PeerArrived(new PeerArrivedRequest
                    {
                        PeerId = subId,
                        PeerUri = $"https://localhost:{subPort}",
                    });
                    Console.WriteLine($"Reply when adding peer: {reply.Stored}");
                }
            }

            //RequestVoteReply reply = client.RequestVote(new RequestVoteRequest
            //{
            //    CandidateId = "test",
            //    LastLogIndex = 1,
            //    LastLogTerm = 2,
            //    Term = 3,
            //});

            // Console.WriteLine($"Term {reply.Term} granted {reply.VoteGranted}");
        }
    }
}
