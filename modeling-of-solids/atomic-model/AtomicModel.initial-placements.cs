namespace modeling_of_solids.atomic_model;

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
    private void InitPlacementSc()
    {
        for (var i = 0; i < Size; i++)
        for (var j = 0; j < Size; j++)
        for (var k = 0; k < Size; k++)
        {
            var idxCell = Size * (Size * i + j) + k;
            Atoms.Add(new Atom(idxCell + 1, AtomsType, new Vector(i, j, k) * Lattice));
        }
    }

    /// <summary>
    /// Начальное размещение атомов в ОЦК-решётку.
    /// </summary>
    private void InitPlacementBcc()
    {
        for (var i = 0; i < Size; i++)
        for (var j = 0; j < Size; j++)
        for (var k = 0; k < Size; k++)
        {
            var idxCell = 2 * (Size * (Size * i + j) + k);
            Atoms.Add(new Atom(idxCell + 1, AtomsType, new Vector(i, j, k) * Lattice));
            Atoms.Add(new Atom(idxCell + 2, AtomsType, new Vector(i + 0.5, j + 0.5, k + 0.5) * Lattice));
        }
    }

    /// <summary>
    /// Начальное размещение атомов в ГЦК-решётку.
    /// </summary>
    private void InitPlaсementFcc()
    {
        for (var i = 0; i < Size; i++)
        for (var j = 0; j < Size; j++)
        for (var k = 0; k < Size; k++)
        {
            var idxCell = 4 * (Size * (Size * i + j) + k);
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
        for (var i = 0; i < Size; i++)
        for (var j = 0; j < Size; j++)
        for (var k = 0; k < Size; k++)
        {
            var idxCell = 8 * (Size * (Size * i + j) + k);
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