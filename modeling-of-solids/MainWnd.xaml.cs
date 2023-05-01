using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using System.Windows;
using System.Windows.Threading;
using modeling_of_solids.atomic_model;
using modeling_of_solids.potentials;
using modeling_of_solids.scene_manager;

namespace modeling_of_solids;

public partial class MainWnd
{
    private AtomicModel _atomic;
    private SceneManager _scene;
    private readonly BackgroundWorker _bgWorkerCreateModel, _bgWorkerCalculation;
    private readonly System.Windows.Forms.Timer _timer;
    private List<List<Vector>> _positionsAtomsList;

    private List<PointD> _rrt;
    private List<double> _kePoints, _pePoints, _fePoints;
    private double _averT;
    private bool _isDisplacement, _isSnapshot, _isRenormSpeeds, _isNewSystem;
    private double _yMaxRb;
    private int _initStep;

    public MainWnd()
    {
        InitializeComponent();

        _bgWorkerCreateModel = (BackgroundWorker)FindResource("BackgroundWorkerCreateModel");
        _bgWorkerCalculation = (BackgroundWorker)FindResource("BackgroundWorkerCalculation");

        _timer = new() { Interval = 30 };
        _timer.Tick += OnTickTimer;
    }

    private void OnLoadedMainWindow(object sender, RoutedEventArgs e)
    {
        // Настройка графиков.
        SetUpChart(ChartEnergy, "Графики энергий системы", "Временной шаг", "Энергия (эВ)");
        SetUpChart(ChartRadDist, "График радиального распределения системы", "r", "g(r)");
        SetUpChart(ChartRt, "График  среднего квадрата смещения системы", "Временной интервал (t)", "Сред. квадрат смещения (R²)");
        SetUpChart(ChartAcfSpeed, "График автокорреляционной функции скорости", "", "");

        // Инициализация сцены для визуализации.
        _scene = new SceneManager { Viewport3D = Viewport };
    }

    #region ---СОБЫТИЯ СОЗДАНИЯ МОДЕЛИ---

    /// <summary>
    /// Событие создание модели.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCreateModel(object sender, RoutedEventArgs e)
    {
        if (_timer.Enabled)
            _timer.Stop();

        var size = NudSize.Value;
        var k = NudDisplacement.Value;
        var atomType = (AtomType)ComboBoxTypeAtom.SelectedIndex;
        var latticeType = (LatticeType)ComboBoxTypeLattice.SelectedIndex;
        var potentialType = (PotentialType)ComboBoxTypePotential.SelectedIndex;

        BtnCreateModel.IsEnabled = false;
        ProgressBar.Value = 0;
        RtbOutputInfo.Document.Blocks.Clear();

        // Инициализация массивов энергий системы.
        _kePoints = new List<double>();
        _pePoints = new List<double>();
        _fePoints = new List<double>();
        
        // Очистка графиков.
        ChartEnergy.Plot.Clear();
        ChartEnergy.Refresh();
        ChartRt.Plot.Clear();
        ChartRt.Refresh();
        ChartRadDist.Plot.Clear();
        ChartRadDist.Refresh();
        ChartAcfSpeed.Plot.Clear();
        ChartAcfSpeed.Refresh();

        _bgWorkerCreateModel.RunWorkerAsync(new FindPrimesInput(new object[] { size, k, atomType, latticeType, potentialType }));
    }

    /// <summary>
    /// DO WORK CREATE MODEL
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="ArgumentException"></exception>
    private void OnBackgroundWorkerDoWorkCreateModel(object sender, DoWorkEventArgs e)
    {
        var input = (FindPrimesInput)(e.Argument ?? throw new ArgumentException("Отсутствуют аргументы"));
        var size = (int)input.Args[0];
        var k = (double)input.Args[1];
        var atomType = (AtomType)input.Args[2];
        var latticeType = (LatticeType)input.Args[3];
        var potentialType = (PotentialType)input.Args[4];
        _bgWorkerCreateModel.ReportProgress(250);

        // Инициализация системы.
        _atomic = new AtomicModel(size, atomType, latticeType, potentialType);
        _initStep = _atomic.CurrentStep;
        _bgWorkerCreateModel.ReportProgress(500);

        // Применение случайного смещения для атомов.
        if (_isDisplacement)
        {
            _atomic.AtomsDisplacement(k);
            _bgWorkerCreateModel.ReportProgress(750);
        }

        // Вычисление начальных параметров системы.
        _atomic.InitCalculation();
        _bgWorkerCreateModel.ReportProgress(1000);

        // Начальное запоминание энергии системы.
        _kePoints.Add(_atomic.Ke);
        _pePoints.Add(_atomic.Pe);
        _fePoints.Add(_atomic.Fe);
    }

    /// <summary>
    /// RUN WORKER COMPLETED CREATE MODEL
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnBackgroundWorkerRunWorkerCompletedCreateModel(object sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Error != null)
            MessageBox.Show(e.Error.Message, "Произошла ошибка");
        else
        {
            _isNewSystem = true;

            // Запоминание позиции атомов на 0-ом шаге.
            _positionsAtomsList = new List<List<Vector>> { _atomic.GetPositionsAtoms() };

            GC.Collect();
            // Отрисовка атомов на сцене.
            _scene.CreateScene(_positionsAtomsList.First(), _atomic.BoxSize, _atomic.GetSigma() / 2d);
            // Обнуление и блокировка слайдера.
            SliderTimeStep.Value = 0;
            SliderTimeStep.IsEnabled = false;

            // Вывод начальной информации.
            RtbOutputInfo.AppendText(InitInfoSystem());

            // Настройка и отрисовка графика радиального распределения.
            var rd = _atomic.GetRadialDistribution();
            _yMaxRb = rd.Max(p => p.Y);
            ChartRadDist.Plot.Clear();
            ChartRadDist.Plot.AddSignalXY(rd.Select(p => p.X).ToArray(), rd.Select(p => p.Y).ToArray(), color: Color.Blue, label: "Радиальное распределение");
            ChartRadDist.Plot.SetAxisLimits(xMin: 0, xMax: 5 * _atomic.Lattice * 0.726, yMin: 0, yMax: _yMaxRb);
            ChartRadDist.Plot.Legend(location: Alignment.UpperRight);
            ChartRadDist.Refresh();

            BtnCreateModel.IsEnabled = true;
            BtnStartCalculation.IsEnabled = true;
            BtnSaveModel.IsEnabled = true;
            BtnCancelCalculation.IsEnabled = false;
            BtnToBegin.IsEnabled = false;
            BtnStepBack.IsEnabled = false;
            BtnPlayTimer.IsEnabled = false;
            BtnPauseTimer.IsEnabled = false;
            BtnStepForward.IsEnabled = false;
            BtnToEnd.IsEnabled = false;
            BtnFaster.IsEnabled = false;
            BtnSlower.IsEnabled = false;
        }
    }

    /// <summary>
    /// PROGRESS CHANGED CREATE MODEL
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
    private void OnStartCalculation(object sender, RoutedEventArgs e)
    {
        var countStep = NudCountStep.Value;
        var snapshotStep = NudSnapshotStep.Value;
        var stepRt = NudStepRt.Value;
        var T = NudTemperature.Value;
        var stepNorm = NudStepNorm.Value;

        _atomic.Dt = (int)NudTimeStep.Value * Math.Pow(10, (int)NudTimeStepOrder.Value);

        BtnStartCalculation.IsEnabled = false;
        BtnCancelCalculation.IsEnabled = true;

        // Инициализация массива среднего квадрата смещения.
        _rrt = new List<PointD> { new(0, 0) };
        _averT = 0;

        // Очистка графиков.
        ChartEnergy.Plot.Clear();
        ChartEnergy.Plot.SetAxisLimits(_initStep, _initStep + countStep - 1);
        ChartEnergy.Refresh();
        ChartRt.Plot.Clear();
        ChartRt.Refresh();
        ChartAcfSpeed.Plot.Clear();
        ChartAcfSpeed.Refresh();


        // Сброс ProgressBar.
        ProgressBar.Value = 0;

        // Вывод начальной информации.
        RtbOutputInfo.AppendText(_isRenormSpeeds ? "\n\nЗапуск перенормировки скоростей...\n" : "\nЗапуск моделирования...\n");
        RtbOutputInfo.AppendText($"Количество временных шагов: {countStep}\n" + (_isRenormSpeeds ? $"Шаг перенормировки: {stepNorm}\n\n" : "\n"));
        RtbOutputInfo.AppendText(TableHeader());
        RtbOutputInfo.AppendText(TableData(_initStep - 1, 1));
        RtbOutputInfo.ScrollToEnd();

        // Запуск расчётов.
        _bgWorkerCalculation.RunWorkerAsync(new FindPrimesInput(new object[] { countStep, snapshotStep, stepRt, T, stepNorm }));
    }

    /// <summary>
    /// DO WORK CALCULATION
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="Exception"></exception>
    /// <exception cref="ArgumentException"></exception>
    private void OnBackgroundWorkerDoWorkCalculation(object sender, DoWorkEventArgs e)
    {
        if (_atomic == null)
            throw new NullReferenceException();

        var input = (FindPrimesInput)(e.Argument ?? throw new ArgumentException("Отсутствуют аргументы"));
        var countStep = (int)input.Args[0];
        var snapshotStep = (int)input.Args[1];
        var stepRt = (int)input.Args[2];
        var T = (int)input.Args[3];
        var stepNorm = (int)input.Args[4];

        // Начальная перенормировка скоростей, если она включено.
        if (_isRenormSpeeds)
        {
            _atomic.InitVelocityNormalization(T);
            _atomic.PulseZeroing();
        }

        _atomic.CurrentStep = 1;

        // Запуск моделирования.
        for (var i = _initStep; i - _initStep < countStep; i++)
        {
            // Отслеживание отмены моделирования.
            if (_bgWorkerCalculation.CancellationPending)
            {
                e.Cancel = true;
                _initStep += _atomic.CurrentStep - 1;
                return;
            }

            // Расчёт шага методом Верле.
            _atomic.Verlet();

            // Проведение перенормировки скоростей, если она включено.
            if (_isRenormSpeeds && i % stepNorm == 0)
                _atomic.VelocityNormalization(T);

            _kePoints.Add(_atomic.Ke);
            _pePoints.Add(_atomic.Pe);
            _fePoints.Add(_atomic.Fe);
            _averT += _atomic.T;
            _positionsAtomsList.Add(_atomic.GetPositionsAtoms());

            // Вывод информации в UI.
            if ((_isSnapshot && i % snapshotStep == 0) || i == _initStep + countStep - 1)
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, () =>
                {
                    RtbOutputInfo.AppendText(TableData(i, _isSnapshot ? snapshotStep : countStep));
                    RtbOutputInfo.ScrollToEnd();
                    _atomic.Flux = Vector.Zero;

                    // Настройка и отрисовка графика радиального распределения.
                    var rd = _atomic.GetRadialDistribution();
                    ChartRadDist.Plot.Clear();
                    ChartRadDist.Plot.AddSignalXY(rd.Select(p => p.X).ToArray(), rd.Select(p => p.Y).ToArray(), color: Color.Blue, label: "Радиальное распределение");
                    ChartRadDist.Plot.SetAxisLimits(xMin: 0, xMax: 5 * _atomic.Lattice * 0.726, yMin: 0, yMax: _yMaxRb);
                    ChartRadDist.Plot.Legend(location: Alignment.UpperRight);
                    ChartRadDist.Refresh();
                });

            // Расчёт среднего квадрата смещения.
            if (i % stepRt == 0 || i == _initStep + countStep - 1)
                _rrt.Add(new PointD(i - _initStep + 1, _atomic.GetAverageSquareOffset()));

            // Обновление ProgressBar.
            _bgWorkerCalculation.ReportProgress((int)((double)(i - _initStep) / countStep * 1000d) + 1);
        }

        _averT /= countStep;
        _initStep += _atomic.CurrentStep - 1;
    }

    /// <summary>
    /// RUN WORKER COMPLETED CALCULATION
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnBackgroundWorkerRunWorkerCompletedCalculation(object sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Cancelled)
            MessageBox.Show("Моделирование отменено");
        if (e.Error != null)
            MessageBox.Show(e.Error.Message, "Произошла ошибка");
        if (_atomic == null)
            throw new NullReferenceException();

        // Отрисовка графика энергий системы.
        ChartEnergy.Plot.AddSignalConst(_kePoints.ToArray(), color: Color.Red, label: "Кинетическая энергия");
        ChartEnergy.Plot.AddSignalConst(_pePoints.ToArray(), color: Color.Blue, label: "Потенциальная энергия");
        ChartEnergy.Plot.AddSignalConst(_fePoints.ToArray(), color: Color.Green, label: "Полная энергия");
        ChartEnergy.Plot.AddHorizontalLine(0, Color.FromArgb(120, Color.Black));
        ChartEnergy.Plot.AddVerticalLine(0, Color.FromArgb(200, Color.Black));
        ChartEnergy.Plot.Margins(x: 0.0, y: 0.6);
        ChartEnergy.Plot.Legend(location: Alignment.UpperRight);
        ChartEnergy.Refresh();

        // Отрисовка графика радиального распределения.
        var rd = _atomic.GetRadialDistribution();
        ChartRadDist.Plot.Clear();
        ChartRadDist.Plot.AddSignalXY(rd.Select(p => p.X).ToArray(), rd.Select(p => p.Y).ToArray(), color: Color.Blue, label: "Радиальное распределение");
        ChartRadDist.Plot.SetAxisLimits(xMin: 0, xMax: 5 * _atomic.Lattice * 0.726, yMin: 0, yMax: _yMaxRb);
        ChartRadDist.Plot.Legend(location: Alignment.UpperRight);
        ChartRadDist.Refresh();

        // Отрисовка графика среднего квадрата смещения распределения.
        ChartRt.Plot.AddSignalXY(_rrt.Select(p => p.X).ToArray(), _rrt.Select(p => p.Y).ToArray(),
            color: Color.Indigo, label: $"Средний квадрат смещения (средняя температура - {_averT.ToString("F3")} К)");
        ChartRt.Plot.SetAxisLimits(
            xMin: 0, xMax: _rrt.Max(p => p.X) == 0 ? NudStepRt.Value : _rrt.Max(p => p.X),
            yMin: 0, yMax: (_rrt.Max(p => p.Y) == 0 ? 0.1 : _rrt.Max(p => p.Y)) * 1.5);
        ChartRt.Plot.Legend(location: Alignment.UpperRight);
        ChartRt.Refresh();

        // Отрисовка графика АКФ скорости.
        var zt = _atomic.GetAcfs();
        ChartAcfSpeed.Plot.AddSignal(zt, color: Color.Green, label: "Автокорреляционная функция скорости");
        ChartAcfSpeed.Plot.SetAxisLimits(xMin: 0, xMax: zt.Length, yMin: zt.Min() - 0.1, yMax: zt.Max() + 0.1);
        ChartAcfSpeed.Plot.AddHorizontalLine(0, Color.FromArgb(120, Color.Black));
        ChartAcfSpeed.Plot.AddVerticalLine(0, Color.FromArgb(200, Color.Black));
        ChartAcfSpeed.Plot.Legend(location: Alignment.UpperRight);
        ChartAcfSpeed.Refresh();

        BtnStartCalculation.IsEnabled = true;
        BtnCancelCalculation.IsEnabled = false;
        _isNewSystem = false;
        SliderTimeStep.IsEnabled = true;
        SliderTimeStep.Maximum = _positionsAtomsList.Count - 1;
        BtnToBegin.IsEnabled = false;
        BtnStepBack.IsEnabled = false;
        BtnPlayTimer.IsEnabled = true;
        BtnPauseTimer.IsEnabled = false;
        BtnStepForward.IsEnabled = true;
        BtnToEnd.IsEnabled = true;
        BtnFaster.IsEnabled = false;
        BtnSlower.IsEnabled = false;
    }

    /// <summary>
    /// PROGRESS CHANGED CALCULATION
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnBackgroundWorkerProgressChangedCalculation(object sender, ProgressChangedEventArgs e)
    {
        ProgressBar.Value = e.ProgressPercentage;
    }

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

    #endregion
}