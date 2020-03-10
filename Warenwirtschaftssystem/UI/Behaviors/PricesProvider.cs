using System.Collections;
using System.Collections.ObjectModel;

namespace Warenwirtschaftssystem.UI.Behaviors
{
    public class PricesProvider : IAutoCompleteProvider
    {
        public ObservableCollection<decimal> Suggestions { get; } = new ObservableCollection<decimal>();


        public IList GetSuggestions(string filter)
        {
            return Suggestions;
        }
    }
}
