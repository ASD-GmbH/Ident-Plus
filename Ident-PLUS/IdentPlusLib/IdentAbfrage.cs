using System.Threading.Tasks;

namespace IdentPlusLib
{
    public delegate Task<Reply> IdentAbfrage(Query query);
}