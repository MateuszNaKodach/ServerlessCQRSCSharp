using System;

namespace CQRS.Essentials.Abstractions.CQRS
{
    public interface ICommand
    {
        Guid Id { get; }
    }
}
