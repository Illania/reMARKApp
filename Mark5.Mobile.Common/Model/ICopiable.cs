namespace Mark5.Mobile.Common.Model
{
    public interface ICopiable<T>
    {
        T ShallowCopy();

        T DeepCopy();
    }
}