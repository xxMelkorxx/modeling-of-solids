using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace modeling_of_solids
{
    public partial class MainWindow : Window
    {
        private AtomicModel _atomicModel;
        private bool _isDisplacement;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnLoadMainWindow(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Событие создание модели.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCreateModel(object sender, RoutedEventArgs e)
        {
            var size = NudSize.Value ?? 1;
            
            RtbOutputInfo.Document.Blocks.Clear();

            // Инициализация системы.
            _atomicModel = new AtomicModel(size, AtomType.Ar, LatticeType.FCC, PotentialType.LennardJones);

            // Применение случайного смещения для атомов.
            if (_isDisplacement)
                _atomicModel.AtomsDisplacement(NudDisplacement.Value ?? 0);

            // Вычисление начальных параметров системы.
            _atomicModel.InitCalculation();
            
            // Вывод начальной информации.
            RtbOutputInfo.AppendText(InitInfoSystem());

            BtnStartCalculation.IsEnabled = true;
            BtnPauseCalculation.IsEnabled = false;
            BtnResetCalculation.IsEnabled = false;
        }

        /// <summary>
        /// Событие запуска/возобновления вычислений.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartCalculation(object sender, RoutedEventArgs e)
        {
            BtnPauseCalculation.IsEnabled = true;
            BtnResetCalculation.IsEnabled = true;
        }

        /// <summary>
        /// Событие остановки вычислений. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPauseCalculation(object sender, RoutedEventArgs e)
        {
            
        }

        /// <summary>
        /// Событие сброса вычислений.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResetCalculation(object sender, RoutedEventArgs e)
        {
            BtnPauseCalculation.IsEnabled = false;
            BtnResetCalculation.IsEnabled = false;
        }

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

        /// <summary>
        /// Начальная информация о системе.
        /// </summary>
        /// <returns></returns>
        private string InitInfoSystem()
        {
            var text = "Структура создана...\n";
            text += string.Format("Тип атомов - {0}\n", _atomicModel.AtomsType);
            text += string.Format("Размер структуры (Nx/Ny) - {0}/{0}\n", _atomicModel.Size);
            text += string.Format("Размер структуры (Lx/Ly) - {0}/{0} нм\n", _atomicModel.L);
            text += string.Format("Число атомов - {0}\n", _atomicModel.CountAtoms);
            text += string.Format("Параметр решётки - {0} нм\n", _atomicModel.Lattice);
            text += string.Format("Кинетическая энергия - {0} эВ\n", _atomicModel.Ke.ToString("F5"));
            text += string.Format("Потенциальная энергия - {0} эВ\n", _atomicModel.Pe.ToString("F5"));
            text += string.Format("Полная энергия - {0} эВ\n", _atomicModel.Fe.ToString("F5"));
            text += string.Format("Температура - {0} К\n", _atomicModel.T.ToString("F1"));
            return text;
        }

        /// <summary>
        /// Заголовок таблицы.
        /// </summary>
        /// <returns></returns>
        private string TableHeader() => string.Format("{0} |{1} |{2} |{3} |{4} |\n",
            "Шаг".PadLeft(6),
            "Кин. энергия (эВ)".PadLeft(18),
            "Пот. энергия (эВ)".PadLeft(18),
            "Полн. энергия (эВ)".PadLeft(19),
            "Температура (К)".PadLeft(16)
        );

        /// <summary>
        /// Вывод данных в таблицу.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private string TableData(int i) => string.Format("{0} |{1} |{2} |{3} |{4} |\n",
            i.ToString().PadLeft(6),
            _atomicModel.Ke.ToString("F5").PadLeft(18),
            _atomicModel.Pe.ToString("F5").PadLeft(18),
            _atomicModel.Fe.ToString("F5").PadLeft(19),
            _atomicModel.T.ToString("F1").PadLeft(16)
        );

        /// <summary>
        /// Звуковой сигнал.
        /// </summary>
        /// <param name="freq">Частота сигнала.</param>
        /// <param name="duration">Длительность сигнала (мкс).</param>
        /// <param name="count">Повторений.</param>
        private static void AlarmBeep(int freq, int duration, int count)
        {
            for (var i = 0; i < count; i++)
                Console.Beep(freq, duration);
        }
    }
}