namespace Retranslator
{
    public class Language
    {
        public string Title { get; set; }
        public int Code { get; set; }

        public override int GetHashCode()
        {
            return Code;
        }

        public override string ToString()
        {
            return string.Format("{1}\t0x{2}\t{0}", Title, Code, Code.ToString("X4"));
        }
    }
}
