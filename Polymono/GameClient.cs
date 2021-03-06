﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using Polymono.Game;
using Polymono.Graphics;
using Polymono.Graphics.Components;
using Polymono.Networking;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Polymono
{
    public enum GameState
    {
        Menu, Lobby, LobbyRoll,
        PlayerOptions, Moving
    }

    class GameClient : AGameClient
    {
        // Game states
        public GameState State;
        // Camera data
        public Camera Camera;
        //public Label LabelTest;
        // Game objects
        public Board Board;
        public Dice DiceOne;
        public Dice DiceTwo;

        #region Menus
        // Test menu
        public Menu MnuTest;
        public Label LblTest;
        public Button BtnTest;
        public Textbox TxtTest;
        // Main menu
        public Menu MnuMain;
        public Button BtnCreate;
        public Button BtnJoin;
        public Button BtnExit;

        // Network lobby menu
        public Menu MnuNetwork;
        public Label LblNetName;
        public Textbox TxtNetName;
        public Label LblNetAddress;
        public Textbox TxtNetAddress;
        public Checkbox ChkNetType;
        public Label LblNetPort;
        public Textbox TxtNetPort;
        public Button BtnNetJoin;
        public Button BtnNetCreate;
        public Button BtnNetBack;

        // Message menu
        public Menu MnuMessage;
        public Label LblMsgBox;
        public Textbox TxtMsgInput;
        public Button BtnMsgSend;

        // Game menu
        public Menu MnuGame;
        public Button BtnGameStart;
        public Button BtnGameExit;
        // Player options menu
        // <Roll dice> <Purchase property> <Trade>
        public Menu MnuPlayerOptions;
        public Button BtnRollDice;
        public Button BtnPurchaseProperty;
        public Button BtnTrade;
        public Button BtnEndTurn;
        // Player jail menu
        // <Pay 50> <Use card>
        public Menu MnuPlayerJailOptions;
        public Button BtnPayJail;
        public Button BtnUseCard;

        // Trade menu
        public Menu MnuTrade;
        #endregion

        //Misc object
        public Skybox Skybox;
        public Light ActiveLight = new Light(new Vector3(0.0f, 5.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));

        public Dictionary<string, AModel> AModels;

        public INetwork network;
        public INetwork Network {
            set {
                if (value is Server)
                {
                    network = value;
                    IsServer = true;
                    StartReceiving = true;
                }
                else if (value is Client)
                {
                    network = value;
                    IsServer = false;
                    StartReceiving = true;
                }
            }
            get {
                return network;
            }
        }
        public bool IsServer = false;
        public bool StartReceiving = false;

        public GameClient(int playerCount) : base()
        {
            State = GameState.Menu;
            Camera = new Camera(new Vector3(0.0f, 1.0f, 0.0f));
            AModels = new Dictionary<string, AModel>();
        }

        protected override void LoadObjects()
        {
            // Skybox
            Skybox = new Skybox(Programs[ProgramID.Skybox], @"Resources\Objects\sphere.obj", false);
            Skybox.CreateBuffer();
            // Board
            Board = new Board(Programs[ProgramID.Full], Programs[ProgramID.Player], this);
            Board.Model.CreateBuffer();
            Models.Add(Board.Model.ID, Board.Model);
            // Player model
            AModels.Add("Player", new ModelObject(
                Programs[ProgramID.Player],
                @"Resources\Objects\player.obj",
                Color4.White,
                Vector3.Zero,
                Vector3.Zero,
                new Vector3(0.1f),
                @"Resources\Objects\player.mtl",
                @"b0b0b0"));
            AModels["Player"].CreateBuffer();
            // Dice
            DiceOne = new Dice(Programs[ProgramID.Dice],
                new Vector3(0.0f, 0.05f, 0.25f), Vector3.Zero, new Vector3(0.05f));
            DiceTwo = new Dice(Programs[ProgramID.Dice],
                new Vector3(0.25f, 0.05f, 0.0f), Vector3.Zero, new Vector3(0.05f));
            DiceOne.Model.CreateBuffer();
            DiceTwo.Model.CreateBuffer();
            Models.Add(DiceOne.Model.ID, DiceOne.Model);
            Models.Add(DiceTwo.Model.ID, DiceTwo.Model);

            // Loads the controls of the program.
            LoadControls();

            // Reset hide/show states for all menus.
            Menu.HIDEALL();
            MnuMain.ShowAll();
            MnuTest.ShowAll();
        }

        protected void LoadControls()
        {
            #region Main menu
            MnuMain = new Menu();
            BtnCreate = new Button(Programs[ProgramID.Button],
                new Vector3(Width / 2 - 64, Height - 128, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuMain,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    MnuMain.HideAll();
                    MnuNetwork.ShowAll();
                    LblNetAddress.HideAll();
                    TxtNetAddress.HideAll();
                    BtnNetJoin.HideAll();
                    return Task.Delay(0);
                }, "Create game");
            BtnJoin = new Button(Programs[ProgramID.Button],
                new Vector3(Width / 2 - 64, Height - 192, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuMain,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    MnuMain.HideAll();
                    MnuNetwork.ShowAll();
                    BtnNetCreate.HideAll();
                    return Task.Delay(0);
                }, "Join game");
            BtnExit = new Button(Programs[ProgramID.Button],
                new Vector3(Width / 2 - 64, Height - 256, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuMain,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    Exit();
                    return Task.Delay(0);
                }, "Exit");
            MnuMain.CreateBuffers();
            #endregion
            #region Test menu
            MnuTest = new Menu();
            LblTest = new Label(Programs[ProgramID.Label],
                Vector3.Zero, Vector3.Zero, Vector3.One,
                Controls, Models, MnuTest,
                92, 18, 2, Width, Height, Matrix4.Identity, new Color4(0.1f, 0.1f, 0.1f, 0.6f), "X:0 Y:0");
            BtnTest = new Button(Programs[ProgramID.Button],
                new Vector3(8.0f, 40.0f, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuTest,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    Polymono.Print("Hello");
                    return Task.Delay(0);
                }, "Test button");
            TxtTest = new Textbox(Programs[ProgramID.Label],
                new Vector3(8.0f, 80.0f, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuTest,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                "Test Textbox");
            MnuTest.CreateBuffers();
            #endregion
            #region Server creation menu
            MnuNetwork = new Menu();
            LblNetName = new Label(Programs[ProgramID.Label],
                new Vector3((Width / 2) - 64, Height - (64 * 2) + 24, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                92, 18, 2, Width, Height, Matrix4.Identity, new Color4(0.1f, 0.1f, 0.1f, 0.6f),
                "Enter name:");
            TxtNetName = new Textbox(Programs[ProgramID.Label],
                new Vector3((Width / 2) - 64, Height - (64 * 2), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                "");
            LblNetAddress = new Label(Programs[ProgramID.Label],
                new Vector3((Width / 2) - 160, Height - (64 * 3) + 24, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                108, 18, 2, Width, Height, Matrix4.Identity, new Color4(0.1f, 0.1f, 0.1f, 0.6f),
                "Network address:");
            TxtNetAddress = new Textbox(Programs[ProgramID.Label],
                new Vector3((Width / 2) - 160, Height - (64 * 3), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                "");
            ChkNetType = new Checkbox(Programs[ProgramID.Button],
                new Vector3((Width / 2) - 160 + 128 + 8, Height - (64 * 3), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                32, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f));
            LblNetPort = new Label(Programs[ProgramID.Label],
                new Vector3((Width / 2) + 32, Height - (64 * 3) + 24, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                92, 18, 2, Width, Height, Matrix4.Identity, new Color4(0.1f, 0.1f, 0.1f, 0.6f),
                "Network port:");
            TxtNetPort = new Textbox(Programs[ProgramID.Label],
                new Vector3((Width / 2) + 32, Height - (64 * 3), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                "");
            BtnNetCreate = new Button(Programs[ProgramID.Button],
                new Vector3((Width / 2) - 64, Height - (64 * 4), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f), // Default model
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f), // Clicked model
                () =>
                {
                    try
                    {
                        bool v6;
                        if (ChkNetType.State == ControlState.Clicked)
                        {
                            v6 = true;
                        }
                        else
                        {
                            v6 = false;
                        }
                        Server server = new Server(this, TxtNetName.Text, v6);
                        int port = Convert.ToInt32(TxtNetPort.Text);
                        server.LocalHandler().Bind(port);
                        server.LocalHandler().Listen(10);
                        server.Start();
                        Network = server;
                        State = GameState.Lobby;
                        MnuNetwork.HideAll();
                        MnuMessage.ShowAll();
                        MnuGame.ShowAll();
                    }
                    catch (SocketException se)
                    {
                        Polymono.Error(se.Message);
                        Polymono.ErrorF(se.StackTrace);
                    }
                    catch (FormatException fe)
                    {
                        Polymono.Error("Port is not a readable number.");
                        Polymono.Error(fe.Message);
                        Polymono.ErrorF(fe.StackTrace);
                    }
                    return Task.Delay(0);
                }, "Create Server");
            BtnNetJoin = new Button(Programs[ProgramID.Button],
                new Vector3((Width / 2) - 64, Height - (64 * 4), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f), // Default model
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f), // Clicked model
                () =>
                {
                    try
                    {
                        bool v6;
                        if (ChkNetType.State == ControlState.Clicked)
                        {
                            v6 = true;
                        }
                        else
                        {
                            v6 = false;
                        }
                        Client client = new Client(this, v6);
                        string address = TxtNetAddress.Text;
                        int port = Convert.ToInt32(TxtNetPort.Text);
                        string name = TxtNetName.Text;
                        client.ConnectAsync(address, port, name);
                        Network = client;
                        State = GameState.Lobby;
                        MnuNetwork.HideAll();
                        MnuMessage.ShowAll();
                    }
                    catch (SocketException se)
                    {
                        Polymono.Error(se.Message);
                        Polymono.ErrorF(se.StackTrace);
                    }
                    catch (FormatException fe)
                    {
                        Polymono.Error("Port is not a readable number.");
                        Polymono.Error(fe.Message);
                        Polymono.ErrorF(fe.StackTrace);
                    }
                    return Task.Delay(0);
                }, "Join server");
            BtnNetBack = new Button(Programs[ProgramID.Button],
                new Vector3((Width / 2) - 64, Height - (64 * 6), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    MnuNetwork.HideAll();
                    MnuMain.ShowAll();
                    return Task.Delay(0);
                }, "Back");
            MnuNetwork.CreateBuffers();
            #endregion
            #region Message menu
            MnuMessage = new Menu();
            LblMsgBox = new Label(Programs[ProgramID.Label],
                new Vector3(32, Height / 2, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuMessage,
                256, 256, 2, Width, Height, Matrix4.Identity, new Color4(0.1f, 0.1f, 0.1f, 0.6f), "Chat room text" + Environment.NewLine + "Chat room text2");
            TxtMsgInput = new Textbox(Programs[ProgramID.Label],
                new Vector3(32, Height / 2 - 256, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuMessage,
                192, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                "");
            BtnMsgSend = new Button(Programs[ProgramID.Button],
                new Vector3(32 + 192, Height / 2 - 256, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuMessage,
                64, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    string text = TxtMsgInput.Text.Trim();
                    TxtMsgInput.Text = "";
                    LblMsgBox.Text += Environment.NewLine + text;
                    Network.SendAsync(PacketHandler.Create(PacketType.Message,
                        Protocol.Chat.EncodeMessage(text)));
                    return Task.Delay(0);
                }, "Send");
            MnuMessage.CreateBuffers();
            #endregion
            #region Game menu
            MnuGame = new Menu();
            BtnGameStart = new Button(Programs[ProgramID.Button],
                new Vector3((Width / 2) - 64, Height - (64 * 4), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuGame,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    if (IsServer && Board.GetPlayers().Count > 1)
                    {
                        // Start game.
                        ((Server)Network).AcceptClients = false;
                        Network.SendAsync(PacketHandler.Create(PacketType.StartGame,
                            Protocol.Game.EncodeStartGame(Board.CurrentPlayerID)));
                        State = GameState.LobbyRoll;
                        MnuGame.HideAll();
                        BtnRollDice.ShowAll();
                        Polymono.Debug("Starting game.");
                    }
                    else
                    {
                        // Not enough players, not server.
                        Polymono.Debug($"Cannot start game: [Is server:{IsServer}] [Players: {Board.GetPlayers().Count}]");
                        Polymono.Debug("Needs to be server and have more than 1 player in lobby.");
                    }
                    // Starts game
                    return Task.Delay(0);
                }, "Start Game");
            BtnGameExit = new Button(Programs[ProgramID.Button],
                new Vector3((Width / 2) - 64, Height - (64 * 6), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuGame,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                async () =>
                {
                    await Network.SendAsync(PacketHandler.Create(PacketType.Disconnect,
                        Protocol.Connection.EncodeDisconnect(Board.CurrentPlayerID, "Exit lobby")));
                    Network = null;
                    Polymono.Debug("Exiting game.");
                    // Exits game
                }, "Exit Game");
            MnuPlayerOptions = new Menu();
            BtnRollDice = new Button(Programs[ProgramID.Button],
                new Vector3((Width / 2) - 64, (64 * 2), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuPlayerOptions,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    int rollOne = DiceOne.GetNumber();
                    int rollTwo = DiceTwo.GetNumber();
                    Network.SendAsync(PacketHandler.Create(PacketType.DiceRoll,
                        Protocol.Game.EncodeDiceRoll(rollOne, rollTwo,
                        Board.CurrentPlayerID)));
                    if (State == GameState.LobbyRoll)
                    {
                        BtnRollDice.HideAll();
                        Board.EndCurrentTurn(Board.CurrentPlayerID);
                        Network.SendAsync(PacketHandler.Create(PacketType.EndTurn,
                            Protocol.Game.EncodeEndTurn(Board.CurrentPlayerID)));
                    }
                    else if (State == GameState.PlayerOptions)
                    {
                        MnuPlayerOptions.HideAll();
                        Board.MoveSpaces(rollOne + rollTwo, Board.CurrentPlayerID);
                        State = GameState.Moving;
                    }
                    return Task.Delay(0);
                }, "Roll Dice");
            BtnPurchaseProperty = new Button(Programs[ProgramID.Button],
                new Vector3((Width / 2) - 256, (64 * 2), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuPlayerOptions,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    // Purchase current property.
                    return Task.Delay(0);
                }, "Purchase");
            BtnTrade = new Button(Programs[ProgramID.Button],
                new Vector3((Width / 2) + 192, (64 * 2), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuPlayerOptions,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    // Trade screen.
                    return Task.Delay(0);
                }, "Trade");
            BtnEndTurn = new Button(Programs[ProgramID.Button],
                new Vector3((Width / 2) - 64, (64 * 2), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuPlayerOptions,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    MnuPlayerOptions.HideAll();
                    // Trade screen.
                    Board.EndCurrentTurn(Board.CurrentPlayerID);
                    Network.SendAsync(PacketHandler.Create(PacketType.EndTurn,
                        Protocol.Game.EncodeEndTurn(Board.CurrentPlayerID)));
                    return Task.Delay(0);
                }, "End Turn");
            MnuGame.CreateBuffers();
            MnuPlayerOptions.CreateBuffers();
            #endregion
            #region Game menu
            MnuPlayerJailOptions = new Menu();
            BtnPayJail = new Button(Programs[ProgramID.Button],
                new Vector3(), Vector3.Zero, Vector3.One,
                Controls, Models, MnuPlayerJailOptions,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    // Pay Jail
                    return Task.Delay(0);
                }, "Roll Dice");
            BtnUseCard = new Button(Programs[ProgramID.Button],
                new Vector3(), Vector3.Zero, Vector3.One,
                Controls, Models, MnuPlayerJailOptions,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    // Use card
                    return Task.Delay(0);
                }, "Purchase");
            MnuPlayerJailOptions.CreateBuffers();
            #endregion
        }

        bool IsMovingDone = true;

        protected override void UpdateObjects()
        {
            // Update inputs and camera.
            UpdateInput(UTimeDelta);
            UpdateCamera();
            // Manage game state.
            LblTest.Text = $"X:{Mouse.X} Y:{Mouse.Y}";
            LblTest.SetTranslate(new Vector3(Mouse.X + 16, Height - Mouse.Y, 0.0f));
            LblTest.Update();
            #region Network updating
            UpdateGameFromNetwork();
            #endregion
            #region Update game states
            // If a player should move
            // Move until
            SetWindowTitle(IsServer ? "Server" : "Client", TxtNetName?.Text ?? "N/A", State.ToString(), Board.CurrentPlayerTurn.ToString());
            switch (State)
            {
                case GameState.Menu:
                    break;
                case GameState.Lobby:
                    break;
                case GameState.LobbyRoll:
                    break;
                case GameState.PlayerOptions:
                    break;
                case GameState.Moving:
                    bool isDone = Board.UpdatePositions(Board.CurrentPlayerTurn, (float)(1d / UpdatePeriod));
                    if (isDone && Board.CurrentPlayerTurn == Board.CurrentPlayerID)
                    {
                        MnuPlayerOptions.ShowAll();
                        BtnRollDice.HideAll();
                        Network.SendAsync(PacketHandler.Create(PacketType.MoveState,
                            Protocol.Game.EncodeMoveDone(Board.CurrentPlayerTurn)));
                        State = GameState.PlayerOptions;
                    }
                    break;
                default:
                    break;
            }
            #endregion
            #region Update Matrices.
            // Update view matrices.
            ViewMatrix = Camera.GetViewMatrix();
            StaticViewMatrix = Camera.GetStaticViewMatrix();
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(ToRadians(Camera.Zoom),
                (float)Width / Height,
                0.1f, 1000f);
            UIProjectionMatrix = Matrix4.CreateOrthographicOffCenter(
                ClientRectangle.X, ClientRectangle.Width,
                ClientRectangle.Y, ClientRectangle.Height,
                -1.0f, 1.0f);
            // Update projection matrices.
            foreach (var control in Controls.Values)
            {
                control.ProjectionMatrix = UIProjectionMatrix;
                control.Update();
            }
            // Update matrices.
            Skybox.UpdateModelMatrix();
            foreach (var model in Models.Values)
            {
                model.UpdateModelMatrix();
            }
            DiceTwo.UpdateModelMatrix();
            foreach (var player in Board.GetPlayers())
            {
                player.UpdateModelMatrix();
            }
            #endregion
        }

        private void UpdateGameFromNetwork()
        {
            if (Network == null)
            {
                return;
            }
            Queue<Packet> packetsQueue = Network.GetPacketQueue();
            List<Packet> packets = new List<Packet>();
            for (int i = 0; i < packetsQueue.Count; i++)
            {
                packets.Add(packetsQueue.Dequeue());
            }
            foreach (var packet in packets)
            {
                int senderID;
                switch (packet.Type)
                {
                    case PacketType.Null:
                        Polymono.Print(ConsoleLevel.Warning, "Protocol type is null.");
                        break;
                    case PacketType.StartGame:
                        senderID = Protocol.Game.DecodeStartGameSenderID(packet.Data);
                        Polymono.Debug($"Packet decoding: {packet.Type} [Sender ID: {senderID}]");
                        if (State == GameState.Lobby)
                        {
                            // Correct state.
                            State = GameState.LobbyRoll;
                            break;
                        }
                        break;
                    case PacketType.EndTurn:
                        senderID = Protocol.Game.DecodeEndTurnSenderID(packet.Data);
                        Polymono.Debug($"Packet decoding: {packet.Type} [Sender ID: {senderID}]");
                        // Signal an end turn of current player's turn.
                        // Find out which player's turn is next.
                        Board.EndCurrentTurn(senderID);
                        bool isCurrentTurn = Board.CurrentPlayerTurn == Board.CurrentPlayerID;
                        switch (State)
                        {
                            case GameState.LobbyRoll:
                                // If in pre-game rolling stage, and is current turn, turn on roll button.
                                if (isCurrentTurn)
                                {
                                    BtnRollDice.ShowAll();
                                }
                                break;
                            case GameState.PlayerOptions:
                                // If in in-game options stage, and is current turn, turn on player options.
                                if (isCurrentTurn)
                                {
                                    if (Board.GetPlayer().IsInJail)
                                    {
                                        MnuPlayerJailOptions.ShowAll();
                                    }
                                    else
                                    {
                                        MnuPlayerOptions.ShowAll();
                                        BtnEndTurn.HideAll();
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case PacketType.Message:
                        string message = Protocol.Chat.DecodeMessage(packet.Data);
                        LblMsgBox.Text += Environment.NewLine + message;
                        Polymono.Debug($"Packet decoding: {packet.Type} [Message: {message}]");
                        break;
                    case PacketType.MoveState:
                        break;
                    case PacketType.DiceRoll:
                        int diceOne = Protocol.Game.DecodeDiceRollOne(packet.Data);
                        int diceTwo = Protocol.Game.DecodeDiceRollTwo(packet.Data);
                        senderID = Protocol.Game.DecodeDiceRollSenderID(packet.Data);
                        Polymono.Debug($"Packet decoding: {packet.Type} [Dice One: {diceOne}] [Dice Two: {diceTwo}] [Sender ID: {senderID}]");
                        // Move requested player to position.
                        if (State == GameState.PlayerOptions)
                        {
                            // Do moving code
                            if (Board.CurrentPlayerTurn == senderID)
                            {
                                Board.MoveSpaces(diceOne + diceTwo, senderID);
                                State = GameState.Moving;
                            } // Else :: Invalid state for packet.
                            else
                            {
                                Polymono.Warning($"Dice roll packet received with wrong sender. [Sender ID: {senderID}] [Current ID: {Board.CurrentPlayerTurn}]");
                            }
                        }
                        else if (State == GameState.LobbyRoll)
                        {
                            // Assign to highest roll.
                            Board.GetPlayer(senderID).HighestRoll = diceOne + diceTwo;
                        }
                        break;
                    case PacketType.Move:
                        senderID = Protocol.Game.DecodeMoveDone(packet.Data);
                        Polymono.Debug($"Packet decoding: {packet.Type} [sender ID:{senderID}]");
                        Board.FinalisePlayerMovement(senderID);
                        State = GameState.PlayerOptions;
                        break;
                    case PacketType.ClientSync:
                        int id = Protocol.Connection.DecodeClientID(packet.Data);
                        string name = Protocol.Connection.DecodeClientName(packet.Data);
                        Polymono.Debug($"Packet decoding: {packet.Type} [ID: {id}] [Name: {name}]");
                        // Create player data.
                        Board.AddPlayer(id, name);
                        break;
                    default:
                        Polymono.Debug($"Unmanaged protocol type: {packet.Type}");
                        break;
                }
            }
        }

        protected override void RenderObjects()
        {
            // Skybox renderer.
            Programs[ProgramID.Skybox].UseProgram();
            Programs[ProgramID.Skybox].UniformMatrix4("projection", ref ProjectionMatrix);
            Programs[ProgramID.Skybox].UniformMatrix4("view", ref StaticViewMatrix);
            Programs[ProgramID.Skybox].UniformMatrix4("model", ref Skybox.ModelMatrix);
            Programs[ProgramID.Skybox].Uniform1("time", (float)RTime);
            Skybox.Render();
            //// Basic renderer.
            Programs[ProgramID.Full].UseProgram();
            Programs[ProgramID.Full].UniformMatrix4("projection", ref ProjectionMatrix);
            Programs[ProgramID.Full].UniformMatrix4("view", ref ViewMatrix);
            Board.Model.Render();
            // Dice one renderer.
            Programs[ProgramID.Dice].UseProgram();
            Programs[ProgramID.Dice].Uniform3("light_position", ref ActiveLight.Position);
            Programs[ProgramID.Dice].Uniform3("light_color", ref ActiveLight.Color);
            Programs[ProgramID.Dice].Uniform1("light_ambientIntensity", ActiveLight.DiffuseIntensity);
            Programs[ProgramID.Dice].Uniform1("light_diffuseIntensity", ActiveLight.AmbientIntensity);
            Programs[ProgramID.Dice].UniformMatrix4("projection", ref ProjectionMatrix);
            Programs[ProgramID.Dice].UniformMatrix4("view", ref ViewMatrix);
            Programs[ProgramID.Dice].UniformMatrix4("model", ref DiceOne.ModelMatrix);
            Programs[ProgramID.Dice].Uniform1("time", (float)RTime);
            DiceOne.Model.Render();
            // Dice two renderer.
            Programs[ProgramID.Dice].UniformMatrix4("model", ref DiceTwo.ModelMatrix);
            Programs[ProgramID.Dice].Uniform1("time", (float)RTime);
            DiceTwo.Model.Render();
            // Player renderer.
            Programs[ProgramID.Player].UseProgram();
            foreach (var player in Board.GetPlayers())
            {
                Programs[ProgramID.Player].UniformMatrix4("projection", ref ProjectionMatrix);
                Programs[ProgramID.Player].UniformMatrix4("view", ref ViewMatrix);
                Programs[ProgramID.Player].UniformMatrix4("model", ref player.ModelMatrix);
                Programs[ProgramID.Player].Uniform4("colour", ref player.Colour);
                player.Model.Render();
            }
        }

        protected override void RenderUI()
        {
            MnuMain.RenderFull();
            MnuNetwork.RenderFull();
            MnuMessage.RenderFull();
            MnuGame.RenderFull();
            MnuPlayerOptions.RenderFull();
            MnuPlayerJailOptions.RenderFull();
            BtnTest.RenderFull();
            // Mouse position renderer
            if (Focused && !isTrackingCursor)
            {
                LblTest.RenderFull();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(ToRadians(Camera.Zoom), (float)Width / Height, 0.1f, 1000f);
            UIProjectionMatrix = Matrix4.CreateOrthographicOffCenter(ClientRectangle.X, ClientRectangle.Width, ClientRectangle.Y, ClientRectangle.Height, -1.0f, 1.0f);
            foreach (var model in BtnCreate.Models.Values)
            {
                model.Position = new Vector3(Width / 2 - 64, Height - (64 * 2), 0.0f);
            }
            foreach (var model in BtnJoin.Models.Values)
            {
                model.Position = new Vector3(Width / 2 - 64, Height - (64 * 3), 0.0f);
            }
            foreach (var model in BtnExit.Models.Values)
            {
                model.Position = new Vector3(Width / 2 - 64, Height - (64 * 6), 0.0f);
            }
            foreach (var control in Controls.Values)
            {
                control.Resizing = true;
            }
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            if (Focused)
            {
                if (isTrackingCursor)
                {
                    ResetMouse();
                    CursorVisible = false;
                }
                else
                {
                    CursorVisible = true;
                }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Camera.ProcessMouseScroll(e.DeltaPrecise);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            List<IClickable> clickables = new List<IClickable>();
            foreach (Control control in Controls.Values)
            {
                if (control is IClickable clickable && !control.IsHidden())
                {
                    clickables.Add(clickable);
                }
            }
            foreach (var clickable in clickables)
            {
                clickable.Click(new Vector2(e.X, e.Y));
            }
        }

        protected void UpdateCamera()
        {
            if (Focused && isTrackingCursor)
            {
                Vector2 delta = Camera.LastPosition - new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
                Camera.ProcessMouseMovement(delta.X, delta.Y);
                ResetMouse();
            }
            else
            {
                Camera.LastPosition = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
            }
        }

        protected void ResetMouse()
        {
            // Sets position of the mouse to the middle of the screen.
            OpenTK.Input.Mouse.SetPosition(Bounds.Left + Bounds.Width / 2, Bounds.Top + Bounds.Height / 2);
            Camera.LastPosition = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
        }

        protected void UpdateInput(double deltaTime)
        {
            KeyboardState state = Keyboard.GetState();
            if (Focused && state.IsAnyKeyDown)
            {
                float deltaTimef = (float)deltaTime;
                #region Text input
                bool controlFocused = false;
                foreach (var control in Controls.Values)
                {
                    if (control is Textbox textbox)
                    {
                        bool focused = textbox.InputText(state, LastKeyboardState);
                        if (focused)
                        {
                            controlFocused = true;
                        }
                    }
                }
                #endregion
                #region Camera manipulation.
                if (!controlFocused)
                {
                    if (IsUniquePress(state, Key.ControlLeft))
                    {
                        isTrackingCursor = !isTrackingCursor;
                        if (isTrackingCursor)
                        {
                            CursorVisible = false;
                        }
                        else
                        {
                            CursorVisible = true;
                        }
                    }
                    if (state.IsKeyDown(Key.ShiftLeft))
                    {
                        deltaTimef *= 5;
                    }
                    if (state.IsKeyDown(Key.W))
                    {
                        Camera.ProcessKeyboard(CameraMovement.Forward, deltaTimef);
                    }
                    if (state.IsKeyDown(Key.S))
                    {
                        Camera.ProcessKeyboard(CameraMovement.Backward, deltaTimef);
                    }
                    if (state.IsKeyDown(Key.A))
                    {
                        Camera.ProcessKeyboard(CameraMovement.Left, deltaTimef);
                    }
                    if (state.IsKeyDown(Key.D))
                    {
                        Camera.ProcessKeyboard(CameraMovement.Right, deltaTimef);
                    }
                    if (state.IsKeyDown(Key.E))
                    {
                        Camera.ProcessKeyboard(CameraMovement.Up, deltaTimef);
                    }
                    if (state.IsKeyDown(Key.Q))
                    {
                        Camera.ProcessKeyboard(CameraMovement.Down, deltaTimef);
                    }
                }
                #endregion
                if (state.IsKeyDown(Key.Escape))
                {
                    Exit();
                }
            }
            LastKeyboardState = state;
        }

        protected bool IsUniqueClick(MouseState state, OpenTK.Input.ButtonState buttonState,
            OpenTK.Input.ButtonState lastButtonState)
        {
            return (buttonState == OpenTK.Input.ButtonState.Pressed
                && !(lastButtonState == OpenTK.Input.ButtonState.Pressed));
        }

        protected bool IsUniquePress(KeyboardState state, Key key)
        {
            return (state.IsKeyDown(key) && !LastKeyboardState.IsKeyDown(key));
        }
    }
}
