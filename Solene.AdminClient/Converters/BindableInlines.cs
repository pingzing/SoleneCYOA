using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace Solene.AdminClient.Converters
{
    public class BindableRuns
    {
        public static IEnumerable<Run> GetBindableRuns(DependencyObject obj)
        {
            return (IEnumerable<Run>)obj.GetValue(BindableRunsProperty);
        }

        public static void SetBindableRuns(DependencyObject obj, IEnumerable<Run> value)
        {
            obj.SetValue(BindableRunsProperty, value);
        }

        public static readonly DependencyProperty BindableRunsProperty = DependencyProperty.RegisterAttached(
            "BindableRuns",
            typeof(IEnumerable<Run>),
            typeof(BindableRuns),
            new PropertyMetadata(null, OnPropertyChanged));


        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (!(sender is TextBlock target))
            {
                return;
            }

            target.Inlines.Clear();
            var runs = args.NewValue as IEnumerable<Run>;
            foreach(var run in runs)
            {
                target.Inlines.Add(run);
            }
        }
    }
}
