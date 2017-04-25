﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace raindrop.ModelBinders
{
    public interface IJsonAttribute
    {
        object TryConvert(string modelValue, Type targetType, out bool success);
    }
}
