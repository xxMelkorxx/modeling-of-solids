using System;
using System.Collections.Generic;

namespace modeling_of_solids
{
	[Serializable]
	public partial class AtomicModel
	{
		/// <summary>
		/// Список атомов.
		/// </summary>
		public List<Atom> Atoms { get; private set; }

		/// <summary>
		/// Размер расчётной ячейки.
		/// </summary>
		public int Size { get; private set; }

		/// <summary>
		/// Размер расчётной ячейки (нм).
		/// </summary>
		public double BoxSize { get; private set; }

		/// <summary>
		/// Число атомов.
		/// </summary>
		public int CountAtoms => Atoms.Count;

		/// <summary>
		/// Кинетическая энергия системы.
		/// </summary>
		public double Ke { get; private set; }

		/// <summary>
		/// Потенциальная энергия системы.
		/// </summary>
		public double Pe { get; private set; }

		/// <summary>
		/// Полная энергия системы.
		/// </summary>
		public double Fe => Pe + Ke;

		/// <summary>
		/// Температура системы.
		/// </summary>
		public double T => 2d / 3d * Ke / (kB / eV * CountAtoms);

		/// <summary>
		/// Давление системы.
		/// </summary>
		public double P => (Ke + 0.5 * _virial / CountAtoms) / V * 1e4;

		private double _virial;

		/// <summary>
		/// Объём системы.
		/// </summary>
		public double V => BoxSize * BoxSize * BoxSize;

		/// <summary>
		/// Тип атомов.
		/// </summary>
		public AtomType AtomsType { get; }

		/// <summary>
		/// Параметр решётки
		/// </summary>
		public double Lattice
		{
			get
			{
				return AtomsType switch
				{
					AtomType.Ar => 0.526,
					AtomType.Cu => 0.3615,
					AtomType.Fe => 0.2866,
					AtomType.Au => 0.40781,
					AtomType.Si => 0.54307,
					AtomType.Ge => 0.566,
					_ => throw new ArgumentNullException()
				};
			}
		}

		/// <summary>
		/// Масса атома.
		/// </summary>
		public double WeightAtom
		{
			get
			{
				return AtomsType switch
				{
					AtomType.Ar => 39.948 * 1.66054e-27,
					AtomType.Cu => 63.546 * 1.66054e-27,
					AtomType.Fe => 55.845 * 1.66054e-27,
					AtomType.Au => 196.966 * 1.66054e-27,
					AtomType.Si => 28.086 * 1.66054e-27,
					AtomType.Ge => 72.63 * 1.66054e-27,
					_ => throw new ArgumentNullException()
				};
			}
		}

		/// <summary>
		/// Класс потенциала.
		/// </summary>
		private Potential? _potential;

		// Параметры симуляции.
		/// <summary>
		/// Величина временного шага.
		/// </summary>
		public double dt;

		// Константы.
		/// <summary>
		/// 1 эВ в Дж с нм.
		/// </summary>
		private const double eV = 0.1602176634;

		/// <summary>
		/// Постоянная Больцмана (Дж/К с нм).
		/// </summary>
		private const double kB = 1.380649e-5;

		/// <summary>
		/// Генератор случайных чисел.
		/// </summary>
		private readonly Random _rnd = new(DateTime.Now.Millisecond);

		/// <summary>
		/// Создание атомной модели.
		/// </summary>
		/// <param name="size"></param>
		/// <param name="atomType"></param>
		/// <param name="latticeType"></param>
		/// <param name="potentialType"></param>
		public AtomicModel(int size, AtomType atomType, LatticeType latticeType, PotentialType potentialType)
		{
			Atoms = new List<Atom>();
			AtomsType = atomType;
			Size = size;
			BoxSize = size * Lattice;

			// Выбор типа решётки.
			InitPlacement(latticeType);

			// Выбор типа потенциала.
			InitPotential(potentialType);

			// Начальная инициализация параметров.
			InitCalculation();
		}

		/// <summary>
		/// Инициализация начального размещения атомов.
		/// </summary>
		/// <param name="typeLattice"></param>
		private void InitPlacement(LatticeType typeLattice)
		{
			switch (typeLattice)
			{
				case LatticeType.SC: InitPlacementSC(); break;
				case LatticeType.BCC: InitPlacementBCC(); break;
				case LatticeType.FCC: InitPlaсementFCC(); break;
				case LatticeType.Diamond: InitPlaсementDiamond(); break;
			}
		}

		/// <summary>
		/// Инициализация потенциала.
		/// </summary>
		private void InitPotential(PotentialType potentialType)
		{
			switch (potentialType)
			{
				case PotentialType.LJ: _potential = new PotentialLJ { Type = AtomsType }; break;
				case PotentialType.MLJ: _potential = new PotentialMLJ { Type = AtomsType }; break;
			}
		}
	}
}