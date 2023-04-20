using modeling_of_solids.atomic_model;
using modeling_of_solids.potentials;

namespace modeling_of_solids.testing;

[TestClass]
public class SeparationSquredTesting
{
	[TestMethod("���� ��� ������� ���������")]
	public void TestAtZeroValues()
	{
		AtomicModel model = new(3, AtomType.Ar, LatticeType.FCC, PotentialType.LJ);
		Vector vec1 = new(0, 0, 0);
		Vector vec2 = new(0, 0, 0);

		var result = model.SeparationSqured(vec1, vec2, out var dxdydz);
		Assert.AreEqual(result, 0);
		Assert.AreEqual(dxdydz, Vector.Zero);
	}
}