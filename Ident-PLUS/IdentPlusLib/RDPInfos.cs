namespace IdentPlusLib
{
    public struct RDPInfos : Reply
    {
        public RDPInfos(string name, string rdpAdresse, string rdpUserName)
        {
            Name = name;
            RDPAdresse = rdpAdresse;
            RDPUserName = rdpUserName;
        }

        public readonly string Name;
        public readonly string RDPAdresse;
        public readonly string RDPUserName;
    }
}