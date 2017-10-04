namespace IdentPlusLib
{
    public struct NotFound : Reply
    {
        public static Reply Instance = new NotFound();
    }
}