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
        public static string GetLanguage(string completeTextString)
        {
            var factory = new RankedLanguageIdentifierFactory();
            //set the dictionary path
            var identifier = factory.Load("NTextCat\\LanguageModels\\Core14.profile.xml");
            //get the language
            var languages = identifier.Identify(completeTextString);
            var mostCertainLanguage = languages.FirstOrDefault();
            if (mostCertainLanguage != null)
            {
                //get the language in two-digit form e.g. en, de, fr...
                String language = mostCertainLanguage.Item1.Iso639_3;
                return language;
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