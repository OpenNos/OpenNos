/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using NTextCat;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace OpenNos.Core
{
    public class Language
    {
        #region Members

        private static Language instance;
        private ResourceManager _manager;
        private CultureInfo _resourceCulture;
        private RankedLanguageIdentifier _identifier;
        private RankedLanguageIdentifierFactory _factory;
        #endregion

        #region Instantiation

        private Language()
        {
            _resourceCulture = new CultureInfo(System.Configuration.ConfigurationManager.AppSettings["language"]);
            if (Assembly.GetEntryAssembly() != null)
            {
                _manager = new ResourceManager(Assembly.GetEntryAssembly().GetName().Name + ".Resource.LocalizedResources", Assembly.GetEntryAssembly());
            }
        }

        #endregion

        #region Properties

        public static Language Instance
        {
            get
            {
                return instance ?? (instance = new Language());
            }
        }

        #endregion

        #region Methods
        public string GetLanguage(string completeTextString)
        {
            if(_factory == null || _identifier == null)
            {
                _factory = new RankedLanguageIdentifierFactory();
                _identifier = _factory.Load(@"Core14.profile.xml");
            }
            var mostCertainLanguage = _identifier.Identify(completeTextString).FirstOrDefault();
            if (mostCertainLanguage != null)
            {
                //get the language in two-digit form e.g. en, de, fr...
                CultureInfo cult = CultureInfo.GetCultures(CultureTypes.NeutralCultures).FirstOrDefault(ci => string.Equals(ci.ThreeLetterISOLanguageName, mostCertainLanguage.Item1.Iso639_2T, StringComparison.OrdinalIgnoreCase));    
                return cult.TwoLetterISOLanguageName;
            }
            else
            {
                return String.Empty;
            }
        }
        public string GetMessageFromKey(string message)
        {
            string resourceMessage = _manager != null ? _manager.GetString(message, _resourceCulture) : string.Empty;

            return !string.IsNullOrEmpty(resourceMessage) ? resourceMessage : $"#<{message}>";
        }

        #endregion
    }
}