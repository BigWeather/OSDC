using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if Allow_XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endif
using GamesLibrary;

namespace IntoTheNewWorld
{
    /// <summary>
    /// The trading interface for Into the New World.
    /// </summary>
    public class TradeWindow : Window
    {
        private class TradeItemHotspot : Hotspot
        {
            public int tradeItemIndex { get; private set; }

            public TradeItemHotspot(Rectangle bounds, int zorder, bool enabled, int tradeItemIndex)
                : base(bounds, false, zorder, enabled)
            {
                this.tradeItemIndex = tradeItemIndex;
            }
        }

        public enum TradingPartner
        {
            Europe, Fleet, Explorer, Native, Cache, Mission, TradingPost, Settlement, Fort, Loot
        }

        private enum TradingDirection
        {
            Left, Right, None
        }

        private enum TradeItemIconLocation
        {
            Left, MiddleLeft, MiddleRight, Right
        }

        public enum LockedState
        {
            None, Locked, Unlocked
        }

        private class Arrow
        {
            public Graphic gTail { get; private set; }
            public Graphic gBody { get; private set; }
            public Graphic gHead { get; private set; }
#if OLD_TEXTURE
            public Texture2D t2d 
            {
                get
                {
                    if (_t2d == null)
                        _t2d = IntoTheNewWorld.Instance.getTexture(gBody);
                    return _t2d;
                }
            }
            private Texture2D _t2d = null;
#endif

            public Arrow(Graphic gTail, Graphic gBody, Graphic gHead)
            {
                this.gTail = gTail;
                this.gBody = gBody;
                this.gHead = gHead;
            }
        }

        private Arrow _arrowLtR;
        private Arrow _arrowRtL;

        public VariableBundle leftTrader { get; private set; }
        public VariableBundle rightTrader { get; private set; }

        private string _leftTraderString;
        private string _rightTraderString;

        private List<string> _tradeItems;
        private List<int> _offsets;
        private List<int> _values;
        private List<TradeItemHotspot> _tradeItemHotspots;
        private Rectangle _arrowAvailableL = default(Rectangle);
        private Rectangle _arrowAvailableR = default(Rectangle);
        private Rectangle _leftItem = default(Rectangle);
        private Rectangle _rightItem = default(Rectangle);
        private Rectangle _middleLeftItem = default(Rectangle);
        private Rectangle _middleRightItem = default(Rectangle);

        private List<LockedState> _lockedLeft;
        private List<LockedState> _lockedRight;
        private List<bool> _desired;
        private List<bool> _undesired;

        private TradingPartner _partner;

        private Dictionary<string, int> _dictIndexByTradeItem;
        private Dictionary<string, int> _dictValueByTradeItem;

        private bool _partnerAcceptsTrade = true;
        private string _tradeAdviceMessage = "";
        private int _factionChange = 0;
        private float _receiveToOfferRatio = 1.0f;

        private int _unlocks = 0;
        private int _locksTradePartner = 2; // TODO: In future don't hardcode!

        private List<KeyValuePair<string, int>> _sortedTradeItemsLeft;

        private Button _buttonOK;
        private Button _buttonCancel;
        private Button _buttonTakeAll = null;

        private int _itemsTop = 40;
        private int _itemsBottom = 40;
        private int _itemsYBuffer = 10;

        private int _textHeight;
        private SpriteFont _font = IntoTheNewWorld.Instance.miramonte;

        public TradeWindow(Vector2 v2Center, int width, VariableBundle leftTrader, VariableBundle rightTrader, TradingPartner partner, int unlocks, string leftTraderString, string rightTraderString) 
            : base(v2Center, IntoTheNewWorld.Instance) 
        {
            this.bounds = new Rectangle(this.bounds.X, this.bounds.Y, width, this.bounds.Height);

            this.leftTrader = leftTrader;
            this.rightTrader = rightTrader;
            _partner = partner;
            _unlocks = unlocks;
            _leftTraderString = leftTraderString;
            _rightTraderString = rightTraderString;

            _dictValueByTradeItem = getAcceptedTradeItems();

            List<string> leftTradeItems = this.leftTrader.variables.Where(variable => (this.leftTrader.getValue<int>(variable) > 0) && _dictValueByTradeItem.ContainsKey(variable)).ToList();
            List<string> rightTradeItems = this.rightTrader.variables.Where(variable => (this.rightTrader.getValue<int>(variable) > 0) && _dictValueByTradeItem.ContainsKey(variable)).ToList();

            // The list of items that can be traded is the union of the two traders' pile of items to trade.
            //_tradeItems = this.leftTrader.variables.Union(this.rightTrader.variables).Distinct().ToList();
            _tradeItems = leftTradeItems.Union(rightTradeItems).Distinct().ToList();

            _dictIndexByTradeItem = new Dictionary<string, int>();
            for (int i = 0; i < _tradeItems.Count; i++)
                _dictIndexByTradeItem.Add(_tradeItems[i], i); 
            
            _offsets = new List<int>(_tradeItems.Count);
            _tradeItems.ForEach(ti => _offsets.Add(0));

            _values = new List<int>(_tradeItems.Count);
            _tradeItems.ForEach(ti => _values.Add(1));
            for (int i = 0; i < _tradeItems.Count; i++)
                _values[i] = _dictValueByTradeItem[_tradeItems[i]];

            //_sums = new List<int>(_tradeItems.Count);
            //foreach (string tradeItem in _tradeItems)
            //{
            //    int sum = 0;
            //    int val;
            //    if (leftTrader.getValue(tradeItem, out val))
            //        sum += val;
            //    if (rightTrader.getValue(tradeItem, out val))
            //        sum += val;
            //    _sums.Add(sum);
            //}

            //_tradeItemHotspots = new List<Rectangle>(_tradeItems.Count);
            //_tradeItems.ForEach(ti => _tradeItemHotspots.Add(new Rectangle(0, 0, 0, 0)));

            Graphic gArrowLtRTail = IntoTheNewWorld.Instance.dictGraphics["trade_ltr_arrow_tail"];
            Graphic gArrowLtRBody = IntoTheNewWorld.Instance.dictGraphics["trade_ltr_arrow_body"];
            Graphic gArrowLtRHead = IntoTheNewWorld.Instance.dictGraphics["trade_ltr_arrow_head"];
            _arrowLtR = new Arrow(gArrowLtRTail, gArrowLtRBody, gArrowLtRHead);

            Graphic gArrowRtLTail = IntoTheNewWorld.Instance.dictGraphics["trade_rtl_arrow_tail"];
            Graphic gArrowRtLBody = IntoTheNewWorld.Instance.dictGraphics["trade_rtl_arrow_body"];
            Graphic gArrowRtLHead = IntoTheNewWorld.Instance.dictGraphics["trade_rtl_arrow_head"];
            _arrowRtL = new Arrow(gArrowRtLTail, gArrowRtLBody, gArrowRtLHead);

            _lockedLeft = new List<LockedState>(_tradeItems.Count);
            _tradeItems.ForEach(ti => _lockedLeft.Add(LockedState.None));
            _lockedRight = new List<LockedState>(_tradeItems.Count);
            _tradeItems.ForEach(ti => _lockedRight.Add(LockedState.None));
            _desired = new List<bool>(_tradeItems.Count);
            _tradeItems.ForEach(ti => _desired.Add(false));
            _undesired = new List<bool>(_tradeItems.Count);
            _tradeItems.ForEach(ti => _undesired.Add(false));

            if (isAIPartner())
                initializeAIState();

            initializeButtons();
            initializeHotspots();

            _textHeight = (int)_font.MeasureString("1").Y;

            int height = _itemsTop + _tradeItems.Count * (gArrowLtRBody.defaultBounds.Height + _itemsYBuffer + _textHeight) + _itemsBottom + _buttonOK.bounds.Height + this.buttonBottomBuffer;
            this.bounds = new Rectangle(this.bounds.X, this.bounds.Y, this.bounds.Width, height);
        }

        public void initializeButtons()
        {
            string buttonOKText = "Make deal";
            if (!isAIPartner())
                buttonOKText = "OK";

            _buttonOK = new Button(new Rectangle(0, 0, 0, 0), buttonOKText, _font, Color.Green, true);
            _buttonOK.Press += new EventHandler<PressEventArgs>(_buttonOK_Press);
            this.addButton(_buttonOK);

            _buttonCancel = new Button(new Rectangle(0, 0, 0, 0), "Cancel", _font, Color.Red, true);
            _buttonCancel.Press += new EventHandler<PressEventArgs>(_buttonCancel_Press);
            this.addButton(_buttonCancel);

            if ((_partner == TradingPartner.Cache) || (_partner == TradingPartner.Loot))
            {
                if (_tradeItems.Any(tradeItem => (rightTrader.getValue<int>(tradeItem) != 0)))
                {
                    _buttonTakeAll = new Button(new Rectangle(0, 0, 0, 0), "Take All", _font, Color.Black, true);
                    _buttonTakeAll.Press += new EventHandler<PressEventArgs>(_buttonTakeAll_Press);
                    this.addButton(_buttonTakeAll);
                }
            }
        }

        void _buttonTakeAll_Press(object sender, PressEventArgs e)
        {
            for (int i = 0; i < _tradeItems.Count; i++)
            {
                int rightValue = rightTrader.getValue<int>(_tradeItems[i]);
                if (rightValue == 0)
                    continue;

                _offsets[i] = -rightValue;
            }
        }

        void _buttonCancel_Press(object sender, PressEventArgs e)
        {
            IntoTheNewWorld.Instance.Hide(this);            
        }

        void _buttonOK_Press(object sender, PressEventArgs e)
        {
            if (_partnerAcceptsTrade)
            {
                // Save the left trader.
                for (int i = 0; i < _tradeItems.Count; i++)
                {
                    if (_offsets[i] == 0)
                        continue;

                    leftTrader.adjustValue(_tradeItems[i], -_offsets[i]);
                }

                // Save the right trader.
                for (int i = 0; i < _tradeItems.Count; i++)
                {
                    if (_offsets[i] == 0)
                        continue;

                    rightTrader.adjustValue(_tradeItems[i], _offsets[i]);
                }
            }

            // TODO: Handle other faction types.
            if (isAIPartner() && (_partner == TradingPartner.Native))
            {
                int native_like = IntoTheNewWorld.Instance.players[0].state.getValue<int>("native_like");
                if (_factionChange > 0)
                    IntoTheNewWorld.Instance.players[0].state.setValue("native_like", Math.Min(100, native_like + _factionChange));
                else
                    IntoTheNewWorld.Instance.players[0].state.setValue("native_like", Math.Max(-100, native_like + _factionChange));
            }

            IntoTheNewWorld.Instance.forceUpdateGameState = true;

            IntoTheNewWorld.Instance.Hide(this);
        }

        public void initializeHotspots()
        {
            _tradeItemHotspots = new List<TradeItemHotspot>(_tradeItems.Count);

            for (int i = 0; i < _tradeItems.Count; i++)
            {
                Rectangle bounds = new Rectangle(0, 0, 0, 0);
                int zorder = 0;

                TradeItemHotspot hotspot = new TradeItemHotspot(bounds, zorder, true, i);
                hotspot.PrimaryActivation += new EventHandler<PrimaryActivationEventArgs>(tradeItemHotspot_PrimaryActivation);
                hotspot.SecondaryActivation += new EventHandler<SecondaryActivationEventArgs>(tradeItemHotspot_SecondaryActivation);
                hotspot.PointerOver += new EventHandler<PointerOverEventArgs>(tradeItemHotspot_PointerOver);

                _tradeItemHotspots.Add(hotspot);
            }

            this.hotspots.AddRange(_tradeItemHotspots.ConvertAll(hotspot => (Hotspot)hotspot));
        }

        private bool overLeftItem { get { return ((pointerPosWindow.X >= _leftItem.X) && (pointerPosWindow.X <= (_leftItem.X + _leftItem.Width))); } }
        private bool overRightItem { get { return ((pointerPosWindow.X >= _rightItem.X) && (pointerPosWindow.X <= (_rightItem.X + _rightItem.Width))); } }
        private bool overMiddleLeftItem { get { return ((pointerPosWindow.X >= _middleLeftItem.X) && (pointerPosWindow.X <= (_middleLeftItem.X + _middleLeftItem.Width))); } }
        private bool overMiddleRightItem { get { return ((pointerPosWindow.X >= _middleRightItem.X) && (pointerPosWindow.X <= (_middleRightItem.X + _middleRightItem.Width))); } }
        private bool overMiddleItem { get { return (overMiddleLeftItem || overMiddleRightItem); } } 

        void tradeItemHotspot_PrimaryActivation(object sender, PrimaryActivationEventArgs e)
        {
            int tradeItemIndex = ((TradeItemHotspot)sender).tradeItemIndex;

            if (this.overLeftItem)
            {
                // If over the left icon, set amount traded to 0, trade will be to the right
                // (provided there is no AI partner, otherwise the player can't control).
                if (!isAIPartnerActive())
                    _offsets[tradeItemIndex] = 0;
            }
            else if (this.overRightItem)
            {
                // If over the right icon, set amount traded to 0, trade will be to the left.
                _offsets[tradeItemIndex] = 0;
            }
            else if (this.overMiddleItem)
            {
                // If over the middle icons, set amount traded to the maximum.  However, if the
                // trade is to the right and there is a AI partner then it won't be changed.
                // Also if it is to the left and there is a AI partner and the trade item is locked
                // it won't be changed either.
                if (_offsets[tradeItemIndex] != 0)
                {
                    if ((_offsets[tradeItemIndex] > 0) && this.overMiddleLeftItem)
                    {
                        if (!isAIPartnerActive())
                        {
                            int lvalue = leftTrader.getValue<int>(_tradeItems[tradeItemIndex]);
                            _offsets[tradeItemIndex] = lvalue;
                        }
                    }
                    else if ((_offsets[tradeItemIndex] < 0) && this.overMiddleRightItem)
                    {
                        if (!isAIPartnerActive() || (_lockedRight[tradeItemIndex] != LockedState.Locked))
                        {
                            int rvalue = rightTrader.getValue<int>(_tradeItems[tradeItemIndex]);
                            _offsets[tradeItemIndex] = -rvalue;
                        }
                    }
                }
            }
        }

        void tradeItemHotspot_SecondaryActivation(object sender, SecondaryActivationEventArgs e)
        {
            if (!isAIPartnerActive())
                return;

            int tradeItemIndex = ((TradeItemHotspot)sender).tradeItemIndex;

            if (this.overLeftItem)
            {
                // Allow adding lock or removing lock.  If unlocked then that state is not changeable
                // (as the AI did it).
                if (_lockedLeft[tradeItemIndex] == LockedState.None)
                    _lockedLeft[tradeItemIndex] = LockedState.Locked;
                else if (_lockedLeft[tradeItemIndex] == LockedState.Locked)
                    _lockedLeft[tradeItemIndex] = LockedState.None;

                if (_offsets[tradeItemIndex] > 0)
                    _offsets[tradeItemIndex] = 0;
            }
            else if (this.overRightItem)
            {
                // Allow unlocking, provided the player can...
                if ((_lockedRight[tradeItemIndex] == LockedState.Locked) && unlock())
                    _lockedRight[tradeItemIndex] = LockedState.Unlocked;
                else if ((_lockedRight[tradeItemIndex] == LockedState.Unlocked) && relock())
                {
                    _lockedRight[tradeItemIndex] = LockedState.Locked;
                    _offsets[tradeItemIndex] = 0;
                }
            }
        }

        private bool overLeftArrow { get { return ((pointerPosWindow.X >= _arrowAvailableL.X) && (pointerPosWindow.X <= (_arrowAvailableL.X + _arrowAvailableL.Width))); } }
        private bool overRightArrow { get { return ((pointerPosWindow.X >= _arrowAvailableR.X) && (pointerPosWindow.X <= (_arrowAvailableR.X + _arrowAvailableR.Width))); } }

        void tradeItemHotspot_PointerOver(object sender, PointerOverEventArgs e)
        {
            if (!e.hasFocus)
                return;

            int tradeItemIndex = ((TradeItemHotspot)sender).tradeItemIndex;

            // Handle which arrow is selected.
            Rectangle arrowBody = default(Rectangle);
            TradingDirection tradingDirection = TradingDirection.None;
            if (overLeftArrow)
            {
                arrowBody = _arrowAvailableL;
                tradingDirection = TradingDirection.Right;
            }
            else if (overRightArrow)
            {
                arrowBody = _arrowAvailableR;
                tradingDirection = TradingDirection.Left;
            }

            // If there is an active AI partner then we can't select the left arrow (trade direction right).
            if (isAIPartnerActive() && (tradingDirection == TradingDirection.Right))
                return;

            // If there is an AI partner and a trade item is locked by the AI (right side), we can't
            // select the arrow.
            if (isAIPartnerActive() && (tradingDirection == TradingDirection.Left) && (_lockedRight[tradeItemIndex] == LockedState.Locked))
                return;
                    
            // Ensure that there are goods to trade for the arrow being clicked upon, if not, bail.
            int lvalue = leftTrader.getValue<int>(_tradeItems[tradeItemIndex]);
            if ((lvalue <= 0) && (tradingDirection == TradingDirection.Right))
                return;

            int rvalue = rightTrader.getValue<int>(_tradeItems[tradeItemIndex]);
            if ((rvalue <= 0) && (tradingDirection == TradingDirection.Left))
                return;

            //int arrowX = (int)e.pointerLocationHotspot.X - arrowBody.X;
            int arrowX = (int)pointerPosWindow.X - arrowBody.X;

            if (tradingDirection == TradingDirection.Right)
                _offsets[tradeItemIndex] = (int)(((float)arrowX / (float)arrowBody.Width) * lvalue);
            else if (tradingDirection == TradingDirection.Left)
                _offsets[tradeItemIndex] = -1 * (int)(((float)(arrowBody.Width - arrowX) / (float)arrowBody.Width) * rvalue);
        }

        public override void HandleInput(GameTime gameTime)
        {
#if false
            //throw new NotImplementedException();   

            //Vector2 pointerPosWindow;
            //bool primarySelected = false;
            //bool secondarySelected = false;

//#if WINDOWS
            //MouseState ms = Mouse.GetState();
            //pointerPosWindow = screenToWindow(this.pointerPos);
            //primarySelected = (ms.LeftButton == ButtonState.Pressed);
            //secondarySelected = (ms.RightButton == ButtonState.Pressed);
//#endif

            //Rectangle pointerPosHotspot = _tradeItemHotspots.FirstOrDefault(tih => tih.Contains(new Point((int)pointerPosWindow.X, (int)pointerPosWindow.Y)));

            //bool overLeftItem = ((pointerPosWindow.X >= _leftItem.X) && (pointerPosWindow.X <= (_leftItem.X + _leftItem.Width)));
            //bool overRightItem = ((pointerPosWindow.X >= _rightItem.X) && (pointerPosWindow.X <= (_rightItem.X + _rightItem.Width)));
            //bool overMiddleLeftItem = ((pointerPosWindow.X >= _middleLeftItem.X) && (pointerPosWindow.X <= (_middleLeftItem.X + _middleLeftItem.Width)));
            //bool overMiddleRightItem = ((pointerPosWindow.X >= _middleRightItem.X) && (pointerPosWindow.X <= (_middleRightItem.X + _middleRightItem.Width)));
            //bool overMiddleItem = (overMiddleLeftItem || overMiddleRightItem);

            //// Click
            //if (!_primarySelected && primarySelected)
            //    _selectedTradeItemHotspot = pointerPosHotspot;

            //// Release
            //if (_primarySelected && !primarySelected)
            //    _selectedTradeItemHotspot = default(Rectangle);

            //// MouseOver
            //if (!primarySelected)
            //    _hoveredTradeItemHotspot = pointerPosHotspot;

            //// Lock / unlock
            //if (_secondarySelected && !secondarySelected)
            //{
            //    // Get the index of the selected trade item.
            //    int selectedTradeItemIndex = -1;
            //    for (int i = 0; i < _tradeItemHotspots.Count; i++)
            //    {
            //        if (_tradeItemHotspots[i] != pointerPosHotspot)
            //            continue;

            //        selectedTradeItemIndex = i;
            //        break;
            //    }

            //    if ((selectedTradeItemIndex >= 0) && (pointerPosHotspot != default(Rectangle)))
            //    {
            //        if (overLeftItem)
            //        {
            //            // Lock / unlock the item.
            //            if (isAIPartner())
            //            {
            //                // Allow adding lock or removing lock.  If unlocked then that state is not changeable.
            //                if (_lockedLeft[selectedTradeItemIndex] == LockedState.None)
            //                    _lockedLeft[selectedTradeItemIndex] = LockedState.Locked;
            //                else if (_lockedLeft[selectedTradeItemIndex] == LockedState.Locked)
            //                    _lockedLeft[selectedTradeItemIndex] = LockedState.None;

            //                if (_offsets[selectedTradeItemIndex] > 0)
            //                    _offsets[selectedTradeItemIndex] = 0;
            //            }
            //        }
            //        else if (overRightItem)
            //        {
            //            // Allow unlocking, provided the player can...
            //            if (isAIPartner())
            //            {
            //                if ((_lockedRight[selectedTradeItemIndex] == LockedState.Locked) && unlock())
            //                    _lockedRight[selectedTradeItemIndex] = LockedState.Unlocked;
            //                else if ((_lockedRight[selectedTradeItemIndex] == LockedState.Unlocked) && relock())
            //                {
            //                    _lockedRight[selectedTradeItemIndex] = LockedState.Locked;
            //                    _offsets[selectedTradeItemIndex] = 0;
            //                }
            //            }
            //        }
            //    }
            //}
                
            //// Selection handling
            //if (_selectedTradeItemHotspot != default(Rectangle))
            //{
            //    // Get the index of the selected trade item.
            //    int selectedTradeItemIndex = -1;
            //    for (int i = 0; i < _tradeItemHotspots.Count; i++)
            //    {
            //        if (_tradeItemHotspots[i] != _selectedTradeItemHotspot)
            //            continue;

            //        selectedTradeItemIndex = i;
            //        break;
            //    }

            //    if ((selectedTradeItemIndex >= 0) && (pointerPosHotspot != default(Rectangle)))
            //    {
            //        if (overLeftItem)
            //        {
            //            // If over the left icon, set amount traded to 0, trade will be to the right
            //            // (provided there is no AI partner, otherwise the player can't control).
            //            if (!isAIPartner())
            //                _offsets[selectedTradeItemIndex] = 0;
            //        }
            //        else if (overRightItem)
            //        {
            //            // If over the right icon, set amount traded to 0, trade will be to the left.
            //            _offsets[selectedTradeItemIndex] = 0;
            //        }
            //        else if (overMiddleItem)
            //        {
            //            // If over the middle icon, set amount traded to the maximum.  However, if the
            //            // trade is to the right and there is a AI partner then it won't be changed.
            //            if (_offsets[selectedTradeItemIndex] != 0)
            //            {
            //                if ((_offsets[selectedTradeItemIndex] > 0) && overMiddleLeftItem)
            //                {
            //                    if (!isAIPartner())
            //                    {
            //                        int lvalue = leftTrader.getValue<int>(_tradeItems[selectedTradeItemIndex]);
            //                        _offsets[selectedTradeItemIndex] = lvalue;
            //                    }
            //                }
            //                else if ((_offsets[selectedTradeItemIndex] < 0) && overMiddleRightItem)
            //                {
            //                    if (!isAIPartner() || (_lockedRight[selectedTradeItemIndex] != LockedState.Locked))
            //                    {
            //                        int rvalue = rightTrader.getValue<int>(_tradeItems[selectedTradeItemIndex]);
            //                        _offsets[selectedTradeItemIndex] = -rvalue;
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    // Determine the selected arrow.
            //    Rectangle arrowSelected = default(Rectangle);
            //    TradingDirection tradingDirection = TradingDirection.None;
            //    if ((pointerPosWindow.X >= _arrowAvailableL.X) && (pointerPosWindow.X <= (_arrowAvailableL.X + _arrowAvailableL.Width)))
            //    {
            //        // Left arrow selected, provided there is no AI partner.
            //        if (!isAIPartner())
            //        {
            //            arrowSelected = _arrowAvailableL;
            //            tradingDirection = TradingDirection.Right;
            //        }
            //    }
            //    else if ((pointerPosWindow.X >= _arrowAvailableR.X) && (pointerPosWindow.X <= (_arrowAvailableR.X + _arrowAvailableR.Width)))
            //    {
            //        // Right arrow selected.
            //        arrowSelected = _arrowAvailableR;
            //        tradingDirection = TradingDirection.Left;
            //    }

            //    // If an arrow is selected then set the value according to the mouse pointer.
            //    if ((selectedTradeItemIndex >= 0) && (arrowSelected != default(Rectangle)))
            //    {
            //        int arrowX = (int)pointerPosWindow.X - arrowSelected.X;

            //        int lvalue = leftTrader.getValue<int>(_tradeItems[selectedTradeItemIndex]);
            //        int rvalue = rightTrader.getValue<int>(_tradeItems[selectedTradeItemIndex]);

            //        if (tradingDirection == TradingDirection.Right)
            //        {
            //            if (!isAIPartner())
            //                _offsets[selectedTradeItemIndex] = (int)(((float)arrowX / (float)arrowSelected.Width) * lvalue);
            //        }
            //        else if (tradingDirection == TradingDirection.Left)
            //        {
            //            if (!isAIPartner() || (_lockedRight[selectedTradeItemIndex] != LockedState.Locked))
            //                _offsets[selectedTradeItemIndex] = -1 * (int)(((float)(arrowSelected.Width - arrowX) / (float)arrowSelected.Width) * rvalue);
            //        }
            //    }
            //}

            //_primarySelected = primarySelected;
            //_secondarySelected = secondarySelected;
#endif

            update();
        }

        public override void Draw(GameTime gameTime, VariableBundle gameState, SpriteBatch spriteBatch)
        {
            _buttonOK.decorations = this.decorations;
            _buttonCancel.decorations = this.decorations;
            if (_buttonTakeAll != null)
                _buttonTakeAll.decorations = this.decorations;

            Color textColor = Color.Black;

            Graphic gPixel = IntoTheNewWorld.Instance.dictGraphics["trade_onepixel"];
            Frame fPixel = gPixel.getCurrentFrame(gameTime, gameState);

            Graphic gLocked = IntoTheNewWorld.Instance.dictGraphics["trade_locked"];
            Frame fLocked = gLocked.getCurrentFrame(gameTime, gameState);

            Graphic gUnlocked = IntoTheNewWorld.Instance.dictGraphics["trade_unlocked"];
            Frame fUnlocked = gUnlocked.getCurrentFrame(gameTime, gameState);

            Graphic gDesired = IntoTheNewWorld.Instance.dictGraphics["trade_desired"];
            Frame fDesired = gDesired.getCurrentFrame(gameTime, gameState);

            Graphic gUpGreen = IntoTheNewWorld.Instance.dictGraphics["trade_upgreen"];
            Frame fUpGreen = gUpGreen.getCurrentFrame(gameTime, gameState);

            Graphic gDownRed = IntoTheNewWorld.Instance.dictGraphics["trade_downred"];
            Frame fDownRed = gDownRed.getCurrentFrame(gameTime, gameState);

            // TODO: Just let Window handle this?  Or have it everywhere?
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, Window.rasterizerState, null, this.mxTWindowToScreen);

            Graphic g;

            // Draw the background if it wasn't already provided to the Window.
            if (this.background == null)
            {
                g = IntoTheNewWorld.Instance.dictGraphics["trade_background"];
                //spriteBatch.Draw(IntoTheNewWorld.Instance.getTexture(g), new Rectangle(0, 0, this.bounds.Width, this.bounds.Height), g.getCurrentFrame(gameTime, gameState).bounds, Color.White);
#if OLD_TEXTURE
                spriteBatch.Draw(IntoTheNewWorld.Instance.getTexture(gPixel), new Rectangle(0, 0, this.bounds.Width, this.bounds.Height), fPixel.bounds, Color.Tan);
#else
                gPixel.Draw(gameTime, gameState, spriteBatch, new Rectangle(0, 0, this.bounds.Width, this.bounds.Height), Color.Tan);
#endif
            }

            // TODO: Combine items and arrows drawing, cache Graphics, etc.

            // Draw items...
            int itemsXBuffer = 20;
            int leftTraderX = itemsXBuffer + (isAIPartner() ? fUpGreen.bounds.Width : 0);
            int rightTraderX = -1;
            int middleLeftTraderX = -1;
            int middleRightTraderX = -1;

            int itemsMaxWidth = 0;

            int middleTraderX = -1;

            int currentY = _itemsTop;
            for (int i = 0; i < _tradeItems.Count; i++)
            {
                string tradeItem = _tradeItems[i];

                g = IntoTheNewWorld.Instance.dictGraphics["trade_" + tradeItem];
                Frame f = g.getCurrentFrame(gameTime, gameState);

                itemsMaxWidth = Math.Max(itemsMaxWidth, f.bounds.Width);

                rightTraderX = this.bounds.Width - itemsXBuffer - f.bounds.Width;
                middleLeftTraderX = (int)this.center.X - (f.bounds.Width) - itemsXBuffer;
                middleRightTraderX = (int)this.center.X + itemsXBuffer;
                if (_offsets[i] > 0)
                    middleTraderX = middleLeftTraderX;
                else
                    middleTraderX = middleRightTraderX;

                // Draw the hover background.
                _tradeItemHotspots[i].bounds = new Rectangle(leftTraderX, currentY, (rightTraderX + f.bounds.Width) - leftTraderX, f.bounds.Height);
                if (this.pointerOverHotspot == _tradeItemHotspots[i])
#if OLD_TEXTURE
                    spriteBatch.Draw(IntoTheNewWorld.Instance.getTexture(gPixel), this.pointerOverHotspot.bounds, fPixel.bounds, Color.Beige);
#else
                    gPixel.Draw(gameTime, gameState, spriteBatch, this.pointerOverHotspot.bounds, Color.Beige);
#endif

#if OLD_TEXTURE
                Texture2D t2dTradeItem = IntoTheNewWorld.Instance.getTexture(g);
#endif

                Rectangle bounds;

                // Draw the desired green up arrow.
#if OLD_TEXTURE
                if (_desired[i])
                    spriteBatch.Draw(t2dTradeItem, new Rectangle(leftTraderX - fUpGreen.bounds.Width, currentY + ((f.bounds.Height - fUpGreen.bounds.Height) / 2), fUpGreen.bounds.Width, fUpGreen.bounds.Height), fUpGreen.bounds, Color.White);
                else if (_undesired[i])
                    spriteBatch.Draw(t2dTradeItem, new Rectangle(leftTraderX - fDownRed.bounds.Width, currentY + ((f.bounds.Height - fDownRed.bounds.Height) / 2), fDownRed.bounds.Width, fDownRed.bounds.Height), fDownRed.bounds, Color.White);
#else
                if (_desired[i])
                    gUpGreen.Draw(gameTime, gameState, spriteBatch, new Point(leftTraderX - fUpGreen.bounds.Width, currentY + ((f.bounds.Height - fUpGreen.bounds.Height) / 2)));
                else if (_undesired[i])
                    gDownRed.Draw(gameTime, gameState, spriteBatch, new Point(leftTraderX - fDownRed.bounds.Width, currentY + ((f.bounds.Height - fDownRed.bounds.Height) / 2)));
#endif

                // Draw left.
#if OLD_TEXTURE
                bounds = new Rectangle(leftTraderX, currentY, f.bounds.Width, f.bounds.Height);
                spriteBatch.Draw(t2dTradeItem, bounds, f.bounds, Color.White);
                if (isAIPartnerActive())
                {
                    if (_lockedLeft[i] == LockedState.Locked)
                        spriteBatch.Draw(t2dTradeItem, new Rectangle(leftTraderX, currentY, fLocked.bounds.Width, fLocked.bounds.Height), fLocked.bounds, Color.White);
                    else if (_lockedLeft[i] == LockedState.Unlocked)
                        spriteBatch.Draw(t2dTradeItem, new Rectangle(leftTraderX, currentY, fUnlocked.bounds.Width, fUnlocked.bounds.Height), fUnlocked.bounds, Color.White);
                    if (_desired[i])
                        spriteBatch.Draw(t2dTradeItem, new Rectangle(leftTraderX + f.bounds.Width - fDesired.bounds.Width, currentY, fDesired.bounds.Width, fDesired.bounds.Height), fDesired.bounds, Color.White);
                }
#else
                g.Draw(gameTime, gameState, spriteBatch, new Point(leftTraderX, currentY));
                if (isAIPartnerActive())
                {
                    if (_lockedLeft[i] == LockedState.Locked)
                        gLocked.Draw(gameTime, gameState, spriteBatch, new Point(leftTraderX, currentY));
                    else if (_lockedLeft[i] == LockedState.Unlocked)
                        gUnlocked.Draw(gameTime, gameState, spriteBatch, new Point(leftTraderX, currentY));
                    if (_desired[i])
                        gDesired.Draw(gameTime, gameState, spriteBatch, new Point(leftTraderX + f.bounds.Width - fDesired.bounds.Width, currentY));
                }
#endif

                // Draw right.
#if OLD_TEXTURE
                bounds = new Rectangle(rightTraderX, currentY, f.bounds.Width, f.bounds.Height);
                spriteBatch.Draw(t2dTradeItem, bounds, f.bounds, Color.White);
                if (isAIPartnerActive())
                {
                    if (_lockedRight[i] == LockedState.Locked)
                        spriteBatch.Draw(t2dTradeItem, new Rectangle(rightTraderX, currentY, fLocked.bounds.Width, fLocked.bounds.Height), fLocked.bounds, Color.White);
                    else if (_lockedRight[i] == LockedState.Unlocked)
                        spriteBatch.Draw(t2dTradeItem, new Rectangle(rightTraderX, currentY, fUnlocked.bounds.Width, fUnlocked.bounds.Height), fUnlocked.bounds, Color.White);
                }
#else
                g.Draw(gameTime, gameState, spriteBatch, new Point(rightTraderX, currentY));
                if (isAIPartnerActive())
                {
                    if (_lockedRight[i] == LockedState.Locked)
                        gLocked.Draw(gameTime, gameState, spriteBatch, new Point(rightTraderX, currentY));
                    else if (_lockedRight[i] == LockedState.Unlocked)
                        gUnlocked.Draw(gameTime, gameState, spriteBatch, new Point(rightTraderX, currentY));
                }
#endif

                // Draw middle.
#if OLD_TEXTURE
                if (_offsets[i] != 0)
                    spriteBatch.Draw(t2dTradeItem, new Rectangle(middleTraderX, currentY, f.bounds.Width, f.bounds.Height), f.bounds, Color.White);
#else
                if (_offsets[i] != 0)
                    g.Draw(gameTime, gameState, spriteBatch, new Point(middleTraderX, currentY));
#endif

                int textY = currentY + f.bounds.Height;

                int leftMen = leftTrader.getValue<int>("men");
                bool food = (tradeItem == "food");

                int sum = leftTrader.getValue<int>(tradeItem);
                int adjustedValue = (sum - _offsets[i]);
                string svalue = "" + sum + ((adjustedValue != sum) ? (" (" + adjustedValue + ")") : "");
                if (food)
                {
                    sum = IntoTheNewWorld.Instance.totalFoodToWeeks(sum, leftMen);
                    adjustedValue = IntoTheNewWorld.Instance.totalFoodToWeeks(adjustedValue, leftMen);
                    svalue = "" + sum + "w" + ((adjustedValue != sum) ? (" (" + adjustedValue + "w)") : "");
                }
                spriteBatch.DrawString(_font, svalue, new Vector2(leftTraderX, textY), textColor);

                sum = rightTrader.getValue<int>(tradeItem);
                adjustedValue = (sum + _offsets[i]);
                svalue = ((adjustedValue != sum) ? ("(" + adjustedValue + ") ") : "") + sum;
                if (food)
                {
                    sum = IntoTheNewWorld.Instance.totalFoodToWeeks(sum, leftMen);
                    adjustedValue = IntoTheNewWorld.Instance.totalFoodToWeeks(adjustedValue, leftMen);
                    svalue = ((adjustedValue != sum) ? ("(" + adjustedValue + "w) ") : "") + sum + "w";
                }
                Vector2 v2svalueDims = _font.MeasureString(svalue);
                spriteBatch.DrawString(_font, svalue, new Vector2(rightTraderX + f.bounds.Width - v2svalueDims.X, textY), textColor);

                int offset = _offsets[i];
                if (_offsets[i] != 0)
                {
                    offset = Math.Abs(offset);
                    svalue = ("" + offset);
                    if (food)
                    {
                        offset = IntoTheNewWorld.Instance.totalFoodToWeeks(offset, leftMen);
                        svalue = (offset + "w");
                    }
                    //int textWidth = (int)_font.MeasureString("" + Math.Abs(_offsets[i])).X;
                    //spriteBatch.DrawString(_font, "" + Math.Abs(_offsets[i]), new Vector2((middleTraderX + (f.bounds.Width / 2)) - (textWidth / 2), textY), textColor);
                    int textWidth = (int)_font.MeasureString(svalue).X;
                    spriteBatch.DrawString(_font, svalue, new Vector2((middleTraderX + (f.bounds.Width / 2)) - (textWidth / 2), textY), textColor);
                }

                currentY += f.bounds.Height + _itemsYBuffer + _textHeight;
            }

            _leftItem = new Rectangle(leftTraderX, 0, itemsMaxWidth, 0);
            _rightItem = new Rectangle(rightTraderX, 0, itemsMaxWidth, 0);
            _middleLeftItem = new Rectangle(middleLeftTraderX, 0, itemsMaxWidth, 0);
            _middleRightItem = new Rectangle(middleRightTraderX, 0, itemsMaxWidth, 0);

            // Draw arrows...
            int arrowXBuffer = 10;
            int arrowY;

            int arrowHeight = _arrowLtR.gBody.getCurrentFrame(gameTime, gameState).bounds.Height;

            currentY = _itemsTop;
            for (int i = 0; i < _offsets.Count; i++)
            {
                //int arrowLeftX = itemsXBuffer + itemsMaxWidth + arrowXBuffer;
                int arrowLeftX = leftTraderX + itemsMaxWidth + arrowXBuffer;
                int arrowRightX = this.bounds.Width - itemsXBuffer - itemsMaxWidth - arrowXBuffer;

                string tradeItem = _tradeItems[i];
                int offset = _offsets[i];
                if (tradeItem == "food")
                    offset = IntoTheNewWorld.Instance.totalFoodToWeeks(offset, leftTrader.getValue<int>("men"));

                g = IntoTheNewWorld.Instance.dictGraphics["trade_" + tradeItem];
                Frame f = g.getCurrentFrame(gameTime, gameState);

                arrowY = currentY + ((f.bounds.Height - arrowHeight) / 2);

                //drawArrow(spriteBatch, gameTime, gameState, offset, arrowY, arrowLeftX, arrowRightX, middleLeftTraderX, middleRightTraderX, (f.bounds.Width / 2) + arrowXBuffer, tradeItem);
                drawArrow(spriteBatch, gameTime, gameState, offset, arrowY, arrowLeftX, arrowRightX, middleLeftTraderX, middleRightTraderX, arrowXBuffer, itemsMaxWidth, tradeItem);

                currentY += f.bounds.Height + _itemsYBuffer + _textHeight;
            }

            if (isAIPartner())
            {
                //spriteBatch.DrawString(_font, _tradeAdviceMessage + " (" + _receiveToOfferRatio + ")", new Vector2(itemsXBuffer, currentY), textColor);
                spriteBatch.DrawString(_font, _tradeAdviceMessage, new Vector2(itemsXBuffer, currentY), textColor);
            }

            // Draw traders...
            spriteBatch.DrawString(_font, _leftTraderString, new Vector2(itemsXBuffer, _itemsYBuffer), textColor);
            Vector2 textSize = _font.MeasureString(_rightTraderString);
            spriteBatch.DrawString(_font, _rightTraderString, new Vector2(this.bounds.Width - itemsXBuffer - (int)textSize.X, _itemsYBuffer), textColor);

            // Draw offer / receive...
            if (_offsets.Any(offset => offset != 0))
            {
                string leftText = "offer";
                string rightText = "receive";
                if (_partner == TradingPartner.Cache)
                {
                    leftText = "store";
                    rightText = "retrieve";
                }
                else if (_partner == TradingPartner.Loot)
                {
                    leftText = "drop";
                    rightText = "take";
                }

                spriteBatch.DrawString(_font, leftText, new Vector2(middleLeftTraderX, _itemsYBuffer), Color.Red);
                spriteBatch.DrawString(_font, rightText, new Vector2(middleRightTraderX, _itemsYBuffer), Color.Green);
            }

            //// Draw Deal / NO Deal.
            //if (_partnerAcceptsTrade)
            //{
            //    textSize = _font.MeasureString("Deal");
            //    spriteBatch.DrawString(_font, "Deal", new Vector2((int)this.center.X - ((int)textSize.X / 2), this.bounds.Height - 25), Color.Green);
            //}
            //else
            //{
            //    textSize = _font.MeasureString("NO Deal");
            //    spriteBatch.DrawString(_font, "NO deal", new Vector2((int)this.center.X - ((int)textSize.X / 2), this.bounds.Height - 25), Color.Red);
            //}

            //spriteBatch.Draw(IntoTheNewWorld.Instance.getTexture(gPixel), new Rectangle(_arrowAvailableL.X, 0, _arrowAvailableL.Width, 200), fPixel.bounds, Color.Red);
            //spriteBatch.Draw(IntoTheNewWorld.Instance.getTexture(gPixel), new Rectangle(_arrowAvailableR.X, 0, _arrowAvailableR.Width, 200), fPixel.bounds, Color.Green);

            spriteBatch.End();

            // Draw OK / Cancel
            _buttonOK.bounds = new Rectangle(this.leftBuffer, this.bounds.Height - this.buttonBottomBuffer - _buttonOK.bounds.Height, _buttonOK.bounds.Width, _buttonOK.bounds.Height);
            //_buttonOK.Draw(gameTime, gameState, spriteBatch, IntoTheNewWorld.Instance, this.mxTWindowToScreen);
            _buttonCancel.bounds = new Rectangle(this.bounds.Width - this.rightBuffer - _buttonCancel.bounds.Width, this.bounds.Height - this.buttonBottomBuffer - _buttonCancel.bounds.Height, _buttonCancel.bounds.Width, _buttonCancel.bounds.Height);
            //_buttonCancel.Draw(gameTime, gameState, spriteBatch, IntoTheNewWorld.Instance, this.mxTWindowToScreen);

            // Draw Take All
            if (_buttonTakeAll != null)
                _buttonTakeAll.bounds = new Rectangle((this.bounds.Width / 2) - (_buttonTakeAll.bounds.Width / 2), this.bounds.Height - this.buttonBottomBuffer - _buttonTakeAll.bounds.Height, _buttonTakeAll.bounds.Width, _buttonTakeAll.bounds.Height);
        }

        private void drawArrow(SpriteBatch spriteBatch, GameTime gameTime, VariableBundle gameState, int offset, int arrowY, int arrowLeftX, int arrowRightX, int middleLeftTraderX, int middleRightTraderX, int middleBufferX, int itemWidth, string tradeItem)
        {
            // Set the correct arrow direction based on whether the trade item is being given away or received.
            Arrow arrow = _arrowLtR;
            VariableBundle trader = this.leftTrader;
            if (offset < 0)
            {
                arrow = _arrowRtL;
                trader = this.rightTrader;
            }

            // Load the current frame for the arrow parts.
            Frame fTail = arrow.gTail.getCurrentFrame(gameTime, gameState);
            Frame fBody = arrow.gBody.getCurrentFrame(gameTime, gameState);
            Frame fHead = arrow.gHead.getCurrentFrame(gameTime, gameState);
#if OLD_TEXTURE
            Texture2D t2dArrow = arrow.t2d;
#endif

            // Determine the available width of the arrow -- this is the space between the head and tail, i.e. the body.
            // This width will scale with the amount of the item being traded (relative to the total amount available to trade).
            int arrowAvailableWidth = ((middleLeftTraderX - middleBufferX) - arrowLeftX) - fTail.bounds.Width - fHead.bounds.Width;

            _arrowAvailableL = new Rectangle(arrowLeftX + fTail.bounds.Width, 0, arrowAvailableWidth, 0);
            _arrowAvailableR = new Rectangle(middleRightTraderX + itemWidth + middleBufferX + fHead.bounds.Width, 0, arrowAvailableWidth, 0);

            if (offset == 0)
                return;

            // Get the current sum of the good.  This will be the original value supplied to the dialog, as it
            // isn't adjusted in place until a successful trade -- offset stores the changes in the meantime so Cancel
            // can just undo everything "for free".
            int sum = trader.getValue<int>(tradeItem);
            if (tradeItem == "food")
                sum = IntoTheNewWorld.Instance.totalFoodToWeeks(sum, leftTrader.getValue<int>("men"));

            // Determine the actual width of the body.
            int arrowWidth = (int)(((float)Math.Abs(offset) / (float)sum) * (float)arrowAvailableWidth);

            int tailX = arrowLeftX;
            int bodyX = arrowLeftX + fTail.bounds.Width;
            int headX = arrowLeftX + fTail.bounds.Width + arrowWidth;
            if (offset < 0)
            {
                tailX = arrowRightX - fTail.bounds.Width;
                bodyX = arrowRightX - fTail.bounds.Width - arrowWidth;
                headX = arrowRightX - fTail.bounds.Width - arrowWidth - fHead.bounds.Width;
            }

            // Draw tail, body and head.
#if OLD_TEXTURE
            spriteBatch.Draw(t2dArrow, new Rectangle(tailX, arrowY, fTail.bounds.Width, fTail.bounds.Height), fTail.bounds, Color.White);
            spriteBatch.Draw(t2dArrow, new Rectangle(bodyX, arrowY, arrowWidth, fBody.bounds.Height), fBody.bounds, Color.White);
            spriteBatch.Draw(t2dArrow, new Rectangle(headX, arrowY, fHead.bounds.Width, fHead.bounds.Height), fHead.bounds, Color.White);
#else
            arrow.gTail.Draw(gameTime, gameState, spriteBatch, new Point(tailX, arrowY));
            arrow.gBody.Draw(gameTime, gameState, spriteBatch, new Rectangle(bodyX, arrowY, arrowWidth, fBody.bounds.Height));
            arrow.gHead.Draw(gameTime, gameState, spriteBatch, new Point(headX, arrowY));
#endif
        }

        private bool isAIPartner()
        {
            if ((_partner == TradingPartner.Cache) ||
                (_partner == TradingPartner.Europe) ||
                (_partner == TradingPartner.Fleet) ||
                (_partner == TradingPartner.Loot))
                return false;

            return true;
        }

        private bool isAIPartnerActive()
        {
            if (!isAIPartner())
                return false;

            return false;
        }

        private bool unlock()
        {
            if (_unlocks <= 0)
                return false;

            _unlocks--;
            return true;
        }

        private bool relock()
        {
            _unlocks++;
            return true;
        }

        private void initializeAIState()
        {
            if (!isAIPartner())
                return;

            List<KeyValuePair<string, int>> sortedTradeItems = _dictValueByTradeItem.OrderByDescending(kvp => kvp.Value).ToList();
            _sortedTradeItemsLeft = sortedTradeItems.Where(kvp => this.leftTrader.getValue<int>(kvp.Key) > 0).ToList();
            List<KeyValuePair<string, int>> sortedTradeItemsRight = sortedTradeItems.Where(kvp => this.rightTrader.getValue<int>(kvp.Key) > 0).ToList();

            //if (isAIPartnerActive())
            {
                if (_sortedTradeItemsLeft.Count > 0)
                {
                    string desiredTradeItem = _sortedTradeItemsLeft[0].Key;
                    if (desiredTradeItem != default(string))
                    {
                        int idx;
                        if (_dictIndexByTradeItem.TryGetValue(desiredTradeItem, out idx))
                            _desired[idx] = true;
                    }

                    if (_sortedTradeItemsLeft.Count > 1)
                    {
                        string undesiredTradeItem = _sortedTradeItemsLeft[_sortedTradeItemsLeft.Count - 1].Key;
                        if (undesiredTradeItem != default(string))
                        {
                            int idx;
                            if (_dictIndexByTradeItem.TryGetValue(undesiredTradeItem, out idx))
                                _undesired[idx] = true;
                        }
                    }
                }
            }

            if (isAIPartnerActive())
            {
                if (sortedTradeItemsRight.Count > 0)
                {
                    for (int i = 0; i < _locksTradePartner; i++)
                    {
                        if (i >= sortedTradeItemsRight.Count)
                            break;

                        int idx;
                        if (_dictIndexByTradeItem.TryGetValue(sortedTradeItemsRight[i].Key, out idx))
                            _lockedRight[idx] = LockedState.Locked;
                    }
                }
            }
        }

        private Dictionary<string, int> getAcceptedTradeItems()
        {
            // { "men", "food", "ships", "gold", "oldworld_goods", "newworld_goods", "horses", "weapons" };
            string[] acceptedTradeItems;
            int[] values = null;

            switch (_partner)
            {
                case TradingPartner.Cache:
                    acceptedTradeItems = IntoTheNewWorldCache.getAcceptedItems(false);
                    break;
                case TradingPartner.Europe:
                    acceptedTradeItems = new string[] { "men", "food", "ships", "gold", "oldworld_goods", "newworld_goods", "horses", "weapons" };
                    break;
                case TradingPartner.Explorer:
                    acceptedTradeItems = new string[] { "food", "gold", "oldworld_goods", "newworld_goods", "horses", "weapons" };
                    values = new int[] { 1, 20, 1, 5, 7, 10 };
                    break;
                case TradingPartner.Fleet:
                    acceptedTradeItems = new string[] { "men", "food", "gold", "oldworld_goods", "newworld_goods", "horses", "weapons" };
                    break;
                case TradingPartner.Fort:
                case TradingPartner.Mission:
                case TradingPartner.Settlement:
                case TradingPartner.TradingPost:
                    acceptedTradeItems = new string[] { "men", "food", "gold", "oldworld_goods", "newworld_goods", "horses", "weapons" }; // TODO: Maybe not men?
                    break;
                case TradingPartner.Loot:
                    acceptedTradeItems = IntoTheNewWorldCache.getAcceptedItems(true);
                    break;
                case TradingPartner.Native:
                    acceptedTradeItems = new string[] { "food", "gold", "oldworld_goods", "newworld_goods", "horses", "weapons" };
                    values = new int[] { 1, 10, 3, 1, 7, 12 };
                    break;
                default:
                    acceptedTradeItems = new string[] { };
                    break;
            }

            if (values == null)
            {
                values = new int[acceptedTradeItems.Length];
                for (int i = 0; i < acceptedTradeItems.Length; i++)
                    values[i] = 1;
            }

            // TODO: Tweak values based on specific AI needs / stockpiles.

            int foodMultiplier = IntoTheNewWorld.Instance.weeksToTotalFood(4, 1);
            Dictionary<string, int> dictValueByTradeItem = new Dictionary<string, int>();
            for (int i = 0; i < acceptedTradeItems.Length; i++)
            {
                int value = values[i];
                if (acceptedTradeItems[i] != "food")
                    value *= foodMultiplier;
                dictValueByTradeItem.Add(acceptedTradeItems[i], value);
            }

            return dictValueByTradeItem;
        }

        private void update()
        {
            if (!isAIPartner())
                return;

            // Figure out the worth of what the player is asking of the partner...
            float receivingValue = 0;
            for (int i = 0; i < _tradeItems.Count; i++)
            {
                if (_offsets[i] >= 0)
                    continue;

                // Item credit
                float itemCredit = _offsets[i] * _values[i];

                // If the offset is negative and the item is unlocked then it'll cost
                // the player more, make the credit more negative.
                if (_lockedRight[i] == LockedState.Unlocked)
                    itemCredit *= 1.25f;

                receivingValue += itemCredit;
            }

            //float minimumOfferingValue = 0.95f * Math.Abs(receivingValue);

            // Take from the player...
            float offeringValue = 0;
            float penalty = 0.00f;
            foreach (KeyValuePair<string, int> tradeItemLeft in _sortedTradeItemsLeft)
            {
                int idx;
                if (!_dictIndexByTradeItem.TryGetValue(tradeItemLeft.Key, out idx))
                    continue;

                // If already being traded R to L continue to next item.
                if (_offsets[idx] < 0)
                    continue;

                // If locked, continue.  
                // Locked items annoy the AI trading partner -- incurring a penalty for
                // the next item they try and trade for.
                if (_lockedLeft[idx] == LockedState.Locked)
                {
                    penalty = Math.Min(penalty + 0.1f, 0.5f);
                    continue;
                }

                if (isAIPartnerActive())
                    _offsets[idx] = 0;

                //if (offeringValue >= minimumOfferingValue)
                //    continue;

                // See if there are any to be taken...
                int available = this.leftTrader.getValue<int>(_tradeItems[idx]);
                if (available <= 0)
                    continue;

                // Item credit
                float itemCredit = _values[idx];

                // Lower the item credit by the penalty.
                itemCredit -= penalty;

                // If the offset is positive and the item is desired then give the
                // player extra credit, make the credit more positive.
                if (_desired[idx])
                    itemCredit *= 1.25f;

                // If the offset is positive and the item is undesired then give the
                // player less credit, make the credit less positive.
                if (_undesired[idx])
                    itemCredit *= 0.75f;

                if (isAIPartnerActive())
                {
                    int numTake = Math.Min((int)(Math.Abs(offeringValue + receivingValue) / itemCredit) + 1, available);

                    _offsets[idx] = numTake;
                    offeringValue += (numTake * itemCredit);
                }
                else
                    offeringValue += (_offsets[idx] * itemCredit);
            }

            // TODO: Handle very small amounts how?

            //_partnerAcceptsTrade = (offeringValue >= minimumOfferingValue);
            _partnerAcceptsTrade = false;
            _tradeAdviceMessage = "";
            _factionChange = 0;

            _receiveToOfferRatio = Math.Abs(receivingValue) / Math.Abs(offeringValue);

            if (float.IsInfinity(_receiveToOfferRatio))
            {
                _partnerAcceptsTrade = false;
                _tradeAdviceMessage = "They will never accept the trade proposal, it is a grave insult.";
                _factionChange = -10;
            }
            else if (float.IsNaN(_receiveToOfferRatio))
            {
                _partnerAcceptsTrade = true;
                _tradeAdviceMessage = "Propose a trade to get feedback about how it is likely to be received.";
                _factionChange = 0;
            }
            else if (_receiveToOfferRatio <= 0.10f)
            {
                _partnerAcceptsTrade = true;
                _tradeAdviceMessage = "They will be very pleased with your offering!";
                _factionChange = 10;
            }
            else if (_receiveToOfferRatio <= 0.75f)
            {
                _partnerAcceptsTrade = true;
                _tradeAdviceMessage = "They will be pleased with your generous offer.";
                _factionChange = 5;
            }
            else if (_receiveToOfferRatio <= 0.90f)
            {
                _partnerAcceptsTrade = true;
                _tradeAdviceMessage = "They will be pleased with your trade proposal.";
                _factionChange = 2;
            }
            else if (_receiveToOfferRatio <= 1.10f)
            {
                _partnerAcceptsTrade = true;
                _tradeAdviceMessage = "This appears to be a fair trade for both parties.";
                _factionChange = 0;
            }
            else if (_receiveToOfferRatio <= 1.25f)
            {
                _partnerAcceptsTrade = true;
                _tradeAdviceMessage = "They will not be pleased with your trade proposal, but will accept.";
                _factionChange = -2;
            }
            else if (_receiveToOfferRatio <= 1.90f)
            {
                _partnerAcceptsTrade = false;
                _tradeAdviceMessage = "They will regard the trade proposal as an insult and reject it.";
                _factionChange = -5;
            }
            else
            {
                _partnerAcceptsTrade = false;
                _tradeAdviceMessage = "They will never accept the trade proposal, it is a grave insult.";
                _factionChange = -10;
            }
        }
    }
}
