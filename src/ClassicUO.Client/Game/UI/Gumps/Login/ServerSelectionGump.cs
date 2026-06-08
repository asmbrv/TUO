// SPDX-License-Identifier: BSD-2-Clause

using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using ClassicUO.Configuration;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Assets;
using ClassicUO.Game.UI.Gumps.CharCreation;
using ClassicUO.Network;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using SDL3;

namespace ClassicUO.Game.UI.Gumps.Login
{
    public class ServerSelectionGump : Gump
    {
        private const ushort SELECTED_COLOR = 0x941;
        private const ushort NORMAL_COLOR = 0x034F;

        public static ServerSelectionGump Instance { get; private set; }

        public ServerSelectionGump(World world) : base(world, 0, 0)
        {
            Instance?.Dispose();
            Instance = this;

            UIManager.GetGump<LoginGump>()?.Dispose();
            UIManager.GetGump<CharacterSelectionGump>()?.Dispose();
            UIManager.GetGump<CharCreationGump>()?.Dispose();

            //AddChildren(new LoginBackground(true));

            // Define o gump como o container da tela inteira para evitar snapping no canto
            Width = 1024;
            Height = 768;

            Add
            (
                new Button((int) Buttons.Prev, 0x15A1, 0x15A3, 0x15A2)
                {
                    X = 5, Y = 650, ButtonAction = ButtonAction.Activate
                }
            );

            Add
            (
                new Button((int) Buttons.Next, 0x15A4, 0x15A6, 0x15A5)
                {
                    X = 870, Y = 650, ButtonAction = ButtonAction.Activate
                }
            );

            ushort textColor = 0x921;

            //Add(new Label(ResGumps.SelectWhichShardToPlayOn, false, textColor, font: 9)
            //{
            //    X = 275, Y = 210
            //});

            Add(new Label(ResGumps.Latency, false, textColor, font: 9)
            {
                X = 495, Y = 210
            });

            Add(new Label(ResGumps.PacketLoss, false, textColor, font: 9)
            {
                X = 560, Y = 210
            });

            //Add(new Label(ResGumps.SortBy, false, textColor, font: 9)
            //{
            //    X = 273, Y = 508
            //});

            //Add
            //(
            //    new Button((int) Buttons.SortTimeZone, 0x093B, 0x093C, 0x093D)
            //    {
            //        X = 230, Y = 366
            //    }
            //);

            //Add
            //(
            //    new Button((int) Buttons.SortFull, 0x093E, 0x093F, 0x0940)
            //    {
            //        X = 338, Y = 366
            //    }
            //);

            //Add
            //(
            //    new Button((int) Buttons.SortConnection, 0x0941, 0x0942, 0x0943)
            //    {
            //        X = 446, Y = 366
            //    }
            //);

            // World Pic Bg
            //Add(new GumpPic(150, 390, 0x0589, 0));

            // Earth
            //Add
            //(
            //    new Button((int) Buttons.Earth, 0x15E8, 0x15EA, 0x15E9)
            //    {
            //        X = 160, Y = 400, ButtonAction = ButtonAction.Activate
            //    }
            // );

            // Sever Scroll Area Bg
            Add
            (
                new ResizePic(0x0DAC)
                {
                    X = 270, Y = 230, Width = 393 - 14, Height = 271
                }
            );

            // Sever Scroll Area
            var scrollArea = new ScrollArea
            (
                270,
                230,
                393,
                271,
                true
            );

            var databox = new DataBox(0, 0, 1, 1);
            databox.WantUpdateSize = true;
            LoginScene loginScene = Client.Game.GetScene<LoginScene>();

            scrollArea.ScissorRectangle.Y = 16;
            scrollArea.ScissorRectangle.Height = -32;

            int index = loginScene.GetServerIndexFromSettings();
            ServerListEntry selected = null;

            foreach (ServerListEntry server in loginScene.Servers)
            {
                databox.Add(new ServerEntryGump(server, 9, NORMAL_COLOR, SELECTED_COLOR));
                if(server.Index == index)
                    selected = server;
            }

            databox.ReArrangeChildren();

            Add(scrollArea);
            scrollArea.Add(databox);

            //if (loginScene.Servers.Length != 0)
            //{
            //    if (selected == null)
            //        selected = loginScene.Servers[0];
            //    
            //    Add
            //    (
            //        new Label(selected.Name, false, 0x0481, font: 9)
            //        {
            //            X = 243,
            //            Y = 420
            //        }
            //    );
            //}

            AcceptKeyboardInput = true;
            CanCloseWithRightClick = false;
        }

        public override void OnButtonClick(int buttonID)
        {
            LoginScene loginScene = Client.Game.GetScene<LoginScene>();

            if (buttonID >= (int) Buttons.Server)
            {
                int index = buttonID - (int) Buttons.Server;
                loginScene.SelectServer((byte) index);
            }
            else
            {
                switch ((Buttons) buttonID)
                {
                    case Buttons.Next:
                    case Buttons.Earth:

                        if (loginScene.Servers.Length != 0)
                        {
                            int index = loginScene.GetServerIndexFromSettings();

                            loginScene.SelectServer((byte) loginScene.Servers[index].Index);
                        }

                        break;

                    case Buttons.Prev:
                        loginScene.StepBack();

                        break;
                }
            }
        }

        protected override void OnControllerButtonUp(SDL.SDL_GamepadButton button)
        {
            base.OnControllerButtonUp(button);
            if (button == SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_SOUTH)
            {
                LoginScene loginScene = Client.Game.GetScene<LoginScene>();

                if (loginScene.Servers?.Any(s => s != null) ?? false)
                {
                    int index = loginScene.GetServerIndexFromSettings();

                    loginScene.SelectServer((byte)loginScene.Servers[index].Index);
                }
            }
        }

        public override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            if (key == SDL.SDL_Keycode.SDLK_RETURN || key == SDL.SDL_Keycode.SDLK_KP_ENTER)
            {
                LoginScene loginScene = Client.Game.GetScene<LoginScene>();

                if (loginScene.Servers?.Any(s => s != null) ?? false)
                {
                    int index = loginScene.GetServerIndexFromSettings();
                    bool serverSelected = false;

                    foreach (ServerListEntry s in loginScene.Servers)
                        if (s.Index == index)
                        {
                            loginScene.SelectServer((byte)index);
                            serverSelected = true;
                            break;
                        }
                    
                    if (!serverSelected)
                    {
                        loginScene.SelectServer((byte)loginScene.Servers[0].Index);
                    }
                }
            }
        }

        private enum Buttons
        {
            Prev,
            Next,
            SortTimeZone,
            SortFull,
            SortConnection,
            Earth,
            Server = 99
        }

        private class ServerEntryGump : Control
        {
            private readonly int _buttonId;
            private readonly ServerListEntry _entry;
            private readonly HoveredLabel _server_packet_loss;
            private readonly HoveredLabel _server_ping;
            private readonly HoveredLabel _serverName;
            private uint _pingCheckTime = 0;

            public ServerEntryGump(ServerListEntry entry, byte font, ushort normal_hue, ushort selected_hue)
            {
                _entry = entry;

                _buttonId = entry.Index;

                Add
                (
                    _serverName = new HoveredLabel
                    (
                        entry.Name,
                        false,
                        normal_hue,
                        selected_hue,
                        selected_hue,
                        font: font
                    )
                    {
                        X = 10,
                        AcceptMouseInput = false
                    }
                );

                Add
                (
                    _server_ping = new HoveredLabel
                    (
                        CUOEnviroment.NoServerPing ? string.Empty : "-",
                        false,
                        normal_hue,
                        selected_hue,
                        selected_hue,
                        font: font
                    )
                    {
                        X = 250,
                        AcceptMouseInput = false
                    }
                );

                Add
                (
                    _server_packet_loss = new HoveredLabel
                    (
                        CUOEnviroment.NoServerPing ? string.Empty : "-",
                        false,
                        normal_hue,
                        selected_hue,
                        selected_hue,
                        font: font
                    )
                    {
                        X = 320,
                        AcceptMouseInput = false
                    }
                );


                AcceptMouseInput = true;
                Width = 370;
                Height = 25;

                WantUpdateSize = false;
            }

            protected override void OnMouseEnter(int x, int y)
            {
                base.OnMouseEnter(x, y);

                _serverName.IsSelected = true;
                _server_packet_loss.IsSelected = true;
                _server_ping.IsSelected = true;
            }

            protected override void OnMouseExit(int x, int y)
            {
                base.OnMouseExit(x, y);

                _serverName.IsSelected = false;
                _server_packet_loss.IsSelected = false;
                _server_ping.IsSelected = false;
            }

            public override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                if (button == MouseButtonType.Left)
                {
                    OnButtonClick((int) Buttons.Server + _buttonId);
                }
            }

            public override void Update()
            {
                base.Update();

                if (CUOEnviroment.NoServerPing == false && _pingCheckTime < Time.Ticks)
                {
                    _pingCheckTime = Time.Ticks + 2000;
                    _entry.DoPing();

                    switch (_entry.PingStatus)
                    {
                        case IPStatus.Success:
                            _server_ping.Text = _entry.Ping == -1 ? $"-" : _entry.Ping.ToString();

                            break;

                        case IPStatus.DestinationNetworkUnreachable:
                        case IPStatus.DestinationHostUnreachable:
                        case IPStatus.DestinationProtocolUnreachable:
                        case IPStatus.DestinationPortUnreachable:
                        case IPStatus.DestinationUnreachable:
                            _server_ping.Text = "unreach.";

                            break;

                        case IPStatus.TimedOut:
                            _server_ping.Text = "time out";

                            break;

                        default:
                            _server_ping.Text = $"unk. [{(int) _entry.PingStatus}]";

                            break;
                    }

                    _server_packet_loss.Text = $"{_entry.PacketLoss}%";
                }
            }
        }
    }
}
