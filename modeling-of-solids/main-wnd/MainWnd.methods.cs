using System;
using System.Collections.Generic;
using System.Drawing;
using ScottPlot;
using ScottPlot.Control;

namespace modeling_of_solids;

public partial class MainWnd
{
    private class FindPrimesInput
    {
        public List<object> Args;

        public FindPrimesInput(IEnumerable<object> args)
        {
            Args = new List<object>();
            foreach (var arg in args)
                Args.Add(arg);
        }
    }

    private static void SetUpChart(IPlotControl chart, string title, string labelX, string labelY)
    {
        chart.Plot.Title(title);
        chart.Plot.XLabel(labelX);
        chart.Plot.YLabel(labelY);
        chart.Plot.XAxis.MajorGrid(enable: true, color: Color.FromArgb(50, Color.Black));
        chart.Plot.YAxis.MajorGrid(enable: true, color: Color.FromArgb(50, Color.Black));
        chart.Plot.XAxis.MinorGrid(enable: true, color: Color.FromArgb(30, Color.Black), lineStyle: LineStyle.Dot);
        chart.Plot.YAxis.MinorGrid(enable: true, color: Color.FromArgb(30, Color.Black), lineStyle: LineStyle.Dot);
        chart.Plot.Margins(x: 0.0, y: 0.6);
        chart.Plot.SetAxisLimits(xMin: 0, yMin: 0);
        chart.Refresh();
    }

    /// <summary>
    /// Начальная информация о системе.
    /// </summary>
    /// <returns></returns>
    private string InitInfoSystem()
    {
        return "Структура создана...\n" +
               $"Тип атомов - {_atomic.AtomsType}\n" +
               $"Размер структуры (Nx/Ny) - {_atomic.Size}/{_atomic.Size}\n" +
               $"Размер структуры (Lx/Ly) - {_atomic.BoxSize.ToString("F5")}/{_atomic.BoxSize.ToString("F5")} нм\n" +
               $"Число атомов - {_atomic.CountAtoms}\n" +
               $"Параметр решётки - {_atomic.Lattice} нм\n" +
               $"Кинетическая энергия - {_atomic.Ke.ToString("F5")} эВ\n" +
               $"Потенциальная энергия - {_atomic.Pe.ToString("F5")} эВ\n" +
               $"Полная энергия - {_atomic.Fe.ToString("F5")} эВ\n" +
               $"Температура - {_atomic.T.ToString("F1")} К\n" +
               $"Объём - {_atomic.V.ToString("F5")} нм²\n" +
               $"Давление - {_atomic.P1.ToString("F1")} Па\n";
    }

    /// <summary>
    /// Заголовок таблицы.
    /// </summary>
    /// <returns></returns>
    private static string TableHeader()
    {
        return $"{"Шаг".PadLeft(6)} |" +
               $"{"Кин. энергия (эВ)".PadLeft(18)} |" +
               $"{"Пот. энергия (эВ)".PadLeft(18)} |" +
               $"{"Полн. энергия (эВ)".PadLeft(19)} |" +
               $"{"Температура (К)".PadLeft(16)} |" +
               $"{"Давление 1 (Па)".PadLeft(16)} |" +
               $"{"Давление 2 (Па)".PadLeft(16)} |\n";
    }

    /// <summary>
    /// Вывод данных в таблицу.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="nsnap"></param>
    /// <returns></returns>
    private string TableData(int i, int nsnap)
    {
        return $"{i.ToString().PadLeft(6)} |" +
               $"{_atomic.Ke.ToString("F5").PadLeft(18)} |" +
               $"{_atomic.Pe.ToString("F5").PadLeft(18)} |" +
               $"{_atomic.Fe.ToString("F5").PadLeft(19)} |" +
               $"{_atomic.T.ToString("F1").PadLeft(16)} |" +
               $"{_atomic.P1.ToString("F1").PadLeft(16)} |" +
               $"{(_atomic.P2 / nsnap).ToString("F1").PadLeft(16)} |\n";
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
}