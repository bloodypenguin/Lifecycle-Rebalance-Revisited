// <copyright file="TransportOptions.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using UnityEngine;
    using AlgernonCommons;
    using AlgernonCommons.UI;
    using AlgernonCommons.Translation;
    using ColossalFramework;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for setting transport options.
    /// </summary>
    internal class TransportOptions : OptionsPanelTab
    {
        // Enumerative constants.
        protected const int NumDensity = 2;
        protected const int NumWealth = 3;
        protected const int NumAges = 5;
        protected const int NumTransport = 3;

        // Layout constants.
        protected const float Margin = 10f;
        protected const float LeftTitle = 50f;
        protected const float LeftItem = 75f;
        protected const float Indent = 40f;
        protected const float TitleHeight = 70f;
        protected const float ColumnIconHeight = TitleHeight - 10f;
        protected const float FieldWidth = 40f;
        protected const float RowHeight = 23f;
        protected const float ColumnWidth = FieldWidth + Margin;
        protected const float Column1 = 280f;
        protected const float Column2 = Column1 + ColumnWidth;
        protected const float Column3 = Column2 + ColumnWidth;
        protected const float GroupWidth = (ColumnWidth * 3) + Margin;
        protected const float Group1 = Column1;
        protected const float Group2 = Group1 + GroupWidth;
        protected const float Group3 = Group2 + GroupWidth;


        // Textfield arrays.
        // Wealth/Density, age, transport mode.
        protected UITextField[][][] wealthLow, wealthMed, wealthHigh, ecoWealthLow, ecoWealthMed, ecoWealthHigh;

        // Reference variables.
        protected float currentY = TitleHeight;


        /// <summary>
        /// Adds death options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal TransportOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("LBR_TRN"), tabIndex, out _);

            // Set tab object reference.
            tabStrip.tabs[tabIndex].objectUserData = this;
        }


        /// <summary>
        /// Performs initial setup; called via event when tab is first selected.
        /// </summary>
        internal override void Setup()
        {
            // Don't do anything if already set up.
            if (!isSetup)
            {
                // Perform initial setup.
                isSetup = true;
                Logging.Message("setting up ", this.GetType());

                UICheckBox transportCheckBox = UICheckBoxes.AddPlainCheckBox(panel, 30f, 5f, Translations.Translate("LBR_TRN_CUS"));
                transportCheckBox.isChecked = ModSettings.Settings.UseTransportModes;
                transportCheckBox.eventCheckChanged += (c, isChecked) => { ModSettings.Settings.UseTransportModes = isChecked; };

                // Set up textfield arrays; low/high density.
                wealthLow = new UITextField[NumDensity][][];
                wealthMed = new UITextField[NumDensity][][];
                wealthHigh = new UITextField[NumDensity][][];
                ecoWealthLow = new UITextField[NumDensity][][];
                ecoWealthMed = new UITextField[NumDensity][][];
                ecoWealthHigh = new UITextField[NumDensity][][];

                // Second level of textfield arrays; age groups.
                for (int i = 0; i < NumDensity; ++i)
                {
                    wealthLow[i] = new UITextField[NumAges][];
                    wealthMed[i] = new UITextField[NumAges][];
                    wealthHigh[i] = new UITextField[NumAges][];
                    ecoWealthLow[i] = new UITextField[NumAges][];
                    ecoWealthMed[i] = new UITextField[NumAges][];
                    ecoWealthHigh[i] = new UITextField[NumAges][];

                    // Third level of textfield arrays; transport modes.
                    for (int j = 0; j < NumAges; ++j)
                    {
                        wealthLow[i][j] = new UITextField[NumTransport];
                        wealthMed[i][j] = new UITextField[NumTransport];
                        wealthHigh[i][j] = new UITextField[NumTransport];
                        ecoWealthLow[i][j] = new UITextField[NumTransport];
                        ecoWealthMed[i][j] = new UITextField[NumTransport];
                        ecoWealthHigh[i][j] = new UITextField[NumTransport];
                    }
                }

                // Headings.
                for (int i = 0; i < NumWealth; ++i)
                {
                    // Wealth headings.
                    float wealthX = (i * GroupWidth) + Column1;
                    WealthIcon(panel, wealthX, 25f, ColumnWidth * 3, i + 1, Translations.Translate("LBR_TRN_WEA"), "InfoIconLandValue");

                    // Transport mode headings.
                    ColumnIcon(panel, (i * GroupWidth) + Column1, ColumnIconHeight, FieldWidth, Translations.Translate("LBR_TRN_CAR"), "InfoIconTrafficCongestion");
                    ColumnIcon(panel, (i * GroupWidth) + Column2, ColumnIconHeight, FieldWidth, Translations.Translate("LBR_TRN_BIK"), "IconPolicyEncourageBiking");
                    ColumnIcon(panel, (i * GroupWidth) + Column3, ColumnIconHeight, FieldWidth, Translations.Translate("LBR_TRN_TAX"), "SubBarPublicTransportTaxi");
                }

                // Rows by group.
                RowHeaderIcon(panel, currentY, Translations.Translate("LBR_TRN_RLO"), "ZoningResidentialLow", "Thumbnails", true);
                AddDensityGroup(panel, wealthLow[0], wealthMed[0], wealthHigh[0]);
                RowHeaderIcon(panel, currentY, Translations.Translate("LBR_TRN_RHI"), "ZoningResidentialHigh", "Thumbnails");
                AddDensityGroup(panel, wealthLow[1], wealthMed[1], wealthHigh[1]);
                RowHeaderIcon(panel, currentY, Translations.Translate("LBR_TRN_ERL"), "IconPolicySelfsufficient", "Ingame");
                AddDensityGroup(panel, ecoWealthLow[0], ecoWealthMed[0], ecoWealthHigh[0]);
                RowHeaderIcon(panel, currentY, Translations.Translate("LBR_TRN_ERH"), "IconPolicySelfsufficient", "Ingame");
                AddDensityGroup(panel, ecoWealthLow[1], ecoWealthMed[1], ecoWealthHigh[1]);

                // Buttons.
                AddButtons(panel);

                // Populate text fields.
                PopulateFields();
            }
        }

        /// <summary>
        /// Event handler filter for text fields to ensure only integer values are entered.
        /// </summary>
        /// <param name="c">Relevant control</param>
        /// <param name="value">Text value</param>
        private void TextFilter(UITextField c, string value)
        {
            // If it's not blank and isn't an integer, remove the last character and set selection to end.
            if (!value.IsNullOrWhiteSpace() && !int.TryParse(value, out int _))
            {
                c.text = value.Substring(0, value.Length - 1);
                c.MoveSelectionPointRight();
            }
        }

        /// <summary>
        /// Updates the DataStore with information from the text fields.
        /// </summary>
        private void ApplyFields()
        {
            // Iterate through each density group.
            for (int i = 0; i < NumDensity; ++i)
            {
                // Iterate through each age group within this density group.
                for (int j = 0; j < NumAges; ++j)
                {
                    // Iterate through each transport mode within this age group.
                    for (int k = 0; k < NumTransport; ++k)
                    {
                        ParseInt(ref DataStore.wealth_low[i][j][k], wealthLow[i][j][k].text);
                        ParseInt(ref DataStore.wealth_med[i][j][k], wealthMed[i][j][k].text);
                        ParseInt(ref DataStore.wealth_high[i][j][k], wealthHigh[i][j][k].text);

                        ParseInt(ref DataStore.eco_wealth_low[i][j][k], ecoWealthLow[i][j][k].text);
                        ParseInt(ref DataStore.eco_wealth_med[i][j][k], ecoWealthMed[i][j][k].text);
                        ParseInt(ref DataStore.eco_wealth_high[i][j][k], ecoWealthHigh[i][j][k].text);
                    }
                }
            }

            // Save new settings.
            PanelUtils.SaveXML();

            // Refresh settings.
            PopulateFields();
        }

        /// <summary>
        /// Attempts to parse a string for an integer value; if the parse fails, simply does nothing (leaving the original value intact).
        /// </summary>
        /// <param name="intVar">Integer variable to store result (left unchanged if parse fails)</param>
        /// <param name="text">Text to parse</param>
        private void ParseInt(ref int intVar, string text)
        {
            if (int.TryParse(text, out int result))
            {
                // Bounds check.
                if (result < 0)
                {
                    result = 0;
                }
                else if (result > 100)
                {
                    result = 100;
                }

                intVar = result;
            }
        }

        /// <summary>
        /// Populates the text fields with information from the DataStore.
        /// </summary>
        private void PopulateFields()
        {
            // Iterate through each density group.
            for (int i = 0; i < NumDensity; ++i)
            {
                // Iterate through each age group within this density group.
                for (int j = 0; j < NumAges; ++j)
                {
                    // Iterate through each transport mode within this age group.
                    for (int k = 0; k < NumTransport; ++k)
                    {
                        wealthLow[i][j][k].text = DataStore.wealth_low[i][j][k].ToString();
                        wealthMed[i][j][k].text = DataStore.wealth_med[i][j][k].ToString();
                        wealthHigh[i][j][k].text = DataStore.wealth_high[i][j][k].ToString();

                        ecoWealthLow[i][j][k].text = DataStore.eco_wealth_low[i][j][k].ToString();
                        ecoWealthMed[i][j][k].text = DataStore.eco_wealth_med[i][j][k].ToString();
                        ecoWealthHigh[i][j][k].text = DataStore.eco_wealth_high[i][j][k].ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Adds a density field group to the panel.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        /// <param name="lowWealth">Low-wealth textfield array</param>
        /// <param name="medWealth">Medium-wealth textfield array</param>
        /// <param name="highWealth">High-wealth textfield array</param>
        private void AddDensityGroup(UIPanel panel, UITextField[][] lowWealth, UITextField[][] medWealth, UITextField[][] highWealth)
        {
            string[] ageLabels = new string[] { Translations.Translate("LBR_TRN_CHL"), Translations.Translate("LBR_TRN_TEN"), Translations.Translate("LBR_TRN_YAD"), Translations.Translate("LBR_TRN_ADL"), Translations.Translate("LBR_TRN_SEN") };

            // Add a row for each age group within this density group.
            for (int i = 0; i < NumAges; ++i)
            {
                // Row label.
                RowLabel(panel, currentY, ageLabels[i]);

                // Add textfields for each transport mode within this age group.
                for (int j = 0; j < NumTransport; ++j)
                {
                    lowWealth[i][j] = AddTextField(panel, FieldWidth, (j * ColumnWidth) + Group1, currentY);
                    medWealth[i][j] = AddTextField(panel, FieldWidth, (j * ColumnWidth) + Group2, currentY);
                    highWealth[i][j] = AddTextField(panel, FieldWidth, (j * ColumnWidth) + Group3, currentY);
                }

                // Increment Y position.
                currentY += RowHeight;
            }

            // Add an extra bit of space at the end.
            currentY += 5f;
        }

        /// <summary>
        /// Adds a column header icon label.
        /// </summary>
        /// <param name="panel">UI panel</param>
        /// <param name="xPos">Reference X position</param>
        /// <param name="xPos">Reference Y position</param>
        /// <param name="width">Width of reference item (for centering)</param>
        /// <param name="text">Tooltip text</param>
        /// <param name="icon">Icon name</param>
        private void WealthIcon(UIPanel panel, float xPos, float yPos, float width, int count, string text, string icon)
        {
            // Constants for positioning of icons.
            const float iconSize = 35f;
            const float iconOffset = 20f;
            const float iconRemainder = iconSize - iconOffset;

            // Create mini-panel for the icon background.
            UIPanel thumbPanel = panel.AddUIComponent<UIPanel>();
            thumbPanel.width = (count * iconOffset) + iconRemainder;
            thumbPanel.height = iconSize;
            thumbPanel.relativePosition = new Vector3(xPos + ((width - thumbPanel.width) / 2), yPos);
            thumbPanel.clipChildren = true;
            thumbPanel.tooltip = text;

            // Actual icon(s).
            for (int i = 0; i < count; ++i)
            {
                UISprite thumbSprite = thumbPanel.AddUIComponent<UISprite>();
                thumbSprite.relativePosition = new Vector3(i * iconOffset, 0);
                thumbSprite.size = new Vector3(iconSize, iconSize);
                thumbSprite.atlas = UITextures.InGameAtlas;
                thumbSprite.spriteName = icon;
            }
        }

        /// <summary>
        /// Adds control bouttons the the panel.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        private void AddButtons(UIPanel panel)
        {
            // Add a little bit of space.
            currentY += Margin;

            // Reset button.
            UIButton resetButton = UIButtons.AddButton(panel, Margin, currentY, Translations.Translate("LBR_RTD"));
            resetButton.eventClicked += (component, clickEvent) => ResetToDefaults();

            UIButton revertToSaveButton = UIButtons.AddButton(panel, UILayout.PositionRightOf(resetButton), Translations.Translate("LBR_RTS"));
            revertToSaveButton.eventClicked += (component, clickEvent) => { PopulateFields(); };

            UIButton saveButton = UIButtons.AddButton(panel, UILayout.PositionRightOf(revertToSaveButton), Translations.Translate("LBR_SAA"));
            saveButton.eventClicked += (component, clickEvent) => ApplyFields();
        }

        /// <summary>
        /// Adds a column header icon label.
        /// </summary>
        /// <param name="panel">UI panel</param>
        /// <param name="xPos">Reference X position</param>
        /// <param name="yPos">Reference Y position</param>
        /// <param name="width">Width of reference item (for centering)</param>
        /// <param name="text">Tooltip text</param>
        /// <param name="icon">Icon name</param>
        private void ColumnIcon(UIPanel panel, float xPos, float yPos, float width, string text, string icon)
        {
            // Create mini-panel for the icon background.
            UIPanel thumbPanel = panel.AddUIComponent<UIPanel>();
            thumbPanel.width = 35f;
            thumbPanel.height = 35f;
            thumbPanel.relativePosition = new Vector3(xPos + ((width - 35f) / 2), yPos);
            thumbPanel.clipChildren = true;
            thumbPanel.backgroundSprite = "IconPolicyBaseRect";
            thumbPanel.tooltip = text;

            // Actual icon.
            UISprite thumbSprite = thumbPanel.AddUIComponent<UISprite>();
            thumbSprite.relativePosition = Vector3.zero;
            thumbSprite.size = thumbPanel.size;
            thumbSprite.atlas = UITextures.InGameAtlas;
            thumbSprite.spriteName = icon;
        }

        /// <summary>
        /// Adds a column header icon label.
        /// </summary>
        /// <param name="panel">UI panel</param>
        /// <param name="xPos">Reference X position</param>
        /// <param name="text">Tooltip text</param>
        /// <param name="icon">Icon name</param>
        /// <param name="atlas">Icon atlas name</param>
        /// <param name="wrapText">True if label width is fixed to column width and text wrapped, false otherwise</param>
        private void RowHeaderIcon(UIPanel panel, float yPos, string text, string icon, string atlas, bool wrapText = false)
        {
            const float SpriteSize = 35f;

            // Actual icon.
            UISprite thumbSprite = panel.AddUIComponent<UISprite>();
            thumbSprite.relativePosition = new Vector3(Margin, yPos - 2.5f);
            thumbSprite.width = SpriteSize;
            thumbSprite.height = SpriteSize;
            thumbSprite.atlas = UITextures.GetTextureAtlas(atlas);
            thumbSprite.spriteName = icon;

            // Text label.
            UILabel lineLabel = panel.AddUIComponent<UILabel>();
            lineLabel.textScale = 1.0f;
            lineLabel.width = Column1 - LeftTitle - Margin;
            lineLabel.autoHeight = true;
            lineLabel.wordWrap = wrapText;
            lineLabel.autoSize = !wrapText;
            lineLabel.verticalAlignment = UIVerticalAlignment.Middle;
            lineLabel.text = text;
            lineLabel.relativePosition = new Vector3(LeftTitle, yPos - 2.5f + ((SpriteSize - lineLabel.height) /2f));

            // Increment Y.
            currentY += 30f;
        }

        /// <summary>
        /// Adds a row text label.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        /// <param name="yPos">Reference Y position</param>
        /// <param name="text">Label text</param>
        private void RowLabel(UIPanel panel, float yPos, string text)
        {
            // Text label.
            UILabel lineLabel = panel.AddUIComponent<UILabel>();
            lineLabel.textScale = 0.9f;
            lineLabel.verticalAlignment = UIVerticalAlignment.Middle;
            lineLabel.text = text;

            // X position: by default it's LeftItem, but we move it further left if the label is too long to fit (e.g. long translation strings).
            float xPos = Mathf.Min(LeftItem, (Column1 - Margin) - lineLabel.width);
            // But never further left than the edge of the screen.
            if (xPos < 0)
            {
                xPos = LeftItem;
                // Too long to fit in the given space, so we'll let this wrap across and just move the textfields down an extra line.
                currentY += RowHeight;
            }
            lineLabel.relativePosition = new Vector3(xPos, yPos + 2);
        }

        /// <summary>
        /// Adds an input text field at the specified coordinates.
        /// </summary>
        /// <param name="textField">Textfield object</param>
        /// <param name="panel">panel to add to</param>
        /// <param name="posX">Relative X postion</param>
        /// <param name="posY">Relative Y position</param>
        /// <param name="tooltip">Tooltip, if any</param>
        private UITextField AddTextField(UIPanel panel, float width, float posX, float posY, string tooltip = null)
        {
            UITextField textField = UITextFields.AddSmallTextField(panel, posX, posY, width);
            textField.eventTextChanged += (c, value) => TextFilter((UITextField)c, value);

            // Add tooltip.
            if (tooltip != null)
            {
                textField.tooltip = tooltip;
            }

            return textField;
        }

        /// <summary>
        /// Resets all textfields to mod default values.
        /// </summary>
        private void ResetToDefaults()
        {
            DataStore.wealth_low = new int[][][] { new int[][] { new int [] { 0, 40, 0},
                                                             new int [] {10, 30, 0},
                                                             new int [] {45, 20, 1},
                                                             new int [] {60, 10, 2},
                                                             new int [] {30,  2, 3} },

                                               new int[][] { new int [] {0, 40, 0},
                                                             new int [] {2, 30, 0},
                                                             new int [] {3, 20, 1},
                                                             new int [] {5, 10, 2},
                                                             new int [] {4,  2, 3} }};

            DataStore.wealth_med = new int[][][] { new int[][] { new int [] { 0, 40, 0},
                                                             new int [] {12, 30, 1},
                                                             new int [] {50, 20, 2},
                                                             new int [] {65, 10, 4},
                                                             new int [] {35,  2, 6} },

                                               new int[][] { new int [] {0, 40, 0},
                                                             new int [] {3, 30, 1},
                                                             new int [] {5, 20, 2},
                                                             new int [] {7, 10, 3},
                                                             new int [] {6,  2, 5} }};

            DataStore.wealth_high = new int[][][] { new int[][] { new int [] { 0, 40, 0},
                                                              new int [] {15, 30, 2},
                                                              new int [] {55, 20, 3},
                                                              new int [] {70, 10, 4},
                                                              new int [] {45,  2, 6} },

                                                new int[][] { new int [] { 0, 40, 0},
                                                              new int [] { 4, 30, 2},
                                                              new int [] { 7, 20, 3},
                                                              new int [] { 9, 10, 4},
                                                              new int [] { 8,  1, 5} }};

            DataStore.eco_wealth_low = new int[][][] { new int[][] { new int [] { 0, 40, 0},
                                                                 new int [] { 7, 30, 0},
                                                                 new int [] {25, 20, 1},
                                                                 new int [] {40, 10, 2},
                                                                 new int [] {20,  5, 3} },

                                                   new int[][] { new int [] {0, 40, 0},
                                                                 new int [] {1, 30, 0},
                                                                 new int [] {2, 20, 1},
                                                                 new int [] {4, 10, 2},
                                                                 new int [] {3,  2, 3} }};

            DataStore.eco_wealth_med = new int[][][] { new int[][] { new int [] { 0, 40, 0},
                                                                 new int [] { 8, 30, 1},
                                                                 new int [] {33, 20, 2},
                                                                 new int [] {43, 10, 4},
                                                                 new int [] {23,  2, 6} },

                                                   new int[][] { new int [] {0, 40, 0},
                                                                 new int [] {2, 30, 1},
                                                                 new int [] {4, 20, 2},
                                                                 new int [] {5, 10, 3},
                                                                 new int [] {4,  2, 5} }};

            DataStore.eco_wealth_high = new int[][][] { new int[][] { new int [] { 0, 40, 0},
                                                                  new int [] {10, 30, 2},
                                                                  new int [] {37, 20, 3},
                                                                  new int [] {46, 10, 4},
                                                                  new int [] {30,  2, 6} },

                                                    new int[][] { new int [] { 0, 40, 0},
                                                                  new int [] { 3, 30, 2},
                                                                  new int [] { 4, 20, 3},
                                                                  new int [] { 6, 10, 4},
                                                                  new int [] { 5,  1, 5} }};

            // Populate text fields with these.
            PopulateFields();
        }
    }
}