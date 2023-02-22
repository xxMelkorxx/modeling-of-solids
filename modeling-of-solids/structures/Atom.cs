namespace modeling_of_solids
{
	public enum AtomType
	{
		Ar,
	}

	public struct Atom
	{
		/// <summary>
		/// Идентификатор.
		/// </summary>
		public int ID;

		/// <summary>
		/// Вектор координаты.
		/// </summary>
		public Vector Position;

		/// <summary>
		/// Вектор скорости.
		/// </summary>
		public Vector Velocity;

		/// <summary>
		/// Вектор ускорения.
		/// </summary>
		public Vector Acceleration;

		/// <summary>
		/// Тип атома.
		/// </summary>
		public AtomType Type;

		/// <summary>
		/// Создание атома.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="pos"></param>
		public Atom(int id, AtomType atomType, Vector pos)
		{
			ID = id;
			Type= atomType;
			Position = pos;
			Velocity = Vector.Zero;
			Acceleration = Vector.Zero;
		}
	}
}
