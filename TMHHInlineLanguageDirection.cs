/*
 * Copyright 2016 Softtran. All Rights Reserved.
 * Author: maxim@softtran.ru (Maxim Zabolotsky)
 *
 */

using Sdl.LanguagePlatform.Core;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using Sdl.LanguagePlatform.TranslationMemory;
using System;
using System.Collections.Generic;
using System.Linq;

using Sdl.Core.Globalization;
using SoftTran.XML;
using Sdl.FileTypeSupport.Framework.NativeApi;
using System.Text.RegularExpressions;

namespace SoftTran.TM.Hyperhub.Inline
{
    #region "Translation Memory Suggestion"
    public class Suggestion
    {
        private float m_score;
        private string m_origin;
        private string m_src = "";
        private string m_tgt = "";
        private string m_note = "";

        // score between sorces segment and source text into TM
        public float score { get { return m_score; } set { m_score = value; } }
        // location original segment
        public string origin { get { return m_origin; } set { m_origin = value; } }
        // source text into TM
        public string src { get { return m_src; } set { m_src = value; } }
        // target text into TM
        public string tgt { get { return m_tgt; } set { m_tgt = value; } }
        // comments
        public string note { get { return m_note; } set { m_note = value; } }

        // extract value from note
        public T GetValueFromNote<T>(string p_ValName)
        {
            T a_Res = default(T);
            if (typeof(T) == typeof(bool))
            {
                a_Res = (T)(object)false;
            }
            if (!string.IsNullOrEmpty(m_note))
            {
                Regex a_Reg = new Regex(string.Format(@"^{0}\s*=(.*)$", p_ValName), RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (a_Reg.IsMatch(m_note))
                {
                    string a_Tmp = a_Reg.Match(m_note).Groups[1].Value;
                    a_Res = (T)Convert.ChangeType(a_Tmp, typeof(T));
                }
            }
            return a_Res;
        }
    }
    #endregion


    #region "Translation Memory Provider"
    public class TMHHInlineLanguageDirection : ITranslationProviderLanguageDirection
    {
        private LanguagePair m_languageDirection;
        private ITranslationProvider m_provider;
        public TMHHInlineLanguageDirection(ITranslationProvider p_provider, LanguagePair languageDirection)
        {
            m_provider = p_provider;
            m_languageDirection = languageDirection;
        }

        public System.Globalization.CultureInfo SourceLanguage
        {
            get { return m_languageDirection.SourceCulture; }
        }

        public System.Globalization.CultureInfo TargetLanguage
        {
            get { return m_languageDirection.TargetCulture; }
        }

        public ITranslationProvider TranslationProvider
        {
            get { return m_provider; }
        }

        /// <summary>
        /// Performs the actual search by looping through the
        /// delimited segment pairs contained in the text file.
        /// Depening on the search mode, a segment lookup (with exact machting) or a source / target
        /// concordance search is done.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        #region "SearchSegment"
        public SearchResults SearchSegment(SearchSettings settings, Segment segment)
        {
            return null;
        }
        #endregion

        /// <summary>
        /// Apply tag from the source segment to the new (target) segmebt
        /// </summary>
        /// <param name="p_SRC">Source segment</param>
        /// <param name="p_NewSeg">New (target) segmebt</param>
        /// <returns>Result segment with text from "NewSeg" and tags from "SRC"</returns>
        private Segment CreateTranslationProposal(Segment p_SRC, Segment p_NewSeg)
        {
            Segment a_New = new Segment(p_NewSeg.Culture);
            foreach (SegmentElement a_Item in p_NewSeg.Elements)
            {
                Tag a_Tag = a_Item as Tag;
                if(a_Tag == null)
                {
                    a_New.Add(a_Item);
                }
                else
                {
                    Tag a_TagNew = p_SRC.FindTag(a_Tag.Type, a_Tag.Anchor);
                    if (a_TagNew != null)
                    {
                        a_New.Add(a_TagNew.Duplicate());
                    }
                    else
                    {
                        a_New.Add(a_Tag);
                    }
                }
            }
            return a_New;
        }

        /// <summary>
        /// Serialize JSON string - Translation Memory Suggestions
        /// </summary>
        /// <param name="p_Val">JSON string</param>
        /// <param name="p_ShowTags">Kill tags</param>
        /// <returns>Translation Memory Suggestions</returns>
        private Segment CreateSegment(string p_Val, bool p_ShowTags)
        {
            Segment orgSegment = new Segment();
            if (!string.IsNullOrEmpty(p_Val))
            {
                if (p_ShowTags)
                {
                    try
                    {
                        object[] a_List = (object [])p_Val.ToObject<object>();
                        Dictionary<string, string> a_SRCId2TGTId = new Dictionary<string, string>();
                        foreach(object a_Item in a_List)
                        {
                            Dictionary<string, object> a_Val = a_Item as Dictionary<string, object>;
                            if (a_Val.Count == 1)
                            {
                                orgSegment.Add(new Text((string)a_Val["Value"]));
                            }
                            else
                            {
                                Tag a_Tag = new Tag();
                                a_Tag.AlignmentAnchor = (int)a_Val["AlignmentAnchor"];
                                a_Tag.Anchor = (int)a_Val["Anchor"];
                                a_Tag.TagID = (string)a_Val["TagID"];
                                a_Tag.TextEquivalent = (string)a_Val["TextEquivalent"];
                                a_Tag.Type = (TagType)a_Val["Type"];
                                orgSegment.Add(a_Tag);
                            }
                        }
                    }
                    catch
                    {
                        orgSegment.Add(p_Val);
                    }
                }
                else
                {
                    orgSegment.Add(p_Val);
                }
            }
            return orgSegment;
        }

        /// <summary>
        /// Creates the translation unit as it is later shown in the Translation Results
        /// window of SDL Trados Studio. This member also determines the match score
        /// (in our implementation always 100%, as only exact matches are supported)
        /// as well as the confirmation lelvel, i.e. Translated.
        /// </summary>
        /// <param name="searchSegment"></param>
        /// <param name="translation"></param>
        /// <param name="sourceSegment"></param>
        /// <returns></returns>
        #region "CreateSearchResult"
        private SearchResult CreateSearchResultNew(
            Segment p_SRCTags,
            string sourceSegment, 
            string translation,
            int p_score, 
            string p_Note = null,
            bool p_ShowTags = true)
        {
            #region "TranslationUnit"
            TranslationUnit tu = new TranslationUnit();
            tu.SourceSegment = CreateSegment(sourceSegment, p_ShowTags);
            tu.TargetSegment = CreateSegment(translation, p_ShowTags);

            if (!string.IsNullOrEmpty(p_Note))
            {
                tu.FieldValues.Add(new SingleStringFieldValue("Note", p_Note));
            }
            #endregion

            tu.ResourceId = new PersistentObjectToken(tu.GetHashCode(), Guid.Empty);

            #region "TuProperties"
            tu.Origin = TranslationUnitOrigin.TM;


            SearchResult searchResult = new SearchResult(tu);
            searchResult.ScoringResult = new ScoringResult();
            searchResult.ScoringResult.BaseScore = p_score;
            TranslationUnit tuProposal = new TranslationUnit();
            tuProposal.SourceSegment = CreateTranslationProposal(p_SRCTags, tu.SourceSegment);
            tuProposal.TargetSegment = CreateTranslationProposal(p_SRCTags, tu.TargetSegment);
            searchResult.TranslationProposal = tuProposal;

            tu.ConfirmationLevel = ConfirmationLevel.Translated;
            #endregion

            return searchResult;
        }
        #endregion


        public bool CanReverseLanguageDirection
        {
            get { return false; }
        }
        
        public SearchResults[] SearchSegments(SearchSettings settings, Segment[] segments)
        {
            return null;
        }

        public SearchResults[] SearchSegmentsMasked(SearchSettings settings, Segment[] segments, bool[] mask)
        {
            return null;
        }

        public SearchResults SearchText(SearchSettings settings, string segment)
        {
            return null;
        }

        private static string c_TMNoteLayout = "{0}\n\nDomain:\t{1}\nApp:\t{2}\nMod:\t{3}\nType:\t{4}\nFileName:\t{5}\nItem:\t{6}";
        private static Regex m_ParsFileName = new Regex(@"^/?([^/]+)/([^/]+)/([^/]+)/(.*?)/([^/]+)$");

        /// <summary>
        /// Try search suggestion for segments
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="translationUnit"></param>
        /// <returns></returns>
        public SearchResults SearchTranslationUnit(SearchSettings settings, TranslationUnit translationUnit)
        {
            if (translationUnit == null) return null;

            List<Suggestion> a_Suggestions = null;
            IList<IContextInfo> a_Contexts = translationUnit
                .DocumentSegmentPair.Source.ParentParagraphUnit.Properties.Contexts.Contexts;

            // show suggestions with tag markers
            bool a_ShowTags = (m_provider as TMHHInlineTranslationProviderPlugin).Options.ShowTags;
            if (a_ShowTags)
            {
                // with tags
                a_Suggestions = a_Contexts.FirstOrDefault(pObj => pObj.MetaDataContainsKey("suggestionsWithTag"))?
                    .GetMetaData("suggestionsWithTag")?.ToObject<List<Suggestion>>();
            }
            else
            {
                // without tags
                a_Suggestions = a_Contexts.FirstOrDefault(pObj => pObj.MetaDataContainsKey("suggestions"))?
                    .GetMetaData("suggestions")?.ToObject<List<Suggestion>>();
            }

            SearchResults a_Res = null;
            if (a_Suggestions?.Count > 0)
            {
                settings.SortSpecification = new SortSpecification();

                // extract by scope and ordering the suggestions
                IEnumerable<Suggestion> a_SuggestionsShow = a_Suggestions
                    .Where(pObj => settings.MinScore <= pObj.score)
                    .OrderByDescending(order => order.score)
                    .ThenBy(order => order.GetValueFromNote<bool>("isCrossDomain") ? 1 : 0);

                // show only "MaxResults" count of suggestions
                if (settings.MaxResults > 0)
                {
                    a_SuggestionsShow = a_SuggestionsShow.Take(settings.MaxResults);
                }

                if (a_SuggestionsShow.Count() > 0)
                {
                    a_Res = new SearchResults(new SortSpecification());
                    a_Res.SourceSegment = translationUnit.SourceSegment.Duplicate();

                    foreach (Suggestion a_Suggestion in a_Suggestions)
                    {
                        // show this Translation Memory Suggestion
                        string a_Domain = a_Suggestion.GetValueFromNote<string>("domain");

                        GroupCollection a_Info = null;
                        if (m_ParsFileName.IsMatch(a_Suggestion.origin))
                        {
                            a_Info = m_ParsFileName.Match(a_Suggestion.origin).Groups;
                        }

                        // Create TM noute Layout
                        string a_Note = string.Format(c_TMNoteLayout,
                        a_Suggestion.origin,
                        a_Domain,
                        a_Info?[1]?.Value ?? "",
                        a_Info?[2]?.Value ?? "",
                        a_Info?[3]?.Value ?? "",
                        a_Info?[4]?.Value ?? "",
                        a_Info?[5]?.Value ?? "");

                        SearchResult a_SRes = CreateSearchResultNew(a_Res.SourceSegment, 
                            a_Suggestion.src, 
                            a_Suggestion.tgt,
                            (int)Math.Truncate(a_Suggestion.score), 
                            a_Note);

                        a_Res.Add(a_SRes);
                    }
                }
            }

            return a_Res;
        }

        public SearchResults[] SearchTranslationUnits(SearchSettings settings, TranslationUnit[] translationUnits)
        {
            return translationUnits?.Select(pObj => SearchTranslationUnit(settings, pObj)).ToArray();
        }

        public SearchResults[] SearchTranslationUnitsMasked(SearchSettings settings, TranslationUnit[] translationUnits, bool[] mask)
        {
            if (translationUnits == null)
            {
                throw new ArgumentNullException("translationUnits in SearchTranslationUnitsMasked");
            }
            if (mask == null || mask.Length != translationUnits.Length)
            {
                throw new ArgumentException("mask in SearchTranslationUnitsMasked");
            }

            List<SearchResults> results = new List<SearchResults>();

            int i = 0;
            foreach (var tu in translationUnits)
            {
                if (mask == null || mask[i])
                {
                    var result = SearchTranslationUnit(settings, tu);
                    results.Add(result);
                }
                else
                {
                    results.Add(null);
                }
                i++;
            }

            return results.ToArray();
        }



        #region "NotForThisImplementation"
        /// <summary>
        /// Not required for this implementation.
        /// </summary>
        /// <param name="translationUnits"></param>
        /// <param name="settings"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public ImportResult[] AddTranslationUnitsMasked(TranslationUnit[] translationUnits, ImportSettings settings, bool[] mask)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not required for this implementation.
        /// </summary>
        /// <param name="translationUnit"></param>
        /// <returns></returns>
        public ImportResult UpdateTranslationUnit(TranslationUnit translationUnit)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not required for this implementation.
        /// </summary>
        /// <param name="translationUnits"></param>
        /// <returns></returns>
        public ImportResult[] UpdateTranslationUnits(TranslationUnit[] translationUnits)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Not required for this implementation.
        /// </summary>
        /// <param name="translationUnits"></param>
        /// <param name="previousTranslationHashes"></param>
        /// <param name="settings"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public ImportResult[] AddOrUpdateTranslationUnitsMasked(TranslationUnit[] translationUnits, int[] previousTranslationHashes, ImportSettings settings, bool[] mask)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not required for this implementation.
        /// </summary>
        /// <param name="translationUnit"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public ImportResult AddTranslationUnit(TranslationUnit translationUnit, ImportSettings settings)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not required for this implementation.
        /// </summary>
        /// <param name="translationUnits"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public ImportResult[] AddTranslationUnits(TranslationUnit[] translationUnits, ImportSettings settings)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not required for this implementation.
        /// </summary>
        /// <param name="translationUnits"></param>
        /// <param name="previousTranslationHashes"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public ImportResult[] AddOrUpdateTranslationUnits(TranslationUnit[] translationUnits, int[] previousTranslationHashes, ImportSettings settings)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
    #endregion
}
