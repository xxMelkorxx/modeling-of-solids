using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
		private BackgroundWorker _bgWorkerCreateModel, _bgWorkerCalculation;
		private AtomicModel? _atomicModel;
		private bool _isDisplacement, _isSnapshot;
		private int _iter;

		private class FindPrimesInput
		{
			public List<object> args;

			public FindPrimesInput(object[] args)
			{
				this.args = new List<object>();
				foreach (var arg in args)
					this.args.Add(arg);
			}
		}

		public MainWindow()
		{
			InitializeComponent();

			_bgWorkerCreateModel = (BackgroundWorker)this.FindResource("backgroundWorkerCreateModel");
			_bgWorkerCalculation = (BackgroundWorker)this.FindResource("backgroundWorkerCalculation");
		}

		private void OnLoadMainWindow(object sender, RoutedEventArgs e)
		{

		}

		#region ---События создания модели---
		/// <summary>
		/// Событие создание модели.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCreateModel(object sender, RoutedEventArgs e)
		{
			var size = NudSize.Value ?? 1;
			var k = NudDisplacement.Value ?? 0;
			_iter = 1;

			BtnCreateModel.IsEnabled = false;
			ProgressBar.Value = 0;
			RtbOutputInfo.Document.Blocks.Clear();

			_bgWorkerCreateModel.RunWorkerAsync(new FindPrimesInput(new object[] { size, k }));
		}

		private void OnBackgroundWorkerDoWorkCreateModel(object sender, DoWorkEventArgs e)
		{
			FindPrimesInput input = (FindPrimesInput)(e.Argument ?? throw new ArgumentException("Отсутствуют аргументы"));
			var size = (int)input.args[0];
			var k = (double)input.args[1];
			_bgWorkerCreateModel.ReportProgress(25);

			// Инициализация системы.
			_atomicModel = new AtomicModel(size, AtomType.Ar, LatticeType.FCC, PotentialType.LennardJones);
			_bgWorkerCreateModel.ReportProgress(50);

			// Применение случайного смещения для атомов.
			if (_isDisplacement)
			{
				_atomicModel.AtomsDisplacement(k);
				_bgWorkerCreateModel.ReportProgress(75);
			}

			// Вычисление начальных параметров системы.
			_atomicModel.InitCalculation();
			_bgWorkerCreateModel.ReportProgress(100);
		}

		private void OnBackgroundWorkerRunWorkerCompletedCreateModel(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
				MessageBox.Show(e.Error.Message, "Произошла ошибка");
			else
			{
				// Вывод начальной информации.
				RtbOutputInfo.AppendText(InitInfoSystem());

				BtnCreateModel.IsEnabled = true;
				BtnStartCalculation.IsEnabled = true;
				BtnPauseCalculation.IsEnabled = false;
				BtnResetCalculation.IsEnabled = false;

				AlarmBeep(1000, 500, 1);
			}
		}

		private void OnBackgroundWorkerProgressChangedCreateModel(object sender, ProgressChangedEventArgs e)
		{
			ProgressBar.Value = e.ProgressPercentage;
		}
		#endregion

		#region ---События панели управления---
		/// <summary>
		/// Событие запуска/возобновления вычислений.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnStartCalculation(object sender, RoutedEventArgs e)
		{
			var countStep = NudCountStep.Value;
			var snapshotStep = NudSnapshotStep.Value;
			var dt = NudTimeStep.Value ?? 1;
			var dt_order = NudTimeStepOrder.Value ?? -14;

			if (_atomicModel != null)
				_atomicModel.dt = dt * Math.Pow(10, dt_order);

			BtnPauseCalculation.IsEnabled = true;
			BtnResetCalculation.IsEnabled = true;

			ChartFe.Plot.Clear();
			ChartKe.Plot.Clear();
			ChartPe.Plot.Clear();

			// Вывод начальной информации.
			RtbOutputInfo.AppendText("\n\nЗапуск моделирования...\n");
			RtbOutputInfo.AppendText("Количество временных шагов: " + countStep + "\n");
			if (_isSnapshot)
			{
				RtbOutputInfo.AppendText(TableHeader());
				RtbOutputInfo.AppendText(TableData(_iter - 1));
			}


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
		#endregion

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

		#region ---Вспомогательные методы---
		/// <summary>
		/// Начальная информация о системе.
		/// </summary>
		/// <returns></returns>
		private string InitInfoSystem()
		{
			var text = "Структура создана...\n";
			if (_atomicModel != null)
			{
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
			else throw new NullReferenceException("Class AtomicModel не инициализирован!");
		}

		/// <summary>
		/// Заголовок таблицы.
		/// </summary>
		/// <returns></returns>
		private static string TableHeader()
		{
			return string.Format("{0} |{1} |{2} |{3} |{4} |\n",
				"Шаг".PadLeft(6),
				"Кин. энергия (эВ)".PadLeft(18),
				"Пот. энергия (эВ)".PadLeft(18),
				"Полн. энергия (эВ)".PadLeft(19),
				"Температура (К)".PadLeft(16));
		}

		/// <summary>
		/// Вывод данных в таблицу.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		private string TableData(int i)
		{
			if (_atomicModel != null)
				return string.Format("{0} |{1} |{2} |{3} |{4} |\n",
					i.ToString().PadLeft(6),
					_atomicModel.Ke.ToString("F5").PadLeft(18),
					_atomicModel.Pe.ToString("F5").PadLeft(18),
					_atomicModel.Fe.ToString("F5").PadLeft(19),
					_atomicModel.T.ToString("F1").PadLeft(16));
			else throw new NullReferenceException("Class AtomicModel не инициализирован!");
		}

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
		#endregion
	}
}