/*
 * Copyright 2016 Softtran. All Rights Reserved.
 * Author: maxim@softtran.ru (Maxim Zabolotsky)
 *
 */

using System;

using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace SoftTran.TM.Hyperhub.Inline
{
    public class TMProcOption
    {
        #region "TranslationMethod"
        public static readonly TranslationMethod ProviderTranslationMethod = TranslationMethod.Other;
        #endregion

        #region "TranslationProviderUriBuilder"
        TranslationProviderUriBuilder _uriBuilder;

        public TMProcOption()
        {
            _uriBuilder = new TranslationProviderUriBuilder(TMHHInlineTranslationProviderPlugin.TMHHInlineProviderScheme);
        }

        public TMProcOption(Uri uri)
        {
            _uriBuilder = new TranslationProviderUriBuilder(uri);
            m_ShowTags = true;
        }
        #endregion

        private bool m_ShowTags;
        public bool ShowTags
        {

            get
            {
                return m_ShowTags;
            }

            set
            {
                m_ShowTags = value;
            }
        }


        #region "Uri"
        public Uri Uri
        {
            get
            {
                return _uriBuilder.Uri;
            }
        }
        #endregion
    }
}
