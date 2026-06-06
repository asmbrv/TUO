﻿// SPDX-License-Identifier: BSD-2-Clause

using System;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Utility;

namespace ClassicUO.Game.UI.Gumps.Login
{
    public class LoginBackground : Gump
    {
        private readonly GumpPicTiled _background;

        public LoginBackground(World world) : base(world, 0, 0)
        {
            int width = Math.Max(640, Client.Game.Window.ClientBounds.Width);
            int height = Math.Max(480, Client.Game.Window.ClientBounds.Height);

            if (Client.Game.UO.Version >= ClientVersion.CV_706400)
            {
                // Background
                _background = new GumpPicTiled(0, 0, width, height, 0x0150) { AcceptKeyboardInput = false };
                Add(_background);

                // UO Flag
                Add(new GumpPic(0, 4, 0x0151, 0) { AcceptKeyboardInput = false });
            }
            else
            {
                // Background
                _background = new GumpPicTiled(0, 0, width, height, 0x0E14) { AcceptKeyboardInput = false };
                Add(_background);

                // Border
                Add(new GumpPic(0, 0, 0x157C, 0) { AcceptKeyboardInput = false });
                // UO Flag
                Add(new GumpPic(0, 4, 0x15A0, 0) { AcceptKeyboardInput = false });

                // Quit Button
                Add
                (
                    new Button(0, 0x1589, 0x158B, 0x158A)
                    {
                        X = 555,
                        Y = 4,
                        ButtonAction = ButtonAction.Activate,
                        AcceptKeyboardInput = false
                    }
                );
            }


            CanCloseWithEsc = false;
            CanCloseWithRightClick = false;
            AcceptKeyboardInput = false;

            LayerOrder = UILayer.Under;
        }

        public void ResizeBackground(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            _background.Width = width;
            _background.Height = height;
        }

        public override void Update()
        {
            base.Update();

            if (World.Instance != null && World.Instance.InGame)
            {
                Dispose();
            }
        }

        public override void OnButtonClick(int buttonID) => Client.Game.Exit();
    }
}
