using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace modeling_of_solids
{
	public partial class MainWindow : Window
	{
		private AtomicModel? _atomicModel;

		private BackgroundWorker _bgWorkerCreateModel, _bgWorkerCalculation;
		private int _iter, _iterCounter;
		//private Mutex mutexObj;

		private bool _isDisplacement, _isSnapshot, _isRenormSpeeds;

		private List<double> _kePoints, _pePoints, _fePoints;

		private SceneManager _scene;
		private List<List<Vector>> _positionsAtomsList;
		private Point3D _cameraPosition = new(2, 2, 5);

		private class FindPrimesInput
		{
			public List<object> Args;

			public FindPrimesInput(object[] args)
			{
				this.Args = new List<object>();
				foreach (var arg in args)
					this.Args.Add(arg);
			}
		}

		public MainWindow()
		{
			InitializeComponent();

			_bgWorkerCreateModel = (BackgroundWorker)this.FindResource("backgroundWorkerCreateModel");
			_bgWorkerCalculation = (BackgroundWorker)this.FindResource("backgroundWorkerCalculation");
		}

		void OnLoadedMainWindow(object sender, RoutedEventArgs e)
		{
			// Настройка графика энергий системы.
			ChartEnergy.Plot.Title("Графики энергий системы");
			ChartEnergy.Plot.XLabel("Временной шаг");
			ChartEnergy.Plot.YLabel("Энергия (эВ)");
			ChartEnergy.Plot.XAxis.MajorGrid(enable: true, color: Color.FromArgb(50, Color.Black));
			ChartEnergy.Plot.YAxis.MajorGrid(enable: true, color: Color.FromArgb(50, Color.Black));
			ChartEnergy.Plot.XAxis.MinorGrid(enable: true, color: Color.FromArgb(30, Color.Black), lineStyle: LineStyle.Dot);
			ChartEnergy.Plot.YAxis.MinorGrid(enable: true, color: Color.FromArgb(30, Color.Black), lineStyle: LineStyle.Dot);
			ChartEnergy.Plot.Margins(x: 0.0, y: 0.6);
			ChartEnergy.Plot.SetAxisLimits(0, 1000);
			ChartEnergy.Refresh();

			// Настройка графика радиального распределения системы.
			ChartRadDist.Plot.Title("График радиального распределения системы.");
			ChartRadDist.Plot.XLabel("r");
			ChartRadDist.Plot.YLabel("g(r)");
			ChartRadDist.Plot.XAxis.MajorGrid(enable: true, color: Color.FromArgb(50, Color.Black));
			ChartRadDist.Plot.YAxis.MajorGrid(enable: true, color: Color.FromArgb(50, Color.Black));
			ChartRadDist.Plot.XAxis.MinorGrid(enable: true, color: Color.FromArgb(30, Color.Black), lineStyle: LineStyle.Dot);
			ChartRadDist.Plot.YAxis.MinorGrid(enable: true, color: Color.FromArgb(30, Color.Black), lineStyle: LineStyle.Dot);
			ChartRadDist.Plot.Margins(x: 0.0, y: 0.6);
			ChartRadDist.Plot.SetAxisLimits(xMin: 0, yMin: 0);
			ChartRadDist.Refresh();

			// Инициализация сцены для визуализации.
			_scene = new SceneManager() { Viewport3D = SceneVP3D };
			_scene.CreateCamera(_cameraPosition);
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

			// Инициализация массивов энергий системы.
			_kePoints = new List<double>();
			_pePoints = new List<double>();
			_fePoints = new List<double>();

			_positionsAtomsList = new List<List<Vector>>();
			GC.Collect();

			_bgWorkerCreateModel.RunWorkerAsync(new FindPrimesInput(new object[] { size, k }));
		}

		private void OnBackgroundWorkerDoWorkCreateModel(object sender, DoWorkEventArgs e)
		{
			var input = (FindPrimesInput)(e.Argument ?? throw new ArgumentException("Отсутствуют аргументы"));
			var size = (int)input.Args[0];
			var k = (double)input.Args[1];
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

			// Начальное запоминание энергии системы.
			_kePoints.Add(_atomicModel.Ke);
			_pePoints.Add(_atomicModel.Pe);
			_fePoints.Add(_atomicModel.Fe);
		}

		private void OnBackgroundWorkerRunWorkerCompletedCreateModel(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
				MessageBox.Show(e.Error.Message, "Произошла ошибка");
			else
			{
				_positionsAtomsList.Add(_atomicModel.GetPositionsAtoms());
				_scene.CreateMeshAtoms(_positionsAtomsList.First(), _atomicModel.GetSigma() / 2);
				SliderTimeStep.Value = 0;
				SliderTimeStep.IsEnabled = false;

				// Вывод начальной информации.
				RtbOutputInfo.AppendText(InitInfoSystem());

				BtnCreateModel.IsEnabled = true;
				BtnStartCalculation.IsEnabled = true;
				BtnCancelCalculation.IsEnabled = false;
				BtnPauseCalculation.IsEnabled = false;
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
			var T = NudTemperature.Value ?? 300;
			var stepNorm = NudStepNorm.Value ?? 100;
			var dt = NudTimeStep.Value ?? 1;
			var dtOrder = NudTimeStepOrder.Value ?? -14;

			if (_atomicModel != null)
				_atomicModel.dt = dt * Math.Pow(10, dtOrder);

			BtnStartCalculation.IsEnabled = false;
			BtnCancelCalculation.IsEnabled = true;
			BtnPauseCalculation.IsEnabled = true;

			// Очистка графиков.
			ChartEnergy.Plot.Clear();
			ChartEnergy.Plot.SetAxisLimits(_iter, _iter + countStep - 1);
			ChartEnergy.Refresh();
			ChartRadDist.Plot.Clear();
			ChartRadDist.Refresh();

			// Сброс ProgressBar.
			ProgressBar.Value = 0;

			// Вывод начальной информации.
			RtbOutputInfo.AppendText(_isRenormSpeeds ? "\n\nЗапуск перенормировки скоростей...\n" : "\n\nЗапуск моделирования...\n");
			RtbOutputInfo.AppendText("Количество временных шагов: " + countStep + "\n" + (_isRenormSpeeds ? "Шаг перенормировки: " + stepNorm + "\n" : ""));
			RtbOutputInfo.AppendText(TableHeader());
			RtbOutputInfo.AppendText(TableData(_iter - 1));
			RtbOutputInfo.ScrollToEnd();

			// Запуск расчётов.
			_bgWorkerCalculation.RunWorkerAsync(new FindPrimesInput(new object[] { countStep, snapshotStep, T, stepNorm }));
		}

		private void OnBackgroundWorkerDoWorkCalculation(object sender, DoWorkEventArgs e)
		{
			if (_atomicModel == null)
				throw new Exception("atomicModel is null");

			var input = (FindPrimesInput)(e.Argument ?? throw new ArgumentException("Отсутствуют аргументы"));
			var countStep = (int)input.Args[0];
			var snapshotStep = (int)input.Args[1];
			var T = (int)input.Args[2];
			var stepNorm = (int)input.Args[3];

			// Начальная перенормировка скоростей, если она включено.
			if (_isRenormSpeeds)
			{
				_atomicModel.InitVelocityNormalization(T);
				_atomicModel.PulseZeroing();
			}

			_iterCounter = 0;
			// Запуск моделирования.
			for (var i = _iter; i - _iter < countStep; i++)
			{
				// Отслеживание отмены моделирования.
				if (_bgWorkerCalculation.CancellationPending)
				{
					e.Cancel = true;
					_iter += _iterCounter;
					return;
				}

				// Расчёт шага методом Верле.
				_atomicModel.Verlet();

				// Проведение перенормировки скоростей, если она включено.
				if (_isRenormSpeeds && i % stepNorm == 0)
				{
					_atomicModel.VelocityNormalization(T);
					_atomicModel.PulseZeroing();
				}

				// Вывод информации в UI.
				var ii = i;
				Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, () =>
				{
					if ((_isSnapshot && ii % snapshotStep == 0) || ii == _iter + countStep - 1)
					{
						RtbOutputInfo.AppendText(TableData(ii));
						RtbOutputInfo.ScrollToEnd();
					}
				});

				// Запоминание энергий системы на каждом шагу.
				_kePoints.Add(_atomicModel.Ke);
				_pePoints.Add(_atomicModel.Pe);
				_fePoints.Add(_atomicModel.Fe);

				_positionsAtomsList.Add(_atomicModel.GetPositionsAtoms());

				_iterCounter++;

				// Обновление ProgressBar.
				if (i % (countStep / 100) == 0)
					_bgWorkerCalculation.ReportProgress((int)((double)(i - _iter) / countStep * 100) + 1);
			}

			_iter += _iterCounter;
		}

		private void OnBackgroundWorkerRunWorkerCompletedCalculation(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled)
			{
				MessageBox.Show("Моделирование отменено");
				//OnCreateModel(null, null);
			}
			else if (e.Error != null)
				MessageBox.Show(e.Error.Message, "Произошла ошибка");

			BtnStartCalculation.IsEnabled = true;
			BtnCancelCalculation.IsEnabled = false;

			// Настройка и отрисовка графика энергий системы.
			ChartEnergy.Plot.AddHorizontalLine(0, Color.FromArgb(120, Color.Black));
			ChartEnergy.Plot.AddVerticalLine(0, Color.FromArgb(200, Color.Black));
			ChartEnergy.Plot.AddSignalConst(_kePoints.ToArray(), color: Color.Red, label: "Кинетическая энергия");
			ChartEnergy.Plot.AddSignalConst(_pePoints.ToArray(), color: Color.Blue, label: "Потенциальная энергия");
			ChartEnergy.Plot.AddSignalConst(_fePoints.ToArray(), color: Color.Green, label: "Полная энергия");
			ChartEnergy.Plot.Margins(x: 0.0, y: 0.6);
			ChartEnergy.Plot.Legend(location: Alignment.UpperRight);
			ChartEnergy.Refresh();

			// Настройка и отрисовка графика радиального распределения.
			var rd = _atomicModel.GetRadialDistribution();
			ChartRadDist.Plot.AddSignalXY(rd.Select(p => p.X).ToArray(), rd.Select(p => p.Y).ToArray(), color: Color.Blue, label: "Радиальное распределение");
			ChartRadDist.Plot.SetAxisLimits(xMin: 0, xMax: 5 * _atomicModel.Lattice * 0.726, yMin: 0);
			ChartRadDist.Plot.Legend(location: Alignment.UpperRight);
			ChartRadDist.Refresh();

			// Настройка слайдера.
			SliderTimeStep.IsEnabled = true;
			SliderTimeStep.Maximum = _positionsAtomsList.Count - 1;
		}

		private void OnBackgroundWorkerProgressChangedCalculation(object sender, ProgressChangedEventArgs e)
		{
			ProgressBar.Value = e.ProgressPercentage;
		}
		#endregion

		#region ---СОБЫТИЯ ЭЛЕМЕНТОВ УПРАВЛЕНИЯ СЦЕНОЙ---
		private void OnValueChangedSliderTimeStep(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			_scene.CreateMeshAtoms(_positionsAtomsList[(int)SliderTimeStep.Value], _atomicModel.GetSigma() / 2);
		}
		#endregion

		#region ---СОБЫТИЙ УПРАВЛЕНИЯ МОДЕЛИРОВАНИЕ---
		/// <summary>
		/// Событие отмена вычислений.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCancelCalculation(object sender, RoutedEventArgs e)
		{
			if (_bgWorkerCalculation.IsBusy)
				_bgWorkerCalculation.CancelAsync();
		}

		private void OnPauseCalculation(object sender, RoutedEventArgs e)
		{

		}
		#endregion

		#region ---СОБЫТИЯ CHECK BOX---
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
		#endregion

		#region ---ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ---

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