/*
 * Copyright 2016 Softtran. All Rights Reserved.
 * Author: maxim@softtran.ru (Maxim Zabolotsky)
 *
 */

using Sdl.LanguagePlatform.TranslationMemoryApi;
using System;

namespace SoftTran.TM.Hyperhub.Inline
{
    [TranslationProviderFactory(
        Id = "TMHHInlineTranslation",
        Name = "TMHHInlineTranslation",
        Description = "TMHHInlineTranslation")]
    public class TMHHInlineTranslationProviderFactory : ITranslationProviderFactory
    {
        public TranslationProviderInfo GetTranslationProviderInfo(Uri translationProviderUri, string translationProviderState)
        {
            return new TranslationProviderInfo
            {
                Name = PluginResources.Plugin_Name,
                TranslationMethod = TMProcOption.ProviderTranslationMethod
            };
        }

        public bool SupportsTranslationProviderUri(Uri translationProviderUri)
        {
            if (translationProviderUri == null)
            {
                throw new ArgumentNullException("translationProviderUri");
            }
            string[] array = translationProviderUri.Scheme.Split(new char[]
			{
				'.'
			});
            return string.Equals(translationProviderUri.Scheme, TMHHInlineTranslationProviderPlugin.TMHHInlineProviderScheme, StringComparison.OrdinalIgnoreCase);
        }

        public ITranslationProvider CreateTranslationProvider(Uri translationProviderUri, string translationProviderState, ITranslationProviderCredentialStore credentialStore)
        {
            if (!this.SupportsTranslationProviderUri(translationProviderUri))
            {
                throw new Exception(PluginResources.HHTMInlin_UriInvalid);
            }
            return new TMHHInlineTranslationProviderPlugin(new TMProcOption(translationProviderUri));
        }
    }
}
