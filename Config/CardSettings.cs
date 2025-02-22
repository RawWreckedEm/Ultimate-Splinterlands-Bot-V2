﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Ultimate_Splinterlands_Bot_V2.Model;
using Ultimate_Splinterlands_Bot_V2.Utils;

namespace Ultimate_Splinterlands_Bot_V2.Config
{
    public record CardSettings
    {
        [JsonIgnore]
        public bool USE_CARD_SETTINGS { get; init; } = false;
        public int WINRATE_MODIFIER_OWNED_CARD_PERCENTAGE { get; init; } = 24;
        public double FLAT_NEGATIVE_MODIFIER_PER_UNOWNED_CARD { get; init; } = 2.5;
        public int USE_FOCUS_ELEMENT_WINRATE_THRESHOLD { get; init; } = 60;
        public int PREFERRED_SUMMONER_ELEMENTS_WINRATE_THRESHOLD { get; set; } = 48;
        public string[] PREFERRED_SUMMONER_ELEMENTS { get; set; } = new string[] { "dragon", "death", "fire", "earth", "water", "life" };
        public int CARD_MIN_LEVEL { set { MONSTER_MIN_LEVEL = value; SUMMONER_MIN_LEVEL = value; } } // legacy
        public int MONSTER_MIN_LEVEL { get; set; } = 1;
        public int SUMMONER_MIN_LEVEL { get; set; } = 1;
        public bool ADD_ZERO_MANA_CARDS { get; init; } = true;
        public bool PLAY_STARTER_CARDS { get; init; } = true;
        public bool DISABLE_OWNED_CARDS_PREFERENCE_BEFORE_CHEST_LEAGUE_RATING { get; init; } = true;
        public bool DISABLE_FOCUS_PRIORITY_BEFORE_CHEST_LEAGUE_RATING { get; init; } = true;

        public CardSettings()
        {

        }
        public CardSettings(string config)
        {
            foreach (var setting in config.Split(Environment.NewLine))
            {
                string[] parts = setting.Split('=');
                if (parts.Length != 2 || setting[0] == '#' || setting.Length < 3)
                {
                    continue;
                }

                string settingsKey = parts[0];

                string value = parts[1];

                var property = this.GetType().GetProperty(settingsKey, BindingFlags.Public | BindingFlags.Instance);

                if (property == null)
                {
                    Console.WriteLine($"could not find setting for: '{parts[0]}'");
                    Console.WriteLine($"Please read the newest patch notes to update your card_settings.txt!");
                    return;
                }

                if (property.PropertyType == typeof(bool))
                {
                    property.SetValue(this, Boolean.Parse(value));
                }
                else if (property.PropertyType == typeof(string))
                {
                    property.SetValue(this, value);
                }
                else if (property.PropertyType == typeof(Int32))
                {
                    property.SetValue(this, Convert.ToInt32(value));
                }
                else if (property.PropertyType == typeof(double))
                {
                    property.SetValue(this, Convert.ToDouble(value, CultureInfo.InvariantCulture));
                }
                else if (property.PropertyType == typeof(string[]))
                {
                    property.SetValue(this, value.Split(','));
                }
                else
                {
                    Console.WriteLine("Field Value: '{0}'", property.GetValue(this));
                    Console.WriteLine("Field Type: {0}", property.PropertyType);
                    Log.WriteToLog($"UNKOWN type '{property.PropertyType}'");
                }
            }
        }

        public Card[] FilterByCardSettings(Card[] unfilteredCards)
        {
            try
            {
                if (!USE_CARD_SETTINGS)
                {
                    return unfilteredCards;
                }

                var filteredCards = unfilteredCards.Where(x =>
                    {
                        int minLevel = x.IsSummoner ? SUMMONER_MIN_LEVEL : MONSTER_MIN_LEVEL;
                        return Convert.ToInt32(x.level) >= minLevel;
                    });
                return filteredCards.ToArray();
            }
            catch (Exception ex)
            {
                Log.WriteToLog($"Error at applying card settings: { ex.Message }", Log.LogType.Error);
                return unfilteredCards;
            }
        }
    }
}
