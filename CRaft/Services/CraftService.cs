using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRaft
{
    public class CraftService : Craft.CraftBase
    {
        private readonly ILogger<CraftService> _logger;
        public CraftService(ILogger<CraftService> logger)
        {
            _logger = logger;
        }

        public override Task<AppendEntriesReply> AppendEntries(AppendEntriesRequest request, ServerCallContext context)
        {
            _logger.LogInformation("AppendEntries request {request}", request);
            return Task.FromResult(new AppendEntriesReply
            {
                Success = false,
                Term = 0L,
            });
        }

        public override Task<RequestVoteReply> RequestVote(RequestVoteRequest request, ServerCallContext context)
        {
            _logger.LogInformation("RequestVote request {request}", request);
            return Task.FromResult(new RequestVoteReply
            {
                Term = 0L,
                VoteGranted = false,
            });
        }

        public override Task<InstallSnapshotReply> InstallSnapshot(InstallSnapshotRequest request, ServerCallContext context)
        {
            _logger.LogInformation("InstallSnapshot request {request}", request);
            return Task.FromResult(new InstallSnapshotReply
            {
                Term = 0L,
            });
        }

        public override Task<SaveEntryReply> SaveEntry(SaveEntryRequest request, ServerCallContext context)
        {
            _logger.LogInformation("SaveEntry request {request}", request);
            return Task.FromResult(new SaveEntryReply
            {
                LeaderUri = "",
                Reason = "",
                Saved = true,
            });
        }

        public override Task<PeerArrivedReply> PeerArrived(PeerArrivedRequest request, ServerCallContext context)
        {
            _logger.LogInformation("PeerArrived request {request}", request);

            var stored = State.NodeState.AddPeer(new State.NodeState.Peer(request.PeerId, request.PeerUri));

            return Task.FromResult(new PeerArrivedReply
            {
                Stored = stored,
            });
        }

        public override Task<GetNodeIdReply> GetNodeId(GetNodeIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("GetNodeId request {request}", request);
            return Task.FromResult(new GetNodeIdReply
            {
                NodeId = State.NodeState.NodeId,
            });
        }
    }
}
