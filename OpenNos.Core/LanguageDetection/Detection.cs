namespace OpenNos.Core.LanguageDetection
{
    public class Detection
    {
        #region Properties

        public float confidence { get; set; }

        public bool isReliable { get; set; }

        public string language { get; set; }

        #endregion
    }
}