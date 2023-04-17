using System;

namespace modeling_of_solids
{
    public partial class MainWnd
{
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
				text += string.Format("Размер структуры (Lx/Ly) - {0}/{0} нм\n", _atomicModel.BoxSize.ToString("F5"));
				text += string.Format("Число атомов - {0}\n", _atomicModel.CountAtoms);
				text += string.Format("Параметр решётки - {0} нм\n", _atomicModel.Lattice);
				text += string.Format("Кинетическая энергия - {0} эВ\n", _atomicModel.Ke.ToString("F5"));
				text += string.Format("Потенциальная энергия - {0} эВ\n", _atomicModel.Pe.ToString("F5"));
				text += string.Format("Полная энергия - {0} эВ\n", _atomicModel.Fe.ToString("F5"));
				text += string.Format("Температура - {0} К\n", _atomicModel.T.ToString("F1"));
				text += string.Format("Объём - {0} нм²\n", Math.Round(_atomicModel.V, 5));
				text += string.Format("Давление - {0} бар\n", _atomicModel.P.ToString("F1"));
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
			return string.Format("{0} |{1} |{2} |{3} |{4} |{5} |{6} |\n",
			   "Шаг".PadLeft(6),
			   "Кин. энергия (эВ)".PadLeft(18),
			   "Пот. энергия (эВ)".PadLeft(18),
			   "Полн. энергия (эВ)".PadLeft(19),
			   "Температура (К)".PadLeft(16),
			   "Объём (нм²)".PadLeft(12),
			   "Давление (бар)".PadLeft(15)
			   );
		}

		/// <summary>
		/// Вывод данных в таблицу.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		private string TableData(int i)
		{
			if (_atomicModel != null)
				return string.Format("{0} |{1} |{2} |{3} |{4} |{5} |{6} |\n",
					i.ToString().PadLeft(6),
					_atomicModel.Ke.ToString("F5").PadLeft(18),
					_atomicModel.Pe.ToString("F5").PadLeft(18),
					_atomicModel.Fe.ToString("F5").PadLeft(19),
					_atomicModel.T.ToString("F1").PadLeft(16),
					_atomicModel.V.ToString("F5").PadLeft(12),
					_atomicModel.P.ToString("F1").PadLeft(15));
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
}
}