namespace reMark.Mobile.Common.Model
{
    public interface ICopiable<T>
    {
        T ShallowCopy();

        T DeepCopy();
    }
}