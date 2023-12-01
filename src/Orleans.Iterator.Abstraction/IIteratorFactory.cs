namespace Orleans.Iterator.Abstraction;
public interface IIteratorFactory
{
    IGrainIterator CreateIterator<TGrainInterface>(params GrainDescriptor[] grainDescriptions)
        where TGrainInterface : IGrain;

}
