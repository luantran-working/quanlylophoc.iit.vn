using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using ClassroomManagement.Models;

namespace ClassroomManagement.Services
{
    public class PollService
    {
        private static PollService? _instance;
        public static PollService Instance => _instance ??= new PollService();

        // Server State
        public Poll? CurrentPoll { get; private set; }
        private readonly ConcurrentDictionary<string, int> _votes = new(); // MachineId -> OptionId
        private readonly object _lock = new object();

        // Client Events
        public event EventHandler<Poll>? PollStarted;
        public event EventHandler<PollResultUpdate>? PollUpdated;
        public event EventHandler? PollStopped;

        // Server Methods
        public async Task StartPollAsync(Poll poll)
        {
            CurrentPoll = poll;
            CurrentPoll.IsActive = true;
            _votes.Clear();

            var msg = new NetworkMessage
            {
                Type = MessageType.PollStart,
                SenderId = "server",
                Payload = JsonSerializer.Serialize(poll)
            };

            await SessionManager.Instance.NetworkServer.BroadcastToAllAsync(msg);
        }

        public async Task StopPollAsync()
        {
            if (CurrentPoll != null)
            {
                CurrentPoll.IsActive = false;
                await SessionManager.Instance.NetworkServer.BroadcastToAllAsync(new NetworkMessage
                {
                    Type = MessageType.PollStop,
                    SenderId = "server"
                });
                CurrentPoll = null;
            }
        }

        public async Task ProcessVoteAsync(PollVote vote)
        {
            if (CurrentPoll == null || !CurrentPoll.IsActive) return;
            if (vote.PollId != CurrentPoll.Id) return;

            // Record vote (can allow change vote or one time only - let's allow overwrite)
            _votes.AddOrUpdate(vote.StudentMachineId, vote.OptionId, (k, v) => vote.OptionId);

            // Calculate results
            var result = new PollResultUpdate
            {
                PollId = CurrentPoll.Id,
                TotalVotes = _votes.Count,
                VoteCounts = new Dictionary<int, int>()
            };

            foreach (var opt in CurrentPoll.Options)
            {
                result.VoteCounts[opt.Id] = _votes.Values.Count(v => v == opt.Id);
            }

            // Broadcast Update
            var msg = new NetworkMessage
            {
                Type = MessageType.PollUpdate,
                SenderId = "server",
                Payload = JsonSerializer.Serialize(result)
            };

            await SessionManager.Instance.NetworkServer.BroadcastToAllAsync(msg);

            // Notify local (Teacher UI)
            PollUpdated?.Invoke(this, result);
        }

        // Client Methods
        public void HandlePollStart(Poll poll)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentPoll = poll;
                PollStarted?.Invoke(this, poll);
            });
        }

        public void HandlePollUpdate(PollResultUpdate update)
        {
             Application.Current.Dispatcher.Invoke(() =>
            {
                PollUpdated?.Invoke(this, update);
            });
        }

        public void HandlePollStop()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentPoll = null;
                PollStopped?.Invoke(this, EventArgs.Empty);
            });
        }

        // Client Reference
        private NetworkClientService? _networkClient;

        public void InitializeClient(NetworkClientService client)
        {
            _networkClient = client;
        }

        public async Task SubmitVoteAsync(int optionId)
        {
            if (CurrentPoll == null) return;

            var vote = new PollVote
            {
                PollId = CurrentPoll.Id,
                OptionId = optionId,
                StudentMachineId = _networkClient?.MachineId ?? ""
            };

            // Client sends to server
            if (_networkClient != null)
            {
                await _networkClient.SendMessageAsync(new NetworkMessage
                {
                    Type = MessageType.PollVote,
                    SenderId = _networkClient.MachineId,
                    Payload = JsonSerializer.Serialize(vote)
                });
            }
        }
    }
}
