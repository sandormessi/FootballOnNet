namespace Client.Controller
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using Client.Models;
    using Client.Models.Message.InitialMessages;
    using Client.Network;
    using Client.Serializer;

    public class ClientGameController
    {
        private bool matchStarted;
        private ulong messageRead;
        private readonly ServerCommunicator communicator;
        public Team HomeTeam { get;}

        public ClientGameController(ServerCommunicator communicator, Team team)
        {
            this.communicator = communicator ?? throw new ArgumentNullException(nameof(communicator));
            HomeTeam = team ?? throw new ArgumentNullException(nameof(team));
            this.communicator.DataReceived += Communicator_DataReceived;
            this.communicator.StartCommunication();
        }

        private void Communicator_DataReceived(object sender, DataReceivedEventArgs<Packet> e)
        {
            ProcessServerData(e.Data);
        }

        public Pitch Pitch { get; private set; }
        public Team AwayTeam { get; private set; }

        public Position BallPosition { get; set; } = new Position { X = 50, Y = 50 };

        public PositionCollection HomePositionCollection
        {
            get
            {
                return homePositionCollection;
            }
            set
            {
                Stream data = DataSerializer.CreateSerializedData(value);
                PacketHeader teamHeader = PacketHeaderCreator.Create(CommandType.ContinuousSet, MessageType.PositionCollection, data.Length);
                communicator.SendDataAsPacket(new Packet(teamHeader, data));
                homePositionCollection = value;
            }
        }

        public PositionCollection AwayPositionCollection { get; }

        public OverallMatchStanding OverallMatchStanding { get; private set; }
        public MatchStanding MatchResult;

        private PositionCollection homePositionCollection;

        private void ProcessServerData(Packet data)
        {
            var messageType = (MessageType)data.Header.MessageType;
            var command = (CommandType)data.Header.Command;

            if (messageRead < 2)
            {
                if (messageRead == 0)
                {
                   if (ProcessPitchData(data, messageType, command))
                   {
                      return;
                   }
                }
                else if (messageRead == 1)
                {
                   if (ProcessTeamData(data, messageType, command))
                   {
                      return;
                   }
                }
            }
            // Continuous match result
            else if((messageType == MessageType.OverallMatchData) && (command == CommandType.ContinuousSet))
            {
               ProcessOverallMatchData(data);
            }
            // End of match
            else if ((messageType == MessageType.MatchResult) && (command == CommandType.Set))
            {
               ProcessMatchResult(data);
            }

            messageRead++;
        }

        public void Stop()
        {
            communicator.Stop();
        }

        public void Initialize()
        {
            // Send Team
            Stream streamData = DataSerializer.CreateSerializedData(HomeTeam);
            PacketHeader header = PacketHeaderCreator.Create(CommandType.Set, MessageType.Team, streamData.Length);
            communicator.SendDataAsPacket(new Packet(header, streamData));

            // Send Pitch getter
            Stream pitchStreamData = DataSerializer.CreateSerializedData(HomeTeam);
            PacketHeader pitchHeader = PacketHeaderCreator.Create(CommandType.Get, MessageType.Pitch, pitchStreamData.Length);
            communicator.SendDataAsPacket(new Packet(pitchHeader, pitchStreamData));
        }

        public event EventHandler MatchStarted;
        public event EventHandler PositionCollectionReceived;
        public event EventHandler MatchOver;
        public event EventHandler OverallMatchStandingReceived;

        protected virtual void OnMatchStarted()
        {
            MatchStarted?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPositionCollectionReceived()
        {
            PositionCollectionReceived?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnMatchOver()
        {
            MatchOver?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnOverallMatchStandingReceived()
        {
            OverallMatchStandingReceived?.Invoke(this, EventArgs.Empty);
        }

        private void ProcessMatchResult(Packet data)
        {
           MatchResult = DataSerializer.ReadSerializedData<MatchStanding>(data.Data);

           Debug.WriteLine($"Final result : {MatchResult.HomeGoals}:{MatchResult.AwayGoals}");
           communicator.Stop();
           OnMatchOver();
        }

        private void ProcessOverallMatchData(Packet data)
        {
           if (!matchStarted)
           {
              OnMatchStarted();
           }

           OverallMatchStanding = DataSerializer.ReadSerializedData<OverallMatchStanding>(data.Data);
           matchStarted = true;
           OnOverallMatchStandingReceived();
           Debug.WriteLine($"Standing: {OverallMatchStanding.Standing.HomeGoals}:{OverallMatchStanding.Standing.AwayGoals}");
        }

        private bool ProcessTeamData(Packet data, MessageType messageType, CommandType command)
        {
           if ((messageType == MessageType.Team) && (command == CommandType.Set))
           {
              AwayTeam = DataSerializer.ReadSerializedData<Team>(data.Data);
              Debug.WriteLine("Team data has been processed.");
           }
           else
           {
              return true;
           }

           return false;
        }

        private bool ProcessPitchData(Packet data, MessageType messageType, CommandType command)
        {
           if ((messageType == MessageType.Pitch) && (command == CommandType.Set))
           {
              Pitch = DataSerializer.ReadSerializedData<Pitch>(data.Data);
              Debug.WriteLine("Pitch data has been processed.");
           }
           else
           {
              return true;
           }

           return false;
        }
   }
}
