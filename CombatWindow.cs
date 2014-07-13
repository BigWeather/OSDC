using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if Allow_XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endif
using GamesLibrary;

namespace IntoTheNewWorld
{
    public class CombatWindow : Window
    {
        private Button _buttonFight;
        private Button _buttonDoNotFight;

        public MapObject attacker { get; private set; }
        public MapObject defender { get; private set; }

        private SpriteFont _font = (SpriteFont)FontManager.Instance.getFont(IntoTheNewWorld.Instance.fontName);

        private string _text;
        private string _textRight;

        public CombatWindow(Vector2 v2Center, int width, MapObject attacker, MapObject defender)
            : base(v2Center, IntoTheNewWorld.Instance)
        {
            this.bounds = new Rectangle(this.bounds.X, this.bounds.Y, width, this.bounds.Height);

            this.attacker = attacker;
            this.defender = defender;

            initializeButtons();

            int attackerLosses;
            int defenderLosses;
            getExpectedLosses(attacker, defender, false, out attackerLosses, out defenderLosses);

            _text = getText(attacker, attacker.text);
            _text += (((attackerLosses == 0) ? "no" : "" + attackerLosses) + " expected casualt" + ((attackerLosses == 1) ? "y" : "ies") + "\n");

            //_text += "\n";

            _textRight = getText(defender, defender.text);
            _textRight += (((defenderLosses == 0) ? "no" : "" + defenderLosses) + " expected casualt" + ((defenderLosses == 1) ? "y" : "ies") + "\n");

            //_text += "\n";
            //_text += (getOdds(attacker, defender) + "% odds");

            Vector2 textSize = _font.MeasureString(_text);
            Vector2 textRightSize = _font.MeasureString(_textRight);

            int textHeight = Math.Max((int)textSize.Y, (int)textRightSize.Y);
            int textGutter = 50;

            int heightNew = this.textTopBuffer + textHeight + this.betweenBuffer + _buttonFight.bounds.Height + this.buttonBottomBuffer;
            int widthNew = this.leftBuffer + (int)textSize.X + textGutter + (int)textRightSize.X + this.rightBuffer;
            this.bounds = new Rectangle(this.bounds.X, this.bounds.Y, widthNew, heightNew);
        }

        public string getText(MapObject participant, string participantName)
        {
            int men = participant.state.getValue<int>("men");
            int weapons = participant.state.getValue<int>("weapons");
            int horses = participant.state.getValue<int>("horses");
            Map.MapNode mapNode = IntoTheNewWorld.Instance._map.getMapNode(participant.positionWorld);
            MapTile mapTile = IntoTheNewWorld.Instance._map.getTile(mapNode);
            Terrain terrain = mapTile.terrain;

            string text = participantName + "\n";
            text += (men + " men\n");
            text += (weapons + " weapons\n");
            text += (horses + " horses\n");
            text += (terrain.graphicIdentifier + " terrain\n");

            return text;
        }

        public void initializeButtons()
        {
            _buttonFight = new Button(new Rectangle(0, 0, 0, 0), "Engage", _font, Color.Green, true);
            _buttonFight.Press += new EventHandler<PressEventArgs>(_buttonFight_Press);
            this.addButton(_buttonFight);

            string doNotFightText = "Flee";
            if (attacker == IntoTheNewWorld.Instance.players[0])
                doNotFightText = "Cancel";

            _buttonDoNotFight = new Button(new Rectangle(0, 0, 0, 0), doNotFightText, _font, Color.Red, true);
            _buttonDoNotFight.Press += new EventHandler<PressEventArgs>(_buttonDoNotFight_Press);
            this.addButton(_buttonDoNotFight);
        }

        void _buttonDoNotFight_Press(object sender, PressEventArgs e)
        {
            string combatResultsMessage = null;

            if (this.defender == IntoTheNewWorld.Instance.players[0])
                CombatWindow.resolveCombat(this.attacker, this.defender, true, out combatResultsMessage);

            IntoTheNewWorld.Instance.Hide(this);

            if (combatResultsMessage != null)
                displayCombatResults(combatResultsMessage);
        }

        void _buttonFight_Press(object sender, PressEventArgs e)
        {
            string combatResultsMessage;
            CombatWindow.resolveCombat(this.attacker, this.defender, false, out combatResultsMessage); // NOTE: false doesn't mean the defender won't flee, just that it's not automatic.

            int factionCost = 0;
            if (this.defender is City)
                factionCost = 30;
            else if (this.defender is Trader)
                factionCost = 20;
            else if (this.defender is Warrior)
                factionCost = 10;

            int native_like = IntoTheNewWorld.Instance.players[0].state.getValue<int>("native_like");
            IntoTheNewWorld.Instance.players[0].state.setValue("native_like", Math.Max(-100, native_like - factionCost));

            IntoTheNewWorld.Instance.forceUpdateGameState = true;

            IntoTheNewWorld.Instance.Hide(this);

            displayCombatResults(combatResultsMessage);
        }

        private void displayCombatResults(string combatResultsMessage)
        {
            Vector2 v2Center = this.center;
            v2Center += new Vector2(this.bounds.X, this.bounds.Y);
            MessageBox combatResultsMessageBox = new MessageBox(combatResultsMessage, _font, v2Center, IntoTheNewWorld.Instance);
            combatResultsMessageBox.decorations = this.decorations;
            combatResultsMessageBox.background = this.background;
            IntoTheNewWorld.Instance.Show(combatResultsMessageBox);

            IntoTheNewWorld.Instance.forceUpdateGameState = true;
        }

        public override void HandleInput(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime, VariableBundle gameState, SpriteBatch spriteBatch)
        {
            // TODO: Both TradeWindow and this do this, as well as set bounds below...  Centralize?
            // TODO: Also they both do the same SpriteBatch.Begin, etc.  Pre-Draw and Post-Draw, maybe?
            _buttonFight.decorations = this.decorations;
            _buttonDoNotFight.decorations = this.decorations;

            Color textColor = Color.Black;

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, Window.rasterizerState, null, this.mxTWindowToScreen);

            Vector2 v2TextRightSize = _font.MeasureString(_textRight);

            spriteBatch.DrawString(_font, _text, new Vector2(this.leftBuffer, this.textTopBuffer), textColor);
            spriteBatch.DrawString(_font, _textRight, new Vector2(this.bounds.Width - this.rightBuffer - (int)v2TextRightSize.X, this.textTopBuffer), textColor);

            spriteBatch.End();

            _buttonFight.bounds = new Rectangle(this.leftBuffer, this.bounds.Height - this.buttonBottomBuffer - _buttonFight.bounds.Height, _buttonFight.bounds.Width, _buttonFight.bounds.Height);
            //_buttonOK.Draw(gameTime, gameState, spriteBatch, IntoTheNewWorld.Instance, this.mxTWindowToScreen);
            _buttonDoNotFight.bounds = new Rectangle(this.bounds.Width - this.rightBuffer - _buttonDoNotFight.bounds.Width, this.bounds.Height - this.buttonBottomBuffer - _buttonDoNotFight.bounds.Height, _buttonDoNotFight.bounds.Width, _buttonDoNotFight.bounds.Height);
        }

        public static float getBonus(MapObject participant, bool attacker)
        {
            int men = participant.state.getValue<int>("men");
            if (men <= 0)
                return 1.0f;

            int weapons = participant.state.getValue<int>("weapons");
            int horses = participant.state.getValue<int>("horses");
            Map.MapNode mapNode = IntoTheNewWorld.Instance._map.getMapNode(participant.positionWorld);
            MapTile mapTile = IntoTheNewWorld.Instance._map.getTile(mapNode);
            Terrain terrain = mapTile.terrain;

            float bonus = 1.0f;
            bonus += (2.0f * ((float)Math.Min(men, weapons) / (float)men));
            if (terrain.majorType == Terrain.TerrainMajorType.Plains)
                bonus += ((float)Math.Min(men, horses) / (float)men);

            // Terrain bonuses...
            if ((!attacker) && (terrain.majorType == Terrain.TerrainMajorType.Hills))
                bonus += 0.5f;
            else if ((!attacker) && (terrain.majorType == Terrain.TerrainMajorType.Mountains))
                bonus += 1.0f;

            return bonus;
        }

        public static int getOdds(MapObject attacker, MapObject defender)
        {
            int attackerLosses;
            int defenderLosses;
            getExpectedLosses(attacker, defender, false, out attackerLosses, out defenderLosses);

            int attackerMen = attacker.state.getValue<int>("men");
            return (int)((1.0f - ((float)attackerLosses / ((float)attackerMen))) * 100.0f);

#if false
            return (int)(((float)attackerLosses / ((float)attackerLosses + (float)defenderLosses)) * 100.0f);
#endif

#if false
            return (int)(((float)attackerBonus / ((float)attackerBonus + (float)defenderBonus)) * 100.0f);
#endif
        }

        private static void getExpectedLosses(MapObject attacker, MapObject defender, bool defenderFlees, out int attackerLosses, out int defenderLosses)
        {
            float attackerBonus = getBonus(attacker, true);
            float defenderBonus = getBonus(defender, false);

#if false
            attackerLosses = 0;
            defenderLosses = 0;

            int odds = getOdds(attacker, defender);

            int attackerMen = attacker.state.getValue<int>("men");
            int attackerMenOriginal = attackerMen;
            if (defenderFlees)
                attackerMen = (int)(attackerMen * ((float)odds / 1000.0f));
            else
                attackerMen = (int)(attackerMen * ((float)odds / 100.0f));
            if (attackerMen < 10)
                attackerMen = 0;
            attackerLosses = attackerMenOriginal - attackerMen;

            // TODO: Account for better fleeing with horses...

            int defenderMen = defender.state.getValue<int>("men");
            int defenderMenOriginal = defenderMen;
            if (defenderFlees)
                defenderMen = (int)(defenderMen * (1.0f - ((float)odds / 300.0f)));
            else
                defenderMen = (int)(defenderMen * (1.0f - ((float)odds / 100.0f)));
            if (defenderMen < 10)
                defenderMen = 0;
            defenderLosses = defenderMenOriginal - defenderMen;
#else
            // TODO: Account for better fleeing with horses, better pursuing with horses.

            int attackerMen = attacker.state.getValue<int>("men");
            int defenderMen = defender.state.getValue<int>("men");

            int attackerMenEffective = (int)(attackerBonus * attackerMen);
            int defenderMenEffective = (int)(defenderBonus * defenderMen);

            attackerLosses = defenderMenEffective / 4;
            defenderLosses = attackerMenEffective / 4;

            if (defenderFlees)
                attackerLosses /= 10;
            //if ((attackerMen - attackerLosses) < 10)
            //    attackerLosses = attackerMen;
            if (attackerLosses > attackerMen)
                attackerLosses = attackerMen;

            // TODO: Account for better fleeing with horses...

            if (defenderFlees)
                defenderLosses /= 3;
            //if ((defenderMen - defenderLosses) < 10)
            //    defenderLosses = defenderMen;
            if (defenderLosses > defenderMen)
                defenderLosses = defenderMen;
#endif
        }

        public static void resolveCombat(MapObject attacker, MapObject defender, bool defenderFlees, out string combatResultsMessage)
        {
            combatResultsMessage = "Combat results:\n";
            combatResultsMessage += "\n";

            int attackerLosses;
            int defenderLosses;
            getExpectedLosses(attacker, defender, defenderFlees, out attackerLosses, out defenderLosses);

            int attackerMen = attacker.state.getValue<int>("men");
            int defenderMen = defender.state.getValue<int>("men");

            bool defenderFleesFinal = defenderFlees;

            // Calculate whether or not the defender will flee, based on their anticpated losses.
            if (!defenderFlees)
            {
                int defenderLossFleeingThreshold;
                if (defender.state.getValue("defenderLossFleeingThreshold", out defenderLossFleeingThreshold))
                {
                    if (defenderLosses > (defenderMen * (float)((float)defenderLossFleeingThreshold / 100.0f)))
                    {
                        getExpectedLosses(attacker, defender, true, out attackerLosses, out defenderLosses);
                        defenderFleesFinal = true;
                    }
                }
            }

            attacker.state.setValue("men", attackerMen - attackerLosses);
            defender.state.setValue("men", defenderMen - defenderLosses);

            if ((attackerMen - attackerLosses) <= 0)
                combatResultsMessage += "The attacking force is completely defeated.\n";
            else
                combatResultsMessage += "The attacking force suffers " + ((attackerLosses == 0) ? "no" : "" + attackerLosses) + " casualt" + ((attackerLosses == 1) ? "y" : "ies") + ".\n";
            if ((defenderMen - defenderLosses) <= 0)
            {
                if (defenderFleesFinal)
                    combatResultsMessage += "Despite attempting to flee, the defending force is completely defeated.\n";
                else
                    combatResultsMessage += "The defending force is completely defeated.\n";
            }
            else
                combatResultsMessage += "The defending force" + (defenderFleesFinal ? ", fleeing," : "") + " suffers " + ((defenderLosses == 0) ? "no" : "" + defenderLosses) + " casualt" + ((defenderLosses == 1) ? "y" : "ies") + ".\n";
        }
    }
}
