// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Assets;
using ClassicUO.Resources;
using ClassicUO.Utility;
using SDL3;
using System.Collections.Generic;
using ClassicUO.Network;
using ClassicUO.Game.UI.Gumps.CharCreation;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps.Login
{
    public class CharacterSelectionGump : Gump
    {
        private const ushort SELECTED_COLOR = 0x941; // 0x0035 Cor do nome (Dourado)
        private const ushort SELECTED_CHAR_COLOR = 0x921; // 0x0481 Cor do corpo (Branco Gelo)
        private const float SELECTED_CHAR_ALPHA = 0.5f; // Camada alfa para o personagem selecionado (50%)
        private const ushort NORMAL_COLOR = 0x0481;
        private uint _selectedCharacter;
        private CharacterEntryGump[] chars;

        public static CharacterSelectionGump Instance { get; private set; }

        public CharacterSelectionGump(World world) : base(world, 0, 0)
        {
            Instance?.Dispose();
            Instance = this;

            UIManager.GetGump<ServerSelectionGump>()?.Dispose();
            UIManager.GetGump<LoginGump>()?.Dispose();
            UIManager.GetGump<CharCreationGump>()?.Dispose();

            CanCloseWithRightClick = false;

            // Define o gump como o container da tela inteira para evitar snapping no canto
            Width = 1024;
            Height = 768;

            LoginScene loginScene = Client.Game.GetScene<LoginScene>();
            string lastCharName = LastCharacterManager.GetLastCharacter(LoginScene.Account, World.ServerName);
            string lastSelected = loginScene.Characters.FirstOrDefault(o => o == lastCharName);

            if (!string.IsNullOrEmpty(lastSelected))
            {
                _selectedCharacter = (uint)Array.IndexOf(loginScene.Characters, lastSelected);
            }
            else if (loginScene.Characters != null && loginScene.Characters.Length > 0)
            {
                _selectedCharacter = 0;
            }

            bool isAsianLang = string.Compare(Settings.GlobalSettings.Language, "CHT", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                string.Compare(Settings.GlobalSettings.Language, "KOR", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                string.Compare(Settings.GlobalSettings.Language, "JPN", StringComparison.InvariantCultureIgnoreCase) == 0;

            // Título no topo
            Add(new Label(Client.Game.UO.FileManager.Clilocs.GetString(3000050, "Character Selection"), isAsianLang, 0x0481, 1024, (byte)(isAsianLang ? 1 : 9), align: TEXT_ALIGN_TYPE.TS_CENTER)
            {
                X = 0,
                Y = 70
            });

            const int slotWidth = 120; 
            const int centerX = 490;   // (1024 / 2) - (120 / 2) para centralizar o slot de 120px perfeitamente
            
            var gumps = new List<CharacterEntryGump>();

            for (int i = 0; i < loginScene.Characters.Length; i++)
            {
                // Verifica se o slot é permitido pelas características do cliente
                if (i >= World.ClientFeatures.MaxChars)
                    continue;

                // Verifica slots extras bloqueados (6º e 7º slots).
                // Ajustado para garantir que se o 7º slot estiver liberado, o 6º também apareça.
                if (World.ClientLockedFeatures.Flags != 0)
                {
                    bool has6th = World.ClientLockedFeatures.Flags.HasFlag(LockedFeatureFlags.SixthCharacterSlot);
                    bool has7th = World.ClientLockedFeatures.Flags.HasFlag(LockedFeatureFlags.SeventhCharacterSlot);

                    if (i == 5 && !has6th && !has7th)
                        continue;

                    if (i == 6 && !has7th)
                        continue;
                }

                string character = loginScene.Characters[i];

                if (character.NotNullNotEmpty())
                {
                    LemCharData? LEMData = LastEquipmentManager.Load(LoginHandshake.Instance.LastServerName, character, LoginHandshake.Account);

                    StaticPaperDollView view = null;
                    if (LEMData.HasValue)
                    {
                        var equipment = new Dictionary<Layer, StaticPaperDollView.EquipmentEntry>();
                        foreach (KeyValuePair<Layer, LemEquipmentEntry> kvp in LEMData.Value.Equipment)
                        {
                            equipment[kvp.Key] = new StaticPaperDollView.EquipmentEntry(
                                kvp.Value.AnimID, kvp.Value.Hue, kvp.Value.IsPartialHue);
                        }

                        view = new StaticPaperDollView(
                            LEMData.Value.PlayerGraphic,
                            LEMData.Value.BodyHue,
                            LEMData.Value.IsFemale,
                            equipment,
                            new Vector2(216, 306), // Redução de 10% (240 * 0.9 e 340 * 0.9)
                            false);
                    }

                    // Lógica de distribuição fixa baseada no índice do slot (i)
                    // 0=Centro, 1=Direita, 2=Esquerda, 3=Direita+1, 4=Esquerda+1...
                    int multiplier = (i + 1) / 2;
                    if (i % 2 == 0) multiplier *= -1;

                    CharacterEntryGump g;
                    Add(g = new CharacterEntryGump((uint)i, character, view, SelectCharacter, LoginCharacter)
                    {
                        X = centerX + (multiplier * slotWidth), // Agora o slot i=0 ficará no 452, ocupando até 572 (centralizado)
                        Y = 220 + (Math.Abs(multiplier) * 12), // Ajustado Y para não ficar tão baixo (384 é o meio, o slot deve começar acima)
                        Hue = i == _selectedCharacter ? SELECTED_COLOR : NORMAL_COLOR
                    });
                    gumps.Add(g);
                }
            }
            chars = gumps.ToArray();

            if (CanCreateChar(loginScene))
            {
                Add
                (
                    new Button((int)Buttons.New, 0x159D, 0x159F, 0x159E)
                    {
                        X = 480, // Metade do botão para ficar no centro exato da tela
                        Y = 620,
                        ButtonAction = ButtonAction.Activate
                    }
                );
            }

            Add
            (
                new Button((int)Buttons.Delete, 0x159A, 0x159C, 0x159B)
                {
                    X = 480,
                    Y = 650,
                    ButtonAction = ButtonAction.Activate
                }
            );

            Add
            (
                new Button((int)Buttons.Prev, 0x15A1, 0x15A3, 0x15A2)
                {
                    X = 72, Y = 650, ButtonAction = ButtonAction.Activate
                }
            );

            Add
            (
                new Button((int)Buttons.Next, 0x15A4, 0x15A6, 0x15A5)
                {
                    X = 937, Y = 650, ButtonAction = ButtonAction.Activate
                }
            );

            AcceptKeyboardInput = true;
        }

        private bool CanCreateChar(LoginScene scene)
        {
            if (scene.Characters != null)
            {
                int empty = scene.Characters.Count(string.IsNullOrEmpty);

                if (empty >= 0 && scene.Characters.Length - empty < World.ClientFeatures.MaxChars)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void OnControllerButtonUp(SDL.SDL_GamepadButton button)
        {
            base.OnControllerButtonUp(button);

            if (button == SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_SOUTH)
            {
                LoginCharacter(_selectedCharacter);
            }
        }

        public override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            if (key == SDL.SDL_Keycode.SDLK_RETURN || key == SDL.SDL_Keycode.SDLK_KP_ENTER)
            {
                LoginCharacter(_selectedCharacter);
            }
        }

        public override void OnMouseWheel(MouseEventType delta)
        {
            base.OnMouseWheel(delta);

            int i = 0;
            foreach (CharacterEntryGump characterGump in chars)
            {
                if (characterGump.CharacterIndex == _selectedCharacter)
                    break;
                i++;
            }

            if (MouseEventType.WheelScrollUp == delta)
            {
                if (i == 0)
                    _selectedCharacter = chars[chars.Length - 1].CharacterIndex;
                else
                    _selectedCharacter = chars[i - 1].CharacterIndex;
            }
            else
            {
                if (i == chars.Length - 1)
                    _selectedCharacter = chars[0].CharacterIndex;
                else
                    _selectedCharacter = chars[i + 1].CharacterIndex;
            }
            SelectCharacter(_selectedCharacter);
        }
        public override void OnButtonClick(int buttonID)
        {
            LoginScene loginScene = Client.Game.GetScene<LoginScene>();

            switch ((Buttons)buttonID)
            {
                case Buttons.Delete:
                    DeleteCharacter(loginScene);

                    break;

                case Buttons.New when CanCreateChar(loginScene):
                    loginScene.StartCharCreation();

                    break;

                case Buttons.Next:
                    LoginCharacter(_selectedCharacter);

                    break;

                case Buttons.Prev:
                    loginScene.StepBack();

                    break;
            }

            base.OnButtonClick(buttonID);
        }

        private void DeleteCharacter(LoginScene loginScene)
        {
            string charName = loginScene.Characters[_selectedCharacter];

            if (!string.IsNullOrEmpty(charName))
            {
                // Remove instâncias anteriores para evitar sobreposição ou vazamento de memória
                foreach (var child in Children.OfType<LoadingGump>().ToList())
                {
                    Remove(child);
                }

                LoadingGump confirmGump = null;
                confirmGump = new LoadingGump
                (
                    World,
                    string.Format(ResGumps.PermanentlyDelete0, charName),
                    LoginButtons.OK | LoginButtons.Cancel,
                    buttonID =>
                    {
                        if (buttonID == (int)LoginButtons.OK)
                        {
                            loginScene.DeleteCharacter(_selectedCharacter);
                            loginScene.StepBack();
                        }
                        Remove(confirmGump);
                    }
                );

                // Centraliza o gump de confirmação na tela (1024x768)
                confirmGump.X = (Width - confirmGump.Width) / 2;
                confirmGump.Y = (Height - confirmGump.Height) / 2;

                Add(confirmGump);
            }
        }

        private void SelectCharacter(uint index)
        {
            _selectedCharacter = index;

            foreach (CharacterEntryGump characterGump in FindControls<CharacterEntryGump>())
            {
                characterGump.Hue = characterGump.CharacterIndex == index ? SELECTED_COLOR : NORMAL_COLOR;
            }
        }

        private void LoginCharacter(uint index)
        {
            LoginScene loginScene = Client.Game.GetScene<LoginScene>();

            if (loginScene.Characters != null && loginScene.Characters.Length > index && !string.IsNullOrEmpty(loginScene.Characters[index]))
            {
                loginScene.SelectCharacter(index);
            }
        }

        private enum Buttons
        {
            New,
            Delete,
            Next,
            Prev
        }

        private class CharacterEntryGump : Control
        {
            private readonly Label _label;
            private readonly StaticPaperDollView _view;
            private readonly Action<uint> _loginFn;
            private readonly Action<uint> _selectedFn;

            public CharacterEntryGump(uint index, string character, StaticPaperDollView view, Action<uint> selectedFn, Action<uint> loginFn)
            {
                CharacterIndex = index;
                _selectedFn = selectedFn;
                _loginFn = loginFn;

                Width = 120; // Largura ajustada para casar com o novo slotWidth
                Height = 350;

                // Sombra em elipse (Gump 0x0B4F). Hue 1 garante que seja preta.
                Add(new GumpPic((Width - 64) / 2, 192, 0x0B4F, 1)
                {
                    Alpha = 0.5f, // Aumentado para 50% para melhor visibilidade
                    AcceptMouseInput = false
                });

                _view = view;
                if (_view != null)
                {
                    _view.X = (Width - 216) / 2;
                    _view.Y = 0;
                    _view.AcceptMouseInput = false;
                    _view.SelectionHue = SELECTED_CHAR_COLOR;
                    Add(_view);
                }

                // Char Name
                Add(_label = new Label(character, false, NORMAL_COLOR, Width, 9, FontStyle.BlackBorder, align: TEXT_ALIGN_TYPE.TS_CENTER)
                {
                    X = -40, // Como a Label tem a mesma Width (120) do gump e TS_CENTER, X=0 centraliza o texto perfeitamente
                    Y = 200, // Ajustado proporcionalmente à redução da altura do personagem
                    AcceptMouseInput = false
                });

                AcceptMouseInput = true;
            }

            public uint CharacterIndex { get; }

            public ushort Hue
            {
                get => _label.Hue;
                set
                {
                    _label.Hue = value;
                    if (_view != null)
                    {
                        _view.IsSelected = value != SELECTED_COLOR;
                        _view.Alpha = value == SELECTED_COLOR ? 1.0f : SELECTED_CHAR_ALPHA;
                    }
                }
            }

            public override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
            {
                if (button == MouseButtonType.Left)
                {
                    _loginFn(CharacterIndex);

                    return true;
                }

                return false;
            }


            public override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                if (button == MouseButtonType.Left)
                {
                    _selectedFn(CharacterIndex);
                }
            }
        }
    }
}
