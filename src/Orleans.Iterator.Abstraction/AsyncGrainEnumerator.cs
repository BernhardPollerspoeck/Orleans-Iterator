﻿using Orleans.Runtime;

namespace Orleans.Iterator.Abstraction;

public class AsyncGrainEnumerator<TGrainInterface> : IAsyncEnumerator<GrainId>
    where TGrainInterface : IGrain
{
    #region fields
    private readonly IIteratorGrain<TGrainInterface> _grain;
    private readonly GrainDescriptor[] _grainDescriptions;

    private bool _initialized;
    private GrainId? _current;
    #endregion

    #region ctor
    public AsyncGrainEnumerator(IIteratorGrain<TGrainInterface> grain, params GrainDescriptor[] grainDescriptions)
    {
        _grain = grain;
		_grainDescriptions = grainDescriptions;
    }
    #endregion

    #region IAsyncEnumerator<GrainId>
    public GrainId Current => _current ?? throw new InvalidOperationException("Not allowed null value in iterator");

    public async ValueTask<bool> MoveNextAsync()
    {
        if (!_initialized)
        {
            await _grain.Initialize(_grainDescriptions);
            _initialized = true;
        }
        _current = await _grain.GetNextItem();
        return _current.HasValue;
    }
    public async ValueTask DisposeAsync()
    {
        await _grain.DisposeAsync();
        GC.SuppressFinalize(this);
    }
    #endregion
}