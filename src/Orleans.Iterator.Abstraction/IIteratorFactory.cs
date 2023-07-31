namespace Orleans.Iterator.Abstraction;
public interface IIteratorFactory
{
    IGrainIterator CreateIterator<TGrainInterface>(params string[] storeName)
        where TGrainInterface : IGrain;

}
