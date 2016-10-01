namespace Retranslator.ViewModel
{
    public delegate bool FindDelegate(SearchParameters @params);

    public class SearchParameters
    {
        public string Pattern { get; set; }

        public bool CaseSensetive { get; set; }

        public bool WholeWord { get; set; }

        public bool Backward { get; set; }

        public bool ThroughIds { get; set; }
        public bool ThroughIdsEnabled { get; set; }

        public bool ThroughSources { get; set; }

        public bool ThroughTranslations { get; set; }

        public SearchParameters()
        {
            ThroughIdsEnabled = true;
            
            ThroughSources = true;
        }
    }
}
