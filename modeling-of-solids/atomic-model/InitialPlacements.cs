namespace modeling_of_solids
{
	/// <summary>
	/// Тип криссталической решётки.
	/// </summary>
	public enum LatticeType
	{
		SC,
		BCC,
		FCC,
		Diamond
	}

	public partial class AtomicModel
	{
		/// <summary>
		/// Начальное размещение атомов в кубическую решётку.
		/// </summary>
		private void InitPlacementSC()
		{
			for (int i = 0; i < Size; i++)
				for (int j = 0; j < Size; j++)
					for (int k = 0; k < Size; k++)
					{
						int idxCell = Size * (Size * i + j) + k;
						Atoms.Add(new Atom(idxCell + 1, AtomsType, new Vector(i, j, k) * Lattice));
					}
		}

		/// <summary>
		/// Начальное размещение атомов в ОЦК-решётку.
		/// </summary>
		private void InitPlacementBCC()
		{
			for (int i = 0; i < Size; i++)
				for (int j = 0; j < Size; j++)
					for (int k = 0; k < Size; k++)
					{
						int idxCell = 2 * (Size * (Size * i + j) + k);
						Atoms.Add(new Atom(idxCell + 1, AtomsType, new Vector(i, j, k) * Lattice));
						Atoms.Add(new Atom(idxCell + 2, AtomsType, new Vector(i + 0.5, j + 0.5, k + 0.5) * Lattice));
					}
		}

		/// <summary>
		/// Начальное размещение атомов в ГЦК-решётку.
		/// </summary>
		private void InitPlaсementFCC()
		{
			for (int i = 0; i < Size; i++)
				for (int j = 0; j < Size; j++)
					for (int k = 0; k < Size; k++)
					{
						int idxCell = 4 * (Size * (Size * i + j) + k);
						Atoms.Add(new Atom(idxCell + 1, AtomsType, new Vector(i, j, k) * Lattice));
						Atoms.Add(new Atom(idxCell + 2, AtomsType, new Vector(i + 0.5, j, k + 0.5) * Lattice));
						Atoms.Add(new Atom(idxCell + 3, AtomsType, new Vector(i, j + 0.5, k + 0.5) * Lattice));
						Atoms.Add(new Atom(idxCell + 4, AtomsType, new Vector(i + 0.5, j + 0.5, k) * Lattice));
					}
		}

		/// <summary>
		/// Начальное размещение атомов в АЦК-решётку.
		/// </summary>
		private void InitPlaсementDiamond()
		{
			for (int i = 0; i < Size; i++)
				for (int j = 0; j < Size; j++)
					for (int k = 0; k < Size; k++)
					{
						int idxCell = 8 * (Size * (Size * i + j) + k);
						Atoms.Add(new Atom(idxCell + 1, AtomsType, new Vector(i, j, k) * Lattice));
						Atoms.Add(new Atom(idxCell + 2, AtomsType, new Vector(i + 0.5, j, k + 0.5) * Lattice));
						Atoms.Add(new Atom(idxCell + 3, AtomsType, new Vector(i, j + 0.5, k + 0.5) * Lattice));
						Atoms.Add(new Atom(idxCell + 4, AtomsType, new Vector(i + 0.5, j + 0.5, k) * Lattice));
						Atoms.Add(new Atom(idxCell + 5, AtomsType, new Vector(i + 0.25, j + 0.25, k + 0.25) * Lattice));
						Atoms.Add(new Atom(idxCell + 6, AtomsType, new Vector(i + 0.25, j + 0.75, k + 0.75) * Lattice));
						Atoms.Add(new Atom(idxCell + 7, AtomsType, new Vector(i + 0.75, j + 0.25, k + 0.75) * Lattice));
						Atoms.Add(new Atom(idxCell + 8, AtomsType, new Vector(i + 0.75, j + 0.75, k + 0.25) * Lattice));
					}
		}
	}
}
