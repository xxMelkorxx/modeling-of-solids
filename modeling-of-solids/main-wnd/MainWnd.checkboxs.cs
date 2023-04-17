using System.Windows;

namespace modeling_of_solids
{   
    public partial class MainWnd
    {
        private void OnCheckedDisplacement(object sender, RoutedEventArgs e)
        {
            _isDisplacement = true;
            NudDisplacement.IsEnabled = true;
        }

        private void OnUncheckedDisplacement(object sender, RoutedEventArgs e)
        {
            _isDisplacement = false;
            NudDisplacement.IsEnabled = false;
        }

        private void OnCheckedIsSnapshot(object sender, RoutedEventArgs e)
        {
            _isSnapshot = true;
            NudSnapshotStep.IsEnabled = true;
        }

        private void OnUncheckedIsSnapshot(object sender, RoutedEventArgs e)
        {
            _isSnapshot = false;
            NudSnapshotStep.IsEnabled = false;
        }

        private void OnCheckedIsRenormSpeeds(object sender, RoutedEventArgs e)
        {
            _isRenormSpeeds = true;
            NudTemperature.IsEnabled = true;
            NudStepNorm.IsEnabled = true;
        }

        private void OnUncheckedIsRenormSpeeds(object sender, RoutedEventArgs e)
        {
            _isRenormSpeeds = false;
            NudTemperature.IsEnabled = false;
            NudStepNorm.IsEnabled = false;
        }
    }
}