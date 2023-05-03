using System.IO;
using System.Windows;
using Microsoft.Win32;
using modeling_of_solids.atomic_model;

namespace modeling_of_solids;

public partial class MainWnd
{
    private void OnClickBtnSaveModel(object sender, RoutedEventArgs e)
    {
        if (_atomic != null)
        {
            var saveDialog = new SaveFileDialog
            {
                Title = "Сохранить структуру...",
                OverwritePrompt = true,
                CheckPathExists = true,
                Filter = "AtomicModel(*.atomic)|*.atomic|All files (*.*)|*.*"
            };
            if (saveDialog.ShowDialog() == true)
                using (var stream = new StreamWriter(saveDialog.FileName))
                {
                    
                }
        }
    }

    private void OnClickBtnLoadModel(object sender, RoutedEventArgs e)
    {
        
    }
}