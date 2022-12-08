// <copyright file="TransportOptions.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Options panel for setting transport options.
    /// </summary>
    internal sealed class TransportOptions : OptionsPanelTab
    {
        // Enumerative constants.
        private const int NumDensity = 2;
        private const int NumWealth = 3;
        private const int NumAges = 5;
        private const int NumTransport = 3;

        // Layout constants.
        private const float Margin = 10f;
        private const float LeftTitle = 50f;
        private const float LeftItem = 75f;
        private const float TitleHeight = 70f;
        private const float ColumnIconHeight = TitleHeight - 10f;
        private const float FieldWidth = 40f;
        private const float RowHeight = 23f;
        private const float ColumnWidth = FieldWidth + Margin;
        private const float Column1 = 275f;
        private const float Column2 = Column1 + ColumnWidth;
        private const float Column3 = Column2 + ColumnWidth;
        private const float GroupWidth = (ColumnWidth * 3) + Margin;
        private const float Group1 = Column1;
        private const float Group2 = Group1 + GroupWidth;
        private const float Group3 = Group2 + GroupWidth;

        // Textfield arrays.
        // Wealth/Density, age, transport mode.
        private UITextField[][][] _wealthLow;
        private UITextField[][][] _wealthMed;
        private UITextField[][][] _wealthHigh;
        private UITextField[][] _w2wWealthLow;
        private UITextField[][] _w2wWealthMed;
        private UITextField[][] _w2wWealthHigh;
        private UITextField[][][] _ecoWealthLow;
        private UITextField[][][] _ecoWealthMed;
        private UITextField[][][] _ecoWealthHigh;

        // Reference variables.
        private float _currentY = TitleHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransportOptions"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal TransportOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            Panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("LBR_TRN"), tabIndex, out _);

            // Set tab object reference.
            tabStrip.tabs[tabIndex].objectUserData = this;
        }

        /// <summary>
        /// Performs initial setup; called via event when tab is first selected.
        /// </summary>
        internal override void Setup()
        {
            // Don't do anything if already set up.
            if (!IsSetup)
            {
                // Perform initial setup.
                IsSetup = true;
                Logging.Message("setting up ", this.GetType());

                UICheckBox transportCheckBox = UICheckBoxes.AddPlainCheckBox(Panel, 30f, 5f, Translations.Translate("LBR_TRN_CUS"));
                transportCheckBox.isChecked = ModSettings.Settings.UseTransportModes;
                transportCheckBox.eventCheckChanged += (c, isChecked) => { ModSettings.Settings.UseTransportModes = isChecked; };

                // Set up textfield arrays; low/high density.
                _wealthLow = new UITextField[NumDensity][][];
                _wealthMed = new UITextField[NumDensity][][];
                _wealthHigh = new UITextField[NumDensity][][];
                _ecoWealthLow = new UITextField[NumDensity][][];
                _ecoWealthMed = new UITextField[NumDensity][][];
                _ecoWealthHigh = new UITextField[NumDensity][][];

                // Second level of textfield arrays; age groups.
                for (int i = 0; i < NumDensity; ++i)
                {
                    _wealthLow[i] = new UITextField[NumAges][];
                    _wealthMed[i] = new UITextField[NumAges][];
                    _wealthHigh[i] = new UITextField[NumAges][];
                    _ecoWealthLow[i] = new UITextField[NumAges][];
                    _ecoWealthMed[i] = new UITextField[NumAges][];
                    _ecoWealthHigh[i] = new UITextField[NumAges][];

                    // Wall-to-wall doesn't have multiple densities.
                    if (i == 0)
                    {
                        _w2wWealthLow = new UITextField[NumAges][];
                        _w2wWealthMed = new UITextField[NumAges][];
                        _w2wWealthHigh = new UITextField[NumAges][];
                    }

                    // Third level of textfield arrays; transport modes.
                    for (int j = 0; j < NumAges; ++j)
                    {
                        _wealthLow[i][j] = new UITextField[NumTransport];
                        _wealthMed[i][j] = new UITextField[NumTransport];
                        _wealthHigh[i][j] = new UITextField[NumTransport];
                        _ecoWealthLow[i][j] = new UITextField[NumTransport];
                        _ecoWealthMed[i][j] = new UITextField[NumTransport];
                        _ecoWealthHigh[i][j] = new UITextField[NumTransport];

                        // Wall-to-wall doesn't have multiple densities.
                        if (i == 0)
                        {
                            _w2wWealthLow[j] = new UITextField[NumTransport];
                            _w2wWealthMed[j] = new UITextField[NumTransport];
                            _w2wWealthHigh[j] = new UITextField[NumTransport];
                        }
                    }
                }

                // Headings.
                for (int i = 0; i < NumWealth; ++i)
                {
                    // Align with textfields.
                    float xPos = (i * GroupWidth) - 6f;

                    // Wealth headings.
                    float wealthX = xPos + Column1 - 7f;
                    WealthIcon(Panel, wealthX, 25f, ColumnWidth * 3, i + 1, Translations.Translate("LBR_TRN_WEA"), "InfoIconLandValue");

                    // Transport mode headings.
                    ColumnIcon(Panel, xPos + Column1, ColumnIconHeight, Translations.Translate("LBR_TRN_CAR"), "InfoIconTrafficCongestion");
                    ColumnIcon(Panel, xPos + Column2, ColumnIconHeight, Translations.Translate("LBR_TRN_BIK"), "IconPolicyEncourageBiking");
                    ColumnIcon(Panel, xPos + Column3, ColumnIconHeight, Translations.Translate("LBR_TRN_TAX"), "SubBarPublicTransportTaxi");
                }

                _currentY += 30f;

                // Add scrollable panel for textfield groups.
                UIScrollablePanel scrollPanel = Panel.AddUIComponent<UIScrollablePanel>();
                scrollPanel.relativePosition = new Vector2(0f, _currentY);
                scrollPanel.autoSize = false;
                scrollPanel.autoLayout = false;
                scrollPanel.width = Panel.width - 10f;
                scrollPanel.height = Panel.height - _currentY - 40f;
                scrollPanel.clipChildren = true;
                scrollPanel.builtinKeyNavigation = true;
                scrollPanel.scrollWheelDirection = UIOrientation.Vertical;
                UIScrollbars.AddScrollbar(Panel, scrollPanel);

                // Reset Y-position indicator for scroll panel.
                float oldCurrentY = _currentY;
                _currentY = Margin;

                // Rows by group.
                RowHeaderIcon(scrollPanel, _currentY, Translations.Translate("LBR_TRN_RLO"), "ZoningResidentialLow", "Thumbnails", true);
                AddDensityGroup(scrollPanel, _wealthLow[0], _wealthMed[0], _wealthHigh[0]);
                RowHeaderIcon(scrollPanel, _currentY, Translations.Translate("LBR_TRN_RHI"), "ZoningResidentialHigh", "Thumbnails");
                AddDensityGroup(scrollPanel, _wealthLow[1], _wealthMed[1], _wealthHigh[1]);
                RowHeaderIcon(scrollPanel, _currentY, Translations.Translate("LBR_TRN_RW2"), "DistrictSpecializationResidentialWallToWall", "Thumbnails");
                AddDensityGroup(scrollPanel, _w2wWealthLow, _w2wWealthMed, _w2wWealthHigh);
                RowHeaderIcon(scrollPanel, _currentY, Translations.Translate("LBR_TRN_ERL"), "IconPolicySelfsufficient", "Ingame");
                AddDensityGroup(scrollPanel, _ecoWealthLow[0], _ecoWealthMed[0], _ecoWealthHigh[0]);
                RowHeaderIcon(scrollPanel, _currentY, Translations.Translate("LBR_TRN_ERH"), "IconPolicySelfsufficient", "Ingame");
                AddDensityGroup(scrollPanel, _ecoWealthLow[1], _ecoWealthMed[1], _ecoWealthHigh[1]);

                // Restore Y-position indicator.
                _currentY = oldCurrentY + scrollPanel.height;

                // Buttons.
                AddButtons(Panel);

                // Populate text fields.
                PopulateFields();
            }
        }

        /// <summary>
        /// Event handler filter for text fields to ensure only integer values are entered.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="value">Text value.</param>
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
                        ParseInt(ref DataStore.TransportLowWealth[i][j][k], _wealthLow[i][j][k].text);
                        ParseInt(ref DataStore.TransportMedWealth[i][j][k], _wealthMed[i][j][k].text);
                        ParseInt(ref DataStore.TransportHighWealth[i][j][k], _wealthHigh[i][j][k].text);

                        ParseInt(ref DataStore.TranportLowWealthEco[i][j][k], _ecoWealthLow[i][j][k].text);
                        ParseInt(ref DataStore.TransportMedWealthEco[i][j][k], _ecoWealthMed[i][j][k].text);
                        ParseInt(ref DataStore.TransportHighWealthEco[i][j][k], _ecoWealthHigh[i][j][k].text);

                        // Wall-to-wall has no density category.
                        if (i == 0)
                        {
                            ParseInt(ref DataStore.TransportLowWealthW2W[j][k], _w2wWealthLow[j][k].text);
                            ParseInt(ref DataStore.TransportMedWealthW2W[j][k], _w2wWealthMed[j][k].text);
                            ParseInt(ref DataStore.TransportHighWealthW2W[j][k], _w2wWealthHigh[j][k].text);
                        }
                    }
                }
            }

            // Save new settings.
            DataStore.SaveXML();

            // Refresh settings.
            PopulateFields();
        }

        /// <summary>
        /// Attempts to parse a string for an integer value; if the parse fails, simply does nothing (leaving the original value intact).
        /// </summary>
        /// <param name="intVar">Integer variable to store result (left unchanged if parse fails).</param>
        /// <param name="text">Text to parse.</param>
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
                        _wealthLow[i][j][k].text = DataStore.TransportLowWealth[i][j][k].ToString();
                        _wealthMed[i][j][k].text = DataStore.TransportMedWealth[i][j][k].ToString();
                        _wealthHigh[i][j][k].text = DataStore.TransportHighWealth[i][j][k].ToString();

                        _ecoWealthLow[i][j][k].text = DataStore.TranportLowWealthEco[i][j][k].ToString();
                        _ecoWealthMed[i][j][k].text = DataStore.TransportMedWealthEco[i][j][k].ToString();
                        _ecoWealthHigh[i][j][k].text = DataStore.TransportHighWealthEco[i][j][k].ToString();

                        // Wall-to-wall has no density category.
                        if (i == 0)
                        {
                            _w2wWealthLow[j][k].text = DataStore.TransportLowWealthW2W[j][k].ToString();
                            _w2wWealthMed[j][k].text = DataStore.TransportMedWealthW2W[j][k].ToString();
                            _w2wWealthHigh[j][k].text = DataStore.TransportHighWealthW2W[j][k].ToString();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a density field group to the panel.
        /// </summary>
        /// <param name="panel">UI panel instance.</param>
        /// <param name="lowWealth">Low-wealth textfield array.</param>
        /// <param name="medWealth">Medium-wealth textfield array.</param>
        /// <param name="highWealth">High-wealth textfield array.</param>
        private void AddDensityGroup(UIComponent panel, UITextField[][] lowWealth, UITextField[][] medWealth, UITextField[][] highWealth)
        {
            string[] ageLabels = new string[] { Translations.Translate("LBR_TRN_CHL"), Translations.Translate("LBR_TRN_TEN"), Translations.Translate("LBR_TRN_YAD"), Translations.Translate("LBR_TRN_ADL"), Translations.Translate("LBR_TRN_SEN") };

            // Add a row for each age group within this density group.
            for (int i = 0; i < NumAges; ++i)
            {
                // Row label.
                RowLabel(panel, _currentY, ageLabels[i]);

                // Add textfields for each transport mode within this age group.
                for (int j = 0; j < NumTransport; ++j)
                {
                    lowWealth[i][j] = AddTextField(panel, FieldWidth, (j * ColumnWidth) + Group1, _currentY);
                    medWealth[i][j] = AddTextField(panel, FieldWidth, (j * ColumnWidth) + Group2, _currentY);
                    highWealth[i][j] = AddTextField(panel, FieldWidth, (j * ColumnWidth) + Group3, _currentY);
                }

                // Increment Y position.
                _currentY += RowHeight;
            }

            // Add an extra bit of space at the end.
            _currentY += 5f;
        }

        /// <summary>
        /// Adds a column header icon label.
        /// </summary>
        /// <param name="panel">UI panel.</param>
        /// <param name="xPos">Reference X position.</param>
        /// <param name="yPos">Reference Y position.</param>
        /// <param name="width">Width of reference item (for centering).</param>
        /// <param name="count">Number of icons to add.</param>
        /// <param name="text">Tooltip text.</param>
        /// <param name="icon">Icon name.</param>
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
            thumbPanel.relativePosition = new Vector2(xPos + ((width - thumbPanel.width) / 2f), yPos);
            thumbPanel.clipChildren = true;
            thumbPanel.tooltip = text;

            // Actual icon(s).
            for (int i = 0; i < count; ++i)
            {
                UISprite thumbSprite = thumbPanel.AddUIComponent<UISprite>();
                thumbSprite.relativePosition = new Vector2(i * iconOffset, 0);
                thumbSprite.size = new Vector2(iconSize, iconSize);
                thumbSprite.atlas = UITextures.InGameAtlas;
                thumbSprite.spriteName = icon;
            }
        }

        /// <summary>
        /// Adds control bouttons the the panel.
        /// </summary>
        /// <param name="panel">UI panel instance.</param>
        private void AddButtons(UIPanel panel)
        {
            // Add a little bit of space.
            _currentY += Margin;

            // Reset button.
            UIButton resetButton = UIButtons.AddButton(panel, Margin, _currentY, Translations.Translate("LBR_RTD"));
            resetButton.eventClicked += (c, clickEvent) => ResetToDefaults();

            UIButton revertToSaveButton = UIButtons.AddButton(panel, UILayout.PositionRightOf(resetButton), Translations.Translate("LBR_RTS"));
            revertToSaveButton.eventClicked += (c, p) => { PopulateFields(); };

            UIButton saveButton = UIButtons.AddButton(panel, UILayout.PositionRightOf(revertToSaveButton), Translations.Translate("LBR_SAA"));
            saveButton.eventClicked += (c, p) => ApplyFields();
        }

        /// <summary>
        /// Adds a column header icon label.
        /// </summary>
        /// <param name="panel">UI panel.</param>
        /// <param name="xPos">Reference X position.</param>
        /// <param name="yPos">Reference Y position.</param>
        /// <param name="text">Tooltip text.</param>
        /// <param name="icon">Icon name.</param>
        private void ColumnIcon(UIPanel panel, float xPos, float yPos, string text, string icon)
        {
            // Create mini-panel for the icon background.
            UIPanel thumbPanel = panel.AddUIComponent<UIPanel>();
            thumbPanel.width = 35f;
            thumbPanel.height = 35f;
            thumbPanel.relativePosition = new Vector2(xPos, yPos);
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
        /// <param name="panel">UI panel.</param>
        /// <param name="yPos">Reference Y position.</param>
        /// <param name="text">Tooltip text.</param>
        /// <param name="icon">Icon name.</param>
        /// <param name="atlas">Icon atlas name.</param>
        /// <param name="wrapText">True if label width is fixed to column width and text wrapped, false otherwise.</param>
        private void RowHeaderIcon(UIComponent panel, float yPos, string text, string icon, string atlas, bool wrapText = false)
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
            lineLabel.relativePosition = new Vector3(LeftTitle, yPos - 2.5f + ((SpriteSize - lineLabel.height) / 2f));

            // Increment Y.
            _currentY += 30f;
        }

        /// <summary>
        /// Adds a row text label.
        /// </summary>
        /// <param name="panel">UI panel instance.</param>
        /// <param name="yPos">Reference Y position.</param>
        /// <param name="text">Label text.</param>
        private void RowLabel(UIComponent panel, float yPos, string text)
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
                _currentY += RowHeight;
            }

            lineLabel.relativePosition = new Vector3(xPos, yPos + 2);
        }

        /// <summary>
        /// Adds an input text field at the specified coordinates.
        /// </summary>
        /// <param name="panel">panel to add to.</param>
        /// <param name="width">Textfield width.</param>
        /// <param name="posX">Relative X postion.</param>
        /// <param name="posY">Relative Y position.</param>
        /// <param name="tooltip">Tooltip, if any.</param>
        private UITextField AddTextField(UIComponent panel, float width, float posX, float posY, string tooltip = null)
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
            DataStore.TransportLowWealth = new int[][][]
            {
                new int[][]
                {
                    new int[] { 0, 40, 0 },
                    new int[] { 10, 30, 0, },
                    new int[] { 45, 20, 1, },
                    new int[] { 60, 10, 2, },
                    new int[] { 30,  2, 3, },
                },
                new int[][]
                {
                    new int[] { 0, 40, 0, },
                    new int[] { 2, 30, 0, },
                    new int[] { 3, 20, 1, },
                    new int[] { 5, 10, 2, },
                    new int[] { 4,  2, 3, },
                },
            };

            DataStore.TransportMedWealth = new int[][][]
            {
                new int[][]
                {
                    new int[] { 0, 40, 0, },
                    new int[] { 12, 30, 1, },
                    new int[] { 50, 20, 2, },
                    new int[] { 65, 10, 4, },
                    new int[] { 35,  2, 6, },
                },
                new int[][]
                {
                    new int[] { 0, 40, 0, },
                    new int[] { 3, 30, 1, },
                    new int[] { 5, 20, 2, },
                    new int[] { 7, 10, 3, },
                    new int[] { 6,  2, 5, },
                },
            };

            DataStore.TransportHighWealth = new int[][][]
            {
                new int[][]
                {
                    new int[] { 0, 40, 0, },
                    new int[] { 15, 30, 2, },
                    new int[] { 55, 20, 3, },
                    new int[] { 70, 10, 4, },
                    new int[] { 45,  2, 6, },
                },
                new int[][]
                {
                    new int[] { 0, 40, 0, },
                    new int[] { 4, 30, 2, },
                    new int[] { 7, 20, 3, },
                    new int[] { 9, 10, 4, },
                    new int[] { 8,  1, 5, },
                },
            };

            DataStore.TransportLowWealthW2W = new int[][]
            {
                new int[] { 0, 40, 0, },
                new int[] { 2, 30, 0, },
                new int[] { 3, 20, 1, },
                new int[] { 5, 10, 2, },
                new int[] { 4,  2, 3, },
            };

            DataStore.TransportMedWealthW2W = new int[][]
            {
                new int[] { 0, 40, 0, },
                new int[] { 3, 30, 1, },
                new int[] { 5, 20, 2, },
                new int[] { 7, 10, 3, },
                new int[] { 6,  2, 5, },
            };

            DataStore.TransportHighWealthW2W = new int[][]
            {
                new int[] { 0, 40, 0, },
                new int[] { 4, 30, 2, },
                new int[] { 7, 20, 3, },
                new int[] { 9, 10, 4, },
                new int[] { 8,  1, 5, },
            };

            DataStore.TranportLowWealthEco = new int[][][]
            {
                new int[][]
                {
                    new int[] { 0, 40, 0 },
                    new int[] { 7, 30, 0, },
                    new int[] { 25, 20, 1, },
                    new int[] { 40, 10, 2, },
                    new int[] { 20,  5, 3, },
                },
                new int[][]
                {
                    new int[] { 0, 40, 0, },
                    new int[] { 1, 30, 0, },
                    new int[] { 2, 20, 1, },
                    new int[] { 4, 10, 2, },
                    new int[] { 3,  2, 3, },
                },
            };

            DataStore.TransportMedWealthEco = new int[][][]
            {
                new int[][]
                {
                    new int[] { 0, 40, 0 },
                    new int[] { 8, 30, 1, },
                    new int[] { 33, 20, 2, },
                    new int[] { 43, 10, 4, },
                    new int[] { 23,  2, 6, },
                },
                new int[][]
                {
                    new int[] { 0, 40, 0, },
                    new int[] { 2, 30, 1, },
                    new int[] { 4, 20, 2, },
                    new int[] { 5, 10, 3, },
                    new int[] { 4,  2, 5, },
                },
            };

            DataStore.TransportHighWealthEco = new int[][][]
            {
                new int[][]
                {
                    new int[] { 0, 40, 0, },
                    new int[] { 10, 30, 2, },
                    new int[] { 37, 20, 3, },
                    new int[] { 46, 10, 4, },
                    new int[] { 30,  2, 6, },
                },
                new int[][]
                {
                    new int[] { 0, 40, 0, },
                    new int[] { 3, 30, 2, },
                    new int[] { 4, 20, 3, },
                    new int[] { 6, 10, 4, },
                    new int[] { 5,  1, 5, },
                },
            };

            // Populate text fields with these.
            PopulateFields();
        }
    }
}