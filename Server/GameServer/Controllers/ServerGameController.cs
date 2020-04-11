namespace GameServer.Controllers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Timers;

    
    using GameServer.Models;
    
    using GameServer.Network;
    using GameServer.Serializer;

    public class ServerGameController
    {
        private readonly object syncObject = new object();

        private readonly MatchTimer matchTimer;
        private readonly Random random = new Random(2576);

        private ServerCommunicator homeTeamCommunicator;
        private ServerCommunicator awayTeamCommunicator;

        private Game game;

        public ServerGameController()
        {
            int matchTime = random.Next(60, 120) * 1000;
            matchTimer = new MatchTimer(matchTime);

            matchStanding = new OverallMatchStanding { TimeElapsed = new MatchTime { Time = matchTime } };
            matchTimer.TimeElapsed += MatchTimer_Elapsed;

            // The size of the Pith is fixed
            pitch = new Pitch { Width = 500, Height = 500 };

            homeTeamData = new TeamData();
            awayTeamData = new TeamData();
            dataController = new TeamDataController(homeTeamData, awayTeamData);
            dataController.TeamsAreReady += DataController_TeamsAreReady;
        }

        private void DataController_TeamsAreReady(object sender, EventArgs e)
        {
            #region Send Away Team data to both client

            Stream homeTeamPacketBody = DataSerializer.CreateSerializedData(homeTeamData.Team);
            Stream awayTeamPacketBody = DataSerializer.CreateSerializedData(awayTeamData.Team);

            PacketHeader homeTeamPacketHeader = PacketHeaderCreator.Create(CommandType.Set, MessageType.Team, homeTeamPacketBody.Length);
            PacketHeader awayTeamPacketHeader = PacketHeaderCreator.Create(CommandType.Set, MessageType.Team, awayTeamPacketBody.Length);

            homeTeamCommunicator.SendDataAsPacket(new Packet(awayTeamPacketHeader, awayTeamPacketBody));
            awayTeamCommunicator.SendDataAsPacket(new Packet(homeTeamPacketHeader, homeTeamPacketBody));

            #endregion

            OnMatchStarted();

            #region Send Match Standing continuously

            Task.Factory.StartNew(() =>
            {
                SetInitialPositionCollection();
                while (!homeTeamData.DataSendingStopRequest && !awayTeamData.DataSendingStopRequest)
                {
                    SendOverallMatchData(homeTeamCommunicator, CommandType.ContinuousSet);
                    SendOverallMatchData(awayTeamCommunicator, CommandType.ContinuousSet);
                }

                Console.WriteLine("Send is over.");
            }, TaskCreationOptions.LongRunning);

            Console.WriteLine("Continuous Overall Match Data message has been sent continuously."); 

            #endregion
        }

        private void SetInitialPositionCollection()
        {
            
        }

        public void SetHomeTeamCommunicator(ServerCommunicator homeTeamCommunicator)
        {
            this.homeTeamCommunicator = homeTeamCommunicator ?? throw new ArgumentNullException(nameof(homeTeamCommunicator));
            this.homeTeamCommunicator.DataReceived += HomeTeamCommunicator_DataReceived;
            this.homeTeamCommunicator.StartCommunication();
        }
        public void SetAwayTeamCommunicator(ServerCommunicator awayTeamCommunicator)
        {
            this.awayTeamCommunicator = awayTeamCommunicator ?? throw new ArgumentNullException(nameof(awayTeamCommunicator));
            this.awayTeamCommunicator.DataReceived += AwayTeamCommunicator_DataReceived;
            this.awayTeamCommunicator.StartCommunication();
        }

        private void MatchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Send the match result at the end of the Match to both TEAM (Client)

            awayTeamData.DataSendingStopRequest = true;
            homeTeamData.DataSendingStopRequest = true;

            MatchStanding standing = MatchStandingCreator.Create(game);
            Stream standingAsStream = DataSerializer.CreateSerializedData(standing);

            PacketHeader header = PacketHeaderCreator.Create(CommandType.Set, MessageType.MatchResult, standingAsStream.Length);
            homeTeamCommunicator.SendDataAsPacket(new Packet(header, standingAsStream));

            PacketHeader awayHeader = PacketHeaderCreator.Create(CommandType.Set, MessageType.MatchResult, standingAsStream.Length);
            awayTeamCommunicator.SendDataAsPacket(new Packet(awayHeader, standingAsStream));
        }

        private void AwayTeamCommunicator_DataReceived(object sender, DataReceivedEventArgs<Packet> e)
        {
            ProcessClientPacket(e.Data, awayTeamCommunicator, awayTeamData, homeTeamData);
        }
        private void HomeTeamCommunicator_DataReceived(object sender, DataReceivedEventArgs<Packet> e)
        {
            ProcessClientPacket(e.Data, homeTeamCommunicator, homeTeamData, awayTeamData);
        }

        private readonly Pitch pitch;
        private readonly OverallMatchStanding matchStanding;

        private readonly TeamData homeTeamData;
        private readonly TeamData awayTeamData;
        private readonly TeamDataController dataController;

        private void ProcessClientPacket(Packet packet, ServerCommunicator communicator, TeamData teamData1, TeamData teamData2)
        {
            var messageType = (MessageType)packet.Header.MessageType;
            var command = (CommandType)packet.Header.Command;

            // If the first three initialization messages have not been processed yet
            if (teamData1.MessageRead < 2)
            {
                switch (teamData1.MessageRead)
                {
                    case 0 when (messageType == MessageType.Team) && (command == CommandType.Set):
                        var teamDeserialized = DataSerializer.ReadSerializedData<Team>(packet.Data);
                        if (teamDeserialized is null)
                        {
                            // Do not increment the MessageRead field
                            // Next time the server still be waiting for this message type
                            return;
                        }

                        teamData1.Team = teamDeserialized;

                        Console.WriteLine("Team setter message has been processed.");

                        break;

                    case 0:
                        Console.WriteLine("Invalid packet.");
                        // Do not increment the MessageRead field
                        // Next time the server still be waiting for this message type
                        return;

                    case 1 when (messageType == MessageType.Pitch) && (command == CommandType.Get):
                        Stream data = DataSerializer.CreateSerializedData(pitch);
                        PacketHeader header = PacketHeaderCreator.Create(CommandType.Set, MessageType.Pitch, data.Length);

                        Console.WriteLine("Pitch getter message has been processed.");

                        communicator.SendDataAsPacket(new Packet(header, data));

                        Console.WriteLine("Pitch setter message has been sent.");
                        Console.WriteLine("Team is ready.");
                        teamData1.TeamReady = true;

                        break;
                    
                    case 1:
                        Console.WriteLine("Invalid packet.");
                        // Do not increment the MessageRead field
                        // Next time the server still be waiting for this message type
                        return;
                }
            }
            else
            {
                switch (messageType)
                {
                    case MessageType.PositionCollection when command == CommandType.ContinuousSet:
                        var collection = DataSerializer.ReadSerializedData<PositionCollection>(packet.Data);
                        if (collection is null)
                        {
                            Console.WriteLine("The Position Collection message is invalid.");
                            break;
                        }

                        // Store the Position Collection
                        game.ProcessPositionCollection(collection, communicator == homeTeamCommunicator);

                        Console.WriteLine("Position Collection message has been processed.");

                        break;

                    case MessageType.BallPosition when command == CommandType.ContinuousSet:
                        var readPosition = DataSerializer.ReadSerializedData<Position>(packet.Data);
                        if (readPosition is null)
                        {
                            Console.WriteLine("The Ball Position message is invalid.");
                            break;
                        }

                        // Set the Position of the Ball
                        game.ProcessBallPosition(readPosition);

                        Console.WriteLine("Ball Position message has been processed.");

                        break;

                    //case MessageType.OverallMatchData when command == CommandType.Get:
                    //    Console.WriteLine("Overall Match Data after message initialization has been processed.");

                    //    SendOverallMatchData(communicator, CommandType.Set);

                    //    Console.WriteLine("Overall Match Data message after initialization has been sent.");

                    //    break;

                    default:

                        Console.WriteLine("Invalid/unknown packet.");

                        break;
                }
            }

            teamData1.MessageRead++;
        }

        private void SendOverallMatchData(ServerCommunicator communicator, CommandType commandType)
        {
            lock (syncObject)
            {
                Stream data = DataSerializer.CreateSerializedData(OverallMatchStandingCreator.Create(game, matchTimer,
                    communicator != homeTeamCommunicator));

                PositionCollection test = DataSerializer.ReadSerializedData<OverallMatchStanding>(data).PositionCollection;

                PacketHeader header = PacketHeaderCreator.Create(commandType, MessageType.OverallMatchData, data.Length);
                communicator.SendDataAsPacket(new Packet(header, data));
            }
        }

        public event EventHandler MatchStarted;

        protected virtual void OnMatchStarted()
        {
            // Initialize the Game itself
            game = new Game(pitch, homeTeamData.Team, awayTeamData.Team, matchStanding.TimeElapsed);
            // When the match starts
            // Start the timer as well
            matchTimer.Start();
            MatchStarted?.Invoke(this, EventArgs.Empty);
        }
    }
}
