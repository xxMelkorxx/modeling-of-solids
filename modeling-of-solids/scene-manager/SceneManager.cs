using HelixToolkit.Wpf;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace modeling_of_solids
{
	class SceneManager
	{
		/// <summary>
		/// Элемент, в которой создаётся сцена.
		/// </summary>
		public HelixViewport3D Viewport3D { get; set; }

		private List<Vector> _initPosAtoms;

		/// <summary>
		/// Отрисовка сцены.
		/// </summary>
		/// <param name="positons"></param>
		/// <param name="l"></param>
		/// <param name="radius"></param>
		public void CreateScene(List<Vector> positons, double l, double radius)
		{
			Point3D posCamera = new(-l * 3, l, -l * 2);
			Point3D rotateCenter = new(0, 0, 0);
			Vector3D dirCamera = rotateCenter - posCamera;

			// Создание камеры.
			PerspectiveCamera camera = new()
			{
				Position = posCamera,
				LookDirection = dirCamera,
			};
			Viewport3D.Camera = camera;
			Viewport3D.Items.Clear();
			
			Viewport3D.Children.Add(new DefaultLights());

			// Создание атомов.
			_initPosAtoms = new();
			var material = Materials.Blue;
			positons.ForEach(pos =>
			{
				_initPosAtoms.Add(pos - l / 2);
				SphereVisual3D sphere = new()
				{
					Center = new Point3D(pos.X - l / 2, pos.Y - l / 2, pos.Z - l / 2),
					Radius = radius,
					Material = material
				};
				Viewport3D.Items.Add(sphere);
			});

			// Добавление расчётной ячейки в виде полупрозрачного куба.
			BoxVisual3D box = new()
			{
				Length = l,
				Width = l,
				Height = l,
				Center = rotateCenter,
				Material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(100, 100, 100, 100)))
			};

			Viewport3D.Items.Add(box);
		}

		/// <summary>
		/// Обновление положения атомов.
		/// </summary>
		/// <param name="positons"></param>
		/// <param name="l"></param>
		public void UpdatePositionsAtoms(List<Vector> positons, double l)
		{
			for (var i = 0; i < positons.Count; i++)
				((SphereVisual3D)Viewport3D.Items[i]).Transform = new TranslateTransform3D(
					positons[i].X - l / 2 - _initPosAtoms[i].X,
					positons[i].Y - l / 2 - _initPosAtoms[i].Y,
					positons[i].Z - l / 2 - _initPosAtoms[i].Z);
		}
	}
}