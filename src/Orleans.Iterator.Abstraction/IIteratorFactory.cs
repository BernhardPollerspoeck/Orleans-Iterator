namespace Orleans.Iterator.Abstraction;
public interface IIteratorFactory
{
    IGrainIterator CreateIterator<TGrainInterface>(string storeName)
        where TGrainInterface : IGrain;

}
