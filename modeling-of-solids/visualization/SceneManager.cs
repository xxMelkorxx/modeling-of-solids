using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace modeling_of_solids
{
	class SceneManager
	{
		private readonly Model3DGroup _mainModel3DGroup = new();
		private readonly ModelVisual3D _modelVisual3D = new();
		private Vector3D _rotateCenter = new(0, 0, 0);
		private double _distance, _distanceMax;
		private System.Windows.Point _lastPosition;

		private Point3D _position;
		private Vector3D _direction;

		public Viewport3D Viewport3D { get; set; }

		/// <summary>
		/// Создание камеры.
		/// </summary>
		/// <param name="position"></param>
		public void CreateCamera(double l)
		{
			_position = new(-l * 3, l, -l * 2);
			_direction = _rotateCenter - (Vector3D)_position;

			PerspectiveCamera camera = new()
			{
				Position = _position,
				LookDirection = _direction,
			};

			_distanceMax = l * 5;

			Viewport3D.Camera = camera;
			Viewport3D.MouseLeftButtonDown += OnMouseLeftButtonDownSceneVP3D;
			Viewport3D.MouseLeftButtonUp += OnMouseLeftButtonUpSceneVP3D;
			Viewport3D.MouseRightButtonDown += OnMouseRightButtonDownSceneVP3D;
			Viewport3D.MouseRightButtonUp += OnMouseRightButtonUpSceneVP3D;
			Viewport3D.MouseWheel += OnMouseWheelSceneVP3D;
			Viewport3D.MouseMove += OnMouseMoveSceneVP3D;
		}

		/// <summary>
		/// Создание источника света.
		/// </summary>
		/// <param name="dir"></param>
		public void CreateLight()
		{
			DirectionalLight directionalLight = new(Colors.White, _direction);
			_mainModel3DGroup.Children.Add(directionalLight);
		}

		/// <summary>
		/// Отрисовка атомов.
		/// </summary>
		/// <param name="positons"></param>
		/// <param name="l"></param>
		/// <param name="radius"></param>
		/// <param name="rowCount"></param>
		/// <param name="columnCount"></param>
		public void CreateMeshAtoms(List<Vector> positons, double l, double radius = 0.1, int rowCount = 10, int columnCount = 10)
		{
			ClearScene(_modelVisual3D);
			//CreateCamera(l);
			CreateLight();

			positons.ForEach(pos => Draw.CreateSphere(_mainModel3DGroup, pos - l / 2, radius, rowCount, columnCount));
			Draw.CreateCube(_mainModel3DGroup, Vector.Zero, l);

			_modelVisual3D.Content = _mainModel3DGroup;
			Viewport3D.Children.Add(_modelVisual3D);
		}

		public void ClearScene(ModelVisual3D model)
		{
			_mainModel3DGroup.Children.Clear();
			Viewport3D.Children.Remove(model);
		}

		private void OnMouseLeftButtonDownSceneVP3D(object sender, MouseButtonEventArgs e)
		{
			Mouse.Capture(Viewport3D);
			_lastPosition = e.GetPosition(Viewport3D);
		}

		private void OnMouseLeftButtonUpSceneVP3D(object sender, MouseButtonEventArgs e)
		{
			Mouse.Capture(null);
		}

		private void OnMouseRightButtonDownSceneVP3D(object sender, MouseButtonEventArgs e)
		{
			Mouse.Capture(Viewport3D);
			_lastPosition = e.GetPosition(Viewport3D);
		}

		private void OnMouseRightButtonUpSceneVP3D(object sender, MouseButtonEventArgs e)
		{
			Mouse.Capture(null);
		}

		private void OnMouseMoveSceneVP3D(object sender, MouseEventArgs e)
		{
			var camera = (PerspectiveCamera)Viewport3D.Camera;

			if (e.LeftButton == MouseButtonState.Pressed)
			{
				// Вычисление изменения позиции мыши.
				var currentPosition = e.GetPosition(Viewport3D);
				double deltaX = currentPosition.X - _lastPosition.X;
				double deltaY = currentPosition.Y - _lastPosition.Y;

				// Вычисление матрицы поворота по горизонтальной оси.
				var horizontalRotation = new Matrix3D();
				horizontalRotation.Rotate(new Quaternion(camera.UpDirection, -deltaX / 5));

				// Вычисление матрицы поворота по вертикальной оси.
				var verticalRotation = new Matrix3D();
				var vertical = Vector3D.CrossProduct(camera.LookDirection, camera.UpDirection);
				verticalRotation.Rotate(new Quaternion(vertical, -deltaY / 5));

				// Вычисление новой позиции камеры.
				var offset = (Vector3D)camera.Position - _rotateCenter;
				offset *= horizontalRotation;
				offset *= verticalRotation;
				camera.Position = _position = (Point3D)(offset + _rotateCenter); 

				// Поворот камеры.
				camera.LookDirection = _rotateCenter - (Vector3D)camera.Position;
				camera.UpDirection *= horizontalRotation;
				//camera.UpDirection *= verticalRotation;
				((DirectionalLight)_mainModel3DGroup.Children[0]).Direction = _rotateCenter - (Vector3D)_position;

				_lastPosition = currentPosition;
			}
			if (e.RightButton == MouseButtonState.Pressed)
			{
				var currentPosition = e.GetPosition(Viewport3D);
				double deltaX = currentPosition.X - _lastPosition.X;
				double deltaY = currentPosition.Y - _lastPosition.Y;

				_rotateCenter = new Vector3D(_rotateCenter.X + deltaX * 0.005, _rotateCenter.Y + deltaY * 0.005, _rotateCenter.Z);
				_position = new Point3D(camera.Position.X + deltaX * 0.005, camera.Position.Y + deltaY * 0.005, camera.Position.Z);
				camera.Position = _position;

				_lastPosition = currentPosition;
			}
		}

		private void OnMouseWheelSceneVP3D(object sender, MouseWheelEventArgs e)
		{
			// Масштабирование камеры.
			var camera = (PerspectiveCamera)Viewport3D.Camera;

			_distance -= e.Delta / 120d * 0.1;
			if (_distance >= 0.1 && _distance <= _distanceMax)
				camera.Position = (Point3D)(_rotateCenter - camera.LookDirection * _distance);
			else if (_distance < 0.1)
			{
				_distance = 0.1;
				camera.Position = (Point3D)(_rotateCenter - camera.LookDirection * _distance);
			}
			else
			{
				_distance = _distanceMax;
				camera.Position = (Point3D)(_rotateCenter - camera.LookDirection * _distance);
			}
		}
	}
}