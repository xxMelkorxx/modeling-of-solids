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
		public double L { get; private set; }

		/// <summary>
		/// Число атомов.
		/// </summary>
		public int CountAtoms => Atoms.Count;

		/// <summary>
		/// Кинетическая энергия системы.
		/// </summary>
		public double Ke => _ke;
		private double _ke;

		/// <summary>
		/// Потенциальная энергия системы.
		/// </summary>
		public double Pe => _pe;
		private double _pe;

		/// <summary>
		/// Полная энергия системы.
		/// </summary>
		public double Fe => _pe + _ke;

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
		public double V => L * L * L;

		/// <summary>
		/// Тип атомов.
		/// </summary>
		public AtomType AtomsType { get; private set; }

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
					_ => 0.526,
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
					_ => 39.948 * 1.66054e-27
				};
			}
		}

		/// <summary>
		/// Класс потенциала.
		/// </summary>
		private Potential _potential;

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

#pragma warning disable CS8618
		/// <summary>
		/// Создание атомной модели.
		/// </summary>
		/// <param name="size"></param>
		/// <param name="atomType"></param>
		/// <param name="latticeType"></param>
		public AtomicModel(int size, AtomType atomType, LatticeType latticeType, PotentialType potentialType)
		{
			Atoms = new List<Atom>();
			AtomsType = atomType;
			Size = size;
			L = size * Lattice;

			InitPlacement(latticeType);
			InitPotential(potentialType);

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
				case PotentialType.LennardJones: _potential = new PotentialLJ(AtomsType); break;
			}
		}
	}
}
