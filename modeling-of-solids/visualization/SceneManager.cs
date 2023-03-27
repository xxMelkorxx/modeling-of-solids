using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace modeling_of_solids
{
	class SceneManager
	{
		private Model3DGroup _mainModel3DGroup = new();
		private ModelVisual3D _modelVisual3D = new();
		private Vector3D _lightDirection = new(0, 0, -2);

		public Viewport3D Viewport3D { get; set; }

		/// <summary>
		/// Создание камеры.
		/// </summary>
		/// <param name="position"></param>
		public void CreateCamera(Point3D position)
		{
			var camera = new PerspectiveCamera
			{
				Position = position,
				LookDirection = new(-position.X, -position.Y, -position.Z)
			};
			Viewport3D.Camera = camera;
		}

		/// <summary>
		/// Создание источника света.
		/// </summary>
		/// <param name="dir"></param>
		public void CreateLight(Vector3D dir)
		{
			var directionalLight = new DirectionalLight(Colors.White, dir);
			_mainModel3DGroup.Children.Add(directionalLight);
		}

		/// <summary>
		/// Отрисовка атомов.
		/// </summary>
		/// <param name="positons"></param>
		/// <param name="radius"></param>
		/// <param name="rowCount"></param>
		/// <param name="columnCount"></param>
		public void CreateMeshAtoms(List<Vector> positons, double radius = 0.1, int rowCount = 10, int columnCount = 10)
		{
			ClearScene(_modelVisual3D);
			CreateLight(_lightDirection);

			positons.ForEach(pos => DrawSphere.CreateSphere(_mainModel3DGroup, pos, radius, rowCount, columnCount));

			_modelVisual3D.Content = _mainModel3DGroup;
			Viewport3D.Children.Add(_modelVisual3D);
		}

		public void ClearScene(ModelVisual3D model)
		{
			_mainModel3DGroup.Children.Clear();
			Viewport3D.Children.Remove(model);
		}
	}
}