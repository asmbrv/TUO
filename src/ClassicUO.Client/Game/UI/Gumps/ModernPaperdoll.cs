using ClassicUO.Assets;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using ClassicUO.Game.Managers.Structs;


namespace ClassicUO.Game.UI.Gumps
{
    public class ModernPaperdoll : AnchorableGump
    {
        #region CONST
        private const int WIDTH = 248, HEIGHT = 410;
        private const int CELL_SPACING = 2, TOP_SPACING = 55;
        private Texture2D MordernPaperdollGump;

        private void InitializeTexture()
        {
            if (MordernPaperdollGump == null)
            {
                PNGLoader.Instance.TryGetEmbeddedTexture("equipment-gump.png", out MordernPaperdollGump);
            }
        }
        #endregion

        #region VARS
        private readonly Dictionary<Layer[], ItemSlot> itemLayerSlots;
        //private Label titleLabel;
        private static int lastX = 100, lastY = 100;
        private static int lastPreviewX = -1, lastPreviewY = -1;
        private GumpPicBase backgroundImage;
        #endregion

        public override GumpType GumpType => GumpType.PaperDoll;

        public ModernPaperdoll(World world, uint localSerial) : base(world, localSerial, 0)
        {
            #region SET VARS
            AcceptMouseInput = true;
            CanMove = true;
            CanCloseWithRightClick = true;
            AnchorType = ProfileManager.CurrentProfile.ModernPaperdollAnchorEnabled ? ANCHOR_TYPE.NONE : ANCHOR_TYPE.DISABLED;
            Width = WIDTH;
            Height = HEIGHT;
            GroupMatrixHeight = Height;
            GroupMatrixWidth = Width;
            if (ProfileManager.CurrentProfile != null)
            {
                lastX = ProfileManager.CurrentProfile.ModernPaperdollPosition.X;
                lastY = ProfileManager.CurrentProfile.ModernPaperdollPosition.Y;
                IsLocked = ProfileManager.CurrentProfile.PaperdollLocked;

                if (lastPreviewX == -1)
                {
                    lastPreviewX = ProfileManager.CurrentProfile.CharacterPreviewPosition.X;
                    lastPreviewY = ProfileManager.CurrentProfile.CharacterPreviewPosition.Y;
                }
            }
            X = lastX;
            Y = lastY;

            itemLayerSlots = new Dictionary<Layer[], ItemSlot>();
            #endregion

            InitializeTexture();
            Add(backgroundImage = new EmbeddedGumpPic(0, 0, MordernPaperdollGump, ProfileManager.CurrentProfile.ModernPaperDollHue));

            var _menuHit = new HitBox(Width - 26, 1, 25, 16, alpha: 0f);
            Add(_menuHit);
            _menuHit.SetTooltip("Open Menu");
            _menuHit.MouseUp += (sender, e) =>
            {
                if (e.Button == MouseButtonType.Left)
                {
                    UIManager.GetGump<MenuGump>()?.Dispose();
                    UIManager.Add(new MenuGump(world, Mouse.Position.X - 145, Mouse.Position.Y - 5, localSerial));
                }
            };

            #region SET UP ITEM SLOTS
            ItemSlot _;

            _ = new ItemSlot(world, 35, 35, new Layer[] { Layer.Earrings }, 0f, 1.1f) { X = 135 + CELL_SPACING, Y = TOP_SPACING + 15 };
            itemLayerSlots.Add(_.Layers, _); //Earrings

            _ = new ItemSlot(world, 50, 50, new Layer[] { Layer.Helmet }, 0f, 1.1f) { X = 100, Y = TOP_SPACING - 40};
            itemLayerSlots.Add(_.Layers, _); //Head

            _ = new ItemSlot(world, 35, 35, new Layer[] { Layer.Necklace }, 0f, 1.1f) { X = 75, Y = TOP_SPACING + 15 };
            itemLayerSlots.Add(_.Layers, _); //Amulet


            _ = new ItemSlot(world, 50, 75, new Layer[] { Layer.OneHanded }, 0f, 1.0f) { X = 10 - CELL_SPACING, Y = 47 + CELL_SPACING + TOP_SPACING };
            itemLayerSlots.Add(_.Layers, _); //L Wep

            _ = new ItemSlot(world, 50, 75, new Layer[] { Layer.Torso }, 0f, 1.1f) { X = 101, Y = 48 + CELL_SPACING + TOP_SPACING };
            itemLayerSlots.Add(_.Layers, _); //Chest

            _ = new ItemSlot(world, 50, 75, new Layer[] { Layer.TwoHanded }, 0f, 1.1f) { X = 190, Y = 47 + CELL_SPACING + TOP_SPACING };
            itemLayerSlots.Add(_.Layers, _); //R Wep


            _ = new ItemSlot(world, 50, 50, new Layer[] { Layer.Arms }, 0f, 1.1f) { X = 7, Y = 185 };
            itemLayerSlots.Add(_.Layers, _); //Arms

            _ = new ItemSlot(world, 50, 50, new Layer[] { Layer.Robe }, 0f, 1.0f) { X = 190, Y = 186 };
            itemLayerSlots.Add(_.Layers, _); //Robe

            _ = new ItemSlot(world, 50, 50, new Layer[] { Layer.Cloak }, 0f, 1.0f) { X = 189, Y = 252 };
            itemLayerSlots.Add(_.Layers, _); //Cloak


            _ = new ItemSlot(world, 35, 35, new Layer[] { Layer.Ring }, 0f, 1.0f) { X = 87 - CELL_SPACING, Y = 123 + CELL_SPACING + TOP_SPACING };
            itemLayerSlots.Add(_.Layers, _); //Ring

            _ = new ItemSlot(world, 80, 35, new Layer[] { Layer.Waist }, 0f, 1.0f) { X = 85, Y = 157 + CELL_SPACING + TOP_SPACING };
            itemLayerSlots.Add(_.Layers, _); //Belt

            _ = new ItemSlot(world, 35, 35, new Layer[] { Layer.Bracelet }, 0f, 1.0f) { X = 127 + CELL_SPACING, Y = 123 + CELL_SPACING + TOP_SPACING };
            itemLayerSlots.Add(_.Layers, _); //Bracelet


            _ = new ItemSlot(world, 50, 50, new Layer[] { Layer.Gloves }, 0f, 1.1f) { X = 10 - CELL_SPACING, Y = 195 + CELL_SPACING + TOP_SPACING};
            itemLayerSlots.Add(_.Layers, _); //Gloves

            _ = new ItemSlot(world, 50, 50, new Layer[] { Layer.Pants }, 0f, 1.1f) { X = 98, Y = 195 + CELL_SPACING + TOP_SPACING};
            itemLayerSlots.Add(_.Layers, _); //Legs

            _ = new ItemSlot(world, 50, 50, new Layer[] { Layer.Shoes }, 0f, 1.1f) { X = 97 + CELL_SPACING, Y = 300};
            itemLayerSlots.Add(_.Layers, _); //Boots



            _ = new ItemSlot(world, 33, 34, new Layer[] { Layer.Talisman }, 0f, 1.2f) { X = 196, Y = 355 };
            itemLayerSlots.Add(_.Layers, _); //Talisman

            //_ = new ItemSlot(world, 33, 34, new Layer[] { Layer.Backpack }, 0f, 1.0f) { X = Width - 38, Y = 222 + CELL_SPACING + TOP_SPACING};
            //itemLayerSlots.Add(_.Layers, _); //Backpack


            _ = new ItemSlot(world, 24, 24, new Layer[] { Layer.Tunic }, 0f, 1.1f) { X = 18, Y = 359 };
            itemLayerSlots.Add(_.Layers, _);

            _ = new ItemSlot(world, 24, 24, new Layer[] { Layer.Shirt }, 0f, 1.1f) { X = 65, Y = 359 };
            itemLayerSlots.Add(_.Layers, _);


            _ = new ItemSlot(world, 24, 24, new Layer[] { Layer.Skirt }, 0f, 1.1f) { X = 112, Y = 359 };
            itemLayerSlots.Add(_.Layers, _);

            _ = new ItemSlot(world, 24, 24, new Layer[] { Layer.Legs }, 0f, 1.1f) { X = 157, Y = 359 };
            itemLayerSlots.Add(_.Layers, _);
            #endregion

            BuildLayerSlots();

            var _virtueHitBox = new HitBox((WIDTH / 2) - 16, 1, 32, 32, "Open Virtues", 0f);
            _virtueHitBox.MouseDoubleClick += (s, e) =>
            {
                GameActions.ReplyGump
                (
                    World,
                    World.Player,
                    0x000001CD,
                    0x00000001,
                    new[]
                    {
                        LocalSerial
                    },
                    new Tuple<ushort, string>[0]
                );
            };
            Add(_virtueHitBox);

            //Add(titleLabel = new Label("", true, 0x03B2, maxwidth: WIDTH - 30, align: TEXT_ALIGN_TYPE.TS_CENTER) { X = 15, Y = 282 + CELL_SPACING + TOP_SPACING, AcceptMouseInput = false });

            var _minHit = new HitBox(1, 1, 14, 18, alpha: 0f);
            _minHit.SetTooltip("Minimize");
            _minHit.MouseUp += (s, e) =>
            {
                Dispose();
                UIManager.Add(new MinimizedPaperdoll(world, LocalSerial) { X = X, Y = Y });
            };
            Add(_minHit);

            RequestUpdateContents();

            if (ProfileManager.CurrentProfile.CharacterPreviewVisible && UIManager.GetGump<CharacterPreview>(localSerial) == null)
            {
                var preview = new CharacterPreview(world, localSerial);
                if (lastPreviewX > 0 || lastPreviewY > 0)
                {
                    preview.X = lastPreviewX;
                    preview.Y = lastPreviewY;
                }
                else
                {
                    preview.X = X + Width + 10;
                    preview.Y = Y;
                }
                UIManager.Add(preview);
            }
        }

        //public void UpdateTitle(string text) => titleLabel.Text = text;

        private void BuildLayerSlots()
        {
            foreach (KeyValuePair<Layer[], ItemSlot> layerSlot in itemLayerSlots)
            {
                Add(layerSlot.Value);
            }
        }

        public void HandleObjectMessage(Entity parent, string text, ushort hue)
        {
            if (parent != null)
                foreach (ItemSlot layerSlot in itemLayerSlots.Values)
                    if (layerSlot.Item != null && layerSlot.Item.Serial == parent.Serial)
                    {
                        layerSlot.AddText(text, hue);
                        return;
                    }
        }

        protected override void UpdateContents()
        {
            base.UpdateContents();
            if (World.Player == null)
                return;

            foreach (KeyValuePair<Layer[], ItemSlot> layerSlot in itemLayerSlots)
            {
                layerSlot.Value.ClearItems();

                foreach (Layer layer in layerSlot.Key)
                {
                    Item i = World.Player.FindItemByLayer(layer);
                    if (i != null && i.IsLootable)
                    {
                        layerSlot.Value.AddItem(World, this, i);
                    }
                }
            }

            UIManager.GetGump<CharacterPreview>()?.PaperDollPreview.RequestUpdate();

            Mobile m = World.Mobiles.Get(LocalSerial);
            if (m != null);
                //UpdateTitle(m.Title);
        }

        public void UpdateOptions()
        {
            backgroundImage.Hue = ProfileManager.CurrentProfile.ModernPaperDollHue;
            AnchorType = ProfileManager.CurrentProfile.ModernPaperdollAnchorEnabled ? ANCHOR_TYPE.NONE : ANCHOR_TYPE.DISABLED;
            foreach (KeyValuePair<Layer[], ItemSlot> layerSlot in itemLayerSlots)
            {
                layerSlot.Value.UpdateOptions();
            }
        }

        public static void UpdateAllOptions() => UIManager.ForEach<ModernPaperdoll>(p => p.UpdateOptions());

        protected override void OnMove(int x, int y)
        {
            if (X != lastX || Y != lastY)
          {
              lastX = X;
              lastY = Y;
              if (ProfileManager.CurrentProfile != null)
                  ProfileManager.CurrentProfile.ModernPaperdollPosition = new Point(X, Y);
          }
        }

        public override void Dispose()
        {
            if (ProfileManager.CurrentProfile != null)
                ProfileManager.CurrentProfile.ModernPaperdollPosition = new Point(X, Y);
            lastX = X;
            lastY = Y;

            base.Dispose();
            //World.OPL.OPLOnReceive -= OPL_OnOPLReceive;
        }

        public override void Restore(XmlElement xml)
        {
            base.Restore(xml);
            if (ProfileManager.CurrentProfile != null)
            {
                X = lastX = ProfileManager.CurrentProfile.ModernPaperdollPosition.X;
                Y = lastY = ProfileManager.CurrentProfile.ModernPaperdollPosition.Y;
            }
        }

        public override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            base.OnMouseUp(x, y, button);

            if (Client.Game.UO.GameCursor.ItemHold.Enabled)
            {
                if (LocalSerial == World.Player.Serial)
                {
                    if (SelectedObject.Object is Item item && (item.Layer == Layer.Backpack || item.ItemData.IsContainer))
                    {
                        GameActions.DropItem
                        (
                            Client.Game.UO.GameCursor.ItemHold.Serial,
                            0xFFFF,
                            0xFFFF,
                            0,
                            item.Serial
                        );

                        Mouse.CancelDoubleClick = true;
                    }
                    else
                    {
                        if (Client.Game.UO.GameCursor.ItemHold.ItemData.IsWearable)
                        {
                            Item equipment = World.Player.FindItemByLayer((Layer)Client.Game.UO.GameCursor.ItemHold.ItemData.Layer);

                            if (equipment == null)
                            {
                                if(ProfileManager.CurrentProfile.QueueManualItemMoves)
                                {
                                    var mr = new MoveRequest(
                                        Client.Game.UO.GameCursor.ItemHold.Serial,
                                        World.Player,
                                        layer: (Layer)Client.Game.UO.GameCursor.ItemHold.ItemData.Layer, moveType: MoveType.Equip);
                                    ObjectActionQueue.Instance.Enqueue(mr.ToObjectActionQueueItem(), ActionPriority.EquipItem);
                                }
                                else
                                    GameActions.Equip(World, World.Player);

                                Mouse.CancelDoubleClick = true;
                                Client.Game.UO.GameCursor.ItemHold.Clear();
                            }
                        }
                    }
                }
            }
            else if (World.TargetManager.IsTargeting)
            {
                if (SelectedObject.Object is Item item)
                    World.TargetManager.Target(item.Serial);
            }
        }

        private class ItemSlot : Control
        {
            public Item Item;
            public readonly Layer[] Layers;
            public float Rotation;
            public float ItemScale;

            private readonly Area _itemArea;
            private readonly AlphaBlendControl _durabilityBar;
            private readonly World _world;
            private readonly List<SimpleTimedTextGump> _timedTexts = [];

            public ItemSlot(World world, int width, int height, Layer[] layers, float rotation = 0f, float itemScale = 1.0f)
            {
                _world = world;
                AcceptMouseInput = true;
                CanMove = true;
                // Right clicks are propagated back to the paperdoll to allow right click close anywhere
                CanCloseWithRightClick = true;
                Width = width;
                Height = height;
                Rotation = rotation;
                ItemScale = itemScale;

                Add(_itemArea = new Area(false) { Width = Width, Height = Height, AcceptMouseInput = true, CanMove = true });
                _itemArea.SetTooltip(layers[0].ToString());

                Add(_durabilityBar = new AlphaBlendControl(0.75f) { Width = Width, Height = 3, Hue = ProfileManager.CurrentProfile.ModernPaperDollDurabilityHue, IsVisible = false });

                this.Layers = layers;
            }

            public void AddText(string text, ushort hue)
            {
                var timedText = new SimpleTimedTextGump(_world, text, (uint)hue, TimeSpan.FromSeconds(2), 200)
                {
                    X = ScreenCoordinateX,
                    Y = ScreenCoordinateY
                };

                // Remove disposed timed texts
                _timedTexts.RemoveAll(tt => tt == null || tt.IsDisposed);

                // Adjust the Y position of existing timed texts
                foreach (SimpleTimedTextGump tt in _timedTexts)
                    tt.Y -= timedText.Height + 5;

                _timedTexts.Add(timedText);
                UIManager.Add(timedText);
            }

            public void UpdateOptions() => _durabilityBar.Hue = ProfileManager.CurrentProfile.ModernPaperDollDurabilityHue;

            public void AddItem(World world, Gump gump, Item item)
            {
                Item = item;
                _itemArea.Add(new ItemGumpFixed(world, gump, item, Width, Height, Rotation, ItemScale) { HighlightOnMouseOver = false });
                UpdateDurability(item);
            }

            private void UpdateDurability(Item item)
            {
                if (IsDisposed || _durabilityBar.IsDisposed || item == null)
                {
                    _durabilityBar.IsVisible = false;
                    return;
                }

                _durabilityBar.Hue = ProfileManager.CurrentProfile.ModernPaperDollDurabilityHue;

                if (_world.DurabilityManager.TryGetDurability(item.Serial, out DurabiltyProp durabilty))
                {
                    if (durabilty.Percentage > (float)ProfileManager.CurrentProfile.ModernPaperDoll_DurabilityPercent / (float)100)
                    {
                        _durabilityBar.IsVisible = false;
                        return;
                    }
                    _durabilityBar.Width = (int)(Width * durabilty.Percentage);
                    _durabilityBar.Y = Height - 3;
                    _durabilityBar.IsVisible = true;
                }
                else
                {
                    _durabilityBar.IsVisible = false;
                }
            }

            public void ClearItems()
            {
                _itemArea.Children.Clear();
                UpdateDurability(null);
                Item = null;
            }

            public override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                ConditionalRequestContextMenuForSlot(x, y, button);
                base.OnMouseUp(x, y, button);
                Parent?.InvokeMouseUp(new Point(x, y), button);
            }

            /// <summary>
            ///     Requests a context menu (popup) for the item in the clicked slot
            /// </summary>
            /// <param name="x">The click's X coordinate, in relation to the slot itself</param>
            /// <param name="y">The click's Y coordinate, in relation to the slot itself</param>
            /// <param name="button">The clicked mouse button. Context menus are requested only for left clicks</param>
            private void ConditionalRequestContextMenuForSlot(int x, int y, MouseButtonType button)
            {
                if (!_world.InGame || Item == null || button != MouseButtonType.Left)
                    return;

                if (_world.DelayedObjectClickManager.IsEnabled)
                    return;

                // Dispatch a request to get the context menu for the item
                _world.DelayedObjectClickManager.Set(
                    Item.Serial,
                    x,
                    y,
                    Time.Ticks + Mouse.MOUSE_DELAY_DOUBLE_CLICK
                );
            }
        }

        private class ItemGumpFixed : ItemGump
        {
            public readonly Item item;
            private readonly float _rotation;
            private readonly float _itemScale;

            public ItemGumpFixed(World world, Gump gump, Item item, int w, int h, float rotation = 0f, float itemScale = 1.0f) : base
            (
                gump,
                item.Serial,
                item.DisplayedGraphic,
                item.Hue,
                item.X,
                item.Y
            )
            {
                if ((Layer)item.ItemData.Layer == Layer.Backpack && item.Container == world.Player.Serial)
                    CanPickUp = false;

                Width = w;
                Height = h;
                _rotation = rotation;
                _itemScale = itemScale;
                WantUpdateSize = false;
                CanCloseWithRightClick = true;

                this.item = item;
            }

            private static ushort GetAnimID(ushort graphic, ushort animID, bool isfemale)
            {
                int offset = isfemale ? Constants.FEMALE_GUMP_OFFSET : Constants.MALE_GUMP_OFFSET;

                if (Client.Game.UO.Version >= Utility.ClientVersion.CV_7000 && animID == 0x03CA                          // graphic for dead shroud
                                                            && (graphic == 0x02B7 || graphic == 0x02B6)) // dead gargoyle graphics
                {
                    animID = 0x0223;
                }

                Client.Game.UO.Animations.ConvertBodyIfNeeded(ref graphic);

                if (Client.Game.UO.FileManager.Animations.EquipConversions.TryGetValue(graphic, out Dictionary<ushort, EquipConvData> dict))
                {
                    if (dict.TryGetValue(animID, out EquipConvData data))
                    {
                        if (data.Gump > Constants.MALE_GUMP_OFFSET)
                        {
                            animID = (ushort)(data.Gump >= Constants.FEMALE_GUMP_OFFSET ? data.Gump - Constants.FEMALE_GUMP_OFFSET : data.Gump - Constants.MALE_GUMP_OFFSET);
                        }
                        else
                        {
                            animID = data.Gump;
                        }
                    }
                }

                if (animID + offset > GumpsLoader.MAX_GUMP_DATA_INDEX_COUNT || Client.Game.UO.Gumps.GetGump((ushort)(animID + offset)).Texture == null)
                {
                    // inverse
                    offset = isfemale ? Constants.MALE_GUMP_OFFSET : Constants.FEMALE_GUMP_OFFSET;
                }

                return (ushort)(animID + offset);
            }

            public override bool Draw(UltimaBatcher2D batcher, int x, int y)
            {
                if (item == null)
                {
                    Dispose();
                }

                if (IsDisposed)
                {
                    return false;
                }

                Vector3 hueVector = ShaderHueTranslator.GetHueVector
                (
                    MouseIsOver && HighlightOnMouseOver ? 0x0035 : item.Hue,
                    item.ItemData.IsPartialHue,
                    1,
                    true
                );

                ref readonly SpriteInfo texture = ref Client.Game.UO.Arts.GetArt((uint)item.DisplayedGraphic);
                Rectangle _rect = Client.Game.UO.Arts.GetRealArtBounds((uint)item.DisplayedGraphic);

                int targetW = _rect.Width;
                int targetH = _rect.Height;

                if (targetW > Width) targetW = Width;
                if (targetH > Height) targetH = Height;

                int finalW = (int)(targetW * _itemScale);
                int finalH = (int)(targetH * _itemScale);

                int drawX = x + (Width - finalW) / 2;
                int drawY = y + (Height - finalH) / 2;

                if (texture.Texture != null)
                {
                    Vector2 origin = new Vector2(_rect.Width / 2f, _rect.Height / 2f);
                    Rectangle destRect = new Rectangle(drawX + finalW / 2, drawY + finalH / 2, finalW, finalH);

                    batcher.Draw
                    (
                        texture.Texture,
                        destRect,
                        new Rectangle
                        (
                            texture.UV.X + _rect.X,
                            texture.UV.Y + _rect.Y,
                            _rect.Width,
                            _rect.Height
                        ),
                        hueVector,
                        MathHelper.ToRadians(_rotation),
                        origin,
                        SpriteEffects.None,
                        0
                    );

                    return true;
                }

                return false;
            }

            public override bool Contains(int x, int y) => true;
        }

        private class MenuButton : Control
        {
            public MenuButton(int width, uint hue, float alpha, string tooltip = "")
            {
                Width = width;
                Height = 16;
                AcceptMouseInput = true;
                var _ = new Area() { Width = Width, Height = Height, AcceptMouseInput = false };

                Add(_);
                Add(new Line(2, 2, Width - 4, 2, hue) { Alpha = alpha, AcceptMouseInput = false });
                Add(new Line(2, 7, Width - 4, 2, hue) { Alpha = alpha, AcceptMouseInput = false });
                Add(new Line(2, 12, Width - 4, 2, hue) { Alpha = alpha, AcceptMouseInput = false });
                SetTooltip(tooltip);
                //_.SetTooltip(tooltip);
            }

            public override bool Contains(int x, int y) => true;
        }

        private class MenuGump : Gump
        {
            public MenuGump(World world, int x, int y, uint localSerial) : base(world, localSerial, 0)
            {
                X = x;
                Y = y;
                Width = 150;
                Height = 281;
                AcceptMouseInput = true;

                Add(new AlphaBlendControl(0.85f) { Width = Width, Height = Height, AcceptMouseInput = false });

                int i = 1;

                var preview = new NiceButton(1, 1, Width - 2, 20, ButtonAction.Activate, "Show Character");
                preview.MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtonType.Left)
                    {
                        UIManager.GetGump<CharacterPreview>()?.Dispose();
                        ProfileManager.CurrentProfile.CharacterPreviewVisible = true;
                        var cp = new CharacterPreview(world, localSerial);
                        if (lastPreviewX > 0 || lastPreviewY > 0)
                        {
                            cp.X = lastPreviewX;
                            cp.Y = lastPreviewY;
                        }
                        else
                        {
                            cp.X = 100;
                            cp.Y = 100;
                        }
                        UIManager.Add(cp);
                    }
                };
                Add(preview);

                var help = new NiceButton(1, 1 + 20 * i++, Width - 2, 20, ButtonAction.Activate, "Help");
                help.MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtonType.Left)
                    {
                        GameActions.RequestHelp();
                    }
                };
                Add(help);

                var options = new NiceButton(1, 1 + 20 * i++, Width - 2, 20, ButtonAction.Activate, "Options");
                options.MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtonType.Left)
                    {
                        GameActions.OpenSettings(world);
                    }
                };
                Add(options);

                var logout = new NiceButton(1, 1 + 20 * i++, Width - 2, 20, ButtonAction.Activate, "Log Out");
                logout.MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtonType.Left)
                    {
                        Client.Game.GetScene<GameScene>()?.RequestQuitGame();
                    }
                };
                Add(logout);

                var quests = new NiceButton(1, 1 + 20 * i++, Width - 2, 20, ButtonAction.Activate, "Quests");
                quests.MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtonType.Left)
                    {
                        GameActions.RequestQuestMenu(world);
                    }
                };
                Add(quests);

                var skills = new NiceButton(1, 1 + 20 * i++, Width - 2, 20, ButtonAction.Activate, "Skills");
                skills.MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtonType.Left)
                    {
                        GameActions.OpenSkills(world);
                    }
                };
                Add(skills);

                var guild = new NiceButton(1, 1 + 20 * i++, Width - 2, 20, ButtonAction.Activate, "Guild");
                guild.MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtonType.Left)
                    {
                        GameActions.OpenGuildGump(world);
                    }
                };
                Add(guild);

                var peace = new NiceButton(1, 1 + 20 * i++, Width - 2, 20, ButtonAction.Activate, "Peace/War");
                peace.MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtonType.Left)
                    {
                        GameActions.ToggleWarMode(world.Player);
                    }
                };
                Add(peace);

                var durability = new NiceButton(1, 1 + 20 * i++, Width - 2, 20, ButtonAction.Activate, "Durability Tracker");
                durability.MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtonType.Left)
                    {
                        UIManager.GetGump<DurabilitysGump>()?.Dispose();
                        UIManager.Add(new DurabilitysGump(world));
                    }
                };
                Add(durability);

                var status = new NiceButton(1, 1 + 20 * i++, Width - 2, 20, ButtonAction.Activate, "Status");
                status.MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtonType.Left)
                    {
                        if (LocalSerial == World.Player)
                        {
                            UIManager.GetGump<BaseHealthBarGump>(LocalSerial)?.Dispose();

                            var status = StatusGumpBase.GetStatusGump();

                            if (status == null)
                            {
                                UIManager.Add(StatusGumpBase.AddStatusGump(world, ProfileManager.CurrentProfile.StatusGumpPosition.X, ProfileManager.CurrentProfile.StatusGumpPosition.Y));
                            }
                            else
                            {
                                status.BringOnTop();
                            }
                        }
                        else
                        {
                            if (UIManager.GetGump<BaseHealthBarGump>(LocalSerial) != null)
                            {
                                return;
                            }

                            if (ProfileManager.CurrentProfile.CustomBarsToggled)
                            {
                                var bounds = new Rectangle(0, 0, HealthBarGumpCustom.HPB_WIDTH, HealthBarGumpCustom.HPB_HEIGHT_SINGLELINE);

                                UIManager.Add
                                (
                                    new HealthBarGumpCustom(world, LocalSerial)
                                    {
                                        X = Mouse.Position.X - (bounds.Width >> 1),
                                        Y = Mouse.Position.Y - 5
                                    }
                                );
                            }
                            else
                            {
                                Rectangle bounds = Client.Game.UO.Gumps.GetGump(0x0804).UV;

                                UIManager.Add
                                (
                                    new HealthBarGump(world, LocalSerial)
                                    {
                                        X = Mouse.Position.X - (bounds.Width >> 1),
                                        Y = Mouse.Position.Y - 5
                                    }
                                );
                            }
                        }
                    }
                };
                Add(status);

                var party = new NiceButton(1, 1 + 20 * i++, Width - 2, 20, ButtonAction.Activate, "Party");
                party.MouseUp += (s, e) =>
                {
                    PartyGump party = UIManager.GetGump<PartyGump>();

                    if (party == null)
                    {
                        int x = Client.Game.Window.ClientBounds.Width / 2 - 272;
                        int y = Client.Game.Window.ClientBounds.Height / 2 - 240;
                        UIManager.Add(new PartyGump(world, x, y, World.Party.CanLoot));
                    }
                    else
                    {
                        party.BringOnTop();
                    }
                };
                Add(party);

                var profileEditor = new NiceButton(1, 1 + 20 * i++, Width - 2, 20, ButtonAction.Activate, "Profile");
                profileEditor.MouseUp += (s, e) =>
                {
                    GameActions.RequestProfile(LocalSerial);
                };
                Add(profileEditor);

                var abilities = new NiceButton(1, 1 + 20 * i++, Width - 2, 20, ButtonAction.Activate, "Abilities");
                abilities.MouseUp += (s, e) =>
                {
                    if (UIManager.GetGump<RacialAbilitiesBookGump>() == null)
                    {
                        UIManager.Add(new RacialAbilitiesBookGump(world, 100, 100));
                    }
                };
                Add(abilities);

                var weaponAbilities = new NiceButton(1, 1 + 20 * i++, Width - 2, 20, ButtonAction.Activate, "Weapon Abilities");
                weaponAbilities.MouseUp += (s, e) =>
                {
                    GameActions.OpenAbilitiesBook(world);
                };
                Add(weaponAbilities);

                Add(new SimpleBorder() { Width = Width, Height = Height });
            }

            protected override void OnMouseExit(int x, int y)
            {
                base.OnMouseExit(x, y);
                Dispose();
            }
        }

        private class CharacterPreview : AnchorableGump
        {
            public readonly PaperDollInteractable PaperDollPreview;
            public CharacterPreview(World world, uint localSerial) : base(world, localSerial, 0)
            {
                Width = 120;
                Height = 8;
                CanCloseWithRightClick = true;
                CanMove = true;
                AcceptMouseInput = true;

                // Configurações de ancoragem
                AnchorType = ANCHOR_TYPE.NONE;
                GroupMatrixWidth = Width;
                GroupMatrixHeight = Height;
                WantUpdateSize = false;

                Add(new AlphaBlendControl(0.2f)
                {
                    CanCloseWithRightClick = true,
                    CanMove = true,
                    Width = Width,
                    Height = Height,
                    Hue = 0
                });

                Add(PaperDollPreview = new PaperDollInteractable(-28, -220, LocalSerial, null) { AcceptMouseInput = false });
}

            public override void CloseWithRightClick()
            {
                if (ProfileManager.CurrentProfile != null)
                    ProfileManager.CurrentProfile.CharacterPreviewVisible = false;
                base.CloseWithRightClick();
            }

            public override bool ShouldBeSaved => false;

            public override GumpType GumpType => GumpType.PaperDoll;

            protected override void OnMove(int x, int y)
            {
                base.OnMove(x, y);
                lastPreviewX = X;
                lastPreviewY = Y;
                if (ProfileManager.CurrentProfile != null)
                    ProfileManager.CurrentProfile.CharacterPreviewPosition = new Point(X, Y);
            }
        }

        private class MinimizedPaperdoll : Gump
        {
            public MinimizedPaperdoll(World world, uint localSerial) : base(world, localSerial, 0)
            {
                Width = 86;
                Height = 23;
                AcceptMouseInput = true;
                CanMove = true;
                CanCloseWithRightClick = true;

                Add(new GumpPic(0, 0, 0x7EE, 0));

                Checkbox _;

                Add(_ = new Checkbox(0x00D2, 0x00D3) { X = 62, Y = 4 });
                _.IsChecked = ProfileManager.CurrentProfile.OpenModernPaperdollAtMinimizeLoc;
                _.SetTooltip("Open paperdoll at this location");
                _.MouseUp += (s, e) =>
                {
                    ProfileManager.CurrentProfile.OpenModernPaperdollAtMinimizeLoc = _.IsChecked;
                };
            }

            public override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                base.OnMouseUp(x, y, button);
                if (button == MouseButtonType.Left)
                {
                    Dispose();
                    UIManager.GetGump<ModernPaperdoll>()?.Dispose();

                    var pd = new ModernPaperdoll(World, LocalSerial);

                    if (ProfileManager.CurrentProfile.OpenModernPaperdollAtMinimizeLoc)
                    {
                        pd.X = X;
                        pd.Y = Y;
                    }

                    UIManager.Add(pd);
                }
            }
        }
    }
}
