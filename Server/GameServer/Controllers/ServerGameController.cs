namespace GameServer.Controllers
{
   using System;
   using System.IO;
   using System.Threading;
   using System.Threading.Tasks;
   using System.Timers;

   using GameServer.Models;
   using GameServer.Models.Internal;
   using GameServer.Models.Message.InitialMessages;
   using GameServer.Network;
   using GameServer.Serializer;

   public class ServerGameController
   {
      private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

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
         var dataController = new TeamDataController(homeTeamData, awayTeamData);
         dataController.TeamsAreReady += DataController_TeamsAreReady;
      }

      private void DataController_TeamsAreReady(object sender, EventArgs e)
      {
         SendAwayTeamDataToBothClient();

         OnMatchStarted();

         SendMatchStandingContinuously();

         Console.WriteLine("Continuous Overall Match Data message has been sent continuously.");
      }

      private void SetInitialPositionCollection()
      {
         for (var i = 0; i < 11; i++)
         {
            var rn = new Random(i);
            homeTeamData.PositionCollection.AddPosition(new Position { X = rn.Next(0, 500), Y = rn.Next(0, 500) });
         }

         for (var i = 0; i < 11; i++)
         {
            var rn = new Random(i);
            awayTeamData.PositionCollection.AddPosition(new Position { X = rn.Next(0, 500), Y = rn.Next(0, 500) });
         }
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
                  if (ProcessTeamMessage(packet, teamData1))
                  {
                     return;
                  }

                  break;

               case 0:
                  Console.WriteLine("Invalid packet.");

                  return;

               case 1 when (messageType == MessageType.Pitch) && (command == CommandType.Get):
                  ProcessPitchMessage(communicator, teamData1);

                  break;

               case 1:
                  Console.WriteLine("Invalid packet.");

                  return;
            }
         }
         else
         {
            switch (messageType)
            {
               case MessageType.PositionCollection when command == CommandType.ContinuousSet:
                  ProcessPositionCollectionMessage(packet, communicator);

                  break;
               case MessageType.BallPosition when command == CommandType.ContinuousSet:
                  ProcessBallMessage(packet);

                  break;
               default:

                  Console.WriteLine("Invalid/unknown packet.");

                  break;
            }
         }

         teamData1.MessageRead++;
      }

      private void SendOverallMatchData(ServerCommunicator communicator, CommandType commandType)
      {
         semaphore.Wait();

         Stream data = DataSerializer.CreateSerializedData(OverallMatchStandingCreator.Create(game, matchTimer,
            communicator != homeTeamCommunicator));

         PositionCollection test = DataSerializer.ReadSerializedData<OverallMatchStanding>(data).PositionCollection;

         PacketHeader header = PacketHeaderCreator.Create(commandType, MessageType.OverallMatchData, data.Length);
         communicator.SendDataAsPacket(new Packet(header, data));

         semaphore.Release();
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

      private void SendMatchStandingContinuously()
      {
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
      }

      private void SendAwayTeamDataToBothClient()
      {
         Stream homeTeamPacketBody = DataSerializer.CreateSerializedData(homeTeamData.Team);
         Stream awayTeamPacketBody = DataSerializer.CreateSerializedData(awayTeamData.Team);

         PacketHeader homeTeamPacketHeader = PacketHeaderCreator.Create(CommandType.Set, MessageType.Team, homeTeamPacketBody.Length);
         PacketHeader awayTeamPacketHeader = PacketHeaderCreator.Create(CommandType.Set, MessageType.Team, awayTeamPacketBody.Length);

         homeTeamCommunicator.SendDataAsPacket(new Packet(awayTeamPacketHeader, awayTeamPacketBody));
         awayTeamCommunicator.SendDataAsPacket(new Packet(homeTeamPacketHeader, homeTeamPacketBody));
      }

      private static bool ProcessTeamMessage(Packet packet, TeamData teamData1)
      {
         var teamDeserialized = DataSerializer.ReadSerializedData<Team>(packet.Data);
         if (teamDeserialized is null)
         {
            return true;
         }

         teamData1.Team = teamDeserialized;

         Console.WriteLine("Team setter message has been processed.");
         return false;
      }

      private void ProcessPitchMessage(ServerCommunicator communicator, TeamData teamData1)
      {
         Stream data = DataSerializer.CreateSerializedData(pitch);
         PacketHeader header = PacketHeaderCreator.Create(CommandType.Set, MessageType.Pitch, data.Length);

         Console.WriteLine("Pitch getter message has been processed.");

         communicator.SendDataAsPacket(new Packet(header, data));

         Console.WriteLine("Pitch setter message has been sent.");
         Console.WriteLine("Team is ready.");
         teamData1.TeamReady = true;
      }

      private void ProcessPositionCollectionMessage(Packet packet, ServerCommunicator communicator)
      {
         var collection = DataSerializer.ReadSerializedData<PositionCollection>(packet.Data);
         if (collection is null)
         {
            Console.WriteLine("The Position Collection message is invalid.");
            return;
         }

         // Store the Position Collection
         game.ProcessPositionCollection(collection, communicator == homeTeamCommunicator);

         Console.WriteLine("Position Collection message has been processed.");
      }

      private void ProcessBallMessage(Packet packet)
      {
         var readPosition = DataSerializer.ReadSerializedData<Position>(packet.Data);
         if (readPosition is null)
         {
            Console.WriteLine("The Ball Position message is invalid.");
            return;
         }

         // Set the Position of the Ball
         game.ProcessBallPosition(readPosition);

         Console.WriteLine("Ball Position message has been processed.");
      }
   }
}