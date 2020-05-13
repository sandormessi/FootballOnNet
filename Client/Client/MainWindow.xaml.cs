namespace Client
{
   using System;
   using System.Collections.Generic;
   using System.IO;
   using System.Linq;
   using System.Net;
   using System.Net.Sockets;
   using System.Threading;
   using System.Threading.Tasks;
   using System.Windows;
   using System.Windows.Controls;

   using Client.Controller;
   using Client.Models;
   using Client.Models.Message.InitialMessages;
   using Client.Network;
   using Client.Serializer;

   /// <summary>Interaction logic for MainWindow.xaml.</summary>
   public partial class MainWindow
   {
      private readonly TcpClient client;

      private readonly IPEndPoint endPoint;

      private readonly ServerCommunicator communicator;

      private readonly Team team = new Team
      {
         // The players int THIS TEAM
         Players = GetPlayers()
      };

      private readonly ClientGameController controller;

      private static List<Player> GetPlayers()
      {
         return new List<Player>
         {
            new Player { Id = Guid.NewGuid(), Name = "Player1" },
            new Player { Id = Guid.NewGuid(), Name = "Player2" },
            new Player { Id = Guid.NewGuid(), Name = "Player3" },
            new Player { Id = Guid.NewGuid(), Name = "Player4" },
            new Player { Id = Guid.NewGuid(), Name = "Player5" },
            new Player { Id = Guid.NewGuid(), Name = "Player6" },
            new Player { Id = Guid.NewGuid(), Name = "Player7" },
            new Player { Id = Guid.NewGuid(), Name = "Player8" },
            new Player { Id = Guid.NewGuid(), Name = "Player9" },
            new Player { Id = Guid.NewGuid(), Name = "Player10" },
            new Player { Id = Guid.NewGuid(), Name = "Player11" }
         };
      }

      public MainWindow()
      {
         InitializeComponent();

         endPoint = new IPEndPoint(IPAddress.Loopback, 55555);

         client = new TcpClient { ReceiveBufferSize = Convert.ToInt32(Math.Pow(2, 16)) };
         client.Connect(endPoint);

         communicator = new ServerCommunicator(client);
         controller = new ClientGameController(communicator, team);

         SubscribeForEvents();
      }

      private void Controller_MatchOver(object sender, EventArgs e)
      {
         info.Text = $"Final result: {controller.MatchResult.HomeGoals}:{controller.MatchResult.AwayGoals}";
      }

      private async void OverallMatchStandingReceived(object sender, EventArgs e)
      {
         PositionCollection awayPositionCollection = controller.OverallMatchStanding.PositionCollection;

         for (var i = 0; i < controller.AwayTeam.Players.Count; i++)
         {
            Player homePlayer = controller.AwayTeam.Players[i];
            Position positionOfPlayer = awayPositionCollection.Positions.FirstOrDefault(x => x.Id == homePlayer.Id);
            if (positionOfPlayer is null)
            {
               continue;
            }

            await MovePlayer(i, positionOfPlayer);
         }

         await MoveTheBall();

         Thread.Sleep(100);

         await footballPitch.Dispatcher.InvokeAsync(() => { timeTable.Text = $"Time: {controller.OverallMatchStanding.TimeElapsed.Time}"; }).Task;
      }

      private async Task MoveTheBall()
      {
         await footballPitch.Dispatcher.InvokeAsync(() =>
         {
            // Sets the ball's position on the pitch
            UIElement ball = footballPitch.Children.Cast<UIElement>().Last();
            Canvas.SetLeft(ball, controller.OverallMatchStanding.BallPosition.X);
            Canvas.SetTop(ball, controller.OverallMatchStanding.BallPosition.Y);
         }).Task;
      }

      private void SubscribeForEvents()
      {
         controller.MatchStarted += Controller_MatchStarted;
         controller.OverallMatchStandingReceived += OverallMatchStandingReceived;
         controller.MatchOver += Controller_MatchOver;
      }

      private async Task MovePlayer(int i, Position positionOfPlayer)
      {
         await footballPitch.Dispatcher.InvokeAsync(() =>
         {
            UIElement dot = footballPitch.Children[i];
            Canvas.SetLeft(dot, positionOfPlayer.X);
            Canvas.SetTop(dot, positionOfPlayer.Y);
         }).Task;
      }

      private void Controller_MatchStarted(object sender, EventArgs e)
      {
         footballPitch.Dispatcher.Invoke(() =>
         {
            footballPitch.Width = controller.Pitch.Width;
            footballPitch.Height = controller.Pitch.Height;
         });

         var initialPlayerPositions = new PositionCollection();

         CreateInitialPlayers(initialPlayerPositions);

         controller.HomePositionCollection = initialPlayerPositions;

         Task.Factory.StartNew(() =>
         {
            while (true)
            {
               SetHomeTeamPositions();
               Stream data = DataSerializer.CreateSerializedData(controller.HomePositionCollection);
               PacketHeader header = PacketHeaderCreator.Create(CommandType.ContinuousSet, MessageType.PositionCollection, data.Length);

               communicator.SendDataAsPacket(new Packet(header, data));

               Stream ballData = DataSerializer.CreateSerializedData(controller.BallPosition);
               PacketHeader ballHeader = PacketHeaderCreator.Create(CommandType.ContinuousSet, MessageType.BallPosition, ballData.Length);

               communicator.SendDataAsPacket(new Packet(ballHeader, ballData));

               Thread.Sleep(25);
            }
         }, TaskCreationOptions.LongRunning).ConfigureAwait(false);
      }

      private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
      {
         controller.Initialize();
      }

      private async void SetHomeTeamPositions()
      {
         MoveAwayTeam();

         MoveHomeTeam();

         controller.BallPosition.X++;
         controller.BallPosition.Y++;

         // Then move the players on the GUI
         await MoveHomePlayers();
      }

      private async Task MoveHomePlayers()
      {
         // When the position of home players are set
         // then move them on the GUI as well
         for (var i = 0; i < controller.AwayTeam.Players.Count; i++)
         {
            Player homePlayer = controller.AwayTeam.Players[i];
            Position positionOfPlayer = controller.HomePositionCollection.Positions.FirstOrDefault(x => x.Id == homePlayer.Id);
            if (positionOfPlayer is null)
            {
               continue;
            }

            await MoveBall(i, positionOfPlayer);
         }
      }

      private void CreateInitialPlayers(PositionCollection initialPlayerPositions)
      {
         initialPlayerPositions.Positions.Add(new Position { Id = controller.HomeTeam.Players[0].Id, X = 0, Y = 250 });
         initialPlayerPositions.Positions.Add(new Position { Id = controller.HomeTeam.Players[1].Id, X = 50, Y = 50 });
         initialPlayerPositions.Positions.Add(new Position { Id = controller.HomeTeam.Players[2].Id, X = 50, Y = 100 });
         initialPlayerPositions.Positions.Add(new Position { Id = controller.HomeTeam.Players[3].Id, X = 50, Y = 150 });
         initialPlayerPositions.Positions.Add(new Position { Id = controller.HomeTeam.Players[4].Id, X = 50, Y = 200 });
         initialPlayerPositions.Positions.Add(new Position { Id = controller.HomeTeam.Players[5].Id, X = 100, Y = 50 });
         initialPlayerPositions.Positions.Add(new Position { Id = controller.HomeTeam.Players[6].Id, X = 100, Y = 100 });
         initialPlayerPositions.Positions.Add(new Position { Id = controller.HomeTeam.Players[7].Id, X = 100, Y = 150 });
         initialPlayerPositions.Positions.Add(new Position { Id = controller.HomeTeam.Players[8].Id, X = 150, Y = 200 });
         initialPlayerPositions.Positions.Add(new Position { Id = controller.HomeTeam.Players[9].Id, X = 150, Y = 50 });
         initialPlayerPositions.Positions.Add(new Position { Id = controller.HomeTeam.Players[10].Id, X = 150, Y = 150 });
      }

      private async Task MoveBall(int i, Position positionOfPlayer)
      {
         await footballPitch.Dispatcher.InvokeAsync(() =>
         {
            // The dots in the pitch from 11 to 21 are the home team dots
            UIElement dot = footballPitch.Children[i + 11];
            Canvas.SetLeft(dot, positionOfPlayer.X);
            Canvas.SetTop(dot, positionOfPlayer.Y);
         }).Task;
      }

      private void MoveHomeTeam()
      {
         foreach (Position homePosition in controller.HomePositionCollection.Positions)
         {
            homePosition.X++;
            homePosition.Y++;
         }
      }

      private void MoveAwayTeam()
      {
         foreach (Position awayPosition in controller.AwayPositionCollection.Positions)
         {
            awayPosition.X++;
            awayPosition.Y++;
         }
      }
   }
}