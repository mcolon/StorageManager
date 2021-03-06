﻿using System.Collections.Generic;

namespace StorageManager.Query
{
    public class StorageQueryResult
    {
        public string QueryContext { get; set; }
        public bool HasMoreResult { get; set; }
    }

    public class StorageQueryResult<T> : StorageQueryResult
    {
        public IEnumerable<T> Records { get; set; }
    }
}