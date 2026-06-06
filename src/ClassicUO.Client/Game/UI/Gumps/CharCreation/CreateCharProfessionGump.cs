// SPDX-License-Identifier: BSD-2-Clause

using System;
using ClassicUO.Configuration;
using System.Collections.Generic;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps.Login;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Assets;

namespace ClassicUO.Game.UI.Gumps.CharCreation
{
    public class CreateCharProfessionGump : Gump
    {
        private readonly ProfessionInfo _Parent;

        public static CreateCharProfessionGump Instance { get; private set; }

        public CreateCharProfessionGump(World world, ProfessionInfo parent = null) : base(world, 0, 0)
        {
            Instance?.Dispose();
            Instance = this;

            UIManager.GetGump<LoginGump>()?.Dispose();
            UIManager.GetGump<ServerSelectionGump>()?.Dispose();
            UIManager.GetGump<CharacterSelectionGump>()?.Dispose();

            // Define o gump como o container da tela inteira (1024x768)
            Width = 1024;
            Height = 768;
            CanCloseWithRightClick = false;

            _Parent = parent;

            if (parent == null || !Client.Game.UO.FileManager.Professions.Professions.TryGetValue(parent, out List<ProfessionInfo> professions) || professions == null)
            {
                professions = new List<ProfessionInfo>(Client.Game.UO.FileManager.Professions.Professions.Keys);
            }

            /* Build the gump */
            Add
            (
                new ResizePic(2600)
                {
                    X = 215,
                    Y = 175,
                    Width = 470,
                    Height = 372
                }
            );

            //Add(new GumpPic(171, -49, 0x0589, 0));
            //Add(new GumpPic(94, -33, 0x058B, 0));
            //Add(new GumpPic(180, -40, 0x15A9, 0));

            ClilocLoader localization = Client.Game.UO.FileManager.Clilocs;

            bool isAsianLang = string.Compare(Settings.GlobalSettings.Language, "CHT", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                string.Compare(Settings.GlobalSettings.Language, "KOR", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                string.Compare(Settings.GlobalSettings.Language, "JPN", StringComparison.InvariantCultureIgnoreCase) == 0;

            bool unicode = isAsianLang;
            byte font = 1;
            ushort hue = (ushort)(isAsianLang ? 0xFFFF : 0x0386);

            Add
            (
                new Label(localization.GetString(3000326, "Choose a class for your character"), unicode, hue, font: font)
                {
                    X = 325,
                    Y = 200
                }
            );

            for (int i = 0; i < professions.Count; i++)
            {
                int cx = i % 2;
                int cy = i >> 1;

                Add
                (
                    new ProfessionInfoGump(professions[i])
                    {
                        X = 260 + cx * 195,
                        Y = 260 + cy * 70,

                        Selected = SelectProfession
                    }
                );
            }

            Add
            (
                new Button((int) Buttons.Prev, 0x15A1, 0x15A3, 0x15A2)
                {
                    X = 5,
                    Y = 650,
                    ButtonAction = ButtonAction.Activate
                }
            );
            
            Add
            (
                new Button((int) 1, 0x15A4, 0x15A6, 0x15A5) // Next Button (Place-holder se necessário)
                {
                    X = 870,
                    Y = 650,
                    ButtonAction = ButtonAction.Activate
                }
            );
        }

        public void SelectProfession(ProfessionInfo info)
        {
            if (info.Type == ProfessionLoader.PROF_TYPE.CATEGORY && Client.Game.UO.FileManager.Professions.Professions.TryGetValue(info, out List<ProfessionInfo> list) && list != null)
            {
                Parent.Add(new CreateCharProfessionGump(World, info));
                Parent.Remove(this);
            }
            else
            {
                CharCreationGump charCreationGump = UIManager.GetGump<CharCreationGump>();

                charCreationGump?.SetProfession(info);
            }
        }

        public override void OnButtonClick(int buttonID)
        {
            switch ((Buttons) buttonID)
            {
                case Buttons.Prev:

                {
                    if (_Parent != null && _Parent.TopLevel)
                    {
                        Parent.Add(new CreateCharProfessionGump(World));
                        Parent.Remove(this);
                    }
                    else
                    {
                        Parent.Remove(this);
                        CharCreationGump charCreationGump = UIManager.GetGump<CharCreationGump>();
                        charCreationGump?.StepBack();
                    }

                    break;
                }
            }

            base.OnButtonClick(buttonID);
        }

        private enum Buttons
        {
            Prev
        }
    }

    public class ProfessionInfoGump : Control
    {
        private readonly ProfessionInfo _info;

        public ProfessionInfoGump(ProfessionInfo info)
        {
            _info = info;

            ClilocLoader localization = Client.Game.UO.FileManager.Clilocs;

            var background = new ResizePic(3000)
            {
                Width = 175,
                Height = 34
            };

            background.SetTooltip(localization.GetString(info.Description), 250);

            Add(background);

            Add
            (
                new Label(localization.GetString(info.Localization), true, 0x00, font: 1)
                {
                    X = 7,
                    Y = 8
                }
            );

            Add(new GumpPic(121, -12, info.Graphic, 0));
        }

        public Action<ProfessionInfo> Selected;

        public override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            base.OnMouseUp(x, y, button);

            if (button == MouseButtonType.Left)
            {
                Selected?.Invoke(_info);
            }
        }
    }
}
