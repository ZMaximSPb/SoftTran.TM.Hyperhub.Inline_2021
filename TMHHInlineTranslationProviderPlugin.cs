/*
 * Copyright 2016 Softtran. All Rights Reserved.
 * Author: maxim@softtran.ru (Maxim Zabolotsky)
 *
 */

using Sdl.LanguagePlatform.Core;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using System;

namespace SoftTran.TM.Hyperhub.Inline
{
    public class TMHHInlineTranslationProviderPlugin : ITranslationProvider
    {
        public static readonly string TMHHInlineProviderScheme = "tmhhinline";
        private TMProcOption m_Options;

        public TMProcOption Options
        {
            get { return m_Options; }
            set { m_Options = value; }
        }

        public TMHHInlineTranslationProviderPlugin(TMProcOption p_Options)
        {
            m_Options = p_Options;
        }

        public Uri Uri
        {
            get
            {
                return m_Options.Uri;
            }
        }

        public string Name
        {
            get
            {
                return PluginResources.Plugin_Name;
            }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public void LoadState(string translationProviderState)
        {
        }

        public void RefreshStatusInfo()
        {

        }

        public string SerializeState()
        {
            // Save settings
            return null;
        }

        public ProviderStatusInfo StatusInfo
        {
            get { return GetStatusInfo(); }
        }


        public bool SupportsDocumentSearches
        {
            get
            {
                return true;
            }
        }

        protected ProviderStatusInfo GetStatusInfo()
        {
            return new ProviderStatusInfo(true, "OK");
		}

        public ITranslationProviderLanguageDirection GetLanguageDirection(LanguagePair languageDirection)
        {
            return new TMHHInlineLanguageDirection(this, languageDirection);
        }

        public bool SupportsLanguageDirection(LanguagePair languageDirection)
        {
            return true;
        }

        #region "SupportsMultipleResults"
        public bool SupportsMultipleResults
        {
            get { return false; }
        }
        #endregion

        #region "SupportsPenalties"
        public bool SupportsPenalties
        {
            get { return false; }
        }
        #endregion

        public bool SupportsPlaceables
        {
            get { return false; }
        }

        public bool SupportsScoring
        {
            get { return false; }
        }

        #region "SupportsSearchForTranslationUnits"
        public bool SupportsSearchForTranslationUnits
        {
            get { return true; }
        }
        #endregion

        #region "SupportsSourceTargetConcordanceSearch"
        public bool SupportsSourceConcordanceSearch
        {
            get { return false; }
        }

        public bool SupportsTargetConcordanceSearch
        {
            get { return false; }
        }
        #endregion

        public bool SupportsStructureContext
        {
            get { return false; }
        }

        #region "SupportsTaggedInput"
        public bool SupportsTaggedInput
        {
            get { return false; }
        }
        #endregion


        public bool SupportsTranslation
        {
            get { return true; }
        }

        #region "SupportsUpdate"
        public bool SupportsUpdate
        {
            get { return false; }
        }
        #endregion

        public bool SupportsWordCounts
        {
            get { return true; }
        }

        public TranslationMethod TranslationMethod
        {
            get { return TranslationMethod.Other; }
        }

        #region "SupportsConcordanceSearch"
        public bool SupportsConcordanceSearch
        {
            get { return false; }
        }
        #endregion

        public bool SupportsFilters
        {
            get { return false; }
        }

        #region "SupportsFuzzySearch"
        public bool SupportsFuzzySearch
        {
            get { return true; }
        }
        #endregion
    }
}
