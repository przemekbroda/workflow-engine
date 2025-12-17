using System;
using System.Collections.Generic;

namespace EventSourcingEngine;

public record EventNode<TState>(HashSet<string> HandlesEvents, Type Executor, HashSet<string> ProducesEvents, List<EventNode<TState>> NextExecutors);

internal record EventNodeInst<TState>(INodeExecutor<TState> Executor, List<EventNodeInst<TState>> NextExecutors) where TState : new();