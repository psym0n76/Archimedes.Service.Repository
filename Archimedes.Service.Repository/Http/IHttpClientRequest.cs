﻿using Archimedes.Library.Message;

namespace Archimedes.Service.Repository
{
    public interface IHttpClientRequest
    {
        void PostPrice(ResponseCandle message);
    }
}