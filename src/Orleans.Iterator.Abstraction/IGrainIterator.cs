﻿namespace Orleans.Iterator.Abstraction;

public interface IGrainIterator : IAsyncEnumerable<GrainId>
{
}
