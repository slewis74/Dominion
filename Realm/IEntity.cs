﻿namespace Realm
{
    public interface IEntity<out TId>
    {
        TId Id { get; }
    }
}