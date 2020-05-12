namespace FootballClient
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

    using FootballClient.Models;
    using FootballClient.Models.Message.InitialMessages;
    using FootballClient.Network;

    /// <summary>Interaction logic for MainWindow.xaml.</summary>
    public partial class MainWindow
    {
        private readonly TcpClient client;
        private readonly IPEndPoint endPoint;
        private readonly ServerCommunicator communicator;
        private readonly Team team = new Team
        {

            // The players int THIS TEAM
            Players = new List<Player>
            {
                new Player { ID = Guid.NewGuid(), Name = "Player1" },
                new Player { ID = Guid.NewGuid(), Name = "Player2" },
                new Player { ID = Guid.NewGuid(), Name = "Player3" },
                new Player { ID = Guid.NewGuid(), Name = "Player4" },
                new Player { ID = Guid.NewGuid(), Name = "Player5" },
                new Player { ID = Guid.NewGuid(), Name = "Player6" },
                new Player { ID = Guid.NewGuid(), Name = "Player7" },
                new Player { ID = Guid.NewGuid(), Name = "Player8" },
                new Player { ID = Guid.NewGuid(), Name = "Player9" },
                new Player { ID = Guid.NewGuid(), Name = "Player10" },
                new Player { ID = Guid.NewGuid(), Name = "Player11" }
            }
        };
        private readonly ClientGameController controller;

        public MainWindow()
        {
            InitializeComponent();

            endPoint = new IPEndPoint(IPAddress.Loopback, 55555);

            client = new TcpClient { ReceiveBufferSize = Convert.ToInt32(Math.Pow(2, 16)) };
            client.Connect(endPoint);

            communicator = new ServerCommunicator(client);
            controller = new ClientGameController(communicator, team);

            controller.MatchStarted += Controller_MatchStarted;
            controller.OverallMatchStandingReceived += OverallMatchStandingReceived;
            controller.MatchOver += Controller_MatchOver;
        }

        private void Controller_MatchOver(object sender, EventArgs e)
        {
            info.Text = $"Final result: {controller.matchResult.HomeGoals}:{controller.matchResult.AwayGoals}";
        }

        private async void OverallMatchStandingReceived(object sender, EventArgs e)
        {
            PositionCollection awayPositionCollection = controller.OverallMatchStanding.PositionCollection;

            for (var i = 0; i < controller.AwayTeam.Players.Count; i++)
            {
                Player homePlayer = controller.AwayTeam.Players[i];
                Position positionOfPlayer = awayPositionCollection.Positions.FirstOrDefault(x => x.ID == homePlayer.ID);
                if (positionOfPlayer is null)
                {
                    continue;
                }

                await footballPitch.Dispatcher.InvokeAsync(() =>
                {
                    UIElement dot = footballPitch.Children[i];
                    Canvas.SetLeft(dot, positionOfPlayer.X);
                    Canvas.SetTop(dot, positionOfPlayer.Y);
                }).Task;
            }

           
            await footballPitch.Dispatcher.InvokeAsync(() =>
            {
                // Sets the ball's position on the pitch
                UIElement ball = footballPitch.Children.Cast<UIElement>().Last();
                Canvas.SetLeft(ball, controller.OverallMatchStanding.BallPosition.X);
                Canvas.SetTop(ball, controller.OverallMatchStanding.BallPosition.Y);
            }).Task;

            Thread.Sleep(100);

            await footballPitch.Dispatcher.InvokeAsync(() => { timeTable.Text = $"Time: {controller.OverallMatchStanding.TimeElapsed.Time}"; })
                .Task;
        }

        private void Controller_MatchStarted(object sender, EventArgs e)
        {
            footballPitch.Dispatcher.Invoke(() =>
            {
                footballPitch.Width = controller.Pitch.Width;
                footballPitch.Height = controller.Pitch.Height;
            });

            var initialPlayerPositions = new PositionCollection();

            initialPlayerPositions.Positions.Add(new Position { ID = controller.HomeTeam.Players[0].ID, X = 0, Y = 250 });
            initialPlayerPositions.Positions.Add(new Position { ID = controller.HomeTeam.Players[1].ID, X = 50, Y = 50 });
            initialPlayerPositions.Positions.Add(new Position { ID = controller.HomeTeam.Players[2].ID, X = 50, Y = 100 });
            initialPlayerPositions.Positions.Add(new Position { ID = controller.HomeTeam.Players[3].ID, X = 50, Y = 150 });
            initialPlayerPositions.Positions.Add(new Position { ID = controller.HomeTeam.Players[4].ID, X = 50, Y = 200 });
            initialPlayerPositions.Positions.Add(new Position { ID = controller.HomeTeam.Players[5].ID, X = 100, Y = 50 });
            initialPlayerPositions.Positions.Add(new Position { ID = controller.HomeTeam.Players[6].ID, X = 100, Y = 100 });
            initialPlayerPositions.Positions.Add(new Position { ID = controller.HomeTeam.Players[7].ID, X = 100, Y = 150 });
            initialPlayerPositions.Positions.Add(new Position { ID = controller.HomeTeam.Players[8].ID, X = 150, Y = 200 });
            initialPlayerPositions.Positions.Add(new Position { ID = controller.HomeTeam.Players[9].ID, X = 150, Y = 50 });
            initialPlayerPositions.Positions.Add(new Position { ID = controller.HomeTeam.Players[10].ID, X = 150, Y = 150 });

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
            await MoveHomePlayers();
        }

        private async Task MoveHomePlayers()
        {
            for (var i = 0; i < controller.AwayTeam.Players.Count; i++)
            {
                Player homePlayer = controller.AwayTeam.Players[i];
                Position positionOfPlayer = controller.HomePositionCollection.Positions.FirstOrDefault(x => x.ID == homePlayer.ID);
                if (positionOfPlayer is null)
                {
                    continue;
                }

                await footballPitch.Dispatcher.InvokeAsync(() =>
                {
                    // The dots in the pitch from 11 to 21 are the home team dots
                    UIElement dot = footballPitch.Children[i + 11];
                    Canvas.SetLeft(dot, positionOfPlayer.X);
                    Canvas.SetTop(dot, positionOfPlayer.Y);
                }).Task;
            }
        }
    }
}
