namespace CoreCodeCamp.Mapping
{
    public interface IMap
    {
        T Map<T>(V ) where T : class;
    }
}
