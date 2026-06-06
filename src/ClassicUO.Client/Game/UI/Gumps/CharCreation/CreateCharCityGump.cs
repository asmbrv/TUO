// SPDX-License-Identifier: BSD-2-Clause

using System.Collections.Generic;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Assets;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;
using MathHelper = ClassicUO.Utility.MathHelper;

namespace ClassicUO.Game.UI.Gumps.CharCreation
{
    public class CreateCharSelectionCityGump : Gump
    {
        private readonly List<CityControl> _cityControls = new List<CityControl>();
        private readonly string[] _cityNames = { "Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno", "Ter Mur" };
        private readonly Label _facetName;
        private readonly HtmlControl _htmlControl;
        private readonly LoginScene _scene;
        private CityInfo _selectedCity;
        private readonly byte _selectedProfession;

        private readonly Point[] _townButtonsText =
        {
            new Point(105, 130),
            new Point(245, 90),
            new Point(165, 200),
            new Point(395, 160),
            new Point(200, 305),
            new Point(335, 250),
            new Point(160, 395),
            new Point(100, 250),
            new Point(270, 130)
        };

        public CreateCharSelectionCityGump(World world, byte profession, LoginScene scene) : base(world, 0, 0)
        {
            Width = 1024;
            Height = 768;
            CanMove = false;
            CanCloseWithRightClick = false;
            CanCloseWithEsc = false;

            _scene = scene;
            _selectedProfession = profession;

            CityInfo city;

            if (Client.Game.UO.Version >= ClientVersion.CV_70130)
            {
                city = scene.GetCity(0);
            }
            else
            {
                city = scene.GetCity(3);

                if (city == null)
                {
                    city = scene.GetCity(0);
                }
            }

            if (city == null)
            {
                Log.Error(ResGumps.NoCityFound);
                Dispose();

                return;
            }

            uint map = 0;

            if (city.IsNewCity)
            {
                map = city.Map;
            }

            _facetName = new Label
            (
                "",
                true,
                0x0481,
                font: 0,
                style: FontStyle.BlackBorder
            )
            {
                X = 390,
                Y = 160
            };


            if (Client.Game.UO.Version >= ClientVersion.CV_70130)
            {
                Add(new GumpPic(225, 185, (ushort) (0x15D9 + map), 0));
                Add(new GumpPic(220, 180, 0x15DF, 0));
                _facetName.Text = _cityNames[map];
            }
            else
            {
                Add(new GumpPic(220, 180, 0x1598, 0));
                _facetName.IsVisible = false;
            }

            Add(_facetName);

            Add
            (
                new Button((int) Buttons.PreviousScreen, 0x15A1, 0x15A3, 0x15A2)
                {
                    X = 5,
                    Y = 650,
                    ButtonAction = ButtonAction.Activate
                }
            );

            Add
            (
                new Button((int) Buttons.Finish, 0x15A4, 0x15A6, 0x15A5)
                {
                    X = 870,
                    Y = 650,
                    ButtonAction = ButtonAction.Activate
                }
            );


            _htmlControl = new HtmlControl
            (
                615,
                191,
                175,
                367,
                true,
                true,
                ishtml: true,
                text: city.Description
            );

            Add(_htmlControl);

            for (int i = 0; i < scene.Cities.Length; i++)
            {
                CityInfo c = scene.GetCity(i);

                if (c == null)
                {
                    continue;
                }

                int x = 0;
                int y = 0;

                if (c.IsNewCity)
                {
                    uint cityFacet = c.Map;

                    if (cityFacet > 5)
                    {
                        cityFacet = 5;
                    }

                    x = 225 + MathHelper.PercentageOf(Client.Game.UO.FileManager.Maps.MapsDefaultSize[cityFacet, 0] - 2048, c.X, 383);
                    y = 185 + MathHelper.PercentageOf(Client.Game.UO.FileManager.Maps.MapsDefaultSize[cityFacet, 1], c.Y, 384);
                }
                else if (i < _townButtonsText.Length)
                {
                    x = _townButtonsText[i].X + 163;

                    y = _townButtonsText[i].Y + 131;
                }

                var control = new CityControl(c, i) { X = x, Y = y };
                Add(control);
                _cityControls.Add(control);
            }

            SetCity(city);
        }


        private void SetCity(int index) => SetCity(_scene.GetCity(index));

        private void SetCity(CityInfo city)
        {
            if (city == null)
            {
                return;
            }

            _selectedCity = city;
            _htmlControl.Text = city.Description;
            SetFacet(city.Map);
        }

        private void SetFacet(uint index)
        {
            if (Client.Game.UO.Version < ClientVersion.CV_70130)
            {
                return;
            }

            if (index >= _cityNames.Length)
            {
                index = (uint) (_cityNames.Length - 1);
            }

            _facetName.Text = _cityNames[index];
        }


        public override void OnButtonClick(int buttonID)
        {
            CharCreationGump charCreationGump = UIManager.GetGump<CharCreationGump>();

            if (charCreationGump == null)
            {
                return;
            }

            switch ((Buttons) buttonID)
            {
                case Buttons.PreviousScreen:
                    charCreationGump.StepBack(_selectedProfession > 0 ? 2 : 1);

                    return;

                case Buttons.Finish:

                    if (_selectedCity != null)
                    {
                        charCreationGump.SetCity(_selectedCity.Index);
                    }

                    charCreationGump.CreateCharacter(_selectedProfession);
                    charCreationGump.IsVisible = false;

                    return;
            }

            if (buttonID >= 2)
            {
                buttonID -= 2;
                SetCity(buttonID);
                SetSelectedLabel(buttonID);
            }
        }

        private void SetSelectedLabel(int index)
        {
            for (int i = 0; i < _cityControls.Count; i++)
            {
                _cityControls[i].IsSelected = index == i;
            }
        }

        private enum Buttons
        {
            PreviousScreen,
            Finish
        }

        private class CityControl : Control
        {
            private readonly Button _button;
            private bool _isSelected;
            private readonly HoveredLabel _label;

            public CityControl(CityInfo c, int index)
            {
                Width = 30;
                Height = 30;
                CanMove = false;

                Add
                (
                    _button = new Button(2 + index, 0x04B9, 0x04BA, 0x04BA)
                    {
                        ButtonAction = ButtonAction.Activate,
                        X = 0,
                        Y = 0
                    }
                );

                _label = new HoveredLabel
                (
                    c.City,
                    false,
                    0x0058,
                    0x0099,
                    0x0481,
                    font: 3
                )
                {
                    Y = -20,
                    Tag = index
                };

                // Centraliza e desloca 5 pixels para a esquerda
                _label.X = ((Width - _label.Width) / 2) - 20;

                if (X + _label.X + _label.Width >= 608)
                {
                    _label.X = 608 - (int)X - _label.Width;
                }

                _label.MouseUp += (sender, e) =>
                {
                    _label.IsSelected = true;
                    int idx = (int) _label.Tag;
                    OnButtonClick(idx + 2);
                };

                Add(_label);
            }


            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected != value)
                    {
                        _isSelected = value;
                        _label.IsSelected = value;
                        _button.IsClicked = value;
                    }
                }
            }


            public override void Update()
            {
                base.Update();

                if (!_isSelected)
                {
                    _button.IsClicked = _button.MouseIsOver || _label.MouseIsOver;
                    _label.ForceHover = _button.MouseIsOver;
                }
            }
        }
    }
}
