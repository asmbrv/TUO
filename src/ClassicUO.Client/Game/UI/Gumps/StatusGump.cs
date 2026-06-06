// SPDX-License-Identifier: BSD-2-Clause

using System;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Assets;
using ClassicUO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps
{
    public abstract class StatusGumpBase : Gump
    {
        protected const ushort LOCK_UP_GRAPHIC = 0x0984;
        protected const ushort LOCK_DOWN_GRAPHIC = 0x0986;
        protected const ushort LOCK_LOCKED_GRAPHIC = 0x082C;


        protected StatusGumpBase(World world) : base(world, 0, 0)
        {
            if (ProfileManager.CurrentProfile.StatusGumpBarMutuallyExclusive)
            {
                // sanity check
                UIManager.GetGump<HealthBarGump>(World.Player)?.Dispose();
            }

            CanCloseWithRightClick = true;
            CanMove = true;
            if (ProfileManager.CurrentProfile != null)
            {
                Location = ProfileManager.CurrentProfile.StatusGumpPosition;
                IsLocked = ProfileManager.CurrentProfile.StatusGumpLocked;
            }

        }

        public override GumpType GumpType => GumpType.StatusGump;
        protected Label[] _labels;
        protected readonly GumpPic[] _lockers = new GumpPic[3];
        protected Point _point;
        protected long _refreshTime;

        public override void OnButtonClick(int buttonID)
        {
            switch ((ButtonType)buttonID)
            {
                case ButtonType.BuffIcon:
                    if (ProfileManager.CurrentProfile.UseImprovedBuffBar)
                    {
                        Gump gump = UIManager.GetGump<ImprovedBuffGump>();

                        if (gump == null)
                        {
                            UIManager.Add(new ImprovedBuffGump(World));
                        }
                        else
                        {
                            gump.SetInScreen();
                            gump.BringOnTop();
                        }
                    }
                    else
                    {
                        BuffGump gump = UIManager.GetGump<BuffGump>();

                        if (gump == null)
                        {
                            UIManager.Add(new BuffGump(World, 100, 100));
                        }
                        else
                        {
                            gump.SetInScreen();
                            gump.BringOnTop();
                        }
                    }

                    break;
            }
        }


        public override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            base.OnMouseUp(x, y, button);

            if (button == MouseButtonType.Left)
            {
                if (World.TargetManager.IsTargeting)
                {
                    World.TargetManager.Target(World.Player);
                    Mouse.LastLeftButtonClickTime = 0;
                }
                else if (x >= _point.X && x <= Width + 16 && y >= _point.Y && y <= Height + 16)
                {
                    Point offset = Mouse.LDragOffset;

                    if (Math.Abs(offset.X) < 5 && Math.Abs(offset.Y) < 5)
                    {
                        UIManager.GetGump<BaseHealthBarGump>(World.Player)?.Dispose();

                        if (ProfileManager.CurrentProfile.CustomBarsToggled)
                            UIManager.Add(new HealthBarGumpCustom(World, World.Player) { X = ScreenCoordinateX, Y = ScreenCoordinateY });
                        else
                            UIManager.Add(new HealthBarGump(World, World.Player) { X = ScreenCoordinateX, Y = ScreenCoordinateY });

                        if (ProfileManager.CurrentProfile.StatusGumpBarMutuallyExclusive)
                            Dispose();
                    }
                }
            }
        }

        protected override void OnMove(int x, int y)
        {
            base.OnMove(x, y);
            ProfileManager.CurrentProfile.StatusGumpPosition = Location;
        }

        public override void OnMouseDown(int x, int y, MouseButtonType button)
        {
            if (World.TargetManager.IsTargeting)
            {
                World.TargetManager.Target(World.Player);
                Mouse.LastLeftButtonClickTime = 0;
            }
        }

        public static StatusGumpBase GetStatusGump()
        {
            StatusGumpBase gump;

            if (ProfileManager.CurrentProfile.UseOldStatusGump)
            {
                gump = UIManager.GetGump<StatusGumpOld>();
            }
            else
            {
                gump = UIManager.GetGump<StatusGumpModern>();
            }
            return gump;
        }

        public static StatusGumpBase AddStatusGump(World world, int x, int y)
        {
            StatusGumpBase gump;

            if (Client.Game.UO.Version < ClientVersion.CV_308Z || ProfileManager.CurrentProfile.UseOldStatusGump)
            {
                gump = new StatusGumpOld(world);
            }
            else
            {
                gump = new StatusGumpModern(world);
            }

            gump.X = x;
            gump.Y = y;

            return gump;
        }

        protected static ushort GetStatLockGraphic(Lock lockStatus)
        {
            switch (lockStatus)
            {
                case Lock.Up: return LOCK_UP_GRAPHIC;

                case Lock.Down: return LOCK_DOWN_GRAPHIC;

                case Lock.Locked: return LOCK_LOCKED_GRAPHIC;

                default: return 0xFFFF;
            }
        }

        protected override void UpdateContents()
        {
            for (int i = 0; i < 3; i++)
            {
                Lock status = i == 0 ? World.Player.StrLock : i == 1 ? World.Player.DexLock : World.Player.IntLock;

                if (_lockers != null && _lockers[i] != null)
                {
                    _lockers[i].Graphic = GetStatLockGraphic(status);
                }
            }
        }

        protected enum ButtonType
        {
            BuffIcon,
            MinimizeMaximize
        }

        protected enum StatType
        {
            Str,
            Dex,
            Int
        }
    }

    public class StatusGumpOld : StatusGumpBase
    {
        public StatusGumpOld(World world) : base(world)
        {
            Point p = Point.Zero;
            _labels = new Label[(int)MobileStats.NumStats];

            Add(new GumpPic(0, 0, 0x0802, 0));
            p.X = 248;
            p.Y = 410;

            int baseX = 29;
            int baseY = 16;
            int lineHeight = 14;
            int xOffset = Client.Game.UO.FileManager.Gumps.UseUOPGumps ? 14 : 14;
            int attrStartY = baseY + lineHeight * 5;

            if (Client.Game.UO.Version >= ClientVersion.CV_5020)
            {
                Add
                (
                    new Button((int)ButtonType.BuffIcon, 0x7538, 0x7539, 0x7539)
                    {
                        X = 12,
                        Y = baseY,
                        ButtonAction = ButtonAction.Activate
                    }
                );
            }

            // Top block: Name, Gold, Luck
            var lbl = new Label(!string.IsNullOrEmpty(World.Player.Name) ? World.Player.Name : string.Empty, false, 0x6CA, 180, font: 9, align: TEXT_ALIGN_TYPE.TS_CENTER)
            {
                X = 30,
                Y = baseY
            };
            _labels[(int)MobileStats.Name] = lbl;
            Add(lbl);

            lbl = new Label($"Gold: {World.Player.Gold}", false, 0x38E, 180, font: 9)
            {
                X = baseX,
                Y = baseY + lineHeight * 2
            };
            _labels[(int)MobileStats.Gold] = lbl;
            Add(lbl);

            lbl = new Label($"Luck: {World.Player.Luck}", false, 0x38E, 180, font: 9)
            {
                X = baseX,
                Y = baseY + lineHeight * 3
            };
            _labels[(int)MobileStats.Luck] = lbl;
            Add(lbl);

            // Attributes: Str, Dex, Int (with lockers)

            Lock status = World.Player.StrLock;
            ushort gumpID = GetStatLockGraphic(status);
            Add(_lockers[0] = new GumpPic(xOffset, attrStartY, gumpID, 0));
            _lockers[0].MouseUp += (sender, e) =>
            {
                World.Player.StrLock = (Lock)(((byte)World.Player.StrLock + 1) % 3);
                GameActions.ChangeStatLock(0, World.Player.StrLock);
                ushort gumpid = GetStatLockGraphic(World.Player.StrLock);
                _lockers[0].Graphic = gumpid;
            };

            lbl = new Label($"Strength: {World.Player.Strength}", false, 0x38E, 180, font: 9)
            {
                X = baseX,
                Y = attrStartY
            };
            _labels[(int)MobileStats.Strength] = lbl;
            Add(lbl);

            status = World.Player.DexLock;
            gumpID = GetStatLockGraphic(status);
            Add(_lockers[1] = new GumpPic(xOffset, attrStartY + lineHeight, gumpID, 0));
            _lockers[1].MouseUp += (sender, e) =>
            {
                World.Player.DexLock = (Lock)(((byte)World.Player.DexLock + 1) % 3);
                GameActions.ChangeStatLock(1, World.Player.DexLock);
                ushort gumpid = GetStatLockGraphic(World.Player.DexLock);
                _lockers[1].Graphic = gumpid;
            };

            lbl = new Label($"Dexterity: {World.Player.Dexterity}", false, 0x38E, 180, font: 9)
            {
                X = baseX,
                Y = attrStartY + lineHeight
            };
            _labels[(int)MobileStats.Dexterity] = lbl;
            Add(lbl);

            status = World.Player.IntLock;
            gumpID = GetStatLockGraphic(status);
            Add(_lockers[2] = new GumpPic(xOffset, attrStartY + lineHeight * 2, gumpID, 0));
            _lockers[2].MouseUp += (sender, e) =>
            {
                World.Player.IntLock = (Lock)(((byte)World.Player.IntLock + 1) % 3);
                GameActions.ChangeStatLock(2, World.Player.IntLock);
                ushort gumpid = GetStatLockGraphic(World.Player.IntLock);
                _lockers[2].Graphic = gumpid;
            };

            lbl = new Label($"Intelligence: {World.Player.Intelligence}", false, 0x38E, 180, font: 9)
            {
                X = baseX,
                Y = attrStartY + lineHeight * 2
            };
            _labels[(int)MobileStats.Intelligence] = lbl;
            Add(lbl);

            Add(new Label(string.Empty, false, 0x0481, 180, font: 9, align: TEXT_ALIGN_TYPE.TS_CENTER)
            {
                X = baseX,
                Y = attrStartY + lineHeight * 3
            });

            // Health / Stamina / Mana / Weight (current / max)
            int miscStart = attrStartY + lineHeight * 4;

            lbl = new Label($"Health: {World.Player.Hits}/{World.Player.HitsMax}", false, 0x38E, 180, font: 9)
            {
                X = baseX,
                Y = miscStart
            };
            _labels[(int)MobileStats.HealthCurrent] = lbl;
            _labels[(int)MobileStats.HealthMax] = new Label($"Max: {World.Player.HitsMax}", false, 0x38E, 180, font: 9) { X = baseX + 120, Y = miscStart };
            Add(lbl);
            //Add(_labels[(int)MobileStats.HealthMax]);

            lbl = new Label($"Stamina: {World.Player.Stamina}/{World.Player.StaminaMax}", false, 0x38E, 180, font: 9)
            {
                X = baseX,
                Y = miscStart + lineHeight
            };
            _labels[(int)MobileStats.StaminaCurrent] = lbl;
            _labels[(int)MobileStats.StaminaMax] = new Label($"Max: {World.Player.StaminaMax}", false, 0x38E, 180, font: 9) { X = baseX + 120, Y = miscStart + lineHeight };
            Add(lbl);
            //Add(_labels[(int)MobileStats.StaminaMax]);

            lbl = new Label($"Mana: {World.Player.Mana}/{World.Player.ManaMax}", false, 0x38E, 180, font: 9)
            {
                X = baseX,
                Y = miscStart + lineHeight * 2
            };
            _labels[(int)MobileStats.ManaCurrent] = lbl;
            _labels[(int)MobileStats.ManaMax] = new Label($"Max: {World.Player.ManaMax}", false, 0x38E, 180, font: 9) { X = baseX + 120, Y = miscStart + lineHeight * 2 };
            Add(lbl);
            //Add(_labels[(int)MobileStats.ManaMax]);

            lbl = new Label($"Weight: {World.Player.Weight}/{World.Player.WeightMax}", false, 0x38E, 180, font: 9)
            {
                X = baseX,
                Y = miscStart + lineHeight * 3
            };
            _labels[(int)MobileStats.WeightCurrent] = lbl;
            _labels[(int)MobileStats.WeightMax] = new Label($"Max: {World.Player.WeightMax}", false, 0x38E, 180, font: 9) { X = baseX + 120, Y = miscStart + lineHeight * 3 };
            Add(lbl);
            //Add(_labels[(int)MobileStats.WeightMax]);

            // Combat stats
            int combatStart = miscStart + lineHeight * 5;

            lbl = new Label($"Damage: {World.Player.DamageMin}-{World.Player.DamageMax}", false, 0x38E, 180, font: 9) { X = baseX, Y = combatStart };
            _labels[(int)MobileStats.Damage] = lbl;
            Add(lbl);

            lbl = new Label($"Hit Chance Inc: {World.Player.HitChanceIncrease}", false, 0x38E, 180, font: 9) { X = baseX, Y = combatStart + lineHeight };
            _labels[(int)MobileStats.HitChanceInc] = lbl;
            Add(lbl);

            lbl = new Label($"Damage Chance Inc: {World.Player.DamageIncrease}", false, 0x38E, 180, font: 9) { X = baseX, Y = combatStart + lineHeight * 2 };
            _labels[(int)MobileStats.DamageChanceInc] = lbl;
            Add(lbl);

            lbl = new Label($"Swing Speed Inc: {World.Player.SwingSpeedIncrease}", false, 0x38E, 180, font: 9) { X = baseX, Y = combatStart + lineHeight * 3 };
            _labels[(int)MobileStats.SwingSpeedInc] = lbl;
            Add(lbl);

            lbl = new Label($"Spell Damage Inc: {World.Player.SpellDamageIncrease}", false, 0x38E, 180, font: 9) { X = baseX, Y = combatStart + lineHeight * 4 };
            _labels[(int)MobileStats.SpellDamageInc] = lbl;
            Add(lbl);

            // Resistances and defense chance
            int resStart = combatStart + lineHeight * 6;

            lbl = new Label($"AR: {World.Player.PhysicalResistance}", false, 0x38E, 180, font: 9) { X = baseX, Y = resStart };
            _labels[(int)MobileStats.AR] = lbl;
            Add(lbl);

            lbl = new Label($"RF: {World.Player.FireResistance}", false, 0x38E, 180, font: 9) { X = baseX, Y = resStart + lineHeight };
            _labels[(int)MobileStats.RF] = lbl;
            Add(lbl);

            lbl = new Label($"RC: {World.Player.ColdResistance}", false, 0x38E, 180, font: 9) { X = baseX, Y = resStart + lineHeight * 2 };
            _labels[(int)MobileStats.RC] = lbl;
            Add(lbl);

            lbl = new Label($"RP: {World.Player.PoisonResistance}", false, 0x38E, 180, font: 9) { X = baseX, Y = resStart + lineHeight * 3 };
            _labels[(int)MobileStats.RP] = lbl;
            Add(lbl);

            lbl = new Label($"RE: {World.Player.EnergyResistance}", false, 0x38E, 180, font: 9) { X = baseX, Y = resStart + lineHeight * 4 };
            _labels[(int)MobileStats.RE] = lbl;
            Add(lbl);

            lbl = new Label($"Defense Chance Inc: {World.Player.DefenseChanceIncrease}", false, 0x38E, 180, font: 9) { X = baseX, Y = resStart + lineHeight * 5 };
            _labels[(int)MobileStats.DefenseChanceInc] = lbl;
            Add(lbl);

            _point = p;
        }

        public override void Update()
        {
            if (IsDisposed)
                return;

            if (_refreshTime < Time.Ticks)
            {
                _refreshTime = (long)Time.Ticks + 250;

                _labels[(int)MobileStats.Name].Text = $"{(!string.IsNullOrEmpty(World.Player.Name) ? World.Player.Name : string.Empty)}";
                _labels[(int)MobileStats.Gold].Text = $"Gold: {World.Player.Gold}";
                _labels[(int)MobileStats.Luck].Text = $"Luck: {World.Player.Luck}";

                _labels[(int)MobileStats.Strength].Text = $"Strength: {World.Player.Strength}";
                _labels[(int)MobileStats.Dexterity].Text = $"Dexterity: {World.Player.Dexterity}";
                _labels[(int)MobileStats.Intelligence].Text = $"Intelligence: {World.Player.Intelligence}";

                _labels[(int)MobileStats.HealthCurrent].Text = $"Health: {World.Player.Hits}/{World.Player.HitsMax}";
                //_labels[(int)MobileStats.HealthMax].Text = $"Max: {World.Player.HitsMax}";

                _labels[(int)MobileStats.StaminaCurrent].Text = $"Stamina: {World.Player.Stamina}/{World.Player.StaminaMax}";
                //_labels[(int)MobileStats.StaminaMax].Text = $"Max: {World.Player.StaminaMax}";

                _labels[(int)MobileStats.ManaCurrent].Text = $"Mana: {World.Player.Mana}/{World.Player.ManaMax}";
                //_labels[(int)MobileStats.ManaMax].Text = $"Max: {World.Player.ManaMax}";

                _labels[(int)MobileStats.WeightCurrent].Text = $"Weight: {World.Player.Weight}/{World.Player.WeightMax}";
                //_labels[(int)MobileStats.WeightMax].Text = $"Max: {World.Player.WeightMax}";

                _labels[(int)MobileStats.Damage].Text = $"Damage: {World.Player.DamageMin}-{World.Player.DamageMax}";
                _labels[(int)MobileStats.HitChanceInc].Text = $"Hit Chance: {World.Player.HitChanceIncrease}";
                _labels[(int)MobileStats.DamageChanceInc].Text = $"Damage Chance: {World.Player.DamageIncrease}";
                _labels[(int)MobileStats.SwingSpeedInc].Text = $"Swing Speed: {World.Player.SwingSpeedIncrease}";
                _labels[(int)MobileStats.SpellDamageInc].Text = $"Spell Damage: {World.Player.SpellDamageIncrease}";

                _labels[(int)MobileStats.AR].Text = $"Physical Resistance: {World.Player.PhysicalResistance}";
                _labels[(int)MobileStats.RF].Text = $"Fire Resistance: {World.Player.FireResistance}";
                _labels[(int)MobileStats.RC].Text = $"Cold Resistance: {World.Player.ColdResistance}";
                _labels[(int)MobileStats.RP].Text = $"Posion Resistance: {World.Player.PoisonResistance}";
                _labels[(int)MobileStats.RE].Text = $"Energy Resistance: {World.Player.EnergyResistance}";
                _labels[(int)MobileStats.DefenseChanceInc].Text = $"Defense Chance: {World.Player.DefenseChanceIncrease}";
            }

            base.Update();
        }


        private enum MobileStats
        {
            Name,
            Gold,
            Luck,

            Strength,
            Dexterity,
            Intelligence,

            HealthCurrent,
            HealthMax,
            StaminaCurrent,
            StaminaMax,
            ManaCurrent,
            ManaMax,
            WeightCurrent,
            WeightMax,

            Damage,
            HitChanceInc,
            DamageChanceInc,
            SwingSpeedInc,
            SpellDamageInc,

            AR,
            RF,
            RC,
            RP,
            RE,
            DefenseChanceInc,

            NumStats
        }
    }

    public class StatusGumpModern : StatusGumpBase
    {
        public StatusGumpModern(World world) : base(world)
        {
            Point p = Point.Zero;
            int xOffset = 0;
            _labels = new Label[(int)MobileStats.NumStats];

            Add(new GumpPic(0, 0, Settings.Graphic_Background, Settings.Hue_Background));

            if (Client.Game.UO.Version >= ClientVersion.CV_308Z)
            {
                p.X = 389;
                p.Y = 152;

                string name = World.Player.Name ?? string.Empty;
                if (!string.IsNullOrEmpty(World.Player.Title))
                    name = World.Player.Title.Contains(name, StringComparison.OrdinalIgnoreCase) ? World.Player.Title : $"{name}, {World.Player.Title}";

                AddStatTextLabel
                (
                    name,
                    MobileStats.Name,
                    Client.Game.UO.FileManager.Gumps.UseUOPGumps ? 90 : 58,
                    42,
                    320,
                    0x6CA,
                    TEXT_ALIGN_TYPE.TS_CENTER
                );


                if (Client.Game.UO.Version >= ClientVersion.CV_5020)
                {
                    Add
                    (
                        new Button((int)ButtonType.BuffIcon, Settings.Graphic_BuffIcon_Normal, Settings.Graphic_BuffIcon_Pressed, Settings.Graphic_BuffIcon_Hover)
                        {
                            X = 19,
                            Y = 43,
                            ButtonAction = ButtonAction.Activate
                        }
                    );
                }

                Lock status = World.Player.StrLock;
                xOffset = Client.Game.UO.FileManager.Gumps.UseUOPGumps ? 20 : 20;
                ushort gumpID = GetStatLockGraphic(status);

                Add(_lockers[0] = new GumpPic(xOffset, 76, gumpID, 0));

                _lockers[0].MouseUp += (sender, e) =>
                {
                    World.Player.StrLock = (Lock)(((byte)World.Player.StrLock + 1) % 3);
                    GameActions.ChangeStatLock(0, World.Player.StrLock);
                    ushort gumpid = GetStatLockGraphic(World.Player.StrLock);

                    _lockers[0].Graphic = gumpid;
                };

                //AddChildren(_lockers[0] = new Button((int)ButtonType.LockerStr, gumpID, gumpID)
                //{
                //    X = xOffset,
                //    Y = 76,
                //    ButtonAction = ButtonAction.Activate,
                //});
                status = World.Player.DexLock;
                xOffset = Client.Game.UO.FileManager.Gumps.UseUOPGumps ? 20 : 20;
                gumpID = GetStatLockGraphic(status);

                Add(_lockers[1] = new GumpPic(xOffset, 102, gumpID, 0));

                _lockers[1].MouseUp += (sender, e) =>
                {
                    World.Player.DexLock = (Lock)(((byte)World.Player.DexLock + 1) % 3);
                    GameActions.ChangeStatLock(1, World.Player.DexLock);
                    ushort gumpid = GetStatLockGraphic(World.Player.DexLock);

                    _lockers[1].Graphic = gumpid;
                };

                //AddChildren(_lockers[1] = new Button((int)ButtonType.LockerDex, gumpID, gumpID)
                //{
                //    X = xOffset,
                //    Y = 102,
                //    ButtonAction = ButtonAction.Activate
                //});
                status = World.Player.IntLock;
                xOffset = Client.Game.UO.FileManager.Gumps.UseUOPGumps ? 20 : 20;
                gumpID = GetStatLockGraphic(status);

                Add(_lockers[2] = new GumpPic(xOffset, 132, gumpID, 0));

                _lockers[2].MouseUp += (sender, e) =>
                {
                    World.Player.IntLock = (Lock)(((byte)World.Player.IntLock + 1) % 3);
                    GameActions.ChangeStatLock(2, World.Player.IntLock);
                    ushort gumpid = GetStatLockGraphic(World.Player.IntLock);

                    _lockers[2].Graphic = gumpid;
                };
                //AddChildren(_lockers[2] = new Button((int)ButtonType.LockerInt, gumpID, gumpID)
                //{
                //    X = xOffset,
                //    Y = 132,
                //    ButtonAction = ButtonAction.Activate
                //});

                if (Client.Game.UO.FileManager.Gumps.UseUOPGumps)
                {
                    xOffset = 80;
                    AddStatTextLabel(World.Player.HitChanceIncrease.ToString(), MobileStats.HitChanceInc, xOffset, 161);

                    Add
                    (
                        new HitBox
                        (
                            58,
                            154,
                            59,
                            24,
                            Client.Game.UO.FileManager.Clilocs.GetString(1075616, ResGumps.HitChanceIncrease),
                            0
                        )
                        { CanMove = true }
                    );
                }
                else
                {
                    xOffset = 68;
                }


                AddStatTextLabel(World.Player.Strength.ToString(), MobileStats.Strength, xOffset, 77);
                AddStatTextLabel(World.Player.Dexterity.ToString(), MobileStats.Dexterity, xOffset, 105);
                AddStatTextLabel(World.Player.Intelligence.ToString(), MobileStats.Intelligence, xOffset, 133);


                Add
                (
                    new HitBox
                    (
                        38,
                        70,
                        59,
                        24,
                        Client.Game.UO.FileManager.Clilocs.GetString(1061146, ResGumps.Strength),
                        0
                    )
                    { CanMove = true }
                );

                Add
                (
                    new HitBox
                    (
                        38,
                        98,
                        59,
                        24,
                        Client.Game.UO.FileManager.Clilocs.GetString(1061147, ResGumps.Dexterity),
                        0
                    )
                    { CanMove = true }
                );

                Add
                (
                    new HitBox
                    (
                        38,
                        126,
                        59,
                        24,
                        Client.Game.UO.FileManager.Clilocs.GetString(1061148, ResGumps.Intelligence),
                        0
                    )
                    { CanMove = true }
                );

                int textWidth = 40;

                if (Client.Game.UO.FileManager.Gumps.UseUOPGumps)
                {
                    xOffset = 150;

                    AddStatTextLabel($"{World.Player.DefenseChanceIncrease}/{World.Player.MaxDefenseChanceIncrease}", MobileStats.DefenseChanceInc, xOffset, 161);

                    Add
                    (
                        new HitBox
                        (
                            124,
                            154,
                            59,
                            24,
                            Client.Game.UO.FileManager.Clilocs.GetString(1075620, ResGumps.DefenseChanceIncrease),
                            0
                        )
                        { CanMove = true }
                    );
                }
                else
                {
                    xOffset = 132;
                }


                xOffset -= 5;

                AddStatTextLabel
                (
                    World.Player.Hits.ToString(),
                    MobileStats.HealthCurrent,
                    xOffset + 5,
                    70,
                    textWidth,
                    alignment: TEXT_ALIGN_TYPE.TS_CENTER
                );

                AddStatTextLabel
                (
                    World.Player.HitsMax.ToString(),
                    MobileStats.HealthMax,
                    xOffset + 5,
                    83,
                    textWidth,
                    alignment: TEXT_ALIGN_TYPE.TS_CENTER
                );

                AddStatTextLabel
                (
                    World.Player.Stamina.ToString(),
                    MobileStats.StaminaCurrent,
                    xOffset + 5,
                    98,
                    textWidth,
                    alignment: TEXT_ALIGN_TYPE.TS_CENTER
                );

                AddStatTextLabel
                (
                    World.Player.StaminaMax.ToString(),
                    MobileStats.StaminaMax,
                    xOffset + 5,
                    111,
                    textWidth,
                    alignment: TEXT_ALIGN_TYPE.TS_CENTER
                );

                AddStatTextLabel
                (
                    World.Player.Mana.ToString(),
                    MobileStats.ManaCurrent,
                    xOffset + 5,
                    126,
                    textWidth,
                    alignment: TEXT_ALIGN_TYPE.TS_CENTER
                );

                AddStatTextLabel
                (
                    World.Player.ManaMax.ToString(),
                    MobileStats.ManaMax,
                    xOffset + 5,
                    139,
                    textWidth,
                    alignment: TEXT_ALIGN_TYPE.TS_CENTER
                );

                xOffset += 5;

                Add
                (
                    new Line
                    (
                        xOffset,
                        138,
                        Math.Abs(xOffset - 168),
                        1,
                        0xFF383838
                    )
                );

                Add
                (
                    new Line
                    (
                        xOffset,
                        110,
                        Math.Abs(xOffset - 168),
                        1,
                        0xFF383838
                    )
                );

                Add
                (
                    new Line
                    (
                        xOffset,
                        82,
                        Math.Abs(xOffset - 168),
                        1,
                        0xFF383838
                    )
                );

                Add
                (
                    new HitBox
                    (
                        110,
                        70,
                        59,
                        24,
                        Client.Game.UO.FileManager.Clilocs.GetString(1061149, ResGumps.HitPoints),
                        0
                    )
                    { CanMove = true }
                );

                Add
                (
                    new HitBox
                    (
                        110,
                        98,
                        59,
                        24,
                        Client.Game.UO.FileManager.Clilocs.GetString(1061150, ResGumps.Stamina),
                        0
                    )
                    { CanMove = true }
                );

                Add
                (
                    new HitBox
                    (
                        110,
                        126,
                        59,
                        24,
                        Client.Game.UO.FileManager.Clilocs.GetString(1061151, ResGeneral.Mana),
                        0
                    )
                    { CanMove = true }
                );

                if (Client.Game.UO.FileManager.Gumps.UseUOPGumps)
                {
                    xOffset = 240;

                    AddStatTextLabel(World.Player.LowerManaCost.ToString(), MobileStats.LowerManaCost, xOffset, 162);

                    Add
                    (
                        new HitBox
                        (
                            205,
                            154,
                            65,
                            24,
                            Client.Game.UO.FileManager.Clilocs.GetString(1075621, ResGumps.LowerManaCost),
                            0
                        )
                        { CanMove = true }
                    );
                }
                else
                {
                    xOffset = 211;
                }

                AddStatTextLabel(World.Player.StatsCap.ToString(), MobileStats.StatCap, xOffset, 77);
                AddStatTextLabel(World.Player.Luck.ToString(), MobileStats.Luck, xOffset, 105);

                xOffset -= 10;

                AddStatTextLabel
                (
                    World.Player.Weight.ToString(),
                    MobileStats.WeightCurrent,
                    xOffset,
                    126,
                    textWidth,
                    alignment: TEXT_ALIGN_TYPE.TS_CENTER
                );

                int lineX = Client.Game.UO.FileManager.Gumps.UseUOPGumps ? 200 : 200;

                Add
                (
                    new Line
                    (
                        lineX,
                        138,
                        Math.Abs(lineX - (Client.Game.UO.FileManager.Gumps.UseUOPGumps ? 238 : 238)),
                        1,
                        0xFF383838
                    )
                );

                AddStatTextLabel
                (
                    World.Player.WeightMax.ToString(),
                    MobileStats.WeightMax,
                    xOffset,
                    139,
                    textWidth,
                    alignment: TEXT_ALIGN_TYPE.TS_CENTER
                );

                xOffset = Client.Game.UO.FileManager.Gumps.UseUOPGumps ? 205 : 188;

                Add
                (
                    new HitBox
                    (
                        xOffset,
                        70,
                        65,
                        24,
                        Client.Game.UO.FileManager.Clilocs.GetString(1061152, ResGumps.MaximumStats),
                        0
                    )
                    { CanMove = true }
                );

                Add
                (
                    new HitBox
                    (
                        xOffset,
                        98,
                        65,
                        24,
                        Client.Game.UO.FileManager.Clilocs.GetString(1061153, ResGumps.Luck),
                        0
                    )
                    { CanMove = true }
                );

                Add
                (
                    new HitBox
                    (
                        xOffset,
                        126,
                        65,
                        24,
                        Client.Game.UO.FileManager.Clilocs.GetString(1061154, ResGeneral.Weight),
                        0
                    )
                    { CanMove = true }
                );

                if (Client.Game.UO.FileManager.Gumps.UseUOPGumps)
                {
                    xOffset = 320;

                    AddStatTextLabel(World.Player.DamageIncrease.ToString(), MobileStats.DamageChanceInc, xOffset + 10, 105);

                    AddStatTextLabel(World.Player.SwingSpeedIncrease.ToString(), MobileStats.SwingSpeedInc, xOffset + 10, 161);

                    Add
                    (
                        new HitBox
                        (
                            285,
                            98,
                            69,
                            24,
                            Client.Game.UO.FileManager.Clilocs.GetString(1075619, ResGumps.WeaponDamageIncrease),
                            0
                        )
                        { CanMove = true }
                    );

                    Add
                    (
                        new HitBox
                        (
                            285,
                            154,
                            69,
                            24,
                            Client.Game.UO.FileManager.Clilocs.GetString(1075629, ResGumps.SwingSpeedIncrease),
                            0
                        )
                        { CanMove = true }
                    );
                }
                else
                {
                    xOffset = 280;

                    AddStatTextLabel(World.Player.Gold.ToString(), MobileStats.Gold, xOffset + 16, 105);

                    Add
                    (
                        new HitBox
                        (
                            260,
                            98,
                            69,
                            24,
                            Client.Game.UO.FileManager.Clilocs.GetString(1061156, ResGumps.Gold),
                            0
                        )
                        { CanMove = true }
                    );
                }

                AddStatTextLabel($"{World.Player.DamageMin}-{World.Player.DamageMax}", MobileStats.Damage, xOffset + 16, 77);

                AddStatTextLabel($"{World.Player.Followers}-{World.Player.FollowersMax}", MobileStats.Followers, xOffset + 16, 133);

                xOffset = Client.Game.UO.FileManager.Gumps.UseUOPGumps ? 285 : 260;

                Add
                (
                    new HitBox
                    (
                        xOffset,
                        70,
                        69,
                        24,
                        Client.Game.UO.FileManager.Clilocs.GetString(1061155, ResGumps.Damage),
                        0
                    )
                    { CanMove = true }
                );

                Add
                (
                    new HitBox
                    (
                        xOffset,
                        126,
                        69,
                        24,
                        Client.Game.UO.FileManager.Clilocs.GetString(1061157, ResGumps.Followers),
                        0
                    )
                    { CanMove = true }
                );

                if (Client.Game.UO.FileManager.Gumps.UseUOPGumps)
                {
                    xOffset = 400;

                    AddStatTextLabel(World.Player.LowerReagentCost.ToString(), MobileStats.LowerReagentCost, xOffset, 77);

                    AddStatTextLabel(World.Player.SpellDamageIncrease.ToString(), MobileStats.SpellDamageInc, xOffset, 105);

                    AddStatTextLabel(World.Player.FasterCasting.ToString(), MobileStats.FasterCasting, xOffset, 133);

                    AddStatTextLabel(World.Player.FasterCastRecovery.ToString(), MobileStats.FasterCastRecovery, xOffset, 161);

                    xOffset = 365;

                    Add
                    (
                        new HitBox
                        (
                            xOffset,
                            70,
                            55,
                            24,
                            Client.Game.UO.FileManager.Clilocs.GetString(1075625, ResGumps.LowerReagentCost),
                            0
                        )
                        { CanMove = true }
                    );

                    Add
                    (
                        new HitBox
                        (
                            xOffset,
                            98,
                            55,
                            24,
                            Client.Game.UO.FileManager.Clilocs.GetString(1075628, ResGumps.SpellDamageIncrease),
                            0
                        )
                        { CanMove = true }
                    );

                    Add
                    (
                        new HitBox
                        (
                            xOffset,
                            126,
                            55,
                            24,
                            Client.Game.UO.FileManager.Clilocs.GetString(1075617, ResGumps.FasterCasting),
                            0
                        )
                        { CanMove = true }
                    );

                    Add
                    (
                        new HitBox
                        (
                            xOffset,
                            154,
                            55,
                            24,
                            Client.Game.UO.FileManager.Clilocs.GetString(1075618, ResGumps.FasterCastRecovery),
                            0
                        )
                        { CanMove = true }
                    );

                    xOffset = 480;

                    AddStatTextLabel(World.Player.Gold.ToString(), MobileStats.Gold, xOffset, 161);

                    Add
                    (
                        new HitBox
                        (
                            445,
                            154,
                            55,
                            24,
                            Client.Game.UO.FileManager.Clilocs.GetString(1061156, ResGumps.Gold),
                            0
                        )
                        { CanMove = true }
                    );

                    xOffset = 475;

                    AddStatTextLabel($"{World.Player.PhysicalResistance}/{World.Player.MaxPhysicResistence}", MobileStats.AR, xOffset + 18, 68);

                    AddStatTextLabel($"{World.Player.FireResistance}/{World.Player.MaxFireResistence}", MobileStats.RF, xOffset + 18, 91);

                    AddStatTextLabel($"{World.Player.ColdResistance}/{World.Player.MaxColdResistence}", MobileStats.RC, xOffset + 18, 108);

                    AddStatTextLabel($"{World.Player.PoisonResistance}/{World.Player.MaxPoisonResistence}", MobileStats.RP, xOffset + 18, 122);

                    AddStatTextLabel($"{World.Player.EnergyResistance}/{World.Player.MaxEnergyResistence}", MobileStats.RE, xOffset + 18, 139);
                }
                else
                {
                    xOffset = 354;

                    AddStatTextLabel(World.Player.PhysicalResistance.ToString(), MobileStats.AR, xOffset + 20, 74);
                    AddStatTextLabel(World.Player.FireResistance.ToString(), MobileStats.RF, xOffset + 20, 91);
                    AddStatTextLabel(World.Player.ColdResistance.ToString(), MobileStats.RC, xOffset + 20, 106);
                    AddStatTextLabel(World.Player.PoisonResistance.ToString(), MobileStats.RP, xOffset + 20, 122);
                    AddStatTextLabel(World.Player.EnergyResistance.ToString(), MobileStats.RE, xOffset + 20, 137);
                }


                xOffset = Client.Game.UO.FileManager.Gumps.UseUOPGumps ? 445 : 334;

                Add
                (
                    new HitBox
                    (
                        xOffset + 10,
                        76,
                        40,
                        14,
                        Client.Game.UO.FileManager.Clilocs.GetString(1061158, ResGumps.PhysicalResistance),
                        0
                    )
                    { CanMove = true }
                );

                Add
                (
                    new HitBox
                    (
                        xOffset + 10,
                        92,
                        40,
                        14,
                        Client.Game.UO.FileManager.Clilocs.GetString(1061159, ResGumps.FireResistance),
                        0
                    )
                    { CanMove = true }
                );

                Add
                (
                    new HitBox
                    (
                        xOffset + 10,
                        106,
                        40,
                        14,
                        Client.Game.UO.FileManager.Clilocs.GetString(1061160, ResGumps.ColdResistance),
                        0
                    )
                    { CanMove = true }
                );

                Add
                (
                    new HitBox
                    (
                        xOffset + 10,
                        120,
                        40,
                        14,
                        Client.Game.UO.FileManager.Clilocs.GetString(1061161, ResGumps.PoisonResistance),
                        0
                    )
                    { CanMove = true }
                );

                Add
                (
                    new HitBox
                    (
                        xOffset + 10,
                        134,
                        40,
                        14,
                        Client.Game.UO.FileManager.Clilocs.GetString(1061162, ResGumps.EnergyResistance),
                        0
                    )
                    { CanMove = true }
                );
            }
            else
            {
                if (Client.Game.UO.Version == ClientVersion.CV_308D)
                {
                    AddStatTextLabel(World.Player.StatsCap.ToString(), MobileStats.StatCap, 171, 124);

                    Add
                    (
                        new HitBox
                        (
                            171,
                            124,
                            34,
                            12,
                            Client.Game.UO.FileManager.Clilocs.GetString(1061152, ResGumps.MaxStats),
                            0
                        )
                        { CanMove = true }
                    );
                }
                else if (Client.Game.UO.Version == ClientVersion.CV_308J)
                {
                    AddStatTextLabel(World.Player.StatsCap.ToString(), MobileStats.StatCap, 180, 131);

                    AddStatTextLabel($"{World.Player.Followers}/{World.Player.FollowersMax}", MobileStats.Followers, 180, 144);

                    Add
                    (
                        new HitBox
                        (
                            180,
                            131,
                            34,
                            12,
                            Client.Game.UO.FileManager.Clilocs.GetString(1061152, ResGumps.MaxStats),
                            0
                        )
                        { CanMove = true }
                    );

                    Add
                    (
                        new HitBox
                        (
                            171,
                            144,
                            34,
                            12,
                            Client.Game.UO.FileManager.Clilocs.GetString(1061157, ResGumps.Followers),
                            0
                        )
                        { CanMove = true }
                    );
                }
            }

            if (Client.Game.UO.FileManager.Gumps.UseUOPGumps)
            {
                p.X = 580;
                p.Y = 180;
            }


            Add
            (
                new HitBox
                (
                    p.X,
                    p.Y,
                    16,
                    16,
                    ProfileManager.CurrentProfile.StatusGumpBarMutuallyExclusive
                        ? ResGumps.Minimize : ResGumps.StatusGumpOpenBar,
                    0
                )
                { CanMove = true }
            );

            _point = p;
        }

        private void AddStatTextLabel
        (
            string text,
            MobileStats stat,
            int x,
            int y,
            int maxWidth = 0,
            ushort hue = 0x038E,
            TEXT_ALIGN_TYPE alignment = TEXT_ALIGN_TYPE.TS_LEFT
        )
        {
            if (hue == 0x0386)
            {
                hue = Settings.Hue_Text;
            }

            var label = new Label
            (
                text,
                false,
                hue,
                maxWidth,
                align: alignment,
                font: 9
            )
            {
                X = x,
                Y = y
            };

            _labels[(int)stat] = label;
            Add(label);
        }

        protected new static ushort GetStatLockGraphic(Lock lockStatus)
        {
            switch (lockStatus)
            {
                case Lock.Up: return Settings.Graphic_LockIcon_Up;

                case Lock.Down: return Settings.Graphic_LockIcon_Down;

                case Lock.Locked: return Settings.Graphic_LockIcon_Locked;

                default: return 0xFFFF;
            }
        }

        public override void Update()
        {
            if (IsDisposed)
            {
                return;
            }

            if (_refreshTime < Time.Ticks)
            {
                _refreshTime = (long)Time.Ticks + 250;

                string name = World.Player.Name ?? string.Empty;
                if (!string.IsNullOrEmpty(World.Player.Title))
                    name = World.Player.Title.Contains(name, StringComparison.OrdinalIgnoreCase) ? World.Player.Title : $"{name}, {World.Player.Title}";

                _labels[(int)MobileStats.Name].Text = name;

                if (Client.Game.UO.FileManager.Gumps.UseUOPGumps)
                {
                    _labels[(int)MobileStats.HitChanceInc].Text = World.Player.HitChanceIncrease.ToString();
                }

                _labels[(int)MobileStats.Strength].Text = World.Player.Strength.ToString();

                _labels[(int)MobileStats.Dexterity].Text = World.Player.Dexterity.ToString();

                _labels[(int)MobileStats.Intelligence].Text = World.Player.Intelligence.ToString();

                if (Client.Game.UO.FileManager.Gumps.UseUOPGumps)
                {
                    _labels[(int)MobileStats.DefenseChanceInc].Text = $"{World.Player.DefenseChanceIncrease}/{World.Player.MaxDefenseChanceIncrease}";
                }

                _labels[(int)MobileStats.HealthCurrent].Text = World.Player.Hits.ToString();

                _labels[(int)MobileStats.HealthMax].Text = World.Player.HitsMax.ToString();

                _labels[(int)MobileStats.StaminaCurrent].Text = World.Player.Stamina.ToString();

                _labels[(int)MobileStats.StaminaMax].Text = World.Player.StaminaMax.ToString();

                _labels[(int)MobileStats.ManaCurrent].Text = World.Player.Mana.ToString();

                _labels[(int)MobileStats.ManaMax].Text = World.Player.ManaMax.ToString();

                if (Client.Game.UO.FileManager.Gumps.UseUOPGumps)
                {
                    _labels[(int)MobileStats.LowerManaCost].Text = World.Player.LowerManaCost.ToString();
                }

                _labels[(int)MobileStats.StatCap].Text = World.Player.StatsCap.ToString();

                _labels[(int)MobileStats.Luck].Text = World.Player.Luck.ToString();

                _labels[(int)MobileStats.WeightCurrent].Text = World.Player.Weight.ToString();

                _labels[(int)MobileStats.WeightMax].Text = World.Player.WeightMax.ToString();

                if (Client.Game.UO.FileManager.Gumps.UseUOPGumps)
                {
                    _labels[(int)MobileStats.DamageChanceInc].Text = World.Player.DamageIncrease.ToString();

                    _labels[(int)MobileStats.SwingSpeedInc].Text = World.Player.SwingSpeedIncrease.ToString();
                }

                _labels[(int)MobileStats.Gold].Text = World.Player.Gold.ToString();

                _labels[(int)MobileStats.Damage].Text = $"{World.Player.DamageMin}-{World.Player.DamageMax}";

                _labels[(int)MobileStats.Followers].Text = $"{World.Player.Followers}/{World.Player.FollowersMax}";

                if (Client.Game.UO.FileManager.Gumps.UseUOPGumps)
                {
                    _labels[(int)MobileStats.LowerReagentCost].Text = World.Player.LowerReagentCost.ToString();

                    _labels[(int)MobileStats.SpellDamageInc].Text = World.Player.SpellDamageIncrease.ToString();

                    _labels[(int)MobileStats.FasterCasting].Text = World.Player.FasterCasting.ToString();

                    _labels[(int)MobileStats.FasterCastRecovery].Text = World.Player.FasterCastRecovery.ToString();

                    _labels[(int)MobileStats.AR].Text = $"{World.Player.PhysicalResistance}/{World.Player.MaxPhysicResistence}";

                    _labels[(int)MobileStats.RF].Text = $"{World.Player.FireResistance}/{World.Player.MaxFireResistence}";

                    _labels[(int)MobileStats.RC].Text = $"{World.Player.ColdResistance}/{World.Player.MaxColdResistence}";

                    _labels[(int)MobileStats.RP].Text = $"{World.Player.PoisonResistance}/{World.Player.MaxPoisonResistence}";

                    _labels[(int)MobileStats.RE].Text = $"{World.Player.EnergyResistance}/{World.Player.MaxEnergyResistence}";
                }
                else
                {
                    _labels[(int)MobileStats.AR].Text = World.Player.PhysicalResistance.ToString();

                    _labels[(int)MobileStats.RF].Text = World.Player.FireResistance.ToString();

                    _labels[(int)MobileStats.RC].Text = World.Player.ColdResistance.ToString();

                    _labels[(int)MobileStats.RP].Text = World.Player.PoisonResistance.ToString();

                    _labels[(int)MobileStats.RE].Text = World.Player.EnergyResistance.ToString();
                }
            }

            base.Update();
        }

        public static class Settings
        {
            public static ushort Graphic_Background { get; set; } = 0x2A6C;
            public static ushort Graphic_BuffIcon_Normal { get; set; } = 0x7538;
            public static ushort Graphic_BuffIcon_Pressed { get; set; } = 0x7539;
            public static ushort Graphic_BuffIcon_Hover { get; set; } = 0x7539;
            public static ushort Graphic_LockIcon_Up { get; set; } = 0x0984;
            public static ushort Graphic_LockIcon_Down { get; set; } = 0x0986;
            public static ushort Graphic_LockIcon_Locked { get; set; } = 0x082C;

            public static ushort Hue_Background { get; set; } = 0;
            public static ushort Hue_CharacterName { get; set; } = 1;
            public static ushort Hue_Text { get; set; } = 1;
        }

        private enum MobileStats
        {
            Name,
            Strength,
            Dexterity,
            Intelligence,
            HealthCurrent,
            HealthMax,
            StaminaCurrent,
            StaminaMax,
            ManaCurrent,
            ManaMax,
            WeightMax,
            Followers,
            WeightCurrent,
            LowerReagentCost,
            SpellDamageInc,
            FasterCasting,
            FasterCastRecovery,
            StatCap,
            HitChanceInc,
            DefenseChanceInc,
            LowerManaCost,
            DamageChanceInc,
            SwingSpeedInc,
            Luck,
            Gold,
            AR,
            RF,
            RC,
            RP,
            RE,
            Damage,
            Sex,
            NumStats
        }
        private readonly GumpPicWithWidth[] _fillBars = new GumpPicWithWidth[3];
    }
}
