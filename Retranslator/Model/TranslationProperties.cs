using System;
using System.Text;

namespace Retranslator.Model
{
    /// <summary>
    /// Информация о переводе.
    /// </summary>
    public class TranslationProperties : ICloneable, IEquatable<TranslationProperties>
    {
        public string TargetApp { get; set; }
        public string Author { get; set; }
        public int SourceLang { get; set; }
        public int TargetLang { get; set; }

        /// <summary>
        /// Информация о переводе.
        /// </summary>
        public TranslationProperties()
        {
            SourceLang = DefaultSourceLang; 
            TargetLang = DefaultTargetLang; 
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("TargetApplication=" + TargetApp);
            sb.AppendLine("Author=" + Author);
            sb.AppendLine("SourceLANGID=" + SourceLang);
            sb.AppendLine("LANGID=" + TargetLang);
            sb.AppendLine("Generator=Retranslator v1.0");
            sb.AppendLine("LastModified=" + DateTime.Now);

            return sb.ToString();
        }

        static public int DefaultSourceLang
        {
            get { return 1033; } // 1033 -> en
        }

        static public int DefaultTargetLang
        {
            get { return 1049; } // 1049 -> ru
        }

        #region ICloneable Implementation

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion

        #region IEquatable Implementation

        public bool Equals(TranslationProperties other)
        {
            return
                TargetApp == other.TargetApp &&
                Author == other.Author &&
                SourceLang == other.SourceLang &&
                TargetLang == other.TargetLang;
        }

        #endregion
    }
}
