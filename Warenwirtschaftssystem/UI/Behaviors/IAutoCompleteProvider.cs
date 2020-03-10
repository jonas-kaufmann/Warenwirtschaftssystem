using System.Collections;

namespace Warenwirtschaftssystem.UI.Behaviors
{
    public interface IAutoCompleteProvider
    {
        public IList GetSuggestions(string filter);
    }
}
