using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace modeling_of_solids
{
	public partial class MainWindow : Window
	{
		private BackgroundWorker _bgWorkerCreateModel, _bgWorkerCalculation, _bgWorkerRenormalizationSpeeds;
		private AtomicModel? _atomicModel;
		private bool _isDisplacement, _isSnapshot;
		private int _iter;
		//private Mutex mutexObj;

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
			_bgWorkerRenormalizationSpeeds = (BackgroundWorker)this.FindResource("backgroundWorkerRenormalizationSpeeds");
		}

		private void OnLoadMainWindow(object? sender, RoutedEventArgs? e)
		{

		}

		#region ---СОБЫТИЯ СОЗДАНИЯ МОДЕЛИ---
		/// <summary>
		/// Событие создание модели.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCreateModel(object? sender, RoutedEventArgs? e)
		{
			var size = NudSize.Value ?? 1;
			var k = NudDisplacement.Value ?? 0;
			_iter = 1;

			BtnCreateModel.IsEnabled = false;
			ProgressBar.Value = 0;
			RtbOutputInfo.Document.Blocks.Clear();

			AlarmBeep(500, 500, 1);

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
				BtnStartRenormalizationSpeeds.IsEnabled = true;
				BtnCancelCalculation.IsEnabled = false;
			}
		}

		private void OnBackgroundWorkerProgressChangedCreateModel(object sender, ProgressChangedEventArgs e)
		{
			ProgressBar.Value = e.ProgressPercentage;
		}
		#endregion

		#region ---СОБЫТИЯ ЗАПУСКА МОДЕЛИРОВАНИЯ---
		/// <summary>
		/// Событие запуска/возобновления вычислений.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnStartCalculation(object? sender, RoutedEventArgs? e)
		{
			var countStep = NudCountStep.Value ?? 1000;
			var snapshotStep = NudSnapshotStep.Value ?? 1;
			var dt = NudTimeStep.Value ?? 1;
			var dt_order = NudTimeStepOrder.Value ?? -14;

			if (_atomicModel != null)
				_atomicModel.dt = dt * Math.Pow(10, dt_order);

			BtnStartCalculation.IsEnabled = false;
			BtnStartRenormalizationSpeeds.IsEnabled = false;
			BtnCancelCalculation.IsEnabled = true;
			BtnPauseCalculation.IsEnabled = true;

			// Очистка графиков.
			ChartFe.Plot.Clear();
			ChartKe.Plot.Clear();
			ChartPe.Plot.Clear();

			// Сброс ProgressBar.
			ProgressBar.Value = 0;

			// Вывод начальной информации.
			RtbOutputInfo.AppendText("\n\nЗапуск моделирования...\n");
			RtbOutputInfo.AppendText("Количество временных шагов: " + countStep + "\n");
			if (_isSnapshot)
			{
				RtbOutputInfo.AppendText(TableHeader());
				RtbOutputInfo.AppendText(TableData(_iter - 1));
			}

			// Запуск расчётов.
			_bgWorkerCalculation.RunWorkerAsync(new FindPrimesInput(new object[] { countStep, snapshotStep }));
		}

		private void OnBackgroundWorkerDoWorkCalculation(object sender, DoWorkEventArgs e)
		{
			// TO DO
			//mutexObj = new();
			if (_bgWorkerCalculation.CancellationPending)
			{
				e.Cancel = true;
				//mutexObj.ReleaseMutex();
				
				return;
			}
			else
			{
				for (int i = 0; i < 1e3; i++)
				{
					Application.Current.Dispatcher.Invoke(
					DispatcherPriority.Background,
					() => {
						RtbOutputInfo.AppendText(i.ToString());
						RtbOutputInfo.ScrollToEnd();
					});
				}
			}
			//mutexObj.ReleaseMutex();
		}

		private void OnBackgroundWorkerRunWorkerCompletedCalculation(object sender, RunWorkerCompletedEventArgs e)
		{
			BtnCancelCalculation.IsEnabled = false;

			if (e.Cancelled)
			{
				MessageBox.Show("Моделирование отменено");
				OnCreateModel(null, null);
			}
			else if (e.Error != null)
			{
				RtbOutputInfo.AppendText("End!");
				// TO DO
			}
		}

		private void OnBackgroundWorkerProgressChangedCalculation(object sender, ProgressChangedEventArgs e)
		{
			ProgressBar.Value = e.ProgressPercentage;
		}
		#endregion

		#region ---СОБЫТИЯ ПЕРЕНОРМИРОВКИ СКОРОСТЕЙ---
		/// <summary>
		/// Событие запуска перенормировки скоростей.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnStartRenormalizationSpeeds(object sender, RoutedEventArgs e)
		{
			var countStep = NudCountStep.Value ?? 1000;
			var snapshotStep = NudSnapshotStep.Value ?? 1;
			var T = NudTemperature.Value ?? 300;
			var stepNorm = NudStepNorm.Value ?? 100;
			var dt = NudTimeStep.Value ?? 1;
			var dt_order = NudTimeStepOrder.Value ?? -14;

			if (_atomicModel != null)
				_atomicModel.dt = dt * Math.Pow(10, dt_order);

			BtnStartCalculation.IsEnabled = false;
			BtnStartRenormalizationSpeeds.IsEnabled = false;
			BtnCancelCalculation.IsEnabled = true;

			//TO DO
		}

		private void OnBackgroundWorkerDoWorkRenormalizationSpeeds(object sender, DoWorkEventArgs e)
		{
			// TO DO

			if (_bgWorkerRenormalizationSpeeds.CancellationPending)
			{
				e.Cancel = true;
				return;
			}
		}

		private void OnBackgroundWorkerRunWorkerCompletedRenormalizationSpeeds(object sender, RunWorkerCompletedEventArgs e)
		{
			BtnCancelCalculation.IsEnabled = false;

			if (e.Cancelled)
			{
				MessageBox.Show("Моделирование отменено");
				OnCreateModel(null, null);
			}
			else if (e.Error != null)
			{

			}
		}

		private void OnBackgroundWorkerProgressChangedRenormalizationSpeeds(object sender, ProgressChangedEventArgs e)
		{
			ProgressBar.Value = e.ProgressPercentage;
		}
		#endregion

		/// <summary>
		/// Событие отмена вычислений.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCancelCalculation(object sender, RoutedEventArgs e)
		{
			if (_bgWorkerCalculation.IsBusy)
				_bgWorkerCalculation.CancelAsync();
			if (_bgWorkerRenormalizationSpeeds.IsBusy)
				_bgWorkerRenormalizationSpeeds.CancelAsync();
		}

		private void OnPauseCalculation(object sender, RoutedEventArgs e)
		{
			//mutexObj.WaitOne();
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