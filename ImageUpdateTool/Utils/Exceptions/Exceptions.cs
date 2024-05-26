using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUpdateTool.Utils.Exceptions;

public class ModelProcessingUnderErrorStatus : Exception
{
    public ModelProcessingUnderErrorStatus() { }
    public ModelProcessingUnderErrorStatus(string message) : base(message) { }
    public ModelProcessingUnderErrorStatus(string message, Exception inner) : base(message, inner) { }
    protected ModelProcessingUnderErrorStatus(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

public class RegexNotMatch : Exception
{
    public RegexNotMatch() { }
    public RegexNotMatch(string message) : base(message) { }
    public RegexNotMatch(string message, Exception inner) : base(message, inner) { }
    protected RegexNotMatch(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
